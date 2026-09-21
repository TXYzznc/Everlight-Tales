using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 点顶六边形 Graphic（可填充 + 可选描边），供盘面格、实体块与盘面外轮廓复用。
    /// 外接圆半径由 <see cref="Circumradius"/> 显式指定（六边形外接盒宽 √3·r、高 2·r），
    /// RectTransform 仅需容纳该外接盒。填充用 <see cref="Graphic.color"/>，描边用 <see cref="StrokeColor"/>。
    /// 支持「残缺墙」裁剪：<see cref="SetClip"/> 后用大六边形 SDF 的六个半平面裁切顶点，
    /// 得到被轮廓切掉的半格（uGUI 无 SpriteMask 直接裁 Graphic，改用多边形顶点裁剪等价）。
    /// </summary>
    public sealed class HexagonGraphic : MaskableGraphic
    {
        [SerializeField] private float m_Circumradius = 20f;

        [SerializeField] private float m_StrokeWidth = 0f;

        [SerializeField] private Color m_StrokeColor = new Color(0f, 0f, 0f, 0.35f);

        private bool _hasClip;

        private Vector2 _clipCenter;

        private float _clipApothem;

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

        /// <summary>
        /// 设置残缺墙裁剪：以大六边形（点顶，边心距 <paramref name="apothem"/>）裁切本六边形，
        /// 保留轮廓内部的部分。<paramref name="centerOffset"/> 是大六边形中心相对本 Graphic
        /// 中心（RectTransform 锚点）的偏移，单位与本 Graphic 本地坐标一致（像素）。
        /// 仅在填充模式下生效（不绘制描边环）。
        /// </summary>
        public void SetClip(Vector2 centerOffset, float apothem)
        {
            _clipCenter = centerOffset;
            _clipApothem = Mathf.Max(0f, apothem);
            _hasClip = true;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float r = Mathf.Max(0f, m_Circumradius);
            Vector2[] corners = BuildCorners(r);

            if (_hasClip)
            {
                corners = ClipToHex(corners);
                if (corners.Length < 3)
                {
                    return;
                }

                AddPolygonFilled(vh, corners, color);
                return;
            }

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

        /// <summary>用点顶大六边形的六个半平面裁切多边形（Sutherland-Hodgman，多边形为凸）。</summary>
        private Vector2[] ClipToHex(Vector2[] poly)
        {
            // 大六边形（点顶）六条边的外法线，逆时针 0°、60°、120°、180°、240°、300°。
            // SDF max(|x|, |0.5x+0.866y|, |0.5x-0.866y|) ≤ apothem 的三组对边法线即 0°、±60°。
            var normals = new Vector2[6]
            {
                new Vector2(1f, 0f),
                new Vector2(0.5f, 0.8660254f),
                new Vector2(-0.5f, 0.8660254f),
                new Vector2(-1f, 0f),
                new Vector2(-0.5f, -0.8660254f),
                new Vector2(0.5f, -0.8660254f),
            };

            var output = poly;
            for (int i = 0; i < normals.Length; i++)
            {
                Vector2 normal = normals[i];
                // 本 Graphic 顶点 p 相对格子中心；大六边形中心相对格子中心 = _clipCenter，
                // 故 p 相对大六边形中心 = p + _clipCenter，半平面 normal·(p + _clipCenter) ≤ apothem
                // 即 normal·p ≤ apothem - normal·_clipCenter。
                float c = _clipApothem - Vector2.Dot(normal, _clipCenter);
                output = ClipToHalfPlane(output, normal, c);
                if (output.Length < 3)
                {
                    return output;
                }
            }

            return output;
        }

        private static Vector2[] ClipToHalfPlane(Vector2[] poly, Vector2 normal, float c)
        {
            int n = poly.Length;
            if (n == 0)
            {
                return poly;
            }

            var result = new System.Collections.Generic.List<Vector2>(n + 1);
            for (int i = 0; i < n; i++)
            {
                Vector2 current = poly[i];
                Vector2 next = poly[(i + 1) % n];
                float dc = Vector2.Dot(normal, current) - c;
                float dn = Vector2.Dot(normal, next) - c;

                if (dc <= 0f)
                {
                    result.Add(current);
                }

                // 边穿越边界：dc 与 dn 异号（含恰在边界上）
                if ((dc < 0f && dn > 0f) || (dc > 0f && dn < 0f))
                {
                    float t = dc / (dc - dn);
                    result.Add(Vector2.Lerp(current, next, t));
                }
            }

            return result.ToArray();
        }

        private static void AddPolygonFilled(VertexHelper vh, Vector2[] corners, Color fill)
        {
            // 凸多边形：以首顶点为扇心三角化。
            int start = vh.currentVertCount;
            for (int i = 0; i < corners.Length; i++)
            {
                vh.AddVert(corners[i], fill, Vector2.zero);
            }

            for (int i = 1; i < corners.Length - 1; i++)
            {
                vh.AddTriangle(start, start + i, start + i + 1);
            }
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
