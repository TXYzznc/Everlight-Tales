using System;
using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    public sealed partial class AnomalyRegion
    {
        public int Id { get; internal set; }
        public int EchoRemaining { get; internal set; }
        public int RedrawCursor { get; internal set; }
        public int LastRedrawRound { get; internal set; } = -1;
        public bool RedrawStopped { get; internal set; }
        public bool SettlingStopped { get; internal set; }
        public bool FreezeNextReversal { get; internal set; }
        private readonly List<int> _carriers = new List<int>();
        public IReadOnlyList<int> CarrierIds => _carriers;

        public void RegisterCarrier(BoardState board, int entityId)
        {
            if (Type != AnomalyType.RedrawCurtain || !board.TryGetEntity(entityId, out BoardEntity entity)
                || (entity.Kind != EntityKind.Part && entity.Kind != EntityKind.TaskMarker) || _carriers.Contains(entityId))
                throw new ArgumentException("Invalid or duplicate disguise carrier.");
            _carriers.Add(entityId);
        }
        internal void RestoreCarriers(IReadOnlyList<int> ids, int nextEntityId)
        {
            if (ids == null || (Type != AnomalyType.RedrawCurtain && ids.Count != 0)) throw new ArgumentException("Invalid carriers.");
            foreach (int id in ids)
            {
                if (id <= 0 || id >= nextEntityId || _carriers.Contains(id)) throw new ArgumentException("Invalid carrier ID.");
                _carriers.Add(id);
            }
            if (RedrawCursor < 0 || (_carriers.Count == 0 ? RedrawCursor != 0 : RedrawCursor >= _carriers.Count))
                throw new ArgumentException("Invalid redraw cursor.");
        }
    }

    public static partial class AnomalyService
    {
        /// <summary>重叠只选择最高优先、最早实例；停用或耗尽不回退到次选区域。</summary>
        public static AnomalyRegion Select(BoardState board, AnomalyType type, HexCoord at)
        {
            AnomalyRegion selected = null;
            foreach (AnomalyRegion region in board.AnomalyRegions)
                if (region.Type == type && region.TryEnter(at) && (selected == null
                    || region.Config.Priority > selected.Config.Priority
                    || region.Config.Priority == selected.Config.Priority && region.Id < selected.Id)) selected = region;
            return selected;
        }
        public static HexDirection GravityAt(BoardState board, HexCoord at, HexDirection global) =>
            LocalSettle(Select(board, AnomalyType.LocalSettleDeflection, at), global);
        public static bool IsSilenced(BoardState board, HexCoord coord, bool storyNode = false)
        {
            foreach (AnomalyRegion region in board.AnomalyRegions)
                if ((region.Type == AnomalyType.SilenceZone || region.Type == AnomalyType.OverloadField)
                    && IsSilenced(region, coord) && (!storyNode || region.Config.SuppressStoryNodes)) return true;
            return false;
        }
        public static bool ResetEcho(AnomalyRegion region)
        {
            if (region == null || region.Type != AnomalyType.EchoZone || !region.Config.Resettable) return false;
            region.EchoRemaining = region.Config.EchoQuota; return true;
        }
        public static bool Stabilize(AnomalyRegion region)
        {
            if (region == null || region.Type != AnomalyType.SettlingRail || region.SettlingStopped) return false;
            region.SettlingStopped = true; return true;
        }
        public static bool CalibrateAt(BoardState board, HexCoord at)
        {
            bool changed = false;
            foreach (AnomalyRegion region in board.AnomalyRegions)
            {
                if (!region.TryEnter(at)) continue;
                switch (region.Type)
                {
                    case AnomalyType.LocalSettleDeflection:
                        if (region.Config.Controllable) { region.DeflectionActive = !region.DeflectionActive; changed = true; } break;
                    case AnomalyType.SilenceZone:
                        if (region.Config.Controllable) { region.Silent = !region.Silent; changed = true; } break;
                    case AnomalyType.OverloadField:
                        if (!region.Silent) { region.Silent = true; changed = true; } break;
                    case AnomalyType.RedrawCurtain:
                        if (!region.RedrawStopped) { region.RedrawStopped = true; changed = true; } break;
                    case AnomalyType.PostTapReversal:
                        if (!region.FreezeNextReversal) { region.FreezeNextReversal = true; changed = true; } break;
                }
            }
            if (changed) board.MarkTraversalChanged();
            return changed;
        }
        public static void RecordAbility(SettlementContext context, BoardEntity source)
        {
            if (source.EnergyCapacity <= 0) return;
            foreach (AnomalyRegion region in context.Board.AnomalyRegions)
            {
                if (region.Type != AnomalyType.OverloadField || !region.TryEnter(source.Coord) || region.Silent) continue;
                region.AddHeat(); if (!region.TryOverload()) continue;
                foreach (BoardEntity entity in context.Board.Entities)
                    if (entity.Kind == EntityKind.Part && entity.EnergyCapacity > 0 && region.TryEnter(entity.Coord))
                        entity.SetEnergy(entity.Energy + region.Config.RechargeAmount);
                context.Log.Add(new SettlementEvent(SettlementEventKind.AnomalyApplied, source.Id, message: "overload#" + region.Id,
                    provenance: context.CurrentEffect));
            }
        }
        public static void ApplyRoundEnd(BoardState board, int roundIndex)
        {
            foreach (AnomalyRegion region in board.AnomalyRegions)
            {
                if (region.Type != AnomalyType.RedrawCurtain || region.LastRedrawRound >= roundIndex) continue;
                region.LastRedrawRound = roundIndex;
                if (region.RedrawStopped || region.CarrierIds.Count == 0) continue;
                int scanned = 0, changed = 0, count = region.CarrierIds.Count;
                while (scanned++ < count && changed < region.Config.PerRoundTargets)
                {
                    int id = region.CarrierIds[region.RedrawCursor]; region.RedrawCursor = (region.RedrawCursor + 1) % count;
                    if (!board.TryGetEntity(id, out BoardEntity entity) || !region.TryEnter(entity.Coord)
                        || entity.DisguiseLayers >= region.Config.MaxLayers) continue;
                    entity.SetDisguiseLayers(entity.DisguiseLayers + 1); changed++;
                }
            }
        }
    }
}
