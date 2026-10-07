using System;
using Everlight.Tales.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Everlight.Tales.UI
{
    public sealed class CarrySlotDropTarget : MonoBehaviour, IDropHandler
    {
        private int _slot;
        private Action<int, PartType> _onDrop;
        public void Bind(int slot, Action<int, PartType> onDrop) { _slot = slot; _onDrop = onDrop; }
        public void OnDrop(PointerEventData data)
        {
            CarryAvailableItem item = data.pointerDrag != null ? data.pointerDrag.GetComponent<CarryAvailableItem>() : null;
            if (item != null && item.IsDragging) _onDrop?.Invoke(_slot, item.Part);
        }
        public void ResetForPool() => _onDrop = null;
    }
}
