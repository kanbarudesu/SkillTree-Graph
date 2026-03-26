namespace SkillTreeGraph.Core
{
    [System.Serializable]
    public class SkillPointCost : SkillCost
    {
        public int PointsRequired = 1;

        public override bool CanAfford(ISkillContext context, int targetLevel)
        {
            //Implement skill point check
            return true;
        }

        public override void Pay(ISkillContext context, int targetLevel)
        {
            //Implement skill point payment
        }

        public override string GetDescription(ISkillContext context, int targetLevel)
        {
            return $"{PointsRequired} SP";
        }
    }
}