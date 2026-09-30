using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>标题页动态存档位条目。由 SaveSlotPage 通过 UIFormBase.SpawnItem 创建。</summary>
    public sealed class SaveSlotItem : UIItemBase
    {
        [SerializeField] private TMP_Text _summary;
        [SerializeField] private TMP_Text _actionLabel;
        [SerializeField] private Button _actionButton;
        [SerializeField] private Button _deleteButton;
        private int _slot;
        private Action<int> _onAction;
        private Action<int> _onDelete;

        public void Bind(int slot, SaveSlotInfo info, Action<int> onAction, Action<int> onDelete)
        {
            _slot = slot;
            _onAction = onAction;
            _onDelete = onDelete;
            if (_summary != null) _summary.text = info != null && info.HasSave ? info.Summary : "空存档位";
            if (_actionLabel != null) _actionLabel.text = info != null && info.HasSave ? "继续" : "新档";
            if (_deleteButton != null) _deleteButton.gameObject.SetActive(info != null && info.HasSave);
            if (_actionButton != null) _actionButton.interactable = true;
        }

        protected override void OnInit()
        {
            base.OnInit();
            if (_actionButton != null) _actionButton.onClick.AddListener(HandleAction);
            if (_deleteButton != null) _deleteButton.onClick.AddListener(HandleDelete);
        }

        private void HandleAction() => _onAction?.Invoke(_slot);
        private void HandleDelete() => _onDelete?.Invoke(_slot);
    }

    public sealed class SaveSlotItemObject : UIItemObject
    {
        public void Bind(int slot, SaveSlotInfo info, Action<int> onAction, Action<int> onDelete)
        {
            itemLogic.GetComponent<SaveSlotItem>().Bind(slot, info, onAction, onDelete);
        }
    }
}
