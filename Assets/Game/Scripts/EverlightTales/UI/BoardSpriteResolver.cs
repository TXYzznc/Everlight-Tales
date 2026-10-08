using Everlight.Tales.Board;
using Everlight.Tales.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>预览和实际盘面共用的对象资源解析；未知对象保留程序绘制回退。</summary>
    public static class BoardSpriteResolver
    {
        public static Sprite Resolve(UIFormalSpriteCatalog catalog, BoardEntity entity, string eventId = null)
        {
            if (catalog == null || entity == null) return null;
            if (entity.Kind == EntityKind.Part) return entity.PartType == PartType.None ? null : catalog.CurrentPart(entity.PartType, entity.FormId);
            if (entity.ObstacleType != ObstacleType.None) return catalog.Get("obstacle:" + entity.ObstacleType);
            if (entity.AnomalyType != AnomalyType.None) return catalog.Get("anomaly:" + entity.AnomalyType);
            if (eventId == "EV-N01" && entity.Goal != null && entity.Goal.Id == "gate") return catalog.Get("维修卷帘门");
            if (entity.Kind == EntityKind.TaskMarker) return catalog.Get("标记任务标记");
            if (entity.Kind == EntityKind.RepairTarget) return catalog.Get("标记维修对象");
            return null;
        }
        public static Image AddArt(Transform parent, Sprite sprite, float diameter, float rotation = 0f)
        {
            if (sprite == null) return null;
            var go = new GameObject("EntityArt", typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>(); image.sprite = sprite; image.type = Image.Type.Simple;
            image.preserveAspect = true; image.raycastTarget = false; image.color = Color.white;
            image.rectTransform.sizeDelta = new Vector2(diameter, diameter); image.rectTransform.localEulerAngles = new Vector3(0,0,rotation);
            return image;
        }
    }
}
