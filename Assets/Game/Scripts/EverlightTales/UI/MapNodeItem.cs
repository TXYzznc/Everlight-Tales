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
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;
        [SerializeField] private Image _building, _selection;
        [SerializeField] private TMP_Text _badge;
        private bool _locked;

        public string PlaceId => _placeId;

        public void SetSelected(bool selected)
        {
            if (_building != null) _building.sprite = _spriteCatalog.Get("place:" + _placeId + ":" + (_locked ? "known_locked" : selected ? "selected" : "unlocked"));
            if (_selection != null) _selection.gameObject.SetActive(selected && _locked);
        }

        public void SetPlaceState(bool locked, int eventCount, bool actionable)
        {
            _locked = locked;
            if (_badge != null) { _badge.text = eventCount.ToString(); _badge.transform.parent.gameObject.SetActive(actionable && eventCount > 0); }
        }

        public void Bind(string label, Color color, UnityAction onClick)
        {
            if (_label != null) _label.text = label ?? string.Empty;
            if (_button == null) _button = GetComponent<Button>();
            var image = GetComponent<Image>();
            if (image != null) image.color = Color.clear;
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
