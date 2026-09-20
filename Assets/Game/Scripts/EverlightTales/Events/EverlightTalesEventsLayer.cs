namespace Everlight.Tales.Events
{
    /// <summary>
    /// 事件层的编译期锚点。该层承载盘面事件总线、能力事件队列、Buff 与触发器，
    /// 负责把模拟器产生的即时结果组织成可播放、可复现的事件序列。
    /// 本层 MUST NOT 引用任何 Unity 引擎程序集。
    /// </summary>
    public static class EverlightTalesEventsLayer
    {
        /// <summary>层标识，用于启动日志与诊断。</summary>
        public const string LayerName = "Everlight.Tales.Events";
    }
}
