using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class NodeOptionButtonController
    {
        private readonly GraphContext _graphContext;
        private readonly GraphControllerContext _controllerContext;
        private readonly UndoManager _undoManager;

        public NodeOptionButtonController(GraphContext graphContext, GraphControllerContext controllerContext, UndoManager undoManager)
        {
            _graphContext = graphContext;
            _controllerContext = controllerContext;
            _undoManager = undoManager;
        }

        public void RegisterNodeButtons(SkillNodeView node)
        {
            var editButton = node.Q<VisualElement>("edit-button");
            var connectButton = node.Q<VisualElement>("connect-button");
            var disconnectButton = node.Q<VisualElement>("disconnect-button");
            var duplicateButton = node.Q<VisualElement>("duplicate-button");
            var removeButton = node.Q<VisualElement>("remove-button");

            editButton.RegisterCallback<PointerDownEvent>(evt => OnEditButtonClicked(node));
            connectButton.RegisterCallback<PointerDownEvent>(evt => OnConnectButtonClicked(node));
            disconnectButton.RegisterCallback<PointerDownEvent>(evt => OnDisconnectButtonClicked(node));
            duplicateButton.RegisterCallback<PointerDownEvent>(evt => OnDuplicateButtonClicked(node));
            removeButton.RegisterCallback<PointerDownEvent>(evt => OnRemoveButtonClicked(node));
        }

        private void OnEditButtonClicked(SkillNodeView nodeView)
        {
            _graphContext.InspectorPanel.contentContainer.Clear();
            _graphContext.InspectorPanel.contentContainer.Add(BuildNodeInspector(nodeView));
            _graphContext.InspectorPanel.TogglePanelDisplay(true);
        }

        private void OnConnectButtonClicked(SkillNodeView nodeView)
        {
            _controllerContext.Interaction.EnterConnectMode();
            _controllerContext.Interaction.OnNodeClicked(nodeView);
        }

        private void OnDisconnectButtonClicked(SkillNodeView nodeView)
        {
            if (nodeView.Data.ChildrenIds.Count == 0 && nodeView.Data.ParentIds.Count == 0) return;
            _undoManager.ExecuteCommand(new DisconnectNodeCommand(nodeView.Data.Id));
        }

        private void OnDuplicateButtonClicked(SkillNodeView nodeView) => _undoManager.ExecuteCommand(new DuplicateNodeCommand(nodeView.Data));

        private void OnRemoveButtonClicked(SkillNodeView nodeView)
        {
            RemoveNode(nodeView);
            _graphContext.InspectorPanel.contentContainer.Clear();
        }

        private void RemoveNode(SkillNodeView nodeView) => _undoManager.ExecuteCommand(new RemoveNodeCommand(nodeView.Data));

        private static readonly HashSet<string> ReadOnlyFieldPaths = new()
        {
            "Id", "ParentIds", "ChildrenIds", "UiToolkitPosition", "CanvasPosition"
        };

        private VisualElement BuildNodeInspector(SkillNodeView nodeView)
        {
            var inspectorContent = new VisualElement();
            var serializedObject = new SerializedObject(nodeView.Data);

            var onPropertyChanged = new Dictionary<string, Action>
            {
                ["Icon"] = nodeView.RefreshUI,
                ["NodeSize"] = nodeView.RefreshSize,
            };

            SkillTreeEditorUtility.BuildInspectorElement(
                inspectorContent,
                serializedObject,
                ReadOnlyFieldPaths.Contains,
                onPropertyChanged);

            return inspectorContent;
        }
    }
}