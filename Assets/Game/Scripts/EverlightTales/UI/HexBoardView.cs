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

        [SerializeField] private Color m_WallColor = new Color(0.30f, 0.22f, 0.22f, 1f);

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

        /// <summary>盘面根节点（含格／实体／轮廓／边缘流光），供特效层挂接与震动。</summary>
        public RectTransform BoardRoot => m_BoardRoot;

        /// <summary>格子像素尺寸（外接圆半径）。</summary>
        public float CellSize => m_CellSize;

        /// <summary>实体块缩放（相对格子尺寸），供预览 ghost 对齐。</summary>
        public float EntityScale => m_EntityScale;

        /// <summary>大六边形轮廓半径（像素）= 2 · (boardRadius + 1) · CellSize。</summary>
        public float OutlineRadius { get; private set; }

        /// <summary>实体块被点击（供零件卡弹窗）；参数为被点击的实体。</summary>
        public System.Action<BoardEntity> EntityClicked;

        /// <summary>正常格被点击（供机械臂搬动选择目标格）；参数为该格坐标。</summary>
        public System.Action<HexCoord> CellClicked;

        /// <summary>把盘面根本地坐标换算到本视图（页面）本地坐标，抵消盘面旋转后文字仍正向。</summary>
        public Vector2 BoardToLocal(Vector2 boardLocal)
        {
            if (m_BoardRoot == null)
            {
                return boardLocal;
            }

            return (Vector2)(m_BoardRoot.localRotation * new Vector3(boardLocal.x, boardLocal.y, 0f));
        }

        private UIFormalSpriteCatalog _spriteCatalog;
        private string _eventId;
        public Sprite GetStructureSprite(string key) => _spriteCatalog?.Get(key);
        public void Configure(UIFormalSpriteCatalog catalog, string eventId = null) { _spriteCatalog = catalog; _eventId = eventId; }

        public void Refresh(BoardState board)
        {
            if (board == null)
            {
                return;
            }

            EnsureRoots();
            ClearTiles();

            foreach (HexCoord cell in board.EnumerateNormal())
            {
                GameObject tile = CreateTile(
                    "cell_" + cell.Q + "_" + cell.R,
                    HexLayout.AxialToPixel(cell, m_CellSize),
                    m_CellSize,
                    m_CellColor,
                    m_CellStrokeColor,
                    1.2f,
                    null);
                _cells.Add(tile);

                // 异常仅铺在真实可玩格内，六边形模板裁切在实体下方。
                foreach (AnomalyRegion region in board.AnomalyRegions)
                {
                    if (!region.TryEnter(cell) || _spriteCatalog == null) continue;
                    Sprite sprite = _spriteCatalog.Get("anomaly:" + region.Type);
                    if (sprite == null) continue;
                    var maskRoot = new GameObject("AnomalyMask", typeof(RectTransform), typeof(HexagonGraphic), typeof(Mask));
                    maskRoot.transform.SetParent(tile.transform, false);
                    maskRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(m_CellSize * 2, m_CellSize * 2);
                    var maskShape = maskRoot.GetComponent<HexagonGraphic>(); maskShape.Circumradius = m_CellSize; maskShape.raycastTarget = false;
                    maskRoot.GetComponent<Mask>().showMaskGraphic = false;
                    BoardSpriteResolver.AddArt(maskRoot.transform, sprite, m_CellSize * 2);
                }

                Button button = tile.AddComponent<Button>();
                Image cellArt = tile.transform.Find("EntityArt")?.GetComponent<Image>();
                button.targetGraphic = cellArt != null ? (Graphic)cellArt : tile.GetComponent<HexagonGraphic>();
                if (cellArt != null)
                {
                    button.transition = Selectable.Transition.SpriteSwap;
                    button.spriteState = new SpriteState
                    {
                        highlightedSprite = _spriteCatalog.Get("SCR-07-01-hover"),
                        pressedSprite = _spriteCatalog.Get("SCR-07-01-selected"),
                        selectedSprite = _spriteCatalog.Get("SCR-07-01-selected"),
                        disabledSprite = _spriteCatalog.Get("SCR-07-01-normal")
                    };
                }
                HexCoord captured = cell;
                button.onClick.AddListener(() => CellClicked?.Invoke(captured));
            }

            // 残缺墙：与正常格同为点顶六边形，但被大六边形轮廓裁切成半格。
            HexBoardShape shape = board.Shape;
            foreach (HexCoord cell in board.EnumerateWall())
            {
                _cells.Add(CreateTile(
                    "wall_" + cell.Q + "_" + cell.R,
                    HexLayout.AxialToPixel(cell, m_CellSize),
                    m_CellSize,
                    m_WallColor,
                    m_CellStrokeColor,
                    1.2f,
                    shape));
            }

            foreach (BoardEntity entity in board.Entities)
            {
                GameObject tile = CreateTile(
                    "entity_" + entity.Id,
                    HexLayout.AxialToPixel(entity.Coord, m_CellSize),
                    m_CellSize * m_EntityScale,
                    EntityVisuals.GetColor(entity),
                    new Color(0f, 0f, 0f, 0.45f),
                    0f,
                    null);
                _entityTiles.Add(entity.Id, tile);
                BoardSpriteResolver.AddArt(tile.transform, BoardSpriteResolver.Resolve(_spriteCatalog, entity, _eventId),
                    2f * m_CellSize * m_EntityScale,
                    entity.ObstacleType == Everlight.Tales.Data.ObstacleType.TurningRail || entity.ObstacleType == Everlight.Tales.Data.ObstacleType.OneWayShutter ? -60f * (int)entity.PassDirection : 0f);

                Button button = tile.AddComponent<Button>();
                button.targetGraphic = tile.GetComponent<HexagonGraphic>();
                BoardEntity captured = entity;
                button.onClick.AddListener(() => EntityClicked?.Invoke(captured));
            }

            RebuildOutline(board.BoardRadius);
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
            UpdateGravityArt(gravity);
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

        private GameObject CreateTile(string name, Vector2 position, float radius, Color color, Color stroke, float strokeWidth, HexBoardShape clipShape)
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

            if (clipShape != null)
            {
                // 大六边形中心在 board_root 原点；格子中心在 board_root 下坐标为 position，
                // 故大六边形中心相对格子中心 = -position；边心距（像素）= 归一化 apothem × cellSize。
                graphic.SetClip(-position, (float)clipShape.Apothem * m_CellSize);
            }

            Sprite surface = _spriteCatalog?.Get(name.StartsWith("wall_") ? "SCR-07-02"
                : name.StartsWith("cell_") ? "SCR-07-01-normal" : null);
            if (surface != null)
            {
                // 沿用真实六边形几何作为裁切与点击面，图片只负责显示。
                var mask = go.AddComponent<Mask>();
                mask.showMaskGraphic = false;
                graphic.color = Color.white;
                graphic.StrokeWidth = 0f;
                BoardSpriteResolver.AddArt(go.transform, surface, radius * 2f);
            }

            return go;
        }

        private void RebuildOutline(int boardRadius)
        {
            // 点顶大六边形轮廓：顶点到中心 = 2 · OuterRadius · CellSize（与参考项目 circumradius 一致）。
            float outlineRadius = 2f * (boardRadius + 1) * m_CellSize;
            OutlineRadius = outlineRadius;

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
            Image frame = SetStructureArt(m_Outline.transform, "FrameArt", "SCR-07-04", outlineRadius * 2f * 1456f / 1360f);
            if (frame != null) m_Outline.StrokeWidth = 0f;
            Image foundation = SetStructureArt(m_BoardRoot, "BoardFoundation", "SCR-07-06", outlineRadius * 2f * 1500f / 1360f);
            if (foundation != null) foundation.transform.SetAsFirstSibling();

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
            Image glow = SetStructureArt(m_EdgeGlow.transform, "GlowArt", "SCR-07-05", outlineRadius * 2f * 1456f / 1360f);
            if (glow != null) { glow.color = m_EdgeGlowColor; m_EdgeGlow.StrokeWidth = 0f; }
            for (int i = 0; i < 6; i++)
            {
                Vector2 direction = HexLayout.AxialToPixel(HexDirections.Offset((HexDirection)i), 1f).normalized;
                Image seat = SetStructureArt(m_BoardRoot, "GravitySeat_" + i, "SCR-07-07-normal", m_CellSize * 2f);
                if (seat == null) continue;
                seat.rectTransform.anchoredPosition = direction * (outlineRadius + m_CellSize * 0.5f);
                seat.rectTransform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                SetStructureArt(seat.transform, "DirectionArrow", "SCR-07-09", m_CellSize);
            }
            for (int i = 0; i < 6; i++)
                if (Mathf.Abs(Mathf.DeltaAngle(_targetAngle, HexLayout.RotationAngleForGravity((HexDirection)i))) < 0.1f)
                { UpdateGravityArt((HexDirection)i); break; }
        }

        private Image SetStructureArt(Transform parent, string name, string key, float size)
        {
            Sprite sprite = _spriteCatalog?.Get(key);
            if (sprite == null) return null;
            Transform existing = parent.Find(name);
            Image image = existing != null ? existing.GetComponent<Image>() : null;
            if (image == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(parent, false);
                image = go.GetComponent<Image>();
            }
            image.sprite = sprite;
            image.color = Color.white;
            image.raycastTarget = false;
            image.preserveAspect = true;
            image.rectTransform.sizeDelta = new Vector2(size, size);
            return image;
        }

        private void UpdateGravityArt(HexDirection gravity)
        {
            if (m_BoardRoot == null || _spriteCatalog == null) return;
            for (int i = 0; i < 6; i++)
            {
                Transform seat = m_BoardRoot.Find("GravitySeat_" + i);
                if (seat == null) continue;
                bool active = i == (int)gravity;
                seat.GetComponent<Image>().sprite = _spriteCatalog.Get(active ? "SCR-07-07-active" : "SCR-07-07-normal");
                Transform arrow = seat.Find("DirectionArrow");
                if (arrow != null) arrow.gameObject.SetActive(active);
            }
        }

        public void ShowArmTargets(BoardState board, BoardEntity selected, bool enabled)
        {
            if (m_TileRoot == null || board == null) return;
            foreach (HexCoord cell in board.EnumerateNormal())
            {
                Transform tile = m_TileRoot.Find("cell_" + cell.Q + "_" + cell.R);
                if (tile == null) continue;
                Image surface = tile.Find("EntityArt")?.GetComponent<Image>();
                if (surface != null) surface.overrideSprite = enabled && selected != null && selected.Coord == cell
                    ? _spriteCatalog?.Get("SCR-07-01-selected") : null;
                bool target = enabled && selected != null && selected.IsMovable && board.EntityAt(cell) == null;
                Transform existing = tile.Find("ArmTargetArt");
                Image art = target ? SetStructureArt(tile, "ArmTargetArt", "SCR-07-03", m_CellSize * 2f) : null;
                if (art != null) art.transform.SetAsLastSibling();
                if (existing != null) existing.gameObject.SetActive(target);
            }
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
