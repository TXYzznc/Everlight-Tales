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

        public void Configure(ButtonRole role, UISpriteButton button)
        {
            if (button == null) return;
            switch (role)
            {
                case ButtonRole.Main: button.Configure(null, null, _mainNormal, _mainHighlighted, _mainPressed, _mainDisabled, _mainSelected); break;
                case ButtonRole.Small: button.Configure(null, null, _smallNormal, _smallHighlighted, _smallPressed, _smallDisabled, _smallSelected); break;
                case ButtonRole.Danger: button.Configure(null, null, _dangerNormal, _dangerHighlighted, _dangerPressed, _dangerDisabled, _dangerSelected); break;
                case ButtonRole.Tab: button.Configure(null, null, _tabNormal, _tabHighlighted, _tabPressed, _tabDisabled, _tabSelected); break;
                case ButtonRole.Filter: button.Configure(null, null, _filterNormal, _filterHighlighted, _filterPressed, _filterDisabled, _filterSelected); break;
                default: button.Configure(null, null, _secondaryNormal, _secondaryHighlighted, _secondaryPressed, _secondaryDisabled, _secondarySelected); break;
            }
        }

        public enum ButtonRole { Main, Secondary, Small, Danger, Tab, Filter }
    }
}
