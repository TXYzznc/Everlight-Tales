#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>为地图事件、图鉴和工作台列表补齐预制体中的 ScrollRect 与内容容器。</summary>
    public static class UiScrollingLayoutMigration
    {
        private const string UiRoot = "Assets/Game/Prefabs/UI";

        [MenuItem("Game Framework/EverlightTales/UI/补齐列表滚动与地图画布", priority = 2098)]
        public static void Run()
        {
            int changed = 0;
            foreach (string file in Directory.GetFiles(UiRoot, "*.prefab", SearchOption.TopDirectoryOnly))
            {
                string path = file.Replace('\\', '/');
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    bool touched = false;
                    foreach (Transform list in FindAll(root.transform, "List_Event")) touched |= EnsureEventScroll(list as RectTransform);
                    foreach (Transform scroll in FindAll(root.transform, "PartScroll")) touched |= EnsureVerticalScroll(scroll as RectTransform, "PartContent");
                    foreach (Transform scroll in FindAll(root.transform, "FormScroll")) touched |= EnsureVerticalScroll(scroll as RectTransform, "FormContent");
                    foreach (Transform scroll in FindAll(root.transform, "CaseScroll")) touched |= EnsureVerticalScroll(scroll as RectTransform, "CaseContent");
                    foreach (Transform scroll in FindAll(root.transform, "MaterialsScroll")) touched |= EnsureVerticalScroll(scroll as RectTransform, "MaterialsContent");
                    foreach (Transform scroll in FindAll(root.transform, "LedgerScroll")) touched |= EnsureVerticalScroll(scroll as RectTransform, "LedgerContent");
                    foreach (Transform map in FindAll(root.transform, "Panel_MapView")) touched |= EnsureMapCanvas(map as RectTransform);
                    Transform tabBar = FindFirst(root.transform, "TabBar");
                    if (tabBar != null) touched |= EnsureNavigationCanvas(tabBar.gameObject);
                    if (root.name == "MainPageShell")
                    {
                        Transform topBar = FindFirst(root.transform, "TopBar");
                        if (topBar != null) touched |= EnsureNavigationCanvas(topBar.gameObject);
                    }
                    if (root.name == "MainPageShell") touched |= RemoveEmbeddedShellPanels(root.transform);
                    if (!touched) continue;
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    changed++;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UiScrollingLayoutMigration] 已更新 " + changed + " 个 UI 预制体。");
        }

        private static bool EnsureEventScroll(RectTransform viewport)
        {
            if (viewport == null) return false;
            RectTransform content = EnsureContent(viewport, "EventContent");
            GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>() ?? content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(440f, 96f);
            grid.spacing = new Vector2(12f, 12f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.UpperLeft;
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return EnsureScroll(viewport, content);
        }

        private static bool EnsureVerticalScroll(RectTransform viewport, string contentName)
        {
            if (viewport == null) return false;
            RectTransform content = EnsureContent(viewport, contentName);
            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>() ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return EnsureScroll(viewport, content);
        }

        private static bool EnsureScroll(RectTransform viewport, RectTransform content)
        {
            Image image = viewport.GetComponent<Image>() ?? viewport.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;
            if (viewport.gameObject.GetComponent<RectMask2D>() == null) viewport.gameObject.AddComponent<RectMask2D>();
            ScrollRect scroll = viewport.GetComponent<ScrollRect>() ?? viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic; scroll.scrollSensitivity = 10f;
            return true;
        }

        private static RectTransform EnsureContent(RectTransform viewport, string contentName)
        {
            Transform existing = viewport.Find(contentName);
            if (existing != null) return existing as RectTransform;
            var go = new GameObject(contentName, typeof(RectTransform));
            RectTransform content = go.transform as RectTransform;
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var children = new List<Transform>();
            foreach (Transform child in viewport)
            {
                if (child != content) children.Add(child);
            }
            foreach (Transform child in children) child.SetParent(content, false);
            return content;
        }

        private static bool EnsureMapCanvas(RectTransform map)
        {
            if (map == null) return false;
            RectTransform content = map.Find("MapContent") as RectTransform;
            if (content == null)
            {
                var go = new GameObject("MapContent", typeof(RectTransform));
                content = go.transform as RectTransform;
                content.SetParent(map, false);
                content.anchorMin = new Vector2(0.5f, 0.5f);
                content.anchorMax = new Vector2(0.5f, 0.5f);
                content.pivot = new Vector2(0.5f, 0.5f);
                content.sizeDelta = new Vector2(1080f, 1200f);
            }

            Transform input = map.Find("MapCanvasInput");
            if (input == null)
            {
                var go = new GameObject("MapCanvasInput", typeof(RectTransform), typeof(Image), typeof(MapCanvasInteraction));
                input = go.transform;
                input.SetParent(map, false);
                input.SetAsFirstSibling();
                RectTransform rect = input as RectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                Image image = go.GetComponent<Image>();
                image.color = new Color(0f, 0f, 0f, 0f);
                image.raycastTarget = true;
            }
            input.GetComponent<MapCanvasInteraction>().Bind(content);
            return true;
        }

        private static IEnumerable<Transform> FindAll(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name) yield return child;
            }
        }

        private static Transform FindFirst(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name) return child;
            }
            return null;
        }

        private static bool EnsureNavigationCanvas(GameObject tabBar)
        {
            Canvas canvas = tabBar.GetComponent<Canvas>();
            if (canvas == null) canvas = tabBar.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            if (tabBar.GetComponent<GraphicRaycaster>() == null) tabBar.AddComponent<GraphicRaycaster>();
            return true;
        }

        private static bool RemoveEmbeddedShellPanels(Transform root)
        {
            bool changed = false;
            foreach (string name in new[] { "Panel_Map", "Panel_Journal", "Panel_Home" })
            {
                // MainPageShell 只保留导航宿主。面板可能位于 SafeArea/Content 的不同层级，
                // 不能只检查某一个名为 Content 的直接子节点。
                var panels = new List<Transform>(FindAll(root, name));
                foreach (Transform panel in panels)
                {
                    if (panel == null) continue;
                    Object.DestroyImmediate(panel.gameObject);
                    changed = true;
                }
            }
            return changed;
        }
    }
}
#endif
