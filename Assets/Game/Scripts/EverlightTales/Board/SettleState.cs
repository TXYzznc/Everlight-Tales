namespace Everlight.Tales.Board
{
    /// <summary>
    /// 六相定势：内盘六相旋转状态与重力方向。
    /// 六个相位边一一对应六方向 D0~D5；对准底部定势座的那条边即最低势位，
    /// 也就是重力方向（SR-005／D-037）。旋转只改变重力方向，不移动棋子、
    /// 不触发能力、不累计代价（D-078）。本类型不引用引擎。
    /// </summary>
    public sealed class SettleState
    {
        private HexDirection _gravity;

        /// <summary>当前重力方向：对准定势座的那条相位边。</summary>
        public HexDirection GravityDirection => _gravity;

        /// <summary>重力方向的单位位移，供下落扫描使用。</summary>
        public HexCoord GravityOffset => HexDirections.Offset(_gravity);

        public SettleState(HexDirection initialGravity = HexDirection.D0)
        {
            _gravity = initialGravity;
        }

        /// <summary>左旋一个相位并吸附到位。</summary>
        public void RotateLeft()
        {
            _gravity = HexDirections.Rotate(_gravity, -1);
        }

        /// <summary>右旋一个相位并吸附到位。</summary>
        public void RotateRight()
        {
            _gravity = HexDirections.Rotate(_gravity, +1);
        }

        /// <summary>直接设定重力方向（供配置与恢复使用）。</summary>
        public void SetGravity(HexDirection direction)
        {
            _gravity = direction;
        }
    }
}
