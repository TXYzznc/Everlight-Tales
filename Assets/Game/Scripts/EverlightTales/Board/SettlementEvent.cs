namespace Everlight.Tales.Board
{
    /// <summary>
    /// 拍击结算日志的事件类别。日志是本次拍击的「演算结果」，
    /// 与表现层播放时间轴分离：结算先于播放，播放只按序回放这些记录。
    /// </summary>
    public enum SettlementEventKind
    {
        /// <summary>扣除一次拍击额度。</summary>
        TapDeducted,

        /// <summary>到期效果结算完成。</summary>
        DueEffectApplied,

        /// <summary>实体沿重力方向滑落若干格。</summary>
        EntityMoved,

        /// <summary>新碰撞：实体滑落时尝试进入被占相邻格而受阻。</summary>
        Collision,

        /// <summary>FIFO 事件被取出执行。</summary>
        TriggerDispatched,

        /// <summary>重力接续：阻挡消失/移开后同拍继续下落（D-067）。</summary>
        GravityContinued,

        /// <summary>拍末效果结算完成。</summary>
        TapEndEffectApplied,

        /// <summary>盘面稳定，本拍结算结束。</summary>
        Stabilized,

        /// <summary>一次零件触发被结算：触发源→被触发零件，携带分数与公共能量增量。</summary>
        TriggerResolved,

        /// <summary>一次有效维修被结算：维修对象进度推进，携带分数与公共能量消耗。</summary>
        RepairApplied,
    }

    /// <summary>
    /// 拍击结算日志的一条记录。只读值语义，供播放与同种子复现。
    /// </summary>
    public readonly struct SettlementEvent
    {
        /// <summary>事件类别。</summary>
        public readonly SettlementEventKind Kind;

        /// <summary>主实体 ID（移动者／触发源等）。</summary>
        public readonly int EntityId;

        /// <summary>移动前坐标（未移动时等于 To 或 Zero）。</summary>
        public readonly HexCoord From;

        /// <summary>移动后／目标坐标。</summary>
        public readonly HexCoord To;

        /// <summary>被撞／被触发实体 ID。</summary>
        public readonly int TargetId;

        /// <summary>附加说明。</summary>
        public readonly string Message;

        /// <summary>本条事件产生的分数增量。</summary>
        public readonly int ScoreDelta;

        /// <summary>本条事件产生的公共维修能量增量（产出为正、消耗为负）。</summary>
        public readonly int EnergyDelta;

        public SettlementEvent(
            SettlementEventKind kind,
            int entityId = 0,
            HexCoord from = default,
            HexCoord to = default,
            int targetId = 0,
            string message = null,
            int scoreDelta = 0,
            int energyDelta = 0)
        {
            Kind = kind;
            EntityId = entityId;
            From = from;
            To = to;
            TargetId = targetId;
            Message = message;
            ScoreDelta = scoreDelta;
            EnergyDelta = energyDelta;
        }

        public override string ToString()
        {
            string extra = Message == null ? string.Empty : " " + Message;
            string gain = ScoreDelta == 0 && EnergyDelta == 0 ? string.Empty : " score+" + ScoreDelta + " energy" + (EnergyDelta >= 0 ? "+" : string.Empty) + EnergyDelta;
            return Kind + " e#" + EntityId + " " + From + "->" + To + " t#" + TargetId + extra + gain;
        }
    }
}
