using System;
using UnityEngine;
using GameEvents;
using SkillTreeGraph.Core;

public class SkillTreeProgression : IEventListener<RequestNodeLevelUpEvent>, IDisposable
{
    private readonly SkillTreeRuntime _runtime;
    private readonly SkillTreeDatabase _database;
    private readonly ISkillContext _context;

    public SkillTreeProgression(SkillTreeRuntime runtime, SkillTreeDatabase database, ISkillContext context)
    {
        _runtime = runtime;
        _database = database;
        _context = context;

        this.StartListening();
    }

    public void Dispose()
    {
        this.StopListening();
    }

    public void OnEvent(RequestNodeLevelUpEvent eventData)
    {
        LevelUp(eventData.NodeId, eventData.OnSuccess, eventData.OnFail);
    }

    private void LevelUp(string nodeId, Action onSuccess, Action<string> onFail)
    {
        if (!_database.TryGetNode(nodeId, out var node)) return;

        var state = _runtime.GetOrCreate(nodeId);

        if (state.State == SkillNodeState.Locked || state.State == SkillNodeState.Maxed)
        {
            onFail?.Invoke("Skill is locked or maxed");
            return;
        }

        int targetLevel = state.CurrentLevel + 1;

        foreach (var cost in node.ResourcesCost)
        {
            if (!cost.CanAfford(_context, targetLevel))
            {
                Debug.Log($"Cannot afford skill: {node.DisplayName}");
                onFail?.Invoke("Resources not enough");
                return;
            }
        }

        foreach (var cost in node.ResourcesCost)
        {
            cost.Pay(_context, targetLevel);
        }

        state.SetLevel(targetLevel);
        onSuccess?.Invoke();

        if (targetLevel >= node.MaxLevel)
            state.SetState(SkillNodeState.Maxed);
        else
            state.SetState(SkillNodeState.Unlocked);

        EvaluateUnlocks();
    }

    public void EvaluateUnlocks()
    {
        foreach (var node in _database.SkillDatabase)
        {
            var state = _runtime.GetOrCreate(node.Id);
            if (state.State != SkillNodeState.Locked) continue;

            bool canUnlock = true;
            foreach (var condition in node.UnlockConditions)
            {
                if (!condition.CanUnlock(node, _runtime, _context))
                {
                    canUnlock = false;
                    break;
                }
            }

            if (!canUnlock) continue;

            state.SetState(SkillNodeState.Available);
            EventManager.TriggerEvent(new NodeAvailableEvent { NodeId = node.Id });
        }
    }
}