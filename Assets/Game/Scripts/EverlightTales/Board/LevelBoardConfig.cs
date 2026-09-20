using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>关键件配置（P1-021）：占名额并固定落位的关键件。</summary>
    public sealed class KeyPieceConfig
    {
        public PartType PartType { get; }

        public HexCoord Position { get; }

        public KeyPieceConfig(PartType partType, HexCoord position)
        {
            PartType = partType;
            Position = position;
        }
    }

    /// <summary>
    /// 关卡盘面配置（P1-021）：边长、关键件固定落位、携带池、初盘零件数、
    /// 初始机械臂次数与关初公共维修能量。含盘面坐标（HexCoord），故落 Board 层；
    /// 零件种类仍来自 Data 的 PartType。纯配置、不引用引擎。
    /// </summary>
    public sealed class LevelBoardConfig
    {
        public string LevelName { get; }

        public int SideLength { get; }

        public IReadOnlyList<KeyPieceConfig> KeyPieces { get; }

        public IReadOnlyList<PartType> CarryPool { get; }

        public int InitialPartCount { get; }

        public int InitialArmMoves { get; }

        public int InitialPublicRepairEnergy { get; }

        /// <summary>现场可借用种类（计入携带上限，去重后进入可用池，P2-007）。</summary>
        public IReadOnlyList<PartType> BorrowedParts { get; }

        /// <summary>固定元素（地形设施／障碍／任务标记，P2-009）。</summary>
        public IReadOnlyList<FixedElementConfig> FixedElements { get; }

        /// <summary>关卡一句话目标（关卡信息区，P2-007）。</summary>
        public string Objective { get; }

        /// <summary>预计耗时格数（关卡信息区，P2-007）。</summary>
        public int Duration { get; }

        public LevelBoardConfig(
            string levelName,
            int sideLength,
            IReadOnlyList<KeyPieceConfig> keyPieces,
            IReadOnlyList<PartType> carryPool,
            int initialPartCount,
            int initialArmMoves,
            int initialPublicRepairEnergy = 0,
            IReadOnlyList<PartType> borrowedParts = null,
            IReadOnlyList<FixedElementConfig> fixedElements = null,
            string objective = "",
            int duration = 0)
        {
            LevelName = levelName;
            SideLength = sideLength;
            KeyPieces = keyPieces ?? new KeyPieceConfig[0];
            CarryPool = carryPool ?? new PartType[0];
            InitialPartCount = initialPartCount;
            InitialArmMoves = initialArmMoves;
            InitialPublicRepairEnergy = initialPublicRepairEnergy;
            BorrowedParts = borrowedParts ?? new PartType[0];
            FixedElements = fixedElements ?? new FixedElementConfig[0];
            Objective = objective;
            Duration = duration;
        }
    }

    /// <summary>教学样张（P1-021）：S0 教学档（边长 5）的示例初盘配置。</summary>
    public static class TutorialLevelConfig
    {
        public static LevelBoardConfig Create()
        {
            return new LevelBoardConfig(
                levelName: "S0 教学样张",
                sideLength: 5,
                keyPieces: new[]
                {
                    new KeyPieceConfig(PartType.InertiaHammer, new HexCoord(0, 0)),
                    new KeyPieceConfig(PartType.MeteringRatchet, new HexCoord(1, 0)),
                },
                carryPool: new[]
                {
                    PartType.InertiaHammer,
                    PartType.MeteringRatchet,
                    PartType.BlastCoil,
                    PartType.None,
                },
                initialPartCount: 6,
                initialArmMoves: 2,
                initialPublicRepairEnergy: 0);
        }
    }
}
