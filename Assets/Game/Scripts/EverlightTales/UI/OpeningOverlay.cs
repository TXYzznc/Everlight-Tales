using System;
using System.Collections.Generic;
using Everlight.Tales.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 序章覆盖层（b43）：新档首进全屏播放序章字幕，可跳过/播完进入地图。
    /// 挂在 MainPageShell 创建的全屏子对象上，完成后自毁。
    /// </summary>
    public sealed class OpeningOverlay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_Subtitle;

        [SerializeField] private Button m_ActionButton;

        [SerializeField] private TextMeshProUGUI m_ActionLabel;

        private OpeningSequence m_Sequence;

        private Action m_OnComplete;

        private bool m_Finished;
        private bool m_StaticRoot;

        public void BindStaticLayout()
        {
            m_StaticRoot = true;
            if (m_Subtitle == null || m_ActionButton == null || m_ActionLabel == null)
                Debug.LogError("[OpeningOverlay][Contract] 字幕、动作按钮或按钮文字引用未绑定。", this);
        }

        public void Play(IReadOnlyList<PrologueStepConfig> steps, Action onComplete)
        {
            m_OnComplete = onComplete;
            m_Sequence = new OpeningSequence(steps);
            BuildUI();
        }

        private void BuildUI()
        {
            if (m_Subtitle != null && m_ActionButton != null)
            {
                m_ActionButton.onClick.RemoveListener(Finish);
                m_ActionButton.onClick.AddListener(Finish);
                return;
            }
            Debug.LogError("OpeningOverlay 静态布局不完整，拒绝运行时创建 UI。", this);
        }

        private void Update()
        {
            if (m_Finished)
            {
                return;
            }

            if (m_Sequence != null && !m_Sequence.IsComplete)
            {
                m_Sequence.Advance(Time.deltaTime);
            }

            PrologueStepConfig current = m_Sequence == null ? null : m_Sequence.Current;
            if (m_Subtitle != null)
            {
                m_Subtitle.text = current == null
                    ? string.Empty
                    : string.IsNullOrEmpty(current.Speaker) ? current.Text : current.Speaker + "：" + current.Text;
            }

            bool done = m_Sequence == null || m_Sequence.IsComplete;
            if (done && m_ActionLabel != null && m_ActionLabel.text != "开始")
            {
                m_ActionLabel.text = "开始";
            }
        }

        private void Finish()
        {
            if (m_Finished)
            {
                return;
            }

            m_Finished = true;
            Action callback = m_OnComplete;
            m_OnComplete = null;
            callback?.Invoke();
            if (m_StaticRoot) gameObject.SetActive(false);
            else Destroy(gameObject);
        }

        private TextMeshProUGUI FindText(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<TextMeshProUGUI>() : null;
        }

    }
}
