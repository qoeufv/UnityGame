using System;
using StrategyRPG.Combat;
using UnityEditor;
using UnityEngine;

/// <summary>Deterministic regression suite; uses temporary assets and does not edit the scene.</summary>
public static class BattleDataValidation
{
    [MenuItem("Tools/Validate Battle Data and Timing %&b")]
    public static void Validate()
    {
        SkillData skill = AssetDatabase.LoadAssetAtPath<SkillData>("Assets/ScriptableObjects/Skills/FlameSunPalm.asset");
        CharacterData character = ScriptableObject.CreateInstance<CharacterData>();
        EquipmentData ring = ScriptableObject.CreateInstance<EquipmentData>();
        EquipmentData sword = ScriptableObject.CreateInstance<EquipmentData>();
        try
        {
            Require(skill != null && skill.Category == SkillCategory.MartialArt &&
                skill.WeaponRequirement == WeaponType.Fist && Mathf.Approximately(skill.DamageMultiplier, 1.3f) &&
                Mathf.Approximately(skill.Accuracy, 0.9f), "Flame Sun Palm authored values");
            CombatantStats attacker = new CombatantStats(character);
            Require(Mathf.Approximately(attacker.Stats.CritRate, 0.03f), "Default crit rate is 3%");
            SerializedObject item = new SerializedObject(ring);
            item.FindProperty("slot").enumValueIndex = (int)EquipmentSlot.Ring;
            item.FindProperty("statBonuses.Attack").intValue = 8;
            item.FindProperty("statBonuses.CritRate").floatValue = 0.02f;
            item.ApplyModifiedPropertiesWithoutUndo();
            attacker.Equip(ring);
            Require(attacker.Stats.Attack == 20 && Mathf.Approximately(attacker.Stats.CritRate, 0.05f), "Equipment adds stats");
            attacker.Equip(ring);
            Require(attacker.Stats.Attack == 20, "Equipping the same slot replaces, never stacks");
            Require(character.BaseStats.Attack == 12, "Shared character data stays unchanged");

            CombatantStats target = new CombatantStats("Target", 1000);
            SkillResult result = SkillResolver.Resolve(attacker, target, skill, 0.1f, 0.5f);
            Require(result.Hit && !result.Critical && result.Damage == 26, "Multiplier resolves at impact");
            Require(target.RemainingTurns(StatusEffectType.Burning) == 3, "Burning applied for 3 turns");
            Require(target.EndTurn() == 10 && target.EndTurn() == 10 && target.EndTurn() == 10 &&
                target.EndTurn() == 0, "Burning deals 1% max HP exactly three times");
            Require(target.RemainingTurns(StatusEffectType.Burning) == 0, "Burning expires");
            int health = target.CurrentHealth;
            result = SkillResolver.Resolve(attacker, target, skill, 0.95f, 0f);
            Require(!result.Hit && target.CurrentHealth == health &&
                target.RemainingTurns(StatusEffectType.Burning) == 0, "Miss applies neither damage nor effects");
            result = SkillResolver.Resolve(attacker, target, skill, 0f, 0f);
            Require(result.Critical && result.Damage == 52, "Critical doubles post-defense damage");
            target.EndTurn();
            SkillResolver.Resolve(attacker, target, skill, 0f, 0.5f);
            Require(target.RemainingTurns(StatusEffectType.Burning) == 3 && target.EndTurn() == 10,
                "Reapplying Burning refreshes duration without stacking");
            target.ResetToFullHealth();
            Require(target.RemainingTurns(StatusEffectType.Burning) == 0, "Battle reset clears effects");

            item = new SerializedObject(sword);
            item.FindProperty("slot").enumValueIndex = (int)EquipmentSlot.Weapon;
            item.FindProperty("weaponType").intValue = (int)WeaponType.Sword;
            item.ApplyModifiedPropertiesWithoutUndo();
            attacker.Equip(sword);
            bool rejected = false;
            try { SkillResolver.Resolve(attacker, target, skill, 0f, 0f); }
            catch (InvalidOperationException) { rejected = true; }
            Require(rejected && target.CurrentHealth == target.MaxHealth, "Weapon requirement rejects without mutation");
            attacker.Unequip(EquipmentSlot.Weapon);
            Require(attacker.Weapon == WeaponType.Fist, "No weapon means Fist");

            SkillImpactClock clock = new SkillImpactClock();
            int impacts = 0;
            clock.Impact += (id, playedSkill) => { Require(playedSkill == skill, "Impact skill identity"); impacts++; };
            int timedId = clock.Begin(skill, SkillImpactMode.Timed, 0.35f);
            clock.Tick(0.2f);
            clock.NotifyAnimationImpact(timedId);
            Require(impacts == 0, "Timed action ignores animation callbacks and waits for impact");
            clock.Tick(0.2f);
            clock.Tick(10f);
            Require(impacts == 1, "Timed impact occurs exactly once");
            int eventId = clock.Begin(skill, SkillImpactMode.AnimationEvent, 0f);
            clock.Tick(10f);
            clock.NotifyAnimationImpact(timedId);
            Require(impacts == 1, "Event mode ignores time and stale action IDs");
            clock.NotifyAnimationImpact(eventId);
            clock.NotifyAnimationImpact(eventId);
            Require(impacts == 2, "Animation event resolves exactly once");
            eventId = clock.Begin(skill, SkillImpactMode.AnimationEvent, 0f);
            clock.Cancel();
            clock.NotifyAnimationImpact(eventId);
            Require(impacts == 2 && !clock.Pending, "Cancelled actions cannot impact");
            Debug.Log("BATTLE_DATA_VALIDATION_PASSED: assets, stats, equipment, hit/miss/crit, Burning, weapon checks, timed/event impact, cancellation.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(character);
            UnityEngine.Object.DestroyImmediate(ring);
            UnityEngine.Object.DestroyImmediate(sword);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Battle data validation failed: " + message);
    }
}
