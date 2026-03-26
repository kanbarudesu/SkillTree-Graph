using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UITweener;
using System.Linq;
using GameEvents;
using SkillTreeGraph.Core;

public class SkillNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventListener<NodeStateChangedEvent>, IEventListener<NodeLevelUpEvent>
{
    [Header("Settings")]
    [SerializeField] private TreeRevealMode revealMode = TreeRevealMode.Progressive;
    [SerializeField] private Color lockedColor;
    [SerializeField] private Color availableColor;
    [SerializeField] private Color unlockedColor;
    [SerializeField] private Color maxedColor;

    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [SerializeField] private Image borderImage;

    [Header("Animations")]
    [SerializeField, SerializeReference, SRPeeker] private UITweenAnimation onSpawn;
    [SerializeField, SerializeReference, SRPeeker] private UITweenAnimation onHover;
    [SerializeField, SerializeReference, SRPeeker] private UITweenAnimation onAvaiable;
    [SerializeField, SerializeReference, SRPeeker] private UITweenAnimation onLevelUp;
    [SerializeField, SerializeReference, SRPeeker] private UITweenAnimation onLevelUpFail;

    private SkillNode _node;
    private SkillNodeRuntimeData _nodeState;
    private SkillTreeRuntime _runtime;
    private ISkillContext _context;

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

        SetVisualState(_nodeState.State);
        onSpawn.Play(RectTransform);
    }

    private void OnDestroy()
    {
        this.StopListening<NodeStateChangedEvent>();
        this.StopListening<NodeLevelUpEvent>();

        button.onClick.RemoveListener(OnNodeClicked);
    }

    private void OnNodeClicked()
    {
        EventManager.TriggerEvent(new RequestNodeLevelUpEvent
        {
            NodeId = _node.Id,
            OnSuccess = () => { onLevelUp.Play(RectTransform); },
            OnFail = message => { onLevelUpFail.Play(RectTransform); }
        });
    }

    public void OnEvent(NodeStateChangedEvent eventData)
    {
        if (_node != null && eventData.NodeId == _node.Id)
        {
            SetVisualState(eventData.NewState);
        }
    }

    public void OnEvent(NodeLevelUpEvent eventData)
    {
        if (_node != null && eventData.NodeId == _node.Id)
            ShowTooltip();
    }

    private void SetVisualState(SkillNodeState state)
    {
        icon.overrideSprite = state == SkillNodeState.Locked
                            && revealMode != TreeRevealMode.ShowAll
                            ? icon.overrideSprite : _node.Icon;

        borderImage.color = state switch
        {
            SkillNodeState.Locked => lockedColor,
            SkillNodeState.Available => availableColor,
            SkillNodeState.Unlocked => unlockedColor,
            SkillNodeState.Maxed => maxedColor,
            _ => borderImage.color
        };

        if (state == SkillNodeState.Available)
            onAvaiable.Play(RectTransform);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHover.Play(RectTransform);
        ShowTooltip();
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
        string description = _node.Description + "\n\n" + GetNodeEffectsDescription();
        string cost = isMaxLevel ? "" : "\n" + GetNodeResourcesCostDescription();

        HoverTooltip.Instance.Show(RectTransform, title, level, description, cost);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HoverTooltip.Instance.Hide();
    }

    private string GetNodeEffectsDescription()
    {
        return string.Join("\n", _node.Effects.Select(effect => effect.GetDescription(_nodeState.CurrentLevel, isMaxLevel)));
    }

    private string GetNodeResourcesCostDescription()
    {
        string requirement = string.Join("\n", _node.ResourcesCost.Select(cost => cost.GetDescription(_context, _nodeState.CurrentLevel + 1)));
        return string.IsNullOrEmpty(requirement) ? "" : $"Require : {requirement}";
    }
}