using TMPro;
using UITweener;
using UnityEngine;
using UnityEngine.UI;

public class HoverTooltip : MonoBehaviour
{
    public static HoverTooltip Instance { get; private set; }

    [Header("Components")]
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private ContentSizeFitter panelFitter;
    [SerializeField] private LayoutElement descLayoutElement;

    [Header("Settings")]
    [SerializeField] private float padding = 20f;
    [SerializeField] private float maxWidth = 200f;

    private RectTransform _rectTransform;

    private readonly Vector3[] _targetCorners = new Vector3[4];
    private readonly Vector3[] _canvasCorners = new Vector3[4];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _rectTransform = GetComponent<RectTransform>();

        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(RectTransform targetItem, string title, string level, string description, string cost)
    {
        gameObject.SetActive(true);

        UpdateTooltip(title, level, description, cost);
        SetTooltipPosition(targetItem);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void UpdateTooltip(string title, string level, string description, string cost)
    {
        titleText.text = title;
        levelText.text = level;
        descText.text = description;
        costText.text = cost;

        descLayoutElement.enabled = false;

        float naturalWidth = LayoutUtility.GetPreferredWidth(descText.rectTransform);

        if (naturalWidth > maxWidth)
        {
            descLayoutElement.enabled = true;
            descLayoutElement.preferredWidth = maxWidth;
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
    }

    private void SetTooltipPosition(RectTransform targetItem)
    {
        targetItem.GetWorldCorners(_targetCorners);

        Vector3 itemTopCenter = (_targetCorners[1] + _targetCorners[2]) / 2f;
        Vector3 itemBottomCenter = (_targetCorners[0] + _targetCorners[3]) / 2f;

        canvasRect.GetWorldCorners(_canvasCorners);
        float canvasTopY = _canvasCorners[1].y;
        float canvasLeftX = _canvasCorners[0].x;
        float canvasRightX = _canvasCorners[2].x;

        float tooltipWorldHeight = _rectTransform.rect.height * canvasRect.localScale.y;
        float worldPadding = padding * canvasRect.localScale.y;

        if (itemTopCenter.y + tooltipWorldHeight + worldPadding > canvasTopY)
        {
            _rectTransform.pivot = new Vector2(0.5f, 1f);
            transform.position = itemBottomCenter - new Vector3(0, worldPadding, 0);
        }
        else
        {
            _rectTransform.pivot = new Vector2(0.5f, 0f);
            transform.position = itemTopCenter + new Vector3(0, worldPadding, 0);
        }

        Vector3 pos = transform.position;
        float tooltipWorldHalfWidth = _rectTransform.rect.width / 2f * canvasRect.localScale.x;

        float minX = canvasLeftX + tooltipWorldHalfWidth + worldPadding;
        float maxX = canvasRightX - tooltipWorldHalfWidth - worldPadding;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }
}