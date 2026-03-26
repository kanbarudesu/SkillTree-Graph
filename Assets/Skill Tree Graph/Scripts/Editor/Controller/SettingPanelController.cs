using UnityEditor;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class SettingPanelController
    {
        private readonly VisualElement _root;
        private readonly VisualElement _settingPanel;
        private readonly VisualElement _settingButton;
        private readonly SkillTreeSettingData _settings;

        private SerializedObject _serializedObject;

        public SettingPanelController(GraphContext context)
        {
            _root = context.Root;
            _settings = context.Settings;

            _settingPanel = _root.Q<VisualElement>("setting-panel");
            _settingButton = _root.Q<VisualElement>("setting-button");

            Initialize();
        }

        private void Initialize()
        {
            if (_settingPanel == null || _settingButton == null)
                return;

            _serializedObject = new SerializedObject(_settings);

            BuildInspector();

            _settingButton.RegisterCallback<ClickEvent>(OnSettingButtonClicked);
        }

        private void BuildInspector()
        {
            var inspectorElement = new ScrollView();
            inspectorElement.mode = ScrollViewMode.Vertical;
            inspectorElement.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            SkillTreeEditorUtility.BuildInspectorElement(inspectorElement, _serializedObject, "Skill Tree Setting");

            _settingPanel.Add(inspectorElement);
        }

        private void OnSettingButtonClicked(ClickEvent evt)
        {
            if (_settingPanel.ClassListContains("panel-exit"))
                _settingPanel.RemoveFromClassList("panel-exit");
            else
                _settingPanel.AddToClassList("panel-exit");
        }

        public void Dispose()
        {
            _settingButton.UnregisterCallback<ClickEvent>(OnSettingButtonClicked);
        }
    }
}