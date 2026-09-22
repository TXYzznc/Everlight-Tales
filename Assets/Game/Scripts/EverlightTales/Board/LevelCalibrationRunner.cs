using System;
using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>批量演算统计报告（P2-017）。</summary>
    public sealed class CalibrationReport
    {
        /// <summary>总种子数。</summary>
        public int TotalSeeds;

        /// <summary>可解（存在某相位能过关）种子数。</summary>
        public int SolvedSeeds;

        /// <summary>每轮拍数上限。</summary>
        public int TapCount;

        /// <summary>拍数分布：索引 0=未过关种子数，1..TapCount=第 N 拍过关的种子数。</summary>
        public int[] TapDistribution;

        /// <summary>过关拍数统计（仅已解种子）。</summary>
        public int MinTaps;

        /// <summary>过关拍数统计（仅已解种子）。</summary>
        public int MaxTaps;

        /// <summary>过关拍数累计（仅已解种子）。</summary>
        public int TotalTaps;

        /// <summary>通过率 = 可解种子 / 总种子。</summary>
        public double PassRate => TotalSeeds == 0 ? 0 : SolvedSeeds / (double)TotalSeeds;

        /// <summary>平均过关拍数（仅已解种子）。</summary>
        public double AvgTaps => SolvedSeeds == 0 ? 0 : TotalTaps / (double)SolvedSeeds;
    }

    /// <summary>
    /// 关卡演算校准工具（P2-017）：批量跑种子统计通过率/拍数分布，供关卡校准。
    /// 策略：枚举 6 个重力相位 × 纯拍击，每个相位从同种子初盘独立演算；
    /// 可选 setup 在拍击前对盘面做预置（如机械臂搬动），供初盘相邻、需先调整的关卡（如卷帘门）。
    /// 通过 = 累计分达标 + 全特殊目标 + 事件专属目标。纯逻辑、零引擎、同种子复现。
    /// </summary>
    public static class LevelCalibrationRunner
    {
        /// <summary>
        /// 批量演算。
        /// </summary>
        /// <param name="boardConfig">盘面配置（含关键件/携带池/固定元素/初盘件数）。</param>
        /// <param name="levelConfig">关卡配置（取首轮拍数与目标分）。</param>
        /// <param name="selectedCarry">携带池（随机件种类，关键件由 boardConfig.KeyPieces 单独放置）。</param>
        /// <param name="specialGoalMet">事件专属目标判定（可为 null=无专属目标）。</param>
        /// <param name="setup">可选：每个 (种子, 相位) 拍击前对盘面预置（如机械臂搬动）。</param>
        /// <param name="seedStart">起始种子。</param>
        /// <param name="seedCount">种子数量。</param>
        public static CalibrationReport Run(
            LevelBoardConfig boardConfig,
            LevelConfig levelConfig,
            IReadOnlyList<PartType> selectedCarry,
            Func<BoardState, bool> specialGoalMet,
            Action<BoardState, SessionState> setup,
            int seedStart,
            int seedCount)
        {
            if (boardConfig == null)
            {
                throw new ArgumentNullException(nameof(boardConfig));
            }

            if (levelConfig == null)
            {
                throw new ArgumentNullException(nameof(levelConfig));
            }

            RoundConfig round = levelConfig.Rounds[0];
            var report = new CalibrationReport
            {
                TotalSeeds = seedCount,
                TapCount = round.TapCount,
                TapDistribution = new int[round.TapCount + 1],
                MinTaps = int.MaxValue,
            };

            for (int seed = seedStart; seed < seedStart + seedCount; seed++)
            {
                int minTaps = int.MaxValue;
                for (int phase = 0; phase < HexDirections.Count; phase++)
                {
                    int taps = SimulatePhase(
                        boardConfig, levelConfig, selectedCarry, specialGoalMet, setup,
                        seed, HexDirections.All[phase]);
                    if (taps > 0 && taps < minTaps)
                    {
                        minTaps = taps;
                    }
                }

                if (minTaps == int.MaxValue)
                {
                    report.TapDistribution[0]++; // 未过关。
                }
                else
                {
                    report.SolvedSeeds++;
                    report.TapDistribution[minTaps]++;
                    report.TotalTaps += minTaps;
                    if (minTaps < report.MinTaps)
                    {
                        report.MinTaps = minTaps;
                    }

                    if (minTaps > report.MaxTaps)
                    {
                        report.MaxTaps = minTaps;
                    }
                }
            }

            if (report.SolvedSeeds == 0)
            {
                report.MinTaps = 0;
            }

            return report;
        }

        /// <summary>对单个种子 + 单个重力相位，从初盘独立演算到过关或拍数耗尽，返回过关拍数（0=未过）。</summary>
        public static int SimulatePhase(
            LevelBoardConfig boardConfig,
            LevelConfig levelConfig,
            IReadOnlyList<PartType> selectedCarry,
            Func<BoardState, bool> specialGoalMet,
            Action<BoardState, SessionState> setup,
            int seed,
            HexDirection gravity)
        {
            var rng = new RandomService(seed);
            BoardState board = InitialBoardBuilder.Build(boardConfig, selectedCarry, rng);

            var level = new LevelState(levelConfig);
            level.BeginLevel();
            SessionState session = level.Session;
            session.ArmMoves = boardConfig.InitialArmMoves;

            setup?.Invoke(board, session);

            var settle = new SettleState(gravity);
            var game = new BoardGame(board, settle, session, level);

            int tapCount = levelConfig.Rounds[0].TapCount;
            for (int tap = 1; tap <= tapCount; tap++)
            {
                game.Tap();

                bool scoreMet = session.Score >= level.Round.TargetScore;
                bool goalsMet = level.Round.AllGoalsComplete;
                bool specialMet = specialGoalMet == null || specialGoalMet(board);
                if (scoreMet && goalsMet && specialMet)
                {
                    return tap;
                }
            }

            return 0;
        }
    }
}
