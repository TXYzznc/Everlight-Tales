using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 点顶六边形 Graphic（可填充 + 可选描边），供盘面格、实体块与盘面外轮廓复用。
    /// 外接圆半径由 <see cref="Circumradius"/> 显式指定（六边形外接盒宽 √3·r、高 2·r），
    /// RectTransform 仅需容纳该外接盒。填充用 <see cref="Graphic.color"/>，描边用 <see cref="StrokeColor"/>。
    /// </summary>
    public sealed class HexagonGraphic : MaskableGraphic
    {
        [SerializeField] private float m_Circumradius = 20f;

        [SerializeField] private float m_StrokeWidth = 0f;

        [SerializeField] private Color m_StrokeColor = new Color(0f, 0f, 0f, 0.35f);

        /// <summary>外接圆半径（中心到顶点的像素距离）。</summary>
        public float Circumradius
        {
            get => m_Circumradius;
            set
            {
                m_Circumradius = value;
                SetVerticesDirty();
            }
        }

        /// <summary>描边宽度（0 表示仅填充）。</summary>
        public float StrokeWidth
        {
            get => m_StrokeWidth;
            set
            {
                m_StrokeWidth = value;
                SetVerticesDirty();
            }
        }

        /// <summary>描边颜色（仅描边，不影响填充）。</summary>
        public Color StrokeColor
        {
            get => m_StrokeColor;
            set
            {
                m_StrokeColor = value;
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float r = Mathf.Max(0f, m_Circumradius);
            var corners = BuildCorners(r);

            if (m_StrokeWidth <= 0f || r <= 0f)
            {
                AddFilled(vh, corners, color);
                return;
            }

            // 填充 + 描边环（外 6 顶点 + 内 6 顶点组成 6 个四边形）
            AddFilled(vh, corners, color);

            float inner = Mathf.Max(0f, r - m_StrokeWidth);
            var innerCorners = BuildCorners(inner);

            int outer = vh.currentVertCount;
            for (int i = 0; i < 6; i++)
            {
                vh.AddVert(corners[i], m_StrokeColor, Vector2.zero);
            }

            int innerStart = vh.currentVertCount;
            for (int i = 0; i < 6; i++)
            {
                vh.AddVert(innerCorners[i], m_StrokeColor, Vector2.zero);
            }

            for (int i = 0; i < 6; i++)
            {
                int o0 = outer + i;
                int o1 = outer + (i + 1) % 6;
                int i0 = innerStart + i;
                int i1 = innerStart + (i + 1) % 6;
                vh.AddTriangle(o0, o1, i1);
                vh.AddTriangle(o0, i1, i0);
            }
        }

        /// <summary>点顶六边形六个顶点，从 30°（右上）起逆时针。</summary>
        private static Vector2[] BuildCorners(float r)
        {
            var corners = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float a = (30f + 60f * i) * Mathf.Deg2Rad;
                corners[i] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
            }

            return corners;
        }

        private static void AddFilled(VertexHelper vh, Vector2[] corners, Color fill)
        {
            int center = vh.currentVertCount;
            vh.AddVert(Vector3.zero, fill, Vector2.zero);
            for (int i = 0; i < 6; i++)
            {
                vh.AddVert(corners[i], fill, Vector2.zero);
            }

            for (int i = 0; i < 6; i++)
            {
                vh.AddTriangle(center, center + 1 + i, center + 1 + (i + 1) % 6);
            }
        }
    }
}
