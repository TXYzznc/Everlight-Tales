using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>卷帘门事件的规则判定；静态事件和盘面内容来自配置快照。</summary>
    public static class RollerDoorEvent
    {
        public static RepairEventConfig CreateConfig()
        {
            LevelPreset preset = LevelPresetCatalog.Get("EV-N01");
            NormalEventConfig eventConfig = FindEvent("EV-N01");
            if (preset == null || eventConfig == null) throw new System.InvalidOperationException("EV-N01 配置尚未加载。");
            var materials = new List<string>();
            foreach (FormMaterialCost reward in RewardMaterialCatalog.ForOwner(eventConfig.Id))
                for (int i = 0; i < reward.Count; i++) materials.Add(reward.MaterialId);
            return new RepairEventConfig(eventConfig.Id, eventConfig.Name, eventConfig.TimeCost, preset.Level,
                new EventReward(eventConfig.RewardFee, materials), initialArmMoves: preset.Board.InitialArmMoves);
        }

        public static LevelBoardConfig CreateBoardConfig()
        {
            LevelPreset preset = LevelPresetCatalog.Get("EV-N01");
            if (preset == null) throw new System.InvalidOperationException("EV-N01 盘面配置尚未加载。");
            return preset.Board;
        }

        private static NormalEventConfig FindEvent(string id)
        {
            foreach (NormalEventConfig entry in NormalEventCatalog.FirstBatch()) if (entry.Id == id) return entry;
            return null;
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
