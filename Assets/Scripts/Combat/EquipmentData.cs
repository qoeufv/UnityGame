using UnityEngine;

namespace StrategyRPG.Combat
{
    [CreateAssetMenu(menuName = "Strategy RPG/Equipment")]
    public class EquipmentData : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private EquipmentSlot slot;
        [SerializeField] private WeaponType weaponType = WeaponType.Fist;
        [SerializeField] private CharacterStatBlock statBonuses;
        [SerializeField] private GameObject visualPrefab;

        public string DisplayName => displayName;
        public EquipmentSlot Slot => slot;
        public WeaponType WeaponType => weaponType;
        public CharacterStatBlock StatBonuses => statBonuses;
        public GameObject VisualPrefab => visualPrefab;
    }
}
