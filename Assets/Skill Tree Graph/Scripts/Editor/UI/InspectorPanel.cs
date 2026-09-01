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
        public static readonly string ussClassName = "inspector-panel";
        public static readonly string hidePanelUssClassName = ussClassName + "__hide-panel";
        public static readonly string titleBarUssClassName = ussClassName + "__titlebar";
        public static readonly string titleLabelUssClassName = ussClassName + "__title-label";
        public static readonly string titleHideButtonUssClassName = ussClassName + "__title-hide-button";
        public static readonly string scrollViewUssClassName = ussClassName + "__scrollview";
        public static readonly string contentUssClassName = ussClassName + "__content";
        public static readonly string templateContainerUssClassName = ussClassName + "__template-container";

        const string k_StyleSheetName = "InspectorPanel";

        public static readonly Vector2 defaultSize = new Vector2(320, 500);

        static StyleSheet _cachedStyleSheet;

        readonly Label _titleLabel;
        readonly Button _hideButton;
        readonly VisualElement _templateContainer;
        readonly ScrollView _scrollView;
        readonly ResizableElement _resizableElement;

        PanelCorner _corner = PanelCorner.TopLeft;
        bool _isPanelVisible = true;

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

        public PanelCorner Corner
        {
            get => _corner;
            set
            {
                if (_corner == value)
                    return;

                _corner = value;
                ApplyCorner();
            }
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

            _templateContainer.Add(titleBar);
            _templateContainer.Add(_scrollView);
            hierarchy.Add(_templateContainer);
            hierarchy.Add(_resizableElement);

            SetSize(initialSize);
            Corner = corner;
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

        private void ApplyCorner()
        {
            style.left = StyleKeyword.Auto;
            style.right = StyleKeyword.Auto;
            style.top = StyleKeyword.Auto;
            style.bottom = StyleKeyword.Auto;

            switch (_corner)
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

        private static StyleSheet FindStyleSheet()
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
