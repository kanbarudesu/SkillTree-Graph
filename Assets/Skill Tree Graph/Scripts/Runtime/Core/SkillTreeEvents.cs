using System;

namespace SkillTreeGraph.Core
{
    public struct NodeLevelUpEvent
    {
        public string NodeId;
        public int NewLevel;
    }

    public struct NodeStateChangedEvent
    {
        public string NodeId;
        public SkillNodeState NewState;
    }

    public struct NodeAvailableEvent
    {
        public string NodeId;
    }

    public struct RequestNodeLevelUpEvent
    {
        public string NodeId;
        public Action OnSuccess;
        public Action<string> OnFail;
    }
}