using Everlight.Tales.Board;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>同一套首案规则标记供准备预览与实际盘面使用，图片不拦截操作。</summary>
    public static class FirstCaseBoardArt
    {
        public static void Render(Transform parent, float cellSize, UIFormalSpriteCatalog catalog, RedShoeLevel level)
        {
            Transform previous = parent.Find("FirstCaseArt");
            if (previous != null) { previous.name = "FirstCaseArt.Retired"; previous.gameObject.SetActive(false); Object.Destroy(previous.gameObject); }
            if (level == null || catalog == null) return;
            var root = new GameObject("FirstCaseArt", typeof(RectTransform)); root.transform.SetParent(parent, false);
            foreach (HexCoord guide in level.Config.RedShoe.GuideCells)
                Mark(root.transform, cellSize, catalog.Get("标记任务标记"), "Guide_" + guide.Q + "_" + guide.R, guide,
                    level.RedShoe.HasCountedGuide(guide) ? new Color(.5f, 1f, .6f) : new Color(1f, 1f, 1f, .45f), .65f);
            foreach (RedShoeTurnCell turn in level.Config.RedShoe.TurnCells)
                Mark(root.transform, cellSize, catalog.Get("转向轨道"), "Turn", turn.Coord, Color.white, .8f);
            bool open = !level.RedShoe.IsBound && RedShoeService.IsBoxRepaired(level.Board, level.Config.RedShoe);
            Mark(root.transform, cellSize, catalog.Get("标记维修对象"), "Entry", level.Config.RedShoe.EntryCoord,
                open ? new Color(.4f, 1f, .6f) : new Color(.6f, .6f, .6f), .8f);
            if (!level.RedShoe.IsRemoved)
            {
                HexCoord next = level.RedShoe.Coord.Neighbor(level.RedShoe.Direction);
                if (level.Board.IsValid(next)) Mark(root.transform, cellSize, catalog.Get("红舞鞋"), "RedShoesFirstStep", next, new Color(1f, 1f, 1f, .25f), 1f);
                Mark(root.transform, cellSize, catalog.Get("红舞鞋"), "RedShoes", level.RedShoe.Coord, Color.white, 1.4f);
            }
        }

        private static void Mark(Transform parent, float size, Sprite sprite, string name, HexCoord coord, Color color, float scale)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>(); image.sprite = sprite; image.color = color;
            image.preserveAspect = true; image.raycastTarget = false;
            image.rectTransform.anchoredPosition = HexLayout.AxialToPixel(coord, size);
            image.rectTransform.sizeDelta = Vector2.one * size * 2f * scale;
        }
    }
}
