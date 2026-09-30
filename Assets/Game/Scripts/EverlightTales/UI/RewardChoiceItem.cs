using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>结算页动态奖励选项卡。</summary>
    public sealed class RewardChoiceItem : UIItemBase
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;
        private Action _onClick;

        public void Bind(string text, Action onClick)
        {
            if (_label != null) _label.text = text ?? string.Empty;
            _onClick = onClick;
            if (_button != null) _button.interactable = true;
        }

        protected override void OnInit()
        {
            base.OnInit();
            if (_button != null) _button.onClick.AddListener(() => _onClick?.Invoke());
        }
    }

    public sealed class RewardChoiceItemObject : UIItemObject
    {
        public void Bind(string text, Action onClick)
        {
            itemLogic.GetComponent<RewardChoiceItem>().Bind(text, onClick);
        }
    }
}
