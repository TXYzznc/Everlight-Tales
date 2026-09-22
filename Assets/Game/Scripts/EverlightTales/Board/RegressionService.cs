using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>一次确定性盘面演算结果（P6-003）。</summary>
    public sealed class RegressionResult
    {
        public string SampleId;
        public int Seed;
        public int Score;
        public bool Pass;
    }

    /// <summary>回归聚合报告（P6-003）：通过率 + 分数分布。</summary>
    public sealed class RegressionReport
    {
        public int TotalRuns;
        public int PassCount;
        public int ScoreMin;
        public int ScoreMax;
        public int ScoreSum;

        public double PassRate => TotalRuns > 0 ? (double)PassCount / TotalRuns : 0.0;

        public double ScoreAvg => TotalRuns > 0 ? (double)ScoreSum / TotalRuns : 0.0;
    }

    /// <summary>
    /// 全案回归统计（P6-003）：关卡/盘面确定性演算（同种子同序列 → 同分数）与通过率/分数分布聚合。
    /// 纯逻辑、零引擎（Board refs Data），供离线回归与工作台回归共用。
    /// </summary>
    public static class RegressionService
    {
        /// <summary>
        /// 跑一个盘面：携带选择 + 初盘生成 + taps 次拍击（默认重力 D0）。
        /// 返回累计分与达标判定（分数达标且全部特殊目标完成）。
        /// </summary>
        public static RegressionResult Simulate(
            string sampleId,
            LevelBoardConfig boardConfig,
            LevelConfig levelConfig,
            IReadOnlyList<PartType> ownedParts,
            int seed,
            int taps)
        {
            var rng = new RandomService(seed);
            var keyParts = new List<PartType>();
            foreach (KeyPieceConfig key in boardConfig.KeyPieces)
            {
                keyParts.Add(key.PartType);
            }

            CarrySelection carry = new CarrySelection(ownedParts, boardConfig.BorrowedParts, keyParts);
            BoardState board = InitialBoardBuilder.Build(boardConfig, carry.SelectedParts(), rng);

            var level = new LevelState(levelConfig);
            level.BeginLevel();
            level.Session.ArmMoves = boardConfig.InitialArmMoves;
            level.Session.PublicRepairEnergy = boardConfig.InitialPublicRepairEnergy;

            var game = new BoardGame(board, new SettleState(HexDirection.D0), level.Session, level);
            for (int i = 0; i < taps; i++)
            {
                game.Tap();
            }

            bool pass = level.Session.Score >= level.Round.TargetScore && level.Round.AllGoalsComplete;
            return new RegressionResult
            {
                SampleId = sampleId,
                Seed = seed,
                Score = level.Session.Score,
                Pass = pass,
            };
        }

        /// <summary>聚合多次演算为报告（通过率 + 分数分布）。</summary>
        public static RegressionReport Aggregate(IReadOnlyList<RegressionResult> results)
        {
            var report = new RegressionReport { TotalRuns = results.Count };
            int min = int.MaxValue;
            int max = int.MinValue;
            int sum = 0;
            int passes = 0;
            foreach (RegressionResult r in results)
            {
                if (r.Score < min)
                {
                    min = r.Score;
                }

                if (r.Score > max)
                {
                    max = r.Score;
                }

                sum += r.Score;
                if (r.Pass)
                {
                    passes++;
                }
            }

            report.PassCount = passes;
            report.ScoreMin = results.Count > 0 ? min : 0;
            report.ScoreMax = results.Count > 0 ? max : 0;
            report.ScoreSum = sum;
            return report;
        }
    }
}
