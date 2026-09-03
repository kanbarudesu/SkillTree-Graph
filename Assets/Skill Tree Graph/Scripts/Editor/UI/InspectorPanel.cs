using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public enum PanelCorner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    [UxmlElement]
    public partial class InspectorPanel : VisualElement
    {
        const string ussClassName = "inspector-panel";
        const string hidePanelUssClassName = ussClassName + "__hide-panel";
        const string titleBarUssClassName = ussClassName + "__titlebar";
        const string titleLabelUssClassName = ussClassName + "__title-label";
        const string titleHideButtonUssClassName = ussClassName + "__title-hide-button";
        const string scrollViewUssClassName = ussClassName + "__scrollview";
        const string contentUssClassName = ussClassName + "__content";
        const string templateContainerUssClassName = ussClassName + "__template-container";
        const string k_StyleSheetName = "InspectorPanel";

        public static readonly Vector2 defaultSize = new Vector2(320, 500);

        static StyleSheet _cachedStyleSheet;

        readonly Label _titleLabel;
        readonly Button _hideButton;
        readonly VisualElement _templateContainer;
        readonly ScrollView _scrollView;
        readonly ResizableElement _resizableElement;

        bool _isPanelVisible = true;

        protected PanelDockingLayout panelDockingLayout { get; private set; } = new PanelDockingLayout
        {
            DockingTop = true,
            DockingLeft = false,
            VerticalOffset = 8,
            HorizontalOffset = 8,
        };

        [UxmlAttribute]
        public string title
        {
            get => _titleLabel.text;
            set => _titleLabel.text = value;
        }

        public Vector2 InitialSize
        {
            get => new Vector2(resolvedStyle.width, resolvedStyle.height);
            set => SetSize(value);
        }

        public override VisualElement contentContainer => _scrollView.contentContainer;

        public InspectorPanel() : this("Graph Inspector", defaultSize) { }

        public InspectorPanel(string titleText) : this(titleText, defaultSize) { }

        public InspectorPanel(string titleText, Vector2 initialSize, PanelCorner corner = PanelCorner.TopRight)
        {
            AddToClassList(ussClassName);

            var styleSheet = FindStyleSheet();
            if (styleSheet != null)
                styleSheets.Add(styleSheet);
            else
                Debug.LogWarning($"{nameof(InspectorPanel)}: could not find a StyleSheet named " +
                                 $"'{k_StyleSheetName}.uss' anywhere in the project. Make sure it's imported.");

            this.AddManipulator(new Dragger() { clampToParentEdges = true });

            _templateContainer = new VisualElement { name = "TemplateContainer" };
            _templateContainer.AddToClassList(templateContainerUssClassName);

            var titleBar = new VisualElement { name = "titlebar" };
            titleBar.AddToClassList(titleBarUssClassName);

            _titleLabel = new Label(titleText) { name = "title-label" };
            _titleLabel.AddToClassList(titleLabelUssClassName);
            titleBar.Add(_titleLabel);

            _hideButton = new Button { name = "hide-button" };
            _hideButton.text = "-";
            _hideButton.AddToClassList(titleHideButtonUssClassName);
            _hideButton.clicked += () => TogglePanelDisplay(_isPanelVisible = !_isPanelVisible);
            titleBar.Add(_hideButton);

            _scrollView = new ScrollView(ScrollViewMode.Vertical) { name = "scrollview" };
            _scrollView.AddToClassList(scrollViewUssClassName);
            _scrollView.contentContainer.AddToClassList(contentUssClassName);

            _resizableElement = new ResizableElement { name = "resize-handles" };
            _resizableElement.RegisterCallback<GeometryChangedEvent>((_) => SerializeLayout());

            _templateContainer.Add(titleBar);
            _templateContainer.Add(_scrollView);
            hierarchy.Add(_templateContainer);
            hierarchy.Add(_resizableElement);

            SetSize(initialSize);
            InitializePanelCorner(corner);
            RegisterCallback<MouseUpEvent>(OnMoveEnd);
        }

        public void TogglePanelDisplay(bool isShowing)
        {
            _isPanelVisible = isShowing;
            if (isShowing)
            {
                RemoveFromClassList(hidePanelUssClassName);
                _scrollView.style.display = DisplayStyle.Flex;
            }
            else
            {
                AddToClassList(hidePanelUssClassName);
                _scrollView.style.display = DisplayStyle.None;
            }
        }

        public void ClampToParentLayout(Rect parentLayout)
        {
            panelDockingLayout.CalculateDockingCornerAndOffset(layout, parentLayout);
            panelDockingLayout.ClampToParentWindow();

            // If the parent window is being resized smaller than this window on either axis
            if (parentLayout.width < this.layout.width || parentLayout.height < this.layout.height)
            {
                // Don't adjust the sub window in this case as it causes flickering errors and looks broken
            }
            else
            {
                panelDockingLayout.ApplyPosition(this);
            }

            SerializeLayout();
        }

        public void DeserializeLayout()
        {
            var serializedLayout = EditorUserSettings.GetConfigValue(_titleLabel.text);
            if (!string.IsNullOrEmpty(serializedLayout))
                panelDockingLayout = JsonUtility.FromJson<PanelDockingLayout>(serializedLayout);
            else
            {
                panelDockingLayout.Size = defaultSize;
                return;
            }

            panelDockingLayout.ApplySize(this);
            panelDockingLayout.ApplyPosition(this);
        }

        private void SerializeLayout()
        {
            if (style.display == DisplayStyle.None) return;

            panelDockingLayout.Size = layout.size;
            var serializedLayout = JsonUtility.ToJson(panelDockingLayout);
            EditorUserSettings.SetConfigValue(_titleLabel.text, serializedLayout);
        }

        private void OnMoveEnd(MouseUpEvent evt)
        {
            panelDockingLayout.CalculateDockingCornerAndOffset(layout, parent.layout);
            panelDockingLayout.ClampToParentWindow();

            SerializeLayout();
        }

        private void InitializePanelCorner(PanelCorner corner)
        {
            style.left = StyleKeyword.Auto;
            style.right = StyleKeyword.Auto;
            style.top = StyleKeyword.Auto;
            style.bottom = StyleKeyword.Auto;

            switch (corner)
            {
                case PanelCorner.TopLeft:
                    style.left = 0;
                    style.top = 0;
                    break;

                case PanelCorner.TopRight:
                    style.right = 0;
                    style.top = 0;
                    break;

                case PanelCorner.BottomLeft:
                    style.left = 0;
                    style.bottom = 0;
                    break;

                case PanelCorner.BottomRight:
                    style.right = 0;
                    style.bottom = 0;
                    break;
            }
        }

        private void SetSize(Vector2 size)
        {
            size.x = Mathf.Max(1f, size.x);
            size.y = Mathf.Max(1f, size.y);

            style.width = size.x;
            style.height = size.y;
        }

        private StyleSheet FindStyleSheet()
        {
            if (_cachedStyleSheet != null)
                return _cachedStyleSheet;

            var guids = AssetDatabase.FindAssets($"{k_StyleSheetName} t:StyleSheet");
            if (guids.Length == 0)
                return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            return _cachedStyleSheet;
        }
    }
}
