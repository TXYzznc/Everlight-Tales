using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>现场调查可点击热点模板。</summary>
    public sealed class InvestigationHotspotItem : UIItemBase
    {
        [SerializeField] private Graphic _ring;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;
        [SerializeField] private Image _confirmedIcon;
        private UnityAction _onClick;
        private string _name;
        public bool IsConfirmed { get; private set; }
        // 颜色由规范图片提供，保持白色乘色以便直接替换正式美术资源。
        private static readonly Color IdleColor = Color.white;
        private static readonly Color ConfirmedColor = Color.white;

        protected override void OnInit()
        {
            base.OnInit();
            _button.onClick.AddListener(OnClicked);
        }

        public void Bind(string label, UnityAction onClick)
        {
            _name = label ?? string.Empty; _onClick = onClick; IsConfirmed = false;
            _label.text = _name;
            _ring.color = IdleColor;
            _confirmedIcon.gameObject.SetActive(false);
            _button.interactable = onClick != null;
            _button.image.color = IdleColor;
            _button.image.overrideSprite = null;
            UIButtonStateUtility.SetSelected(_button, false);
        }

        private void OnClicked()
        {
            if (IsConfirmed || !_button.interactable) return;
            UnityAction callback = _onClick;
            Confirm(_name);
            _onClick = null;
            callback?.Invoke();
        }

        public void Confirm(string label)
        {
            IsConfirmed = true;
            _ring.color = ConfirmedColor;
            _button.image.color = ConfirmedColor;
            _label.text = label ?? string.Empty;
            _confirmedIcon.gameObject.SetActive(true);
            _button.interactable = false;
        }
        public void ResetForPool() { _onClick = null; _name = string.Empty; IsConfirmed = false; _button.interactable = false; }
        private void Update()
        {
            if (!IsConfirmed && _button.interactable)
            {
                Color pulse = IdleColor; pulse.a = 0.9f + 0.1f * Mathf.Sin(Time.unscaledTime * (Mathf.PI * 2f / 1.6f));
                _ring.color = pulse;
            }
        }
    }

    public sealed class InvestigationHotspotItemObject : UIItemObject
    {
        public InvestigationHotspotItem View => itemLogic.GetComponent<InvestigationHotspotItem>();
        public void Bind(string label, UnityAction onClick) => itemLogic.GetComponent<InvestigationHotspotItem>().Bind(label, onClick);
        public void Confirm(string label) => itemLogic.GetComponent<InvestigationHotspotItem>().Confirm(label);
        protected override void OnUnspawn() { View.ResetForPool(); base.OnUnspawn(); }
        protected override void Release(bool isShutdown) { if (itemLogic != null) View.ResetForPool(); base.Release(isShutdown); }
    }
}
