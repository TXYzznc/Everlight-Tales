using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 把自身（或指定目标）RectTransform 适配到 <see cref="Screen.safeArea"/>，
    /// 使挂在其下的 UI 内容避开刘海、圆角与底部手势条等不安全区。
    ///
    /// 适配以 anchorMin/anchorMax 表达：安全区之外的上下边缘自动成为留白，
    /// 因此在更窄或更长的竖屏宽高比下，内容都能保持在安全区内、不被裁切。
    /// 该组件不依赖真机观察——可在编辑器 Game 视图的设备模拟（如 iPhone 刘海机型）
    /// 下直接通过安全区变化验证锚点行为。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("要适配的目标 RectTransform；留空则适配自身。")]
        private RectTransform m_Target = null;

        private Rect m_LastSafeArea = default;
        private Vector2 m_LastScreenSize = default;

        private RectTransform Target
        {
            get { return m_Target != null ? m_Target : (RectTransform)transform; }
        }

        private void Awake()
        {
            Apply();
        }

        private void Update()
        {
            // 屏幕尺寸或安全区变化（旋转、分屏、切换模拟机型）时重新应用。
            if (Screen.width != (int)m_LastScreenSize.x
                || Screen.height != (int)m_LastScreenSize.y
                || Screen.safeArea != m_LastSafeArea)
            {
                Apply();
            }
        }

        /// <summary>
        /// 立即按当前 <see cref="Screen.safeArea"/> 重算锚点。
        /// </summary>
        public void Apply()
        {
            Rect safe = Screen.safeArea;
            m_LastSafeArea = safe;
            m_LastScreenSize = new Vector2(Screen.width, Screen.height);

            RectTransform rt = Target;
            if (rt == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
