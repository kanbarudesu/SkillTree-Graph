using SkillTreeGraph.Core;

namespace SkillTreeGraph.Editor
{
    public static partial class SkillNodeDataMapper
    {
        static partial void OnMapToSaveData(SkillNode node, SkillNodeSaveData data)
        {
            data.CustomField = node.CustomField;
        }

        static partial void OnMapToNode(SkillNodeSaveData data, SkillNode node)
        {
            node.CustomField = data.CustomField;
        }
    }
}