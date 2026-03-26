namespace SkillTreeGraph.Core
{
    [System.Serializable]
    public abstract class SkillEffect
    {
        public abstract void Apply(ISkillContext context, int level);
        public virtual void Remove(ISkillContext context, int level) { }
        
        public virtual string GetDescription(int currentLevel, bool isMaxLevel) => string.Empty;
    }
}