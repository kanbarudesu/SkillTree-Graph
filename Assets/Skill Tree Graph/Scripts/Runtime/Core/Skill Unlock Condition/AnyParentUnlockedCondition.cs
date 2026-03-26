using System.Text;

namespace SkillTreeGraph.Core
{
    [System.Serializable]
    public class AnyParentUnlockedCondition : SkillUnlockCondition
    {
        public override bool CanUnlock(SkillNode node, SkillTreeRuntime runtime, ISkillContext context)
        {
            foreach (var id in node.ParentIds)
            {
                if (runtime.IsUnlocked(id))
                    return true;
            }
            return false;
        }

        override public string GetDescription(SkillNode node, SkillTreeRuntime runtime, ISkillContext context)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < node.ParentIds.Count; i++)
            {
                sb.Append(runtime.GetNode(node.ParentIds[i]).DisplayName);
                if (i < node.ParentIds.Count - 1)
                    sb.Append(" Or ");
            }
            return $"Require {sb} to be unlocked.";
        }
    }
}
