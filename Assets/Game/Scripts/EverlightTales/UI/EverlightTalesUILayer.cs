namespace Everlight.Tales.UI
{
    /// <summary>
    /// 业务 UI 页面的统一标记。竖屏页面数量较多（盘面、准备、结算、三页、地图、加工台、图鉴、家园），
    /// 全部业务页面壳实现本接口，便于统一走查层级、字体与色板规范。
    /// </summary>
    public interface IProjectUIForm
    {
        /// <summary>页面标识，与 UI 分组配置中的资源名一致。</summary>
        string FormKey { get; }
    }

    /// <summary>
    /// UI 层的编译期锚点。该层承载全部 UIForm／UIItem 与业务页面，
    /// 可引用其余四个业务程序集，引用方向不可逆。
    /// </summary>
    public static class EverlightTalesUILayer
    {
        /// <summary>层标识，用于启动日志与诊断。</summary>
        public const string LayerName = "Everlight.Tales.UI";
    }
}
