namespace Everlight.Tales.Data
{
    /// <summary>
    /// 业务数据层的编译期锚点。
    /// 该层只承载表结构、配置 DTO、常量与枚举，不引用引擎，也不承载运行时状态。
    /// </summary>
    public static class EverlightTalesDataLayer
    {
        /// <summary>层标识，用于启动日志与诊断。</summary>
        public const string LayerName = "Everlight.Tales.Data";
    }
}
