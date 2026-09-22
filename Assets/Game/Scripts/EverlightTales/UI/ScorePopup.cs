using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 得分弹窗（b42 特效）：得分事件位置弹出「+N」上飘淡出。参考 2026CIGA 的 ShowScorePop 用 TMP +
    /// 协程；本项目用 uGUI Text + DOTween。挂页面根（不随盘面旋转），文字始终正向。
    /// </summary>
    public sealed class ScorePopup : MonoBehaviour
    {
        [SerializeField] private float m_RiseDistance = 60f;

        [SerializeField] private float m_Duration = 0.6f;

        [SerializeField] private int m_FontSize = 26;

        /// <summary>在页面本地坐标位置弹出一条得分文字，动画结束自动销毁。</summary>
        public void Pop(int score, Vector2 pageLocalPosition)
        {
            var go = new GameObject("score_popup_" + score, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(transform, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchoredPosition = pageLocalPosition;
            rt.sizeDelta = new Vector2(160f, 40f);

            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = m_FontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(1f, 0.92f, 0.20f, 1f);
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.text = "+" + score;

            var group = go.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            group.alpha = 1f;

            float targetY = pageLocalPosition.y + m_RiseDistance;
            rt.DOAnchorPosY(targetY, m_Duration).SetEase(Ease.OutCubic);
            group.DOFade(0f, m_Duration).SetEase(Ease.InQuad)
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
