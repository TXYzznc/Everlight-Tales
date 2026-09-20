using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>单步演算记录：动作描述与该步随机消费数。</summary>
    public sealed class BoardLogStep
    {
        public BoardLogStep(int index, string action, int randomConsumed)
        {
            Index = index;
            Action = action;
            RandomConsumed = randomConsumed;
        }

        public int Index { get; }
        public string Action { get; }
        public int RandomConsumed { get; }
    }

    /// <summary>一次演算的完整记录：种子、初始盘面与每步动作及随机消费数。</summary>
    public sealed class BoardLogRecord
    {
        public BoardLogRecord(int seed, string initialBoard)
        {
            Seed = seed;
            InitialBoard = initialBoard;
        }

        public int Seed { get; }
        public string InitialBoard { get; }
        public List<BoardLogStep> Steps { get; } = new List<BoardLogStep>();

        public int TotalRandomConsumed
        {
            get
            {
                int total = 0;
                foreach (BoardLogStep step in Steps)
                {
                    total += step.RandomConsumed;
                }
                return total;
            }
        }
    }

    /// <summary>盘面演算日志开关（零引擎）：关闭时零开销，不分配、不拼接字符串。</summary>
    public static class BoardLog
    {
        private static bool s_Enabled;
        private static BoardLogRecord s_Current;

        public static bool Enabled => s_Enabled;

        public static void SetEnabled(bool enabled)
        {
            s_Enabled = enabled;
            if (!enabled)
            {
                s_Current = null;
            }
        }

        /// <summary>开始一次演算记录；关闭时直接返回，不分配。</summary>
        public static void Begin(int seed, string initialBoard)
        {
            if (!s_Enabled)
            {
                return;
            }
            s_Current = new BoardLogRecord(seed, initialBoard);
        }

        /// <summary>记录一步演算动作及其随机消费数；关闭时直接返回。</summary>
        public static void Step(string action, int randomConsumed)
        {
            if (!s_Enabled || s_Current == null)
            {
                return;
            }
            s_Current.Steps.Add(new BoardLogStep(s_Current.Steps.Count, action, randomConsumed));
        }

        /// <summary>结束记录并返回；关闭或未开始时返回 null。</summary>
        public static BoardLogRecord End()
        {
            BoardLogRecord record = s_Current;
            s_Current = null;
            return record;
        }
    }

    /// <summary>按记录重放：以记录的种子重建随机源并按每步消费数重放，返回 FNV-1a 校验和（供断言同记录→同结果）。</summary>
    public static class BoardReplay
    {
        public static long ReplayChecksum(BoardLogRecord record)
        {
            if (record == null)
            {
                return 0L;
            }

            RandomService rng = new RandomService(record.Seed);
            long checksum = 1469598103934665603L; // FNV-1a offset basis
            foreach (BoardLogStep step in record.Steps)
            {
                for (int i = 0; i < step.RandomConsumed; i++)
                {
                    checksum ^= rng.NextInt(0, int.MaxValue);
                    checksum *= 1099511628211L; // FNV-1a prime
                }
            }
            return checksum;
        }
    }
}
