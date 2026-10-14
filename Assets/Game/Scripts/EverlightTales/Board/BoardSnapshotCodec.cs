using System;
using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>显式盘面快照编解码；不序列化运行时对象引用或配置缓存。</summary>
    public static class BoardSnapshotCodec
    {
        public static void Capture(RepairAttemptSave save, BoardState board, RedShoeState shoe = null)
        {
            save.BoardRadius = board.BoardRadius;
            save.NextEntityId = board.NextEntityId;
            save.NextAnomalyId = board.NextAnomalyId;
            save.LastAnomalyTap = board.LastAnomalyTap;
            save.StructureAccidents = board.StructureAccidents;
            save.LastObstacleTap = board.LastObstacleTap;
            foreach (HexCoord cell in board.EnumerateNormal())
            {
                var item = new SavedCell { Coord = cell };
                item.HasTerrain = board.TryGetTerrain(cell, out item.Terrain);
                item.HasEndpoint = board.TryGetEndpointLabel(cell, out item.EndpointLabel);
                if (item.HasTerrain || item.HasEndpoint) save.Cells.Add(item);
            }
            var seen = new HashSet<int>();
            foreach (BoardEntity entity in board.Entities)
            {
                SavedEntity item = CaptureEntity(entity, seen, 0, board);
                item.LastMoveSource = board.LastMoveSource(entity.Id);
                save.Entities.Add(item);
            }
            save.Entities.Sort((a, b) => a.Id.CompareTo(b.Id));
            foreach (AnomalyRegion region in board.AnomalyRegions)
                save.Anomalies.Add(new SavedAnomaly
                {
                    Id = region.Id, Type = region.Type, Cells = new List<HexCoord>(region.Cells), ExitCell = region.ExitCell,
                    ExpansionCells = new List<HexCoord>(region.ExpansionCells), DeflectionActive = region.DeflectionActive,
                    Silent = region.Silent, UseCount = region.UseCount, ExpansionCount = region.ExpansionCount,
                    Heat = region.Heat, Cores = region.Cores, TapCounter = region.TapCounter,
                    EchoRemaining = region.EchoRemaining, RedrawCursor = region.RedrawCursor, LastRedrawRound = region.LastRedrawRound,
                    RedrawStopped = region.RedrawStopped, SettlingStopped = region.SettlingStopped,
                    FreezeNextReversal = region.FreezeNextReversal, CarrierIds = new List<int>(region.CarrierIds)
                });
            if (shoe != null)
            {
                save.RedShoe = new SavedRedShoe
                {
                    Coord = shoe.Coord, Direction = shoe.Direction, IsBound = shoe.IsBound,
                    DiversionProgress = shoe.DiversionProgress, RestraintLoad = shoe.RestraintLoad,
                    IsSealed = shoe.IsSealed, IsRemoved = shoe.IsRemoved, RestraintLimit = shoe.RestraintLimit,
                    CountedGuides = new List<HexCoord>(shoe.CountedGuides)
                };
                save.RedShoe.CountedGuides.Sort((a, b) => a.Q != b.Q ? a.Q.CompareTo(b.Q) : a.R.CompareTo(b.R));
            }
        }

        public static BoardState Restore(RepairAttemptSave save)
        {
            Require(save != null && save.Version == RepairAttemptSave.CurrentVersion, "Unsupported attempt version.");
            Require(save.BoardRadius >= 1 && save.BoardRadius <= 32, "Invalid radius.");
            Require(save.Entities != null && save.Cells != null && save.Anomalies != null, "Missing board lists.");
            Require(save.Entities.Count <= 4096 && save.Anomalies.Count <= 4096 && save.Cells.Count <= 16384, "Oversized board.");
            var board = new BoardState(save.BoardRadius);
            var cells = new HashSet<HexCoord>();
            foreach (SavedCell cell in save.Cells)
            {
                Require(cell != null && board.IsValid(cell.Coord) && cells.Add(cell.Coord), "Invalid or duplicate cell.");
                if (cell.HasTerrain)
                {
                    Require(Enum.IsDefined(typeof(TerrainKind), cell.Terrain), "Unknown terrain.");
                    board.SetTerrain(cell.Coord, cell.Terrain);
                }
                if (cell.HasEndpoint) board.SetEndpointLabel(cell.Coord, cell.EndpointLabel);
            }
            var ids = new HashSet<int>();
            foreach (SavedEntity saved in save.Entities)
            {
                BoardEntity entity = RestoreEntity(saved, ids, 0);
                Require(board.Place(entity, saved.Coord, true) == PlaceResult.Ok, "Invalid saved placement.");
                board.RestoreMoveSource(entity.Id, saved.LastMoveSource);
            }
            foreach (SavedAnomaly saved in save.Anomalies)
            {
                Require(saved != null && Enum.IsDefined(typeof(AnomalyType), saved.Type)
                    && saved.Type != AnomalyType.None && AnomalyCatalog.Get(saved.Type) != null, "Unknown anomaly.");
                Require(saved.Cells != null && saved.ExpansionCells != null, "Missing anomaly cells.");
                var regionCells = new HashSet<HexCoord>();
                foreach (HexCoord cell in saved.Cells)
                    Require(board.IsValid(cell) && regionCells.Add(cell), "Invalid anomaly cell.");
                foreach (HexCoord cell in saved.ExpansionCells)
                    Require(board.IsValid(cell) && regionCells.Add(cell), "Invalid expansion cell.");
                if (saved.Type == AnomalyType.ExpandingRift) Require(board.IsValid(saved.ExitCell), "Invalid rift exit.");
                var region = new AnomalyRegion(saved.Type, saved.Cells, saved.ExitCell, saved.ExpansionCells)
                { Id = saved.Id, DeflectionActive = saved.DeflectionActive, Silent = saved.Silent,
                    EchoRemaining = saved.EchoRemaining, RedrawCursor = saved.RedrawCursor, LastRedrawRound = saved.LastRedrawRound,
                    RedrawStopped = saved.RedrawStopped, SettlingStopped = saved.SettlingStopped, FreezeNextReversal = saved.FreezeNextReversal };
                Require(saved.Id > 0 && saved.EchoRemaining >= 0 && saved.LastRedrawRound >= -1
                    && saved.EchoRemaining <= (saved.Type == AnomalyType.EchoZone ? region.Config.EchoQuota : 0), "Invalid anomaly runtime state.");
                region.RestoreState(saved.UseCount, saved.ExpansionCount, saved.Heat, saved.Cores, saved.TapCounter);
                region.RestoreCarriers(saved.CarrierIds, save.NextEntityId);
                foreach (int carrier in region.CarrierIds)
                    if (board.TryGetEntity(carrier, out BoardEntity entity))
                        Require(entity.Kind == EntityKind.Part || entity.Kind == EntityKind.TaskMarker, "Invalid carrier category.");
                board.RegisterAnomaly(region);
            }
            int maxId = 0;
            foreach (int id in ids) maxId = Math.Max(maxId, id);
            Require(save.NextEntityId > maxId, "Invalid entity allocation state.");
            board.RestoreAllocation(save.NextEntityId);
            Require(save.NextAnomalyId >= board.NextAnomalyId && save.LastAnomalyTap >= -1, "Invalid anomaly allocation or serial.");
            board.NextAnomalyId = save.NextAnomalyId; board.LastAnomalyTap = save.LastAnomalyTap;
            Require(save.StructureAccidents >= 0, "Invalid structure accident count.");
            board.StructureAccidents = save.StructureAccidents;
            Require(save.LastObstacleTap >= -1, "Invalid obstacle boundary serial."); board.LastObstacleTap = save.LastObstacleTap;
            ValidateRelations(board);
            foreach (SavedEntity saved in save.Entities) RestoreMoveHistory(board, saved);
            return board;
        }

        private static void RestoreMoveHistory(BoardState board, SavedEntity saved)
        {
            board.RestoreHistoricalMoveSource(saved.Id, saved.LastMoveSource);
            foreach (SavedEntity stored in saved.Stored) RestoreMoveHistory(board, stored);
        }

        public static RedShoeState RestoreRedShoe(SavedRedShoe saved, BoardState board)
        {
            if (saved == null) return null;
            Require(board.IsValid(saved.Coord) && Enum.IsDefined(typeof(HexDirection), saved.Direction)
                && saved.RestraintLimit > 0 && saved.DiversionProgress >= 0 && saved.RestraintLoad >= 0
                && saved.CountedGuides != null && saved.CountedGuides.Count == saved.DiversionProgress,
                "Invalid red shoe state.");
            foreach (HexCoord cell in saved.CountedGuides) Require(board.IsValid(cell), "Invalid counted guide.");
            var shoe = new RedShoeState(saved.Coord, saved.Direction, saved.RestraintLimit)
            {
                IsBound = saved.IsBound, DiversionProgress = saved.DiversionProgress,
                RestraintLoad = saved.RestraintLoad, IsSealed = saved.IsSealed, IsRemoved = saved.IsRemoved
            };
            shoe.RestoreCountedGuides(saved.CountedGuides);
            return shoe;
        }

        private static SavedEntity CaptureEntity(BoardEntity e, HashSet<int> ids, int depth, BoardState board)
        {
            Require(depth <= 32 && ids.Count < 4096 && ids.Add(e.Id), "Cyclic or duplicate entity.");
            var s = new SavedEntity
            {
                Id = e.Id, Kind = e.Kind, PartType = e.PartType, ObstacleType = e.ObstacleType,
                Coord = e.Coord, Energy = e.Energy, EnergyCapacity = e.EnergyCapacity,
                Durability = e.Durability, MaxDurability = e.MaxDurability, IsOpen = e.IsOpen, PassDirection = HexDirections.ToIndex(e.PassDirection),
                ClampedEntityId = e.ClampedEntityId, SealActive = e.SealActive, RepairProgress = e.RepairProgress,
                ClampedByEntityId = e.ClampedByEntityId, PendingClose = e.PendingClose,
                ReleaseWindowRemaining = e.ReleaseWindowRemaining, ReleasedEntityId = e.ReleasedEntityId,
                GrowthRootEntityId = e.GrowthRootEntityId, ProtectedElementId = e.ProtectedElementId,
                LinkedElementIds = new List<int>(e.LinkedElementIds),
                RepairCompleted = e.RepairCompleted, IsLocked = e.IsLocked, Movable = !e.IsFixed,
                FormId = e.FormId, FuseDueTap = e.FuseDueTap, AnomalyType = e.AnomalyType,
                Charge = e.Charge, IsCopy = e.IsCopy, ReleaseMode = e.ReleaseMode, DisguiseLayers = e.DisguiseLayers,
                StabilizesSettling = e.StabilizesSettling,
                WaterLoad = e.WaterLoad, LoadAmount = e.LoadAmount, AcceptsCalibration = e.AcceptsCalibration,
                AcceptsLoad = e.AcceptsLoad, DischargeMode = e.DischargeMode, SoundEndpoint = e.SoundEndpoint,
                LastMoveSource = board.LastMoveSource(e.Id)
            };
            if (e.Goal != null) s.Goal = new SavedGoal
            {
                Id = e.Goal.Id, Kind = e.Goal.Kind, EntityId = e.Goal.EntityId, AnchorLabel = e.Goal.AnchorLabel,
                Required = e.Goal.Required, OrderLabels = new List<int>(e.Goal.OrderLabels)
            };
            if (e.RepairConfig != null)
            {
                s.RepairRequired = e.RepairConfig.RequiredProgress; s.RepairEnergyCost = e.RepairConfig.EnergyCost;
                s.RepairScore = e.RepairConfig.RepairScore;
                s.CompatibleParts.AddRange(e.RepairConfig.CompatibleParts);
            }
            foreach (BoardEntity stored in e.Stored) s.Stored.Add(CaptureEntity(stored, ids, depth + 1, board));
            return s;
        }

        private static BoardEntity RestoreEntity(SavedEntity s, HashSet<int> ids, int depth)
        {
            Require(s != null && depth <= 32 && ids.Count < 4096 && s.Id > 0 && s.Id < int.MaxValue
                && ids.Add(s.Id) && Enum.IsDefined(typeof(EntityKind), s.Kind)
                && Enum.IsDefined(typeof(PartType), s.PartType) && Enum.IsDefined(typeof(ObstacleType), s.ObstacleType)
                && Enum.IsDefined(typeof(AnomalyType), s.AnomalyType), "Invalid or duplicate entity identity.");
            Require(s.Energy >= 0 && s.EnergyCapacity >= 0 && s.Energy <= s.EnergyCapacity
                && s.Durability >= 0 && s.MaxDurability >= s.Durability && s.RepairProgress >= 0 && s.Charge >= 0 && s.DisguiseLayers >= 0
                && s.WaterLoad >= 0 && s.LoadAmount >= 0 && s.FuseDueTap >= 0
                && s.PassDirection >= -1 && s.PassDirection < 6 && s.Stored != null && s.LinkedElementIds != null
                && s.ReleaseWindowRemaining >= 0 && s.ClampedEntityId >= 0 && s.ClampedByEntityId >= 0
                && s.ReleasedEntityId >= 0 && s.GrowthRootEntityId >= 0 && s.ProtectedElementId >= 0, "Invalid entity state.");
            Require(string.IsNullOrEmpty(s.FormId) || (FormCatalog.Get(s.FormId) != null
                && FormCatalog.Get(s.FormId).HostPart == s.PartType), "Unknown or mismatched form.");
            Require(s.Kind == EntityKind.Facility || (s.ObstacleType == ObstacleType.None && s.AnomalyType == AnomalyType.None),
                "Mismatched facility state.");
            Require(s.Kind == EntityKind.TaskMarker || s.Goal == null, "Mismatched entity goal.");
            GoalConfig goal = null;
            if (s.Goal != null)
            {
                Require(Enum.IsDefined(typeof(GoalKind), s.Goal.Kind) && s.Goal.OrderLabels != null
                    && s.Goal.Required >= 0, "Invalid goal.");
                goal = new GoalConfig(s.Goal.Id, s.Goal.Kind, s.Goal.EntityId, s.Goal.AnchorLabel,
                    s.Goal.Required, s.Goal.OrderLabels);
            }
            BoardEntity entity;
            switch (s.Kind)
            {
                case EntityKind.Part:
                    Require(s.PartType == PartType.None || PartCatalog.Get(s.PartType) != null, "Unknown part.");
                    entity = BoardEntity.Part(s.Id, s.PartType, s.FormId);
                    Require(entity.EnergyCapacity == s.EnergyCapacity, "Part capacity changed.");
                    entity.SetEnergy(s.Energy); entity.SetFuseDueTap(s.FuseDueTap);
                    break;
                case EntityKind.RepairTarget:
                    Require(s.CompatibleParts != null && s.RepairRequired > 0 && s.RepairEnergyCost >= 0 && s.RepairScore >= 0, "Invalid repair config.");
                    foreach (PartType part in s.CompatibleParts)
                        Require(PartCatalog.Get(part) != null, "Unknown compatible part.");
                    entity = BoardEntity.RepairTarget(s.Id, new RepairTargetConfig(s.CompatibleParts, s.RepairRequired, s.RepairEnergyCost, s.RepairScore));
                    Require(s.RepairProgress <= s.RepairRequired && s.RepairCompleted == (s.RepairProgress >= s.RepairRequired), "Invalid repair progress.");
                    break;
                case EntityKind.TaskMarker:
                    entity = s.Movable ? BoardEntity.MovableTaskMarker(s.Id, goal) : BoardEntity.TaskMarker(s.Id, goal);
                    break;
                case EntityKind.Facility:
                    if (s.ObstacleType != ObstacleType.None)
                    {
                        ObstacleConfig config = ObstacleCatalog.Get(s.ObstacleType);
                        bool segment = s.ObstacleType == ObstacleType.ProliferatingSeal && s.GrowthRootEntityId != s.Id;
                        Require(config != null && s.MaxDurability == (segment ? config.SegmentMaxDurability : config.MaxDurability), "Unknown obstacle or durability.");
                        RepairTargetConfig repair = null;
                        if (s.RepairRequired != 0)
                        {
                            Require(s.CompatibleParts != null && s.RepairRequired > 0 && s.RepairEnergyCost >= 0 && s.RepairScore >= 0, "Invalid obstacle repair config.");
                            foreach (PartType part in s.CompatibleParts) Require(PartCatalog.Get(part) != null, "Unknown obstacle compatible part.");
                            repair = new RepairTargetConfig(s.CompatibleParts, s.RepairRequired, s.RepairEnergyCost, s.RepairScore);
                        }
                        entity = segment ? BoardEntity.SealSegment(s.Id, config, s.GrowthRootEntityId) : BoardEntity.Obstacle(s.Id, config, repair);
                    }
                    else entity = s.AnomalyType != AnomalyType.None ? BoardEntity.Anomaly(s.Id, s.AnomalyType) : BoardEntity.Facility(s.Id);
                    break;
                default: throw new ArgumentException("Unsupported saved entity kind.");
            }
            Require(s.Movable == !entity.IsFixed, "Entity movement category changed.");
            entity.RestoreState(s.Durability, s.IsOpen, s.PassDirection, s.ClampedEntityId, s.SealActive, s.RepairProgress, s.RepairCompleted);
            entity.ClampedByEntityId = s.ClampedByEntityId; entity.PendingClose = s.PendingClose;
            entity.ReleaseWindowRemaining = s.ReleaseWindowRemaining; entity.ReleasedEntityId = s.ReleasedEntityId;
            entity.GrowthRootEntityId = s.GrowthRootEntityId; entity.ProtectedElementId = s.ProtectedElementId;
            entity.LinkedElementIds.AddRange(s.LinkedElementIds);
            entity.Coord = s.Coord;
            entity.SetLocked(s.IsLocked); entity.AddCharge(s.Charge);
            if (s.IsCopy) entity.SetCopy();
            entity.ReleaseMode = s.ReleaseMode; entity.DischargeMode = s.DischargeMode;
            entity.SetDisguiseLayers(s.DisguiseLayers); entity.AddWater(s.WaterLoad); entity.AddLoad(s.LoadAmount);
            if (s.AcceptsCalibration) entity.SetAcceptsCalibration();
            if (s.StabilizesSettling) entity.SetSettlingStabilizer();
            if (s.AcceptsLoad) entity.SetAcceptsLoad();
            entity.SetSoundEndpoint(s.SoundEndpoint);
            foreach (SavedEntity stored in s.Stored) entity.Store(RestoreEntity(stored, ids, depth + 1));
            return entity;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new ArgumentException(message);
        }

        private static void ValidateRelations(BoardState board)
        {
            foreach (BoardEntity entity in board.Entities)
            {
                if (entity.PendingClose) Require(entity.ObstacleType == ObstacleType.LinkedGate && entity.IsOpen
                    && board.OccupantAt(entity.Coord) != null, "Invalid pending gate close.");
                if (entity.ClampedEntityId != 0)
                {
                    Require(entity.ObstacleType == ObstacleType.ClampingSeat
                        && board.TryGetEntity(entity.ClampedEntityId, out BoardEntity target)
                        && target.Coord == entity.Coord && !target.IsFixed && target.ClampedByEntityId == entity.Id
                        && entity.ReleaseWindowRemaining == 0
                        && entity.ObstacleSetup.AllowsTag(target.Kind == EntityKind.Part ? "normal" : "task"),
                        "Invalid clamp reference.");
                }
                if (entity.ClampedByEntityId != 0)
                    Require(board.TryGetEntity(entity.ClampedByEntityId, out BoardEntity seat)
                        && seat.ObstacleType == ObstacleType.ClampingSeat && seat.ClampedEntityId == entity.Id
                        && seat.Coord == entity.Coord, "Invalid reciprocal clamp reference.");
                if (entity.ReleaseWindowRemaining != 0 || entity.ReleasedEntityId != 0)
                    Require(entity.ObstacleType == ObstacleType.ClampingSeat
                        && entity.ReleaseWindowRemaining <= entity.ObstacleSetup.ReleaseWindowTaps
                        && (entity.ReleasedEntityId == 0 || board.TryGetEntity(entity.ReleasedEntityId, out BoardEntity released)
                            && released.Coord == entity.Coord), "Invalid clamp release state.");
                if (entity.ObstacleType == ObstacleType.ProliferatingSeal)
                {
                    Require(entity.GrowthRootEntityId > 0 && entity.GrowthRootEntityId < board.NextEntityId,
                        "Invalid seal root ID.");
                    if (board.TryGetEntity(entity.GrowthRootEntityId, out BoardEntity root))
                        Require(root.ObstacleType == ObstacleType.ProliferatingSeal && root.GrowthRootEntityId == root.Id,
                            "Invalid seal root relation.");
                    if (entity.GrowthRootEntityId != entity.Id) Require(!entity.SealActive, "Seal segment cannot be an active root.");
                }
                else Require(entity.GrowthRootEntityId == 0, "Unexpected seal relation.");
                var linked = new HashSet<int>();
                foreach (int id in entity.LinkedElementIds)
                {
                    Require(id > 0 && id < board.NextEntityId && id != entity.Id && linked.Add(id), "Invalid linked element ID.");
                    if (entity.ObstacleType == ObstacleType.PressurePillar && board.TryGetEntity(id, out BoardEntity attached))
                        Require(attached.ObstacleType == ObstacleType.BrittlePartition, "Invalid pillar attachment type.");
                }
                if (entity.ProtectedElementId != 0)
                    Require(entity.ObstacleType == ObstacleType.PressurePillar && entity.ObstacleSetup.ProtectedObject
                        && entity.ProtectedElementId > 0 && entity.ProtectedElementId < board.NextEntityId
                        && entity.ProtectedElementId != entity.Id, "Invalid protected element reference.");
            }
        }
    }
}
