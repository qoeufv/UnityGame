using UnityEngine;

namespace StrategyRPG.Combat
{
    /// <summary>Lives alongside the Animator, including when the model is a nested child.</summary>
    public class BattleAnimationEventRelay : MonoBehaviour
    {
        private BattleCharacterController owner;
        private Animator animator;
        public void Bind(BattleCharacterController controller, Animator source)
        { owner = controller; animator = source; }

        // Add an Animation Event named OnSkillImpact to the impact frame (base layer).
        public void OnSkillImpact(AnimationEvent animationEvent)
        {
            if (owner == null || animator == null || !owner.HasPendingImpact) return;
            // Ignore outgoing clips during crossfades and callbacks from another state.
            if (animator.IsInTransition(0) || animationEvent.animatorStateInfo.fullPathHash !=
                animator.GetCurrentAnimatorStateInfo(0).fullPathHash) return;
            owner.NotifyAnimationImpact(owner.CurrentActionId);
        }
    }
}
