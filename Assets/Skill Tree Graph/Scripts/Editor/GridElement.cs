using UnityEngine;
using UnityEngine.UIElements;

namespace SkillTreeGraph.Editor
{
    public class GridElement : VisualElement
    {
        public float GridSize = 50f;
        public float Zoom = 1f;
        public Vector2 PanOffset;

        public GridElement()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;

            painter.lineWidth = 1;

            float scaledGrid = GridSize * Zoom;

            float width = layout.width;
            float height = layout.height;

            float offsetX = PanOffset.x % scaledGrid;
            float offsetY = PanOffset.y % scaledGrid;

            painter.strokeColor = new Color(0.4078432f, 0.4078432f, 0.4078432f);

            // Vertical lines
            for (float x = -scaledGrid; x < width + scaledGrid; x += scaledGrid)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(x + offsetX, 0));
                painter.LineTo(new Vector2(x + offsetX, height));
                painter.Stroke();
            }

            // Horizontal lines
            for (float y = -scaledGrid; y < height + scaledGrid; y += scaledGrid)
            {
                painter.BeginPath();
                painter.MoveTo(new Vector2(0, y + offsetY));
                painter.LineTo(new Vector2(width, y + offsetY));
                painter.Stroke();
            }
        }
    }
}
