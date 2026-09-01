using System.IO;
using System.Linq;
using SkillTreeGraph.Core;
using UnityEditor;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class SkillSaveLoadController
    {
        private readonly GraphContext _graphContext;
        private readonly GraphControllerContext _controllerContext;
        private readonly ExampleSkillTreeProvider _exampleProvider = new();

        private string _currentSavePath;
        private bool _isExampleSaveData = false;

        public SkillSaveLoadController(GraphContext graphContext, GraphControllerContext controllerContext)
        {
            _graphContext = graphContext;
            _controllerContext = controllerContext;

            RegisterEvents();
            LoadInitialSaveData();
        }

        private void RegisterEvents()
        {
            _graphContext.SaveAsAssetButton.RegisterCallback<ClickEvent>(OnSaveAssetsButtonClicked);
            _graphContext.SaveButton.RegisterCallback<ClickEvent>(OnSaveButtonClicked);
            _graphContext.LoadButton.RegisterCallback<ClickEvent>(OnLoadButtonClicked);
            _graphContext.RandomTreeButton.RegisterCallback<ClickEvent>(OnRandomButtonClicked);
        }

        private void UnregisterEvents()
        {
            _graphContext.SaveAsAssetButton.UnregisterCallback<ClickEvent>(OnSaveAssetsButtonClicked);
            _graphContext.SaveButton.UnregisterCallback<ClickEvent>(OnSaveButtonClicked);
            _graphContext.LoadButton.UnregisterCallback<ClickEvent>(OnLoadButtonClicked);
            _graphContext.RandomTreeButton.UnregisterCallback<ClickEvent>(OnRandomButtonClicked);
        }

        public void Dispose()
        {
            UnregisterEvents();
            AutoSave();
        }

        private void FinalizeOperation()
        {
            _controllerContext.Selection.ClearSelection();
            _controllerContext.GroupSelection.ClearSelection();
        }

        private void OnSaveAssetsButtonClicked(ClickEvent evt)
        {
            var db = SkillTreeEditorUtility.LoadScriptableAssets<SkillTreeDatabase>()
                .FirstOrDefault(x => x.Id == _graphContext.Settings.Id);

            string folderPath;
            if (db == null)
            {
                var path = EditorUtility.SaveFolderPanel("Save Assets", "Assets/", "");
                if (string.IsNullOrEmpty(path)) return;
                folderPath = SkillTreePathUtility.GetRelativePath(path);
            }
            else
            {
                if (!EditorUtility.DisplayDialog("Overwrite Assets", "Overwrite existing assets?", "Yes", "Cancel")) return;
                folderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(db));
            }

            SkillTreeAssetGenerator.Generate(folderPath, db, _graphContext.Settings, _graphContext.Collection);
            FinalizeOperation();
        }

        private void OnSaveButtonClicked(ClickEvent evt) => OnSavingData();

        private void OnRandomButtonClicked(ClickEvent evt)
        {
            FinalizeOperation();
            LoadRandomExampleSaveData();
        }

        private static readonly string ScratchSavePath = SkillTreePathUtility.ToAbsolutePath(SkillTreePathUtility.ScratchSaveFile);

        private void AutoSave()
        {
            if (!_graphContext.Settings.AutoSaveLoad || _isExampleSaveData) return;

            if (!string.IsNullOrEmpty(_currentSavePath) && File.Exists(_currentSavePath))
            {
                SaveData(_currentSavePath);
                return;
            }

            AutoSaveScratch();
        }

        private void AutoSaveScratch()
        {
            if (_graphContext.Collection.Nodes.Count == 0) return;

            var saveData = SkillNodeDataMapper.ToSaveData(_graphContext.Settings, _graphContext.Collection);
            SkillTreeFileIO.Save(ScratchSavePath, saveData);

            EditorUtility.DisplayDialog(
                "Skill Tree Autosaved Temporarily",
                "This skill tree hasn't been saved to a file yet, so it was temporarily " +
                $"stashed here so it isn't lost:\n\n{ScratchSavePath}\n\n" +
                "Use Save or Save As to keep it permanently — this temp copy lives under " +
                "Temp/ and is not guaranteed to survive things like clearing Temp/ manually.",
                "OK");
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
            SkillTreePathUtility.EnsureDirectoryExists(SkillTreePathUtility.DefaultSavePath);
            string path = EditorUtility.SaveFilePanel("Save Skill Tree", SkillTreePathUtility.DefaultSavePath, "", "json");
            if (!string.IsNullOrEmpty(path)) SaveData(path);
            FinalizeOperation();
        }

        private void OnLoadButtonClicked(ClickEvent evt)
        {
            SkillTreePathUtility.EnsureDirectoryExists(SkillTreePathUtility.DefaultSavePath);
            string path = EditorUtility.OpenFilePanel("Load Skill Tree", SkillTreePathUtility.DefaultSavePath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                var data = SkillTreeFileIO.Load(path);
                if (data != null)
                {
                    _currentSavePath = path;

                    _graphContext.Settings.SetLastSavePath(SkillTreePathUtility.ToProjectRelativePath(path));
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
                _currentSavePath = SkillTreePathUtility.ToAbsolutePath(_graphContext.Settings.LastSavePath);

                var data = SkillTreeFileIO.Load(_currentSavePath);
                if (data != null)
                {
                    RebuildSkillTree(data);
                    return;
                }
            }

            if (_graphContext.Settings.AutoSaveLoad)
            {
                var scratchData = SkillTreeFileIO.Load(ScratchSavePath);
                if (scratchData != null)
                {
                    _currentSavePath = null;
                    RebuildSkillTree(scratchData);
                    return;
                }
            }

            LoadRandomExampleSaveData();
        }

        private void SaveData(string path)
        {
            var saveData = SkillNodeDataMapper.ToSaveData(_graphContext.Settings, _graphContext.Collection);

            _isExampleSaveData = false;
            _currentSavePath = path;
            _graphContext.Settings.SetLastSavePath(SkillTreePathUtility.ToProjectRelativePath(path));
            EditorUtility.SetDirty(_graphContext.Settings);
            AssetDatabase.SaveAssets();

            SkillTreeFileIO.Save(path, saveData);
            SkillTreeFileIO.Delete(ScratchSavePath);
        }

        private void LoadRandomExampleSaveData()
        {
            var data = _exampleProvider.GetNext();
            if (data == null) return;

            _isExampleSaveData = true;
            _currentSavePath = null;
            SkillTreeFileIO.Delete(ScratchSavePath);
            RebuildSkillTree(data);
        }

        private void RebuildSkillTree(SkillTreeSaveData data)
        {
            _controllerContext.ConnectionRenderer.Clear();
            _controllerContext.GroupSelection.ClearSelection();
            _graphContext.NodeContainer.Clear();
            _graphContext.Collection.Clear();
            _graphContext.Settings.SetCurrentSkillTreeSetting(data.SkillTreeName, data.Id);

            foreach (var savedNode in data.nodes)
            {
                var node = SkillNodeDataMapper.ToNode(savedNode);
                var view = _controllerContext.NodeCreation.CreateNodeView(node, node.UiToolkitPosition, node.NodeSize);
                _graphContext.Collection.AddNode(view);
            }
        }

        public void ClearSavePath()
        {
            _isExampleSaveData = false;
            _currentSavePath = null;
            _graphContext.Settings.SetLastSavePath(string.Empty);
            EditorUtility.SetDirty(_graphContext.Settings);
            AssetDatabase.SaveAssets();
            SkillTreeFileIO.Delete(ScratchSavePath);
        }
    }
}