namespace Everlight.Tales.Data
{
    /// <summary>语言选项（P5-009）。</summary>
    public enum LanguageOption : byte
    {
        ZhHans = 0, // 简体中文
        En = 1,     // English
    }

    /// <summary>设置模型（P5-009）：音量（音乐/音效）+ 语言；菜单占位由 UI 层登记。</summary>
    public sealed class SettingsModel
    {
        public float MusicVolume;   // 0..1
        public float SfxVolume;     // 0..1
        public LanguageOption Language;

        public static readonly float MaxVolume = 1f;
        public static readonly float DefaultVolume = 0.8f;

        public SettingsModel()
        {
            MusicVolume = DefaultVolume;
            SfxVolume = DefaultVolume;
            Language = LanguageOption.ZhHans;
        }

        /// <summary>音量夹取到 [0,1]。</summary>
        public static float ClampVolume(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > MaxVolume)
            {
                return MaxVolume;
            }

            return value;
        }
    }
}
