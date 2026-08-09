using System.Collections.Generic;
using System.Linq;
using SkillTreeGraph.Core;

namespace SkillTreeGraph.Editor
{
    public class CompositeRemoveNodeCommand : IUndoCommand
    {
        private GraphContext _graphContext;
        private GraphControllerContext _controllerContext;

        private List<SkillNode> _nodes;
        private Dictionary<int, SkillNode> _nodeLookup;

        public CompositeRemoveNodeCommand(List<SkillNode> nodes)
        {
            _nodes = nodes;
            _nodeLookup = new Dictionary<int, SkillNode>();
        }

        public void InitializeCommand(GraphContext graphContext, GraphControllerContext controllerContext)
        {
            _graphContext = graphContext;
            _controllerContext = controllerContext;
        }

        public void Execute()
        {
            _nodeLookup.Clear();
            _nodes = _nodes.OrderByDescending(node => _graphContext.Collection.FindIndex(node)).ToList();
            foreach (var node in _nodes)
            {
                int index = _graphContext.Collection.FindIndex(node);
                var nodeView = _graphContext.Collection.GetNodeView(node.Id);

                _graphContext.Collection.RemoveNode(nodeView);

                _controllerContext.Selection.ClearSelection();
                _controllerContext.GroupSelection.ClearSelection();
                _controllerContext.Interaction.ExitMode();
                _controllerContext.ConnectionRenderer.RemoveAllConnectionsForNode(nodeView);

                _nodeLookup.Add(index, node);

                nodeView.RemoveFromHierarchy();
            }
        }

        public void Undo()
        {
            _nodeLookup = _nodeLookup.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            foreach (var kvp in _nodeLookup)
            {
                var nodeView = _controllerContext.NodeCreation.CreateNodeView(kvp.Value, kvp.Value.UiToolkitPosition, kvp.Value.NodeSize);
                _graphContext.Collection.InsertNodeAt(kvp.Key, nodeView);
            }
        }
    }
}
