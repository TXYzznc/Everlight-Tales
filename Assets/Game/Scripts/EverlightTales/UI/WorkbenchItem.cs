using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    public sealed class WorkbenchItem : UIItemBase
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _detail;
        [SerializeField] private TMP_Text _value;
        [SerializeField] private Button _action;

        public void Bind(string title, string detail, string value, Sprite icon, Sprite background, Color color, bool interactable = false)
        {
            if (_background != null && background != null) _background.sprite = background;
            if (_icon != null) { _icon.sprite = icon; _icon.enabled = icon != null; }
            if (_title != null) { _title.text = title ?? string.Empty; _title.color = color; }
            if (_detail != null) { _detail.text = detail ?? string.Empty; _detail.color = color; }
            if (_value != null) { _value.text = value ?? string.Empty; _value.color = color; }
            if (_action != null) _action.interactable = interactable;
        }
    }

    public sealed class WorkbenchItemObject : UIItemObject
    {
        public void Bind(string title, string detail, string value, Sprite icon, Sprite background, Color color, bool interactable = false)
        {
            itemLogic.GetComponent<WorkbenchItem>().Bind(title, detail, value, icon, background, color, interactable);
        }
    }
}
