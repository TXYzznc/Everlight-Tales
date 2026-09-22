using DG.Tweening;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 按钮粒子爆发（b42 特效）：拍击按钮按下时向外飞散的暖色小六边火花。
    /// 参考 2026CIGA 的 SmackButtonParticlesEffect（ParticleSystem 世界空间）；
    /// 本项目 uGUI 用 <see cref="HexagonGraphic"/> 填充块 + DOTween 位移/缩放/淡出，火花挂页面根不随盘面旋转。
    /// </summary>
    public sealed class ButtonParticlesEffect : MonoBehaviour
    {
        [SerializeField, Range(4, 40)] private int m_BurstCount = 14;

        [SerializeField] private Color m_Color = new Color(1f, 0.92f, 0.5f, 1f);

        [SerializeField, Range(20f, 160f)] private float m_Speed = 70f;

        [SerializeField, Range(0.1f, 1f)] private float m_Duration = 0.4f;

        [SerializeField, Range(2f, 14f)] private float m_Size = 6f;

        /// <summary>在页面本地坐标处触发一次粒子爆发（火花从此点向外飞散）。</summary>
        public void Burst(Vector2 pageLocalPosition)
        {
            for (int i = 0; i < m_BurstCount; i++)
            {
                var go = new GameObject("btn_spark", typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(transform, false);
                RectTransform rt = (RectTransform)go.transform;
                rt.anchoredPosition = pageLocalPosition;
                rt.sizeDelta = new Vector2(Mathf.Sqrt(3f) * m_Size, 2f * m_Size);

                var graphic = go.AddComponent<HexagonGraphic>();
                graphic.Circumradius = m_Size;
                graphic.color = m_Color;
                graphic.raycastTarget = false;

                var group = go.AddComponent<CanvasGroup>();
                group.interactable = false;
                group.blocksRaycasts = false;

                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 target = pageLocalPosition + dir * m_Speed * Random.Range(0.5f, 1.3f);

                float dur = m_Duration * Random.Range(0.7f, 1.3f);
                rt.DOAnchorPos(target, dur).SetEase(Ease.OutCubic);
                rt.DOScale(0f, dur).SetEase(Ease.InQuad);
                group.DOFade(0f, dur).SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        if (go != null)
                        {
                            Destroy(go);
                        }
                    });
            }
        }
    }
}
