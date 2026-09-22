using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 设置服务（P5-009）：音量（音乐/音效）与语言的状态管理与夹取。
    /// 纯逻辑（Meta 仅引用 Data）；持久化由 UI 层挂 PlayerPrefs（设置页打开时读写）。
    /// </summary>
    public sealed class SettingsService
    {
        private readonly SettingsModel _model = new SettingsModel();

        public float MusicVolume => _model.MusicVolume;

        public float SfxVolume => _model.SfxVolume;

        public LanguageOption Language => _model.Language;

        public void SetMusicVolume(float value)
        {
            _model.MusicVolume = SettingsModel.ClampVolume(value);
        }

        public void SetSfxVolume(float value)
        {
            _model.SfxVolume = SettingsModel.ClampVolume(value);
        }

        public void SetLanguage(LanguageOption language)
        {
            _model.Language = language;
        }
    }
}
