using System;
using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>盘面格子的几何分类（D-088 修订）：按是否被大六边形轮廓裁切划分。</summary>
    public enum HexCellKind
    {
        /// <summary>正常格：六个顶点全部位于大六边形轮廓内部，可放置棋子、参与移动碰撞。</summary>
        Normal,

        /// <summary>残缺墙体：至少一部分在轮廓内，但有顶点越界，显示为被裁切的半格，不可放置。</summary>
        Wall,

        /// <summary>棋盘外部：与大六边形轮廓完全不相交，不生成格子、不参与逻辑。</summary>
        Outside,
    }

    /// <summary>
    /// 盘面形状（D-088 修订）：点顶大六边形（顶点朝上下）用 SDF 三投影裁切蜂窝网格，
    /// 产出 Normal／Wall／Outside 三分类。参数为「可玩区半径」<see cref="BoardRadius"/>，
    /// 轮廓半径 <see cref="OuterRadius"/> = BoardRadius + 1（含外层残缺墙）。
    /// 几何依据是实际顶点是否越界，而非中心距离（参考项目 2026CIGA 算法）。
    /// 本类型不引用引擎，供离线批量演算与运行时共用；几何用归一化格子（CellSize = 1）。
    /// </summary>
    public sealed class HexBoardShape
    {
        private readonly List<HexCoord> _normal = new List<HexCoord>();
        private readonly List<HexCoord> _wall = new List<HexCoord>();

        /// <summary>可玩区半径（中心到可玩区边缘的轴向距离）。</summary>
        public int BoardRadius { get; }

        /// <summary>含外层残缺墙的轮廓半径，等于 BoardRadius + 1。</summary>
        public int OuterRadius => BoardRadius + 1;

        /// <summary>大六边形轮廓边心距（中心到边的垂直距离），归一化格子单位（CellSize = 1）。</summary>
        public double Apothem { get; }

        /// <summary>正常格数量。</summary>
        public int NormalCount => _normal.Count;

        /// <summary>残缺墙数量。</summary>
        public int WallCount => _wall.Count;

        public HexBoardShape(int boardRadius)
        {
            if (boardRadius < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(boardRadius), "boardRadius must be >= 1.");
            }

            BoardRadius = boardRadius;
            Apothem = Math.Sqrt(3.0) * OuterRadius;
            Build();
        }

        /// <summary>判定坐标的分类（Normal／Wall／Outside）。</summary>
        public HexCellKind Classify(HexCoord coord)
        {
            double cx = LocalX(coord);
            double cy = LocalY(coord);

            // 候选筛选：中心投影不超过 apothem + margin（margin = CellSize = 1），否则完全在轮廓外。
            if (Sdf(cx, cy) > Apothem + 1.0)
            {
                return HexCellKind.Outside;
            }

            // 核心：六个顶点（90° + i·60°，半径 = CellSize = 1）任一越界即为残缺墙。
            for (int i = 0; i < 6; i++)
            {
                double a = (90.0 + i * 60.0) * Math.PI / 180.0;
                double vx = cx + Math.Cos(a);
                double vy = cy + Math.Sin(a);
                if (Sdf(vx, vy) > Apothem + Epsilon)
                {
                    return HexCellKind.Wall;
                }
            }

            return HexCellKind.Normal;
        }

        /// <summary>是否正常格（可放置、可移动）。</summary>
        public bool IsNormal(HexCoord coord) => Classify(coord) == HexCellKind.Normal;

        /// <summary>是否残缺墙（不可放置、阻挡移动）。</summary>
        public bool IsWall(HexCoord coord) => Classify(coord) == HexCellKind.Wall;

        /// <summary>枚举全部正常格（自 q 升序、r 升序，确定性）。</summary>
        public IReadOnlyList<HexCoord> EnumerateNormal() => _normal;

        /// <summary>枚举全部残缺墙（自 q 升序、r 升序，确定性）。</summary>
        public IReadOnlyList<HexCoord> EnumerateWall() => _wall;

        /// <summary>枚举轮廓内全部格子（正常格 + 残缺墙，即候选格）。</summary>
        public IEnumerable<HexCoord> EnumerateInside()
        {
            foreach (HexCoord cell in _normal)
            {
                yield return cell;
            }

            foreach (HexCoord cell in _wall)
            {
                yield return cell;
            }
        }

        private const double Epsilon = 0.0001;

        private static double LocalX(HexCoord coord)
        {
            return Math.Sqrt(3.0) * (coord.Q + coord.R * 0.5);
        }

        private static double LocalY(HexCoord coord)
        {
            return 1.5 * coord.R;
        }

        /// <summary>点顶大六边形 SDF：三组对边法线方向（0°、±60°）上的最大投影。</summary>
        private static double Sdf(double x, double y)
        {
            double cos60 = 0.5;
            double sin60 = Math.Sqrt(3.0) * 0.5;
            double a = Math.Abs(x);
            double b = Math.Abs(cos60 * x + sin60 * y);
            double c = Math.Abs(cos60 * x - sin60 * y);
            return Math.Max(a, Math.Max(b, c));
        }

        private void Build()
        {
            _normal.Clear();
            _wall.Clear();

            int bound = OuterRadius * 2;
            for (int q = -bound; q <= bound; q++)
            {
                for (int r = -bound; r <= bound; r++)
                {
                    HexCoord cell = new HexCoord(q, r);
                    HexCellKind kind = Classify(cell);
                    if (kind == HexCellKind.Normal)
                    {
                        _normal.Add(cell);
                    }
                    else if (kind == HexCellKind.Wall)
                    {
                        _wall.Add(cell);
                    }
                }
            }
        }
    }
}
