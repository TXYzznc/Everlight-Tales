using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>任务、事件、怪谈列表的统一条目模板；详情与操作回调由页面注入。</summary>
    public sealed class JournalItem : UIItemBase
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _detail;
        [SerializeField] private TMP_Text _actionLabel;
        [SerializeField] private Button _button;

        public void Bind(string title, string detail, Color color, UnityAction onClick = null, string actionLabel = null)
        {
            if (_title != null) { _title.text = title ?? string.Empty; _title.color = color; }
            if (_detail != null)
            {
                _detail.text = (detail ?? string.Empty).Replace('\n', ' ');
                _detail.enableWordWrapping = false;
                _detail.overflowMode = TextOverflowModes.Ellipsis;
            }
            if (_title != null)
            {
                _title.enableWordWrapping = false;
                _title.overflowMode = TextOverflowModes.Ellipsis;
            }
            if (_actionLabel != null) { _actionLabel.text = actionLabel ?? string.Empty; _actionLabel.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(actionLabel)); }
            if (_button == null) _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                if (onClick != null) _button.onClick.AddListener(onClick);
            }
        }
    }

    public sealed class JournalItemObject : UIItemObject
    {
        public void Bind(string title, string detail, Color color, UnityAction onClick = null, string actionLabel = null)
        {
            itemLogic.GetComponent<JournalItem>().Bind(title, detail, color, onClick, actionLabel);
        }
    }
}
