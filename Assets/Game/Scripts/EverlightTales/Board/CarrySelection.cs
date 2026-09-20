using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 准备页携带选择模型（P2-007）：可用种类（永久拥有 ∪ 现场借用，去重）、
    /// 关键件自动选入、最多 6 种（D-063）、点击补位／拖动替换／移除。
    /// 纯逻辑、零引擎。
    /// </summary>
    public sealed class CarrySelection
    {
        public const int MaxSlots = 6;

        private readonly List<PartType> _available;
        private readonly List<PartType> _keyParts;
        private readonly PartType[] _slots = new PartType[MaxSlots];

        public IReadOnlyList<PartType> Available => _available;

        public IReadOnlyList<PartType> KeyParts => _keyParts;

        public int SelectedCount { get; private set; }

        public bool IsFull => SelectedCount >= MaxSlots;

        public CarrySelection(IReadOnlyList<PartType> ownedKinds, IReadOnlyList<PartType> borrowedKinds, IReadOnlyList<PartType> keyParts)
        {
            _available = new List<PartType>();
            var seen = new HashSet<PartType>();
            AddUnique(ownedKinds, seen);
            AddUnique(borrowedKinds, seen);
            _keyParts = new List<PartType>(keyParts ?? new PartType[0]);
            AutoSelectKeys();
        }

        public PartType SlotAt(int index)
        {
            return _slots[index];
        }

        public IReadOnlyList<PartType> SelectedParts()
        {
            var result = new List<PartType>();
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] != PartType.None)
                {
                    result.Add(_slots[i]);
                }
            }

            return result;
        }

        public bool Contains(PartType part)
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] == part)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>点击补位：未满时按序号补入第一个空槽；不改变已有槽位。</summary>
        public bool Fill(PartType part)
        {
            if (!_available.Contains(part) || IsFull || Contains(part))
            {
                return false;
            }

            for (int i = 0; i < MaxSlots; i++)
            {
                if (_slots[i] == PartType.None)
                {
                    _slots[i] = part;
                    SelectedCount++;
                    return true;
                }
            }

            return false;
        }

        /// <summary>拖动替换：空槽走补位，已占用槽替换该槽种类。</summary>
        public bool Replace(int slotIndex, PartType part)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots || !_available.Contains(part))
            {
                return false;
            }

            if (_slots[slotIndex] == PartType.None)
            {
                return Fill(part);
            }

            if (Contains(part))
            {
                return false;
            }

            _slots[slotIndex] = part;
            return true;
        }

        /// <summary>移除槽位种类。</summary>
        public bool Remove(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MaxSlots || _slots[slotIndex] == PartType.None)
            {
                return false;
            }

            _slots[slotIndex] = PartType.None;
            SelectedCount--;
            return true;
        }

        /// <summary>未选入的关键件种类（用于关键件提示）。</summary>
        public IReadOnlyList<PartType> MissingKeyParts()
        {
            var result = new List<PartType>();
            foreach (PartType key in _keyParts)
            {
                if (_available.Contains(key) && !Contains(key))
                {
                    result.Add(key);
                }
            }

            return result;
        }

        private void AutoSelectKeys()
        {
            foreach (PartType key in _keyParts)
            {
                if (IsFull)
                {
                    break;
                }

                if (_available.Contains(key) && !Contains(key))
                {
                    Fill(key);
                }
            }
        }

        private void AddUnique(IReadOnlyList<PartType> kinds, HashSet<PartType> seen)
        {
            if (kinds == null)
            {
                return;
            }

            foreach (PartType part in kinds)
            {
                if (part != PartType.None && seen.Add(part))
                {
                    _available.Add(part);
                }
            }
        }
    }
}
