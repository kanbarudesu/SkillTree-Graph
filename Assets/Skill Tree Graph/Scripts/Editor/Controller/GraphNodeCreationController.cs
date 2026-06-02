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
        private readonly UndoManager _undoManager;
        private readonly VisualTreeAsset _buttonTemplate;
        private readonly SkillTreeSettingData _settings;

        public GraphNodeCreationController(GraphContext graphContext, GraphControllerContext controllerContext, UndoManager undoManager, VisualTreeAsset buttonTemplate)
        {
            _root = graphContext.Root;
            _graphContext = graphContext;
            _controllerContext = controllerContext;
            _undoManager = undoManager;
            _buttonTemplate = buttonTemplate;
            _settings = graphContext.Settings;

            _root.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!_settings.CanAddSkillOnClick)
                return;

            if (evt.button != 0)
                return;

            if (!evt.ctrlKey)
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
            evt.StopPropagation();
        }

        private Vector2 ScreenToGraph(Vector2 mousePosition)
        {
#if UNITY_6000_3_OR_NEWER
            float zoom = _graphContext.GraphContent.style.scale.value.value.x;
            Vector2 pan = new Vector2(_graphContext.GraphContent.style.translate.value.x.value, _graphContext.GraphContent.style.translate.value.y.value);
#else
            float zoom = _graphContext.GraphContent.transform.scale.x;
            Vector2 pan = _graphContext.GraphContent.transform.position;
#endif
            return (mousePosition - pan) / zoom;
        }

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

            var dragManipulator = new NodeDragManipulator(_graphContext, nodeView);
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