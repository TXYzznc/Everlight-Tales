namespace Everlight.Tales.Board
{
    /// <summary>
    /// 会话持久状态（P1-008／P1-009）：公共维修能量与累计分数。
    /// 公共维修能量关初为 0、跨拍跨轮累计；分数跨拍跨轮累计、跨关由上层清零。
    /// 本类型不引用引擎。
    /// </summary>
    public sealed class SessionState
    {
        /// <summary>公共维修能量（当前关共享储备）。</summary>
        public int PublicRepairEnergy { get; set; }

        /// <summary>累计分数。</summary>
        public int Score { get; set; }

        /// <summary>机械臂剩余搬动次数（P1-010）。跨轮保留、不自动补满，仅搬动成功时递减。</summary>
        public int ArmMoves { get; set; }
    }
}
