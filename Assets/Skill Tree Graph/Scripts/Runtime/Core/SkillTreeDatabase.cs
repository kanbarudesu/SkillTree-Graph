using System.Collections.Generic;
using UnityEngine;

namespace SkillTreeGraph.Core
{
    [CreateAssetMenu(fileName = "SkillTreeDatabase", menuName = "SkillTreeDatabase", order = 0)]
    public class SkillTreeDatabase : ScriptableObject
    {
        [HideInInspector]
        public string Id;
        public List<SkillNode> SkillDatabase = new();

        private Dictionary<string, SkillNode> _nodeCache;

        private void InitializeCache()
        {
            if (_nodeCache != null && _nodeCache.Count == SkillDatabase.Count) return;

            _nodeCache = new Dictionary<string, SkillNode>();
            foreach (var node in SkillDatabase)
            {
                if (node != null && !string.IsNullOrEmpty(node.Id))
                    _nodeCache[node.Id] = node;
            }
        }

        public void AddNode(SkillNode node)
        {
            SkillDatabase.Add(node);
            if (_nodeCache != null && node != null && !string.IsNullOrEmpty(node.Id))
            {
                _nodeCache[node.Id] = node;
            }
        }

        public void RemoveNode(SkillNode node)
        {
            SkillDatabase.Remove(node);
            if (_nodeCache != null && node != null && !string.IsNullOrEmpty(node.Id))
            {
                _nodeCache.Remove(node.Id);
            }
        }

        public SkillNode GetNode(string id)
        {
            InitializeCache();
            return _nodeCache.TryGetValue(id, out var node) ? node : null;
        }

        public bool TryGetNode(string id, out SkillNode node)
        {
            InitializeCache();
            return _nodeCache.TryGetValue(id, out node);
        }
    }
}