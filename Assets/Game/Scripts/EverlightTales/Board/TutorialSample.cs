using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>教学三段样张标识（P1-022 G1 验收门）。</summary>
    public enum TutorialStageId
    {
        RotateAndCollide,
        EnergyAndRepair,
        BlastAndClear,
    }

    /// <summary>一段教学样张的完整盘面装配：盘面 + 势位 + 关卡运行时（会话在 Level 内）。</summary>
    public sealed class TutorialStage
    {
        public TutorialStageId Id { get; }

        public string Title { get; }

        public BoardState Board { get; }

        public SettleState Settle { get; }

        public LevelState Level { get; }

        public SessionState Session => Level.Session;

        public TutorialStage(TutorialStageId id, string title, BoardState board, SettleState settle, LevelState level)
        {
            Id = id;
            Title = title;
            Board = board;
            Settle = settle;
            Level = level;
        }
    }

    /// <summary>
    /// 教学三段样张（P1-022）：撞锤碰撞 → 棘轮产能+维修对象 → 线圈爆破，均在边长 5（61 格）教学档盘上跑通。
    /// 只固定「教什么、按什么顺序、用什么零件」，坐标是示例、可在实现层微调（28-新手教学与首个可玩切片.md）。
    /// </summary>
    public static class TutorialSample
    {
        /// <summary>第一段：旋转与碰撞。来撞件沿重力撞上撞锤，撞锤反推，完成一次移动碰撞。</summary>
        public static TutorialStage RotateAndCollide()
        {
            var board = new BoardState(5);
            board.Place(BoardEntity.Facility(3), new HexCoord(2, 0)); // 固定墙
            board.Place(BoardEntity.Part(2, PartType.InertiaHammer), new HexCoord(1, 0));
            board.Place(BoardEntity.Part(1, PartType.None), new HexCoord(-1, 0)); // 来撞件
            return new TutorialStage(TutorialStageId.RotateAndCollide, "旋转与碰撞", board, new SettleState(HexDirection.D0), NewLevel(2, 15));
        }

        /// <summary>第二段：能量与维修。棘轮受撞产公共维修能量，适配零件落入维修对象完成一次有效维修。</summary>
        public static TutorialStage EnergyAndRepair()
        {
            var board = new BoardState(5);
            var repair = new RepairTargetConfig(new[] { PartType.MeteringRatchet }, requiredProgress: 1, energyCost: 1);
            board.Place(BoardEntity.RepairTarget(3, repair), new HexCoord(1, 0));
            board.Place(BoardEntity.Part(2, PartType.MeteringRatchet), new HexCoord(-1, 0));
            board.Place(BoardEntity.Part(1, PartType.None), new HexCoord(-3, 0));
            var level = NewLevel(2, 25);
            level.Session.PublicRepairEnergy = 1; // 代表「棘轮已产出」的公共维修能量，供维修费用扣除。
            return new TutorialStage(TutorialStageId.EnergyAndRepair, "能量与维修", board, new SettleState(HexDirection.D0), level);
        }

        /// <summary>第三段：爆破与拆障。来撞件撞上线圈，线圈起爆并冲击邻件（隔板 O-002 留 b13，本段以邻件冲击代表爆破链路）。</summary>
        public static TutorialStage BlastAndClear()
        {
            var board = new BoardState(5);
            board.Place(BoardEntity.Facility(4), new HexCoord(3, 0)); // 固定墙占位，阻止棘轮外滑
            board.Place(BoardEntity.Part(3, PartType.MeteringRatchet), new HexCoord(2, 0)); // 爆破命中的邻件
            board.Place(BoardEntity.Part(2, PartType.BlastCoil), new HexCoord(1, 0));
            board.Place(BoardEntity.Part(1, PartType.None), new HexCoord(-1, 0)); // 来撞件
            return new TutorialStage(TutorialStageId.BlastAndClear, "爆破与拆障", board, new SettleState(HexDirection.D0), NewLevel(2, 20));
        }

        private static LevelState NewLevel(int tapCount, int targetScore)
        {
            var level = new LevelState(new LevelConfig(new[] { new RoundConfig(tapCount, targetScore) }));
            level.BeginLevel();
            return level;
        }
    }
}
