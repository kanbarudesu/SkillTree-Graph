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
            var removeButton = node.Q<VisualElement>("remove-button");
            var inspectorPanel = root.Q<VisualElement>("node-inspector-panel");

            editButton.RegisterCallback<PointerDownEvent>(evt => OnEditButtonClicked(node, inspectorPanel));
            connectButton.RegisterCallback<PointerDownEvent>(evt => OnConnectButtonClicked(node));
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

        private void OnEditButtonClicked(SkillNodeView node, VisualElement inspectorPanel)
        {
            inspectorPanel.Clear();
            inspectorPanel.Add(BuildNodeInspector(node));
            inspectorPanel.RemoveFromClassList("panel-exit");
        }

        private void OnConnectButtonClicked(SkillNodeView node)
        {
            _controllerContext.Interaction.EnterConnectMode();
            _controllerContext.Interaction.OnNodeClicked(node);
        }

        private void OnRemoveButtonClicked(SkillNodeView node, VisualElement inspectorPanel)
        {
            RemoveNode(node);
            inspectorPanel.Clear();
        }

        private void RemoveNode(SkillNodeView nodeView)
        {
            _undoManager.ExecuteCommand(new RemoveNodeCommand(nodeView.Data));
        }

        private VisualElement BuildNodeInspector(SkillNodeView node)
        {
            var inspectorContent = CreateInspectorContent();
            var serializedObject = new SerializedObject(node.Data);

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
                            node.RefreshUI();
                            serializedObject.ApplyModifiedProperties();
                        });
                    }

                    if(iterator.propertyPath == "NodeSize")
                    {
                        field.RegisterValueChangeCallback(evt =>
                        {
                            node.RefreshSize();
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