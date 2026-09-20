using System.Collections.Generic;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 蜂窝格几何规则。盘面以「边长 n」参数化（n = 每边格数，见设计决定 D-088）：
    /// 总格数 = 3n(n−1) + 1；坐标半径（最大轴向距离）= n − 1，
    /// 合法格满足 max(|q|, |r|, |q + r|) ≤ n − 1，即 DistanceFromCenter(coord) &lt; n。
    /// 四档：教学 n=5(61)、试机 5~7、普通 6~8、怪谈 n=10(271)。
    /// 该类型不引用引擎，供离线批量演算与运行时共用。
    /// </summary>
    public static class HexGrid
    {
        /// <summary>边长 n 对应的坐标半径（最大轴向距离），等于 n − 1。</summary>
        public static int RadiusOf(int sideLength)
        {
            return sideLength - 1;
        }

        /// <summary>计算边长 n 对应的总格数，公式 3n(n−1) + 1。</summary>
        public static int CountCells(int sideLength)
        {
            if (sideLength < 1)
            {
                return 0;
            }

            return (3 * sideLength * (sideLength - 1)) + 1;
        }

        /// <summary>判定坐标是否落在边长 n 的合法格集合内。</summary>
        public static bool IsValid(HexCoord coord, int sideLength)
        {
            return sideLength >= 1 && DistanceFromCenter(coord) < sideLength;
        }

        /// <summary>取坐标到盘面中心的最大轴距。</summary>
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

        /// <summary>按边长 n 枚举全部合法格，自 q 升序、r 升序扫描，数量等于 CountCells(n)。</summary>
        public static IEnumerable<HexCoord> Enumerate(int sideLength)
        {
            if (sideLength < 1)
            {
                yield break;
            }

            int radius = sideLength - 1;
            for (int q = -radius; q <= radius; q++)
            {
                int rMin = -radius - q;
                if (rMin < -radius)
                {
                    rMin = -radius;
                }

                int rMax = radius - q;
                if (rMax > radius)
                {
                    rMax = radius;
                }

                for (int r = rMin; r <= rMax; r++)
                {
                    yield return new HexCoord(q, r);
                }
            }
        }
    }
}
