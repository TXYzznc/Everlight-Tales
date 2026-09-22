using DG.Tweening;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 屏幕震动（b42 特效）：拍击时震动盘面根节点。参考 2026CIGA 的 ScreenShakeEffect 震 Main Camera，
    /// 本项目盘面为 ScreenSpaceOverlay 的 uGUI，相机震动不影响 UI，故改为震 board_root 的 RectTransform。
    /// 复用 DOTween.Extension 的 DOShakeAnchorPos。
    /// </summary>
    public sealed class ScreenShakeEffect : MonoBehaviour
    {
        [SerializeField, Range(0f, 40f)] private float m_Strength = 12f;

        [SerializeField, Range(0.05f, 1f)] private float m_Duration = 0.35f;

        [SerializeField, Range(5, 50)] private int m_Vibrato = 22;

        [SerializeField, Range(0f, 90f)] private float m_Randomness = 60f;

        private RectTransform m_Target;

        /// <summary>注入要震动的目标（盘面根节点）。</summary>
        public void SetTarget(RectTransform target)
        {
            m_Target = target;
        }

        /// <summary>触发一次盘面震动（先 Kill 残留并归位，避免连续触发基点漂移）。</summary>
        public void Shake()
        {
            if (m_Target == null)
            {
                return;
            }

            m_Target.DOKill(complete: false);
            m_Target.anchoredPosition = Vector2.zero;

            m_Target.DOShakeAnchorPos(
                m_Duration,
                m_Strength,
                m_Vibrato,
                m_Randomness,
                snapping: false,
                fadeOut: true);
        }

        private void OnDestroy()
        {
            if (m_Target != null)
            {
                m_Target.DOKill();
            }
        }
    }
}
