using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>校准类别（P6-001/P6-002）。</summary>
    public enum CalibrationCategory : byte
    {
        Tutorial = 0,   // 教学
        RedShoe = 1,    // 红舞鞋第一关
        RollerDoor = 2, // 卡住的卷帘门
        Economy = 3,    // 经济成长节奏
    }

    /// <summary>一条校准项（P6-001/P6-002）：当前值 + 口径下界/上界。</summary>
    public sealed class CalibrationBound
    {
        public string Id;
        public CalibrationCategory Category;
        public string Name;
        public int Value;   // 当前校准值
        public int Min;     // 口径下界（含）
        public int Max;     // 口径上界（含）
        public string Note;

        public CalibrationBound(string id, CalibrationCategory category, string name,
            int value, int min, int max, string note)
        {
            Id = id;
            Category = category;
            Name = name;
            Value = value;
            Min = min;
            Max = max;
            Note = note;
        }

        public bool InRange => Value >= Min && Value <= Max;
    }

    /// <summary>
    /// 校准项目录（P6-001/P6-002）：教学/红舞鞋/卷帘门/经济成长的按盘反馈与收支节奏口径。
    /// 值沿用已实现配置（TutorialSample/RedShoeLevelConfig/RollerDoorEvent/EventPricing/FormCatalog），
    /// 口径为试玩前固化的可接受区间，正式调值留试玩后迭代。
    /// </summary>
    public static class CalibrationCatalog
    {
        private static readonly IReadOnlyList<CalibrationBound> _bounds = new[]
        {
            // 教学（P6-001）：三段、0 时间格、宽松通过
            new CalibrationBound("TUT-STAGES", CalibrationCategory.Tutorial, "教学段数", 3, 3, 3, "固定三段"),
            new CalibrationBound("TUT-TIME", CalibrationCategory.Tutorial, "教学时间格", 0, 0, 0, "0 时间格"),
            new CalibrationBound("TUT-SCORE", CalibrationCategory.Tutorial, "教学通过分", 30, 20, 40, "三段 28/30/32 的宽松区间"),

            // 红舞鞋第一关（P6-001）：三轮目标、4 时间格、导流 6
            new CalibrationBound("RS-ROUNDS", CalibrationCategory.RedShoe, "红舞鞋轮数", 3, 3, 3, "三轮"),
            new CalibrationBound("RS-TARGET", CalibrationCategory.RedShoe, "红舞鞋末轮目标分", 170, 140, 200, "30/100/170 末轮口径"),
            new CalibrationBound("RS-TIME", CalibrationCategory.RedShoe, "红舞鞋时间格", 4, 3, 5, "耗时 4"),
            new CalibrationBound("RS-DIVERSION", CalibrationCategory.RedShoe, "导流分离次数", 6, 5, 7, "D-051 导流 6 次分离"),

            // 卷帘门（P6-001）：1 轮 3 拍 30 分、2 时间格、40 费
            new CalibrationBound("RD-ROUNDS", CalibrationCategory.RollerDoor, "卷帘门轮数", 1, 1, 1, "一轮"),
            new CalibrationBound("RD-TAPS", CalibrationCategory.RollerDoor, "卷帘门拍数", 3, 2, 5, "拍数口径"),
            new CalibrationBound("RD-SCORE", CalibrationCategory.RollerDoor, "卷帘门目标分", 30, 20, 40, "目标分口径"),
            new CalibrationBound("RD-TIME", CalibrationCategory.RollerDoor, "卷帘门时间格", 2, 1, 3, "时间格口径"),
            new CalibrationBound("RD-FEE", CalibrationCategory.RollerDoor, "卷帘门维修费", 40, 20, 60, "奖励费口径"),

            // 经济成长（P6-002）：维修费单价、支线领奖、形态费、回访费
            new CalibrationBound("ECO-FEE-CELL", CalibrationCategory.Economy, "维修费单价", 20, 15, 25, "20 费/格"),
            new CalibrationBound("ECO-MOD-CLAIM", CalibrationCategory.Economy, "改装支线领奖", 40, 30, 50, "改装领奖费"),
            new CalibrationBound("ECO-FORM-MIN", CalibrationCategory.Economy, "形态解锁费下界", 80, 60, 100, "F 类起点 80"),
            new CalibrationBound("ECO-FORM-MAX", CalibrationCategory.Economy, "形态解锁费上界", 480, 400, 560, "M 类上限 480"),
            new CalibrationBound("ECO-REVISIT", CalibrationCategory.Economy, "回访奖励费", 120, 100, 140, "回访费口径"),
        };

        public static IReadOnlyList<CalibrationBound> All() => _bounds;

        public static CalibrationBound Get(string id)
        {
            foreach (CalibrationBound b in _bounds)
            {
                if (b.Id == id)
                {
                    return b;
                }
            }

            return null;
        }
    }
}
