namespace Everlight.Tales.Board
{
    /// <summary>
    /// 盘面层的编译期锚点。该层只承载六相定势盘的模拟器逻辑：
    /// 网格、棋子与占用、六相旋转、拍击结算事务、碰撞、FIFO 事件队列、
    /// 计分、能量、机械臂与轮关判定。
    /// 本层 MUST NOT 引用任何 Unity 引擎程序集，以保证离线演算与同种子复现可在无引擎环境运行。
    /// </summary>
    public static class EverlightTalesBoardLayer
    {
        /// <summary>层标识，用于启动日志与诊断。</summary>
        public const string LayerName = "Everlight.Tales.Board";
    }
}
