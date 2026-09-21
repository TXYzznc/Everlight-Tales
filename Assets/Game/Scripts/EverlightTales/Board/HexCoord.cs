using System;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 蜂窝格的轴向坐标。总格数与合法格判定见 <see cref="HexBoardShape"/>。
    /// 该类型只做值语义与算术，不引用引擎，纹理与屏幕坐标由表现层换算。
    /// </summary>
    [Serializable]
    public readonly struct HexCoord : IEquatable<HexCoord>
    {
        /// <summary>轴向 q 分量。</summary>
        public readonly int Q;

        /// <summary>轴向 r 分量。</summary>
        public readonly int R;

        /// <summary>构造一个轴向坐标。</summary>
        public HexCoord(int q, int r)
        {
            Q = q;
            R = r;
        }

        /// <summary>盘面中心。</summary>
        public static readonly HexCoord Zero = new HexCoord(0, 0);

        /// <summary>第三轴分量，等于 q + r，用于距离与合法格判定。</summary>
        public int S => Q + R;

        /// <summary>按方向取相邻格坐标。</summary>
        public HexCoord Neighbor(HexDirection direction)
        {
            HexCoord offset = HexDirections.Offset(direction);
            return new HexCoord(Q + offset.Q, R + offset.R);
        }

        /// <summary>取到另一格的距离，正六边形蜂窝网格距离。</summary>
        public int DistanceTo(HexCoord other)
        {
            int dq = Q - other.Q;
            int dr = R - other.R;
            int ds = S - other.S;
            return (Math.Abs(dq) + Math.Abs(dr) + Math.Abs(ds)) / 2;
        }

        /// <inheritdoc />
        public bool Equals(HexCoord other)
        {
            return Q == other.Q && R == other.R;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is HexCoord other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Q * 397) ^ R;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return "(" + Q + "," + R + ")";
        }

        /// <summary>值相等。</summary>
        public static bool operator ==(HexCoord left, HexCoord right)
        {
            return left.Equals(right);
        }

        /// <summary>值不等。</summary>
        public static bool operator !=(HexCoord left, HexCoord right)
        {
            return !left.Equals(right);
        }
    }
}
