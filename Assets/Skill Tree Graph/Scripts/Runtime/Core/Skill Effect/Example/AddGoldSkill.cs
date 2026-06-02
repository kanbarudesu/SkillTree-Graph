namespace SkillTreeGraph.Core
{
    [System.Serializable]
    public class AddGoldSkill : SkillEffect
    {
        public int Value;

        public override void Apply(ISkillContext context, int level)
        {
            // Apply gold gain logic
        }

        public override string GetDescription(ISkillContext context, int currentLevel, bool isMaxLevel)
        {
            if (isMaxLevel)
                return $"Gold {Value * currentLevel}";
                
            return $"Gold {Value * currentLevel} → {Value * (currentLevel + 1)}";
        }
    }
}