namespace SkillTreeGraph.Core
{
    [System.Serializable]
    public class AddEnergySkill : SkillEffect
    {
        public int Value;

        public override void Apply(ISkillContext context, int level)
        {
            // Apply energy gain logic
        }

        public override string GetDescription(ISkillContext context, int currentLevel, bool isMaxLevel)
        {
            if (isMaxLevel)
                return $"Energy {Value * currentLevel}";
            
            return $"Energy {Value * currentLevel} → {Value * (currentLevel + 1)}";
        }
    }
}