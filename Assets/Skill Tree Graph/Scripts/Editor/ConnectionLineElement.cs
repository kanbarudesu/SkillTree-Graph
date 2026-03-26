using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class ConnectionLineElement : VisualElement
    {
        private SkillNodeView _startNode;
        private SkillNodeView _endNode;

        private VisualElement _connectionLayer;
        private SkillTreeSettingData _settings;

        private Vector2 _startPoint;
        private Vector2 _endPoint;

        private const float ARROW_SIZE = 12f;
        private const float ARROW_ANGLE = 25f;

        public ConnectionLineElement(SkillNodeView startNode, SkillNodeView endNode, VisualElement connectionLayer, SkillTreeSettingData settings)
        {
            _startNode = startNode;
            _endNode = endNode;
            _connectionLayer = connectionLayer;
            _settings = settings;

            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0;
            style.top = 0;
            style.right = 0;
            style.bottom = 0;

            _startNode.OnPositionChanged += OnNodeMoved;
            _endNode.OnPositionChanged += OnNodeMoved;
            generateVisualContent += GenerateVisualContent;

            UpdateEndpoints();
        }

        private void OnNodeMoved(Vector2 _)
        {
            UpdateEndpoints();
            MarkDirtyRepaint();
        }

        private void UpdateEndpoints()
        {
            if (_startNode == null || _endNode == null)
                return;

            Vector2 startWorld = _startNode.worldBound.center;
            Vector2 endWorld = _endNode.worldBound.center;
            Vector2 startLocal = _connectionLayer.WorldToLocal(startWorld);
            Vector2 endLocal = _connectionLayer.WorldToLocal(endWorld);
            Vector2 direction = (endLocal - startLocal).normalized;
            Vector2 startEdge = GetRectangleEdge(startLocal, direction, _startNode.Data.NodeSize);
            Vector2 endEdge = GetRectangleEdge(endLocal, -direction, _endNode.Data.NodeSize);

            _startPoint = startEdge;
            _endPoint = endEdge;
        }

        private void GenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;

            painter.lineWidth = 2f;
            painter.strokeColor = Color.white;

            painter.BeginPath();
            painter.MoveTo(_startPoint);
            painter.LineTo(_endPoint);
            painter.Stroke();

            DrawArrow(painter);
        }

        private void DrawArrow(Painter2D painter)
        {
            Vector2 direction = (_endPoint - _startPoint).normalized;

            if (direction == Vector2.zero)
                return;

            float angle = ARROW_ANGLE * Mathf.Deg2Rad;

            Vector2 right = Rotate(direction, angle);
            Vector2 left = Rotate(direction, -angle);

            Vector2 arrowPoint1 = _endPoint - right * ARROW_SIZE;
            Vector2 arrowPoint2 = _endPoint - left * ARROW_SIZE;

            painter.BeginPath();
            painter.MoveTo(_endPoint);
            painter.LineTo(arrowPoint1);
            painter.MoveTo(_endPoint);
            painter.LineTo(arrowPoint2);
            painter.Stroke();
        }

        private Vector2 Rotate(Vector2 v, float radians)
        {
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

        private Vector2 GetRectangleEdge(Vector2 center, Vector2 direction, float size)
        {
            float halfSize = size * 0.5f;

            float dx = direction.x;
            float dy = direction.y;

            float scaleX = dx == 0 ? float.MaxValue : halfSize / Mathf.Abs(dx);
            float scaleY = dy == 0 ? float.MaxValue : halfSize / Mathf.Abs(dy);

            float t = Mathf.Min(scaleX, scaleY);

            return center + direction * t;
        }

        public void Dispose()
        {
            if (_startNode != null)
                _startNode.OnPositionChanged -= OnNodeMoved;

            if (_endNode != null)
                _endNode.OnPositionChanged -= OnNodeMoved;

            _startNode = null;
            _endNode = null;
        }
    }
}