using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
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

        public InspectorPanel InspectorPanel;
        public InspectorPanel SettingPanel;

        public ToolbarButton NewTreeButton;
        public ToolbarButton SaveAsAssetButton;
        public ToolbarButton SaveButton;
        public ToolbarButton LoadButton;
        public ToolbarButton RandomTreeButton;
        public ToolbarButton UndoButton;
        public ToolbarButton RedoButton;
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
            NodeCreation?.Dispose();
            Interaction?.Dispose();
            SaveLoad?.Dispose();
        }
    }

    class PanelViewSettings
    {
        public bool IsInspectorVisible = true;
        public bool IsSettingVisible = false;
    }

    public class SkillTreeGraphWindow : EditorWindow
    {
        public VisualTreeAsset SkillTreeEditorTree;
        public VisualTreeAsset NodeButtonTemplateTree;

        private readonly GraphControllerContext _controllerContext = new();
        private readonly GraphContext _graphContext = new();
        private UndoManager _undoManager;

        private PanelViewSettings _panelViewSettings;
        private const string k_PanelViewSettings = "PanelViewSettings";

        private bool _isInitialized;

        [MenuItem("Tools/Skill Tree Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<SkillTreeGraphWindow>();
            window.titleContent = new GUIContent("Skill Tree Editor");
            window.minSize = new Vector2(640, 360);
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
                rootVisualElement.UnregisterCallback<GeometryChangedEvent>(UpdatePanelDockingLayout);
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
                var guids = AssetDatabase.FindAssets("t:SkillTreeSettingData");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _graphContext.Settings =
                        AssetDatabase.LoadAssetAtPath<SkillTreeSettingData>(path);
                }
                else
                {
                    _graphContext.Settings = CreateInstance<SkillTreeSettingData>();

                    string folderPath = "Assets/Skill Tree Graph/Settings";
                    string assetPath = folderPath + "/SkillTreeSettingData.asset";

                    SkillTreeEditorUtility.EnsureFolderExists(folderPath);

                    AssetDatabase.CreateAsset(_graphContext.Settings, assetPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                InitializeGraphElements();

                _undoManager = new UndoManager(_graphContext, _controllerContext);
                _undoManager.OnHistoryChanged += UpdateUndoRedoButtons;
                _undoManager.NotifyHistoryChanged();

                _graphContext.Collection = SkillTreeEditorUtility.CreateTransientInstance<SkillTreeCollection>();

                _controllerContext.GraphCam = new GraphCameraController(_graphContext);
                _controllerContext.SettingPanel = new SettingPanelController(_graphContext);
                _controllerContext.ConnectionRenderer = new GraphConnectionController(_graphContext);
                _controllerContext.Interaction = new GraphInteractionController(_graphContext, _controllerContext.ConnectionRenderer, _undoManager);
                _controllerContext.GroupSelection = new NodeGroupSelectionController(_graphContext, _controllerContext.Interaction);
                _controllerContext.NodeCreation = new GraphNodeCreationController(_graphContext, _controllerContext, _controllerContext.GroupSelection, _undoManager, NodeButtonTemplateTree);
                _controllerContext.Selection = new GraphSelectionController(_graphContext, _controllerContext.Interaction);
                _controllerContext.NodeOptionController = new NodeOptionButtonController(_graphContext, _controllerContext, _undoManager);
                _controllerContext.SaveLoad = new SkillSaveLoadController(_graphContext, _controllerContext);

                _graphContext.GridBackground.RegisterCallback<KeyDownEvent>(OnBackgroundKeyDown, TrickleDown.TrickleDown);
                _graphContext.GridBackground.RegisterCallback<KeyUpEvent>(OnBackgroundKeyUp, TrickleDown.TrickleDown);

                _graphContext.UndoButton.clicked += _undoManager.Undo;
                _graphContext.RedoButton.clicked += _undoManager.Redo;
                _graphContext.RandomTreeButton.clicked += _undoManager.ClearHistory;
                _graphContext.NewTreeButton.clicked += InitializeNewSkillTree;

                _isInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _controllerContext.Dispose();
                rootVisualElement.Clear();
            }
        }

        private void UpdateUndoRedoButtons(int undoCount, int redoCount)
        {
            _graphContext.UndoButton.SetEnabled(undoCount > 0);
            _graphContext.RedoButton.SetEnabled(redoCount > 0);
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

            InitializeToolbar();

            var gridContainer = rootVisualElement.Q<VisualElement>("grid-background-container");
            gridContainer.Add(_graphContext.GridBackground);
            _graphContext.GraphContent.Add(_graphContext.ConnectionContainer);
            _graphContext.GraphContent.Add(_graphContext.NodeContainer);
        }

        private void InitializeToolbar()
        {
            var serializedSettings = EditorUserSettings.GetConfigValue(k_PanelViewSettings);
            _panelViewSettings = JsonUtility.FromJson<PanelViewSettings>(serializedSettings) ?? new PanelViewSettings();

            var skillTreeRoot = rootVisualElement.Q<VisualElement>("skill-tree-root");
            _graphContext.InspectorPanel = new InspectorPanel("Graph Inspector", new Vector2(330, 370), PanelCorner.TopRight);
            _graphContext.InspectorPanel.AddToClassList("no-zoom");
            _graphContext.InspectorPanel.DeserializeLayout();
            _graphContext.InspectorPanel.style.display = _panelViewSettings.IsInspectorVisible ? DisplayStyle.Flex : DisplayStyle.None;
            skillTreeRoot.Add(_graphContext.InspectorPanel);

            _graphContext.SettingPanel = new InspectorPanel("Graph Settings", new Vector2(330, 370), PanelCorner.TopLeft);
            _graphContext.SettingPanel.AddToClassList("no-zoom");
            _graphContext.SettingPanel.DeserializeLayout();
            _graphContext.SettingPanel.style.display = _panelViewSettings.IsSettingVisible ? DisplayStyle.Flex : DisplayStyle.None;
            skillTreeRoot.Add(_graphContext.SettingPanel);

            Toolbar toolbar = new Toolbar();
            toolbar.AddToClassList("no-zoom");
            rootVisualElement.Insert(0, toolbar);

            _graphContext.NewTreeButton = new ToolbarButton() { tooltip = "Generate New Skill Tree", iconImage = EditorGUIUtility.IconContent("CreateAddNew").image as Texture2D };
            _graphContext.NewTreeButton.AddToClassList("toolbar-button");
            toolbar.Add(_graphContext.NewTreeButton);

            _graphContext.SaveAsAssetButton = new ToolbarButton() { tooltip = "Save Skill Tree As Asset", iconImage = EditorGUIUtility.IconContent("ScriptableObject Icon").image as Texture2D };
            _graphContext.SaveAsAssetButton.AddToClassList("toolbar-button");
            toolbar.Add(_graphContext.SaveAsAssetButton);

            _graphContext.SaveButton = new ToolbarButton() { tooltip = "Save Skill Tree to Json", iconImage = EditorGUIUtility.IconContent("SaveAs").image as Texture2D };
            _graphContext.SaveButton.AddToClassList("toolbar-button");
            toolbar.Add(_graphContext.SaveButton);

            _graphContext.LoadButton = new ToolbarButton() { tooltip = "Load Skill Tree from Json", iconImage = EditorGUIUtility.IconContent("FolderOpened Icon").image as Texture2D };
            _graphContext.LoadButton.AddToClassList("toolbar-button");
            toolbar.Add(_graphContext.LoadButton);

            _graphContext.RandomTreeButton = new ToolbarButton() { tooltip = "Load Random Skill Tree Example", iconImage = EditorGUIUtility.IconContent("AudioRandomContainer On Icon").image as Texture2D };
            _graphContext.RandomTreeButton.AddToClassList("toolbar-button");
            toolbar.Add(_graphContext.RandomTreeButton);

            _graphContext.UndoButton = new ToolbarButton() { tooltip = "Undo" };
            _graphContext.UndoButton.AddToClassList("undo-toolbar-button");
            var undoButtonImage = new Image();
            undoButtonImage.style.unityBackgroundImageTintColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            _graphContext.UndoButton.Add(undoButtonImage);
            toolbar.Add(_graphContext.UndoButton);

            _graphContext.RedoButton = new ToolbarButton() { tooltip = "Redo" };
            _graphContext.RedoButton.AddToClassList("redo-toolbar-button");
            var redoButtonImage = new Image();
            redoButtonImage.style.unityBackgroundImageTintColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            _graphContext.RedoButton.Add(redoButtonImage);
            toolbar.Add(_graphContext.RedoButton);

            var rightContainer = new VisualElement();
            rightContainer.style.flexDirection = FlexDirection.Row;
            rightContainer.style.marginLeft = StyleKeyword.Auto;
            toolbar.Add(rightContainer);

            var inspectorButton = new ToolbarToggle() { tooltip = "Show Graph Inspector" };
            var inspectorButtonImage = new Image();
            inspectorButton.AddToClassList("toolbar-button");
            inspectorButtonImage.style.backgroundImage = EditorGUIUtility.IconContent("UnityEditor.InspectorWindow").image as Texture2D;
            inspectorButton.Add(inspectorButtonImage);
            inspectorButton.value = _panelViewSettings.IsInspectorVisible;
            inspectorButton.RegisterValueChangedCallback(OnInspectorButtonToggled);
            rightContainer.Add(inspectorButton);

            var settingButton = new ToolbarToggle() { tooltip = "Show Graph Settings" };
            var settingButtonImage = new Image();
            settingButton.AddToClassList("toolbar-button");
            settingButtonImage.style.backgroundImage = EditorGUIUtility.IconContent("Settings Icon").image as Texture2D;
            settingButton.Add(settingButtonImage);
            settingButton.value = _panelViewSettings.IsSettingVisible;
            settingButton.RegisterValueChangedCallback(OnSettingButtonToggled);
            rightContainer.Add(settingButton);

            rootVisualElement.RegisterCallback<GeometryChangedEvent>(UpdatePanelDockingLayout);
        }

        private void OnSettingButtonToggled(ChangeEvent<bool> evt)
        {
            _graphContext.SettingPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            _graphContext.SettingPanel.DeserializeLayout();
            _panelViewSettings.IsSettingVisible = evt.newValue;
            SerializePanelViewSettings();
        }

        private void OnInspectorButtonToggled(ChangeEvent<bool> evt)
        {
            _graphContext.InspectorPanel.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
            _graphContext.InspectorPanel.DeserializeLayout();
            _panelViewSettings.IsInspectorVisible = evt.newValue;
            SerializePanelViewSettings();
        }

        private void SerializePanelViewSettings()
        {
            var serializedViewSettings = JsonUtility.ToJson(_panelViewSettings);
            EditorUserSettings.SetConfigValue(k_PanelViewSettings, serializedViewSettings);
        }

        private void UpdatePanelDockingLayout(GeometryChangedEvent evt)
        {
            _graphContext.InspectorPanel.ClampToParentLayout(rootVisualElement.layout);
            _graphContext.SettingPanel.ClampToParentLayout(rootVisualElement.layout);
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
