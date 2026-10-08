using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    public sealed class FormalNavigationIcon : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TMPro.TMP_Text _label;
        private void LateUpdate()
        {
            bool selected = _button.image != null && _button.spriteState.selectedSprite != null
                && _button.spriteState.selectedSprite != _button.image.sprite
                && _button.image.overrideSprite == _button.spriteState.selectedSprite;
            Color tint = !_button.interactable ? new Color32(110,118,132,255) : selected ? new Color32(23,26,32,255) : new Color32(232,234,240,255);
            if (_icon != null) _icon.color = tint;
            if (_label != null) _label.color = tint;
        }
    }
}
