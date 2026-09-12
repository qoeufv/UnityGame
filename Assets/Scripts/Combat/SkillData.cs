using System;
using System.Collections.Generic;
using UnityEngine;

namespace StrategyRPG.Combat
{
    [Serializable]
    public struct StatusEffectDefinition
    {
        public StatusEffectType Type;
        [Min(1)] public int DurationTurns;
        [Range(0f, 1f)] public float MaxHPDamagePerTurn;
    }

    [CreateAssetMenu(menuName = "Strategy RPG/Skill")]
    public class SkillData : ScriptableObject
    {
        [SerializeField] private string displayName = "Skill";
        [SerializeField] private SkillCategory category;
        [Tooltip("None means unrestricted. Fist requires an unarmed/fist combatant.")]
        [SerializeField] private WeaponType weaponRequirement = WeaponType.None;
        [SerializeField, Min(0f)] private float damageMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float accuracy = 1f;
        [SerializeField] private StatusEffectDefinition[] statusEffects = Array.Empty<StatusEffectDefinition>();
        [Header("Presentation")]
        [SerializeField] private string animationTriggerName = "Skill";
        [SerializeField] private GameObject vfxPrefab;
        [SerializeField] private SkillImpactMode impactMode;
        [Tooltip("Seconds from PlaySkill, using scaled game time.")]
        [SerializeField, Min(0f)] private float impactTime = 0.35f;

        public string DisplayName => displayName;
        public SkillCategory Category => category;
        public WeaponType WeaponRequirement => weaponRequirement;
        public float DamageMultiplier => Mathf.Max(0f, damageMultiplier);
        public float Accuracy => Mathf.Clamp01(accuracy);
        public IReadOnlyList<StatusEffectDefinition> StatusEffects => statusEffects;
        public string AnimationTriggerName => animationTriggerName;
        public GameObject VfxPrefab => vfxPrefab;
        public SkillImpactMode ImpactMode => impactMode;
        public float ImpactTime => Mathf.Max(0f, impactTime);
        public bool SupportsWeapon(WeaponType weapon) =>
            weaponRequirement == WeaponType.None || weaponRequirement == weapon;
    }
}
