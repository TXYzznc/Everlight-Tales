using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    public sealed class CodexItem : UIItemBase
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _stateFrame;
        [SerializeField] private TMP_Text _id;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _state;
        [SerializeField] private TMP_Text _source;
        [SerializeField] private Image _badge;

        public void Bind(string id, string title, string state, string source, Sprite icon, Sprite frame, Color color, bool interactable)
        {
            if (_icon != null && icon != null) { _icon.sprite = icon; _icon.enabled = true; }
            if (_stateFrame != null && frame != null) { _stateFrame.sprite = frame; _stateFrame.enabled = true; }
            if (_id != null) { _id.text = id ?? string.Empty; _id.color = color; }
            if (_title != null) { _title.text = title ?? string.Empty; _title.color = color; }
            if (_state != null) { _state.text = state ?? string.Empty; _state.color = color; }
            if (_source != null) { _source.text = source ?? string.Empty; _source.color = color; }
            if (_badge != null) _badge.enabled = false;
            Button button = GetComponent<Button>();
            if (button != null) button.interactable = interactable;
        }
    }

    public sealed class CodexItemObject : UIItemObject
    {
        public void Bind(string id, string title, string state, string source, Sprite icon, Sprite frame, Color color, bool interactable)
        {
            itemLogic.GetComponent<CodexItem>().Bind(id, title, state, source, icon, frame, color, interactable);
        }
    }
}
