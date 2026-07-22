using SkillTreeGraph.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class GraphNodeCreationController
    {
        private readonly VisualElement _root;
        private readonly GraphContext _graphContext;
        private readonly GraphControllerContext _controllerContext;
        private readonly NodeGroupSelectionController _groupSelection;
        private readonly UndoManager _undoManager;
        private readonly VisualTreeAsset _buttonTemplate;
        private readonly SkillTreeSettingData _settings;

        public GraphNodeCreationController(GraphContext graphContext, GraphControllerContext controllerContext, NodeGroupSelectionController groupSelection, UndoManager undoManager, VisualTreeAsset buttonTemplate)
        {
            _root = graphContext.Root;
            _graphContext = graphContext;
            _controllerContext = controllerContext;
            _groupSelection = groupSelection;
            _undoManager = undoManager;
            _buttonTemplate = buttonTemplate;
            _settings = graphContext.Settings;

            _root.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!_settings.AllowCtrlClickNodeCreation)
                return;

            if (evt.button != 0)
                return;

            if (!evt.ctrlKey && !evt.commandKey)
                return;

            Vector2 graphPosition = ScreenToGraph(evt.localPosition);
            float halfSize = _settings.NodeSize * 0.5f;
            graphPosition -= new Vector2(halfSize, halfSize);

            if (_settings.EnableSnap)
            {
                float snap = _settings.SnapSize;
                graphPosition.x = Mathf.Round(graphPosition.x / snap) * snap;
                graphPosition.y = Mathf.Round(graphPosition.y / snap) * snap;
            }

            CreateNodeAt(graphPosition);
            _controllerContext.Selection.ClearSelection();
            _groupSelection.ClearSelection();
            evt.StopPropagation();
        }

        private Vector2 ScreenToGraph(Vector2 mousePosition) => SkillTreeEditorUtility.ScreenToGraph(_graphContext.GraphContent, mousePosition);

        private void CreateNodeAt(Vector2 position) => _undoManager.ExecuteCommand(new CreateNodeCommand(position));

        private void DrawConnections(SkillNode node, SkillNodeView nodeView)
        {
            foreach (var parentID in node.ParentIds)
            {
                var parent = _graphContext.Collection.GetNodeView(parentID);
                _controllerContext.Interaction.ConnectNode(parent, nodeView);
            }

            foreach (var childID in node.ChildrenIds)
            {
                var child = _graphContext.Collection.GetNodeView(childID);
                _controllerContext.Interaction.ConnectNode(nodeView, child);
            }
        }

        public SkillNodeView CreateNodeView(SkillNode node, Vector2 position, float? nodeSize = null)
        {
            var nodeView = new SkillNodeView(node, _buttonTemplate);
            nodeView.SetSize(nodeSize ?? _settings.NodeSize);
            nodeView.SetPosition(position);

            _controllerContext.Selection.RegisterNode(
                nodeView,
                nodeView.Q<VisualElement>("main-button"),
                nodeView.Q<VisualElement>("option-button-container"),
                () => _controllerContext.Interaction.OnNodeClicked(nodeView)
            );
            _controllerContext.NodeOptionController.RegisterNodeButtons(_graphContext.Root, nodeView);
            _groupSelection.RegisterNode(nodeView, nodeView.Q<VisualElement>("main-button"));

            var dragManipulator = new NodeDragManipulator(_graphContext, nodeView, _groupSelection);
            nodeView.AddManipulator(dragManipulator);

            _graphContext.NodeContainer.Add(nodeView);

            nodeView.RegisterCallback<GeometryChangedEvent>(OnNodeGeometryReady);
            void OnNodeGeometryReady(GeometryChangedEvent evt)
            {
                nodeView.UnregisterCallback<GeometryChangedEvent>(OnNodeGeometryReady);
                DrawConnections(node, nodeView);
            }

            return nodeView;
        }

        public void Dispose() => _root.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
    }
}