namespace Everlight.Tales.Board
{
    /// <summary>
    /// 实体移动来源（P2-016）：区分机械臂搬动、基础定势下落与零件普通推移，
    /// 供「门轴联系标记必须由普通推移送入校正格」这类目标判定。
    /// </summary>
    public enum MoveSource : byte
    {
        /// <summary>机械臂直接搬动。</summary>
        Arm = 0,

        /// <summary>基础定势下落（重力）。</summary>
        Gravity = 1,

        /// <summary>零件普通推移（撞锤／换向齿轮反推）。</summary>
        Push = 2,
    }
}
