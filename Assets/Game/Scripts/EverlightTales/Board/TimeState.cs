using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>时间推进结果。</summary>
    public sealed class TimeAdvanceResult
    {
        public TimeOfDay FromPeriod;
        public TimeOfDay ToPeriod;
        public int FromDay;
        public int ToDay;
        public int FromRemaining;
        public int ToRemaining;
        public bool CrossedPeriod;
        public bool CrossedDay;
    }

    /// <summary>
    /// 游戏时间状态（P2-011）：一天四时段，每段固定格数，结算事务按完整耗时一次性推进。
    /// 恰好用完一段进入下一段，深夜之后进入次日上午。纯逻辑、零引擎。
    /// </summary>
    public sealed class TimeState
    {
        public const int CellsPerPeriod = 4;

        public int Day { get; private set; }

        public TimeOfDay Period { get; private set; }

        public int RemainingCells { get; private set; }

        public TimeState(int day = 1, TimeOfDay period = TimeOfDay.Morning, int remainingCells = CellsPerPeriod)
        {
            Day = day;
            Period = period;
            RemainingCells = remainingCells;
        }

        /// <summary>一次性推进 N 格；返回跨段/跨日信息。</summary>
        public TimeAdvanceResult Advance(int cells)
        {
            var result = new TimeAdvanceResult
            {
                FromPeriod = Period,
                FromDay = Day,
                FromRemaining = RemainingCells,
            };

            RemainingCells -= cells;
            while (RemainingCells <= 0)
            {
                RemainingCells += CellsPerPeriod;
                if (Period == TimeOfDay.DeepNight)
                {
                    Period = TimeOfDay.Morning;
                    Day++;
                    result.CrossedDay = true;
                }
                else
                {
                    Period = (TimeOfDay)((int)Period + 1);
                }

                result.CrossedPeriod = true;
            }

            result.ToPeriod = Period;
            result.ToDay = Day;
            result.ToRemaining = RemainingCells;
            return result;
        }
    }
}
