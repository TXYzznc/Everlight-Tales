namespace Everlight.Tales.Data
{
    /// <summary>陈列物类别（b28，P4-009 陈列/保管）。</summary>
    public enum DisplayKind
    {
        None = 0,
        RepairCompletion = 1,   // 普通维修完成物
        LifeGift = 2,           // 生活事件回礼
        Exhibition = 3,         // 修复展成果
        CaseMemento = 4,        // 怪谈纪念物与档案
    }

    /// <summary>
    /// 陈列物（b28）：完成事件/怪谈后留在店里的只读回顾条目，不产生属性或维护负担。
    /// 纯 DTO（Data 层），由事件结算在 UI 层写入、世界会话持久化。
    /// </summary>
    public sealed class DisplayItem
    {
        public string Id;        // 唯一标识（事件/案件 ID，按此去重）
        public string Name;      // 展示名
        public DisplayKind Kind;
        public string Source;    // 来源（事件/案件 ID）
        public int ObtainedDay;  // 获得时点（第几天）

        public DisplayItem(string id, string name, DisplayKind kind, string source, int obtainedDay)
        {
            Id = id;
            Name = name;
            Kind = kind;
            Source = source;
            ObtainedDay = obtainedDay;
        }
    }
}
