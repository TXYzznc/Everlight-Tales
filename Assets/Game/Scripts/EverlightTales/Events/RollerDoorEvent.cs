using Everlight.Tales.Board;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>
    /// 卷帘门 EV-N01（P2-016，D-066）：一关一轮 3 拍、累计 30 分、耗时 2 格；
    /// 特殊目标=门轴联系标记 B 被普通推移送入校正格 G（机械臂直接放入不计）。
    /// 成功奖励 40 维修费 + MT-002 精密齿轮×1。
    /// </summary>
    public static class RollerDoorEvent
    {
        /// <summary>校正格 G 的端点标签。</summary>
        public const int GateLabel = 1;

        public static RepairEventConfig CreateConfig()
        {
            return new RepairEventConfig(
                "EV-N01",
                "卡住的卷帘门",
                2,
                new LevelConfig(new[]
                {
                    new RoundConfig(3, 30),
                }),
                new EventReward(40, new[] { "MT-002" }),
                initialArmMoves: 2);
        }

        public static LevelBoardConfig CreateBoardConfig()
        {
            return new LevelBoardConfig(
                levelName: "EV-N01 卡住的卷帘门",
                boardRadius: 5,
                keyPieces: new[]
                {
                    new KeyPieceConfig(PartType.InertiaHammer, new HexCoord(0, 0)), // H 惯性撞锤
                },
                carryPool: new[]
                {
                    PartType.InertiaHammer,
                    PartType.MeteringRatchet,
                    PartType.BlastCoil,
                    PartType.ReversalGear,
                },
                initialPartCount: 6,
                initialArmMoves: 2,
                initialPublicRepairEnergy: 0,
                fixedElements: new[]
                {
                    new FixedElementConfig(FixedElementKind.Obstacle, new HexCoord(-1, 0), obstacleType: ObstacleType.FixedWall), // W 传动箱外壳
                    new FixedElementConfig(FixedElementKind.MovableMarker, new HexCoord(1, 0), goal: new GoalConfig("gate", GoalKind.PushedInto, 0, GateLabel)), // B 门轴联系标记
                    new FixedElementConfig(FixedElementKind.EndpointLabel, new HexCoord(2, 0), label: GateLabel), // G 校正格
                },
                objective: "把门轴联系标记推入校正格，让卷帘门重新卡回轨道",
                duration: 2);
        }

        /// <summary>门轴联系标记是否已由普通推移送入校正格（锁定）。</summary>
        public static bool IsGatePushedIn(BoardState board)
        {
            BoardEntity marker = FindGateMarker(board);
            return marker != null && marker.IsLocked;
        }

        public static BoardEntity FindGateMarker(BoardState board)
        {
            foreach (BoardEntity entity in board.Entities)
            {
                if (entity.Kind == EntityKind.TaskMarker && entity.Goal != null && entity.Goal.Kind == GoalKind.PushedInto)
                {
                    return entity;
                }
            }

            return null;
        }
    }
}
