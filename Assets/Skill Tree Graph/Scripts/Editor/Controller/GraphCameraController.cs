using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class GraphCameraController
    {
        private readonly VisualElement _root;
        private readonly VisualElement _graphContent;
        private readonly GridElement _grid;
        private readonly SkillTreeSettingData _settings;

        private float _currentZoom;
        private float _targetZoom;

        private Vector2 _currentPan;
        private Vector2 _targetPan;

        private bool _isPanning;
        private Vector2 _lastMousePosition;

        public GraphCameraController(GraphContext context)
        {
            _root = context.Root;
            _graphContent = context.GraphContent;
            _grid = context.GridBackground;
            _settings = context.Settings;

            _targetZoom = context.Settings.InitialZoom;
            _currentZoom = _targetZoom;

            RegisterCallbacks();
            RegisterGeometryInit();
        }

        #region Initialization

        private void RegisterGeometryInit()
        {
            _root.RegisterCallback<GeometryChangedEvent>(OnGeometryReady);
            if (_root.resolvedStyle.width > 0 && _root.resolvedStyle.height > 0)
                OnGeometryReady(null);
        }

        private void OnGeometryReady(GeometryChangedEvent evt)
        {
            _root.UnregisterCallback<GeometryChangedEvent>(OnGeometryReady);
            InitializeView();
        }

        private void InitializeView()
        {
            Vector2 viewportSize = _root.contentRect.size;

            Vector2 graphCenter = _settings.GraphBounds.center;
            Vector2 viewportCenter = viewportSize * 0.5f;

            _currentPan = viewportCenter - graphCenter * _currentZoom;
            _targetPan = _currentPan;

            ApplyTransform();
        }

        #endregion

        #region Update Loop

        public void Update()
        {
            _currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, Time.deltaTime * _settings.ZoomSmoothSpeed);
            _currentPan = Vector2.Lerp(_currentPan, _targetPan, Time.deltaTime * _settings.PanSmoothSpeed);

            ClampPan();
            ApplyTransform();
        }

        #endregion

        #region Input

        private void RegisterCallbacks()
        {
            _root.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _root.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _root.RegisterCallback<WheelEvent>(OnScroll);
        }

        public void Dispose()
        {
            _root.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            _root.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            _root.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            _root.UnregisterCallback<WheelEvent>(OnScroll);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == 2)
            {
                _isPanning = true;
                _lastMousePosition = evt.position;
                _root.CapturePointer(evt.pointerId);
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isPanning)
                return;

            Vector2 delta = (Vector2)evt.position - _lastMousePosition;
            _lastMousePosition = evt.position;

            _targetPan += delta;
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.button == 2)
            {
                _isPanning = false;
                _root.ReleasePointer(evt.pointerId);
            }
        }

        private void OnScroll(WheelEvent evt)
        {
            var target = evt.target as VisualElement;
            if (target != null && HasClassInHierarchy(target, "no-zoom")) return;

            float delta = -evt.delta.y * _settings.ZoomSpeed;

            Vector2 mouse = evt.localMousePosition;
            Vector2 worldPos = (mouse - _targetPan) / _targetZoom;

            float newZoom = Mathf.Clamp(_targetZoom + delta, _settings.MinZoom, _settings.MaxZoom);

            _targetZoom = newZoom;
            _targetPan = mouse - worldPos * _targetZoom;

            ClampTargetPan();
            evt.StopPropagation();
        }

        private bool HasClassInHierarchy(VisualElement element, string className)
        {
            while (element != null)
            {
                if (element.ClassListContains(className))
                    return true;

                element = element.parent;
            }
            return false;
        }

        #endregion

        #region Clamping

        private void ClampPan()
        {
            Vector2 viewportSize = _root.layout.size;
            Rect bounds = _settings.GraphBounds;

            float minPanX = -bounds.xMax * _currentZoom + viewportSize.x;
            float maxPanX = -bounds.xMin * _currentZoom;

            float minPanY = -bounds.yMax * _currentZoom + viewportSize.y;
            float maxPanY = -bounds.yMin * _currentZoom;

            _currentPan.x = Mathf.Clamp(_currentPan.x, minPanX, maxPanX);
            _currentPan.y = Mathf.Clamp(_currentPan.y, minPanY, maxPanY);

            float graphPixelWidth = bounds.width * _currentZoom;
            float graphPixelHeight = bounds.height * _currentZoom;

            if (graphPixelWidth < viewportSize.x)
                _currentPan.x = viewportSize.x * 0.5f - bounds.center.x * _currentZoom;

            if (graphPixelHeight < viewportSize.y)
                _currentPan.y = viewportSize.y * 0.5f - bounds.center.y * _currentZoom;

            _targetPan = _currentPan;
        }

        private void ClampTargetPan()
        {
            Vector2 viewportSize = _root.layout.size;
            Rect bounds = _settings.GraphBounds;

            float minPanX = -bounds.xMax * _targetZoom + viewportSize.x;
            float maxPanX = -bounds.xMin * _targetZoom;

            float minPanY = -bounds.yMax * _targetZoom + viewportSize.y;
            float maxPanY = -bounds.yMin * _targetZoom;

            _targetPan.x = Mathf.Clamp(_targetPan.x, minPanX, maxPanX);
            _targetPan.y = Mathf.Clamp(_targetPan.y, minPanY, maxPanY);
        }

        #endregion

        #region Transform

        private void ApplyTransform()
        {
            _graphContent.transform.position = _currentPan;
            _graphContent.transform.scale = new Vector3(_currentZoom, _currentZoom, 1f);

            _grid.Zoom = _currentZoom;
            _grid.PanOffset = _currentPan;
            _grid.MarkDirtyRepaint();
        }

        #endregion
    }
}