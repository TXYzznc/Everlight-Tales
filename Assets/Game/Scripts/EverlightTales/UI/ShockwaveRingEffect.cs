using DG.Tweening;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 拍击冲击波（b42 特效）：每次拍击从盘面中心扩散的瞬态冲击波。
    /// 参考 2026CIGA 的 ShockwaveRingEffect（LineRenderer 圆环 + 火花/碎屑粒子）；
    /// 本项目 uGUI 复用 <see cref="HexagonGraphic"/> 描边六边环 + 小型填充六边火花，
    /// 挂 board_root 下随盘面旋转，播放结束自动销毁瞬态对象。
    /// </summary>
    public sealed class ShockwaveRingEffect : MonoBehaviour
    {
        [SerializeField] private Color m_MainColor = new Color(1f, 0.95f, 0.8f, 0.9f);

        [SerializeField] private Color m_EchoColor = new Color(1f, 0.75f, 0.35f, 0.75f);

        [SerializeField] private Color m_SparkColor = new Color(1f, 0.92f, 0.5f, 1f);

        [SerializeField, Range(0.1f, 1f)] private float m_Duration = 0.45f;

        [SerializeField, Range(0.5f, 2f)] private float m_EndScale = 1.25f;

        [SerializeField, Range(4, 24)] private int m_SparkCount = 10;

        [SerializeField, Range(0.3f, 1.5f)] private float m_SparkRadiusFactor = 0.9f;

        private RectTransform m_BoardRoot;

        private float m_OuterRadius;

        /// <summary>注入盘面根节点与轮廓半径（像素）。</summary>
        public void Setup(RectTransform boardRoot, float outerRadius)
        {
            m_BoardRoot = boardRoot;
            m_OuterRadius = outerRadius;
        }

        /// <summary>在盘面本地坐标处播放一次冲击波（通常为盘面中心 Vector2.zero）。</summary>
        public void Play(Vector2 boardLocalPos)
        {
            if (m_BoardRoot == null || m_OuterRadius <= 0f)
            {
                return;
            }

            var host = new GameObject("shockwave", typeof(RectTransform));
            host.transform.SetParent(m_BoardRoot, false);
            var hostRt = (RectTransform)host.transform;
            hostRt.anchoredPosition = boardLocalPos;
            hostRt.sizeDelta = Vector2.zero;

            SpawnRing(host.transform, m_MainColor, 0f, m_EndScale);
            SpawnRing(host.transform, m_EchoColor, 0.05f, m_EndScale * 0.82f);
            SpawnSparks(host.transform);

            DOVirtual.DelayedCall(m_Duration * 1.5f, () =>
            {
                if (host != null)
                {
                    Destroy(host);
                }
            });
        }

        private void SpawnRing(Transform host, Color color, float delay, float endScale)
        {
            var go = new GameObject("ring", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(host, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = Vector2.zero;
            float w = Mathf.Sqrt(3f) * m_OuterRadius;
            float h = 2f * m_OuterRadius;
            rt.sizeDelta = new Vector2(w, h);

            var graphic = go.AddComponent<HexagonGraphic>();
            graphic.Circumradius = m_OuterRadius;
            graphic.color = Color.clear;
            graphic.StrokeWidth = 5f;
            graphic.StrokeColor = color;
            graphic.raycastTarget = false;

            var group = go.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            group.alpha = 0f;

            float startScale = 0.25f;
            rt.localScale = Vector3.one * startScale;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.AppendCallback(() => group.alpha = color.a);
            seq.Join(rt.DOScale(Vector3.one * endScale, m_Duration).SetEase(Ease.OutCubic));
            seq.Join(group.DOFade(0f, m_Duration).SetEase(Ease.InQuad));
        }

        private void SpawnSparks(Transform host)
        {
            for (int i = 0; i < m_SparkCount; i++)
            {
                var go = new GameObject("spark", typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(host, false);
                RectTransform rt = (RectTransform)go.transform;
                rt.anchoredPosition = Vector2.zero;
                float r = 6f;
                rt.sizeDelta = new Vector2(Mathf.Sqrt(3f) * r, 2f * r);

                var graphic = go.AddComponent<HexagonGraphic>();
                graphic.Circumradius = r;
                graphic.color = m_SparkColor;
                graphic.raycastTarget = false;

                var group = go.AddComponent<CanvasGroup>();
                group.interactable = false;
                group.blocksRaycasts = false;

                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                float dist = m_OuterRadius * m_SparkRadiusFactor * Random.Range(0.5f, 1.2f);
                Vector2 target = dir * dist;

                float dur = m_Duration * Random.Range(0.7f, 1.3f);
                rt.DOAnchorPos(target, dur).SetEase(Ease.OutCubic);
                rt.DOScale(0f, dur).SetEase(Ease.InQuad);
                group.DOFade(0f, dur).SetEase(Ease.InQuad);
            }
        }
    }
}
