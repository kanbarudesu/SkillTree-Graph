using System;
using System.Collections.Generic;
using SkillTreeGraph.Core;
using UnityEngine;

namespace SkillTreeGraph.Editor
{
    [Serializable]
    public partial class SkillNodeSaveData
    {
        public string Id, DisplayName, Description, IconGuid, IconName;
        public int MaxLevel = 5;
        [SerializeReference] public List<SkillCost> ResourcesCost;
        public List<string> ParentIds, ChildrenIds;
        [SerializeReference] public List<SkillUnlockCondition> UnlockConditions;
        [SerializeReference] public List<SkillEffect> Effects;
        public Vector2 UiToolkitPosition, CanvasPosition;
        public float NodeSize;
    }
}
