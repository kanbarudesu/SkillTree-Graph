using UnityEditor;

namespace SkillTreeGraph.Editor
{
    public class SettingPanelController
    {
        private readonly InspectorPanel _settingPanel;
        private readonly SkillTreeSettingData _settings;

        private SerializedObject _serializedObject;

        public SettingPanelController(GraphContext context)
        {
            _settings = context.Settings;
            _settingPanel = context.SettingPanel;

            Initialize();
        }

        private void Initialize()
        {
            if (_settingPanel == null)
                return;

            _serializedObject = new SerializedObject(_settings);
            SkillTreeEditorUtility.BuildInspectorElement(_settingPanel.contentContainer, _serializedObject);
        }
    }
}