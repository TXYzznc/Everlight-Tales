using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    public sealed class ArchiveItem : UIItemBase
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _detail;
        [SerializeField] private TMP_Text _value;
        [SerializeField] private Image _badge;

        public void Bind(string title, string detail, string value, Sprite icon, Color textColor, bool interactable = false)
        {
            if (_icon != null && icon != null) { _icon.sprite = icon; _icon.enabled = true; }
            if (_title != null) { _title.text = title ?? string.Empty; _title.color = textColor; }
            if (_detail != null) { _detail.text = detail ?? string.Empty; _detail.color = textColor; }
            if (_value != null) { _value.text = value ?? string.Empty; _value.color = textColor; }
            if (_badge != null) _badge.enabled = false;
            Button button = GetComponent<Button>();
            if (button != null) button.interactable = interactable;
        }
    }

    public sealed class ArchiveItemObject : UIItemObject
    {
        public void Bind(string title, string detail, string value, Sprite icon, Color textColor, bool interactable = false)
        {
            itemLogic.GetComponent<ArchiveItem>().Bind(title, detail, value, icon, textColor, interactable);
        }
    }
}
