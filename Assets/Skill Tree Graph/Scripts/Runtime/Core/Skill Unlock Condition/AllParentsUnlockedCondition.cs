using System.Text;

namespace SkillTreeGraph.Core
{
    [System.Serializable]
    public class AllParentsUnlockedCondition : SkillUnlockCondition
    {
        public override bool CanUnlock(SkillNode node, SkillTreeRuntime runtime, ISkillContext context)
        {
            if (node.ParentIds == null || node.ParentIds.Count == 0)
                return true;

            foreach (var id in node.ParentIds)
            {
                if (!runtime.IsUnlocked(id))
                    return false;
            }
            return true;
        }

        override public string GetDescription(SkillNode node, SkillTreeRuntime runtime, ISkillContext context)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < node.ParentIds.Count; i++)
            {
                sb.Append(runtime.GetNode(node.ParentIds[i]).DisplayName);
                if (i < node.ParentIds.Count - 1)
                    sb.Append(" And ");
            }
            return $"Unlock {sb} first.";
        }
    }
}
