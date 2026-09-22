using DG.Tweening;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 连击脉冲（b42 特效）：连续得分时沿棋盘轮廓向内嵌套的三层六边描边环脉冲。
    /// 参考 2026CIGA 的 BoardHexPulseEffect 用 LineRenderer 世界空间；本项目 uGUI 复用
    /// <see cref="HexagonGraphic"/> 的纯描边环（color=clear + StrokeColor），用 RectTransform 缩放
    /// 驱动扩张、CanvasGroup 驱动透明度，避免每帧重建网格。
    /// 挂 board_root 下随盘面旋转。
    /// </summary>
    public sealed class BoardHexPulseEffect : MonoBehaviour
    {
        [SerializeField] private Color m_LowComboColor = new Color(1f, 0.48f, 0.08f, 1f);

        [SerializeField] private Color m_HighComboColor = new Color(1f, 0.05f, 0.03f, 1f);

        [SerializeField, Range(0.1f, 1.2f)] private float m_Duration = 0.45f;

        [SerializeField, Range(0f, 1f)] private float m_MinAlpha = 0.35f;

        [SerializeField, Range(0f, 1f)] private float m_MaxAlpha = 0.9f;

        [SerializeField, Range(0f, 0.8f)] private float m_ExpandDistance = 0.32f;

        [SerializeField, Range(0f, 0.5f)] private float m_Irregularity = 0.12f;

        [SerializeField] private int m_FullCombo = 8;

        private const int RingCount = 3;

        private readonly float[] _ringDelay = { 0f, 0.07f, 0.14f };

        private readonly float[] _ringScale = { 1f, 0.72f, 0.48f };

        private readonly RectTransform[] _rings = new RectTransform[RingCount];

        private readonly CanvasGroup[] _groups = new CanvasGroup[RingCount];

        private float _outerRadius;

        private float _comboT;

        private int _combo;

        /// <summary>注入轮廓半径（像素）；环以该半径为最大层基准。</summary>
        public void Setup(float outerRadius)
        {
            _outerRadius = outerRadius;
            EnsureRings();
            ApplyHidden();
        }

        /// <summary>按连击数触发脉冲。combo 越大颜色越红、透明度峰值越高、扩张越强。</summary>
        public void Pulse(int combo)
        {
            _combo = Mathf.Max(1, combo);
            _comboT = Mathf.Clamp01((combo - 1f) / Mathf.Max(1f, m_FullCombo - 1f));
            EnsureRings();
            if (_outerRadius <= 0f)
            {
                return;
            }

            for (int i = 0; i < RingCount; i++)
            {
                PlayRing(i);
            }
        }

        private void PlayRing(int i)
        {
            RectTransform ring = _rings[i];
            CanvasGroup group = _groups[i];
            if (ring == null || group == null)
            {
                return;
            }

            ring.DOKill();
            group.DOKill();

            float baseScale = _ringScale[i];
            float expand = m_ExpandDistance * (0.25f + _comboT) * (1f + i * 0.45f);
            float irregular = m_Irregularity * (0.5f + _comboT);
            float peakAlpha = Mathf.Lerp(m_MinAlpha, m_MaxAlpha, _comboT) * _ringScale[i];

            ring.localScale = Vector3.one * baseScale;
            group.alpha = peakAlpha;

            float targetScale = baseScale * (1f + expand + irregular);
            ring.DOScale(Vector3.one * targetScale, m_Duration)
                .SetDelay(_ringDelay[i])
                .SetEase(Ease.OutCubic);
            group.DOFade(0f, m_Duration)
                .SetDelay(_ringDelay[i])
                .SetEase(Ease.InQuad);
        }

        private void ApplyHidden()
        {
            for (int i = 0; i < RingCount; i++)
            {
                if (_groups[i] != null)
                {
                    _groups[i].alpha = 0f;
                }

                if (_rings[i] != null)
                {
                    _rings[i].localScale = Vector3.one * _ringScale[i];
                }
            }
        }

        private void EnsureRings()
        {
            for (int i = 0; i < RingCount; i++)
            {
                if (_rings[i] != null)
                {
                    continue;
                }

                var go = new GameObject("combo_pulse_" + i, typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(transform, false);
                RectTransform rt = (RectTransform)go.transform;
                rt.anchoredPosition = Vector2.zero;
                float w = Mathf.Sqrt(3f) * _outerRadius;
                float h = 2f * _outerRadius;
                rt.sizeDelta = new Vector2(w, h);

                var graphic = go.AddComponent<HexagonGraphic>();
                graphic.Circumradius = _outerRadius;
                graphic.color = Color.clear;
                graphic.StrokeWidth = 4f * _ringScale[i];
                graphic.StrokeColor = m_LowComboColor;
                graphic.raycastTarget = false;

                var group = go.AddComponent<CanvasGroup>();
                group.interactable = false;
                group.blocksRaycasts = false;
                group.alpha = 0f;

                _rings[i] = rt;
                _groups[i] = group;
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < RingCount; i++)
            {
                if (_rings[i] != null)
                {
                    _rings[i].DOKill();
                }

                if (_groups[i] != null)
                {
                    _groups[i].DOKill();
                }
            }
        }
    }
}
