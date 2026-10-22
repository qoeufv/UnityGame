using System;
using System.Linq;
using StrategyRPG.Tactics;
using UnityEditor;
using UnityEngine;

/// <summary>Pure encounter-rule checks; never reads or modifies the open scene.</summary>
public static class FleetTacticsValidation
{
    [MenuItem("Tools/Validate Fleet Tactics Rules")]
    public static void Validate()
    {
        Movement();
        Phases();
        Attacks();
        RandomAttacks();
        Repairs();
        Outcomes();
        FullEncounter();
        Debug.Log("FLEET_TACTICS_RULES_PASSED: movement, phases, sight, accuracy, dodge, critical hits, repair, outcomes and full encounter.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Fleet tactics validation failed: " + message);
    }

    private static FleetBattleState Empty()
    {
        var state = new FleetBattleState();
        state.Units.Clear();
        state.Obstacles.Clear();
        return state;
    }

    private static FleetUnit Add(FleetBattleState state, bool enemy, int x, int y,
        int hp = 40, int attack = 15, int range = 3, int move = 3)
    {
        var unit = new FleetUnit(enemy ? "Enemy" : "Player", enemy,
            new Vector2Int(x, y), hp, attack, range, move);
        unit.accuracy = 1f; unit.critRate = 0f;
        state.Units.Add(unit);
        return unit;
    }

    private static void Movement()
    {
        var s = Empty();
        var u = Add(s, false, 0, 2);
        var blocker = Add(s, false, 1, 1);
        s.Obstacles.Add(new Vector2Int(1, 2));
        var goal = new Vector2Int(2, 2);
        var path = s.Path(u, goal);
        Check(path != null && path.First() == u.cell && path.Last() == goal,
            "path must connect its endpoints");
        Check(path.Count - 1 > FleetBattleState.Distance(u.cell, goal), "path must detour around obstacle");
        for (int i = 1; i < path.Count; i++)
            Check(s.Inside(path[i]) && !s.Obstacles.Contains(path[i]) && s.At(path[i]) == null &&
                FleetBattleState.Distance(path[i - 1], path[i]) == 1, "every path step must be adjacent and unoccupied");
        Check(!s.Move(u, blocker.cell) && !s.Move(u, new Vector2Int(1, 2)) &&
            !s.Move(u, new Vector2Int(-1, 2)) && !s.Move(u, u.cell) && u.ap == 2,
            "invalid movement must not spend AP");
        u.move = 2;
        Check(!s.Move(u, goal), "movement range must use path length, not direct distance");
        u.move = path.Count - 1;
        Check(s.Move(u, goal) && u.ap == 1 && u.cell == goal, "valid movement spends one AP");
        u.ap = 0;
        Check(!s.Move(u, new Vector2Int(2, 3)), "zero AP must forbid movement");
        u.ap = 2;
        s.Obstacles.Clear();
        foreach (var c in new[] { new Vector2Int(3, 2), new Vector2Int(3, 1), new Vector2Int(2, 1),
            new Vector2Int(1, 2), new Vector2Int(1, 3), new Vector2Int(2, 3) }) s.Obstacles.Add(c);
        Check(s.Path(u, new Vector2Int(6, 5)) == null, "enclosed unit must have no escape path");
    }

    private static void Phases()
    {
        var s = Empty();
        var player = Add(s, false, 0, 0);
        var enemy = Add(s, true, 1, 0);
        Check(!s.Attack(enemy, player) && !s.Move(enemy, new Vector2Int(2, 0)), "enemy cannot act in player phase");
        player.ap = 0;
        enemy.ap = 0;
        s.EndPhase();
        Check(s.Phase == FleetPhase.Enemy && s.Round == 1 && enemy.ap == 2 && player.ap == 0,
            "enemy phase refreshes only enemy AP");
        player.ap = 2;
        Check(!s.Attack(player, enemy) && !s.Move(player, new Vector2Int(0, 1)),
            "player cannot act during enemy phase even with AP");
        Check(s.Attack(enemy, player) && enemy.ap == 1, "enemy may attack in its phase");
        s.EndPhase();
        Check(s.Phase == FleetPhase.Player && s.Round == 2 && player.ap == 2 && enemy.ap == 1,
            "new round refreshes player AP only");
    }

    private static void Attacks()
    {
        var s = Empty();
        var u = Add(s, false, 0, 0);
        var target = Add(s, true, 3, 0);
        var ally = Add(s, false, 0, 1);
        Check(s.CanAttack(u, target), "target exactly at maximum range must be legal");
        target.cell = new Vector2Int(4, 0);
        Check(!s.Attack(u, target) && u.ap == 2 && target.hp == 40, "out-of-range attack must not mutate state");
        target.cell = new Vector2Int(3, 0);
        s.Obstacles.Add(new Vector2Int(1, 0));
        Check(!s.HasSight(u.cell, target.cell) && !s.Attack(u, target), "obstacle must block firing line");
        s.Obstacles.Clear();
        Check(!s.Attack(u, ally) && !s.Attack(u, u), "friendly fire and self attack must be rejected");
        Check(s.Attack(u, target) && target.hp == 25 && u.ap == 1, "attack applies damage and consumes one AP");
        Check(s.Attack(u, target) && !s.Attack(u, target) && target.hp == 10,
            "third attack must be rejected after AP exhaustion");
    }

    private static void Repairs()
    {
        var s = Empty();
        var u = Add(s, false, 0, 0);
        Check(!s.Repair(u), "full-health repair must be rejected");
        u.hp = 10;
        Check(!s.Repair(u), "distant repair must be rejected");
        u.cell = s.RepairCell;
        u.ap = 0;
        Check(!s.Repair(u) && !s.RepairUsed, "repair requires AP");
        u.ap = 2;
        Check(s.Repair(u) && u.hp == 30 && u.ap == 1 && s.RepairUsed,
            "repair restores 20 HP and spends one AP");
        Check(!s.Repair(u) && u.hp == 30 && u.ap == 1, "repair station is single use per encounter");
        s = Empty();
        u = Add(s, false, 3, 3);
        u.hp = 35;
        Check(s.Repair(u) && u.hp == u.maxHP, "adjacent repair must cap at max HP");
        s = Empty();
        var enemy = Add(s, true, 3, 2);
        enemy.hp = 10;
        s.EndPhase();
        Check(!s.Repair(enemy), "enemy may not consume player repair station");
    }

    private static void RandomAttacks()
    {
        var missState = Empty();
        var dodgedShooter = Add(missState, false, 0, 0, attack: 20);
        var dodgingTarget = Add(missState, true, 1, 0);
        dodgedShooter.accuracy = 0f;
        Check(missState.Attack(dodgedShooter, dodgingTarget) && dodgedShooter.ap == 1 && dodgingTarget.hp == 40 &&
            missState.LastAttackResult.outcome == FleetAttackOutcome.Miss && missState.LastAttackResult.damage == 0,
            "missed attack must spend AP, deal no damage and report dodge");

        var critState = Empty();
        var criticalShooter = Add(critState, false, 0, 0, attack: 20);
        var criticalTarget = Add(critState, true, 1, 0, hp: 40);
        criticalShooter.critRate = 1f; criticalShooter.criticalMultiplier = 1.5f;
        Check(critState.Attack(criticalShooter, criticalTarget) && criticalTarget.hp == 10 &&
            critState.LastAttackResult.outcome == FleetAttackOutcome.Critical && critState.LastAttackResult.damage == 30,
            "critical attack must apply configured multiplier and report critical result");
    }

    private static void Outcomes()
    {
        foreach (bool enemyWins in new[] { false, true })
        {
            var s = Empty();
            var player = Add(s, false, 0, 0, 10, 50);
            var enemy = Add(s, true, 1, 0, 10, 50);
            if (enemyWins) s.EndPhase();
            var attacker = enemyWins ? enemy : player;
            var victim = enemyWins ? player : enemy;
            Check(s.Attack(attacker, victim) && victim.hp == 0 && !victim.Alive && s.At(victim.cell) == null,
                "lethal damage clamps HP and removes occupancy");
            var expected = enemyWins ? FleetPhase.Defeat : FleetPhase.Victory;
            Check(s.Finished && s.Phase == expected, "last unit death sets correct outcome");
            int round = s.Round;
            s.EndPhase();
            Check(s.Phase == expected && s.Round == round && !s.CanAct(attacker) && !s.CanAct(victim) &&
                !s.Move(attacker, new Vector2Int(0, 1)) && !s.Attack(attacker, victim),
                "terminal battle forbids further actions and turn changes");
        }
        var live = Empty();
        var shooter = Add(live, false, 0, 0, 40, 50);
        var dead = Add(live, true, 1, 0, 10);
        Add(live, true, 7, 5);
        Check(live.Attack(shooter, dead) && !live.Finished && !live.Attack(shooter, dead) && !live.CanAct(dead),
            "nonfinal death must not end encounter or allow targeting corpse");
        Check(live.Move(shooter, dead.cell), "dead unit tile becomes traversable");
    }

    private static void FullEncounter()
    {
        var s = new FleetBattleState();
        foreach (var unit in s.Units) { unit.accuracy = 1f; unit.critRate = 0f; }
        int actions = 0;
        for (int phase = 0; phase < 160 && !s.Finished; phase++)
        {
            bool enemyTurn = s.Phase == FleetPhase.Enemy;
            foreach (var unit in s.Units.Where(u => u.Alive && u.enemy == enemyTurn).ToArray())
            {
                for (int budget = 0; budget < 2 && s.CanAct(unit); budget++)
                {
                    int ap = unit.ap;
                    var target = s.Units.Where(t => s.CanAttack(unit, t)).OrderBy(t => t.hp).FirstOrDefault();
                    if (target != null) Check(s.Attack(unit, target), "selected legal attack must execute");
                    else
                    {
                        var destination = s.Approach(unit);
                        if (!destination.HasValue) break;
                        Check(s.Move(unit, destination.Value), "AI approach must return executable move");
                    }
                    actions++;
                    Check(unit.ap == ap - 1, "every automated action spends exactly one AP");
                    Check(s.Units.All(u => u.hp >= 0 && u.hp <= u.maxHP && u.ap >= 0 && s.Inside(u.cell)),
                        "encounter must preserve health, AP and board bounds");
                    var alive = s.Units.Where(u => u.Alive).ToArray();
                    Check(alive.Select(u => u.cell).Distinct().Count() == alive.Length &&
                        alive.All(u => !s.Obstacles.Contains(u.cell)), "living units cannot overlap or occupy obstacles");
                }
            }
            s.EndPhase();
        }
        Check(s.Finished && actions > 6 && s.Round > 1, "both-side automated encounter must finish within 80 rounds");
        Check(s.Phase == FleetPhase.Victory ? !s.Units.Any(u => u.enemy && u.Alive) : !s.Units.Any(u => !u.enemy && u.Alive),
            "automated result must agree with surviving teams");
        Debug.Log("Fleet tactics automated encounter: " + s.Phase + ", round " + s.Round + ", actions " + actions);
    }
}
