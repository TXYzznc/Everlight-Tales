namespace Everlight.Tales.Data
{
    /// <summary>
    /// 移动端适配配置（P5-010）：竖屏 + 安全区（像素，各边距）+ 参考分辨率。
    /// 真机触控/分辨率实测留发布预热，此处只登记适配口径与默认值。
    /// </summary>
    public sealed class ScreenAdapterConfig
    {
        public string Orientation;     // "Portrait" 竖屏
        public float SafeAreaTop;      // 顶部安全区（像素）
        public float SafeAreaBottom;   // 底部安全区（像素）
        public float SafeAreaLeft;     // 左侧安全区（像素）
        public float SafeAreaRight;    // 右侧安全区（像素）
        public int ReferenceWidth;     // 参考分辨率宽
        public int ReferenceHeight;    // 参考分辨率高

        public ScreenAdapterConfig(string orientation, float safeAreaTop, float safeAreaBottom,
            float safeAreaLeft, float safeAreaRight, int referenceWidth, int referenceHeight)
        {
            Orientation = orientation;
            SafeAreaTop = safeAreaTop;
            SafeAreaBottom = safeAreaBottom;
            SafeAreaLeft = safeAreaLeft;
            SafeAreaRight = safeAreaRight;
            ReferenceWidth = referenceWidth;
            ReferenceHeight = referenceHeight;
        }
    }

    /// <summary>默认竖屏适配（P5-010）：1080×1920 参考 + 刘海/底栏安全区。</summary>
    public static class ScreenAdapter
    {
        public static readonly ScreenAdapterConfig Default = new ScreenAdapterConfig(
            "Portrait", 88f, 68f, 0f, 0f, 1080, 1920);
    }
}
