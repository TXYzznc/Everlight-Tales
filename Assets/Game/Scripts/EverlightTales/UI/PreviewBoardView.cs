using Everlight.Tales.Board;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 关卡预览渲染（P2-008）：在准备页预览面板里画缩小版盘面——
    /// 点顶大六边形轮廓 + 固定元素（按种类配色）+ 关键件（按零件配色），
    /// 随机件落位不进入预览。复用 <see cref="HexagonGraphic"/> 程序化绘制，
    /// 只读 <see cref="PreviewModel"/>，不参与结算。
    /// </summary>
    public sealed class PreviewBoardView : MonoBehaviour
    {
        private static readonly Color OutlineColor = new Color(0.35f, 0.78f, 1f, 0.55f);

        private static readonly Color ObstacleColor = new Color(0.30f, 0.22f, 0.22f, 1f);

        private static readonly Color TaskMarkerColor = new Color(0.95f, 0.80f, 0.20f, 1f);

        private static readonly Color EndpointColor = new Color(0.62f, 0.40f, 0.92f, 1f);

        private static readonly Color FacilityColor = new Color(0.22f, 0.24f, 0.28f, 1f);

        private static readonly Color EntityStroke = new Color(0f, 0f, 0f, 0.45f);

        private RectTransform m_Root;

        /// <summary>渲染预览（清空重建；格子尺寸按容器尺寸与盘面半径自适应）。</summary>
        public void Render(PreviewModel preview)
        {
            if (preview == null)
            {
                return;
            }

            EnsureRoot();
            ClearContent();

            // 大六边形直径 = 4 · (boardRadius + 1) · cellSize，按容器短边缩放。
            Rect rect = ((RectTransform)transform).rect;
            float maxDiameter = Mathf.Min(rect.width, rect.height) * 0.9f;
            float cellSize = maxDiameter / (4f * (preview.BoardRadius + 1));
            cellSize = Mathf.Clamp(cellSize, 4f, 20f);

            CreateOutline(2f * (preview.BoardRadius + 1) * cellSize);

            float entityRadius = cellSize * 0.72f;
            foreach (FixedElementConfig element in preview.FixedElements)
            {
                CreateHex(
                    "fixed_" + (int)element.Kind + "_" + element.Position.Q + "_" + element.Position.R,
                    HexLayout.AxialToPixel(element.Position, cellSize),
                    entityRadius,
                    GetFixedColor(element.Kind));
            }

            foreach (KeyPieceConfig key in preview.KeyPieces)
            {
                CreateHex(
                    "key_" + (int)key.PartType + "_" + key.Position.Q + "_" + key.Position.R,
                    HexLayout.AxialToPixel(key.Position, cellSize),
                    entityRadius,
                    EntityVisuals.GetPartColor(key.PartType));
            }
        }

        private void EnsureRoot()
        {
            if (m_Root != null)
            {
                return;
            }

            var go = new GameObject("preview_root", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            m_Root = (RectTransform)go.transform;
            m_Root.anchorMin = new Vector2(0.5f, 0.5f);
            m_Root.anchorMax = new Vector2(0.5f, 0.5f);
            m_Root.pivot = new Vector2(0.5f, 0.5f);
            m_Root.anchoredPosition = Vector2.zero;
            m_Root.sizeDelta = Vector2.zero;
        }

        private void ClearContent()
        {
            for (int i = m_Root.childCount - 1; i >= 0; i--)
            {
                Destroy(m_Root.GetChild(i).gameObject);
            }
        }

        private void CreateOutline(float radius)
        {
            var go = new GameObject("preview_outline", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(m_Root, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(Mathf.Sqrt(3f) * radius, 2f * radius);

            var graphic = go.AddComponent<HexagonGraphic>();
            graphic.raycastTarget = false;
            graphic.Circumradius = radius;
            graphic.color = Color.clear;
            graphic.StrokeWidth = 2.5f;
            graphic.StrokeColor = OutlineColor;
        }

        private void CreateHex(string name, Vector2 position, float radius, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(m_Root, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            rt.sizeDelta = new Vector2(Mathf.Sqrt(3f) * radius, 2f * radius);

            var graphic = go.AddComponent<HexagonGraphic>();
            graphic.raycastTarget = false;
            graphic.Circumradius = radius;
            graphic.color = color;
            graphic.StrokeWidth = 1f;
            graphic.StrokeColor = EntityStroke;
        }

        private static Color GetFixedColor(FixedElementKind kind)
        {
            switch (kind)
            {
                case FixedElementKind.Obstacle:
                    return ObstacleColor;
                case FixedElementKind.TaskMarker:
                case FixedElementKind.MovableMarker:
                    return TaskMarkerColor;
                case FixedElementKind.EndpointLabel:
                    return EndpointColor;
                case FixedElementKind.Facility:
                default:
                    return FacilityColor;
            }
        }
    }
}
