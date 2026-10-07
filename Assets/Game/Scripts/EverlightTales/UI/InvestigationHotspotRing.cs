using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>未交付专用圆环图片时的无纹理热点轮廓，可在预制体中换成 Image。</summary>
    public sealed class InvestigationHotspotRing : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();
            Rect rect = rectTransform.rect;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            const int segments = 48;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                helper.AddVert(rect.center + direction * radius, color, Vector2.zero);
                helper.AddVert(rect.center + direction * Mathf.Max(0, radius - 4f), color, Vector2.zero);
                if (i > 0) { int n = i * 2; helper.AddTriangle(n - 2, n, n - 1); helper.AddTriangle(n, n + 1, n - 1); }
            }
        }
    }
}
