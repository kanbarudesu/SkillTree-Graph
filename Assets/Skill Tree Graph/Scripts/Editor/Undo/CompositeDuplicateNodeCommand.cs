using System.Collections.Generic;
using SkillTreeGraph.Core;
using UnityEditor;
using UnityEngine;

namespace SkillTreeGraph.Editor
{
    public class CompositeDuplicateNodeCommand : IUndoCommand
    {
        private GraphContext _graphContext;
        private GraphControllerContext _controllerContext;

        private Dictionary<SkillNode, Vector2> _nodeLookup;
        private List<SkillNodeView> _selectedNodeViews;

        public CompositeDuplicateNodeCommand(List<SkillNode> nodes)
        {
            _nodeLookup = new Dictionary<SkillNode, Vector2>();
            _selectedNodeViews = new List<SkillNodeView>();
            foreach (var node in nodes)
            {
                var newNode = SkillTreeEditorUtility.DeepClone(node);
                newNode.Id = GUID.Generate().ToString();
                newNode.ChildrenIds.Clear();
                newNode.ParentIds.Clear();
                var nodePosition = new Vector2(newNode.UiToolkitPosition.x + newNode.NodeSize, newNode.UiToolkitPosition.y + newNode.NodeSize);
                _nodeLookup.Add(newNode, nodePosition);
            }
        }

        public void InitializeCommand(GraphContext graphContext, GraphControllerContext controllerContext)
        {
            _graphContext = graphContext;
            _controllerContext = controllerContext;
        }

        public void Execute()
        {
            _selectedNodeViews.Clear();
            foreach (var kvp in _nodeLookup)
            {
                var nodeView = _controllerContext.NodeCreation.CreateNodeView(kvp.Key, kvp.Value, kvp.Key.NodeSize);
                _graphContext.Collection.AddNode(nodeView);
                _selectedNodeViews.Add(nodeView);
            }
            _controllerContext.GroupSelection.ClearSelection();
            _controllerContext.GroupSelection.SetAndHighlightSelection(_selectedNodeViews);
        }

        public void Undo()
        {
            foreach (var kvp in _nodeLookup)
            {
                var nodeView = _graphContext.Collection.GetNodeView(kvp.Key.Id);
                _graphContext.Collection.RemoveNode(nodeView);
                _controllerContext.ConnectionRenderer.RemoveAllConnectionsForNode(nodeView);
                nodeView.RemoveFromHierarchy();
            }

            _controllerContext.Selection.ClearSelection();
            _controllerContext.GroupSelection.ClearSelection();
            _controllerContext.Interaction.ExitMode();
        }
    }
}