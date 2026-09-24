using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 项目侧 Toast 与 Loading 宿主（P0-007）。不派生 UIFormBase：
    /// 用独立 Canvas（overrideSorting + sortingOrder=500，对齐 Overlay 分组深度）承载，
    /// 因此 Toast 显示在弹窗（Dialog=200）之上，Loading 全屏屏蔽下层交互。
    /// </summary>
    public sealed class GlobalUIRoot : MonoBehaviour
    {
        private const int OverlaySortOrder = 500;
        private const float ToastDurationSeconds = 2f;

        private RectTransform m_ToastContainer = null;
        private GameObject m_LoadingRoot = null;
        private readonly Queue<ToastItem> m_ToastPool = new Queue<ToastItem>();
        private readonly List<ToastItem> m_ActiveToasts = new List<ToastItem>();
        private int m_LoadingRefCount = 0;

        private void Awake()
        {
            BuildHierarchy();
        }

        private void BuildHierarchy()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = OverlaySortOrder;

            // Toast 容器：底部居中，VerticalLayoutGroup 保证同帧多个 Toast 顺序确定（插入序）。
            var containerGo = new GameObject("ToastContainer", typeof(RectTransform));
            containerGo.transform.SetParent(canvasGo.transform, false);
            m_ToastContainer = containerGo.GetComponent<RectTransform>();
            m_ToastContainer.anchorMin = new Vector2(0.5f, 0.04f);
            m_ToastContainer.anchorMax = new Vector2(0.5f, 0.04f);
            m_ToastContainer.pivot = new Vector2(0.5f, 0f);
            m_ToastContainer.sizeDelta = new Vector2(760f, 0f);
            var layout = containerGo.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.spacing = 12f;

            // Loading 全屏遮罩（屏蔽下层交互）。
            var loadingGo = new GameObject("Loading", typeof(RectTransform), typeof(Image));
            loadingGo.transform.SetParent(canvasGo.transform, false);
            var loadingRt = loadingGo.GetComponent<RectTransform>();
            Stretch(loadingRt);
            var loadingImg = loadingGo.GetComponent<Image>();
            loadingImg.color = new Color(0f, 0f, 0f, 0.55f);
            loadingImg.raycastTarget = true;
            m_LoadingRoot = loadingGo;
            m_LoadingRoot.SetActive(false);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(loadingGo.transform, false);
            Stretch(labelGo.GetComponent<RectTransform>());
            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = "加载中…";
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 48;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
        }

        public void ShowToast(string message)
        {
            ToastItem item = AcquireToast();
            item.SetMessage(message);
            item.transform.SetParent(m_ToastContainer, false);
            item.gameObject.SetActive(true);
            m_ActiveToasts.Add(item);
            StartCoroutine(ExpireToast(item));
        }

        public void ShowLoading()
        {
            m_LoadingRefCount++;
            m_LoadingRoot.SetActive(true);
        }

        public void HideLoading()
        {
            m_LoadingRefCount = Mathf.Max(0, m_LoadingRefCount - 1);
            if (m_LoadingRefCount == 0)
            {
                m_LoadingRoot.SetActive(false);
            }
        }

        private ToastItem AcquireToast()
        {
            if (m_ToastPool.Count > 0)
            {
                return m_ToastPool.Dequeue();
            }
            return BuildToast();
        }

        private ToastItem BuildToast()
        {
            var go = new GameObject("Toast", typeof(RectTransform), typeof(Image), typeof(ToastItem));
            go.transform.SetParent(m_ToastContainer, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.8f);
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(680f, 84f);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            Stretch(labelGo.GetComponent<RectTransform>());
            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = string.Empty;
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 32;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            return go.GetComponent<ToastItem>();
        }

        private IEnumerator ExpireToast(ToastItem item)
        {
            yield return new WaitForSeconds(ToastDurationSeconds);
            if (item == null)
            {
                yield break;
            }
            item.gameObject.SetActive(false);
            m_ActiveToasts.Remove(item);
            m_ToastPool.Enqueue(item);
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
