using UnityEngine;
using UnityEngine.EventSystems;

namespace Everlight.Tales.UI
{
    /// <summary>地图画布的平移与滚轮缩放输入层。该组件挂在透明输入 Image 上，节点按钮仍位于其上层。</summary>
    public sealed class MapCanvasInteraction : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
    {
        [SerializeField] private RectTransform m_Content;
        [SerializeField] private float m_MinScale = 0.65f;
        [SerializeField] private float m_MaxScale = 1.8f;
        [SerializeField] private float m_ZoomStep = 0.1f;

        private Vector2 m_LastPointer;
        private bool m_Dragging;

        public void Bind(RectTransform content)
        {
            m_Content = content;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (m_Content == null) return;
            m_LastPointer = eventData.position;
            m_Dragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!m_Dragging || m_Content == null) return;
            Vector2 delta = eventData.position - m_LastPointer;
            m_Content.anchoredPosition += delta;
            m_LastPointer = eventData.position;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (m_Content == null) return;
            float next = Mathf.Clamp(m_Content.localScale.x + eventData.scrollDelta.y * m_ZoomStep * 0.1f, m_MinScale, m_MaxScale);
            m_Content.localScale = new Vector3(next, next, 1f);
        }

        private void OnDisable()
        {
            m_Dragging = false;
        }
    }
}
