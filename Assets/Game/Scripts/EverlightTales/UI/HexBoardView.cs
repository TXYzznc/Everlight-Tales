using System.Collections.Generic;
using Everlight.Tales.Board;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 盘面渲染视图（P1-014／P1-015）。程序化摆放蜂窝格（紧密连接的点顶小六边形）与在盘实体，
    /// 外圈描出大六边形轮廓；并驱动「六相定势」旋转的弹簧动画——旋转只改重力方向，视觉上整盘随重力归位。
    /// 视图只读模型：外部调 <see cref="Refresh"/> 同步；本类不做结算。
    /// </summary>
    public sealed class HexBoardView : MonoBehaviour
    {
        [SerializeField] private float m_CellSize = 28f;

        [SerializeField, Range(0.4f, 0.95f)] private float m_EntityScale = 0.72f;

        [SerializeField] private float m_SpringStiffness = 250f;

        [SerializeField] private float m_SpringDamping = 15f;

        [SerializeField] private Color m_CellColor = new Color(0.17f, 0.20f, 0.27f, 1f);

        [SerializeField] private Color m_CellStrokeColor = new Color(0f, 0f, 0f, 0.45f);

        [SerializeField] private Color m_OutlineColor = new Color(0.35f, 0.78f, 1f, 0.55f);

        [SerializeField] private Color m_EdgeGlowColor = new Color(0.35f, 0.78f, 1f, 1f);

        private RectTransform m_BoardRoot;

        private RectTransform m_TileRoot;

        private HexagonGraphic m_Outline;

        private HexagonGraphic m_EdgeGlow;

        private CanvasGroup m_EdgeGlowGroup;

        private readonly List<GameObject> _cells = new List<GameObject>();

        private readonly Dictionary<int, GameObject> _entityTiles = new Dictionary<int, GameObject>();

        // 弹簧旋转状态
        private float _visualAngle;

        private float _targetAngle;

        private float _springVelocity;

        private float _glowAlpha;

        private bool _rotationInitialized;

        /// <summary>当前已渲染的格子数。</summary>
        public int CellCount => _cells.Count;

        /// <summary>当前已渲染的实体数。</summary>
        public int EntityCount => _entityTiles.Count;

        /// <summary>当前视觉旋转角（度），供外部读取调试。</summary>
        public float VisualAngle => _visualAngle;

        public void Refresh(BoardState board)
        {
            if (board == null)
            {
                return;
            }

            EnsureRoots();
            ClearTiles();

            foreach (HexCoord cell in HexGrid.Enumerate(board.SideLength))
            {
                _cells.Add(CreateTile(
                    "cell_" + cell.Q + "_" + cell.R,
                    HexLayout.AxialToPixel(cell, m_CellSize),
                    m_CellSize,
                    m_CellColor,
                    m_CellStrokeColor,
                    1.2f));
            }

            foreach (BoardEntity entity in board.Entities)
            {
                GameObject tile = CreateTile(
                    "entity_" + entity.Id,
                    HexLayout.AxialToPixel(entity.Coord, m_CellSize),
                    m_CellSize * m_EntityScale,
                    EntityVisuals.GetColor(entity),
                    new Color(0f, 0f, 0f, 0.45f),
                    0f);
                _entityTiles.Add(entity.Id, tile);
            }

            RebuildOutline(board.SideLength);
            ApplyRotation();
        }

        public bool TryGetEntityTile(int id, out GameObject tile)
        {
            return _entityTiles.TryGetValue(id, out tile);
        }

        /// <summary>按当前重力方向更新视觉旋转目标（供旋转按钮／装配后调用）。</summary>
        public void SetGravity(HexDirection gravity)
        {
            float newTarget = HexLayout.RotationAngleForGravity(gravity);
            if (!_rotationInitialized)
            {
                _visualAngle = newTarget;
                _targetAngle = newTarget;
                _springVelocity = 0f;
                _rotationInitialized = true;
                ApplyRotation();
                return;
            }

            float delta = Mathf.DeltaAngle(_visualAngle, newTarget);
            _targetAngle = newTarget;

            // 反向蓄力再弹向目标，形成弹性过冲
            float kick = Mathf.Sign(delta);
            _visualAngle -= kick * 18f;
            _springVelocity -= kick * 30f;
        }

        private void Update()
        {
            if (!_rotationInitialized)
            {
                return;
            }

            float displacement = Mathf.DeltaAngle(_visualAngle, _targetAngle);
            _springVelocity += displacement * m_SpringStiffness * Time.deltaTime;
            _springVelocity *= Mathf.Exp(-m_SpringDamping * Time.deltaTime);
            _visualAngle += _springVelocity * Time.deltaTime;

            if (Mathf.Abs(displacement) < 0.5f && Mathf.Abs(_springVelocity) < 1f)
            {
                _visualAngle = _targetAngle;
                _springVelocity = 0f;
            }

            ApplyRotation();

            // 边缘流光：旋转越快越亮
            float speed = Mathf.Abs(_springVelocity);
            float targetGlow = Mathf.Clamp01(speed / 320f);
            _glowAlpha = Mathf.Lerp(_glowAlpha, targetGlow, Time.deltaTime * 14f);
            if (m_EdgeGlowGroup != null)
            {
                m_EdgeGlowGroup.alpha = _glowAlpha;
            }
        }

        private void ApplyRotation()
        {
            if (m_BoardRoot != null)
            {
                m_BoardRoot.localRotation = Quaternion.Euler(0f, 0f, _visualAngle);
            }
        }

        private GameObject CreateTile(string name, Vector2 position, float radius, Color color, Color stroke, float strokeWidth)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(m_TileRoot, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = position;
            float w = Mathf.Sqrt(3f) * radius;
            float h = 2f * radius;
            rt.sizeDelta = new Vector2(w, h);

            var graphic = go.AddComponent<HexagonGraphic>();
            graphic.Circumradius = radius;
            graphic.color = color;
            if (strokeWidth > 0f)
            {
                graphic.StrokeWidth = strokeWidth;
                graphic.StrokeColor = stroke;
            }

            return go;
        }

        private void RebuildOutline(int sideLength)
        {
            int radius = sideLength - 1;
            // 外接圆穿过最外圈角落格的外侧顶点
            float outlineRadius = m_CellSize * Mathf.Sqrt(3f) * (radius + 0.5f);

            if (m_Outline == null)
            {
                var go = new GameObject("board_outline", typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(m_BoardRoot, false);
                var rt = (RectTransform)go.transform;
                rt.anchoredPosition = Vector2.zero;
                m_Outline = go.AddComponent<HexagonGraphic>();
                m_Outline.raycastTarget = false;
            }

            float outlineW = Mathf.Sqrt(3f) * outlineRadius;
            float outlineH = 2f * outlineRadius;
            ((RectTransform)m_Outline.transform).sizeDelta = new Vector2(outlineW, outlineH);
            m_Outline.Circumradius = outlineRadius;
            m_Outline.StrokeWidth = 2.5f;
            m_Outline.color = Color.clear;
            m_Outline.StrokeColor = m_OutlineColor;

            if (m_EdgeGlow == null)
            {
                var go = new GameObject("board_edge_glow", typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(m_BoardRoot, false);
                var rt = (RectTransform)go.transform;
                rt.anchoredPosition = Vector2.zero;
                m_EdgeGlow = go.AddComponent<HexagonGraphic>();
                m_EdgeGlow.raycastTarget = false;
                m_EdgeGlowGroup = go.AddComponent<CanvasGroup>();
                m_EdgeGlowGroup.alpha = 0f;
                m_EdgeGlowGroup.interactable = false;
                m_EdgeGlowGroup.blocksRaycasts = false;
            }

            ((RectTransform)m_EdgeGlow.transform).sizeDelta = new Vector2(outlineW, outlineH);
            m_EdgeGlow.Circumradius = outlineRadius;
            m_EdgeGlow.StrokeWidth = 5f;
            m_EdgeGlow.color = Color.clear;
            m_EdgeGlow.StrokeColor = m_EdgeGlowColor;
        }

        /// <summary>
        /// 盘面格子挂在一个专用容器下，避免 <see cref="ClearTiles"/> 误删同层级的轮廓／边缘流光等兄弟节点。
        /// </summary>
        private void EnsureRoots()
        {
            if (m_BoardRoot == null)
            {
                var root = new GameObject("board_root", typeof(RectTransform));
                root.transform.SetParent(transform, false);
                m_BoardRoot = (RectTransform)root.transform;
                m_BoardRoot.anchorMin = Vector2.zero;
                m_BoardRoot.anchorMax = Vector2.one;
                m_BoardRoot.anchoredPosition = Vector2.zero;
                m_BoardRoot.sizeDelta = Vector2.zero;
            }

            if (m_TileRoot == null)
            {
                var tiles = new GameObject("board_tiles", typeof(RectTransform));
                tiles.transform.SetParent(m_BoardRoot, false);
                m_TileRoot = (RectTransform)tiles.transform;
                m_TileRoot.anchorMin = Vector2.zero;
                m_TileRoot.anchorMax = Vector2.one;
                m_TileRoot.anchoredPosition = Vector2.zero;
                m_TileRoot.sizeDelta = Vector2.zero;
            }
        }

        private void ClearTiles()
        {
            _cells.Clear();
            _entityTiles.Clear();
            if (m_TileRoot == null)
            {
                return;
            }

            for (int i = m_TileRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(m_TileRoot.GetChild(i).gameObject);
            }
        }
    }
}
