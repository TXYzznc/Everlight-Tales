using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    [Flags]
    public enum ListRowFields
    {
        None = 0, Id = 1, Icon = 2, Detail = 4, RightText = 8, StateFrame = 16, Action = 32,
        Status = 64,
        All = Id | Icon | Detail | RightText | StateFrame | Action | Status
    }

    public struct ListRowData
    {
        public string Title, Detail, Id, RightText, ActionText;
        public Sprite Icon, StateFrame;
        public Color TextColor;
        public string StatusText;
        public Sprite StatusIcon;
        public Color StatusColor;
        public ListRowFields Fields;
        public bool Interactable, Selected, ActionInteractable;
        public UnityAction OnClick, OnAction;

        public ListRowData(string title)
        {
            Title = title; Detail = Id = RightText = ActionText = null;
            Icon = StateFrame = null; TextColor = Color.white; Fields = ListRowFields.All;
            Interactable = ActionInteractable = true; Selected = false;
            OnClick = OnAction = null;
            StatusText = null; StatusIcon = null; StatusColor = Color.white;
        }
    }

    /// <summary>只显示页面注入的数据；不包含业务判断、跳转或默认详情行为。</summary>
    public sealed class ListRowItem : UIItemBase
    {
        [SerializeField] private Button _button;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TMP_Text _title, _detail, _id, _rightText, _actionText;
        [SerializeField] private Image _icon, _stateFrame;
        [SerializeField] private RectTransform _iconRoot;
        [SerializeField] private LayoutElement _height;
        [SerializeField] private RectTransform _rect;
        [SerializeField] private RectTransform _statusGroup;
        [SerializeField] private Image _statusIcon;
        [SerializeField] private TMP_Text _statusText;

        private bool _selected;
        public Button Button => _button;

        private void OnEnable()
        {
            // uGUI Button 的 OnEnable 会恢复瞬时状态，随后重应用业务选中图片。
            if (_button != null) UIButtonStateUtility.SetSelected(_button, _selected);
        }

        public void Bind(ListRowData data)
        {
            if (!HasContract()) return;
            SetText(_title, data.Title, data.TextColor, true);
            bool detail = Has(data.Fields, ListRowFields.Detail) && !string.IsNullOrEmpty(data.Detail);
            SetText(_detail, data.Detail, data.TextColor, detail);
            SetText(_id, data.Id, data.TextColor, Has(data.Fields, ListRowFields.Id) && !string.IsNullOrEmpty(data.Id));
            SetText(_rightText, data.RightText, data.TextColor, Has(data.Fields, ListRowFields.RightText) && !string.IsNullOrEmpty(data.RightText));
            bool icon = Has(data.Fields, ListRowFields.Icon) && data.Icon != null;
            bool frame = Has(data.Fields, ListRowFields.StateFrame) && data.StateFrame != null;
            _icon.sprite = icon ? data.Icon : null;
            _icon.color = Color.white;
            _icon.enabled = icon;
            _stateFrame.sprite = frame ? data.StateFrame : null;
            _stateFrame.enabled = frame;
            _iconRoot.gameObject.SetActive(icon || frame);
            bool status = Has(data.Fields, ListRowFields.Status) && !string.IsNullOrEmpty(data.StatusText);
            _statusGroup.gameObject.SetActive(status);
            SetText(_statusText, data.StatusText, data.StatusColor, status);
            _statusIcon.sprite = status ? data.StatusIcon : null;
            _statusIcon.color = Color.white;
            _statusIcon.gameObject.SetActive(status && data.StatusIcon != null);
            bool action = Has(data.Fields, ListRowFields.Action) && !string.IsNullOrEmpty(data.ActionText);
            _actionText.text = action ? data.ActionText : string.Empty;
            _actionText.color = data.TextColor;
            _actionButton.gameObject.SetActive(action);
            _button.onClick.RemoveAllListeners();
            _button.interactable = data.Interactable && data.OnClick != null;
            if (_button.interactable) _button.onClick.AddListener(data.OnClick);
            _actionButton.onClick.RemoveAllListeners();
            _actionButton.interactable = action && data.ActionInteractable && data.OnAction != null;
            if (_actionButton.interactable) _actionButton.onClick.AddListener(data.OnAction);
            float rowHeight = detail ? 96f : 64f;
            _height.minHeight = _height.preferredHeight = rowHeight;
            _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowHeight);
            // 对外部临时焦点和旧租用的视觉状态作完整重置。
            Image background = _button.targetGraphic as Image;
            if (background != null) background.overrideSprite = null;
            _selected = data.Selected;
            UIButtonStateUtility.SetSelected(_button, _selected);
            LayoutRebuilder.MarkLayoutForRebuild(_rect);
        }

        public void ResetForPool() => Bind(new ListRowData(string.Empty) { Interactable = false, Fields = ListRowFields.None });

        private bool HasContract()
        {
            bool valid = _button != null && _actionButton != null && _title != null && _detail != null
                && _id != null && _rightText != null && _actionText != null && _icon != null
                && _stateFrame != null && _iconRoot != null && _height != null && _rect != null
                && _statusGroup != null && _statusIcon != null && _statusText != null;
            if (!valid) Debug.LogError("[ListRowItem][Contract] 缺少序列化显示引用。", this);
            return valid;
        }

        private static bool Has(ListRowFields fields, ListRowFields field) => (fields & field) != 0;
        private static void SetText(TMP_Text text, string value, Color color, bool visible)
        {
            text.text = visible ? (value ?? string.Empty).Replace('\n', ' ') : string.Empty;
            text.color = color;
            text.gameObject.SetActive(visible);
        }
    }

    public sealed class ListRowItemObject : UIItemObject
    {
        private ListRowItem _view;
        public ulong Lease { get; private set; }
        public bool IsSpawned { get; private set; }
        public ListRowItem View => _view;
        public void Bind(ListRowData data) => _view.Bind(data);

        protected override void OnInit()
        {
            base.OnInit();
            _view = gameObject.GetComponent<ListRowItem>();
            if (_view == null) throw new InvalidOperationException("ListRowItem 模板缺少通用显示组件。");
            AcquireLease();
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            AcquireLease();
        }

        private void AcquireLease() { Lease++; IsSpawned = true; }
        protected override void OnUnspawn()
        {
            IsSpawned = false;
            if (_view != null) _view.ResetForPool();
            base.OnUnspawn();
        }

        protected override void Release(bool isShutdown)
        {
            IsSpawned = false;
            if (_view != null) _view.ResetForPool();
            base.Release(isShutdown);
        }
    }
}
