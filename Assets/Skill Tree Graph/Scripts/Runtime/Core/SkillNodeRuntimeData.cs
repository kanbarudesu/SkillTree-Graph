using GameEvents;

namespace SkillTreeGraph.Core
{
    public class SkillNodeRuntimeData
    {
        public string Id { get; private set; }
        public int CurrentLevel { get; private set; }
        public int NodeDepth { get; private set; }
        public SkillNodeState State { get; private set; }

        public SkillNodeRuntimeData(string id)
        {
            Id = id;
            NodeDepth = 0;
            State = SkillNodeState.Locked;
        }

        public void SetState(SkillNodeState newState)
        {
            // if (State == newState) return;

            State = newState;
            EventManager.TriggerEvent(new NodeStateChangedEvent { NodeId = Id, NewState = newState });
        }

        public void SetLevel(int newLevel)
        {
            if (CurrentLevel == newLevel) return;

            CurrentLevel = newLevel;
            EventManager.TriggerEvent(new NodeLevelUpEvent { NodeId = Id, NewLevel = newLevel });
        }

        public void SetNodeDepth(int depth) => NodeDepth = depth;
    }
}