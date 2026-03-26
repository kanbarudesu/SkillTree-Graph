using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeConnectionRenderer : MaskableGraphic
{
    private class Connection
    {
        public RectTransform From;
        public RectTransform To;

        public float StartTime;
        public float Duration;
        public float Progress;
        public bool IsAnimating;

        public bool Dirty;

        public Vector2[] CurvePoints;
    }

    private enum ConnectionMode
    {
        Straight,
        Bezier
    }

    [Header("Settings")]
    [SerializeField] private ConnectionMode _connectionMode;
    [SerializeField] private int _curveSegments = 20;
    [SerializeField] private float _thickness = 6f;
    [SerializeField] private float _animationDuration = 0.3f;
    [SerializeField, Range(0f, 1f)] private float _curveStrength = 0.5f;

    private readonly List<Connection> _connections = new List<Connection>(256);

    private RectTransform _rect;

    protected override void Awake()
    {
        base.Awake();
        _rect = transform as RectTransform;
    }

    public void Clear()
    {
        _connections.Clear();
        SetVerticesDirty();
    }

    public void AddConnection(RectTransform from, RectTransform to)
    {
        var conn = new Connection
        {
            From = from,
            To = to,
            Duration = _animationDuration,
            StartTime = Time.time,
            Progress = _animationDuration > 0 ? 0f : 1f,
            IsAnimating = _animationDuration > 0,
            Dirty = true,
            CurvePoints = new Vector2[_curveSegments + 1]
        };

        _connections.Add(conn);

        SetVerticesDirty();
    }

    private Vector2 WorldToLocal(Vector3 world)
    {
        return _rect.InverseTransformPoint(world);
    }

    private Vector2 GetControlPoint(Vector2 parent, Vector2 child)
    {
        // Straight line mode
        if (_connectionMode == ConnectionMode.Straight)
            return Vector2.Lerp(parent, child, 0.5f);

        // Bezier mode
        float xDelta = child.x - parent.x;
        float yDelta = child.y - parent.y;

        if (Mathf.Abs(xDelta) < 0.01f || Mathf.Abs(yDelta) < 0.01f)
            return Vector2.Lerp(parent, child, 0.5f);

        Vector2 control = new Vector2(parent.x, child.y);
        Vector2 mid = Vector2.Lerp(parent, child, 0.5f);

        return Vector2.Lerp(mid, control, _curveStrength);
    }

    private Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        float rt = 1f - t;

        return rt * rt * a +
               2f * rt * t * b +
               t * t * c;
    }

    private void UpdateCurve(Connection c)
    {
        Vector2 start = WorldToLocal(c.From.position);
        Vector2 end = WorldToLocal(c.To.position);

        Vector2 control = GetControlPoint(start, end);

        for (int i = 0; i <= _curveSegments; i++)
        {
            float t = (float)i / _curveSegments;
            c.CurvePoints[i] = Bezier(start, control, end, t);
        }

        c.Dirty = false;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        int vertIndex = 0;

        for (int i = 0; i < _connections.Count; i++)
        {
            var c = _connections[i];

            if (c.From == null || c.To == null)
                continue;

            if (c.Dirty)
                UpdateCurve(c);

            int maxSegment = Mathf.CeilToInt(_curveSegments * c.Progress);

            if (maxSegment <= 0)
                continue;

            Vector2 prev = c.CurvePoints[0];

            for (int s = 1; s <= maxSegment; s++)
            {
                Vector2 point = c.CurvePoints[s];

                DrawSegment(vh, prev, point, ref vertIndex);

                prev = point;
            }
        }
    }

    private void DrawSegment(VertexHelper vh, Vector2 start, Vector2 end, ref int index)
    {
        Vector2 dir = (end - start).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * (_thickness * 0.5f);

        UIVertex v0 = UIVertex.simpleVert;
        UIVertex v1 = UIVertex.simpleVert;
        UIVertex v2 = UIVertex.simpleVert;
        UIVertex v3 = UIVertex.simpleVert;

        v0.color = color;
        v1.color = color;
        v2.color = color;
        v3.color = color;

        v0.position = start - normal;
        v1.position = start + normal;
        v2.position = end + normal;
        v3.position = end - normal;

        vh.AddVert(v0);
        vh.AddVert(v1);
        vh.AddVert(v2);
        vh.AddVert(v3);

        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);

        index += 4;
    }

    private void LateUpdate()
    {
        bool dirtyMesh = false;

        for (int i = 0; i < _connections.Count; i++)
        {
            var c = _connections[i];

            if (c.IsAnimating)
            {
                float t = (Time.time - c.StartTime) / c.Duration;

                float newProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));

                if (!Mathf.Approximately(newProgress, c.Progress))
                {
                    c.Progress = newProgress;
                    dirtyMesh = true;
                }

                if (c.Progress >= 1f)
                    c.IsAnimating = false;
            }
        }

        if (dirtyMesh)
            SetVerticesDirty();
    }
}