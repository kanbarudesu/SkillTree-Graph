using System.Collections.Generic;
using SkillTreeGraph.Core;
using UnityEngine;

namespace SkillTreeGraph.Editor
{
    public class SkillTreeCollection : ScriptableObject
    {
        [SerializeField]
        private List<SkillNode> _nodes = new();
        public IReadOnlyList<SkillNode> Nodes => _nodes;

        private List<SkillNodeView> _nodeViews = new();
        public IReadOnlyList<SkillNodeView> NodeViews => _nodeViews;

        private Dictionary<string, SkillNode> _nodeLookup = new();
        private Dictionary<string, SkillNodeView> _nodeViewLookup = new();

        public void AddNode(SkillNodeView nodeView)
        {
            var node = nodeView.Data;

            _nodes.Add(node);
            _nodeViews.Add(nodeView);

            _nodeLookup[node.Id] = node;
            _nodeViewLookup[node.Id] = nodeView;
        }

        public void RemoveNode(SkillNodeView nodeView, bool includeConnection = true)
        {
            var node = nodeView.Data;

            if (includeConnection)
                RemoveNodeConnection(node);

            _nodeViews.Remove(nodeView);
            _nodes.Remove(node);

            _nodeLookup.Remove(node.Id);
            _nodeViewLookup.Remove(node.Id);
        }

        public void InsertNodeAt(int index, SkillNodeView nodeView)
        {
            var node = nodeView.Data;

            _nodeViews.Insert(index, nodeView);
            _nodes.Insert(index, node);

            _nodeLookup[node.Id] = node;
            _nodeViewLookup[node.Id] = nodeView;
        }

        public int FindIndex(SkillNode node) => _nodes.IndexOf(node);
        public SkillNode GetNode(string nodeId) => _nodeLookup.TryGetValue(nodeId, out var node) ? node : null;

        public SkillNodeView GetNodeView(string nodeId) => _nodeViewLookup.TryGetValue(nodeId, out var view) ? view : null;

        public void Clear()
        {
            foreach (var nodeView in _nodeViews)
                nodeView.RemoveFromHierarchy();
                
            _nodes.Clear();
            _nodeViews.Clear();
            _nodeLookup.Clear();
            _nodeViewLookup.Clear();
        }

        private void RemoveNodeConnection(SkillNode node)
        {
            foreach (var parentId in node.ParentIds)
            {
                if (_nodeLookup.TryGetValue(parentId, out var parent))
                    parent.ChildrenIds.Remove(node.Id);
            }

            foreach (var childId in node.ChildrenIds)
            {
                if (_nodeLookup.TryGetValue(childId, out var child))
                    child.ParentIds.Remove(node.Id);
            }
        }
    }
}