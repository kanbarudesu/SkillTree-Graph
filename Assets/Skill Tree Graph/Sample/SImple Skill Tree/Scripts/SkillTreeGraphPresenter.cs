using System;
using System.Collections.Generic;
using GameEvents;

namespace SkillTreeGraph.Core
{
    public enum TreeRevealMode
    {
        Progressive,
        ShowAll
    }

    public class SkillTreeGraphPresenter : IEventListener<NodeStateChangedEvent>, IDisposable
    {
        private readonly SkillTreeDatabase _database;
        private readonly SkillTreeRuntime _runtime;
        private readonly SkillTreeGraphBuilder _builder;
        private readonly TreeRevealMode _revealMode;

        private bool _isInitialized = false;

        public SkillTreeGraphPresenter(SkillTreeDatabase database, SkillTreeRuntime runtime, SkillTreeGraphBuilder builder, TreeRevealMode revealMode)
        {
            _database = database;
            _runtime = runtime;
            _builder = builder;
            _revealMode = revealMode;

            this.StartListening();
        }

        public void Dispose()
        {
            this.StopListening();
        }

        public void BuildTree()
        {
            _builder.Clear();

            var queue = new Queue<SkillNode>();
            var spawned = new HashSet<string>();

            var roots = _database.SkillDatabase.FindAll(x => x.ParentIds == null || x.ParentIds.Count == 0);
            foreach (var root in roots)
            {
                queue.Enqueue(root);
                spawned.Add(root.Id);
            }

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var state = _runtime.GetOrCreate(node.Id);

                _builder.SpawnNode(node, state);

                bool shouldRevealChildren = _revealMode == TreeRevealMode.ShowAll ||
                                            state.State != SkillNodeState.Locked ||
                                            node.ParentIds.Count == 0;

                if (shouldRevealChildren && node.ChildrenIds != null)
                {
                    foreach (var childId in node.ChildrenIds)
                    {
                        if (!spawned.Contains(childId) && _database.TryGetNode(childId, out var child))
                        {
                            queue.Enqueue(child);
                            spawned.Add(childId);
                        }
                    }
                }
            }

            _isInitialized = true;
        }

        public void OnEvent(NodeStateChangedEvent eventData)
        {
            if (!_isInitialized || _revealMode == TreeRevealMode.ShowAll) return;

            if (eventData.NewState == SkillNodeState.Available || eventData.NewState == SkillNodeState.Unlocked)
            {
                if (!_database.TryGetNode(eventData.NodeId, out var node)) return;
                if (node.ChildrenIds == null) return;

                foreach (var childId in node.ChildrenIds)
                {
                    if (!_builder.IsSpawned(childId) && _database.TryGetNode(childId, out var child))
                    {
                        var childState = _runtime.GetOrCreate(childId);
                        _builder.SpawnNode(child, childState);
                    }
                }
            }
        }
    }
}