using UnityEngine;
using System.Collections.Generic;

namespace SkillTreeGraph.Core
{
    public partial class SkillNode : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        public string DisplayName;
        [TextArea]
        public string Description;
        public Sprite Icon;

        [Header("Progression")]
        public int MaxLevel = 5;
        [SerializeReference, SRPeeker]
        public List<SkillCost> ResourcesCost = new();

        [Header("Unlock Conditions")]
        [SerializeReference, SRPeeker]
        public List<SkillUnlockCondition> UnlockConditions = new();

        [Header("Effects")]
        [SerializeReference, SRPeeker]
        public List<SkillEffect> Effects = new();

        [Header("Tree Structure")]
        public List<string> ParentIds = new();
        public List<string> ChildrenIds = new();

        [Header("UI Layout")]
        public Vector2 UiToolkitPosition;
        public Vector2 CanvasPosition;
        public float NodeSize;
    }
}