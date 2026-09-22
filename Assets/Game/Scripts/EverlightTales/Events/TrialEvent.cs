using Everlight.Tales.Board;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>
    /// 试机关卡配置（P4-012，D-062）：借用样机（锁定形态）的一关一轮 3 拍、耗时 2 格。
    /// 简化样张特殊目标 = 试机标记被普通推移送入端点格（锁定）；D-060 形态行为未实现前，
    /// 四关共用「推移到位」判定，各关专属特殊目标（双路推动/隔空射线/定时爆破）待形态行为接入。
    /// </summary>
    public static class TrialEvent
    {
        public const int TrialGoalLabel = 2;

        public static LevelBoardConfig CreateBoardConfig(TrialConfig trial)
        {
            return new LevelBoardConfig(
                levelName: trial.Id + " " + trial.Name,
                boardRadius: 5,
                keyPieces: new[]
                {
                    new KeyPieceConfig(trial.HostPart, new HexCoord(0, 0)), // 借用样机（锁定形态）
                },
                carryPool: new[]
                {
                    PartType.InertiaHammer,
                    PartType.MeteringRatchet,
                    PartType.BlastCoil,
                    PartType.ReversalGear,
                },
                initialPartCount: 4,
                initialArmMoves: 2,
                initialPublicRepairEnergy: 0,
                fixedElements: new[]
                {
                    new FixedElementConfig(FixedElementKind.Obstacle, new HexCoord(-1, 0), obstacleType: ObstacleType.FixedWall),
                    new FixedElementConfig(FixedElementKind.MovableMarker, new HexCoord(1, 0), goal: new GoalConfig("trial", GoalKind.PushedInto, 0, TrialGoalLabel)),
                    new FixedElementConfig(FixedElementKind.EndpointLabel, new HexCoord(2, 0), label: TrialGoalLabel),
                },
                objective: trial.SpecialGoal,
                duration: 2);
        }

        /// <summary>试机标记是否已由普通推移送入端点格（锁定）。</summary>
        public static bool IsSpecialGoalMet(BoardState board)
        {
            BoardEntity marker = FindMarker(board);
            return marker != null && marker.IsLocked;
        }

        public static BoardEntity FindMarker(BoardState board)
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
