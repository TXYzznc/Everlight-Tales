using System;
using System.Collections;
using Everlight.Tales.Data;
using Everlight.Tales.Meta.Input;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>准备页可携带物品列表条目。</summary>
    public sealed class CarryAvailableItem : UIItemBase, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
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
        private string _formId;
        private Action<PartType> _onClick;
        private Action<PartType> _onFormClick;
        private Action<PartType, string, Vector2> _onLongPress;
        private Action _onHideDetail;
        private ScrollRect _scroll;
        private RectTransform _ghost;
        private Coroutine _longPressRoutine;
        private bool _scrolling;
        private bool _selected;
        private bool _suppressClick;
        private bool _longPressTriggered;
        public PartType Part => _part;
        public bool IsDragging { get; private set; }
        public void SetKey(bool key) { if (_keyIcon != null) _keyIcon.gameObject.SetActive(key); }

        public void Bind(PartType part, string label, Action<PartType> onClick)
        {
            Bind(part, label, string.Empty, null, "基础形态", string.Empty, false, false, onClick, null, null, null, string.Empty);
        }

        public void Bind(PartType part, string title, string detail, Sprite icon, string form, string hint,
            bool mayAnomalize, bool canSwitchForm, Action<PartType> onClick, Action<PartType> onFormClick)
        {
            Bind(part, title, detail, icon, form, hint, mayAnomalize, canSwitchForm, onClick, onFormClick, null, null, string.Empty);
        }

        public void Bind(PartType part, string title, string detail, Sprite icon, string form, string hint,
            bool mayAnomalize, bool canSwitchForm, Action<PartType> onClick, Action<PartType> onFormClick,
            Action<PartType, string, Vector2> onLongPress, Action onHideDetail, string formId)
        {
            CancelDrag();
            _part = part;
            _formId = formId ?? string.Empty;
            _onClick = onClick;
            _onFormClick = onFormClick;
            _onLongPress = onLongPress;
            _onHideDetail = onHideDetail;
            _suppressClick = false;
            _label.text = title ?? string.Empty;
            _detail.text = detail ?? string.Empty;
            _detail.gameObject.SetActive(!string.IsNullOrEmpty(detail));
            _icon.sprite = icon;
            _icon.gameObject.SetActive(icon != null);
            _formLabel.text = form ?? "基础形态";
            _hint.text = hint ?? string.Empty;
            _hint.gameObject.SetActive(!string.IsNullOrEmpty(hint));
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
        public void ResetForPool()
        {
            CancelDrag();
            _onClick = null;
            _onFormClick = null;
            _onLongPress = null;
            _onHideDetail = null;
            _formId = string.Empty;
            _part = PartType.None;
            _button.interactable = false;
            SetSelectedSlot(-1);
            SetKey(false);
            _icon.sprite = null;
            _icon.gameObject.SetActive(false);
            _anomalyIcon.gameObject.SetActive(false);
            _hint.text = string.Empty;
            _hint.gameObject.SetActive(false);
        }

        public void OnInitializePotentialDrag(PointerEventData data) { if (_scroll != null) _scroll.OnInitializePotentialDrag(data); }

        public void OnPointerDown(PointerEventData data)
        {
            if (!_button.interactable || data.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            StopLongPress();
            _longPressTriggered = false;
            _longPressRoutine = StartCoroutine(WaitForLongPress(data));
        }

        public void OnPointerUp(PointerEventData data)
        {
            bool suppressClick = _longPressTriggered;
            StopLongPress();
            _onHideDetail?.Invoke();
            if (suppressClick)
            {
                _suppressClick = true;
            }
        }

        public void OnPointerExit(PointerEventData data)
        {
            bool suppressClick = _longPressTriggered;
            StopLongPress();
            _onHideDetail?.Invoke();
            if (suppressClick)
            {
                _suppressClick = true;
            }
        }

        public void OnBeginDrag(PointerEventData data)
        {
            StopLongPress();
            _onHideDetail?.Invoke();
            _scrolling = _scroll != null && Mathf.Abs(data.position.y - data.pressPosition.y) > Mathf.Abs(data.position.x - data.pressPosition.x);
            _suppressClick = true;
            if (_scrolling)
            {
                _scroll?.OnBeginDrag(data);
                return;
            }

            BeginItemDrag(data);
        }

        public void OnDrag(PointerEventData data)
        {
            if (_scrolling)
            {
                // 列表内的纵向拖动仍然滚动；一旦指针离开列表视口，就升级为
                // 跨区拖拽，确保拖向右侧卡槽时能看到跟随鼠标的零件预览。
                if (!IsPointerInsideScrollViewport(data) || IsHorizontalDrag(data))
                {
                    _scroll?.OnEndDrag(data);
                    _scrolling = false;
                    BeginItemDrag(data);
                }
                else
                {
                    _scroll?.OnDrag(data);
                    return;
                }
            }

            MoveGhost(data);
        }

        public void OnEndDrag(PointerEventData data)
        {
            if (_scrolling) _scroll?.OnEndDrag(data);
            CancelDrag();
        }

        private bool IsPointerInsideScrollViewport(PointerEventData data)
        {
            return _scroll != null && _scroll.viewport != null &&
                   RectTransformUtility.RectangleContainsScreenPoint(_scroll.viewport, data.position, data.pressEventCamera);
        }

        private bool IsHorizontalDrag(PointerEventData data)
        {
            return Mathf.Abs(data.position.x - data.pressPosition.x) > Mathf.Abs(data.position.y - data.pressPosition.y);
        }

        private void BeginItemDrag(PointerEventData data)
        {
            if (!_button.interactable)
            {
                return;
            }

            IsDragging = true;
            Canvas canvas = GetComponentInParent<Canvas>()?.rootCanvas;
            if (canvas == null || _icon == null || _icon.sprite == null)
            {
                IsDragging = false;
                return;
            }

            GameObject ghost = new GameObject("CarryDragPreview", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup), typeof(Image));
            _ghost = (RectTransform)ghost.transform;
            _ghost.SetParent(canvas.transform, false);
            _ghost.SetAsLastSibling();
            _ghost.sizeDelta = new Vector2(72f, 72f);
            Canvas ghostCanvas = ghost.GetComponent<Canvas>();
            ghostCanvas.overrideSorting = true;
            ghostCanvas.sortingLayerID = canvas.sortingLayerID;
            ghostCanvas.sortingOrder = canvas.sortingOrder + 100;
            CanvasGroup group = ghost.GetComponent<CanvasGroup>();
            group.alpha = 0.8f;
            group.interactable = false;
            group.blocksRaycasts = false;
            Image image = ghost.GetComponent<Image>();
            image.sprite = _icon.sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.raycastTarget = false;
            MoveGhost(data);
        }

        private IEnumerator WaitForLongPress(PointerEventData data)
        {
            float elapsed = 0f;
            while (elapsed < InputTiming.LongPressSeconds)
            {
                if (IsDragging || _suppressClick)
                {
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _longPressRoutine = null;
            if (!IsDragging && !_suppressClick)
            {
                _longPressTriggered = true;
                _onLongPress?.Invoke(_part, _formId, data.position);
            }
        }

        private void StopLongPress()
        {
            if (_longPressRoutine != null)
            {
                StopCoroutine(_longPressRoutine);
                _longPressRoutine = null;
            }
        }

        private void MoveGhost(PointerEventData data)
        {
            if (_ghost != null && RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_ghost.parent, data.position, data.pressEventCamera, out Vector2 local))
            {
                _ghost.localPosition = local;
            }
        }

        private void CancelDrag()
        {
            StopLongPress();
            _onHideDetail?.Invoke();
            if (_ghost != null) Destroy(_ghost.gameObject);
            _ghost = null;
            IsDragging = false;
            _scrolling = false;
            _longPressTriggered = false;
        }

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
