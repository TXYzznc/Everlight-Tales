namespace Everlight.Tales.Board
{
    /// <summary>
    /// 棋子实体类别，对应设计 SR-004 的四类盘面对象：
    /// 零件（P 类）、固定设施、任务标记、待维修对象。
    /// </summary>
    public enum EntityKind : byte
    {
        /// <summary>零件，默认可受定势／能力／机械臂移动。</summary>
        Part = 0,

        /// <summary>固定设施，锁住盘面结构，不随定势移动。</summary>
        Facility = 1,

        /// <summary>任务标记，默认不被普通移动。</summary>
        TaskMarker = 2,

        /// <summary>待维修对象。</summary>
        RepairTarget = 3,
    }
}
