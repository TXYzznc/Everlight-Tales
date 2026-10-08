using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>独立对话流程页：由本页管理内容与立绘；内嵌面板不作为独立 UIForm 初始化。</summary>
    public sealed class DialoguePageForm : UIFormBase, IProjectUIForm
    {
        public string FormKey => "DialoguePage";

        [SerializeField] private FormalPortrait _portrait;
        public void SetSpeaker(string identity) => _portrait.Show(identity);
        public void SetDialogue(string speaker, string content)
        {
            SetSpeaker(speaker);
            m_TitleText.text = speaker ?? string.Empty;
            m_ContentText.text = content ?? string.Empty;
        }
        [SerializeField] private DialogView m_DialogView;
        [SerializeField] private TextMeshProUGUI m_TitleText;
        [SerializeField] private TextMeshProUGUI m_ContentText;
        [SerializeField] private Button[] m_Buttons;
        [SerializeField] private TextMeshProUGUI[] m_ButtonLabels;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            if (m_DialogView == null) m_DialogView = GetComponentInChildren<DialogView>(true);
            Transform panel = m_DialogView != null ? m_DialogView.transform : null;
            if (panel != null)
            {
                if (m_TitleText == null) m_TitleText = panel.Find("Txt_Title")?.GetComponent<TextMeshProUGUI>();
                if (m_ContentText == null) m_ContentText = panel.Find("Txt_Content")?.GetComponent<TextMeshProUGUI>();
                if (m_Buttons == null || m_Buttons.Length == 0)
                {
                    m_Buttons = new Button[3];
                    for (int i = 0; i < m_Buttons.Length; i++) m_Buttons[i] = panel.Find("Button_" + i)?.GetComponent<Button>();
                }
                if (m_ButtonLabels == null || m_ButtonLabels.Length == 0)
                {
                    m_ButtonLabels = new TextMeshProUGUI[3];
                    for (int i = 0; i < m_ButtonLabels.Length; i++) m_ButtonLabels[i] = panel.Find("Button_" + i + "/Txt_Button_" + i)?.GetComponent<TextMeshProUGUI>();
                }
            }
            if (m_DialogView != null)
            {
                m_DialogView.BindStaticLayout(m_TitleText, m_ContentText, m_Buttons, m_ButtonLabels);
            }
        }
    }
}
