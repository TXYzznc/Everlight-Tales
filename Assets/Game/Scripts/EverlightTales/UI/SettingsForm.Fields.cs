using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// SettingsForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/SettingsForm.contract.json 生成，请勿手改）。
    ///
    /// 与 SettingsForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// SettingsForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class SettingsForm
    {
        [SerializeField] private Slider _musicSlider;
        public Slider MusicSlider => _musicSlider;
        [SerializeField] private Image _musicFill;
        public Image MusicFill => _musicFill;
        [SerializeField] private Image _musicHandle;
        public Image MusicHandle => _musicHandle;
        [SerializeField] private TMP_Text _musicValue;
        public TMP_Text MusicValue => _musicValue;
        [SerializeField] private Slider _sfxSlider;
        public Slider SfxSlider => _sfxSlider;
        [SerializeField] private Image _sfxFill;
        public Image SfxFill => _sfxFill;
        [SerializeField] private Image _sfxHandle;
        public Image SfxHandle => _sfxHandle;
        [SerializeField] private TMP_Text _sfxValue;
        public TMP_Text SfxValue => _sfxValue;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
    }
}
