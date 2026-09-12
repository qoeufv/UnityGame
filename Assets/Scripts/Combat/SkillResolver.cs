using System;
using UnityEngine;

namespace StrategyRPG.Combat
{
    public readonly struct SkillResult
    {
        public readonly bool Hit;
        public readonly bool Critical;
        public readonly int Damage;
        public SkillResult(bool hit, bool critical, int damage)
        { Hit = hit; Critical = critical; Damage = damage; }
    }

    /// <summary>Gameplay only: no GameObjects, Animators, coroutines, or VFX spawning.</summary>
    public static class SkillResolver
    {
        // Rolls are supplied by the caller for deterministic tests/replays. Expected range [0, 1).
        public static SkillResult Resolve(CombatantStats attacker, CombatantStats target,
            SkillData skill, float hitRoll, float critRoll)
        {
            if (attacker == null || target == null || skill == null) throw new ArgumentNullException();
            if (hitRoll < 0f || hitRoll >= 1f || critRoll < 0f || critRoll >= 1f ||
                float.IsNaN(hitRoll) || float.IsNaN(critRoll)) throw new ArgumentOutOfRangeException("roll");
            if (attacker.IsDefeated || target.IsDefeated) throw new InvalidOperationException("Combatant is defeated.");
            if (!skill.SupportsWeapon(attacker.Weapon)) throw new InvalidOperationException("Skill weapon requirement is not met.");
            if (hitRoll >= skill.Accuracy * (1f - target.Stats.Dodge)) return new SkillResult(false, false, 0);

            bool critical = critRoll < attacker.Stats.CritRate;
            float rawDamage = Mathf.Max(0f, attacker.Stats.Attack * skill.DamageMultiplier - target.Stats.Defense);
            int damage = target.TakeDamage(Mathf.RoundToInt(rawDamage * (critical ? 2f : 1f)));
            if (!target.IsDefeated)
                foreach (StatusEffectDefinition effect in skill.StatusEffects) target.ApplyStatus(effect);
            return new SkillResult(true, critical, damage);
        }
    }
}
