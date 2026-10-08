using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    public sealed class FormalPortrait : MonoBehaviour
    {
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;
        [SerializeField] private Image _image;
        private string _identity;
        public void Show(string identity)
        {
            if (_identity == identity) return;
            _identity = identity;
            _image.sprite = identity == "玩家" || identity == "沈遥" ? _spriteCatalog.Get(identity) : null;
            _image.enabled = _image.sprite != null;
        }
        private void OnDisable() { _identity = null; if (_image != null) _image.enabled = false; }
    }
}
