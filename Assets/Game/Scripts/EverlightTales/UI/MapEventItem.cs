using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>地图地点详情中的重复事件卡。页面只负责提供数据和点击回调。</summary>
    public sealed class MapEventItem : UIItemBase
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _meta;
        [SerializeField] private Button _button;

        public void Bind(string title, string meta, UnityAction onClick)
        {
            if (_title != null) _title.text = title ?? string.Empty;
            if (_meta != null) _meta.text = meta ?? string.Empty;
            if (_button == null) _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                if (onClick != null) _button.onClick.AddListener(onClick);
            }
        }
    }

    public sealed class MapEventItemObject : UIItemObject
    {
        public void Bind(string title, string meta, UnityAction onClick)
        {
            itemLogic.GetComponent<MapEventItem>().Bind(title, meta, onClick);
        }
    }
}
