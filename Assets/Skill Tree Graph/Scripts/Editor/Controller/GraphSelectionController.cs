using System;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class GraphSelectionController
    {
        private GraphControllerContext _controllerContext;

        private VisualElement _background;
        private VisualElement _selectedNode;
        private VisualElement _selectedOptionContainer;
        private VisualElement _inspectorPanel;

        public VisualElement SelectedNode => _selectedNode;

        public GraphSelectionController(GraphContext context, GraphControllerContext controllerContext)
        {
            _controllerContext = controllerContext;
            _background = context.GridBackground;
            _inspectorPanel = context.Root.Q<VisualElement>("node-inspector-panel");
            RegisterBackgroundClick();
        }

        private void RegisterBackgroundClick()
        {
            _background.RegisterCallback<PointerDownEvent>(OnBackgroundClicked);
        }

        private void OnBackgroundClicked(PointerDownEvent evt)
        {
            if (evt.target == _background)
            {
                ClearSelection();
                _controllerContext.Interaction.ExitMode();
                _background.Focus();
            }
        }

        public void RegisterNode(VisualElement node, VisualElement nodeButton, VisualElement optionContainer, Action OnNodeClicked = null)
        {
            nodeButton.RegisterCallback<PointerDownEvent>(evt =>
            {
                SelectNode(node, optionContainer, OnNodeClicked);
                evt.StopPropagation();
            });
        }

        private void SelectNode(VisualElement node, VisualElement optionContainer, Action OnNodeClicked = null)
        {
            if (_selectedNode == node)
                return;

            ClearSelection();

            _selectedNode = node;
            _selectedOptionContainer = optionContainer;

            _selectedOptionContainer.style.display = DisplayStyle.Flex;

            OnNodeClicked?.Invoke();
        }

        public void ClearSelection()
        {
            if (_selectedOptionContainer != null)
                _selectedOptionContainer.style.display = DisplayStyle.None;

            _selectedNode = null;
            _selectedOptionContainer = null;
            _inspectorPanel.AddToClassList("panel-exit");
        }

        public void Dispose()
        {
            _background.UnregisterCallback<PointerDownEvent>(OnBackgroundClicked);
        }
    }
}