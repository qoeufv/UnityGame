using System;
using System.Collections.Generic;
using UnityEngine;

namespace StrategyRPG.Combat
{
    [CreateAssetMenu(menuName = "Strategy RPG/Character")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] private string displayName = "Character";
        [SerializeField] private CharacterStatBlock baseStats = CharacterStatBlock.Default;
        [SerializeField] private WeaponType unarmedWeaponType = WeaponType.Fist;
        [SerializeField] private GameObject battlePrefab;
        [SerializeField] private EquipmentData[] equipment = Array.Empty<EquipmentData>();
        [SerializeField] private SkillData[] skills = Array.Empty<SkillData>();

        public string DisplayName => displayName;
        public CharacterStatBlock BaseStats => baseStats.Clamped();
        public WeaponType UnarmedWeaponType => unarmedWeaponType;
        public GameObject BattlePrefab => battlePrefab;
        public IReadOnlyList<EquipmentData> Equipment => equipment;
        public IReadOnlyList<SkillData> Skills => skills;
    }
}
