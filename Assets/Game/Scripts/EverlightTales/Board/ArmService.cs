namespace Everlight.Tales.Board
{
    /// <summary>机械臂搬动结果（P1-010）。</summary>
    public enum ArmMoveResult
    {
        /// <summary>搬动成功（次数已减 1）。</summary>
        Ok,

        /// <summary>剩余次数不足。</summary>
        NoMovesLeft,

        /// <summary>实体不在盘面上。</summary>
        NotOnBoard,

        /// <summary>固定设施不可搬动。</summary>
        FixedEntity,

        /// <summary>目标格不在盘面内。</summary>
        InvalidTarget,

        /// <summary>目标格已被占用。</summary>
        Occupied,
    }

    /// <summary>
    /// 机械臂搬动服务（P1-010）。消耗 1 次次数把一枚棋子移到合法空格；
    /// 搬动不触发零件能力、不产生分数、不启动重力（SR/17-盘面与每拍奖励流程）。
    /// 次数跨轮保留、不自动补满，仅搬动成功时递减；本类型不引用引擎。
    /// </summary>
    public static class ArmService
    {
        public static ArmMoveResult Move(BoardState board, SessionState session, BoardEntity entity, HexCoord target)
        {
            if (board == null || session == null || entity == null)
            {
                return ArmMoveResult.NotOnBoard;
            }

            if (session.ArmMoves <= 0)
            {
                return ArmMoveResult.NoMovesLeft;
            }

            // 原地不视为搬动，不消耗次数（等价「取消」）。
            if (target == entity.Coord)
            {
                return ArmMoveResult.Ok;
            }

            switch (board.Move(entity, target, MoveSource.Arm))
            {
                case MoveResult.Ok:
                    session.ArmMoves--;
                    return ArmMoveResult.Ok;
                case MoveResult.NotOnBoard:
                    return ArmMoveResult.NotOnBoard;
                case MoveResult.FixedEntity:
                    return ArmMoveResult.FixedEntity;
                case MoveResult.InvalidTarget:
                    return ArmMoveResult.InvalidTarget;
                default:
                    return ArmMoveResult.Occupied;
            }
        }
    }
}
