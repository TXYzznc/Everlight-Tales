using System;
using Everlight.Tales.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>准备页可携带物品列表条目。</summary>
    public sealed class CarryAvailableItem : UIItemBase, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _detail;
        [SerializeField] private TMP_Text _formLabel;
        [SerializeField] private TMP_Text _hint;
        [SerializeField] private TMP_Text _selectedLabel;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _anomalyIcon;
        [SerializeField] private Image _keyIcon;
        [SerializeField] private Button _formButton;
        [SerializeField] private LayoutElement _height;
        private PartType _part;
        private Action<PartType> _onClick;
        private Action<PartType> _onFormClick;
        private ScrollRect _scroll;
        private RectTransform _ghost;
        private bool _scrolling;
        private bool _selected;
        private bool _suppressClick;
        public PartType Part => _part;
        public bool IsDragging { get; private set; }
        public void SetKey(bool key) { if (_keyIcon != null) _keyIcon.gameObject.SetActive(key); }

        public void Bind(PartType part, string label, Action<PartType> onClick)
        {
            Bind(part, label, string.Empty, null, "基础形态", string.Empty, false, false, onClick, null);
        }

        public void Bind(PartType part, string title, string detail, Sprite icon, string form, string hint,
            bool mayAnomalize, bool canSwitchForm, Action<PartType> onClick, Action<PartType> onFormClick)
        {
            CancelDrag();
            _part = part; _onClick = onClick; _onFormClick = onFormClick; _suppressClick = false;
            _label.text = title ?? string.Empty;
            _detail.text = detail ?? string.Empty; _detail.gameObject.SetActive(!string.IsNullOrEmpty(detail));
            _icon.sprite = icon; _icon.gameObject.SetActive(icon != null);
            _formLabel.text = form ?? "基础形态";
            _hint.text = hint ?? string.Empty; _hint.gameObject.SetActive(!string.IsNullOrEmpty(hint));
            _anomalyIcon.gameObject.SetActive(mayAnomalize);
            _height.minHeight = _height.preferredHeight = string.IsNullOrEmpty(hint) ? 144f : 172f;
            _formButton.interactable = canSwitchForm && onFormClick != null;
            _button.interactable = part != PartType.None && onClick != null;
            _scroll = GetComponentInParent<ScrollRect>();
            SetSelectedSlot(-1);
        }

        public void SetSelectedSlot(int slot)
        {
            _selected = slot >= 0;
            _selectedLabel.text = _selected ? "槽" + (slot + 1) : string.Empty;
            _selectedLabel.gameObject.SetActive(_selected);
            UIButtonStateUtility.SetSelected(_button, _selected);
        }

        protected override void OnInit()
        {
            base.OnInit();
            _button.onClick.AddListener(() => { if (!_suppressClick && _button.interactable) _onClick?.Invoke(_part); });
            _formButton.onClick.AddListener(() => { if (_formButton.interactable) _onFormClick?.Invoke(_part); });
        }

        private void OnEnable() { if (_button != null) UIButtonStateUtility.SetSelected(_button, _selected); }
        private void OnDisable() => CancelDrag();
        public void ResetForPool() { CancelDrag(); _onClick = null; _onFormClick = null; _part = PartType.None; _button.interactable = false; SetSelectedSlot(-1); SetKey(false); _icon.sprite = null; _icon.gameObject.SetActive(false); _anomalyIcon.gameObject.SetActive(false); _hint.text = string.Empty; _hint.gameObject.SetActive(false); }
        public void OnInitializePotentialDrag(PointerEventData data) { if (_scroll != null) _scroll.OnInitializePotentialDrag(data); }
        public void OnBeginDrag(PointerEventData data)
        {
            _scrolling = _scroll != null && Mathf.Abs(data.position.y - data.pressPosition.y) > Mathf.Abs(data.position.x - data.pressPosition.x);
            _suppressClick = true;
            if (_scrolling) { _scroll.OnBeginDrag(data); return; }
            if (!_button.interactable) return;
            IsDragging = true;
            Canvas canvas = GetComponentInParent<Canvas>().rootCanvas;
            GameObject ghost = new GameObject("CarryDragPreview", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            _ghost = (RectTransform)ghost.transform; _ghost.SetParent(canvas.transform, false); _ghost.sizeDelta = new Vector2(280f, 64f);
            ghost.GetComponent<CanvasGroup>().blocksRaycasts = false;
            Image image = ghost.GetComponent<Image>(); image.sprite = _button.image.sprite; image.type = Image.Type.Sliced; image.raycastTarget = false;
            GameObject title = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)); title.transform.SetParent(_ghost, false);
            RectTransform rt = (RectTransform)title.transform; rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = new Vector2(12, 0); rt.offsetMax = new Vector2(-12, 0);
            TMP_Text text = title.GetComponent<TMP_Text>(); text.font = _label.font; text.fontSize = 22; text.text = _label.text; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            MoveGhost(data);
            Image art = BoardSpriteResolver.AddArt(_ghost, _icon.sprite, 48);
            if (art != null) art.rectTransform.anchoredPosition = new Vector2(-90, 0);
        }
        public void OnDrag(PointerEventData data) { if (_scrolling) _scroll.OnDrag(data); else MoveGhost(data); }
        public void OnEndDrag(PointerEventData data) { if (_scrolling) _scroll.OnEndDrag(data); CancelDrag(); }
        private void MoveGhost(PointerEventData data)
        {
            if (_ghost != null && RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_ghost.parent, data.position, data.pressEventCamera, out Vector2 local)) _ghost.localPosition = local;
        }
        private void CancelDrag() { if (_ghost != null) Destroy(_ghost.gameObject); _ghost = null; IsDragging = false; _scrolling = false; }
        private void LateUpdate() { if (!IsDragging && !_scrolling) _suppressClick = false; }
    }

    public sealed class CarryAvailableItemObject : UIItemObject
    {
        public CarryAvailableItem View => itemLogic.GetComponent<CarryAvailableItem>();
        public void Bind(PartType part, string label, Action<PartType> onClick)
        {
            itemLogic.GetComponent<CarryAvailableItem>().Bind(part, label, onClick);
        }
        protected override void OnUnspawn() { View.ResetForPool(); base.OnUnspawn(); }
        protected override void Release(bool isShutdown) { if (itemLogic != null) View.ResetForPool(); base.Release(isShutdown); }
    }
}

