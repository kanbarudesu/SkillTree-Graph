using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class NodeDragManipulator : PointerManipulator
    {
        private SkillTreeSettingData _settings;
        private GraphContext _graphContext;
        private VisualElement _graphContent;
        private SkillNodeView _node;
        private readonly NodeGroupSelectionController _groupSelection;

        private Vector2 _startMousePosition;
        private Vector2 _startNodePosition;

        private Dictionary<SkillNodeView, Vector2> _groupStartPositions;

        private bool _dragging;

        public NodeDragManipulator(GraphContext context, SkillNodeView node, NodeGroupSelectionController groupSelection)
        {
            _graphContext = context;
            _graphContent = context.GraphContent;
            _settings = context.Settings;
            _node = node;
            _groupSelection = groupSelection;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
                return;

            _startMousePosition = evt.position;
            _startNodePosition = new Vector2(target.resolvedStyle.left, target.resolvedStyle.top);

            _groupStartPositions = null;
            if (_groupSelection.Contains(_node) && _groupSelection.SelectedNodes.Count > 1)
            {
                _groupStartPositions = new Dictionary<SkillNodeView, Vector2>();
                foreach (var groupedNode in _groupSelection.SelectedNodes)
                {
                    if (groupedNode == _node) continue;
                    _groupStartPositions[groupedNode] = groupedNode.Data.UiToolkitPosition;
                }
            }

            target.CapturePointer(evt.pointerId);
            _dragging = true;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_dragging)
                return;

            Vector2 mouseDelta = (Vector2)evt.position - _startMousePosition;
#if UNITY_6000_3_OR_NEWER
            float zoom = _graphContent.style.scale.value.value.x;
#else
            float zoom = _graphContent.transform.scale.x;
#endif
            Vector2 graphDelta = mouseDelta / zoom;

            Vector2 newPosition = _startNodePosition + graphDelta;

            if (!evt.shiftKey && _settings.EnableSnap)
            {
                newPosition.x = Mathf.Round(newPosition.x / _settings.SnapSize) * _settings.SnapSize;
                newPosition.y = Mathf.Round(newPosition.y / _settings.SnapSize) * _settings.SnapSize;
            }

            float nodeWidth = target.resolvedStyle.width;
            float nodeHeight = target.resolvedStyle.height;

            float minX = _settings.GraphBounds.xMin;
            float maxX = _settings.GraphBounds.xMax - nodeWidth;

            float minY = _settings.GraphBounds.yMin;
            float maxY = _settings.GraphBounds.yMax - nodeHeight;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            target.style.left = newPosition.x;
            target.style.top = newPosition.y;

            _node.SetPosition(newPosition);

            if (_groupStartPositions != null)
            {
                Vector2 appliedDelta = newPosition - _startNodePosition;
                foreach (var kvp in _groupStartPositions)
                    kvp.Key.SetPosition(kvp.Value + appliedDelta);
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_dragging)
                return;

            target.ReleasePointer(evt.pointerId);
            _dragging = false;
            _groupStartPositions = null;
            _graphContext.GridBackground.Focus();
        }
    }
}
