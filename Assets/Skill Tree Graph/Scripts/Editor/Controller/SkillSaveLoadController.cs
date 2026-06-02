using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkillTreeGraph.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class SkillSaveLoadController
    {
        [Serializable]
        public class SkillTreeSaveData
        {
            public string Id;
            public string SkillTreeName;
            public List<SkillNodeSaveData> nodes = new();
        }

        [Serializable]
        public class SkillNodeSaveData
        {
            public string Id, DisplayName, Description, IconGuid, IconName;
            public int MaxLevel = 5;
            [SerializeReference] public List<SkillCost> ResourcesCost;
            public List<string> ParentIds, ChildrenIds;
            [SerializeReference] public List<SkillUnlockCondition> UnlockConditions;
            [SerializeReference] public List<SkillEffect> Effects;
            public Vector2 UiToolkitPosition, CanvasPosition;
            public float NodeSize;

            public SkillNodeSaveData(SkillNode node) => SyncData(node, this);
        }

        private const string DEFAULT_SAVE_PATH = "Assets/Skill Tree Graph/";
        private const string EXAMPLE_SAVE_PATH = "Example Save Data/";

        private readonly GraphContext _graphContext;
        private readonly GraphControllerContext _controllerContext;
        private readonly Dictionary<string, Button> _buttons = new();

        private List<TextAsset> _exampleSaveEntries;
        private int _currentExampleEntrieIndex = 0;
        private string _currentSavePath;

        public SkillSaveLoadController(GraphContext graphContext, GraphControllerContext controllerContext)
        {
            _graphContext = graphContext;
            _controllerContext = controllerContext;

            string[] buttonNames = { "save-assets-button", "save-button", "load-button", "random-button" };
            foreach (var name in buttonNames)
                _buttons[name] = graphContext.Root.Q<Button>(name);

            ToggleEvents(true);
            LoadInitialSaveData();
        }

        private void ToggleEvents(bool register)
        {
            Action<Button, EventCallback<ClickEvent>> action = register
                ? (btn, cb) => btn.RegisterCallback<ClickEvent>(cb)
                : (btn, cb) => btn.UnregisterCallback<ClickEvent>(cb);

            action(_buttons["save-assets-button"], OnSaveAssetsButtonClicked);
            action(_buttons["save-button"], OnSaveButtonClicked);
            action(_buttons["load-button"], OnLoadButtonClicked);
            action(_buttons["random-button"], OnRandomButtonClicked);
        }

        public void Dispose()
        {
            ToggleEvents(false);
            AutoSave();
        }

        #region Mapping Logic
        private static void SyncData(SkillNode node, SkillNodeSaveData data)
        {
            data.Id = node.Id;
            data.DisplayName = node.DisplayName;
            data.Description = node.Description;
            data.MaxLevel = node.MaxLevel;
            data.UiToolkitPosition = node.UiToolkitPosition;
            data.CanvasPosition = node.CanvasPosition;
            data.NodeSize = node.NodeSize;

            if (node.Icon != null)
            {
                data.IconGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(node.Icon));
                data.IconName = node.Icon.name;
            }

            data.ResourcesCost = new(node.ResourcesCost ?? new());
            data.ParentIds = new(node.ParentIds ?? new());
            data.ChildrenIds = new(node.ChildrenIds ?? new());
            data.UnlockConditions = new(node.UnlockConditions ?? new());
            data.Effects = new(node.Effects ?? new());
        }

        private SkillNode ConvertToNode(SkillNodeSaveData data)
        {
            var node = ScriptableObject.CreateInstance<SkillNode>();
            node.Id = data.Id;
            node.DisplayName = data.DisplayName;
            node.Description = data.Description;
            node.Icon = SkillTreeEditorUtility.LoadSprite(data.IconGuid, data.IconName);
            node.MaxLevel = data.MaxLevel;
            node.ResourcesCost = new(data.ResourcesCost ?? new());
            node.ParentIds = new(data.ParentIds ?? new());
            node.ChildrenIds = new(data.ChildrenIds ?? new());
            node.UnlockConditions = new(data.UnlockConditions ?? new());
            node.Effects = new(data.Effects ?? new());
            node.UiToolkitPosition = data.UiToolkitPosition;
            node.CanvasPosition = data.CanvasPosition;
            node.NodeSize = data.NodeSize;
            return node;
        }
        #endregion

        #region Helper Methods
        private string GetRelativePath(string absolutePath) =>
            "Assets" + absolutePath.Substring(Application.dataPath.Length);

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        private void FinalizeOperation() => _controllerContext.Selection.ClearSelection();
        #endregion

        private void OnSaveAssetsButtonClicked(ClickEvent evt)
        {
            var db = SkillTreeEditorUtility.LoadScriptableAssets<SkillTreeDatabase>()
                .FirstOrDefault(x => x.Id == _graphContext.Settings.Id);

            string folderPath;
            if (db == null)
            {
                var path = EditorUtility.SaveFolderPanel("Save Assets", "Assets/", "");
                if (string.IsNullOrEmpty(path)) return;
                folderPath = GetRelativePath(path);
            }
            else
            {
                if (!EditorUtility.DisplayDialog("Overwrite Assets", "Overwrite existing assets?", "Yes", "Cancel")) return;
                folderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(db));
            }

            GenerateSkillTreeAssets(folderPath, db);
            FinalizeOperation();
        }

        private void OnSaveButtonClicked(ClickEvent evt) => OnSavingData();

        private void OnRandomButtonClicked(ClickEvent evt)
        {
            FinalizeOperation();
            LoadRandomExampleSaveData();
        }

        private void AutoSave()
        {
            if (_graphContext.Settings.AutoSaveLoad && !string.IsNullOrEmpty(_currentSavePath) && File.Exists(_currentSavePath))
                SaveData(_currentSavePath);
        }

        public void OnSavingData()
        {
            if (TryOverwriteSaveData()) return;
            SaveAsNewFile();
        }

        public bool TryOverwriteSaveData()
        {
            if (string.IsNullOrEmpty(_currentSavePath) || !File.Exists(_currentSavePath))
                return false;

            if (!EditorUtility.DisplayDialog("Overwrite File", "Update existing save file?", "Yes", "Cancel"))
                return false;

            SaveData(_currentSavePath);
            return true;
        }

        public void SaveAsNewFile()
        {
            EnsureDirectoryExists(DEFAULT_SAVE_PATH);
            string path = EditorUtility.SaveFilePanel("Save Skill Tree", DEFAULT_SAVE_PATH, "", "json");
            if (!string.IsNullOrEmpty(path)) SaveData(path);
            FinalizeOperation();
        }

        private void OnLoadButtonClicked(ClickEvent evt)
        {
            EnsureDirectoryExists(DEFAULT_SAVE_PATH);
            string path = EditorUtility.OpenFilePanel("Load Skill Tree", DEFAULT_SAVE_PATH, "json");
            if (!string.IsNullOrEmpty(path))
            {
                var data = LoadData(path);
                if (data != null)
                {
                    _currentSavePath = path;

                    _graphContext.Settings.SetLastSavePath(ToProjectRelativePath(path));
                    EditorUtility.SetDirty(_graphContext.Settings);
                    AssetDatabase.SaveAssets();

                    RebuildSkillTree(data);
                }
            }
            FinalizeOperation();
        }

        private void LoadInitialSaveData()
        {
            if (_graphContext.Settings.AutoSaveLoad && !string.IsNullOrEmpty(_graphContext.Settings.LastSavePath))
            {
                _currentSavePath = ToAbsolutePath(_graphContext.Settings.LastSavePath);

                var data = LoadData(_currentSavePath);
                if (data != null)
                {
                    RebuildSkillTree(data);
                    return;
                }
            }
            LoadRandomExampleSaveData();
        }

        private void SaveData(string path)
        {
            var saveData = InitializeSaveData();

            _currentSavePath = path;
            _graphContext.Settings.SetLastSavePath(ToProjectRelativePath(path));
            EditorUtility.SetDirty(_graphContext.Settings);
            AssetDatabase.SaveAssets();

            File.WriteAllText(path, EditorJsonUtility.ToJson(saveData, true));
            AssetDatabase.Refresh();
        }

        private SkillTreeSaveData LoadData(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
            var data = new SkillTreeSaveData();
            EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(path), data);
            return data;
        }

        private void LoadRandomExampleSaveData()
        {
            if (_exampleSaveEntries == null || _currentExampleEntrieIndex >= _exampleSaveEntries.Count)
            {
                var saveFiles = Resources.LoadAll<TextAsset>(EXAMPLE_SAVE_PATH);
                if (saveFiles.Length == 0) return;

                _exampleSaveEntries = saveFiles.OrderBy(x => UnityEngine.Random.value).ToList();
                _currentExampleEntrieIndex = 0;
            }

            var selectedFile = _exampleSaveEntries[_currentExampleEntrieIndex++];
            var data = new SkillTreeSaveData();
            EditorJsonUtility.FromJsonOverwrite(selectedFile.text, data);
            _currentSavePath = null;
            RebuildSkillTree(data);
        }

        private void RebuildSkillTree(SkillTreeSaveData data)
        {
            _controllerContext.ConnectionRenderer.Clear();
            _graphContext.NodeContainer.Clear();
            _graphContext.Collection.Clear();
            _graphContext.Settings.SetCurrentSkillTreeSetting(data.SkillTreeName, data.Id);

            foreach (var savedNode in data.nodes)
            {
                var node = ConvertToNode(savedNode);
                var view = _controllerContext.NodeCreation.CreateNodeView(node, node.UiToolkitPosition, node.NodeSize);
                _graphContext.Collection.AddNode(view);
            }
        }

        private SkillTreeSaveData InitializeSaveData()
        {
            var data = new SkillTreeSaveData
            {
                Id = string.IsNullOrEmpty(_graphContext.Settings.Id) ? GUID.Generate().ToString() : _graphContext.Settings.Id,
                SkillTreeName = _graphContext.Settings.SkillTreeName
            };

            foreach (var node in _graphContext.Collection.Nodes)
                data.nodes.Add(new SkillNodeSaveData(node));

            return data;
        }

        private void GenerateSkillTreeAssets(string path, SkillTreeDatabase database)
        {
            bool isExisting = database != null;
            if (!isExisting)
            {
                database = ScriptableObject.CreateInstance<SkillTreeDatabase>();
                database.Id = _graphContext.Settings.Id;
                AssetDatabase.CreateAsset(database, $"{path}/{_graphContext.Settings.SkillTreeName}-SkillTree.asset");
            }

            var currentNodes = _graphContext.Collection.Nodes;
            var currentIds = new HashSet<string>(currentNodes.Select(n => n.Id));

            AssetDatabase.StartAssetEditing();
            try
            {
                var nodesToRemove = database.SkillDatabase.Where(n => n == null || !currentIds.Contains(n.Id)).ToList();
                foreach (var nodeToDelete in nodesToRemove)
                {
                    string assetPath = AssetDatabase.GetAssetPath(nodeToDelete);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        database.RemoveNode(nodeToDelete);
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                }

                int i = 0;
                foreach (var node in _graphContext.Collection.Nodes)
                {
                    string nodeName = string.IsNullOrEmpty(node.DisplayName) ? $"SkillNode_{i}" : node.DisplayName;

                    if (isExisting && database.TryGetNode(node.Id, out var existing))
                    {
                        EditorUtility.CopySerializedManagedFieldsOnly(node, existing);
                        AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(existing), nodeName);
                        existing.name = nodeName;
                        EditorUtility.SetDirty(existing);
                    }
                    else
                    {
                        var asset = ScriptableObject.CreateInstance<SkillNode>();
                        EditorUtility.CopySerialized(node, asset);
                        AssetDatabase.CreateAsset(asset, $"{path}/{nodeName}.asset");
                        database.AddNode(asset);
                    }
                    i++;
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(database);
        }

        public void ClearSavePath()
        {
            _currentSavePath = null;
            _graphContext.Settings.SetLastSavePath(string.Empty);
            EditorUtility.SetDirty(_graphContext.Settings);
            AssetDatabase.SaveAssets();
        }

        private string ToProjectRelativePath(string absolutePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.GetRelativePath(projectRoot, absolutePath).Replace('\\', '/');
        }

        private string ToAbsolutePath(string relativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.GetFullPath(Path.Combine(projectRoot, relativePath));
        }
    }
}