using System.Text;

namespace SkillTreeGraph.Core
{
    [System.Serializable]
    public class AnyParentAtLevelCondition : SkillUnlockCondition
    {
        public int RequiredLevel = 1;

        public override bool CanUnlock(SkillNode node, SkillTreeRuntime runtime, ISkillContext context)
        {
            foreach (var id in node.ParentIds)
            {
                if (runtime.GetLevel(id) >= RequiredLevel)
                    return true;
            }
            return false;
        }

        public override string GetDescription(SkillNode node, SkillTreeRuntime runtime, ISkillContext context)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < node.ParentIds.Count; i++)
            {
                sb.Append(runtime.GetNode(node.ParentIds[i]).DisplayName);
                if (i < node.ParentIds.Count - 1)
                    sb.Append(" Or ");
            }
            return $"Require {sb} at level {RequiredLevel} first.";
        }
    }
}