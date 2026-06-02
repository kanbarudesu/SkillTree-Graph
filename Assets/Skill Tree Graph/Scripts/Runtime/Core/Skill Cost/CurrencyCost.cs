namespace SkillTreeGraph.Core
{
    [System.Serializable]
    public class CurrencyCost : SkillCost
    {
        public string CurrencyId = "Gold";
        public int BaseCost = 100;
        public int CostMultiplierPerLevel = 50;

        private int GetCost(int level) => BaseCost + (CostMultiplierPerLevel * (level - 1));

        public override bool CanAfford(ISkillContext context, SkillNodeRuntimeData nodeState, int targetLevel)
        {
            //Implement currency check
            return true;
        }

        public override void Pay(ISkillContext context, SkillNodeRuntimeData nodeState, int targetLevel)
        {
            //Implement currency payment
        }

        public override string GetDescription(ISkillContext context, SkillNodeRuntimeData nodeState, int targetLevel)
        {
            return $"{GetCost(targetLevel)} {CurrencyId}";
        }
    }
}