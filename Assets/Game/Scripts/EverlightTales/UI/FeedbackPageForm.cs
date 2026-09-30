using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>独立反馈页：固定反馈状态与提交入口，具体提交渠道由上层流程注入。</summary>
    public sealed class FeedbackPageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "FeedbackPage";

        [SerializeField] private TMP_InputField m_MessageInput;
        [SerializeField] private TextMeshProUGUI m_StatusText;
        [SerializeField] private Button m_SubmitButton;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_MessageInput == null) m_MessageInput = transform.Find("Panel_Feedback/Input_Message")?.GetComponent<TMP_InputField>();
            if (m_StatusText == null) m_StatusText = transform.Find("Panel_Feedback/Txt_Status")?.GetComponent<TextMeshProUGUI>();
            if (m_SubmitButton == null) m_SubmitButton = transform.Find("Panel_Feedback/Btn_Submit")?.GetComponent<Button>();
            if (m_SubmitButton != null)
            {
                m_SubmitButton.onClick.RemoveAllListeners();
                m_SubmitButton.onClick.AddListener(() =>
                {
                    if (m_StatusText != null) m_StatusText.text = string.IsNullOrWhiteSpace(m_MessageInput != null ? m_MessageInput.text : string.Empty) ? "请先填写反馈内容" : "反馈已记录";
                });
            }
        }
    }
}
