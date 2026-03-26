using System.Collections.Generic;
using System.Linq;

namespace SkillTreeGraph.Core
{
    public class SkillTreeRuntime
    {
        private readonly Dictionary<string, SkillNodeRuntimeData> _states = new();
        private readonly SkillTreeDatabase _database;

        public SkillTreeRuntime(SkillTreeDatabase database)
        {
            _database = database;
        }

        public SkillNodeRuntimeData GetOrCreate(string id)
        {
            if (!_states.TryGetValue(id, out var state))
            {
                state = new SkillNodeRuntimeData(id);
                _states[id] = state;
            }

            return state;
        }

        public bool IsUnlocked(string id)
        {
            return _states.TryGetValue(id, out var state) &&
                  (state.State == SkillNodeState.Unlocked || state.State == SkillNodeState.Maxed);
        }

        public bool TryGetState(string id, out SkillNodeRuntimeData state)
        {
            return _states.TryGetValue(id, out state);
        }

        public SkillNode GetNode(string id)
        {
            return _database.GetNode(id);
        }

        public int GetLevel(string id)
        {
            return _states.TryGetValue(id, out var s) ? s.CurrentLevel : 0;
        }

        public IEnumerable<string> GetAllNodeIds()
        {
            return _database.SkillDatabase.Select(x => x.Id);
        }

        public SkillTreeSaveData ExportSaveData()
        {
            var data = new SkillTreeSaveData();
            foreach (var kvp in _states)
            {
                data.Nodes.Add(new NodeSaveData
                {
                    Id = kvp.Key,
                    Level = kvp.Value.CurrentLevel,
                    State = kvp.Value.State
                });
            }
            return data;
        }

        public void ImportSaveData(SkillTreeSaveData data)
        {
            if (data == null) return;

            foreach (var nodeData in data.Nodes)
            {
                var state = GetOrCreate(nodeData.Id);

                state.SetLevel(nodeData.Level);
                state.SetState(nodeData.State);
            }
        }
    }
}