using System;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class GraphSelectionController
    {
        private VisualElement _background;
        private VisualElement _selectedNode;
        private VisualElement _selectedOptionContainer;
        private VisualElement _inspectorPanel;
        private readonly GraphInteractionController _interaction;

        public VisualElement SelectedNode => _selectedNode;

        public GraphSelectionController(GraphContext context, GraphInteractionController interaction)
        {
            _background = context.GridBackground;
            _inspectorPanel = context.Root.Q<VisualElement>("node-inspector-panel");
            _interaction = interaction;
            RegisterBackgroundClick();
        }

        private void RegisterBackgroundClick()
        {
            _background.RegisterCallback<KeyDownEvent>(OnBackgroundKeyDown);
            _background.RegisterCallback<PointerDownEvent>(OnBackgroundClicked);
        }

        private void OnBackgroundKeyDown(KeyDownEvent evt)
        {
            if (_interaction.CurrentMode == GraphInteractionMode.Connect && _selectedNode != null)
            {
                ClearSelection();
                _background.Focus();
            }
        }

        private void OnBackgroundClicked(PointerDownEvent evt)
        {
            if (evt.target == _background)
            {
                ClearSelection();
                _background.Focus();
            }
        }

        public void RegisterNode(VisualElement node, VisualElement nodeButton, VisualElement optionContainer, Action OnNodeClicked = null)
        {
            nodeButton.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (_interaction.CurrentMode == GraphInteractionMode.Connect)
                {
                    OnNodeClicked?.Invoke();
                    evt.StopPropagation();
                    return;
                }

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