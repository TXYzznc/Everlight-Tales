namespace Everlight.Tales.Board
{
    /// <summary>
    /// 地形层类别。轨道／区域属于地形层，可与实体层在同一格共存（SR-004）。
    /// </summary>
    public enum TerrainKind : byte
    {
        /// <summary>轨道：影响移动方向或接续。</summary>
        Track = 0,

        /// <summary>区域：棋盘异常或效果区域。</summary>
        Area = 1,
    }
}
