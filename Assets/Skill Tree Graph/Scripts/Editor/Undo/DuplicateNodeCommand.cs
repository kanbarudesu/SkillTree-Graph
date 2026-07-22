using SkillTreeGraph.Core;
using UnityEditor;
using UnityEngine;

namespace SkillTreeGraph.Editor
{
    public class DuplicateNodeCommand : IUndoCommand
    {
        private GraphContext _graphContext;
        private GraphControllerContext _controllerContext;

        private SkillNode _node;
        private SkillNodeView _nodeView;
        private Vector2 _nodePosition;

        public DuplicateNodeCommand(SkillNode node)
        {
            _node = SkillTreeEditorUtility.DeepClone(node);
            _node.Id = GUID.Generate().ToString();
            _node.ChildrenIds.Clear();
            _node.ParentIds.Clear();
            _nodePosition = new Vector2(_node.UiToolkitPosition.x + _node.NodeSize * 1.5f, _node.UiToolkitPosition.y);
        }

        public void InitializeCommand(GraphContext graphContext, GraphControllerContext controllerContext)
        {
            _graphContext = graphContext;
            _controllerContext = controllerContext;
        }

        public void Execute()
        {
            _nodeView = _controllerContext.NodeCreation.CreateNodeView(_node, _nodePosition, _node.NodeSize);
            _graphContext.Collection.AddNode(_nodeView);
        }

        public void Undo()
        {
            _nodeView = _graphContext.Collection.GetNodeView(_node.Id);
            _graphContext.Collection.RemoveNode(_nodeView);
            _controllerContext.Selection.ClearSelection();
            _controllerContext.GroupSelection.ClearSelection();
            _controllerContext.Interaction.ExitMode();
            _controllerContext.ConnectionRenderer.RemoveAllConnectionsForNode(_nodeView);

            _nodeView.RemoveFromHierarchy();
        }
    }
}
