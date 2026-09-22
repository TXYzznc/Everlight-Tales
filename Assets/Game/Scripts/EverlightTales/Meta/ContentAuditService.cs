using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 内容完整性审计（P6-006/P6-007/P6-008）：文案校准 / 资源绑定 / 清单审校的完整性统计。
    /// 纯逻辑（Meta 仅引用 Data）；ART2 资源到位后重跑本服务确认绑定完成度。
    /// </summary>
    public static class ContentAuditService
    {
        // ---- 文案校准（P6-006） ----

        public static int CalibratedCopyCount()
        {
            int n = 0;
            foreach (CopyEntry e in CopyCatalog.All())
            {
                if (e.Calibrated)
                {
                    n++;
                }
            }

            return n;
        }

        public static bool AllCopyCalibrated()
        {
            return CalibratedCopyCount() == CopyCatalog.All().Count;
        }

        // ---- 资源绑定（P6-007） ----

        public static int BoundAssetCount()
        {
            int n = 0;
            foreach (AssetSlot s in AssetBindingCatalog.All())
            {
                if (s.Bound)
                {
                    n++;
                }
            }

            return n;
        }

        public static IReadOnlyList<string> UnboundAssets()
        {
            var result = new List<string>();
            foreach (AssetSlot s in AssetBindingCatalog.All())
            {
                if (!s.Bound)
                {
                    result.Add(s.Id);
                }
            }

            return result;
        }

        public static bool AllAssetsBound()
        {
            return UnboundAssets().Count == 0;
        }

        // ---- 清单审校（P6-008） ----

        public static int ReviewedChecklistCount()
        {
            int n = 0;
            foreach (ChecklistItem item in ContentChecklist.All())
            {
                if (item.Reviewed)
                {
                    n++;
                }
            }

            return n;
        }

        public static bool AllChecklistReviewed()
        {
            return ReviewedChecklistCount() == ContentChecklist.All().Count;
        }
    }
}
