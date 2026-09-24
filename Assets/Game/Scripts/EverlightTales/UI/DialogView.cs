using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 通用弹窗/确认（P0-007）。基于 UIFormBase，挂 Dialog(200) 分组。
    /// 全屏 Dimed 遮罩阻断下层交互（模态）；按钮结果经 UIParams.ButtonClickCallback 回传，
    /// 复用 ClickUIButton 的点击音，并用 m_ResultConsumed 做连点不重复触发保护。
    /// </summary>
    public sealed class DialogView : UIFormBase
    {
        [SerializeField] private TextMeshProUGUI m_TitleText = null;
        [SerializeField] private TextMeshProUGUI m_ContentText = null;
        [SerializeField] private Button[] m_Buttons = null;
        [SerializeField] private TextMeshProUGUI[] m_ButtonLabels = null;

        private bool m_ResultConsumed = false;

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            m_ResultConsumed = false;
        }

        /// <summary>由 GlobalUI 经 OpenCallback 调用，设置标题/内容/按钮。</summary>
        public void Setup(string title, string content, (string tag, string label)[] buttons)
        {
            if (m_TitleText != null)
            {
                m_TitleText.text = title ?? string.Empty;
            }
            if (m_ContentText != null)
            {
                m_ContentText.text = content ?? string.Empty;
            }

            int count = buttons != null ? buttons.Length : 0;
            for (int i = 0; i < m_Buttons.Length; i++)
            {
                bool active = i < count;
                m_Buttons[i].gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }

                m_ButtonLabels[i].text = buttons[i].label;
                m_Buttons[i].onClick.RemoveAllListeners();
                string tag = buttons[i].tag;
                m_Buttons[i].onClick.AddListener(() => HandleButton(tag));
            }
        }

        private void HandleButton(string tag)
        {
            if (m_ResultConsumed)
            {
                return; // 连点保护：结果只回传一次
            }
            m_ResultConsumed = true;
            ClickUIButton(tag);   // 复用框架：播放点击音 + 派发 ButtonClickCallback
            CloseWithAnimation(); // 走框架关闭流程
        }
    }
}
