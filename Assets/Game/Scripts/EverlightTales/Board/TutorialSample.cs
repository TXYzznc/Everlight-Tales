using System.Collections.Generic;
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
            return Build(TutorialStageId.RotateAndCollide, "TUT-RC");
        }

        /// <summary>第二段：能量与维修。棘轮受撞产公共维修能量，适配零件落入维修对象完成一次有效维修。</summary>
        public static TutorialStage EnergyAndRepair()
        {
            return Build(TutorialStageId.EnergyAndRepair, "TUT-ER");
        }

        /// <summary>第三段：爆破与拆障。来撞件撞上线圈，线圈起爆并冲击邻件（隔板 O-002 留 b13，本段以邻件冲击代表爆破链路）。</summary>
        public static TutorialStage BlastAndClear()
        {
            return Build(TutorialStageId.BlastAndClear, "TUT-BC");
        }

        private static TutorialStage Build(TutorialStageId id, string key)
        {
            LevelPreset preset = LevelPresetCatalog.Get(key);
            if (preset == null) throw new System.InvalidOperationException("教学关卡配置尚未加载：" + key);
            var selected = new List<PartType>();
            foreach (KeyPieceConfig piece in preset.Board.KeyPieces) selected.Add(piece.PartType);
            BoardState board = InitialBoardBuilder.Build(preset.Board, selected, new RandomService(0));
            var level = new LevelState(preset.Level);
            level.BeginLevel();
            level.Session.PublicRepairEnergy = preset.Board.InitialPublicRepairEnergy;
            TutorialHintConfig hint = TutorialHintCatalog.Get((int)id);
            if (hint == null) throw new System.InvalidOperationException("教学标题配置尚未加载：" + id);
            string title = hint.Title;
            return new TutorialStage(id, title, board, new SettleState(HexDirection.D0), level);
        }
    }
}
