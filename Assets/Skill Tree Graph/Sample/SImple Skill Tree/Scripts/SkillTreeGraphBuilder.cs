using System.Collections.Generic;
using UnityEngine;
using SkillTreeGraph.Core;

public class SkillTreeGraphBuilder
{
    private readonly Transform _nodeLayer;
    private readonly SkillNodeUI _nodePrefab;
    private readonly SkillTreeConnectionRenderer _connectionRenderer;
    private readonly SkillTreeRuntime _runtime;
    private readonly ISkillContext _context;

    private readonly Dictionary<string, SkillNodeUI> _spawnedNodeUi = new();

    public SkillTreeGraphBuilder(Transform nodeLayer, SkillNodeUI nodePrefab, SkillTreeConnectionRenderer connectionRenderer, SkillTreeRuntime runtime, ISkillContext context)
    {
        _nodeLayer = nodeLayer;
        _nodePrefab = nodePrefab;
        _connectionRenderer = connectionRenderer;
        _runtime = runtime;
        _context = context;
    }

    public void Clear()
    {
        foreach (Transform child in _nodeLayer)
            Object.Destroy(child.gameObject);

        _spawnedNodeUi.Clear();
        _connectionRenderer.Clear();
    }

    public bool IsSpawned(string id)
    {
        return _spawnedNodeUi.ContainsKey(id);
    }

    public SkillNodeUI SpawnNode(SkillNode node, SkillNodeRuntimeData state)
    {
        if (_spawnedNodeUi.TryGetValue(node.Id, out var existing))
            return existing;

        var nodeUi = Object.Instantiate(_nodePrefab, _nodeLayer);
        nodeUi.Initialize(node, state, _runtime, _context);

        var rect = nodeUi.RectTransform;
        rect.anchoredPosition = new Vector2(node.CanvasPosition.x, node.CanvasPosition.y);

        _spawnedNodeUi[node.Id] = nodeUi;

        if (node.ParentIds != null)
        {
            foreach (var parentId in node.ParentIds)
            {
                if (_spawnedNodeUi.TryGetValue(parentId, out var parentNode))
                {
                    _connectionRenderer.AddConnection(parentNode.RectTransform, nodeUi.RectTransform);
                }
            }
        }

        if (node.ChildrenIds != null)
        {
            foreach (var childId in node.ChildrenIds)
            {
                if (_spawnedNodeUi.TryGetValue(childId, out var childNode))
                {
                    _connectionRenderer.AddConnection(nodeUi.RectTransform, childNode.RectTransform);
                }
            }
        }

        return nodeUi;
    }
}