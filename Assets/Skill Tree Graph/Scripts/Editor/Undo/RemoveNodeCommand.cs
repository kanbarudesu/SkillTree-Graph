using SkillTreeGraph.Core;

namespace SkillTreeGraph.Editor
{
    public class RemoveNodeCommand : IUndoCommand
    {
        private GraphContext _graphContext;
        private GraphControllerContext _controllerContext;

        private readonly SkillNode _node;

        private SkillNodeView _nodeView;
        private int _index;

        public RemoveNodeCommand(SkillNode node)
        {
            _node = node;
        }

        public void InitializeCommand(GraphContext graphContext, GraphControllerContext controllerContext)
        {
            _graphContext = graphContext;
            _controllerContext = controllerContext;
        }

        public void Execute()
        {
            _index = _graphContext.Collection.FindIndex(_node);
            _nodeView = _graphContext.Collection.GetNodeView(_node.Id);

            _graphContext.Collection.RemoveNode(_nodeView);

            _controllerContext.Selection.ClearSelection();
            _controllerContext.Interaction.ExitMode();
            _controllerContext.ConnectionRenderer.RemoveAllConnectionsForNode(_nodeView);

            _nodeView.RemoveFromHierarchy();
        }

        public void Undo()
        {
            _nodeView = _controllerContext.NodeCreation.CreateNodeView(_node, _node.UiToolkitPosition);
            _graphContext.Collection.InsertNodeAt(_index, _nodeView);
        }
    }
}
