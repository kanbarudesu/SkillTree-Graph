using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class GraphContext
    {
        public VisualElement Root;
        public VisualElement GraphContent;
        public VisualElement NodeContainer;
        public VisualElement ConnectionContainer;
        public GridElement GridBackground;
        public SkillTreeSettingData Settings;
        public SkillTreeCollection Collection;
    }

    public class GraphControllerContext
    {
        public GraphCameraController GraphCam;
        public GraphSelectionController Selection;
        public NodeGroupSelectionController GroupSelection;
        public SettingPanelController SettingPanel;
        public GraphNodeCreationController NodeCreation;
        public NodeOptionButtonController NodeOptionController;
        public GraphConnectionController ConnectionRenderer;
        public GraphInteractionController Interaction;
        public SkillSaveLoadController SaveLoad;

        public void Dispose()
        {
            GraphCam?.Dispose();
            Selection?.Dispose();
            GroupSelection?.Dispose();
            SettingPanel?.Dispose();
            NodeCreation?.Dispose();
            Interaction?.Dispose();
            SaveLoad?.Dispose();
        }
    }

    public class SkillTreeGraphWindow : EditorWindow
    {
        public VisualTreeAsset SkillTreeEditorTree;
        public VisualTreeAsset NodeButtonTemplateTree;

        private readonly GraphControllerContext _controllerContext = new();
        private readonly GraphContext _graphContext = new();
        private UndoManager _undoManager;

        private Button _newSkillTreeButton;
        private Button _undoButton;
        private Button _redoButton;

        private bool _isInitialized;

        [MenuItem("Tools/Skill Tree Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<SkillTreeGraphWindow>();
            window.titleContent = new GUIContent("Skill Tree Editor");
            window.minSize = new Vector2(1280, 720);
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Dispose;
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                InitializeWindow();
            }
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= Dispose;
            Dispose();
        }

        private void Update()
        {
            if (_isInitialized && _graphContext.Collection == null && !EditorApplication.isPlaying)
            {
                Dispose();
                InitializeWindow();
                return;
            }

            _controllerContext.GraphCam?.Update();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.EnteredEditMode:
                    InitializeWindow();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                case PlayModeStateChange.ExitingEditMode:
                    Dispose();
                    break;
            }
        }

        private void Dispose()
        {
            if (_isInitialized)
            {
                _controllerContext.Dispose();
                _isInitialized = false;
            }
        }

        private void InitializeWindow()
        {
            if (_isInitialized) return;
            rootVisualElement.Clear();

            if (EditorApplication.isPlaying)
            {
                ShowPlayModeMessage();
                _isInitialized = true;
                return;
            }

            try
            {
                _graphContext.Settings = Resources.Load<SkillTreeSettingData>("Skill Tree Data/SkillTreeSettingData");
                if (_graphContext.Settings == null)
                {
                    _graphContext.Settings = CreateInstance<SkillTreeSettingData>();

                    string folderPath = "Assets/Resources/Skill Tree Data";
                    if (!AssetDatabase.IsValidFolder(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        AssetDatabase.Refresh();
                    }

                    AssetDatabase.CreateAsset(_graphContext.Settings, $"{folderPath}/SkillTreeSettingData.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log("Created SkillTreeSettingData.asset in " + folderPath);
                }

                InitializeGraphElements();

                _undoManager = new UndoManager(_graphContext, _controllerContext);
                _graphContext.Collection = SkillTreeEditorUtility.CreateTransientInstance<SkillTreeCollection>();

                _controllerContext.GraphCam = new GraphCameraController(_graphContext);
                _controllerContext.SettingPanel = new SettingPanelController(_graphContext);
                _controllerContext.ConnectionRenderer = new GraphConnectionController(_graphContext);
                _controllerContext.Interaction = new GraphInteractionController(_graphContext, _controllerContext.ConnectionRenderer, _undoManager);
                _controllerContext.GroupSelection = new NodeGroupSelectionController(_graphContext, _controllerContext.Interaction);
                _controllerContext.NodeCreation = new GraphNodeCreationController(_graphContext, _controllerContext, _controllerContext.GroupSelection, _undoManager, NodeButtonTemplateTree);
                _controllerContext.Selection = new GraphSelectionController(_graphContext, _controllerContext.Interaction);
                _controllerContext.NodeOptionController = new NodeOptionButtonController(_controllerContext, _undoManager);
                _controllerContext.SaveLoad = new SkillSaveLoadController(_graphContext, _controllerContext);

                _graphContext.GridBackground.RegisterCallback<KeyDownEvent>(OnBackgroundKeyDown, TrickleDown.TrickleDown);
                _graphContext.GridBackground.RegisterCallback<KeyUpEvent>(OnBackgroundKeyUp, TrickleDown.TrickleDown);

                _undoButton = rootVisualElement.Q<Button>("undo-button");
                _undoButton.clicked += _undoManager.Undo;

                _redoButton = rootVisualElement.Q<Button>("redo-button");
                _redoButton.clicked += _undoManager.Redo;

                _newSkillTreeButton = rootVisualElement.Q<Button>("new-skill-tree-button");
                _newSkillTreeButton.clicked += InitializeNewSkillTree;

                _isInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _controllerContext.Dispose();
                rootVisualElement.Clear();
            }
        }

        private void ShowPlayModeMessage()
        {
            var container = new VisualElement();
            container.style.flexGrow = 1;
            container.style.alignItems = Align.Center;
            container.style.justifyContent = Justify.Center;

            var label = new Label("Skill Tree Editor cannot be used in Play Mode.");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.fontSize = 20;

            container.Add(label);
            rootVisualElement.Add(container);
        }

        private void OnBackgroundKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Z && evt.ctrlKey)
            {
                _undoManager.Undo();
                evt.StopPropagation();
            }

            if (evt.keyCode == KeyCode.Y && evt.ctrlKey)
            {
                _undoManager.Redo();
                evt.StopPropagation();
            }

            if (evt.keyCode == KeyCode.N && evt.ctrlKey)
            {
                InitializeNewSkillTree();
                evt.StopPropagation();
            }

            if (evt.keyCode == KeyCode.S && evt.ctrlKey && !evt.shiftKey)
            {
                if (_controllerContext.SaveLoad.TryOverwriteSaveData() == false)
                    _controllerContext.SaveLoad.SaveAsNewFile();
                evt.StopPropagation();
            }

            if (evt.keyCode == KeyCode.S && evt.ctrlKey && evt.shiftKey)
            {
                _controllerContext.SaveLoad.SaveAsNewFile();
                evt.StopPropagation();
            }

            if (evt.keyCode == KeyCode.Delete)
            {
                var nodes = _controllerContext.GroupSelection.SelectedNodes.Select(x => x.Data).ToList();
                _undoManager.ExecuteCommand(new CompositeRemoveNodeCommand(nodes));
            }

            if (evt.keyCode == KeyCode.D && evt.ctrlKey)
            {
                var nodes = _controllerContext.GroupSelection.SelectedNodes.Select(x => x.Data).ToList();
                _undoManager.ExecuteCommand(new CompositeDuplicateNodeCommand(nodes));
            }

            if (evt.keyCode == KeyCode.C)
                _controllerContext.Interaction.OnHoldConnectKeyDown(evt);
        }

        private void OnBackgroundKeyUp(KeyUpEvent evt)
        {
            if (evt.keyCode == KeyCode.C)
                _controllerContext.Interaction.OnHoldConnectKeyUp(evt);
        }

        private void InitializeGraphElements()
        {
            SkillTreeEditorTree.CloneTree(rootVisualElement);
            _graphContext.Root = rootVisualElement;
            _graphContext.GraphContent = rootVisualElement.Q<VisualElement>("skill-tree-content");

            _graphContext.GridBackground = new GridElement();
            _graphContext.GridBackground.name = "grid-background";
            _graphContext.GridBackground.focusable = true;
            _graphContext.GridBackground.style.backgroundColor = new StyleColor(new Color(0.2745098f, 0.2745098f, 0.2745098f, 1f));
            _graphContext.GridBackground.style.position = Position.Absolute;
            _graphContext.GridBackground.style.left = 0;
            _graphContext.GridBackground.style.top = 0;
            _graphContext.GridBackground.style.right = 0;
            _graphContext.GridBackground.style.bottom = 0;

            _graphContext.NodeContainer = new VisualElement();
            _graphContext.NodeContainer.name = "node-container";
            _graphContext.NodeContainer.style.position = Position.Absolute;
            _graphContext.NodeContainer.style.left = 0;
            _graphContext.NodeContainer.style.top = 0;

            _graphContext.ConnectionContainer = new VisualElement();
            _graphContext.ConnectionContainer.name = "connection-container";
            _graphContext.ConnectionContainer.pickingMode = PickingMode.Ignore;
            _graphContext.ConnectionContainer.style.position = Position.Absolute;
            _graphContext.ConnectionContainer.style.left = 0;
            _graphContext.ConnectionContainer.style.top = 0;
            _graphContext.ConnectionContainer.style.right = 0;
            _graphContext.ConnectionContainer.style.bottom = 0;

            var gridContainer = rootVisualElement.Q<VisualElement>("grid-background-container");
            gridContainer.Add(_graphContext.GridBackground);
            _graphContext.GraphContent.Add(_graphContext.ConnectionContainer);
            _graphContext.GraphContent.Add(_graphContext.NodeContainer);
        }

        private void InitializeNewSkillTree()
        {
            _graphContext.Settings.SetCurrentSkillTreeSetting("New Skill Tree", "");
            _graphContext.Collection.Clear();
            _controllerContext.ConnectionRenderer.Clear();
            _controllerContext.Selection.ClearSelection();
            _controllerContext.GroupSelection.ClearSelection();
            _controllerContext.SaveLoad.ClearSavePath();
            _undoManager.ClearHistory();
        }
    }
}
