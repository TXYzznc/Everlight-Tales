using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 特殊目标判定（P2-004）：把锚点标签映射为盘面节点（端点标签），
    /// 并按「到达／保持／顺序」三型判定。纯逻辑、不引用引擎。
    /// </summary>
    public static class GoalEvaluator
    {
        /// <summary>判断指定实体（0=任意）是否落在锚点标签所在格。</summary>
        public static bool AtLabel(BoardState board, int label, int entityId = 0)
        {
            if (!board.TryGetLabelCoord(label, out HexCoord coord))
            {
                return false;
            }

            BoardEntity occupant = board.EntityAt(coord);
            if (occupant == null)
            {
                return false;
            }

            return entityId == 0 || occupant.Id == entityId;
        }
    }

    /// <summary>
    /// 特殊目标运行时状态（P2-004）：按拍推进，追踪保持拍数与顺序进度。
    /// 到达一次即达成；保持需连续驻留指定拍数；顺序需按序到访各锚点。
    /// </summary>
    public sealed class AnchorGoalState
    {
        public GoalConfig Config { get; }

        public bool IsComplete { get; private set; }

        private int _holdTicks;

        private int _orderIndex;

        public AnchorGoalState(GoalConfig config)
        {
            Config = config;
        }

        /// <summary>按当前盘面推进一次判定。</summary>
        public void Tick(BoardState board)
        {
            if (IsComplete)
            {
                return;
            }

            switch (Config.Kind)
            {
                case GoalKind.Arrive:
                    if (GoalEvaluator.AtLabel(board, Config.AnchorLabel, Config.EntityId))
                    {
                        IsComplete = true;
                    }

                    break;

                case GoalKind.Hold:
                    _holdTicks = GoalEvaluator.AtLabel(board, Config.AnchorLabel, Config.EntityId) ? _holdTicks + 1 : 0;
                    if (_holdTicks >= Config.Required)
                    {
                        IsComplete = true;
                    }

                    break;

                case GoalKind.Order:
                    if (_orderIndex < Config.OrderLabels.Count
                        && GoalEvaluator.AtLabel(board, Config.OrderLabels[_orderIndex], Config.EntityId))
                    {
                        _orderIndex++;
                        if (_orderIndex >= Config.OrderLabels.Count)
                        {
                            IsComplete = true;
                        }
                    }

                    break;

                case GoalKind.PushedInto:
                    IsComplete = IsLockedAtLabel(board, Config.AnchorLabel, Config.EntityId);
                    break;
            }
        }

        private static bool IsLockedAtLabel(BoardState board, int label, int entityId)
        {
            if (!board.TryGetLabelCoord(label, out HexCoord coord))
            {
                return false;
            }

            BoardEntity occupant = board.EntityAt(coord);
            if (occupant == null || !occupant.IsLocked)
            {
                return false;
            }

            return entityId == 0 || occupant.Id == entityId;
        }
    }
}
