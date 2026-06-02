using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class NodeOptionButtonController
    {
        private readonly GraphControllerContext _controllerContext;
        private readonly UndoManager _undoManager;

        public NodeOptionButtonController(GraphControllerContext controllerContext, UndoManager undoManager)
        {
            _controllerContext = controllerContext;
            _undoManager = undoManager;
        }

        public void RegisterNodeButtons(VisualElement root, SkillNodeView node)
        {
            var editButton = node.Q<VisualElement>("edit-button");
            var connectButton = node.Q<VisualElement>("connect-button");
            var disconnectButton = node.Q<VisualElement>("disconnect-button");
            var duplicateButton = node.Q<VisualElement>("duplicate-button");
            var removeButton = node.Q<VisualElement>("remove-button");
            var inspectorPanel = root.Q<VisualElement>("node-inspector-panel");

            editButton.RegisterCallback<PointerDownEvent>(evt => OnEditButtonClicked(node, inspectorPanel));
            connectButton.RegisterCallback<PointerDownEvent>(evt => OnConnectButtonClicked(node));
            disconnectButton.RegisterCallback<PointerDownEvent>(evt => OnDisconnectButtonClicked(node));
            duplicateButton.RegisterCallback<PointerDownEvent>(evt => OnDuplicateButtonClicked(node));
            removeButton.RegisterCallback<PointerDownEvent>(evt => OnRemoveButtonClicked(node, inspectorPanel));
        }

        private ScrollView CreateInspectorContent()
        {
            return new ScrollView
            {
                name = "inspector-content",
                mode = ScrollViewMode.Vertical,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden
            };
        }

        private void OnEditButtonClicked(SkillNodeView nodeView, VisualElement inspectorPanel)
        {
            inspectorPanel.Clear();
            inspectorPanel.Add(BuildNodeInspector(nodeView));
            inspectorPanel.RemoveFromClassList("panel-exit");
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

        private void OnRemoveButtonClicked(SkillNodeView nodeView, VisualElement inspectorPanel)
        {
            RemoveNode(nodeView);
            inspectorPanel.Clear();
        }

        private void RemoveNode(SkillNodeView nodeView) => _undoManager.ExecuteCommand(new RemoveNodeCommand(nodeView.Data));

        private VisualElement BuildNodeInspector(SkillNodeView nodeView)
        {
            var inspectorContent = CreateInspectorContent();
            var serializedObject = new SerializedObject(nodeView.Data);

            var label = new Label("Skill Node Setting");
            label.style.fontSize = 25;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            inspectorContent.Add(label);

            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.propertyPath == "m_Script") continue;

                    var field = new PropertyField(iterator.Copy());
                    if (ShouldDisable(iterator.propertyPath))
                        field.SetEnabled(false);

                    if (iterator.propertyPath == "Icon")
                    {
                        field.RegisterValueChangeCallback(evt =>
                        {
                            nodeView.RefreshUI();
                            serializedObject.ApplyModifiedProperties();
                        });
                    }

                    if (iterator.propertyPath == "NodeSize")
                    {
                        field.RegisterValueChangeCallback(evt =>
                        {
                            nodeView.RefreshSize();
                            serializedObject.ApplyModifiedProperties();
                        });
                    }

                    inspectorContent.Add(field);

                } while (iterator.NextVisible(false));
            }
            inspectorContent.Bind(serializedObject);

            return inspectorContent;
        }

        private bool ShouldDisable(string propertyPath)
        {
            return propertyPath == "Id"
                || propertyPath == "ParentIds"
                || propertyPath == "ChildrenIds"
                || propertyPath == "UiToolkitPosition"
                || propertyPath == "CanvasPosition";
        }
    }
}