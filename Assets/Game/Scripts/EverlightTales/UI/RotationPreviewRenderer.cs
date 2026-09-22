using System.Collections.Generic;
using Everlight.Tales.Board;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 旋转预览（b42 特效）：旋转后显示每枚将移动棋子的落点 ghost（半透明实体色填充块）
    /// 与被撞棋子的橙色 hit ring（描边环）。参考 2026CIGA 的 RotationPreviewRenderer 用 SpriteRenderer
    /// + LineRenderer；本项目 uGUI 复用 <see cref="HexagonGraphic"/>。
    /// 挂 board_root 下随盘面旋转。
    /// </summary>
    public sealed class RotationPreviewRenderer : MonoBehaviour
    {
        [SerializeField] private Color m_GhostColor = new Color(0.45f, 0.82f, 1f, 0.30f);

        [SerializeField] private Color m_HitRingColor = new Color(1f, 0.52f, 0.08f, 0.60f);

        [SerializeField] private float m_HitRingStroke = 3.5f;

        private Transform m_Root;

        private float m_CellSize;

        private float m_EntityScale;

        private readonly List<GameObject> _objects = new List<GameObject>();

        /// <summary>注入容器、格子尺寸与实体缩放（与 HexBoardView 对齐）。</summary>
        public void Setup(Transform root, float cellSize, float entityScale)
        {
            m_Root = root;
            m_CellSize = cellSize;
            m_EntityScale = entityScale;
        }

        /// <summary>按当前势位刷新预览：落点 ghost + 被撞 hit ring。先清空上一次的临时对象。</summary>
        public void Refresh(IReadOnlyList<PreviewMove> moves, BoardState board, HexCoord gravityOffset)
        {
            Clear();
            if (m_Root == null || moves == null)
            {
                return;
            }

            var hitSet = new HashSet<int>();
            foreach (PreviewMove move in moves)
            {
                BoardEntity entity = board.EntityAt(move.From);
                if (entity == null)
                {
                    continue;
                }

                // 被撞目标 = 落点沿重力方向下一格被占的实体
                HexCoord beyond = new HexCoord(move.To.Q + gravityOffset.Q, move.To.R + gravityOffset.R);
                BoardEntity target = board.EntityAt(beyond);
                if (target != null)
                {
                    hitSet.Add(target.Id);
                }

                CreateGhost(entity, move.To);
            }

            foreach (int targetId in hitSet)
            {
                BoardEntity target = FindEntity(board, targetId);
                if (target != null)
                {
                    CreateHitRing(target.Coord);
                }
            }
        }

        /// <summary>销毁本次预览产生的全部临时对象。</summary>
        public void Clear()
        {
            foreach (GameObject obj in _objects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            _objects.Clear();
        }

        private static BoardEntity FindEntity(BoardState board, int id)
        {
            foreach (BoardEntity entity in board.Entities)
            {
                if (entity.Id == id)
                {
                    return entity;
                }
            }

            return null;
        }

        private void CreateGhost(BoardEntity entity, HexCoord coord)
        {
            Color baseColor = EntityVisuals.GetColor(entity);
            var go = new GameObject("preview_ghost", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(m_Root, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = HexLayout.AxialToPixel(coord, m_CellSize);
            float radius = m_CellSize * m_EntityScale;
            float w = Mathf.Sqrt(3f) * radius;
            float h = 2f * radius;
            rt.sizeDelta = new Vector2(w, h);

            var graphic = go.AddComponent<HexagonGraphic>();
            graphic.Circumradius = radius;
            graphic.color = new Color(baseColor.r, baseColor.g, baseColor.b, m_GhostColor.a);
            graphic.raycastTarget = false;

            _objects.Add(go);
        }

        private void CreateHitRing(HexCoord coord)
        {
            var go = new GameObject("preview_hit_ring", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(m_Root, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = HexLayout.AxialToPixel(coord, m_CellSize);
            float radius = m_CellSize * m_EntityScale * 1.35f;
            float w = Mathf.Sqrt(3f) * radius;
            float h = 2f * radius;
            rt.sizeDelta = new Vector2(w, h);

            var graphic = go.AddComponent<HexagonGraphic>();
            graphic.Circumradius = radius;
            graphic.color = Color.clear;
            graphic.StrokeWidth = m_HitRingStroke;
            graphic.StrokeColor = m_HitRingColor;
            graphic.raycastTarget = false;

            _objects.Add(go);
        }

        private void OnDestroy()
        {
            Clear();
        }
    }
}
