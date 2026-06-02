using System.Collections.Generic;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class GraphConnectionController
    {
        private readonly VisualElement _connectionContainer;
        private readonly SkillTreeSettingData _settings;

        private readonly Dictionary<string, ConnectionLineElement> _connections = new Dictionary<string, ConnectionLineElement>();

        public GraphConnectionController(GraphContext context)
        {
            _connectionContainer = context.ConnectionContainer;
            _settings = context.Settings;
        }

        private string GetKey(SkillNodeView parent, SkillNodeView child)
        {
            return $"{parent.Data.Id}_{child.Data.Id}";
        }

        public void DrawConnection(SkillNodeView parent, SkillNodeView child)
        {
            string key = GetKey(parent, child);

            if (_connections.ContainsKey(key))
                return;

            var line = new ConnectionLineElement(parent, child, _connectionContainer, _settings);

            _connectionContainer.Add(line);
            _connections.Add(key, line);
        }

        public void RemoveConnection(SkillNodeView parent, SkillNodeView child)
        {
            string key = GetKey(parent, child);

            if (!_connections.TryGetValue(key, out var line))
                return;

            line.Dispose();
            _connectionContainer.Remove(line);
            _connections.Remove(key);
        }

        public void RemoveAllConnectionsForNode(SkillNodeView node)
        {
            var keysToRemove = new List<string>();

            foreach (var kvp in _connections)
            {
                if (kvp.Key.StartsWith(node.Data.Id + "_") ||
                    kvp.Key.EndsWith("_" + node.Data.Id))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                var line = _connections[key];
                line.Dispose();
                _connectionContainer.Remove(line);
                _connections.Remove(key);
            }
        }

        public void Clear()
        {
            foreach (var kvp in _connections)
            {
                kvp.Value.Dispose();
                _connectionContainer.Remove(kvp.Value);
            }

            _connections.Clear();
            _connectionContainer.Clear();
        }
    }
}