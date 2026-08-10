using System;
using System.Collections.Generic;

namespace SkillTreeGraph.Editor
{
    [Serializable]
    public class SkillTreeSaveData
    {
        public string Id;
        public string SkillTreeName;
        public List<SkillNodeSaveData> nodes = new();
    }
}
