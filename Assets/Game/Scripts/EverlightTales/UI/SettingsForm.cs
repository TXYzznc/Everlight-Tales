using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 设置界面（P5-009）：音乐/音效音量滑动条 + 关闭。
    /// 逻辑复用 <see cref="SettingsService"/>（纯逻辑）与 <see cref="GameAudio"/>（应用到分组），
    /// 持久化由本 UI 层挂 PlayerPrefs（打开时读、拖动时写）。
    /// 契约绑定字段见同名的 <c>SettingsForm.Fields.cs</c>（同一 partial 类）。
    /// </summary>
    public sealed partial class SettingsForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "Settings";

        private static readonly SettingsService Settings = new SettingsService();

        private const string MusicPrefsKey = "et.settings.music";

        private const string SfxPrefsKey = "et.settings.sfx";

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            // Slider 的 fill/handle/targetGraphic 结构由契约落地，运行时补上引用。
            WireSlider(MusicSlider, MusicFill, MusicHandle);
            WireSlider(SfxSlider, SfxFill, SfxHandle);

            CloseButton.onClick.AddListener(OnClickClose);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            LoadFromPrefs();

            MusicSlider.onValueChanged.RemoveAllListeners();
            MusicSlider.onValueChanged.AddListener(OnMusicChanged);
            SfxSlider.onValueChanged.RemoveAllListeners();
            SfxSlider.onValueChanged.AddListener(OnSfxChanged);

            MusicSlider.SetValueWithoutNotify(Settings.MusicVolume);
            SfxSlider.SetValueWithoutNotify(Settings.SfxVolume);
            RefreshLabels();
        }

        private static void WireSlider(Slider slider, Image fill, Image handle)
        {
            if (slider == null)
            {
                return;
            }

            if (fill != null)
            {
                slider.fillRect = (RectTransform)fill.transform;
            }

            if (handle != null)
            {
                slider.handleRect = (RectTransform)handle.transform;
                slider.targetGraphic = handle;
            }
        }

        private static void LoadFromPrefs()
        {
            Settings.SetMusicVolume(PlayerPrefs.GetFloat(MusicPrefsKey, SettingsModel.DefaultVolume));
            Settings.SetSfxVolume(PlayerPrefs.GetFloat(SfxPrefsKey, SettingsModel.DefaultVolume));
            GameAudio.SetMusicVolume(Settings.MusicVolume);
            GameAudio.SetSoundVolume(Settings.SfxVolume);
        }

        private void OnMusicChanged(float value)
        {
            Settings.SetMusicVolume(value);
            GameAudio.SetMusicVolume(Settings.MusicVolume);
            PlayerPrefs.SetFloat(MusicPrefsKey, Settings.MusicVolume);
            RefreshLabels();
        }

        private void OnSfxChanged(float value)
        {
            Settings.SetSfxVolume(value);
            GameAudio.SetSoundVolume(Settings.SfxVolume);
            PlayerPrefs.SetFloat(SfxPrefsKey, Settings.SfxVolume);
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            if (MusicValue != null)
            {
                MusicValue.text = Mathf.RoundToInt(Settings.MusicVolume * 100f) + "%";
            }

            if (SfxValue != null)
            {
                SfxValue.text = Mathf.RoundToInt(Settings.SfxVolume * 100f) + "%";
            }
        }
    }
}
