namespace Everlight.Tales.Board
{
    /// <summary>
    /// 六相定势盘的六个相位方向。取值与盘面六条边一一对应，顺序固定，不随盘面旋转而改变含义。
    /// </summary>
    public enum HexDirection
    {
        /// <summary>相位 D0。</summary>
        D0 = 0,

        /// <summary>相位 D1。</summary>
        D1 = 1,

        /// <summary>相位 D2。</summary>
        D2 = 2,

        /// <summary>相位 D3。</summary>
        D3 = 3,

        /// <summary>相位 D4。</summary>
        D4 = 4,

        /// <summary>相位 D5。</summary>
        D5 = 5,
    }

    /// <summary>
    /// 六方向常量表与相位换算。仅使用自身序号运算，不依赖引擎数学库。
    /// </summary>
    public static class HexDirections
    {
        /// <summary>方向总数。</summary>
        public const int Count = 6;

        /// <summary>全部六个方向，索引与 <see cref="HexDirection"/> 的取值一致。</summary>
        public static readonly HexDirection[] All =
        {
            HexDirection.D0,
            HexDirection.D1,
            HexDirection.D2,
            HexDirection.D3,
            HexDirection.D4,
            HexDirection.D5,
        };

        /// <summary>按索引取方向，索引按六取模，支持负索引。</summary>
        public static HexDirection FromIndex(int index)
        {
            int wrapped = index % Count;
            if (wrapped < 0)
            {
                wrapped += Count;
            }

            return All[wrapped];
        }

        /// <summary>取方向的序号。</summary>
        public static int ToIndex(HexDirection direction)
        {
            return (int)direction;
        }

        /// <summary>按步长旋转方向，用于左旋／右旋后的相位吸附。</summary>
        public static HexDirection Rotate(HexDirection direction, int steps)
        {
            return FromIndex(ToIndex(direction) + steps);
        }

        /// <summary>取相反方向，用于重力接续的判定。</summary>
        public static HexDirection Opposite(HexDirection direction)
        {
            return FromIndex(ToIndex(direction) + Count / 2);
        }

        /// <summary>
        /// 轴向坐标下的单位位移。数组顺序与 <see cref="HexDirection"/> 一致：
        /// q 向右增长，r 向右下增长，(q + r) 向左下增长。
        /// </summary>
        public static readonly HexCoord[] Offsets =
        {
            new HexCoord(1, 0),
            new HexCoord(1, -1),
            new HexCoord(0, -1),
            new HexCoord(-1, 0),
            new HexCoord(-1, 1),
            new HexCoord(0, 1),
        };

        /// <summary>取方向的单位位移。</summary>
        public static HexCoord Offset(HexDirection direction)
        {
            return Offsets[ToIndex(direction)];
        }
    }
}
