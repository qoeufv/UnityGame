using System;
using System.Collections.Generic;
using UnityEngine;

namespace StrategyRPG.Combat
{
    /// <summary>
    /// Per-battle state. Copies authored stats and equipment; never mutates shared assets.
    /// Status durations advance only when the battle rules call EndTurn.
    /// </summary>
    [Serializable]
    public class CombatantStats
    {
        [SerializeField] private string displayName;
        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;

        private CharacterStatBlock baseStats;
        private readonly Dictionary<EquipmentSlot, EquipmentData> equipment = new Dictionary<EquipmentSlot, EquipmentData>();
        private readonly Dictionary<StatusEffectType, StatusEffectDefinition> effects = new Dictionary<StatusEffectType, StatusEffectDefinition>();
        private WeaponType unarmedWeapon = WeaponType.Fist;
        public CharacterStatBlock Stats { get; private set; }
        public WeaponType Weapon => equipment.TryGetValue(EquipmentSlot.Weapon, out EquipmentData item)
            ? item.WeaponType : unarmedWeapon;

        public CombatantStats(CharacterData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            displayName = data.DisplayName;
            baseStats = data.BaseStats;
            unarmedWeapon = data.UnarmedWeaponType;
            foreach (EquipmentData item in data.Equipment)
            {
                if (item == null) continue;
                if (equipment.ContainsKey(item.Slot))
                    throw new ArgumentException($"{displayName} has duplicate equipment in {item.Slot}.");
                equipment.Add(item.Slot, item);
            }
            RecalculateStats();
            ResetToFullHealth();
        }

        public void Equip(EquipmentData item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            equipment[item.Slot] = item;
            RecalculateStats();
        }

        public void Unequip(EquipmentSlot slot)
        {
            equipment.Remove(slot);
            RecalculateStats();
        }

        private void RecalculateStats()
        {
            CharacterStatBlock total = baseStats;
            foreach (EquipmentData item in equipment.Values) total = total.Add(item.StatBonuses);
            Stats = total.Clamped();
            maxHealth = Stats.HP;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        public void ApplyStatus(StatusEffectDefinition effect)
        {
            if (IsDefeated || effect.DurationTurns <= 0) return;
            effect.MaxHPDamagePerTurn = Mathf.Clamp01(effect.MaxHPDamagePerTurn);
            effects[effect.Type] = effect; // Refresh, never stack duplicate Burning instances.
        }

        public int RemainingTurns(StatusEffectType type) =>
            effects.TryGetValue(type, out StatusEffectDefinition effect) ? effect.DurationTurns : 0;

        public int EndTurn()
        {
            int damage = 0;
            foreach (StatusEffectType type in new List<StatusEffectType>(effects.Keys))
            {
                StatusEffectDefinition effect = effects[type];
                if (!IsDefeated && type == StatusEffectType.Burning && effect.MaxHPDamagePerTurn > 0f)
                    damage += TakeDamage(Mathf.Max(1, Mathf.CeilToInt(MaxHealth * effect.MaxHPDamagePerTurn)));
                effect.DurationTurns--;
                if (effect.DurationTurns <= 0) effects.Remove(type);
                else effects[type] = effect;
            }
            return damage;
        }

        public string DisplayName => displayName;
        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool IsDefeated => currentHealth <= 0;

        public CombatantStats(string displayName, int maxHealth)
        {
            this.displayName = displayName;
            baseStats = CharacterStatBlock.Default;
            baseStats.HP = maxHealth;
            RecalculateStats();
            ResetToFullHealth();
        }

        public void ResetToFullHealth()
        {
            currentHealth = maxHealth;
            effects.Clear();
        }

        public int TakeDamage(int amount)
        {
            int previousHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - Mathf.Max(0, amount));
            return previousHealth - currentHealth;
        }

        public int Heal(int amount)
        {
            int previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Max(0, amount));
            return currentHealth - previousHealth;
        }
    }
}
