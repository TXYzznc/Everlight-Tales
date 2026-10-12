using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 盘面预制体的静态装配根。固定视觉和动态内容容器由预制体提供，
    /// <see cref="HexBoardView"/> 只负责向容器填充当前盘面数据。
    /// </summary>
    public sealed class BoardVisualRoot : MonoBehaviour
    {
        [SerializeField] private RectTransform _tileRoot;
        [SerializeField] private RectTransform _entityRoot;
        [SerializeField] private RectTransform _overlayRoot;
        [SerializeField] private RectTransform _fxRoot;
        [SerializeField] private Image _foundation;
        [SerializeField] private HexagonGraphic _outline;
        [SerializeField] private HexagonGraphic _edgeGlow;
        [SerializeField] private CanvasGroup _edgeGlowGroup;
        [SerializeField] private GameObject _entityTemplate;

        public RectTransform TileRoot => _tileRoot;
        public RectTransform EntityRoot => _entityRoot;
        public RectTransform OverlayRoot => _overlayRoot;
        public RectTransform FxRoot => _fxRoot;
        public Image Foundation => _foundation;
        public HexagonGraphic Outline => _outline;
        public HexagonGraphic EdgeGlow => _edgeGlow;
        public CanvasGroup EdgeGlowGroup => _edgeGlowGroup;
        public GameObject EntityTemplate => _entityTemplate;

        public bool IsConfigured => _tileRoot != null && _entityRoot != null
            && _overlayRoot != null && _fxRoot != null
            && _foundation != null && _outline != null && _edgeGlow != null
            && _edgeGlowGroup != null && _entityTemplate != null;
    }
}
