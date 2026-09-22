using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 校准口径服务（P6-001/P6-002）：校验校准项是否落在口径区间内，产出越界清单与报告。
    /// 纯逻辑（Meta 仅引用 Data）；试玩前固化口径，正式调值后重跑本服务确认口径。
    /// </summary>
    public static class CalibrationService
    {
        /// <summary>越界校准项（值低于下界或高于上界）。</summary>
        public static IReadOnlyList<CalibrationBound> OutOfRange()
        {
            var result = new List<CalibrationBound>();
            foreach (CalibrationBound b in CalibrationCatalog.All())
            {
                if (!b.InRange)
                {
                    result.Add(b);
                }
            }

            return result;
        }

        /// <summary>全部校准项是否都在口径区间内。</summary>
        public static bool ValidateAll()
        {
            return OutOfRange().Count == 0;
        }

        /// <summary>按类别统计校准项数与越界数。</summary>
        public static int CountBy(CalibrationCategory category)
        {
            int n = 0;
            foreach (CalibrationBound b in CalibrationCatalog.All())
            {
                if (b.Category == category)
                {
                    n++;
                }
            }

            return n;
        }

        /// <summary>校准报告：每项一行「ID 值 [下界,上界] 通过/越界」。</summary>
        public static string Report()
        {
            var lines = new List<string>();
            foreach (CalibrationBound b in CalibrationCatalog.All())
            {
                lines.Add(b.Id + " " + b.Value + " [" + b.Min + "," + b.Max + "] " + (b.InRange ? "通过" : "越界"));
            }

            return string.Join("\n", lines);
        }
    }
}
