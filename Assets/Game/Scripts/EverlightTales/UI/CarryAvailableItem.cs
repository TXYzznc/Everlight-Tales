using System;
using Everlight.Tales.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>准备页可携带物品列表条目。</summary>
    public sealed class CarryAvailableItem : UIItemBase
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;
        private PartType _part;
        private Action<PartType> _onClick;

        public void Bind(PartType part, string label, Action<PartType> onClick)
        {
            _part = part;
            _onClick = onClick;
            if (_label != null) _label.text = label ?? string.Empty;
        }

        protected override void OnInit()
        {
            base.OnInit();
            if (_button != null) _button.onClick.AddListener(() => _onClick?.Invoke(_part));
        }
    }

    public sealed class CarryAvailableItemObject : UIItemObject
    {
        public void Bind(PartType part, string label, Action<PartType> onClick)
        {
            itemLogic.GetComponent<CarryAvailableItem>().Bind(part, label, onClick);
        }
    }
}



