using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StrategyRPG.Tactics
{
    public enum FleetActionType { Move, Attack, FieldRepair, EndPhase }

    /// <summary>An index-based action remains valid across deep-cloned battle states.</summary>
    public readonly struct FleetBattleAction : IEquatable<FleetBattleAction>
    {
        public readonly FleetActionType type;
        public readonly int actor;
        public readonly int target;
        public readonly Vector2Int cell;

        private FleetBattleAction(FleetActionType type, int actor, int target, Vector2Int cell)
        { this.type = type; this.actor = actor; this.target = target; this.cell = cell; }

        public static FleetBattleAction Move(int actor, Vector2Int cell) => new FleetBattleAction(FleetActionType.Move, actor, -1, cell);
        public static FleetBattleAction Attack(int actor, int target) => new FleetBattleAction(FleetActionType.Attack, actor, target, default);
        public static FleetBattleAction Repair(int actor) => new FleetBattleAction(FleetActionType.FieldRepair, actor, -1, default);
        public static FleetBattleAction EndPhase() => new FleetBattleAction(FleetActionType.EndPhase, -1, -1, default);

        public bool Equals(FleetBattleAction other) => type == other.type && actor == other.actor && target == other.target && cell == other.cell;
        public override bool Equals(object obj) => obj is FleetBattleAction other && Equals(other);
        public override int GetHashCode() => (((int)type * 397 ^ actor) * 397 ^ target) * 397 ^ cell.GetHashCode();
        public override string ToString() => type == FleetActionType.Move ? $"Move unit {actor} to {cell}" :
            type == FleetActionType.Attack ? $"Unit {actor} attacks {target}" :
            type == FleetActionType.FieldRepair ? $"Repair unit {actor}" : "End phase";
    }

    /// <summary>Pure action generation and transitions shared by AI search and validation.</summary>
    public static class FleetBattleTransition
    {
        private const int MovementCandidatesPerUnit = 4;

        public static List<FleetBattleAction> LegalActions(FleetBattleState state)
        {
            var actions = new List<FleetBattleAction>();
            if (state == null || state.Finished) return actions;
            bool enemyTurn = state.Phase == FleetPhase.Enemy;
            for (int actor = 0; actor < state.Units.Count; actor++)
            {
                FleetUnit unit = state.Units[actor];
                if (!unit.Alive || unit.enemy != enemyTurn || !state.CanAct(unit)) continue;
                for (int target = 0; target < state.Units.Count; target++)
                    if (state.CanAttack(unit, state.Units[target])) actions.Add(FleetBattleAction.Attack(actor, target));
                if (state.CanFieldRepair(unit)) actions.Add(FleetBattleAction.Repair(actor));
                foreach (var cell in RankedMoves(state, unit).Take(MovementCandidatesPerUnit))
                    actions.Add(FleetBattleAction.Move(actor, cell));
            }
            actions.Add(FleetBattleAction.EndPhase());
            return actions;
        }

        private static IEnumerable<Vector2Int> RankedMoves(FleetBattleState state, FleetUnit unit)
        {
            var opponents = state.Units.Where(other => other.Alive && other.enemy != unit.enemy).ToArray();
            var reachable = state.ReachableCells(unit);
            return from pair in reachable
                   let cell = pair.Key
                   let canFire = opponents.Any(target => FleetBattleState.Distance(cell, target.cell) <= unit.range && state.HasSight(cell, target.cell))
                   let nearest = opponents.Length == 0 ? 0 : opponents.Min(target => FleetBattleState.Distance(cell, target.cell))
                   let pathLength = pair.Value
                   orderby canFire descending, nearest ascending, pathLength ascending, cell.x ascending, cell.y ascending
                   select cell;
        }

        public static bool Apply(FleetBattleState state, FleetBattleAction action)
        {
            if (state == null || state.Finished) return false;
            if (action.type == FleetActionType.EndPhase) { state.EndPhase(); return true; }
            if (action.actor < 0 || action.actor >= state.Units.Count) return false;
            FleetUnit actor = state.Units[action.actor];
            switch (action.type)
            {
                case FleetActionType.Move: return state.Move(actor, action.cell);
                case FleetActionType.Attack:
                    return action.target >= 0 && action.target < state.Units.Count && state.Attack(actor, state.Units[action.target]);
                case FleetActionType.FieldRepair: return state.FieldRepair(actor);
                default: return false;
            }
        }
    }

    /// <summary>Open-loop stochastic MCTS. Attack outcomes are resampled on every simulation.</summary>
    public sealed class FleetMctsPlanner
    {
        private sealed class Node
        {
            public int visits;
            public double totalValue;
            public readonly Dictionary<FleetBattleAction, Node> children = new Dictionary<FleetBattleAction, Node>();
        }

        private readonly FleetBattleState rootState;
        private readonly bool perspectiveEnemy;
        private readonly System.Random random;
        private readonly Node root = new Node();
        private int simulationIndex;
        public int Simulations => root.visits;

        public FleetMctsPlanner(FleetBattleState state, bool perspectiveEnemy, int seed)
        {
            rootState = state.Clone();
            this.perspectiveEnemy = perspectiveEnemy;
            random = new System.Random(seed);
        }

        public void RunIterations(int count)
        {
            for (int i = 0; i < count; i++) RunIteration();
        }

        public FleetBattleAction BestAction()
        {
            var legal = FleetBattleTransition.LegalActions(rootState);
            if (legal.Count == 0) return FleetBattleAction.EndPhase();
            return legal.Where(action => root.children.ContainsKey(action))
                .OrderByDescending(action => root.children[action].visits)
                .ThenByDescending(action => root.children[action].totalValue / Math.Max(1, root.children[action].visits))
                .DefaultIfEmpty(legal[0]).First();
        }

        private void RunIteration()
        {
            var state = rootState.Clone(++simulationIndex * 7919);
            var path = new List<Node> { root };
            Node node = root;
            const int treeDepth = 5;
            for (int depth = 0; depth < treeDepth && !state.Finished; depth++)
            {
                var legal = FleetBattleTransition.LegalActions(state);
                if (legal.Count == 0) break;
                var unexplored = legal.Where(action => !node.children.ContainsKey(action)).ToArray();
                FleetBattleAction action;
                if (unexplored.Length > 0)
                {
                    action = unexplored[random.Next(unexplored.Length)];
                    var child = new Node(); node.children[action] = child; node = child; path.Add(node);
                    if (!FleetBattleTransition.Apply(state, action)) break;
                    break;
                }
                bool maximizing = (state.Phase == FleetPhase.Enemy) == perspectiveEnemy;
                action = legal.OrderByDescending(candidate => Uct(node.children[candidate], node.visits, maximizing)).First();
                node = node.children[action]; path.Add(node);
                if (!FleetBattleTransition.Apply(state, action)) break;
            }

            Rollout(state, 5);
            double value = Evaluate(state, perspectiveEnemy);
            foreach (var visited in path) { visited.visits++; visited.totalValue += value; }
        }

        private double Uct(Node child, int parentVisits, bool maximizing)
        {
            double mean = child.totalValue / Math.Max(1, child.visits);
            double exploitation = maximizing ? mean : -mean;
            return exploitation + 1.35 * Math.Sqrt(Math.Log(Math.Max(2, parentVisits)) / Math.Max(1, child.visits));
        }

        private void Rollout(FleetBattleState state, int depth)
        {
            for (int step = 0; step < depth && !state.Finished; step++)
            {
                var legal = FleetBattleTransition.LegalActions(state);
                if (legal.Count == 0) break;
                FleetBattleAction action = random.NextDouble() < .22 ? legal[random.Next(legal.Count)] : HeuristicAction(state, legal);
                if (!FleetBattleTransition.Apply(state, action)) break;
            }
        }

        private FleetBattleAction HeuristicAction(FleetBattleState state, List<FleetBattleAction> legal)
        {
            var useful = legal.Where(action => action.type != FleetActionType.EndPhase).ToArray();
            if (useful.Length == 0) return FleetBattleAction.EndPhase();
            return useful.OrderByDescending(action => ActionScore(state, action) + random.NextDouble() * 3.0).First();
        }

        private static double ActionScore(FleetBattleState state, FleetBattleAction action)
        {
            if (action.type == FleetActionType.Attack)
            {
                var attacker = state.Units[action.actor]; var target = state.Units[action.target];
                double expected = attacker.accuracy * attacker.attack * (1.0 + attacker.critRate * (attacker.criticalMultiplier - 1.0));
                return 80 + expected + (expected >= target.hp ? 45 : 0) - target.hp * .05;
            }
            if (action.type == FleetActionType.FieldRepair)
            {
                var actor = state.Units[action.actor];
                return 54 + (1.0 - (double)actor.hp / actor.maxHP) * 50;
            }
            if (action.type == FleetActionType.Move)
            {
                var actor = state.Units[action.actor];
                int nearest = state.Units.Where(unit => unit.Alive && unit.enemy != actor.enemy)
                    .Select(unit => FleetBattleState.Distance(action.cell, unit.cell)).DefaultIfEmpty(20).Min();
                return 35 - nearest * 3;
            }
            return -20;
        }

        public static double Evaluate(FleetBattleState state, bool perspectiveEnemy)
        {
            if (state.Phase == FleetPhase.Victory) return perspectiveEnemy ? -10000 : 10000;
            if (state.Phase == FleetPhase.Defeat) return perspectiveEnemy ? 10000 : -10000;
            double score = 0;
            foreach (var unit in state.Units)
            {
                double sign = unit.enemy == perspectiveEnemy ? 1 : -1;
                score += sign * (unit.Alive ? 260 : 0);
                score += sign * 420 * unit.hp / Math.Max(1.0, unit.maxHP);
                score += sign * unit.ap * 8;
                score += sign * unit.fieldRepairs * 6;
            }
            return score;
        }
    }
}
