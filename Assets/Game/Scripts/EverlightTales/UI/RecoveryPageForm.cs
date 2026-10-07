using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>独立恢复页：只呈现并转发恢复动作，规则由 RecoveryService/WorldSession 保持不变。</summary>
    public sealed class RecoveryPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "RecoveryPage";

        [SerializeField] private TextMeshProUGUI m_StatusText;
        [SerializeField] private Button m_ContinueButton;
        [SerializeField] private Button m_AbandonButton;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_StatusText == null || m_ContinueButton == null || m_AbandonButton == null)
            {
                Debug.LogError("[RecoveryPageForm][Contract] 页面引用未完整绑定，拒绝装配恢复操作。", this);
                return;
            }
            bool has = WorldSession.Current != null && WorldSession.Current.HasAttemptSave;
            if (m_StatusText != null) m_StatusText.text = has ? "检测到未完成的维修尝试，是否继续？" : "当前没有未完成的维修尝试。";
            if (m_ContinueButton != null)
            {
                m_ContinueButton.interactable = has;
                m_ContinueButton.onClick.RemoveAllListeners();
                m_ContinueButton.onClick.AddListener(() => GlobalUI.ShowToast("继续维修（首版尝试存档未持久化，暂跳过）"));
            }
            if (m_AbandonButton != null)
            {
                m_AbandonButton.onClick.RemoveAllListeners();
                m_AbandonButton.onClick.AddListener(() => GlobalUI.ShowToast("已放弃维修尝试"));
            }
        }
    }
}
