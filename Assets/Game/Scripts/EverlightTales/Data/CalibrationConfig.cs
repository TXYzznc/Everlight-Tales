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
        private static IReadOnlyList<CalibrationBound> _bounds = System.Array.Empty<CalibrationBound>();
        public static void Install(IEnumerable<CalibrationBound> bounds)
        {
            var list = new List<CalibrationBound>();
            var keys = new HashSet<string>();
            foreach (CalibrationBound bound in bounds)
            {
                if (bound == null || string.IsNullOrEmpty(bound.Id) || string.IsNullOrEmpty(bound.Name) || string.IsNullOrEmpty(bound.Note) || bound.Min > bound.Max || bound.Value < bound.Min || bound.Value > bound.Max || !keys.Add(bound.Id))
                    throw new System.ArgumentException("CalibrationCatalog 存在无效校准项或重复键。", nameof(bounds));
                list.Add(bound);
            }
            _bounds = list.AsReadOnly();
        }

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

        public static int Value(string id)
        {
            CalibrationBound bound = Get(id);
            if (bound == null) throw new System.InvalidOperationException("校准项尚未加载：" + id);
            return bound.Value;
        }
    }
}
