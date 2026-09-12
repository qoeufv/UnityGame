using System;
using UnityEngine;

namespace StrategyRPG.Combat
{
    // Keep numeric values compatible with existing Animator parameters and prefab serialization.
    public enum WeaponType { None = 0, Fist = 1, Sword = 2, Saber = 3, Staff = 4 }
    public enum SkillCategory { MartialArt = 0, InnerSkill = 1, Qinggong = 2 }
    public enum EquipmentSlot { Helmet, Bracer, ChestArmor, Weapon, Ring, Necklace }
    public enum StatusEffectType { Burning }
    public enum SkillImpactMode { Timed, AnimationEvent }

    [Serializable]
    public struct CharacterStatBlock
    {
        public int Attack;
        public int Defense;
        public int HP;
        [Range(0f, 1f)] public float Dodge;
        public int Speed;
        [Range(0f, 1f)] public float CritRate;

        public static CharacterStatBlock Default => new CharacterStatBlock
        { Attack = 12, Defense = 0, HP = 100, Speed = 10, CritRate = 0.03f };

        public CharacterStatBlock Add(CharacterStatBlock bonus) => new CharacterStatBlock
        {
            Attack = Attack + bonus.Attack, Defense = Defense + bonus.Defense,
            HP = HP + bonus.HP, Dodge = Dodge + bonus.Dodge,
            Speed = Speed + bonus.Speed, CritRate = CritRate + bonus.CritRate
        };

        public CharacterStatBlock Clamped() => new CharacterStatBlock
        {
            Attack = Mathf.Max(0, Attack), Defense = Mathf.Max(0, Defense),
            HP = Mathf.Max(1, HP), Dodge = Mathf.Clamp01(Dodge),
            Speed = Mathf.Max(0, Speed), CritRate = Mathf.Clamp01(CritRate)
        };
    }
}
