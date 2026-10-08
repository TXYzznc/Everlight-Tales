using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    public sealed class CarrySlotVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;
        [SerializeField] private Image _background, _object, _key;
        private bool _filled, _hovered;
        public void Bind(Sprite sprite, bool filled, bool key)
        {
            _filled = filled; _object.sprite = sprite; _object.gameObject.SetActive(filled && sprite != null);
            _key.gameObject.SetActive(filled && key); Refresh();
        }
        public void OnPointerEnter(PointerEventData data) { _hovered = true; Refresh(); }
        public void OnPointerExit(PointerEventData data) { _hovered = false; Refresh(); }
        private void OnDisable() { _hovered = false; }
        private void Refresh()
        {
            _background.overrideSprite = null;
            _background.sprite = _spriteCatalog.Get(_hovered ? "SHR-004-highlight" : _filled ? "SHR-004-filled" : "SHR-004-empty");
            _background.color = Color.white;
        }
    }
}
