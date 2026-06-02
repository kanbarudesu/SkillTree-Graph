using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using GameEvents;
using SkillTreeGraph.Core;

public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventListener<NodeStateChangedEvent>, IEventListener<NodeLevelUpEvent>
{
    [Header("Settings")]
    [SerializeField] private TreeRevealMode revealMode = TreeRevealMode.Progressive;
    [SerializeField] private Color lockedColor;
    [SerializeField] private Color availableColor;
    [SerializeField] private Color cantAffordColor;
    [SerializeField] private Color unlockedColor;
    [SerializeField] private Color maxedColor;

    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private Image borderImage;

    private SkillNode _node;
    private SkillNodeRuntimeData _nodeState;
    private SkillTreeRuntime _runtime;
    private ISkillContext _context;

    private bool _wasAffordable;

    private bool isMaxLevel => _nodeState.CurrentLevel == _node.MaxLevel;
    public RectTransform RectTransform => transform as RectTransform;

    public void Initialize(SkillNode node, SkillNodeRuntimeData nodeState, SkillTreeRuntime runtime, ISkillContext context)
    {
        _node = node;
        _nodeState = nodeState;
        _runtime = runtime;
        _context = context;

        RectTransform.sizeDelta = new Vector2(_node.NodeSize, _node.NodeSize);

        this.StartListening<NodeStateChangedEvent>();
        this.StartListening<NodeLevelUpEvent>();
        button.onClick.AddListener(OnNodeClicked);

        RefreshDisplay(_nodeState.State);
    }

    private void OnDestroy()
    {
        this.StopListening<NodeStateChangedEvent>();
        this.StopListening<NodeLevelUpEvent>();

        button.onClick.RemoveListener(OnNodeClicked);
    }

    private void OnNodeClicked() => EventManager.TriggerEvent(new RequestNodeLevelUpEvent { NodeId = _node.Id, });

    private void RefreshDisplay(SkillNodeState state)
    {
        bool canAfford = CanAfford();
        bool isCurrentlyAffordable = (state == SkillNodeState.Available || state == SkillNodeState.Unlocked) && canAfford;

        if (isCurrentlyAffordable != _wasAffordable)
        {
            //Play animation if needed here when the node becomes not affordable
            _wasAffordable = isCurrentlyAffordable;
        }

        borderImage.color = state switch
        {
            SkillNodeState.Locked => lockedColor,
            SkillNodeState.Available => availableColor,
            SkillNodeState.Unlocked => unlockedColor,
            SkillNodeState.Maxed => maxedColor,
            _ => borderImage.color
        };

        if (!canAfford && state != SkillNodeState.Locked && state != SkillNodeState.Maxed)
            borderImage.color = cantAffordColor;

        icon.overrideSprite = state == SkillNodeState.Locked && revealMode != TreeRevealMode.ShowAll ? icon.overrideSprite : _node.Icon;
    }

    private bool CanAfford()
    {
        foreach (var cost in _node.ResourcesCost)
            if (!cost.CanAfford(_context, _nodeState, _nodeState.CurrentLevel + 1))
                return false;
        return true;
    }

    private void ShowTooltip()
    {
        if (_nodeState.State == SkillNodeState.Locked)
        {
            var conditions = _node.UnlockConditions.Select(c => c.GetDescription(_node, _runtime, _context));
            string lockedDescription = string.Join("\n", conditions);

            HoverTooltip.Instance.Show(RectTransform, "Locked", "", lockedDescription, "");
            return;
        }

        string title = _node.DisplayName;
        string level = $"Level {_nodeState.CurrentLevel + "/" + _node.MaxLevel}";
        string description = _node.Description + "\n" + GetNodeEffectsDescription();
        string cost = isMaxLevel ? "" : GetNodeResourcesCostDescription();

        HoverTooltip.Instance.Show(RectTransform, title, level, description, cost);
    }

    private string GetNodeEffectsDescription()
    {
        return string.Join("\n", _node.Effects.Select(effect => effect.GetDescription(_context, _nodeState.CurrentLevel, isMaxLevel)));
    }

    private string GetNodeResourcesCostDescription()
    {
        string requirement = string.Join("\n", _node.ResourcesCost.Select(cost => cost.GetDescription(_context, _nodeState, _nodeState.CurrentLevel + 1)));
        return string.IsNullOrEmpty(requirement) ? "" : $"Require : {requirement}";
    }

    public void OnEvent(NodeStateChangedEvent eventData)
    {
        if (_node != null && eventData.NodeId == _node.Id)
            RefreshDisplay(eventData.NewState);
    }

    public void OnEvent(NodeLevelUpEvent eventData)
    {
        if (_node != null && eventData.NodeId == _node.Id)
            ShowTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
            ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverTooltip.Instance.Hide();
    }
}