namespace Everlight.Tales.Meta.Input
{
    /// <summary>
    /// 默认输入来源选择。编辑器使用鼠标回退来源，其余平台使用触摸来源；
    /// 选择结果由条件编译决定，MUST NOT 在运行时靠"是否检测到触摸"猜测。
    /// </summary>
    public static class InputProviderFactory
    {
        /// <summary>创建当前平台的默认来源。</summary>
        public static IInputProvider CreateDefault()
        {
#if UNITY_EDITOR
            return new EditorMouseInputProvider();
#else
            return new TouchInputProvider();
#endif
        }
    }
}
