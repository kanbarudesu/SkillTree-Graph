using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class NodeGroupSelectionController
    {
        private const string GroupedClass = "node-grouped";

        private readonly GraphContext _graphContext;
        private readonly VisualElement _background;
        private readonly VisualElement _marqueeBox;
        private readonly GraphInteractionController _interaction;

        private readonly HashSet<SkillNodeView> _selected = new();

        private bool _marqueeActive;
        private Vector2 _marqueeStartScreenPos;
        private HashSet<SkillNodeView> _selectionBeforeMarquee;

        public IReadOnlyCollection<SkillNodeView> SelectedNodes => _selected;

        public NodeGroupSelectionController(GraphContext graphContext, GraphInteractionController interaction)
        {
            _graphContext = graphContext;
            _background = graphContext.GridBackground;
            _interaction = interaction;

            _marqueeBox = CreateMarqueeBox();
            _background.Add(_marqueeBox);

            _background.RegisterCallback<KeyDownEvent>(OnBackgroundKeyDown);
            _background.RegisterCallback<PointerDownEvent>(OnBackgroundPointerDown);
            _background.RegisterCallback<PointerMoveEvent>(OnBackgroundPointerMove);
            _background.RegisterCallback<PointerUpEvent>(OnBackgroundPointerUp);
        }

        public bool Contains(SkillNodeView node) => _selected.Contains(node);

        public void RegisterNode(SkillNodeView node, VisualElement mainButton)
        {
            mainButton.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (_interaction.CurrentMode == GraphInteractionMode.Connect) return;

                if (evt.shiftKey || evt.commandKey)
                {
                    ToggleInGroup(node);
                    evt.StopPropagation();
                }
                else if (!_selected.Contains(node))
                    SelectOnly(node);
            });
        }

        public void ToggleInGroup(SkillNodeView node)
        {
            if (_selected.Remove(node))
                SetHighlighted(node, false);
            else
            {
                _selected.Add(node);
                SetHighlighted(node, true);
            }
        }

        public void SelectOnly(SkillNodeView node)
        {
            ClearSelection();
            _selected.Add(node);
            SetHighlighted(node, true);
        }

        public void ClearSelection()
        {
            foreach (var node in _selected)
                SetHighlighted(node, false);
            _selected.Clear();
        }

        public void SetAndHighlightSelection(IEnumerable<SkillNodeView> nodes)
        {
            foreach (var node in nodes)
            {
                _selected.Add(node);
                SetHighlighted(node, true);
            }
        }

        private void SetHighlighted(SkillNodeView node, bool highlighted)
        {
            var mainButton = node.Q<VisualElement>("main-button");
            if (mainButton == null) return;

            if (highlighted) mainButton.AddToClassList(GroupedClass);
            else mainButton.RemoveFromClassList(GroupedClass);
        }

        private static VisualElement CreateMarqueeBox()
        {
            var box = new VisualElement { name = "marquee-select-box", pickingMode = PickingMode.Ignore };
            box.style.position = Position.Absolute;
            box.style.display = DisplayStyle.None;
            box.style.backgroundColor = new StyleColor(new Color(0.3f, 0.6f, 1f, 0.15f));
            box.style.borderLeftWidth = 1;
            box.style.borderRightWidth = 1;
            box.style.borderTopWidth = 1;
            box.style.borderBottomWidth = 1;

            var borderColor = new StyleColor(new Color(0.3f, 0.6f, 1f, 0.9f));
            box.style.borderLeftColor = borderColor;
            box.style.borderRightColor = borderColor;
            box.style.borderTopColor = borderColor;
            box.style.borderBottomColor = borderColor;

            return box;
        }

        private void OnBackgroundKeyDown(KeyDownEvent evt)
        {
            if (_interaction.CurrentMode == GraphInteractionMode.Connect && _selected.Count > 0)
                ClearSelection();
        }

        private void OnBackgroundPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;

            _marqueeActive = true;
            _marqueeStartScreenPos = evt.localPosition;
            _selectionBeforeMarquee = new HashSet<SkillNodeView>(_selected);

            if (evt.target == _background)
            {
                ClearSelection();
                _background.Focus();
            }

            _background.CapturePointer(evt.pointerId);
        }

        private void OnBackgroundPointerMove(PointerMoveEvent evt)
        {
            if (!_marqueeActive) return;

            Vector2 current = evt.localPosition;
            bool additive = evt.shiftKey;

            DrawMarqueeBox(_marqueeStartScreenPos, current);
            RecomputeMarqueeSelection(_marqueeStartScreenPos, current, additive);
        }

        private void OnBackgroundPointerUp(PointerUpEvent evt)
        {
            if (!_marqueeActive) return;

            _marqueeActive = false;
            _selectionBeforeMarquee = null;
            _background.ReleasePointer(evt.pointerId);
            HideMarqueeBox();
        }

        private void DrawMarqueeBox(Vector2 a, Vector2 b)
        {
            _marqueeBox.style.left = Mathf.Min(a.x, b.x);
            _marqueeBox.style.top = Mathf.Min(a.y, b.y);
            _marqueeBox.style.width = Mathf.Abs(b.x - a.x);
            _marqueeBox.style.height = Mathf.Abs(b.y - a.y);
            _marqueeBox.style.display = DisplayStyle.Flex;
        }

        private void HideMarqueeBox() => _marqueeBox.style.display = DisplayStyle.None;

        private void RecomputeMarqueeSelection(Vector2 screenA, Vector2 screenB, bool additive)
        {
            Vector2 graphA = SkillTreeEditorUtility.ScreenToGraph(_graphContext.GraphContent, screenA);
            Vector2 graphB = SkillTreeEditorUtility.ScreenToGraph(_graphContext.GraphContent, screenB);

            float minX = Mathf.Min(graphA.x, graphB.x);
            float maxX = Mathf.Max(graphA.x, graphB.x);
            float minY = Mathf.Min(graphA.y, graphB.y);
            float maxY = Mathf.Max(graphA.y, graphB.y);

            var newSelection = additive
                ? new HashSet<SkillNodeView>(_selectionBeforeMarquee)
                : new HashSet<SkillNodeView>();

            foreach (var node in _graphContext.Collection.NodeViews)
            {
                Vector2 pos = node.Data.UiToolkitPosition;
                float size = node.Data.NodeSize;

                bool intersects = pos.x < maxX && pos.x + size > minX && pos.y < maxY && pos.y + size > minY;
                if (intersects)
                    newSelection.Add(node);
            }

            ApplySelection(newSelection);
        }

        private void ApplySelection(HashSet<SkillNodeView> newSelection)
        {
            foreach (var node in _selected.Where(node => !newSelection.Contains(node)))
                SetHighlighted(node, false);

            foreach (var node in newSelection.Where(node => !_selected.Contains(node)))
                SetHighlighted(node, true);

            _selected.Clear();
            foreach (var node in newSelection)
                _selected.Add(node);
        }

        public void Dispose()
        {
            _background.UnregisterCallback<PointerDownEvent>(OnBackgroundPointerDown, TrickleDown.TrickleDown);
            _background.UnregisterCallback<PointerMoveEvent>(OnBackgroundPointerMove, TrickleDown.TrickleDown);
            _background.UnregisterCallback<PointerUpEvent>(OnBackgroundPointerUp, TrickleDown.TrickleDown);
        }
    }
}
