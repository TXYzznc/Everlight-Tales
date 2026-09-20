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
            foreach (BoardEntity entity in board.Entities)
            {
                if (entity.IsMovable)
                {
                    movable.Add(entity);
                }
                else
                {
                    occupied.Add(entity.Coord);
                }
            }

            movable.Sort(new GravityOrderComparer { Offset = settle.GravityOffset });

            HexCoord offset = settle.GravityOffset;
            var moves = new List<PreviewMove>();
            foreach (BoardEntity entity in movable)
            {
                HexCoord current = entity.Coord;
                while (true)
                {
                    HexCoord next = new HexCoord(current.Q + offset.Q, current.R + offset.R);
                    if (!board.IsValid(next) || occupied.Contains(next))
                    {
                        break;
                    }

                    current = next;
                }

                occupied.Add(current);
                if (current != entity.Coord)
                {
                    moves.Add(new PreviewMove(entity.Id, entity.Coord, current));
                }
            }

            return moves;
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
