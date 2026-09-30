using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>来客区当天到店委托条目。</summary>
    public sealed class GuestDelegationItem : UIItemBase
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _buttonLabel;

        public void Bind(string title, UnityAction onClick)
        {
            if (_title != null) _title.text = title ?? string.Empty;
            if (_button == null) _button = GetComponent<Button>();
            if (_buttonLabel != null) _buttonLabel.text = "去处理";
            if (_button != null) { _button.onClick.RemoveAllListeners(); if (onClick != null) _button.onClick.AddListener(onClick); }
        }
    }

    public sealed class GuestDelegationItemObject : UIItemObject
    {
        public void Bind(string title, UnityAction onClick) => itemLogic.GetComponent<GuestDelegationItem>().Bind(title, onClick);
    }
}
