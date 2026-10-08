using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>图标跟随排版后的首个字符，兼容左对齐、右对齐及数值长度变化。</summary>
    public sealed class FormalCounterIcon : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _icon;

        private void OnEnable()
        {
            _label.OnPreRenderText += PositionIcon;
            _label.SetVerticesDirty();
        }

        private void OnDisable() => _label.OnPreRenderText -= PositionIcon;

        private void PositionIcon(TMP_TextInfo info)
        {
            for (int i = 0; i < info.characterCount; i++)
            {
                TMP_CharacterInfo character = info.characterInfo[i];
                if (!character.isVisible) continue;
                _icon.rectTransform.anchoredPosition = new Vector2(character.bottomLeft.x - 24,
                    (character.ascender + character.descender) * .5f);
                return;
            }
        }
    }
}
