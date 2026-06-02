using UnityEngine;
using SkillTreeGraph.Core;
using System;

public class SkillTreeController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TreeRevealMode revealMode = TreeRevealMode.Progressive;

    [Header("Save/Load")]
    [Tooltip("If set to false, the tree will save/load itself using PlayerPrefs. If true, your custom Save Manager must call ExportSaveData() and ImportSaveData().")]
    [SerializeField] private bool useCustomSaving;
    [SerializeField] private string saveKey = "SaveData_SkillTree";

    [Header("References")]
    [SerializeField] private GameObject playerGameObject;
    [SerializeField] private Transform nodeLayer;
    [SerializeField] private SkillNodeUI nodePrefab;
    [SerializeField] private SkillTreePanZoom panZoom;
    [SerializeField] private SkillTreeDatabase treeDatabase;
    [SerializeField] private SkillTreeConnectionRenderer connectionRenderer;

    private SkillTreeRuntime _runtimeData;
    private SkillContext _playerContext;

    private SkillTreeGraphBuilder _graphBuilder;

    private SkillTreeProgression _progression;
    private SkillTreeEffectSystem _effectSystem;
    private SkillTreeGraphPresenter _graphPresenter;

    private ISkillTreeSaveStorage _saveStorage;
    private IDisposable[] _disposables;

    private void Awake()
    {
        _runtimeData = new SkillTreeRuntime(treeDatabase);
        _playerContext = new SkillContext(playerGameObject);

        _graphBuilder = new SkillTreeGraphBuilder(nodeLayer, nodePrefab, connectionRenderer, _runtimeData, _playerContext);

        _progression = new SkillTreeProgression(_runtimeData, treeDatabase, _playerContext);
        _effectSystem = new SkillTreeEffectSystem(_runtimeData, _playerContext);
        _graphPresenter = new SkillTreeGraphPresenter(treeDatabase, _runtimeData, _graphBuilder, revealMode);

        _saveStorage = new PlayerPrefsSaveStorage(saveKey);
        _disposables = new IDisposable[] { _progression, _effectSystem, _graphPresenter };
    }

    private void Start()
    {
        if (!useCustomSaving)
        {
            var data = _saveStorage.Load();
            if (data != null) _runtimeData.ImportSaveData(data);
        }

        BuildSkillTree();
    }

    public void BuildSkillTree()
    {
        _graphPresenter.BuildTree();
        _progression.EvaluateUnlocks();
    }

    public void RefreshTreeDisplay()
    {
        _progression.EvaluateUnlocks();
    }

    public SkillTreeSaveData ExportSaveData()
    {
        return _runtimeData.ExportSaveData();
    }

    //Need to call this before building the tree
    public void ImportSaveData(SkillTreeSaveData data)
    {
        _runtimeData.ImportSaveData(data);
    }

    private void OnApplicationQuit()
    {
        //Auto Save
        if (!useCustomSaving && _saveStorage != null)
        {
            _saveStorage.Save(_runtimeData.ExportSaveData());
        }
    }

    private void OnDestroy()
    {
        if (_disposables != null)
            foreach (var d in _disposables) d?.Dispose();
    }
}