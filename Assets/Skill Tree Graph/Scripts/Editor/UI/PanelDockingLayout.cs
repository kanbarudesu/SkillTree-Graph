using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    [Serializable]
    public class PanelDockingLayout
    {
        [SerializeField] private bool dockingLeft;
        public bool DockingLeft
        {
            get => dockingLeft;
            set => dockingLeft = value;
        }

        [SerializeField] private bool dockingTop;
        public bool DockingTop
        {
            get => dockingTop;
            set => dockingTop = value;
        }

        [SerializeField] private float verticalOffset;
        public float VerticalOffset
        {
            get => verticalOffset;
            set => verticalOffset = value;
        }

        [SerializeField] private float horizontalOffset;
        public float HorizontalOffset
        {
            get => horizontalOffset;
            set => horizontalOffset = value;
        }

        [SerializeField] private Vector2 size;
        public Vector2 Size
        {
            get => size;
            set => size = value;
        }

        public void CalculateDockingCornerAndOffset(Rect layout, Rect parentLayout)
        {
            Vector2 layoutCenter = new Vector2(layout.x + layout.width * .5f, layout.y + layout.height * .5f);
            layoutCenter /= parentLayout.size;

            dockingLeft = layoutCenter.x < .5f;
            dockingTop = layoutCenter.y < .5f;

            if (dockingLeft)
                horizontalOffset = layout.x;
            else
                horizontalOffset = parentLayout.width - layout.x - layout.width;

            if (dockingTop)
                verticalOffset = layout.y;
            else
                verticalOffset = parentLayout.height - layout.y - layout.height;

            size = layout.size;
        }

        public void ClampToParentWindow()
        {
            horizontalOffset = Mathf.Max(0f, horizontalOffset);
            verticalOffset = Mathf.Max(0f, verticalOffset);
        }

        public void ApplyPosition(VisualElement target)
        {
            if (DockingLeft)
            {
                target.style.right = float.NaN;
                target.style.left = HorizontalOffset;
            }
            else
            {
                target.style.right = HorizontalOffset;
                target.style.left = float.NaN;
            }

            if (DockingTop)
            {
                target.style.bottom = float.NaN;
                target.style.top = VerticalOffset;
            }
            else
            {
                target.style.top = float.NaN;
                target.style.bottom = VerticalOffset;
            }
        }

        public void ApplySize(VisualElement target)
        {
            target.style.width = Size.x;
            target.style.height = Size.y;
        }

        public Rect GetLayout(Rect parentLayout)
        {
            Rect layout = new Rect();
            layout.size = Size;

            if (DockingLeft)
                layout.x = HorizontalOffset;
            else
                layout.x = parentLayout.width - Size.x - HorizontalOffset;

            if (DockingTop)
                layout.y = VerticalOffset;
            else
                layout.y = parentLayout.height - Size.y - VerticalOffset;

            return layout;
        }
    }
}