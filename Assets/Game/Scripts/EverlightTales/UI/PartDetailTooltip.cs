using Everlight.Tales.Data;
using TMPro;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>通用零件详情浮窗。布局来自 PartDetailTooltip 预制体。</summary>
    public sealed class PartDetailTooltip : MonoBehaviour
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _body;
        private RectTransform _rect;
        private RectTransform _parentRect;
        private Canvas _canvas;

        private void Awake()
        {
            EnsureReferences();
        }

        public void Show(PartType part, string formId, Vector2 screenPosition)
        {
            EnsureReferences();
            if (_rect == null || _parentRect == null || _title == null || _body == null)
            {
                return;
            }

            _title.text = PartDetailFormatter.DisplayName(part, formId);
            _body.text = PartDetailFormatter.Build(part, formId);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            Camera eventCamera = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera
                : null;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, screenPosition, eventCamera, out Vector2 local))
            {
                return;
            }

            Vector2 size = _rect.rect.size;
            Rect parent = _parentRect.rect;
            float halfWidth = size.x * 0.5f;
            float halfHeight = size.y * 0.5f;
            local += new Vector2(24f, 24f);
            local.x = Mathf.Clamp(local.x, parent.xMin + halfWidth + 16f, parent.xMax - halfWidth - 16f);
            local.y = Mathf.Clamp(local.y, parent.yMin + halfHeight + 16f, parent.yMax - halfHeight - 16f);
            _rect.anchoredPosition = local;
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private void EnsureReferences()
        {
            if (_rect == null) _rect = transform as RectTransform;
            if (_parentRect == null && _rect != null) _parentRect = _rect.parent as RectTransform;
            if (_canvas == null) _canvas = GetComponentInParent<Canvas>();
            if (_title == null)
            {
                Transform child = transform.Find("Txt_PartDetailTitle") ?? transform.Find("Txt_CarryDetailTitle");
                if (child != null) _title = child.GetComponent<TMP_Text>();
            }
            if (_body == null)
            {
                Transform child = transform.Find("Txt_PartDetailBody") ?? transform.Find("Txt_CarryDetailBody");
                if (child != null) _body = child.GetComponent<TMP_Text>();
            }
        }
    }
}
