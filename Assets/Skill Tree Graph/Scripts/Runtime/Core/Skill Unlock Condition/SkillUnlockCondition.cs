namespace SkillTreeGraph.Core
{
    [System.Serializable]
    public abstract class SkillUnlockCondition
    {
        public abstract bool CanUnlock(SkillNode node, SkillTreeRuntime runtime, ISkillContext context);
        public virtual string GetDescription(SkillNode node, SkillTreeRuntime runtime, ISkillContext context) => string.Empty;
    }
}