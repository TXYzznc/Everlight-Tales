using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 服务区（b28 zone 4，批 19 升级为快捷入口集）：配装 / 恢复面板 / 设置。
    /// 非经营：不做客流/好感/经营数值（D-323）。
    /// </summary>
    public sealed class ServicePanel : MonoBehaviour
    {
        [SerializeField] private RectTransform m_Root;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _hint;
        [SerializeField] private Button _loadout;
        [SerializeField] private Button _recover;
        [SerializeField] private Button _settings;

        [ContextMenu("Validate UI Contract")]
        private void ValidateUiContract()
        {
            if (m_Root == null || _title == null || _hint == null || _loadout == null || _recover == null || _settings == null)
                Debug.LogError("[ServicePanel][Contract] 页面级引用未完整绑定。", this);
            else Debug.Log("[ServicePanel][Contract] 页面级引用校验通过。", this);
        }

        public void BindStaticLayout()
        {
            Transform root = m_Root != null ? m_Root.parent : (transform.name == "Panel_Service" ? transform : transform.Find("Panel_Service"));
            Transform content = m_Root != null ? m_Root : (root != null ? root.Find("Panel_ServiceContent") : null);
            if (m_Root == null) m_Root = content as RectTransform;
            if (_title == null) _title = content?.Find("Txt_Title")?.GetComponent<TMP_Text>();
            if (_hint == null) _hint = content?.Find("Txt_Hint")?.GetComponent<TMP_Text>();
            if (_loadout == null) _loadout = content?.Find("Btn_Loadout")?.GetComponent<Button>();
            if (_recover == null) _recover = content?.Find("Btn_Recover")?.GetComponent<Button>();
            if (_settings == null) _settings = content?.Find("Btn_Settings")?.GetComponent<Button>();
            if (_title != null) _title.text = "服务";
            if (_hint != null) _hint.text = "快捷入口（非经营）";
        }

        public void Build()
        {
            if (m_Root == null || _title == null || _hint == null || _loadout == null || _recover == null || _settings == null)
            {
                Debug.LogError("ServicePanel 静态布局不完整，拒绝运行时创建 UI。", this);
                return;
            }
            _loadout.onClick.RemoveAllListeners();
            _loadout.onClick.AddListener(() => GlobalUI.ShowToast("配装请在准备页调整（进入事件前）"));
            _recover.onClick.RemoveAllListeners();
            _recover.onClick.AddListener(() =>
            {
                bool has = WorldSession.Current != null && WorldSession.Current.HasAttemptSave;
                GlobalUI.ShowDialog("恢复维修尝试", has ? "检测到未完成的维修尝试，可继续或放弃。" : "当前没有未完成的维修尝试。");
            });
            _settings.onClick.RemoveAllListeners();
            _settings.onClick.AddListener(() => GF.UI.OpenUIForm(UIViews.Settings));
        }
    }
}
