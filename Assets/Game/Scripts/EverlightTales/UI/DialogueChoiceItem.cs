using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>对话选项模板。对话运行时从正式 Item 资源实例化，不再拼接文本节点。</summary>
    public sealed class DialogueChoiceItem : UIItemBase
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;

        public void Bind(string label, UnityAction onClick)
        {
            if (_label != null) _label.text = label ?? string.Empty;
            if (_button == null) _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                if (onClick != null) _button.onClick.AddListener(onClick);
            }
        }
    }
}
