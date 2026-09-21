using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>一条拥有的材料（P4-001）：异常纹样带来源怪谈，不同来源互不通用。</summary>
    public sealed class MaterialStack
    {
        public string MaterialId;
        public string SourceCase;
        public int Count;

        public string Key => string.IsNullOrEmpty(SourceCase) ? MaterialId : MaterialId + ":" + SourceCase;
    }

    /// <summary>材料背包（P4-001）：拥有与显示，纯数据容器（Add/Remove/GetCount）。</summary>
    public sealed class MaterialBackpack
    {
        private readonly List<MaterialStack> _stacks = new List<MaterialStack>();

        public IReadOnlyList<MaterialStack> Stacks => _stacks;

        public void Add(string materialId, int count, string sourceCase = "")
        {
            if (count <= 0)
            {
                return;
            }

            MaterialStack stack = Find(materialId, sourceCase);
            if (stack == null)
            {
                _stacks.Add(new MaterialStack { MaterialId = materialId, SourceCase = sourceCase ?? "", Count = count });
            }
            else
            {
                stack.Count += count;
            }
        }

        public int GetCount(string materialId, string sourceCase = "")
        {
            MaterialStack stack = Find(materialId, sourceCase);
            return stack == null ? 0 : stack.Count;
        }

        public bool Remove(string materialId, int count, string sourceCase = "")
        {
            if (count <= 0)
            {
                return false;
            }

            MaterialStack stack = Find(materialId, sourceCase);
            if (stack == null || stack.Count < count)
            {
                return false;
            }

            stack.Count -= count;
            if (stack.Count == 0)
            {
                _stacks.Remove(stack);
            }

            return true;
        }

        private MaterialStack Find(string materialId, string sourceCase)
        {
            foreach (MaterialStack s in _stacks)
            {
                if (s.MaterialId == materialId && (s.SourceCase ?? "") == (sourceCase ?? ""))
                {
                    return s;
                }
            }

            return null;
        }
    }
}
