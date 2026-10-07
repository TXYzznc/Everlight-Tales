using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
        [SerializeField] private GameObject m_DimedRoot = null;
        [SerializeField] private Button m_DimedButton = null;
        private Coroutine m_AutoCloseCoroutine = null;
        private Coroutine m_DimedEnableCoroutine = null;

        private bool m_ResultConsumed = false;
        private bool m_DimedDismissEnabled = false;

        protected override void OnOpen(object userData)
        {
            // UIFormBase.OnOpen 会同步执行 Params.OpenCallback；必须先完成本类状态初始化，
            // 否则 SetupToast 启动的协程会在返回后被这里的清理逻辑立即停止。
            Debug.Log($"[UI诊断][DialogView] OnOpen prepare instance={GetInstanceID()}, serial={(UIForm != null ? UIForm.SerialId : -1)}, active={gameObject.activeInHierarchy}, timeScale={Time.timeScale}", this);
            m_ResultConsumed = false;
            m_DimedRoot?.SetActive(true);
            m_DimedDismissEnabled = false;
            if (m_DimedEnableCoroutine != null)
            {
                StopCoroutine(m_DimedEnableCoroutine);
                m_DimedEnableCoroutine = null;
            }
            if (m_DimedRoot != null)
            {
                if (m_DimedButton == null)
                {
                    Debug.LogError("[DialogView][Contract] 缺少遮罩按钮引用 m_DimedButton。", this);
                    return;
                }
                m_DimedButton.transition = Selectable.Transition.None;
                m_DimedButton.onClick.RemoveAllListeners();
                m_DimedButton.onClick.AddListener(OnDimedClicked);
            }
            if (m_AutoCloseCoroutine != null)
            {
                StopCoroutine(m_AutoCloseCoroutine);
                m_AutoCloseCoroutine = null;
            }

            base.OnOpen(userData);
            Debug.Log($"[UI诊断][DialogView] OnOpen complete instance={GetInstanceID()}, serial={(UIForm != null ? UIForm.SerialId : -1)}, active={gameObject.activeInHierarchy}, autoClose={(m_AutoCloseCoroutine != null)}, dimedDismiss={m_DimedDismissEnabled}", this);
        }

        public void BindStaticLayout(TextMeshProUGUI title, TextMeshProUGUI content, Button[] buttons, TextMeshProUGUI[] labels)
        {
            m_TitleText = title;
            m_ContentText = content;
            m_Buttons = buttons ?? new Button[0];
            m_ButtonLabels = labels ?? new TextMeshProUGUI[0];
        }

        /// <summary>由 GlobalUI 经 OpenCallback 调用，设置标题/内容/按钮。</summary>
        public void Setup(string title, string content, (string tag, string label)[] buttons)
        {
            Debug.Log($"[UI诊断][DialogView] Setup title={title}, buttonCount={(buttons != null ? buttons.Length : 0)}, active={gameObject.activeInHierarchy}, interactable={Interactable}, timeScale={Time.timeScale}", this);
            m_DimedRoot?.SetActive(true);
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

            if (count == 0)
            {
                BeginDimedDismiss();
            }
            else
            {
                m_DimedDismissEnabled = false;
                if (m_DimedRoot != null) m_DimedRoot.SetActive(true);
            }
        }

        /// <summary>以 GF UIDialog 形式显示短暂提示，提示内容由预制体承载，自动关闭。</summary>
        public void SetupToast(string message, float durationSeconds = 2f)
        {
            Debug.Log($"[UI诊断][DialogView] SetupToast message={message}, duration={durationSeconds}, active={gameObject.activeInHierarchy}, timeScale={Time.timeScale}", this);
            Setup("提示", message, System.Array.Empty<(string tag, string label)>());
            m_AutoCloseCoroutine = StartCoroutine(AutoCloseToast(durationSeconds));
            Debug.Log($"[UI诊断][DialogView] SetupToast coroutineStarted={m_AutoCloseCoroutine != null}, dimedDismissEnabled={m_DimedDismissEnabled}", this);
        }

        private void BeginDimedDismiss()
        {
            Debug.Log($"[UI诊断][DialogView] BeginDimedDismiss active={gameObject.activeInHierarchy}, dimed={(m_DimedRoot != null ? m_DimedRoot.activeInHierarchy : false)}, timeScale={Time.timeScale}", this);
            if (m_DimedRoot != null) m_DimedRoot.SetActive(true);
            m_DimedDismissEnabled = false;
            if (m_DimedEnableCoroutine != null) StopCoroutine(m_DimedEnableCoroutine);
            m_DimedEnableCoroutine = StartCoroutine(EnableDimedDismissAfter(1f));
        }

        private IEnumerator EnableDimedDismissAfter(float seconds)
        {
            Debug.Log($"[UI诊断][DialogView] DimedDismiss wait start seconds={seconds}, realtime={Time.realtimeSinceStartup:F2}", this);
            // DialogView 所属 UI 可能暂停游戏时间，遮罩关闭仍必须按真实时间生效。
            yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, seconds));
            m_DimedEnableCoroutine = null;
            m_DimedDismissEnabled = true;
            Debug.Log($"[UI诊断][DialogView] DimedDismiss enabled realtime={Time.realtimeSinceStartup:F2}, active={gameObject.activeInHierarchy}", this);
        }

        private void OnDimedClicked()
        {
            Debug.Log($"[UI诊断][DialogView] Dimed clicked enabled={m_DimedDismissEnabled}, consumed={m_ResultConsumed}, active={gameObject.activeInHierarchy}", this);
            if (!m_DimedDismissEnabled || m_ResultConsumed)
            {
                return;
            }
            m_ResultConsumed = true;
            m_DimedDismissEnabled = false;
            Debug.Log($"[UI诊断][DialogView] Dimed closing serial={(UIForm != null ? UIForm.SerialId : -1)}", this);
            CloseWithAnimation();
        }

        private IEnumerator AutoCloseToast(float durationSeconds)
        {
            Debug.Log($"[UI诊断][DialogView] AutoClose wait start seconds={durationSeconds}, realtime={Time.realtimeSinceStartup:F2}", this);
            // Toast 的自动关闭不能依赖 Time.timeScale，否则模态 UI 暂停时会永久停留。
            yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, durationSeconds));
            m_AutoCloseCoroutine = null;
            Debug.Log($"[UI诊断][DialogView] AutoClose elapsed realtime={Time.realtimeSinceStartup:F2}, active={gameObject.activeInHierarchy}, consumed={m_ResultConsumed}", this);
            CloseWithAnimation();
        }

        private void HandleButton(string tag)
        {
            if (m_ResultConsumed)
            {
                return; // 连点保护：结果只回传一次
            }
            m_ResultConsumed = true;
            m_DimedDismissEnabled = false;
            ClickUIButton(tag);   // 复用框架：播放点击音 + 派发 ButtonClickCallback
            CloseWithAnimation(); // 走框架关闭流程
        }

        protected override void OnCloseAnimationComplete()
        {
            Debug.Log($"[UI诊断][DialogView] Close animation complete serial={(UIForm != null ? UIForm.SerialId : -1)}, active={gameObject.activeInHierarchy}", this);
            base.OnCloseAnimationComplete();
        }
    }
}
