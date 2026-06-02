using System.Collections.Generic;

namespace SkillTreeGraph.Editor
{
    public class DisconnectNodeCommand : IUndoCommand
    {
        private GraphContext _graphContext;
        private GraphControllerContext _controllerContext;

        private readonly string _nodeId;
        private SkillNodeView _currentNodeView;
        private List<SkillNodeView> _parentNodeViews;
        private List<SkillNodeView> _childNodeViews;

        public DisconnectNodeCommand(string nodeId)
        {
            _nodeId = nodeId;
        }

        public void InitializeCommand(GraphContext graphContext, GraphControllerContext controllerContext)
        {
            _graphContext = graphContext;
            _controllerContext = controllerContext;

            _currentNodeView = _graphContext.Collection.GetNodeView(_nodeId);
            _parentNodeViews = new List<SkillNodeView>();
            _childNodeViews = new List<SkillNodeView>();

            foreach (var parentID in _currentNodeView.Data.ParentIds)
                _parentNodeViews.Add(_graphContext.Collection.GetNodeView(parentID));

            foreach (var childID in _currentNodeView.Data.ChildrenIds)
                _childNodeViews.Add(_graphContext.Collection.GetNodeView(childID));
        }

        public void Execute()
        {
            foreach (var parent in _parentNodeViews)
                _controllerContext.Interaction.DisconnectNode(parent, _currentNodeView);

            foreach (var child in _childNodeViews)
                _controllerContext.Interaction.DisconnectNode(_currentNodeView, child);
        }

        public void Undo()
        {
            foreach (var parent in _parentNodeViews)
                _controllerContext.Interaction.ConnectNode(parent, _currentNodeView);

            foreach (var child in _childNodeViews)
                _controllerContext.Interaction.ConnectNode(_currentNodeView, child);
        }
    }
}