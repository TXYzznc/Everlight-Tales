namespace Everlight.Tales.Data
{
    /// <summary>
    /// 业务分层清单。作为各层程序集的集中引用点，便于启动日志与审计断言。
    /// 该类型位于数据层，因此五个业务程序集都可以引用它而不产生反向依赖：
    /// 各层自己的锚点常量由本层持有，清单只汇总名称字符串。
    /// </summary>
    public static class ProjectLayers
    {
        /// <summary>业务程序集总数。</summary>
        public const int Count = 5;

        /// <summary>数据层名称。</summary>
        public const string Data = EverlightTalesDataLayer.LayerName;

        /// <summary>盘面模拟器层名称。</summary>
        public const string Board = "Everlight.Tales.Board";

        /// <summary>世界与元游戏层名称。</summary>
        public const string Meta = "Everlight.Tales.Meta";

        /// <summary>事件与触发器层名称。</summary>
        public const string Events = "Everlight.Tales.Events";

        /// <summary>业务 UI 层名称。</summary>
        public const string UI = "Everlight.Tales.UI";

        /// <summary>按分层顺序返回全部层名。</summary>
        public static string[] All()
        {
            return new[] { Data, Board, Meta, Events, UI };
        }
    }
}
