using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>怪谈形态运行时状态（P2-006）：附着宿主、指定目标与剩余次数。</summary>
    public sealed class MFormState
    {
        public MFormConfig Config { get; }

        /// <summary>宿主零件实体 ID（0=未附着）。</summary>
        public int HostEntityId { get; private set; }

        /// <summary>被指定对象 ID（M-012 改道目标）。</summary>
        public int AssignedTargetId { get; private set; }

        /// <summary>本次作业剩余使用次数（跨轮保留）。</summary>
        public int UsesRemaining { get; private set; }

        public bool IsAttached => HostEntityId != 0;

        public MFormState(MFormConfig config)
        {
            Config = config;
            UsesRemaining = config.UsesPerJob;
        }

        /// <summary>附着到宿主零件（不可重复附着）。</summary>
        public bool Attach(int hostEntityId)
        {
            if (IsAttached)
            {
                return false;
            }

            HostEntityId = hostEntityId;
            return true;
        }

        /// <summary>指定改道目标对象。</summary>
        public void AssignTarget(int targetId)
        {
            AssignedTargetId = targetId;
        }

        /// <summary>消耗一次使用次数。</summary>
        public bool TryConsumeUse()
        {
            if (UsesRemaining <= 0)
            {
                return false;
            }

            UsesRemaining--;
            return true;
        }
    }

    /// <summary>怪谈形态行为解释器（P2-006）。首批 M-012 借力改道夹：为指定对象提供合法相邻改道路线。</summary>
    public static class MFormService
    {
        /// <summary>M-012：取指定对象当前格的合法相邻空格作为改道路线（不含原方向语义，由关卡实例决定）。</summary>
        public static IReadOnlyList<HexCoord> RedirectCandidates(BoardState board, BoardEntity target)
        {
            var result = new List<HexCoord>();
            if (target == null)
            {
                return result;
            }

            for (int i = 0; i < HexDirections.Count; i++)
            {
                HexCoord neighbor = target.Coord.Neighbor(HexDirections.All[i]);
                if (board.IsValid(neighbor) && !board.IsOccupied(neighbor))
                {
                    result.Add(neighbor);
                }
            }

            return result;
        }
    }
}
