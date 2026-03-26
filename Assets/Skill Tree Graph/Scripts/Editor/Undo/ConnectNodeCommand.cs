namespace SkillTreeGraph.Editor
{
    public class ConnectNodeCommand : IUndoCommand
    {
        private GraphContext _graphContext;
        private GraphControllerContext _controllerContext;

        private string _parentNodeId;
        private string _childNodeId;

        public ConnectNodeCommand(string parentNodeId, string childNodeId)
        {
            _parentNodeId = parentNodeId;
            _childNodeId = childNodeId;
        }

        public void InitializeCommand(GraphContext graphContext, GraphControllerContext controllerContext)
        {
            _graphContext = graphContext;
            _controllerContext = controllerContext;
        }

        public void Execute()
        {
            var _parentNode = _graphContext.Collection.GetNodeView(_parentNodeId);
            var _childNode = _graphContext.Collection.GetNodeView(_childNodeId);

            _controllerContext.Interaction.RedrawConnections(_parentNode, _childNode);
        }

        public void Undo()
        {
            var _parentNode = _graphContext.Collection.GetNodeView(_parentNodeId);
            var _childNode = _graphContext.Collection.GetNodeView(_childNodeId);

            _parentNode.Data.ChildrenIds.Remove(_childNode.Data.Id);
            _childNode.Data.ParentIds.Remove(_parentNode.Data.Id);

            _controllerContext.ConnectionRenderer.RemoveConnection(_parentNode, _childNode);
        }
    }
}