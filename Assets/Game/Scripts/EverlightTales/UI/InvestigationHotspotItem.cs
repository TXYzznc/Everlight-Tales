using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>现场调查可点击热点模板。</summary>
    public sealed class InvestigationHotspotItem : UIItemBase
    {
        [SerializeField] private Image _ring;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;

        public void Bind(string label, UnityAction onClick)
        {
            if (_label != null) _label.text = label ?? string.Empty;
            if (_ring == null) _ring = GetComponent<Image>();
            if (_button == null) _button = GetComponent<Button>();
            if (_ring != null) _ring.color = new Color(0.66f, 0.42f, 0.78f, 0.35f);
            if (_button != null) { _button.onClick.RemoveAllListeners(); if (onClick != null) _button.onClick.AddListener(onClick); }
        }

        public void Confirm(string label)
        {
            if (_ring != null) _ring.color = new Color(0.88f, 0.66f, 0.35f, 0.55f);
            if (_label != null) _label.text = (label ?? string.Empty) + " ✓";
        }
    }
}
