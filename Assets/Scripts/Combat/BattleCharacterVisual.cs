using System;
using UnityEngine;

namespace StrategyRPG.Combat
{
    /// <summary>
    /// Visual representation and animation controller for a turn-based battle combatant.
    /// Decoupled from damage formulas and stats. Provides sockets, animation triggers,
    /// and weapon/effect attachment points.
    /// </summary>
    [SelectionBase]
    public class BattleCharacterVisual : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform modelRoot;

        [Header("Sockets & Anchors")]
        [SerializeField] private Transform weaponSocketR;
        [SerializeField] private Transform weaponSocketL;
        [SerializeField] private Transform effectRoot;
        [SerializeField] private Transform hitPoint;
        [SerializeField] private Transform uiAnchor;

        [Header("Weapon Prefabs / Templates")]
        [SerializeField] private GameObject swordPrefab;
        [SerializeField] private GameObject saberPrefab;
        [SerializeField] private GameObject staffPrefab;
        [SerializeField] private GameObject fistPrefab;

        [Header("Current State (Read Only)")]
        [SerializeField] private WeaponType currentWeapon = WeaponType.Sword;
        [SerializeField] private bool isDefending = false;

        private GameObject currentEquippedWeaponObj;

        // Animator Parameter Hashes
        private static readonly int ParamAttack = Animator.StringToHash("Attack");
        private static readonly int ParamSkill = Animator.StringToHash("Skill");
        private static readonly int ParamSkillType = Animator.StringToHash("SkillType");
        private static readonly int ParamWeaponType = Animator.StringToHash("WeaponType");
        private static readonly int ParamDefend = Animator.StringToHash("Defend");
        private static readonly int ParamHit = Animator.StringToHash("Hit");
        private static readonly int ParamDie = Animator.StringToHash("Die");
        private static readonly int ParamVictory = Animator.StringToHash("Victory");
        private static readonly int ParamDodge = Animator.StringToHash("Dodge");
        private static readonly int ParamReset = Animator.StringToHash("Reset");

        public Animator AnimatorComponent => animator;
        public Transform ModelRoot => modelRoot;
        public Transform WeaponSocketR => weaponSocketR;
        public Transform WeaponSocketL => weaponSocketL;
        public Transform EffectRoot => effectRoot;
        public Transform HitPoint => hitPoint;
        public Transform UIAnchor => uiAnchor;
        public WeaponType CurrentWeapon => currentWeapon;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (modelRoot == null && animator != null)
            {
                modelRoot = animator.transform;
            }

            EquipWeapon(currentWeapon);
        }

        #region Animation Triggers
        /// <summary>
        /// Plays basic idle martial stance.
        /// </summary>
        public void PlayIdle()
        {
            isDefending = false;
            if (animator != null)
            {
                animator.SetBool(ParamDefend, false);
                animator.SetTrigger(ParamReset);
            }
        }

        /// <summary>
        /// Plays basic attack animation with current or specified weapon.
        /// </summary>
        public void PlayAttack(WeaponType weapon = WeaponType.None)
        {
            if (weapon != WeaponType.None && weapon != currentWeapon)
            {
                EquipWeapon(weapon);
            }

            if (animator != null)
            {
                animator.SetInteger(ParamWeaponType, (int)currentWeapon);
                animator.SetTrigger(ParamAttack);
            }
        }

        /// <summary>
        /// Plays skill animation based on wuxia skill category.
        /// </summary>
        public void PlaySkill(SkillCategory category)
        {
            if (animator != null)
            {
                animator.SetInteger(ParamSkillType, (int)category);
                animator.SetTrigger(ParamSkill);
            }
        }

        public void PlaySkill(SkillData skill)
        {
            if (skill == null || animator == null || animator.runtimeAnimatorController == null) return;
            isDefending = false;
            animator.SetBool(ParamDefend, false);
            animator.SetInteger(ParamSkillType, (int)skill.Category);
            if (!string.IsNullOrWhiteSpace(skill.AnimationTriggerName))
                animator.SetTrigger(skill.AnimationTriggerName);
        }

        /// <summary>
        /// Toggles defensive guard posture.
        /// </summary>
        public void PlayDefend(bool defend = true)
        {
            isDefending = defend;
            if (animator != null)
            {
                animator.SetBool(ParamDefend, defend);
                if (defend)
                {
                    animator.SetTrigger(ParamDefend);
                }
            }
        }

        /// <summary>
        /// Plays hit reaction recoil animation.
        /// </summary>
        public void PlayHit()
        {
            if (animator != null)
            {
                animator.SetTrigger(ParamHit);
            }
        }

        /// <summary>
        /// Plays death collapse animation.
        /// </summary>
        public void PlayDeath()
        {
            if (animator != null)
            {
                animator.SetTrigger(ParamDie);
            }
        }

        /// <summary>
        /// Plays victory martial salute / flourish.
        /// </summary>
        public void PlayVictory()
        {
            if (animator != null)
            {
                animator.SetTrigger(ParamVictory);
            }
        }

        /// <summary>
        /// Plays evasive qinggong leap / dodge.
        /// </summary>
        public void PlayDodge()
        {
            if (animator != null)
            {
                animator.SetTrigger(ParamDodge);
            }
        }
        #endregion

        #region Weapon Sockets
        /// <summary>
        /// Equips a weapon into the right-hand socket and updates the animator weapon type.
        /// </summary>
        public void EquipWeapon(WeaponType weapon)
        {
            currentWeapon = weapon;

            if (currentEquippedWeaponObj != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(currentEquippedWeaponObj);
                }
                else
                {
                    DestroyImmediate(currentEquippedWeaponObj);
                }
                currentEquippedWeaponObj = null;
            }

            GameObject prefabToSpawn = null;
            switch (weapon)
            {
                case WeaponType.Sword: prefabToSpawn = swordPrefab; break;
                case WeaponType.Saber: prefabToSpawn = saberPrefab; break;
                case WeaponType.Staff: prefabToSpawn = staffPrefab; break;
                case WeaponType.Fist: prefabToSpawn = fistPrefab; break;
            }

            if (prefabToSpawn != null && weaponSocketR != null)
            {
                currentEquippedWeaponObj = Instantiate(prefabToSpawn, weaponSocketR);
                currentEquippedWeaponObj.transform.localPosition = Vector3.zero;
                currentEquippedWeaponObj.transform.localRotation = Quaternion.identity;
            }

            if (animator != null)
            {
                animator.SetInteger(ParamWeaponType, (int)weapon);
            }
        }
        #endregion

        #region Effect & Position Queries
        public Vector3 GetHitPosition()
        {
            return hitPoint != null ? hitPoint.position : transform.position + Vector3.up * 1.2f;
        }

        public Vector3 GetUIPosition()
        {
            return uiAnchor != null ? uiAnchor.position : transform.position + Vector3.up * 2.3f;
        }

        public GameObject SpawnEffect(GameObject effectPrefab, bool parentToCharacter = false)
        {
            if (effectPrefab == null) return null;
            Transform parent = parentToCharacter ? (effectRoot != null ? effectRoot : transform) : null;
            Vector3 spawnPos = effectRoot != null ? effectRoot.position : transform.position;
            return Instantiate(effectPrefab, spawnPos, Quaternion.identity, parent);
        }
        #endregion
    }
}
