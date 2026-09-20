using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 棋盘异常区域（P2-003）：区域格集合 + 配置 + 运行时状态。
    /// 区域由关卡实例注册到 BoardState；触发判定由 AnomalyService 按类型解释。
    /// </summary>
    public sealed class AnomalyRegion
    {
        public AnomalyType Type { get; }

        public AnomalyConfig Config { get; }

        /// <summary>区域入口格集合。</summary>
        public IReadOnlyList<HexCoord> Cells { get; }

        /// <summary>A-002 登记出口格。</summary>
        public HexCoord ExitCell { get; }

        /// <summary>A-002 扩张格序列。</summary>
        public IReadOnlyList<HexCoord> ExpansionCells { get; }

        /// <summary>A-001 偏转是否生效。</summary>
        public bool DeflectionActive { get; set; } = true;

        /// <summary>A-004 静默是否生效（A-006 芯耗尽后转静默）。</summary>
        public bool Silent { get; set; }

        /// <summary>A-002 成功传送次数。</summary>
        public int UseCount { get; private set; }

        /// <summary>A-002 已扩张格数。</summary>
        public int ExpansionCount { get; private set; }

        /// <summary>A-006 场热。</summary>
        public int Heat { get; private set; }

        /// <summary>A-006 过载芯剩余。</summary>
        public int Cores { get; private set; }

        /// <summary>A-008 拍计数。</summary>
        public int TapCounter { get; private set; }

        public AnomalyRegion(AnomalyType type, IReadOnlyList<HexCoord> cells, HexCoord exitCell = default, IReadOnlyList<HexCoord> expansionCells = null)
        {
            Type = type;
            Config = AnomalyCatalog.Get(type);
            Cells = cells ?? System.Array.Empty<HexCoord>();
            ExitCell = exitCell;
            ExpansionCells = expansionCells ?? System.Array.Empty<HexCoord>();
            Cores = Config == null ? 0 : Config.CoreCount;
        }

        /// <summary>坐标是否在当前入口范围内（含已扩张格）。</summary>
        public bool TryEnter(HexCoord coord)
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i] == coord)
                {
                    return true;
                }
            }

            for (int i = 0; i < ExpansionCount && i < ExpansionCells.Count; i++)
            {
                if (ExpansionCells[i] == coord)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>记录一次成功传送并按阈值扩张。</summary>
        public void RecordUse()
        {
            UseCount++;
            if (Config != null && UseCount >= Config.Threshold && ExpansionCount < ExpansionCells.Count)
            {
                ExpansionCount++;
            }
        }

        /// <summary>场热 +1。</summary>
        public void AddHeat()
        {
            Heat++;
        }

        /// <summary>场热达到阈值则触发一次过载（扣热、补能、耗芯），返回是否过载。</summary>
        public bool TryOverload()
        {
            if (Config == null || Heat < Config.Threshold)
            {
                return false;
            }

            Heat = 0;
            Cores--;
            if (Cores <= 0)
            {
                Silent = true; // 芯耗尽转静默（语义引用 A-004）。
            }

            return true;
        }

        /// <summary>拍计数 +1，达周期即应反转（A-008）。</summary>
        public bool TickTap()
        {
            TapCounter++;
            return Config != null && TapCounter >= Config.TapPeriod;
        }
    }

    /// <summary>棋盘异常触发解释器（P2-003）：按区域类型解释定势偏转、静默、传送、沉降。</summary>
    public static class AnomalyService
    {
        /// <summary>A-001：区域内局部定势 = 全盘定势偏转 +1（可开关）。</summary>
        public static HexDirection LocalSettle(AnomalyRegion region, HexDirection global)
        {
            if (region != null && region.Type == AnomalyType.LocalSettleDeflection && region.DeflectionActive)
            {
                return HexDirections.Rotate(global, region.Config == null ? 1 : region.Config.Deflection);
            }

            return global;
        }

        /// <summary>A-004：区域内能力源被抑制（静默生效且坐标在区域内）。</summary>
        public static bool IsSilenced(AnomalyRegion region, HexCoord coord)
        {
            return region != null && region.Silent && region.TryEnter(coord);
        }

        /// <summary>A-005：区域内沉降方向。</summary>
        public static HexDirection SettleDirection(AnomalyRegion region)
        {
            return region != null && region.Type == AnomalyType.SettlingRail && region.Config != null
                ? HexDirections.FromIndex(region.Config.SettleDirection)
                : default;
        }

        /// <summary>A-002：实体从入口区原子传送到出口格（出口空才成功，成功计一次使用）。</summary>
        public static bool TryTeleport(BoardState board, AnomalyRegion region, BoardEntity entity, out HexCoord to)
        {
            to = default;
            if (region == null || region.Type != AnomalyType.ExpandingRift || entity == null)
            {
                return false;
            }

            if (!region.TryEnter(entity.Coord) || board.IsOccupied(region.ExitCell))
            {
                return false;
            }

            if (board.Move(entity, region.ExitCell) != MoveResult.Ok)
            {
                return false;
            }

            to = region.ExitCell;
            region.RecordUse();
            return true;
        }
    }
}
