using Everlight.Tales.Data;
using Everlight.Tales.Meta;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 怪谈页布局（P3-011 纯逻辑）：按批次分组，批次内按状态排序。
    /// 状态序：待维修/维修中 → 调查中 → 等待回访 → 已解决 → 已回访（未触发不显示）。
    /// </summary>
    public static class CasePageLayout
    {
        public static int StatusOrder(CaseStateKind kind)
        {
            switch (kind)
            {
                case CaseStateKind.AwaitingRepair: return 0;
                case CaseStateKind.Repairing: return 1;
                case CaseStateKind.Investigating: return 2;
                case CaseStateKind.AwaitingRevisit: return 3;
                case CaseStateKind.Resolved: return 4;
                case CaseStateKind.Revisited: return 5;
                default: return 6; // NotTriggered
            }
        }

        public static int Compare(CaseState a, CaseState b)
        {
            int batch = string.CompareOrdinal(a.Config.Batch, b.Config.Batch);
            if (batch != 0)
            {
                return batch;
            }

            int status = StatusOrder(a.Kind).CompareTo(StatusOrder(b.Kind));
            if (status != 0)
            {
                return status;
            }

            return string.CompareOrdinal(a.Config.Id, b.Config.Id);
        }
    }
}
