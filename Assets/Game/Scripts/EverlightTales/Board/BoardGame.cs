using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 盘面操作循环编排（P1-016 逻辑）。把已落地的 Board 层能力（旋转、机械臂、预演、拍击结算、过轮判定）
    /// 串成一次拍击的操作循环：旋转 → 可选搬动 → 预演 → 拍击 → 过轮判定。
    /// 纯逻辑、不引用引擎，供 UI 表现层绑定与离线演算共用。
    /// </summary>
    public sealed class BoardGame
    {
        public BoardState Board { get; }

        public SettleState Settle { get; }

        public SessionState Session { get; }

        public LevelState Level { get; }
        public RedShoeLevel RedShoeLevel { get; }
        public BuffBonuses Bonuses { get; set; }

        public TapSettlement TapTransaction { get; } = new TapSettlement();

        /// <summary>最近一次拍击的结算结果（供表现层读取事件日志驱动特效）。</summary>
        public SettlementResult LastSettlement { get; private set; }

        public BoardGame(BoardState board, SettleState settle, SessionState session, LevelState level, RedShoeLevel redShoeLevel = null)
        {
            Board = board;
            Settle = settle;
            Session = session;
            Level = level;
            RedShoeLevel = redShoeLevel;
        }

        /// <summary>左旋一个相位。</summary>
        public void RotateLeft()
        {
            Settle.RotateLeft();
            if (RedShoeLevel != null) RedShoeLevel.RedShoe.Direction = HexDirections.Rotate(RedShoeLevel.RedShoe.Direction, -1);
        }

        /// <summary>右旋一个相位。</summary>
        public void RotateRight()
        {
            Settle.RotateRight();
            if (RedShoeLevel != null) RedShoeLevel.RedShoe.Direction = HexDirections.Rotate(RedShoeLevel.RedShoe.Direction, 1);
        }

        /// <summary>按当前势位预演每枚会动棋子的第一步终点（P1-013）。</summary>
        public System.Collections.Generic.IReadOnlyList<PreviewMove> Preview()
        {
            return PreviewService.Compute(Board, Settle);
        }

        /// <summary>机械臂搬动一枚棋子（P1-010），返回搬动结果。</summary>
        public ArmMoveResult ArmMove(BoardEntity entity, HexCoord target)
        {
            if (RedShoeLevel != null && !RedShoeLevel.RedShoe.IsRemoved && target == RedShoeLevel.RedShoe.Coord) return ArmMoveResult.Occupied;
            return ArmService.Move(Board, Session, entity, target);
        }

        /// <summary>执行一次拍击：以本轮剩余额度结算，并回填额度、返回过轮判定（P1-011）。</summary>
        public RoundPassResult Tap()
        {
            // D-060 定时线圈：本拍到期待爆实体先于基础定势下落执行，结算后拍序号 +1。
            var dueEffects = FuseCoilService.BuildDueEffects(Board, Session.TapSerial);
            SettlementResult result = TapTransaction.Settle(
                Board,
                Settle,
                new TapContext(Level.Round.TapQuotaRemaining, dueEffects),
                Session, Bonuses);
            if (Bonuses != null) Session.PublicRepairEnergy += Bonuses.TapEndEnergy;
            Session.TapSerial++;
            LastSettlement = result;
            if (RedShoeLevel != null)
            {
                RedShoeService.StepAfterTap(Board, RedShoeLevel.Config.RedShoe, RedShoeLevel.RedShoe);
                foreach (SpecialGoalState goal in Level.Round.Goals)
                {
                    int current = goal.Id == "导流" ? RedShoeLevel.RedShoe.DiversionProgress
                        : goal.Id == "封存匣维修" ? Board.EntityAt(RedShoeLevel.Config.RedShoe.BoxCoord)?.RepairProgress ?? 0
                        : RedShoeLevel.RedShoe.IsSealed ? 1 : 0;
                    goal.Progress(current - goal.Current);
                }
            }
            RoundPassResult pass = Level.ResolveAfterTap(result.TapQuotaAfter);
            if (RedShoeLevel != null)
            {
                if (RedShoeLevel.RedShoe.HasFailed) return RoundPassResult.Failed;
                if (pass == RoundPassResult.Passed && !RedShoeRoundEvaluator.IsRoundComplete(
                    RedShoeLevel.Config.Rounds[Level.RoundIndex], RedShoeLevel)) return RoundPassResult.Failed;
            }
            return pass;
        }
    }
}
