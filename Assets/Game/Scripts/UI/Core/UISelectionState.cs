using System;
using System.Collections.Generic;

namespace Everlight.Tales.UI
{
    /// <summary>列表选中项的通用状态。数据选中与详情区域应以此状态为唯一来源。</summary>
    public sealed class UISelectionState<T>
    {
        public bool HasSelection { get; private set; }
        public T Selected { get; private set; }
        public event Action<T> Changed;
        public event Action Cleared;

        public bool Select(T value)
        {
            Selected = value;
            HasSelection = true;
            Changed?.Invoke(value);
            return true;
        }

        public void Clear()
        {
            if (!HasSelection)
            {
                Selected = default;
                return;
            }

            HasSelection = false;
            Selected = default;
            Cleared?.Invoke();
        }

        public bool Restore(IReadOnlyList<T> available, Func<T, bool> isValid, bool selectFirst = true)
        {
            if (available == null || available.Count == 0)
            {
                Clear();
                return false;
            }

            if (HasSelection && isValid != null && isValid(Selected)) return true;
            if (!selectFirst)
            {
                Clear();
                return false;
            }

            Select(available[0]);
            return true;
        }
    }
}
