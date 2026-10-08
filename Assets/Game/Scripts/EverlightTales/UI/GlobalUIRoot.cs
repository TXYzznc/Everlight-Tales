using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    public enum ToastKind { Info, Success, Warning, Danger }

    /// <summary>全局加载遮罩与非阻塞提示。生命周期和计时不受游戏暂停影响。</summary>
    public sealed class GlobalUIRoot : MonoBehaviour
    {
        private sealed class Notice
        {
            public string Text;
            public ToastKind Kind;
            public int Count = 1;
            public float LastAdded, ShownAt, Expires;
            public RectTransform Rect;
            public TMP_Text Label;
            public float MeasuredWidth = -1, Height = 80;
            public CanvasGroup Group;
        }
        private readonly List<Notice> _visible = new List<Notice>(3);
        private readonly List<Notice> _queue = new List<Notice>(10);
        private UIFormalSpriteCatalog _catalog;
        private TMP_FontAsset _font;
        private RectTransform _canvas, _content;
        private GameObject _loading;
        private Image _loadingArt;
        private TMP_Text _loadingLabel;
        private int _loadingRefs;
        private float _bottomInset = 200f;
        public int VisibleCount => _visible.Count;
        public int QueuedCount => _queue.Count;

        private void Awake()
        {
            var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 500;
            // 与项目根 Canvas 同一缩放基准；这是独立持久化全局宿主。
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2340); scaler.matchWidthOrHeight = 0.5f;
            _canvas = (RectTransform)go.transform;
            _content = Rect("ToastSafeArea", _canvas, Vector2.zero); Stretch(_content);
            _content.gameObject.AddComponent<SafeAreaFitter>();
            _loading = Rect("Loading", _canvas, Vector2.zero).gameObject; Stretch((RectTransform)_loading.transform);
            Image mask = _loading.AddComponent<Image>(); mask.color = new Color(0, 0, 0, .55f); mask.raycastTarget = true;
            _loadingArt = Picture("LoadingArt", _loading.transform, new Vector2(192, 192));
            _loadingArt.rectTransform.anchoredPosition = new Vector2(0, 24);
            _loadingLabel = Label("Label", _loading.transform, new Vector2(400, 48));
            _loadingLabel.rectTransform.anchoredPosition = new Vector2(0, -112); _loadingLabel.text = "加载中…";
            _loading.SetActive(false);
        }
        public void Configure(UIFormalSpriteCatalog catalog, TMP_FontAsset font)
        {
            _catalog = catalog; _font = font;
            _loadingArt.sprite = catalog != null ? catalog.Get("SHR-042") : null;
            _loadingArt.enabled = _loadingArt.sprite != null;
            _loadingLabel.font = font != null ? font : TMP_Settings.defaultFontAsset;
            foreach (Notice n in _visible) Paint(n);
        }
        public void SetBottomInset(float inset) => _bottomInset = Mathf.Max(0, inset);
        public void ShowLoading() { _loadingRefs++; _loading.SetActive(true); }
        public void HideLoading() { _loadingRefs = Mathf.Max(0, _loadingRefs - 1); _loading.SetActive(_loadingRefs > 0); }
        public void ShowToast(string message, ToastKind kind)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            float now = Time.unscaledTime;
            foreach (Notice n in _visible) if (Merge(n, message, kind, now)) return;
            foreach (Notice n in _queue) if (Merge(n, message, kind, now)) return;
            var notice = new Notice { Text = message, Kind = kind, LastAdded = now };
            if (_visible.Count < 3) { Show(notice); return; }
            if (_queue.Count == 10)
            {
                int discard = _queue.FindIndex(n => n.Kind == ToastKind.Info || n.Kind == ToastKind.Success);
                if (discard < 0 && (kind == ToastKind.Warning || kind == ToastKind.Danger)) discard = _queue.FindIndex(n => n.Kind == ToastKind.Warning);
                if (discard < 0 && kind == ToastKind.Danger) discard = 0;
                if (discard < 0) return;
                _queue.RemoveAt(discard);
            }
            _queue.Add(notice);
        }
        private bool Merge(Notice n, string text, ToastKind kind, float now)
        {
            if (n.Text != text || n.Kind != kind || now - n.LastAdded > 1f) return false;
            n.Count++; n.LastAdded = now;
            if (n.Rect != null) { n.Expires = now + Duration(kind); Paint(n); }
            return true;
        }
        private static float Duration(ToastKind kind) => kind == ToastKind.Warning || kind == ToastKind.Danger ? 4f : 2.5f;
        private void Show(Notice n)
        {
            n.Rect = Rect("Toast_" + n.Kind, _content, new Vector2(960, 80));
            n.Rect.anchorMin = n.Rect.anchorMax = new Vector2(.5f, 0); n.Rect.pivot = new Vector2(.5f, 0);
            n.Rect.gameObject.AddComponent<Image>().raycastTarget = false;
            n.Group = n.Rect.gameObject.AddComponent<CanvasGroup>(); n.Group.blocksRaycasts = false; n.Group.interactable = false;
            n.Label = Label("Text", n.Rect, Vector2.zero); Stretch(n.Label.rectTransform);
            n.Label.rectTransform.offsetMin = new Vector2(80, 12); n.Label.rectTransform.offsetMax = new Vector2(-32, -12);
            n.Label.fontSize = 26; n.Label.enableWordWrapping = true; n.Label.maxVisibleLines = 2; n.Label.overflowMode = TextOverflowModes.Ellipsis;
            n.ShownAt = Time.unscaledTime; n.Expires = n.ShownAt + Duration(n.Kind);
            _visible.Add(n); Paint(n);
        }
        private void Paint(Notice n)
        {
            n.Label.font = _font != null ? _font : TMP_Settings.defaultFontAsset;
            n.MeasuredWidth = -1;
            n.Label.text = n.Text + (n.Count > 1 ? " ×" + n.Count : "");
            Image background = n.Rect.GetComponent<Image>();
            background.sprite = _catalog != null ? _catalog.Get("SHR-041-" + n.Kind.ToString().ToLowerInvariant()) : null;
            background.type = Image.Type.Sliced; background.color = background.sprite != null ? Color.white : new Color(.1f, .12f, .16f, .96f);
        }
        private void Update()
        {
            float now = Time.unscaledTime;
            for (int i = _visible.Count - 1; i >= 0; i--)
            {
                Notice n = _visible[i];
                if (now >= n.Expires) { Destroy(n.Rect.gameObject); _visible.RemoveAt(i); }
            }
            while (_visible.Count < 3 && _queue.Count > 0) { Notice next = _queue[0]; _queue.RemoveAt(0); Show(next); }
            float y = _bottomInset + 24;
            float width = Mathf.Min(960, Mathf.Max(160, _content.rect.width - 96));
            foreach (Notice n in _visible)
            {
                if (!Mathf.Approximately(n.MeasuredWidth, width)) { n.Height = n.Label.GetPreferredValues(n.Label.text, width - 112, 1000).y > 40 ? 112 : 80; n.MeasuredWidth = width; }
                float height = n.Height;
                n.Rect.sizeDelta = new Vector2(width, height); n.Rect.anchoredPosition = new Vector2(0, y); y += height + 12;
                n.Group.alpha = Mathf.Min(Mathf.Clamp01((now - n.ShownAt) / .15f), Mathf.Clamp01((n.Expires - now) / .15f));
            }
        }
        private static RectTransform Rect(string name, Transform parent, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform; rect.sizeDelta = size; return rect;
        }
        private static Image Picture(string name, Transform parent, Vector2 size)
        {
            Image image = Rect(name, parent, size).gameObject.AddComponent<Image>(); image.preserveAspect = true; image.raycastTarget = false; return image;
        }
        private static TMP_Text Label(string name, Transform parent, Vector2 size)
        {
            var label = Rect(name, parent, size).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset; label.fontSize = 28; label.color = Color.white; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false; return label;
        }
        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
    }
}
