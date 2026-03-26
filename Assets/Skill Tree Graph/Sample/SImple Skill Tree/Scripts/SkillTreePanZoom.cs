using UnityEngine;
using UnityEngine.EventSystems;
using SkillTreeGraph.Core;

public class SkillTreePanZoom : MonoBehaviour, IDragHandler, IBeginDragHandler, IScrollHandler
{
    [Header("References")]
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private RectTransform viewport;

    [Header("Zoom")]
    [SerializeField] private float initialZoom = 1.5f;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 2.0f;

    [Header("Pan")]
    [SerializeField] private float panSpeed = 1f;

    [Header("Graph Size (virtual space)")]
    [SerializeField] private Vector2 graphSize = new Vector2(3000, 3000);

    private float currentZoom = 1f;

    void Start()
    {
        CenterGraph();
    }

    void CenterGraph()
    {
        graphContainer.localScale = Vector3.one;
        graphContainer.anchoredPosition = Vector2.zero;
        currentZoom = initialZoom;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Middle &&
            eventData.button != PointerEventData.InputButton.Left)
            return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.delta * panSpeed;

        graphContainer.anchoredPosition += delta;

        ClampPosition();
    }

    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;

        if (Mathf.Approximately(scroll, 0))
            return;

        Zoom(scroll);
    }

    void Zoom(float scroll)
    {
        float previousZoom = currentZoom;

        currentZoom += scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        if (Mathf.Approximately(previousZoom, currentZoom))
            return;

        Vector2 mouseLocalBefore;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            graphContainer,
            Input.mousePosition,
            null,
            out mouseLocalBefore
        );

        graphContainer.localScale = Vector3.one * currentZoom;

        Vector2 mouseLocalAfter;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            graphContainer,
            Input.mousePosition,
            null,
            out mouseLocalAfter
        );

        Vector2 delta = mouseLocalAfter - mouseLocalBefore;

        graphContainer.anchoredPosition += delta * currentZoom;

        ClampPosition();
    }

    void ClampPosition()
    {
        Vector2 viewportSize = viewport.rect.size;

        Vector2 scaledGraph = graphSize * currentZoom;

        float limitX = Mathf.Max(0, (scaledGraph.x - viewportSize.x) * 0.5f);
        float limitY = Mathf.Max(0, (scaledGraph.y - viewportSize.y) * 0.5f);

        Vector2 pos = graphContainer.anchoredPosition;

        pos.x = Mathf.Clamp(pos.x, -limitX, limitX);
        pos.y = Mathf.Clamp(pos.y, -limitY, limitY);

        graphContainer.anchoredPosition = pos;
    }
}
