using System;
using System.Collections.Generic;

namespace SkillTreeGraph.Core
{
    [Serializable]
    public class SkillTreeSaveData
    {
        public List<NodeSaveData> Nodes = new List<NodeSaveData>();
    }

    [Serializable]
    public class NodeSaveData
    {
        public string Id;
        public int Level;
        public SkillNodeState State;
    }
}