using SkillTreeGraph.Core;
using UnityEditor;
using UnityEngine;

namespace SkillTreeGraph.Editor
{
    public class CreateNodeCommand : IUndoCommand
    {
        private GraphContext _graphContext;
        private GraphControllerContext _controllerContext;

        private SkillNode _node;
        private SkillNodeView _nodeView;
        private Vector2 _nodePosition;

        public CreateNodeCommand(Vector2 position)
        {
            _nodePosition = position;
        }

        public void InitializeCommand(GraphContext graphContext, GraphControllerContext controllerContext)
        {
            _graphContext = graphContext;
            _controllerContext = controllerContext;

            _node = ScriptableObject.CreateInstance<SkillNode>();
            if (string.IsNullOrEmpty(_node.Id))
                _node.Id = GUID.Generate().ToString();
        }

        public void Execute()
        {
            _nodeView = _controllerContext.NodeCreation.CreateNodeView(_node, _nodePosition);
            _graphContext.Collection.AddNode(_nodeView);
        }

        public void Undo()
        {
            _nodeView = _graphContext.Collection.GetNodeView(_node.Id);
            _nodePosition = _node.UiToolkitPosition;
            _graphContext.Collection.RemoveNode(_nodeView);
            _controllerContext.Selection.ClearSelection();
            _controllerContext.Interaction.ExitMode();
            _controllerContext.ConnectionRenderer.RemoveAllConnectionsForNode(_nodeView);

            _nodeView.RemoveFromHierarchy();
        }
    }
}
