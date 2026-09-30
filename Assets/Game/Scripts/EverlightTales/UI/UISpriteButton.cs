using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    public enum UISpriteButtonRole
    {
        Main,
        Secondary,
        Small,
        Danger,
        Icon,
        Tab,
        Filter,
        Card,
        ListRow,
        Hex,
    }

    /// <summary>按钮的正式美术状态。按钮不再通过颜色表达 Normal/Highlighted/Pressed。</summary>
    [DisallowMultipleComponent]
    public sealed class UISpriteButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image _image;
        [SerializeField] private Button _button;
        [SerializeField] private Sprite _normal;
        [SerializeField] private Sprite _highlighted;
        [SerializeField] private Sprite _pressed;
        [SerializeField] private Sprite _disabled;
        [SerializeField] private Sprite _selected;

        private bool _selectedState;
        private bool _pointerInside;
        private bool _pointerDown;

        public void Configure(Image image, Button button, Sprite normal, Sprite highlighted, Sprite pressed, Sprite disabled, Sprite selected)
        {
            _image = image != null ? image : GetComponent<Image>();
            _button = button != null ? button : GetComponent<Button>();
            _normal = normal;
            _highlighted = highlighted;
            _pressed = pressed;
            _disabled = disabled;
            _selected = selected;
            if (_button != null)
            {
                _button.targetGraphic = _image;
                _button.transition = Selectable.Transition.SpriteSwap;
                _button.spriteState = new SpriteState
                {
                    highlightedSprite = _highlighted,
                    pressedSprite = _pressed,
                    selectedSprite = _selected,
                    disabledSprite = _disabled
                };
            }
            ApplyVisual();
        }

        public void SetSelected(bool selected)
        {
            _selectedState = selected;
            ApplyVisual();
        }

        public static UISpriteButton Attach(Button button)
        {
            if (button == null) return null;
            UISpriteButton state = button.GetComponent<UISpriteButton>();
            if (state == null) state = button.gameObject.AddComponent<UISpriteButton>();
            if (state._image == null) state._image = button.GetComponent<Image>();
            state._button = button;
            return state;
        }

        /// <summary>为运行时动态创建的按钮接入同一套正式 Sprite Swap 资源。</summary>
        public static UISpriteButton ConfigureButton(Button button, UISpriteButtonRole role)
        {
            UISpriteButton state = Attach(button);
            if (state != null) state.ConfigureRole(role);
            return state;
        }

        public void ConfigureRole(UISpriteButtonRole role)
        {
            UIFormalButtonLibrary library = Resources.Load<UIFormalButtonLibrary>("UI/FormalButtonLibrary");
            if (library == null) return;
            library.Configure(ToLibraryRole(role), this);
        }

        private static UIFormalButtonLibrary.ButtonRole ToLibraryRole(UISpriteButtonRole role)
        {
            switch (role)
            {
                case UISpriteButtonRole.Main: return UIFormalButtonLibrary.ButtonRole.Main;
                case UISpriteButtonRole.Small: return UIFormalButtonLibrary.ButtonRole.Small;
                case UISpriteButtonRole.Danger: return UIFormalButtonLibrary.ButtonRole.Danger;
                case UISpriteButtonRole.Tab: return UIFormalButtonLibrary.ButtonRole.Tab;
                case UISpriteButtonRole.Filter: return UIFormalButtonLibrary.ButtonRole.Filter;
                default: return UIFormalButtonLibrary.ButtonRole.Secondary;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) { _pointerInside = true; ApplyVisual(); }
        public void OnPointerExit(PointerEventData eventData) { _pointerInside = false; _pointerDown = false; ApplyVisual(); }
        public void OnPointerDown(PointerEventData eventData) { _pointerDown = true; ApplyVisual(); }
        public void OnPointerUp(PointerEventData eventData) { _pointerDown = false; ApplyVisual(); }

        private void Awake()
        {
            if (_image == null) _image = GetComponent<Image>();
            if (_button == null) _button = GetComponent<Button>();
            if (_button != null) _button.transition = Selectable.Transition.SpriteSwap;
        }

        private void LateUpdate()
        {
            // Unity Button 的 SpriteSwap 会在 pointer exit 后恢复 normal；
            // 选中页签需要在该时机保持 active Sprite。
            if (_selectedState && !_pointerInside && !_pointerDown) ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (_image == null) return;
            if (_pointerDown && _pressed != null) { _image.sprite = _pressed; return; }
            if (_pointerInside && _highlighted != null) { _image.sprite = _highlighted; return; }
            if (_selectedState && _selected != null) { _image.sprite = _selected; return; }
            if (_button != null && !_button.interactable && _disabled != null) { _image.sprite = _disabled; return; }
            if (_normal != null) _image.sprite = _normal;
        }
    }
}
