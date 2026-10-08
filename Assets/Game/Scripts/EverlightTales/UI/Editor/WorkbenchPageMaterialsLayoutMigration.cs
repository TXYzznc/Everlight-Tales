#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>为独立 WorkbenchPage 补齐材料滚动列表和详情面板。</summary>
    public static class WorkbenchPageMaterialsLayoutMigration
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/WorkbenchPage.prefab";

        [MenuItem("Game Framework/EverlightTales/UI/修复 WorkbenchPage 材料列表与详情")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("请先停止 Play Mode。");
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                WorkbenchPanel panel = root.GetComponentInChildren<WorkbenchPanel>(true);
                if (panel == null) throw new System.InvalidOperationException("WorkbenchPage.prefab 中缺少 WorkbenchPanel。");
                Transform materials = panel.transform.Find("Panel_WorkbenchBody/Panel_Materials");
                if (materials == null) throw new System.InvalidOperationException("WorkbenchPage.prefab 缺少 Panel_WorkbenchBody/Panel_Materials。");
                RectTransform scroll = Rect(materials, "MaterialsScroll");
                scroll.anchorMin = new Vector2(0, 1); scroll.anchorMax = Vector2.one; scroll.pivot = new Vector2(.5f, 1);
                scroll.offsetMin = new Vector2(12, 410); scroll.offsetMax = new Vector2(-12, -18);
                Ensure<Image>(scroll.gameObject).color = new Color(0, 0, 0, 0);
                ScrollRect sr = Ensure<ScrollRect>(scroll.gameObject); sr.horizontal = false; sr.vertical = true; sr.movementType = ScrollRect.MovementType.Elastic; sr.scrollSensitivity = 10f;
                RectTransform viewport = Rect(scroll, "Viewport"); Stretch(viewport); Ensure<Image>(viewport.gameObject).color = new Color(0, 0, 0, 0); Ensure<RectMask2D>(viewport.gameObject);
                RectTransform content = Rect(viewport, "MaterialsContent"); content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(.5f, 1); content.anchoredPosition = Vector2.zero; content.sizeDelta = Vector2.zero;
                VerticalLayoutGroup layout = Ensure<VerticalLayoutGroup>(content.gameObject); layout.padding = new RectOffset(0, 12, 0, 12); layout.spacing = 8; layout.childControlWidth = true; layout.childControlHeight = false; layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
                ContentSizeFitter fitter = Ensure<ContentSizeFitter>(content.gameObject); fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; sr.viewport = viewport; sr.content = content;
                RectTransform detail = Rect(materials, "Panel_MaterialDetail"); detail.anchorMin = Vector2.zero; detail.anchorMax = new Vector2(1, 0); detail.pivot = new Vector2(.5f, 0); detail.anchoredPosition = new Vector2(0, 18); detail.sizeDelta = new Vector2(-24, 370); Ensure<Image>(detail.gameObject).color = new Color(.06f, .08f, .1f, .94f);
                RectTransform icon = Rect(detail, "Icon"); icon.anchorMin = new Vector2(0, 1); icon.anchorMax = new Vector2(0, 1); icon.pivot = new Vector2(0, 1); icon.anchoredPosition = new Vector2(18, -18); icon.sizeDelta = new Vector2(72, 72); Ensure<Image>(icon.gameObject);
                Text(detail, "Txt_Title", "材料详情", 28, new Vector2(108, -18), new Vector2(-126, -12));
                Text(detail, "Txt_Usage", "", 20, new Vector2(24, -104), new Vector2(-24, -150));
                Text(detail, "Txt_Source", "", 20, new Vector2(24, -208), new Vector2(-24, -250));
                Text(detail, "Txt_Flavor", "", 18, new Vector2(24, -280), new Vector2(-24, -24));
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log("[WorkbenchPageMaterialsLayoutMigration] WorkbenchPage 材料滚动列表与详情面板补齐完成。");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }

        private static RectTransform Rect(Transform parent, string name)
        {
            Transform found = parent.Find(name);
            if (found == null) { GameObject go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); found = go.transform; }
            return (RectTransform)found;
        }
        private static T Ensure<T>(GameObject go) where T : Component => go.GetComponent<T>() ?? go.AddComponent<T>();
        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero; }
        private static void Text(Transform parent, string name, string value, float size, Vector2 min, Vector2 max)
        {
            RectTransform rect = Rect(parent, name); rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one; rect.pivot = new Vector2(.5f, 1); rect.offsetMin = min; rect.offsetMax = max;
            TMP_Text text = Ensure<TextMeshProUGUI>(rect.gameObject); text.text = value; text.font = TMP_Settings.defaultFontAsset; text.fontSize = size; text.alignment = TextAlignmentOptions.TopLeft; text.color = Color.white; text.raycastTarget = false;
        }
    }
}
#endif
