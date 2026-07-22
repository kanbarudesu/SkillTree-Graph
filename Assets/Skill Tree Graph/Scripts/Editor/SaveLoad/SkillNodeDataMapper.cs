using System.Collections.Generic;
using SkillTreeGraph.Core;
using UnityEditor;

namespace SkillTreeGraph.Editor
{
    public static class SkillNodeDataMapper
    {
        public static SkillNodeSaveData ToSaveData(SkillNode node)
        {
            var data = new SkillNodeSaveData
            {
                Id = node.Id,
                DisplayName = node.DisplayName,
                Description = node.Description,
                MaxLevel = node.MaxLevel,
                UiToolkitPosition = node.UiToolkitPosition,
                CanvasPosition = node.CanvasPosition,
                NodeSize = node.NodeSize
            };

            if (node.Icon != null)
            {
                data.IconGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(node.Icon));
                data.IconName = node.Icon.name;
            }

            data.ResourcesCost = new List<SkillCost>(node.ResourcesCost ?? new List<SkillCost>());
            data.ParentIds = new List<string>(node.ParentIds ?? new List<string>());
            data.ChildrenIds = new List<string>(node.ChildrenIds ?? new List<string>());
            data.UnlockConditions = new List<SkillUnlockCondition>(node.UnlockConditions ?? new List<SkillUnlockCondition>());
            data.Effects = new List<SkillEffect>(node.Effects ?? new List<SkillEffect>());

            return data;
        }

        public static SkillNode ToNode(SkillNodeSaveData data)
        {
            var node = SkillTreeEditorUtility.CreateTransientInstance<SkillNode>();
            node.Id = data.Id;
            node.DisplayName = data.DisplayName;
            node.Description = data.Description;
            node.Icon = SkillTreeEditorUtility.LoadSprite(data.IconGuid, data.IconName);
            node.MaxLevel = data.MaxLevel;
            node.ResourcesCost = new List<SkillCost>(data.ResourcesCost ?? new List<SkillCost>());
            node.ParentIds = new List<string>(data.ParentIds ?? new List<string>());
            node.ChildrenIds = new List<string>(data.ChildrenIds ?? new List<string>());
            node.UnlockConditions = new List<SkillUnlockCondition>(data.UnlockConditions ?? new List<SkillUnlockCondition>());
            node.Effects = new List<SkillEffect>(data.Effects ?? new List<SkillEffect>());
            node.UiToolkitPosition = data.UiToolkitPosition;
            node.CanvasPosition = data.CanvasPosition;
            node.NodeSize = data.NodeSize;
            return node;
        }

        public static SkillTreeSaveData ToSaveData(SkillTreeSettingData settings, SkillTreeCollection collection)
        {
            var data = new SkillTreeSaveData
            {
                Id = string.IsNullOrEmpty(settings.Id) ? GUID.Generate().ToString() : settings.Id,
                SkillTreeName = settings.SkillTreeName
            };

            foreach (var node in collection.Nodes)
                data.nodes.Add(ToSaveData(node));

            return data;
        }
    }
}
