using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>红舞鞋单步结果（P2-013）。</summary>
    public enum RedShoeStepOutcome : byte
    {
        /// <summary>已封存／已移除，步进为空操作。</summary>
        None = 0,

        /// <summary>普通步进（未命中转向／导流／封存口）。</summary>
        Moved = 1,

        /// <summary>进入转向格并切换舞步方向。</summary>
        Turned = 2,

        /// <summary>进入尚未计入的导流格，导流 +1。</summary>
        Diverted = 3,

        /// <summary>导流达标，当场解除穿着者束缚（分离）。</summary>
        Separated = 4,

        /// <summary>进入封存口完成封存。</summary>
        Sealed = 5,

        /// <summary>封存口未开启，被挡回上一格。</summary>
        EntryBlocked = 6,

        /// <summary>前方受阻（占用或出界），步进消耗。</summary>
        Blocked = 7,

        /// <summary>出界（前方无合法格）。</summary>
        OffBoard = 8,
    }

    /// <summary>方向转向器格（LR-RS-02）：红鞋进入后切换到标记方向。</summary>
    public sealed class RedShoeTurnCell
    {
        public HexCoord Coord;
        public HexDirection Direction;

        public RedShoeTurnCell(HexCoord coord, HexDirection direction)
        {
            Coord = coord;
            Direction = direction;
        }
    }

    /// <summary>
    /// 红舞鞋专属规则配置（P2-013）：起点、舞步方向、导流格、转向格、封存匣与封存口。
    /// 纯配置，不引用引擎。
    /// </summary>
    public sealed class RedShoeConfig
    {
        public HexCoord StartCoord;
        public HexDirection StartDirection;
        public IReadOnlyList<HexCoord> GuideCells;
        public IReadOnlyList<RedShoeTurnCell> TurnCells;
        public HexCoord BoxCoord;
        public int BoxRequired;
        public HexCoord EntryCoord;
        public int RequiredDiversion;
        public int RestraintLimit;
        public int StepsPerTap;

        public RedShoeConfig(
            HexCoord startCoord,
            HexDirection startDirection,
            IReadOnlyList<HexCoord> guideCells,
            IReadOnlyList<RedShoeTurnCell> turnCells,
            HexCoord boxCoord,
            int boxRequired,
            HexCoord entryCoord,
            int requiredDiversion,
            int restraintLimit,
            int stepsPerTap = 1)
        {
            StartCoord = startCoord;
            StartDirection = startDirection;
            GuideCells = guideCells ?? System.Array.Empty<HexCoord>();
            TurnCells = turnCells ?? System.Array.Empty<RedShoeTurnCell>();
            BoxCoord = boxCoord;
            BoxRequired = boxRequired;
            EntryCoord = entryCoord;
            RequiredDiversion = requiredDiversion;
            RestraintLimit = restraintLimit;
            StepsPerTap = stepsPerTap;
        }
    }

    /// <summary>红舞鞋单步结果。</summary>
    public sealed class RedShoeStepResult
    {
        public RedShoeStepOutcome Outcome;
        public HexCoord From;
        public HexCoord To;
        public bool RestraintIncreased;
        public bool DiversionIncreased;
        public bool DirectionChanged;
    }

    /// <summary>红舞鞋运行时状态（P2-013）：位置、舞步方向、束缚、导流进度、负荷与封存。</summary>
    public sealed class RedShoeState
    {
        public HexCoord Coord;
        public HexDirection Direction;
        public bool IsBound = true;
        public int DiversionProgress;
        public int RestraintLoad;
        public bool IsSealed;
        public bool IsRemoved;
        public int RestraintLimit;

        private readonly HashSet<HexCoord> _countedGuides = new HashSet<HexCoord>();

        public RedShoeState(HexCoord coord, HexDirection direction, int restraintLimit)
        {
            Coord = coord;
            Direction = direction;
            RestraintLimit = restraintLimit;
        }

        /// <summary>分离前束缚达到上限即失败。</summary>
        public bool HasFailed => IsBound && RestraintLoad >= RestraintLimit;

        /// <summary>该导流格是否已计入（已计入不重复计数）。</summary>
        public bool HasCountedGuide(HexCoord coord)
        {
            return _countedGuides.Contains(coord);
        }

        internal bool MarkGuide(HexCoord coord)
        {
            return _countedGuides.Add(coord);
        }
    }

    /// <summary>
    /// 红舞鞋规则步进解释器（P2-013）：LR-RS-01 持续运动、LR-RS-02 借力转向、
    /// LR-RS-03 把舞步引走、LR-RS-04 鞋还在人先离开。纯逻辑、不引用引擎。
    /// </summary>
    public static class RedShoeService
    {
        public static RedShoeStepResult StepOnce(BoardState board, RedShoeConfig config, RedShoeState state)
        {
            var result = new RedShoeStepResult { From = state.Coord, To = state.Coord };
            if (state.IsSealed || state.IsRemoved)
            {
                return result;
            }

            HexCoord target = state.Coord.Neighbor(state.Direction);
            result.To = target;

            if (!board.IsValid(target))
            {
                result.Outcome = RedShoeStepOutcome.OffBoard;
                AddRestraint(state, result);
                return result;
            }

            if (board.IsOccupied(target))
            {
                result.Outcome = RedShoeStepOutcome.Blocked;
                AddRestraint(state, result);
                return result;
            }

            state.Coord = target;

            // LR-RS-02：进入转向格，读取该格方向切换后续舞步方向。
            RedShoeTurnCell turn = FindTurn(config, target);
            if (turn != null)
            {
                state.Direction = turn.Direction;
                result.DirectionChanged = true;
                result.Outcome = RedShoeStepOutcome.Turned;
                return result;
            }

            // LR-RS-03：规则步进实际经过尚未计入的导流格时，导流 +1。
            if (IsGuide(config, target) && state.MarkGuide(target))
            {
                state.DiversionProgress++;
                result.DiversionIncreased = true;
                if (state.IsBound && state.DiversionProgress >= config.RequiredDiversion)
                {
                    state.IsBound = false; // LR-RS-04：导流达标，当场分离穿着者。
                    result.Outcome = RedShoeStepOutcome.Separated;
                }
                else
                {
                    result.Outcome = RedShoeStepOutcome.Diverted;
                }

                return result;
            }

            // LR-RS-04：封存口仅在分离后且封存匣修好时接纳；否则挡回上一格。
            if (target == config.EntryCoord)
            {
                if (!state.IsBound && IsBoxRepaired(board, config))
                {
                    state.IsSealed = true;
                    state.IsRemoved = true;
                    result.Outcome = RedShoeStepOutcome.Sealed;
                }
                else
                {
                    state.Coord = result.From;
                    result.To = result.From;
                    result.Outcome = RedShoeStepOutcome.EntryBlocked;
                    AddRestraint(state, result); // 分离前束缚 +1，分离后内部跳过。
                }

                return result;
            }

            result.Outcome = RedShoeStepOutcome.Moved;
            return result;
        }

        /// <summary>每次拍击后按 StepsPerTap 逐次规则步进；封存后停止。</summary>
        public static List<RedShoeStepResult> StepAfterTap(BoardState board, RedShoeConfig config, RedShoeState state)
        {
            var results = new List<RedShoeStepResult>();
            for (int i = 0; i < config.StepsPerTap; i++)
            {
                results.Add(StepOnce(board, config, state));
                if (state.IsSealed || state.IsRemoved)
                {
                    break;
                }
            }

            return results;
        }

        /// <summary>封存匣是否已修好（达到要求进度）。</summary>
        public static bool IsBoxRepaired(BoardState board, RedShoeConfig config)
        {
            BoardEntity box = board.EntityAt(config.BoxCoord);
            return box != null && box.Kind == EntityKind.RepairTarget && box.RepairCompleted;
        }

        private static void AddRestraint(RedShoeState state, RedShoeStepResult result)
        {
            if (!state.IsBound)
            {
                return; // 分离后负荷不再伤及穿着者（LR-RS-01）。
            }

            state.RestraintLoad++;
            result.RestraintIncreased = true;
        }

        private static RedShoeTurnCell FindTurn(RedShoeConfig config, HexCoord coord)
        {
            foreach (RedShoeTurnCell turn in config.TurnCells)
            {
                if (turn.Coord == coord)
                {
                    return turn;
                }
            }

            return null;
        }

        private static bool IsGuide(RedShoeConfig config, HexCoord coord)
        {
            foreach (HexCoord guide in config.GuideCells)
            {
                if (guide == coord)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
