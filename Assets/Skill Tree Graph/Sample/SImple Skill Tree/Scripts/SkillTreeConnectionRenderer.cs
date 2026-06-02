using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeConnectionRenderer : MaskableGraphic
{
    private struct ConnectionData
    {
        public RectTransform From;
        public RectTransform To;
        public float StartTime;
        public bool IsAnimating;
        public float Progress;
    }

    [Header("Settings")]
    [SerializeField] private float thickness = 24f;
    [SerializeField] private int curveSegments = 32;
    [SerializeField, Range(0f, 1f)] private float curveStrength = 0.5f;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private Texture lineTexture;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform container;

    private readonly List<ConnectionData> _connections = new(512);
    private readonly List<int> _activeAnimationIndices = new(32);

    public override Texture mainTexture => lineTexture != null ? lineTexture : base.mainTexture;

    private Vector3 _lastLocalPos;
    private Vector3 _lastLocalScale;

    public void Clear()
    {
        _connections.Clear();
        _activeAnimationIndices.Clear();
        SetVerticesDirty();
    }

    public void AddConnection(RectTransform from, RectTransform to)
    {
        if (from == null || to == null) return;

        var conn = new ConnectionData
        {
            From = from,
            To = to,
            StartTime = Time.time,
            IsAnimating = animationDuration > 0,
            Progress = animationDuration > 0 ? 0f : 1f
        };

        _connections.Add(conn);

        if (conn.IsAnimating)
            _activeAnimationIndices.Add(_connections.Count - 1);

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_connections.Count == 0) return;

        Rect viewportRect = GetLocalViewportRect();
        int vertexIndex = 0;

        for (int i = 0; i < _connections.Count; i++)
        {
            var conn = _connections[i];

            Vector2 start = conn.From.anchoredPosition;
            Vector2 end = conn.To.anchoredPosition;

            if (!IsVisible(start, end, viewportRect)) continue;

            DrawBezier(vh, start, end, conn.Progress, ref vertexIndex);
        }
    }

    private void DrawBezier(VertexHelper vh, Vector2 start, Vector2 end, float progress, ref int vIdx)
    {
        if (progress <= 0) return;

        Vector2 control = GetControlPoint(start, end);
        Vector2 prevPoint = start;
        int segmentsToDraw = Mathf.CeilToInt(curveSegments * progress);

        for (int i = 1; i <= segmentsToDraw; i++)
        {
            float t = i / (float)curveSegments * progress;
            float tActual = i / (float)curveSegments;
            if (tActual > progress) tActual = progress;

            Vector2 currPoint = CalculateBezier(start, control, end, tActual);

            AddLineSegment(vh, prevPoint, currPoint, ref vIdx, (i - 1f) / curveSegments, tActual);
            prevPoint = currPoint;

            if (tActual >= progress) break;
        }
    }

    private void LateUpdate()
    {
        if (container != null)
        {
            if (container.localPosition != _lastLocalPos || container.localScale != _lastLocalScale)
            {
                _lastLocalPos = container.localPosition;
                _lastLocalScale = container.localScale;
                SetVerticesDirty();
            }
        }

        if (_activeAnimationIndices.Count == 0) return;

        bool needsRedraw = false;
        for (int i = _activeAnimationIndices.Count - 1; i >= 0; i--)
        {
            int index = _activeAnimationIndices[i];
            var conn = _connections[index];

            float elapsed = Time.time - conn.StartTime;
            float newProgress = Mathf.Clamp01(elapsed / animationDuration);

            if (!Mathf.Approximately(conn.Progress, newProgress))
            {
                conn.Progress = newProgress;
                _connections[index] = conn;
                needsRedraw = true;
            }

            if (newProgress >= 1f)
            {
                _activeAnimationIndices.RemoveAt(i);
            }
        }

        if (needsRedraw)
            SetVerticesDirty();
    }

    private void AddLineSegment(VertexHelper vh, Vector2 start, Vector2 end, ref int vIdx, float u0, float u1)
    {
        Vector2 dir = (end - start).normalized;
        if (dir == Vector2.zero) return;
        Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);
        vh.AddVert(start - normal, color, new Vector2(u0, 0));
        vh.AddVert(start + normal, color, new Vector2(u0, 1));
        vh.AddVert(end + normal, color, new Vector2(u1, 1));
        vh.AddVert(end - normal, color, new Vector2(u1, 0));
        vh.AddTriangle(vIdx, vIdx + 1, vIdx + 2);
        vh.AddTriangle(vIdx, vIdx + 2, vIdx + 3);
        vIdx += 4;
    }

    private Vector2 CalculateBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t) => (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;
    private Vector2 GetControlPoint(Vector2 start, Vector2 end) => Vector2.Lerp(new Vector2(start.x, end.y), Vector2.Lerp(start, end, 0.5f), curveStrength);

    private bool IsVisible(Vector2 p1, Vector2 p2, Rect view)
    {
        Vector2 cp = GetControlPoint(p1, p2);

        float minX = Mathf.Min(p1.x, Mathf.Min(p2.x, cp.x));
        float maxX = Mathf.Max(p1.x, Mathf.Max(p2.x, cp.x));
        float minY = Mathf.Min(p1.y, Mathf.Min(p2.y, cp.y));
        float maxY = Mathf.Max(p1.y, Mathf.Max(p2.y, cp.y));

        float m = thickness;
        return maxX >= view.xMin - m && minX <= view.xMax + m && maxY >= view.yMin - m && minY <= view.yMax + m;
    }

    private Rect GetLocalViewportRect()
    {
        if (viewport == null) return new Rect(-10000, -10000, 20000, 20000);
        Vector3[] corners = new Vector3[4];
        viewport.GetWorldCorners(corners);
        Vector2 min = transform.InverseTransformPoint(corners[0]);
        Vector2 max = transform.InverseTransformPoint(corners[2]);
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }
}