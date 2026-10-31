using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StrategyRPG.Tactics
{
    public enum FleetPhase { Player, Enemy, Victory, Defeat }
    public enum FleetAttackOutcome { Miss, Hit, Critical }

    [Serializable]
    public struct FleetAttackResult
    {
        public FleetAttackOutcome outcome;
        public int damage;
        public bool Hit => outcome != FleetAttackOutcome.Miss;
        public bool Critical => outcome == FleetAttackOutcome.Critical;
    }

    [Serializable]
    public class FleetUnit
    {
        public string name;
        public bool enemy;
        public Vector2Int cell;
        public int hp, maxHP, attack, range, move, ap = 2;
        [Range(0f, 1f)] public float accuracy = .85f;
        [Range(0f, 1f)] public float critRate = .10f;
        public float criticalMultiplier = 1.5f;
        public int fieldRepairs = 1;
        public bool Alive => hp > 0;
        public FleetUnit(string name, bool enemy, Vector2Int cell, int hp, int attack, int range, int move)
        { this.name = name; this.enemy = enemy; this.cell = cell; this.hp = maxHP = hp; this.attack = attack; this.range = range; this.move = move; }
    }

    /// <summary>Seeded encounter rules. No scenes, UI, animation or persistence.</summary>
    public class FleetBattleState
    {
        public const int Width = 8, Height = 6;
        private static readonly Vector2Int[] Directions = { new Vector2Int(1,0), new Vector2Int(1,-1), new Vector2Int(0,-1), new Vector2Int(-1,0), new Vector2Int(-1,1), new Vector2Int(0,1) };
        public readonly List<FleetUnit> Units = new List<FleetUnit>();
        public readonly HashSet<Vector2Int> Obstacles = new HashSet<Vector2Int>();
        public readonly Vector2Int RepairCell = new Vector2Int(3, 2);
        public bool RepairUsed { get; private set; }
        public FleetPhase Phase { get; private set; } = FleetPhase.Player;
        public int Round { get; private set; } = 1;
        public bool Finished => Phase == FleetPhase.Victory || Phase == FleetPhase.Defeat;
        public FleetAttackResult LastAttackResult { get; private set; }
        private uint randomState;

        private FleetBattleState(bool empty) { }

        public FleetBattleState(int randomSeed = 440)
        {
            randomState = randomSeed == 0 ? 1u : unchecked((uint)randomSeed);
            Units.Add(new FleetUnit("Vanguard", false, new Vector2Int(0,1), 60, 18, 3, 2));
            Units.Add(new FleetUnit("Needle", false, new Vector2Int(0,3), 40, 14, 2, 3));
            Units.Add(new FleetUnit("Longbow", false, new Vector2Int(1,4), 42, 16, 4, 2));
            Units.Add(new FleetUnit("Raider I", true, new Vector2Int(7,1), 38, 12, 2, 2));
            Units.Add(new FleetUnit("Raider II", true, new Vector2Int(6,3), 38, 12, 2, 2));
            Units.Add(new FleetUnit("Sentinel", true, new Vector2Int(7,4), 50, 14, 3, 2));
            foreach (var cell in new[] { new Vector2Int(3,1), new Vector2Int(4,3), new Vector2Int(2,4), new Vector2Int(5,0) }) Obstacles.Add(cell);
        }

        /// <summary>Deep copy used by AI simulations. Scene objects are never referenced.</summary>
        public FleetBattleState Clone(int simulationSalt = 0)
        {
            var copy = new FleetBattleState(true)
            {
                RepairUsed = RepairUsed,
                Phase = Phase,
                Round = Round,
                LastAttackResult = LastAttackResult,
                randomState = MixRandom(randomState, simulationSalt)
            };
            foreach (var cell in Obstacles) copy.Obstacles.Add(cell);
            foreach (var unit in Units)
            {
                copy.Units.Add(new FleetUnit(unit.name, unit.enemy, unit.cell, unit.maxHP, unit.attack, unit.range, unit.move)
                {
                    hp = unit.hp,
                    ap = unit.ap,
                    accuracy = unit.accuracy,
                    critRate = unit.critRate,
                    criticalMultiplier = unit.criticalMultiplier,
                    fieldRepairs = unit.fieldRepairs
                });
            }
            return copy;
        }

        private static uint MixRandom(uint state, int salt)
        {
            if (salt == 0) return state;
            uint mixed = state ^ unchecked((uint)salt * 0x9E3779B9u);
            mixed ^= mixed >> 16; mixed *= 0x7FEB352Du;
            mixed ^= mixed >> 15; mixed *= 0x846CA68Bu;
            mixed ^= mixed >> 16;
            return mixed == 0 ? 1u : mixed;
        }
        public static int Distance(Vector2Int a, Vector2Int b)
        { Vector2Int d = a - b; return (Math.Abs(d.x) + Math.Abs(d.y) + Math.Abs(d.x + d.y)) / 2; }
        public bool Inside(Vector2Int c) => c.x >= 0 && c.x < Width && c.y >= 0 && c.y < Height;
        public FleetUnit At(Vector2Int c) => Units.FirstOrDefault(u => u.Alive && u.cell == c);
        public bool CanAct(FleetUnit u) => u != null && Units.Contains(u) && u.Alive && u.ap > 0 && !Finished && u.enemy == (Phase == FleetPhase.Enemy);
        public List<Vector2Int> Path(FleetUnit unit, Vector2Int destination)
        {
            if (unit == null || !Inside(destination) || Obstacles.Contains(destination) || (At(destination) != null && destination != unit.cell)) return null;
            var previous = new Dictionary<Vector2Int, Vector2Int>();
            var queue = new Queue<Vector2Int>(); queue.Enqueue(unit.cell); previous[unit.cell] = unit.cell;
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                if (cell == destination)
                {
                    var path = new List<Vector2Int> { cell };
                    while (cell != unit.cell) { cell = previous[cell]; path.Add(cell); }
                    path.Reverse(); return path;
                }
                foreach (var direction in Directions)
                {
                    var next = cell + direction;
                    if (!Inside(next) || Obstacles.Contains(next) || At(next) != null || previous.ContainsKey(next)) continue;
                    previous[next] = cell; queue.Enqueue(next);
                }
            }
            return null;
        }
        public bool CanMove(FleetUnit u, Vector2Int cell)
        {
            if (!CanAct(u)) return false;
            var path = Path(u, cell); return path != null && path.Count > 1 && path.Count - 1 <= u.move;
        }
        /// <summary>All legal destinations and path distances from one bounded flood fill.</summary>
        public Dictionary<Vector2Int, int> ReachableCells(FleetUnit unit)
        {
            var distances = new Dictionary<Vector2Int, int>();
            if (!CanAct(unit)) return distances;
            var occupied = new HashSet<Vector2Int>(Units.Where(other => other.Alive && other != unit).Select(other => other.cell));
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(unit.cell); distances[unit.cell] = 0;
            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue(); int distance = distances[cell];
                if (distance >= unit.move) continue;
                foreach (var direction in Directions)
                {
                    Vector2Int next = cell + direction;
                    if (!Inside(next) || Obstacles.Contains(next) || occupied.Contains(next) || distances.ContainsKey(next)) continue;
                    distances[next] = distance + 1; queue.Enqueue(next);
                }
            }
            distances.Remove(unit.cell);
            return distances;
        }
        public bool Move(FleetUnit u, Vector2Int cell)
        { if (!CanMove(u, cell)) return false; u.cell = cell; u.ap--; return true; }
        public bool HasSight(Vector2Int from, Vector2Int to)
        {
            int length = Distance(from, to);
            for (int i = 1; i < length; i++)
            {
                var sample = RoundAxial(Vector2.Lerp(from, to, (float)i / length));
                if (Obstacles.Contains(sample)) return false;
            }
            return true;
        }
        public static Vector2Int RoundAxial(Vector2 p)
        {
            float z = -p.x - p.y;
            int x = Mathf.RoundToInt(p.x), y = Mathf.RoundToInt(p.y), rz = Mathf.RoundToInt(z);
            float dx = Mathf.Abs(x - p.x), dy = Mathf.Abs(y - p.y), dz = Mathf.Abs(rz - z);
            if (dx > dy && dx > dz) x = -y - rz;
            else if (dy > dz) y = -x - rz;
            return new Vector2Int(x, y);
        }
        public bool CanAttack(FleetUnit u, FleetUnit target) => CanAct(u) && target != null && Units.Contains(target) && target.Alive && u.enemy != target.enemy && Distance(u.cell, target.cell) <= u.range && HasSight(u.cell, target.cell);
        public bool Attack(FleetUnit u, FleetUnit target)
        {
            if (!CanAttack(u, target)) return false;
            u.ap--;
            if (NextRoll() >= Mathf.Clamp01(u.accuracy))
            {
                LastAttackResult = new FleetAttackResult { outcome = FleetAttackOutcome.Miss, damage = 0 };
                return true;
            }
            bool critical = NextRoll() < Mathf.Clamp01(u.critRate);
            int damage = Math.Max(1, Mathf.RoundToInt(u.attack * (critical ? Math.Max(1f, u.criticalMultiplier) : 1f)));
            target.hp = Math.Max(0, target.hp - damage);
            LastAttackResult = new FleetAttackResult
            {
                outcome = critical ? FleetAttackOutcome.Critical : FleetAttackOutcome.Hit,
                damage = damage
            };
            if (!Units.Any(v => v.enemy && v.Alive)) Phase = FleetPhase.Victory;
            else if (!Units.Any(v => !v.enemy && v.Alive)) Phase = FleetPhase.Defeat;
            return true;
        }
        private float NextRoll()
        {
            randomState ^= randomState << 13;
            randomState ^= randomState >> 17;
            randomState ^= randomState << 5;
            return (randomState & 0x00FFFFFFu) / 16777216f;
        }
        public bool CanRepair(FleetUnit u) => CanAct(u) && !u.enemy && !RepairUsed && u.hp < u.maxHP && Distance(u.cell, RepairCell) <= 1;
        public bool Repair(FleetUnit u)
        { if (!CanRepair(u)) return false; u.ap--; u.hp = Math.Min(u.maxHP, u.hp + 20); RepairUsed = true; return true; }
        public bool CanFieldRepair(FleetUnit u) => CanAct(u) && u.fieldRepairs > 0 && u.hp < u.maxHP;
        public bool FieldRepair(FleetUnit u)
        {
            if (!CanFieldRepair(u)) return false;
            u.ap--; u.fieldRepairs--; u.hp = Math.Min(u.maxHP, u.hp + 12); return true;
        }
        public void EndPhase()
        {
            if (Finished) return;
            if (Phase == FleetPhase.Player) Phase = FleetPhase.Enemy;
            else { Phase = FleetPhase.Player; Round++; }
            foreach (var u in Units) if (u.Alive && u.enemy == (Phase == FleetPhase.Enemy)) u.ap = 2;
        }
        /// <summary>Choose a legal approach cell, preferring a firing position.</summary>
        public Vector2Int? Approach(FleetUnit unit)
        {
            if (!CanAct(unit)) return null;
            var opponents = Units.Where(u => u.Alive && u.enemy != unit.enemy).ToArray();
            if (!CanAct(unit) || opponents.Length == 0) return null;
            Vector2Int? best = null; int score = int.MaxValue;
            for (int q = 0; q < Width; q++) for (int r = 0; r < Height; r++)
            {
                var cell = new Vector2Int(q,r); if (!CanMove(unit, cell)) continue;
                int distance = opponents.Min(u => Distance(cell, u.cell));
                bool firing = opponents.Any(u => Distance(cell,u.cell) <= unit.range && HasSight(cell,u.cell));
                int candidate = firing ? Path(unit, cell).Count * 5 - distance : 100 + distance * 5 + Path(unit, cell).Count;
                if (candidate < score) { score = candidate; best = cell; }
            }
            return best;
        }
    }
}
