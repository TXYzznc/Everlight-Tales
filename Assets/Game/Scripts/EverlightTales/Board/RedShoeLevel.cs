using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>红舞鞋初盘零件固定落位（P2-014）。</summary>
    public sealed class RedShoePartPlacement
    {
        public PartType PartType;
        public HexCoord Coord;

        public RedShoePartPlacement(PartType partType, HexCoord coord)
        {
            PartType = partType;
            Coord = coord;
        }
    }

    /// <summary>红舞鞋单轮目标（P2-014）：分数、拍数、导流/封存匣/封存特殊目标。</summary>
    public sealed class RedShoeRoundConfig
    {
        public int TargetScore;
        public int TapCount;
        public int MinDiversion;
        public bool RequireBoxRepaired;
        public bool RequireSealed;

        public RedShoeRoundConfig(int targetScore, int tapCount, int minDiversion = 0, bool requireBoxRepaired = false, bool requireSealed = false)
        {
            TargetScore = targetScore;
            TapCount = tapCount;
            MinDiversion = minDiversion;
            RequireBoxRepaired = requireBoxRepaired;
            RequireSealed = requireSealed;
        }
    }

    /// <summary>红舞鞋第一关配置（P2-014，D-051）：固定盘面 + 三轮目标 + 耗时。</summary>
    public sealed class RedShoeLevelConfig
    {
        public int SideLength;
        public RedShoeConfig RedShoe;
        public IReadOnlyList<RedShoePartPlacement> InitialParts;
        public int InitialArmMoves;
        public IReadOnlyList<RedShoeRoundConfig> Rounds;
        public int TimeCost;

        public RedShoeLevelConfig(
            int sideLength,
            RedShoeConfig redShoe,
            IReadOnlyList<RedShoePartPlacement> initialParts,
            int initialArmMoves,
            IReadOnlyList<RedShoeRoundConfig> rounds,
            int timeCost)
        {
            SideLength = sideLength;
            RedShoe = redShoe;
            InitialParts = initialParts ?? System.Array.Empty<RedShoePartPlacement>();
            InitialArmMoves = initialArmMoves;
            Rounds = rounds ?? System.Array.Empty<RedShoeRoundConfig>();
            TimeCost = timeCost;
        }

        /// <summary>红舞鞋第一关（D-051）：一关三轮各 3 拍，30/100/170 分，导流→分离修匣→封存。</summary>
        public static RedShoeLevelConfig Tutorial()
        {
            var redShoe = new RedShoeConfig(
                new HexCoord(0, -2),
                HexDirection.D5,
                new[]
                {
                    new HexCoord(0, -1),
                    new HexCoord(-1, 1),
                    new HexCoord(-2, 2),
                    new HexCoord(-1, 2),
                    new HexCoord(0, 2),
                    new HexCoord(2, 1),
                },
                new[] { new RedShoeTurnCell(new HexCoord(0, 0), HexDirection.D4) },
                new HexCoord(2, 0),
                2,
                new HexCoord(3, 0),
                6,
                6,
                1);

            var parts = new[]
            {
                new RedShoePartPlacement(PartType.InertiaHammer, new HexCoord(-2, -2)),
                new RedShoePartPlacement(PartType.InertiaHammer, new HexCoord(-2, 0)),
                new RedShoePartPlacement(PartType.InertiaHammer, new HexCoord(0, -3)),
                new RedShoePartPlacement(PartType.MeteringRatchet, new HexCoord(-1, -1)),
                new RedShoePartPlacement(PartType.BlastCoil, new HexCoord(1, -2)),
                new RedShoePartPlacement(PartType.RivetPliers, new HexCoord(1, 1)),
            };

            var rounds = new[]
            {
                new RedShoeRoundConfig(30, 3, minDiversion: 3),
                new RedShoeRoundConfig(100, 3, minDiversion: 6, requireBoxRepaired: true),
                new RedShoeRoundConfig(170, 3, requireSealed: true),
            };

            return new RedShoeLevelConfig(5, redShoe, parts, 2, rounds, 4);
        }
    }

    /// <summary>构建好的红舞鞋关卡运行时（P2-014）。</summary>
    public sealed class RedShoeLevel
    {
        public BoardState Board;
        public RedShoeState RedShoe;
        public LevelState Level;
        public RedShoeLevelConfig Config;

        public RedShoeLevel(BoardState board, RedShoeState redShoe, LevelState level, RedShoeLevelConfig config)
        {
            Board = board;
            RedShoe = redShoe;
            Level = level;
            Config = config;
        }
    }

    /// <summary>红舞鞋关卡构建器（P2-014）：固定盘面 + 三轮运行时。</summary>
    public static class RedShoeLevelBuilder
    {
        public static RedShoeLevel Build(RedShoeLevelConfig config)
        {
            var board = new BoardState(config.SideLength);

            int id = 1;
            foreach (RedShoePartPlacement placement in config.InitialParts)
            {
                board.Place(BoardEntity.Part(id++, placement.PartType), placement.Coord);
            }

            int boxId = id;
            board.Place(
                BoardEntity.RepairTarget(boxId, new RepairTargetConfig(new[] { PartType.RivetPliers }, config.RedShoe.BoxRequired, 2)),
                config.RedShoe.BoxCoord);

            var redShoe = new RedShoeState(config.RedShoe.StartCoord, config.RedShoe.StartDirection, config.RedShoe.RestraintLimit);

            var roundConfigs = new List<RoundConfig>();
            foreach (RedShoeRoundConfig round in config.Rounds)
            {
                roundConfigs.Add(new RoundConfig(round.TapCount, round.TargetScore));
            }

            var level = new LevelState(new LevelConfig(roundConfigs));
            level.BeginLevel();

            return new RedShoeLevel(board, redShoe, level, config);
        }
    }

    /// <summary>红舞鞋轮目标与整关完成判定（P2-014）。</summary>
    public static class RedShoeRoundEvaluator
    {
        public static bool IsRoundComplete(RedShoeRoundConfig round, RedShoeLevel level)
        {
            if (level.Level.Session.Score < round.TargetScore)
            {
                return false;
            }

            if (level.RedShoe.DiversionProgress < round.MinDiversion)
            {
                return false;
            }

            if (round.RequireBoxRepaired && !RedShoeService.IsBoxRepaired(level.Board, level.Config.RedShoe))
            {
                return false;
            }

            if (round.RequireSealed && !level.RedShoe.IsSealed)
            {
                return false;
            }

            return true;
        }

        public static bool IsLevelComplete(RedShoeLevel level)
        {
            return level.Level.IsLevelComplete && level.RedShoe.IsSealed;
        }
    }
}
