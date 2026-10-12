using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>
    /// 事件费用结构（P3-002）：一次内容单元的时间成本、维修费收入与材料奖励，
    /// 与事件模板的触发/内容/结果一并组装。
    /// </summary>
    public sealed class EventCost
    {
        public int TimeCost;
        public int RepairFee;
        public IReadOnlyList<string> Materials;

        public EventCost(int timeCost, int repairFee, IReadOnlyList<string> materials = null)
        {
            TimeCost = timeCost;
            RepairFee = repairFee;
            Materials = materials ?? System.Array.Empty<string>();
        }
    }

    /// <summary>事件费用装配规则（P3-002，D-082 经济基准）。</summary>
    public static class EventPricing
    {
        /// <summary>普通收入基准，读取校准表中的维修费单价。</summary>
        public static int FeePerCell
        {
            get
            {
                return CalibrationCatalog.Value("ECO-FEE-CELL");
            }
        }

        /// <summary>普通事件按时间格与校准表单价换算维修费。</summary>
        public static int BaselineFee(int timeCost)
        {
            return timeCost * FeePerCell;
        }

        /// <summary>组装一份普通事件费用：时间成本 + 基准维修费 + 可选材料。</summary>
        public static EventCost Assemble(int timeCost, IReadOnlyList<string> materials = null)
        {
            return new EventCost(timeCost, BaselineFee(timeCost), materials);
        }

        /// <summary>剧情／任务／怪谈奖励可在基准上修正，返回修正后的费用结构。</summary>
        public static EventCost AssembleAdjusted(int timeCost, int repairFee, IReadOnlyList<string> materials = null)
        {
            return new EventCost(timeCost, repairFee, materials);
        }
    }
}
