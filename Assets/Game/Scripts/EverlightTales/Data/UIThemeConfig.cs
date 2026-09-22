namespace Everlight.Tales.Data
{
    /// <summary>
    /// 界面主题规范（P5-005）：全局配色 + 文字层级 + 间距，供各页面高层统一引用。
    /// 颜色用 #RRGGBB 字符串登记，由 UI 层解析为 Color；纯配置落 Data。
    /// </summary>
    public sealed class UIThemeConfig
    {
        public string Name;         // 主题名
        public string Primary;      // 主色（面板/边框）
        public string Secondary;    // 辅色（次级背景/分隔）
        public string Accent;       // 强调色（可交互/高亮）
        public string Background;   // 全局背景
        public string TextTitle;    // 标题文字色
        public string TextBody;     // 正文文字色
        public string TextHint;     // 辅助文字色
        public float SpacingSmall;  // 紧凑间距
        public float SpacingMedium; // 常规间距
        public float SpacingLarge;  // 宽松间距
        public int TitleFontSize;   // 标题字号
        public int BodyFontSize;    // 正文字号
        public int HintFontSize;    // 辅助字号

        public UIThemeConfig(string name, string primary, string secondary, string accent, string background,
            string textTitle, string textBody, string textHint,
            float spacingSmall, float spacingMedium, float spacingLarge,
            int titleFontSize, int bodyFontSize, int hintFontSize)
        {
            Name = name;
            Primary = primary;
            Secondary = secondary;
            Accent = accent;
            Background = background;
            TextTitle = textTitle;
            TextBody = textBody;
            TextHint = textHint;
            SpacingSmall = spacingSmall;
            SpacingMedium = spacingMedium;
            SpacingLarge = spacingLarge;
            TitleFontSize = titleFontSize;
            BodyFontSize = bodyFontSize;
            HintFontSize = hintFontSize;
        }
    }

    /// <summary>全局主题入口（P5-005）：默认「长明修理铺」暖色 + 深底主题。</summary>
    public static class UITheme
    {
        public static readonly UIThemeConfig Default = new UIThemeConfig(
            "长明修理铺",
            "#2B2B2B", "#4A4A4A", "#C89B5A", "#1A1A1A",
            "#F5F0E6", "#D8D0C0", "#8A8378",
            8f, 16f, 32f,
            28, 18, 14);
    }
}
