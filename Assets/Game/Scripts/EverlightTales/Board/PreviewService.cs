using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>第一步预演结果：一枚将移动棋子的第一步终点（P1-013）。</summary>
    public readonly struct PreviewMove
    {
        public readonly int EntityId;

        public readonly HexCoord From;

        public readonly HexCoord To;

        public PreviewMove(int entityId, HexCoord from, HexCoord to)
        {
            EntityId = entityId;
            From = from;
            To = to;
        }
    }

    /// <summary>
    /// 第一步预演（P1-013）。按当前势位只算每枚会动棋子的第一步直接移动终点：
    /// 沿重力滑到边缘或阻挡前，不展开碰撞、触发、爆破、分裂或再下落（17-盘面与每拍奖励流程）。
    /// 排序与真实下落一致（重力投影前到后，同投影按 q/r/ID 升序），保证终点与结算首步吻合。
    /// 本类型不引用引擎。
    /// </summary>
    public static class PreviewService
    {
        public static IReadOnlyList<PreviewMove> Compute(BoardState board, SettleState settle)
        {
            var occupied = new HashSet<HexCoord>();
            var movable = new List<BoardEntity>();
            var pendingClosed = new HashSet<int>();
            var simulatedUses = new Dictionary<int, int>();
            var simulatedExpansion = new Dictionary<int, int>();
            foreach (AnomalyRegion region in board.AnomalyRegions)
            { simulatedUses[region.Id] = region.UseCount; simulatedExpansion[region.Id] = region.ExpansionCount; }
            foreach (BoardEntity entity in board.Entities)
            {
                if (!entity.IsLayeredFixture) occupied.Add(entity.Coord);
                if (entity.IsMovable) movable.Add(entity);
            }
            movable.Sort(new GravityOrderComparer { Offset = settle.GravityOffset });
            var moves = new List<PreviewMove>();
            var visits = new HashSet<(HexCoord, HexDirection)>();
            var portals = new HashSet<int>();
            foreach (BoardEntity entity in movable)
            {
                occupied.Remove(entity.Coord);
                HexCoord current = entity.Coord;
                visits.Clear();
                while (true)
                {
                    HexDirection direction = AnomalyService.GravityAt(board, current, settle.GravityDirection);
                    if (!visits.Add((current, direction))) break;
                    HexCoord next = current.Neighbor(direction);
                    BoardEntity nextFixture = board.FixtureAt(next);
                    if (!board.IsValid(next) || occupied.Contains(next) || !board.CanEnterFixture(next, direction)
                        || nextFixture != null && pendingClosed.Contains(nextFixture.Id)) break;
                    Leave(board, current, pendingClosed);
                    current = next;
                    if (Clamps(board, current, entity)) break;
                    portals.Clear();
                    while (true)
                    {
                        AnomalyRegion selected = null;
                        foreach (AnomalyRegion region in board.AnomalyRegions)
                        {
                            if (region.Type != Data.AnomalyType.ExpandingRift) continue;
                            bool inside = false;
                            for (int i = 0; i < region.Cells.Count; i++) inside |= region.Cells[i] == current;
                            for (int i = 0; i < simulatedExpansion[region.Id]; i++) inside |= region.ExpansionCells[i] == current;
                            if (inside && (selected == null || region.Config.Priority > selected.Config.Priority
                                || region.Config.Priority == selected.Config.Priority && region.Id < selected.Id)) selected = region;
                        }
                        if (selected == null || !portals.Add(selected.Id) || selected.ExitCell == current
                            || occupied.Contains(selected.ExitCell) || !board.CanEnterFixture(selected.ExitCell, BoardState.Incoming(current, selected.ExitCell))
                            || board.FixtureAt(selected.ExitCell) != null && pendingClosed.Contains(board.FixtureAt(selected.ExitCell).Id)) break;
                        Leave(board, current, pendingClosed); current = selected.ExitCell;
                        if (++simulatedUses[selected.Id] % selected.Config.Threshold == 0
                            && simulatedExpansion[selected.Id] < System.Math.Min(selected.Config.MaxExpansion, selected.ExpansionCells.Count))
                            simulatedExpansion[selected.Id]++;
                        if (Clamps(board, current, entity)) break;
                    }
                    if (Clamps(board, current, entity)) break;
                }
                occupied.Add(current);
                if (current != entity.Coord) moves.Add(new PreviewMove(entity.Id, entity.Coord, current));
            }

            return moves;
        }

        private static void Leave(BoardState board, HexCoord at, HashSet<int> closed)
        {
            BoardEntity fixture = board.FixtureAt(at);
            if (fixture != null && fixture.PendingClose) closed.Add(fixture.Id);
        }
        private static bool Clamps(BoardState board, HexCoord at, BoardEntity entity)
        {
            BoardEntity fixture = board.FixtureAt(at);
            return fixture?.ObstacleType == Data.ObstacleType.ClampingSeat && fixture.ClampedEntityId == 0
                && fixture.ReleaseWindowRemaining == 0 && fixture.ReleasedEntityId != entity.Id
                && fixture.ObstacleSetup.AllowsTag(entity.Kind == EntityKind.Part ? "normal" : "task");
        }

        /// <summary>重力方向投影排序：前到后（投影大者在前），同投影按 q、r、ID 升序。</summary>
        private sealed class GravityOrderComparer : IComparer<BoardEntity>
        {
            public HexCoord Offset;

            public int Compare(BoardEntity x, BoardEntity y)
            {
                int px = (x.Coord.Q * Offset.Q) + (x.Coord.R * Offset.R);
                int py = (y.Coord.Q * Offset.Q) + (y.Coord.R * Offset.R);
                if (px != py)
                {
                    return py.CompareTo(px);
                }

                if (x.Coord.Q != y.Coord.Q)
                {
                    return x.Coord.Q.CompareTo(y.Coord.Q);
                }

                if (x.Coord.R != y.Coord.R)
                {
                    return x.Coord.R.CompareTo(y.Coord.R);
                }

                return x.Id.CompareTo(y.Id);
            }
        }
    }
}
