using System;
using GameEvents;
using SkillTreeGraph.Core;

public class SkillTreeEffectSystem : IEventListener<NodeLevelUpEvent>, IDisposable
{
    private readonly SkillTreeRuntime _runtime;
    private readonly ISkillContext _context;

    public SkillTreeEffectSystem(SkillTreeRuntime runtime, ISkillContext context)
    {
        _runtime = runtime;
        _context = context;

        this.StartListening();
    }

    public void Dispose()
    {
        this.StopListening();
    }

    public void OnEvent(NodeLevelUpEvent eventData)
    {
        if (eventData.NewLevel > 0)
        {
            ApplyNodeEffects(eventData.NodeId, eventData.NewLevel);
        }
    }

    private void ApplyNodeEffects(string nodeId, int level)
    {
        var node = _runtime.GetNode(nodeId);
        if (node == null) return;

        foreach (var effect in node.Effects)
        {
            effect.Apply(_context, level);
        }
    }
}
