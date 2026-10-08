using UnityEngine;

namespace Everlight.Tales.UI
{
    [CreateAssetMenu(menuName = "Everlight Tales/UI/Formal Button Library", fileName = "FormalButtonLibrary")]
    public sealed class UIFormalButtonLibrary : ScriptableObject
    {
        [SerializeField] private Sprite _mainNormal, _mainHighlighted, _mainPressed, _mainDisabled, _mainSelected;
        [SerializeField] private Sprite _secondaryNormal, _secondaryHighlighted, _secondaryPressed, _secondaryDisabled, _secondarySelected;
        [SerializeField] private Sprite _smallNormal, _smallHighlighted, _smallPressed, _smallDisabled, _smallSelected;
        [SerializeField] private Sprite _dangerNormal, _dangerHighlighted, _dangerPressed, _dangerDisabled, _dangerSelected;
        [SerializeField] private Sprite _tabNormal, _tabHighlighted, _tabPressed, _tabDisabled, _tabSelected;
        [SerializeField] private Sprite _filterNormal, _filterHighlighted, _filterPressed, _filterDisabled, _filterSelected;

        [SerializeField] private UIFormalSpriteCatalog _catalog;
        public enum ButtonRole { Main, Secondary, Small, Danger, Tab, Filter, Icon, AutoFill }
        public void ApplyRewardAction(UnityEngine.UI.Button button)
        {
            if (button == null || _catalog == null || button.image == null) return;
            Sprite normal = _catalog.Get("SHR-023-gold-normal");
            Sprite hover = _catalog.Get("SHR-023-gold-hover");
            Sprite pressed = _catalog.Get("SHR-023-gold-pressed");
            Sprite disabled = _catalog.Get("SHR-023-gold-disabled");
            if (normal == null || hover == null || pressed == null || disabled == null)
            {
                Debug.LogError("[UIFormalButtonLibrary][RewardAction] 金色小按钮四态资源不完整。", this);
                Apply(button, ButtonRole.Small);
                return;
            }
            button.image.overrideSprite = null;
            button.image.sprite = normal;
            button.image.color = Color.white;
            button.image.type = UnityEngine.UI.Image.Type.Sliced;
            button.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;
            button.spriteState = new UnityEngine.UI.SpriteState
            {
                highlightedSprite = hover, pressedSprite = pressed,
                selectedSprite = normal, disabledSprite = disabled
            };
        }

        public void Apply(UnityEngine.UI.Button button, ButtonRole role, int tabCount = 3)
        {
            if (button == null || _catalog == null || button.image == null) return;
            string family = role == ButtonRole.Main ? "SHR-021" : role == ButtonRole.Small ? "SHR-023" : role == ButtonRole.Danger ? "SHR-024"
                : role == ButtonRole.Icon ? "SHR-025" : role == ButtonRole.Tab ? (tabCount == 4 ? "SHR-027" : tabCount == 5 ? "SHR-028" : "SHR-026")
                : role == ButtonRole.Filter ? "SHR-029" : role == ButtonRole.AutoFill ? "SHR-030" : "SHR-022";
            bool active = role == ButtonRole.Tab || role == ButtonRole.Filter;
            button.image.sprite = _catalog.Get(family + "-normal"); button.image.color = Color.white; button.image.type = UnityEngine.UI.Image.Type.Sliced;
            button.transition = UnityEngine.UI.Selectable.Transition.SpriteSwap;
            button.spriteState = new UnityEngine.UI.SpriteState { highlightedSprite = _catalog.Get(family + "-hover"), pressedSprite = _catalog.Get(family + (active ? "-active" : "-pressed")),
                selectedSprite = _catalog.Get(family + (active ? "-active" : "-normal")), disabledSprite = _catalog.Get(family + (active ? "-normal" : "-disabled")) };
        }
    }
}
