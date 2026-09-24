using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>恢复后的维修尝试状态（P2-010）。</summary>
    public sealed class RestoredAttempt
    {
        public BoardState Board;
        public LevelState Level;
        public BuffSet Buffs;
        public RandomService Rng;
    }

    /// <summary>
    /// 维修尝试存档服务（P2-010）：捕获盘面/轮次/分数/能量/机械臂/Buff/随机状态，
    /// 恢复为同一轮起点（D-032）。落 Events 层（需同时触达 Board 运行时与 BuffSet）。
    /// </summary>
    public static class RepairSaveService
    {
        public static RepairAttemptSave Capture(string instanceId, string levelId, BoardState board, LevelState level, RandomService rng, BuffSet buffs)
        {
            var save = new RepairAttemptSave();
            save.InstanceId = instanceId;
            save.LevelId = levelId;
            save.RoundIndex = level.RoundIndex;
            save.TapsUsed = level.Round.TapCount - level.Round.TapQuotaRemaining;
            save.Score = level.Session.Score;
            save.PublicRepairEnergy = level.Session.PublicRepairEnergy;
            save.ArmMoves = level.Session.ArmMoves;
            save.Seed = rng.Seed;
            save.RandomConsumed = rng.ConsumedCount;
            save.TapSerial = level.Session.TapSerial;

            foreach (BoardEntity entity in board.Entities)
            {
                var saved = new SavedEntity
                {
                    Id = entity.Id,
                    Kind = entity.Kind,
                    PartType = entity.PartType,
                    ObstacleType = entity.ObstacleType,
                    Coord = entity.Coord,
                    Energy = entity.Energy,
                    EnergyCapacity = entity.EnergyCapacity,
                    Durability = entity.Durability,
                    IsOpen = entity.IsOpen,
                    PassDirection = HexDirections.ToIndex(entity.PassDirection),
                    ClampedEntityId = entity.ClampedEntityId,
                    SealActive = entity.SealActive,
                    RepairProgress = entity.RepairProgress,
                    RepairCompleted = entity.RepairCompleted,
                    IsLocked = entity.IsLocked,
                    Movable = !entity.IsFixed,
                    AnchorLabel = entity.Goal != null ? entity.Goal.AnchorLabel : 0,
                    FormId = entity.FormId,
                    FuseDueTap = entity.FuseDueTap,
                };

                if (entity.RepairConfig != null)
                {
                    saved.RepairRequired = entity.RepairConfig.RequiredProgress;
                    saved.RepairEnergyCost = entity.RepairConfig.EnergyCost;
                    foreach (PartType part in entity.RepairConfig.CompatibleParts)
                    {
                        saved.CompatibleParts.Add(part);
                    }
                }

                save.Entities.Add(saved);
            }

            foreach (BuffState buff in buffs.All)
            {
                save.Buffs.Add(new SavedBuff { Id = buff.Config.Id, Stacks = buff.Stacks });
            }

            return save;
        }

        public static RestoredAttempt Restore(RepairAttemptSave save, int boardRadius, LevelConfig levelConfig)
        {
            var board = new BoardState(boardRadius);
            foreach (SavedEntity saved in save.Entities)
            {
                board.Place(FromSaved(saved), saved.Coord);
            }

            var level = new LevelState(levelConfig);
            level.BeginLevel();
            for (int i = 0; i < save.RoundIndex; i++)
            {
                level.AdvanceRound();
            }

            level.Session.Score = save.Score;
            level.Session.PublicRepairEnergy = save.PublicRepairEnergy;
            level.Session.ArmMoves = save.ArmMoves;
            level.Session.TapSerial = save.TapSerial;

            var buffs = new BuffSet();
            foreach (SavedBuff saved in save.Buffs)
            {
                BuffConfig config = BuffCatalog.Get(saved.Id);
                if (config == null)
                {
                    continue;
                }

                BuffState state = buffs.Add(config);
                for (int k = 1; k < saved.Stacks && state != null; k++)
                {
                    state.AddStack();
                }
            }

            var rng = new RandomService(save.Seed);
            rng.Restore(save.Seed, save.RandomConsumed);

            return new RestoredAttempt { Board = board, Level = level, Buffs = buffs, Rng = rng };
        }

        private static BoardEntity FromSaved(SavedEntity saved)
        {
            BoardEntity entity;
            switch (saved.Kind)
            {
                case EntityKind.Part:
                    entity = BoardEntity.Part(saved.Id, saved.PartType, saved.FormId);
                    entity.SetEnergy(saved.Energy);
                    entity.SetFuseDueTap(saved.FuseDueTap);
                    break;

                case EntityKind.RepairTarget:
                    entity = BoardEntity.RepairTarget(saved.Id, new RepairTargetConfig(saved.CompatibleParts, saved.RepairRequired, saved.RepairEnergyCost));
                    break;

                case EntityKind.TaskMarker:
                    if (saved.Movable)
                    {
                        GoalConfig goal = saved.AnchorLabel > 0
                            ? new GoalConfig("gate", GoalKind.PushedInto, saved.Id, saved.AnchorLabel)
                            : null;
                        entity = BoardEntity.MovableTaskMarker(saved.Id, goal);
                    }
                    else
                    {
                        entity = BoardEntity.TaskMarker(saved.Id);
                    }

                    break;

                case EntityKind.Facility:
                default:
                    entity = saved.ObstacleType != ObstacleType.None
                        ? BoardEntity.Obstacle(saved.Id, ObstacleCatalog.Get(saved.ObstacleType))
                        : BoardEntity.Facility(saved.Id);
                    break;
            }

            entity.RestoreState(saved.Durability, saved.IsOpen, saved.PassDirection, saved.ClampedEntityId, saved.SealActive, saved.RepairProgress, saved.RepairCompleted);
            entity.SetLocked(saved.IsLocked);
            return entity;
        }
    }
}
