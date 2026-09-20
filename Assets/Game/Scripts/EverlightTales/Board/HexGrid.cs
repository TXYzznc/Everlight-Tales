namespace Everlight.Tales.Board
{
    /// <summary>
    /// 蜂窝格几何规则。盘面边长 n 决定总格数与合法格集合，规则见设计决定
    /// 「盘面为六边形网格，尺寸分四档配置」：总格数 = 3n(n−1) + 1，
    /// 合法格满足 max(|q|, |r|, |q + r|) ≤ n。
    /// 该类型不引用引擎，供离线批量演算与运行时共用。
    /// </summary>
    public static class HexGrid
    {
        /// <summary>计算边长 n 对应的总格数。</summary>
        public static int CountCells(int radius)
        {
            if (radius <= 0)
            {
                return 1;
            }

            return (3 * radius * (radius - 1)) + 1;
        }

        /// <summary>判定坐标是否落在边长 n 的合法格集合内。</summary>
        public static bool IsValid(HexCoord coord, int radius)
        {
            int q = coord.Q < 0 ? -coord.Q : coord.Q;
            int r = coord.R < 0 ? -coord.R : coord.R;
            int s = coord.S < 0 ? -coord.S : coord.S;
            return q <= radius && r <= radius && s <= radius;
        }

        /// <summary>取坐标到盘面中心的距离，等于到中心的最大轴距。</summary>
        public static int DistanceFromCenter(HexCoord coord)
        {
            int q = coord.Q < 0 ? -coord.Q : coord.Q;
            int r = coord.R < 0 ? -coord.R : coord.R;
            int s = coord.S < 0 ? -coord.S : coord.S;
            if (r > q)
            {
                q = r;
            }

            return s > q ? s : q;
        }
    }
}
