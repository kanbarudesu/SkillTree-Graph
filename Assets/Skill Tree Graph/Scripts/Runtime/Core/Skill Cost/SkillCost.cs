namespace SkillTreeGraph.Core
{
    [System.Serializable]
    public abstract class SkillCost
    {
        public abstract bool CanAfford(ISkillContext context, SkillNodeRuntimeData nodeState, int targetLevel);
        public abstract void Pay(ISkillContext context, SkillNodeRuntimeData nodeState, int targetLevel);
        public abstract string GetDescription(ISkillContext context, SkillNodeRuntimeData nodeState, int targetLevel);
    }
}