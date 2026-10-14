using System;
using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    public sealed class RestoredAttempt
    {
        public BoardState Board;
        public LevelState Level;
        public BuffSet Buffs;
        public RandomService Rng;
        public SettleState Settle;
        public RedShoeState RedShoe;
    }

    /// <summary>完整轮边界捕获与恢复；所有错误显式拒绝，不生成半有效盘面。</summary>
    public static class RepairSaveService
    {
        public static RepairAttemptSave Capture(string instanceId, string levelId, BoardState board,
            LevelState level, RandomService rng, BuffSet buffs, SettleState settle = null, RedShoeState shoe = null)
        {
            if (board == null || level?.Round == null || rng == null || buffs == null)
                throw new ArgumentException("Missing attempt state.");
            var save = new RepairAttemptSave
            {
                InstanceId = instanceId, LevelId = levelId, RoundIndex = level.RoundIndex,
                TapsUsed = level.Round.TapCount - level.Round.TapQuotaRemaining,
                Score = level.Session.Score, PublicRepairEnergy = level.Session.PublicRepairEnergy,
                ArmMoves = level.Session.ArmMoves, Seed = rng.Seed, RandomConsumed = rng.ConsumedCount,
                RandomState = rng.State, TapSerial = level.Session.TapSerial,
                Gravity = settle?.GravityDirection ?? HexDirection.D0
            };
            BoardSnapshotCodec.Capture(save, board, shoe);
            foreach (SpecialGoalState goal in level.Round.Goals) save.GoalProgress.Add(goal.Current);
            foreach (BuffState buff in buffs.All)
                save.Buffs.Add(new SavedBuff { Id = buff.Config.Id, Stacks = buff.Stacks });
            save.Buffs.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            return save;
        }

        public static RestoredAttempt Restore(RepairAttemptSave save, int boardRadius, LevelConfig levelConfig)
        {
            if (save == null || levelConfig == null || save.BoardRadius != boardRadius
                || save.RoundIndex < 0 || save.RoundIndex >= levelConfig.Rounds.Count
                || save.Score < 0 || save.PublicRepairEnergy < 0 || save.ArmMoves < 0
                || save.TapSerial < 0 || save.RandomConsumed < 0
                || !Enum.IsDefined(typeof(HexDirection), save.Gravity) || save.Buffs == null || save.GoalProgress == null)
                throw new ArgumentException("Invalid attempt header.");
            var board = BoardSnapshotCodec.Restore(save);
            var level = new LevelState(levelConfig);
            level.BeginLevel();
            for (int i = 0; i < save.RoundIndex; i++) level.AdvanceRound();
            if (save.TapsUsed < 0 || save.TapsUsed > level.Round.TapCount || save.GoalProgress.Count != level.Round.Goals.Count)
                throw new ArgumentException("Invalid saved round.");
            level.ResolveAfterTap(level.Round.TapCount - save.TapsUsed);
            for (int i = 0; i < save.GoalProgress.Count; i++)
            {
                if (save.GoalProgress[i] < 0) throw new ArgumentException("Invalid goal progress.");
                level.Round.Goals[i].Progress(save.GoalProgress[i]);
            }
            level.Session.Score = save.Score;
            level.Session.PublicRepairEnergy = save.PublicRepairEnergy;
            level.Session.ArmMoves = save.ArmMoves;
            level.Session.TapSerial = save.TapSerial;
            var buffs = new BuffSet();
            var seen = new HashSet<string>();
            foreach (SavedBuff saved in save.Buffs)
            {
                if (saved == null || string.IsNullOrEmpty(saved.Id) || !seen.Add(saved.Id))
                    throw new ArgumentException("Duplicate or missing buff.");
                BuffConfig config = BuffCatalog.Get(saved.Id);
                if (config == null || saved.Stacks < 1 || saved.Stacks > config.MaxStacks
                    || (!config.Stackable && saved.Stacks != 1)) throw new ArgumentException("Unknown or invalid buff.");
                for (int i = 0; i < saved.Stacks; i++)
                    if (buffs.Add(config) == null) throw new ArgumentException("Invalid buff stack.");
            }
            var rng = new RandomService(save.Seed);
            rng.RestoreState(save.Seed, save.RandomConsumed, save.RandomState);
            return new RestoredAttempt
            {
                Board = board, Level = level, Buffs = buffs, Rng = rng,
                Settle = new SettleState(save.Gravity), RedShoe = BoardSnapshotCodec.RestoreRedShoe(save.RedShoe, board)
            };
        }
    }
}
