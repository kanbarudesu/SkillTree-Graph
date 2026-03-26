using UnityEngine;

namespace SkillTreeGraph.Editor
{
    [CreateAssetMenu(fileName = "SkillTreeSettingData", menuName = "SkillTreeGraph/SkillTreeSettingData")]
    public class SkillTreeSettingData : ScriptableObject
    {
        [Header("Skill Tree")]
        [SerializeField]
        private string skillTreeName;
        private string id;

        [Header("Node")]
        [SerializeField] private float defaultNodeSize = 50f;

        [Header("Zoom")]
        [SerializeField] private float initialZoom = 1f;
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 2.5f;
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private float zoomSmoothSpeed = 10f;

        [Header("Panning")]
        [SerializeField] private float panSmoothSpeed = 10f;
        [SerializeField] private Rect graphBounds = new Rect(-2000, -2000, 4000, 4000);

        [Header("Snap")]
        [SerializeField] private bool enableSnap = true;
        [SerializeField] private float snapSize = 50f;

        [Header("Other")]
        [SerializeField] private bool canAddSkillOnClick = true;
        [SerializeField] private bool autoSaveLoad = true;

        public string SkillTreeName => skillTreeName;
        public string Id => id;
        public float NodeSize => defaultNodeSize;

        public float InitialZoom => initialZoom;
        public float MinZoom => minZoom;
        public float MaxZoom => maxZoom;
        public float ZoomSpeed => zoomSpeed;
        public float ZoomSmoothSpeed => zoomSmoothSpeed;

        public float PanSmoothSpeed => panSmoothSpeed;
        public Rect GraphBounds => graphBounds;

        public bool EnableSnap => enableSnap;
        public float SnapSize => snapSize;

        public bool CanAddSkillOnClick => canAddSkillOnClick;
        public bool AutoSaveLoad => autoSaveLoad;

        public void SetCurrentSkillTreeSetting(string skillTreeName, string id)
        {
            this.skillTreeName = skillTreeName;
            this.id = id;
        }
    }
}