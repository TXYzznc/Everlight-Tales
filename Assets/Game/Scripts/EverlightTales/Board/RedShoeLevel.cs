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
        public int BoardRadius;
        public RedShoeConfig RedShoe;
        public IReadOnlyList<RedShoePartPlacement> InitialParts;
        public int InitialArmMoves;
        public IReadOnlyList<RedShoeRoundConfig> Rounds;
        public int TimeCost;
        public LevelPreset SourceLevel { get; }

        public RedShoeLevelConfig(
            int boardRadius,
            RedShoeConfig redShoe,
            IReadOnlyList<RedShoePartPlacement> initialParts,
            int initialArmMoves,
            IReadOnlyList<RedShoeRoundConfig> rounds,
            int timeCost,
            LevelPreset sourceLevel = null)
        {
            BoardRadius = boardRadius;
            RedShoe = redShoe;
            InitialParts = initialParts ?? System.Array.Empty<RedShoePartPlacement>();
            InitialArmMoves = initialArmMoves;
            Rounds = rounds ?? System.Array.Empty<RedShoeRoundConfig>();
            TimeCost = timeCost;
            SourceLevel = sourceLevel;
        }

        /// <summary>红舞鞋第一关（D-051）：一关三轮各 3 拍，30/100/170 分，导流→分离修匣→封存。</summary>
        public static RedShoeLevelConfig Tutorial()
        {
            return ChallengeAssemblyCatalog.RequireCase(FirstCaseContent.CaseId).CreateConfig();
        }

        public static RedShoeLevelConfig From(LevelPreset preset, RedShoePreset redShoePreset)
        {
            if (preset == null || redShoePreset == null || preset.Key != redShoePreset.LevelKey)
                throw new System.ArgumentException("红鞋装配缺少匹配关卡/专属规则。");
            var redShoe = new RedShoeConfig(
                redShoePreset.StartCoord,
                redShoePreset.StartDirection,
                new List<HexCoord>(redShoePreset.GuideCells),
                CopyTurns(redShoePreset.TurnCells),
                redShoePreset.BoxCoord,
                redShoePreset.BoxRequired,
                redShoePreset.EntryCoord,
                redShoePreset.RequiredDiversion,
                redShoePreset.RestraintLimit,
                redShoePreset.StepsPerTap);

            var parts = new List<RedShoePartPlacement>();
            foreach (KeyPieceConfig key in preset.Board.KeyPieces)
                parts.Add(new RedShoePartPlacement(key.PartType, key.Position));

            var rounds = new List<RedShoeRoundConfig>();
            for (int i = 0; i < preset.Level.Rounds.Count; i++)
            {
                RoundConfig round = preset.Level.Rounds[i];
                int minDiversion = 0;
                bool requireBoxRepaired = false;
                bool requireSealed = false;
                foreach (SpecialGoalConfig goal in round.Goals)
                {
                    if (goal.Id == "diversion") minDiversion = goal.Required;
                    else if (goal.Id == "box_repaired") requireBoxRepaired = goal.Required > 0;
                    else if (goal.Id == "sealed") requireSealed = goal.Required > 0;
                }
                rounds.Add(new RedShoeRoundConfig(round.TargetScore, round.TapCount,
                    minDiversion: minDiversion,
                    requireBoxRepaired: requireBoxRepaired,
                    requireSealed: requireSealed));
            }

            return new RedShoeLevelConfig(preset.Board.BoardRadius, redShoe, parts, preset.Board.InitialArmMoves, rounds, preset.Board.Duration, preset);
        }
        private static IReadOnlyList<RedShoeTurnCell> CopyTurns(IReadOnlyList<RedShoeTurnCell> source)
        {
            var result = new List<RedShoeTurnCell>();
            foreach (RedShoeTurnCell turn in source) result.Add(new RedShoeTurnCell(turn.Coord, turn.Direction));
            return result;
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
            LevelPreset preset = config.SourceLevel ?? throw new System.ArgumentException("红鞋运行时必须提供显式装配的关卡。");
            FixedElementBinding.Validate(preset.Board);
            var board = new BoardState(config.BoardRadius);
            int id = 1;
            foreach (RedShoePartPlacement placement in config.InitialParts)
                if (board.Place(BoardEntity.Part(id++, placement.PartType), placement.Coord) != PlaceResult.Ok)
                    throw new System.InvalidOperationException(preset.Key + " 关键件落位失败。");
            var ids = new Dictionary<int, int>();
            foreach (FixedElementConfig element in preset.Board.FixedElements)
            {
                if (element.Kind == FixedElementKind.EndpointLabel) { board.SetEndpointLabel(element.Position, element.Label); continue; }
                BoardEntity entity = element.CreateEntity(id++);
                if (board.Place(entity, element.Position) != PlaceResult.Ok)
                    throw new System.InvalidOperationException(element.OwnerKey + " 红鞋固定元素落位失败。");
                if (element.ElementId != 0) ids.Add(element.ElementId, entity.Id);
            }
            FixedElementBinding.Resolve(board, preset.Board, ids);

            var redShoe = new RedShoeState(config.RedShoe.StartCoord, config.RedShoe.StartDirection, config.RedShoe.RestraintLimit);

            var level = new LevelState(CreateLevelConfig(config));
            level.BeginLevel();
            level.Session.ArmMoves = config.InitialArmMoves;
            level.Session.PublicRepairEnergy = preset.Board.InitialPublicRepairEnergy;

            return new RedShoeLevel(board, redShoe, level, config);
        }

        public static LevelConfig CreateLevelConfig(RedShoeLevelConfig config)
        {
            var rounds = new List<RoundConfig>();
            foreach (RedShoeRoundConfig round in config.Rounds)
            {
                var goals = new List<SpecialGoalConfig>();
                if (round.MinDiversion > 0) goals.Add(new SpecialGoalConfig("导流", round.MinDiversion));
                if (round.RequireBoxRepaired) goals.Add(new SpecialGoalConfig("封存匣维修", config.RedShoe.BoxRequired));
                if (round.RequireSealed) goals.Add(new SpecialGoalConfig("红鞋封存", 1));
                rounds.Add(new RoundConfig(round.TapCount, round.TargetScore, goals));
            }
            return new LevelConfig(rounds);
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
