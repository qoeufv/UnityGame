using System;

namespace StrategyRPG.Combat
{
    /// <summary>Single-impact action clock, independent of animation and damage resolution.</summary>
    public sealed class SkillImpactClock
    {
        public event Action<int, SkillData> Impact;
        public int ActionId { get; private set; }
        public bool Pending { get; private set; }
        private SkillImpactMode mode;
        private float remaining;
        private SkillData skill;

        public int Begin(SkillData actionSkill, SkillImpactMode timingMode, float seconds)
        {
            ActionId++;
            skill = actionSkill;
            mode = timingMode;
            remaining = Math.Max(0f, seconds);
            Pending = true;
            return ActionId;
        }

        public void Tick(float deltaTime)
        {
            if (!Pending || mode != SkillImpactMode.Timed) return;
            remaining -= Math.Max(0f, deltaTime);
            if (remaining <= 0f) Emit();
        }

        public void NotifyAnimationImpact(int actionId)
        {
            if (Pending && mode == SkillImpactMode.AnimationEvent && actionId == ActionId) Emit();
        }

        public void Cancel() { Pending = false; skill = null; ActionId++; }
        private void Emit()
        {
            Pending = false; // Clear first: subscribers may start another action.
            SkillData completedSkill = skill;
            skill = null;
            Impact?.Invoke(ActionId, completedSkill);
        }
    }
}
