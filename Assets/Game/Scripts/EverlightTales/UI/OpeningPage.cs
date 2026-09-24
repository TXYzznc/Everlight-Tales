using Everlight.Tales.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 序章演示页（P3-005 表现层）：播放一段序章字幕序列，逐帧显示说话人与文本，播完完成。
    /// 播片/立绘/音频为后续美术资源挂接点，本页先以字幕 + 节奏完成闭环。
    /// </summary>
    public sealed class OpeningPage : MonoBehaviour
    {
        public TextMeshProUGUI SubtitleText;

        public OpeningSequence Sequence { get; private set; }

        public bool IsComplete => Sequence == null || Sequence.IsComplete;

        public void Play(OpeningSequence sequence)
        {
            Sequence = sequence;
            Refresh();
        }

        private void Update()
        {
            if (Sequence == null || Sequence.IsComplete)
            {
                return;
            }

            Sequence.Advance(Time.deltaTime);
            Refresh();
        }

        public void Skip()
        {
            if (Sequence != null)
            {
                Sequence.Skip();
                Refresh();
            }
        }

        private void Refresh()
        {
            if (SubtitleText == null)
            {
                return;
            }

            PrologueStepConfig current = Sequence == null ? null : Sequence.Current;
            SubtitleText.text = current == null
                ? string.Empty
                : string.IsNullOrEmpty(current.Speaker) ? current.Text : current.Speaker + "：" + current.Text;
        }
    }
}
