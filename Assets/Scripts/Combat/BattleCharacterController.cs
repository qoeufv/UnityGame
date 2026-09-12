using System;
using UnityEngine;

namespace StrategyRPG.Combat
{
    /// <summary>Presentation facade. Reports impact timing; never applies gameplay effects.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BattleCharacterVisual))]
    public class BattleCharacterController : MonoBehaviour
    {
        [SerializeField] private BattleCharacterVisual visual;
        [SerializeField] private SkillImpactMode basicAttackImpactMode;
        [SerializeField, Min(0f)] private float basicAttackImpactTime = 0.3f;
        private readonly SkillImpactClock clock = new SkillImpactClock();

        public event Action<int, SkillData> Impact;
        public int CurrentActionId => clock.ActionId;
        public bool HasPendingImpact => clock.Pending;
        public BattleCharacterVisual Visual => EnsureVisual();

        private BattleCharacterVisual EnsureVisual()
        {
            if (visual == null) visual = GetComponent<BattleCharacterVisual>();
            return visual;
        }

        private void Awake()
        {
            EnsureVisual();
            clock.Impact += ForwardImpact;
            Animator animator = visual != null ? visual.AnimatorComponent : null;
            if (animator == null) animator = GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                BattleAnimationEventRelay relay = animator.GetComponent<BattleAnimationEventRelay>();
                if (relay == null) relay = animator.gameObject.AddComponent<BattleAnimationEventRelay>();
                relay.Bind(this, animator);
            }
        }

        private void Update() => clock.Tick(Time.deltaTime);
        private void OnDisable() => CancelAction();
        private void OnDestroy() => clock.Impact -= ForwardImpact;
        private void ForwardImpact(int id, SkillData skill) => Impact?.Invoke(id, skill);
        public void CancelAction() => clock.Cancel();
        public void NotifyAnimationImpact(int actionId) => clock.NotifyAnimationImpact(actionId);

        public void PlayBasicAttack()
        {
            clock.Begin(null, basicAttackImpactMode, basicAttackImpactTime);
            EnsureVisual()?.PlayDefend(false);
            EnsureVisual()?.PlayAttack();
        }

        public void PlaySkill(SkillData skill)
        {
            if (skill == null) throw new ArgumentNullException(nameof(skill));
            clock.Begin(skill, skill.ImpactMode, skill.ImpactTime);
            EnsureVisual()?.PlaySkill(skill);
        }

        public void PlayDefend() { CancelAction(); EnsureVisual()?.PlayDefend(true); }
        public void PlayHit() { CancelAction(); EnsureVisual()?.PlayHit(); }
        public void PlayDeath() { CancelAction(); EnsureVisual()?.PlayDeath(); }
        public void PlayIdle() { CancelAction(); EnsureVisual()?.PlayIdle(); }
    }
}
