using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 项目侧 Loading 宿主（P0-007）。Toast 使用已注册的 DialogView UIDialog，
    /// 此组件只负责加载遮罩引用计数。
    /// </summary>
    public sealed class GlobalUIRoot : MonoBehaviour
    {
        private GameObject m_LoadingRoot = null;
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
            canvas.sortingOrder = 500;

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

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
