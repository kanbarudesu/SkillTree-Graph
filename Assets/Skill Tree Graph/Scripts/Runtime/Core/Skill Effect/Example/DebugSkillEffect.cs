using UnityEngine;

namespace SkillTreeGraph.Core
{
    [System.Serializable]
    public class DebugSkillEffect : SkillEffect
    {
        public override void Apply(ISkillContext context, int level)
        {
            Debug.Log($"Apply effect to {context.PlayerRoot.name} with level {level}");
        }
        public override string GetDescription(ISkillContext context, int currentLevel, bool isMaxLevel) => "Debug Effect";
    }
}