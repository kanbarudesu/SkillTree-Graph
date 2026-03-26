using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using SkillTreeGraph.Core;

namespace SkillTreeGraph.Editor
{
    public class SkillNodeView : VisualElement
    {
        public SkillNode Data { get; private set; }

        public Action<Vector2> OnPositionChanged;

        private readonly VisualElement _iconImage;
        private Sprite _defaultIcon;

        public SkillNodeView(SkillNode data, VisualTreeAsset nodeTemplate)
        {
            Data = data;

            var root = nodeTemplate.CloneTree().Q<VisualElement>("node-button");
            Add(root);

            style.position = Position.Absolute;

            _iconImage = this.Q<VisualElement>("button-icon");

            _iconImage.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                _defaultIcon = _iconImage.resolvedStyle.backgroundImage.sprite;
                RefreshUI();
                SetPosition(data.UiToolkitPosition);
            });
        }

        public void RefreshUI()
        {
            bool isIconNullOrEmpty = Data.Icon == null || Data.Icon == _defaultIcon;
            _iconImage.style.backgroundImage = new StyleBackground(isIconNullOrEmpty ? _defaultIcon : Data.Icon);
        }

        public void RefreshSize()
        {
            style.width = Data.NodeSize;
            style.height = Data.NodeSize;

            EditorUtility.SetDirty(Data);
        }

        public void SetPosition(Vector2 pos)
        {
            style.left = pos.x;
            style.top = pos.y;

            Data.UiToolkitPosition = pos;
            Data.CanvasPosition = new Vector2(layout.center.x, -layout.center.y);
            OnPositionChanged?.Invoke(pos);

            EditorUtility.SetDirty(Data);
        }

        public void SetSize(float nodeSize)
        {
            style.width = nodeSize;
            style.height = nodeSize;

            Data.NodeSize = nodeSize;
            EditorUtility.SetDirty(Data);
        }
    }
}