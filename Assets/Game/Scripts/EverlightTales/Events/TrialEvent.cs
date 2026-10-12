using Everlight.Tales.Board;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>试机的规则判定；盘面固定内容来自关卡关系表。</summary>
    public static class TrialEvent
    {
        public static LevelBoardConfig CreateBoardConfig(TrialConfig trial)
        {
            if (trial == null) throw new System.ArgumentNullException(nameof(trial));
            LevelPreset preset = LevelPresetCatalog.Get(trial.Id);
            if (preset == null) throw new System.InvalidOperationException("试机盘面配置尚未加载：" + trial.Id);
            return preset.Board;
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
