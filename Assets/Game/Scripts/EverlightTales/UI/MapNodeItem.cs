using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    public sealed class MapNodeItem : UIItemBase
    {
        [SerializeField] private string _placeId;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;

        public string PlaceId => _placeId;

        public void SetSelected(bool selected)
        {
            UIButtonStateUtility.SetSelected(_button != null ? _button : GetComponent<Button>(), selected);
        }

        public void Bind(string label, Color color, UnityAction onClick)
        {
            if (_label != null) _label.text = label ?? string.Empty;
            if (_button == null) _button = GetComponent<Button>();
            var image = GetComponent<Image>();
            if (image != null) image.color = color;
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                if (onClick != null) _button.onClick.AddListener(onClick);
            }
        }
    }

    public sealed class MapNodeItemObject : UIItemObject
    {
        public void Bind(string label, Color color, UnityAction onClick) => itemLogic.GetComponent<MapNodeItem>().Bind(label, color, onClick);
    }
}
