#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>
    /// 逐页整理 HomePage 五个复用页面的布局。每个入口只修改一个正式页面预制体，
    /// 便于在 Inspector 中逐页复核，也避免重新生成 HomePage 静态子节点。
    /// </summary>
    public static class HomePagePrefabPolish
    {
        private const string Root = "Assets/Game/Prefabs/UI/";
        private const string ButtonSprite = "Assets/Game/Sprites/UI/控件/SHR-022-normal次按钮.png";

        [MenuItem("Game Framework/EverlightTales/UI/逐页整理 HomePage 五区/GuestPage")]
        public static void PolishGuestPage() => Edit("GuestPage", PolishGuest);

        [MenuItem("Game Framework/EverlightTales/UI/逐页整理 HomePage 五区/WorkbenchPage")]
        public static void PolishWorkbenchPage() => Edit("WorkbenchPage", PolishWorkbench);

        [MenuItem("Game Framework/EverlightTales/UI/逐页整理 HomePage 五区/CodexPage")]
        public static void PolishCodexPage() => Edit("CodexPage", PolishCodex);

        [MenuItem("Game Framework/EverlightTales/UI/逐页整理 HomePage 五区/ArchivePage")]
        public static void PolishArchivePage() => Edit("ArchivePage", PolishArchive);

        [MenuItem("Game Framework/EverlightTales/UI/逐页整理 HomePage 五区/ServicePage")]
        public static void PolishServicePage() => Edit("ServicePage", PolishService);

        private static void Edit(string page, Action<GameObject> polish)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请先停止 Play Mode。");
            string path = Root + page + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                polish(root);
                EditorUtility.SetDirty(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HomePagePrefabPolish] 已整理 " + page + "。");
        }

        private static void PolishGuest(GameObject root)
        {
            Transform panel = Find(root.transform, "Panel_Guest");
            Transform content = Find(panel, "Panel_GuestContent");
            if (panel == null || content == null) throw new InvalidOperationException("GuestPage 缺少 Panel_Guest 或 Panel_GuestContent。");
            // 新版固定分区以预制体为准，不再生成旧的平铺标题和滚动区。
            if (content.Find("Section_0") != null) return;
            Stretch(panel as RectTransform);
            Inset(content as RectTransform, 24f, 24f, 24f, 24f);

            SetText(content, "Txt_Thanks", "感谢", 26f);
            SetText(content, "Txt_DelegationHeader", "当天到店委托", 26f);
            SetText(content, "Txt_ModHeader", "改装支线", 26f);
            RectTransform thanks = EnsureScroll(content, "ThanksScrollRect", "ThanksContent");
            RectTransform delegation = EnsureScroll(content, "DelegationScrollRect", "DelegationContent");
            RectTransform mod = EnsureScroll(content, "ModScrollRect", "ModContent");
            SetTopRect(Find(content, "Txt_Thanks") as RectTransform, -20f, 36f);
            SetTopRect(thanks, -64f, 280f);
            SetTopRect(Find(content, "Txt_DelegationHeader") as RectTransform, -364f, 36f);
            SetTopRect(delegation, -408f, 300f);
            SetTopRect(Find(content, "Txt_ModHeader") as RectTransform, -728f, 36f);
            SetTopRect(mod, -772f, 300f);

            GuestPanel guest = root.GetComponentInChildren<GuestPanel>(true);
            if (guest != null)
            {
                SerializedObject so = new SerializedObject(guest);
                Set(so, "m_Root", content);
                Set(so, "m_ThanksContent", Find(thanks, "Viewport/ThanksContent"));
                Set(so, "m_DelegationContent", Find(delegation, "Viewport/DelegationContent"));
                Set(so, "m_ModContent", Find(mod, "Viewport/ModContent"));
                Set(so, "_thanks", Component<TMP_Text>(content, "Txt_Thanks"));
                Set(so, "_delegationHeader", Component<TMP_Text>(content, "Txt_DelegationHeader"));
                Set(so, "_modHeader", Component<TMP_Text>(content, "Txt_ModHeader"));
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void PolishWorkbench(GameObject root)
        {
            Transform panel = Find(root.transform, "Panel_Workbench");
            Transform top = Find(panel, "Panel_WorkbenchTop");
            Transform body = Find(panel, "Panel_WorkbenchBody");
            if (panel == null || top == null || body == null) throw new InvalidOperationException("WorkbenchPage 缺少工作台根节点。");
            Stretch(panel as RectTransform);
            SetTopRect(top as RectTransform, 0f, 112f);
            StretchBelow(body as RectTransform, 112f);
            Transform materials = Find(body, "Panel_Materials");
            Transform ledger = Find(body, "Panel_Ledger");
            Transform workbench = Find(body, "Panel_Workbench");
            Stretch(workbench as RectTransform);
            Stretch(materials as RectTransform);
            Stretch(ledger as RectTransform);
            RectTransform scroll = Find(materials, "MaterialsScroll") as RectTransform;
            RectTransform detail = Find(materials, "Panel_MaterialDetail") as RectTransform;
            if (scroll != null) SetTopRect(scroll, -18f, 430f);
            if (detail != null) SetBottomRect(detail, 18f, 370f);
        }

        private static void PolishCodex(GameObject root)
        {
            Transform panel = Find(root.transform, "Panel_Codex");
            Transform top = Find(panel, "Panel_CodexTop");
            Transform list = Find(panel, "Panel_CodexList");
            if (panel == null || top == null || list == null) throw new InvalidOperationException("CodexPage 缺少图鉴根节点。");
            Stretch(panel as RectTransform);
            SetTopRect(top as RectTransform, 0f, 112f);
            StretchBelow(list as RectTransform, 112f);
        }

        private static void PolishArchive(GameObject root)
        {
            Transform panel = Find(root.transform, "Panel_Archive");
            Transform top = Find(panel, "Panel_ArchiveTop");
            Transform list = Find(panel, "Panel_ArchiveList");
            if (panel == null || top == null || list == null) throw new InvalidOperationException("ArchivePage 缺少保管根节点。");
            Stretch(panel as RectTransform);
            SetTopRect(top as RectTransform, 0f, 160f);
            StretchBelow(list as RectTransform, 160f);
        }

        private static void PolishService(GameObject root)
        {
            Transform panel = Find(root.transform, "Panel_Service");
            Transform content = Find(panel, "Panel_ServiceContent");
            if (panel == null || content == null) throw new InvalidOperationException("ServicePage 缺少服务根节点。");
            Stretch(panel as RectTransform);
            Inset(content as RectTransform, 32f, 32f, 32f, 32f);
            SetText(content, "Txt_Title", "服务", 30f);
            SetText(content, "Txt_Hint", "快捷入口（非经营）", 20f);
            SetTopRect(Find(content, "Txt_Title") as RectTransform, -100f, 48f);
            SetTopRect(Find(content, "Txt_Hint") as RectTransform, -160f, 36f);
            SetButton(Find(content, "Btn_Loadout") as RectTransform, "配装（携带选择）", -300f);
            SetButton(Find(content, "Btn_Recover") as RectTransform, "恢复面板", -390f);
            SetButton(Find(content, "Btn_Settings") as RectTransform, "设置", -480f);
        }

        private static RectTransform EnsureScroll(Transform parent, string name, string contentName)
        {
            Transform existing = parent.Find(name);
            GameObject scrollObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(ScrollRect));
            if (existing == null) scrollObject.transform.SetParent(parent, false);
            RectTransform scroll = scrollObject.transform as RectTransform;
            ScrollRect sr = scrollObject.GetComponent<ScrollRect>() ?? scrollObject.AddComponent<ScrollRect>();
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic; sr.scrollSensitivity = 10f;
            Transform viewport = scroll.Find("Viewport");
            GameObject viewportObject = viewport != null ? viewport.gameObject : new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            if (viewport == null) viewportObject.transform.SetParent(scroll, false);
            RectTransform viewportRect = viewportObject.transform as RectTransform;
            Stretch(viewportRect);
            Transform content = viewportRect.Find(contentName);
            if (content == null)
            {
                Transform legacyContent = parent.Find(contentName);
                if (legacyContent != null)
                {
                    legacyContent.SetParent(viewportRect, false);
                    content = legacyContent;
                }
            }
            GameObject contentObject = content != null ? content.gameObject : new GameObject(contentName, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            if (content == null) contentObject.transform.SetParent(viewportRect, false);
            RectTransform contentRect = contentObject.transform as RectTransform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>() ?? contentObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 8, 8);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>() ?? contentObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.viewport = viewportRect;
            sr.content = contentRect;
            return scroll;
        }

        private static void SetButton(RectTransform rect, string text, float y)
        {
            if (rect == null) return;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(420f, 72f);
            Image image = rect.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSprite);
                image.type = Image.Type.Sliced;
                image.color = Color.white;
            }
            SetText(rect, "Txt_Label", text, 24f);
        }

        private static void SetText(Transform root, string name, string text, float size)
        {
            Transform target = Find(root, name);
            if (target == null) return;
            TMP_Text label = target.GetComponent<TMP_Text>();
            if (label == null) return;
            label.text = text;
            label.fontSize = size;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
        }

        private static void SetTopRect(RectTransform rect, float y, float height)
        {
            if (rect == null) return;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(0f, height);
        }

        private static void SetBottomRect(RectTransform rect, float y, float height)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(0f, height);
        }

        private static void StretchBelow(RectTransform rect, float top)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(0f, -top);
        }

        private static void Stretch(RectTransform rect)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void Inset(RectTransform rect, float left, float right, float top, float bottom)
        {
            if (rect == null) return;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static Transform Find(Transform root, string path) => root == null ? null : root.Find(path);
        private static T Component<T>(Transform root, string path) where T : Component
        {
            Transform target = Find(root, path);
            return target == null ? null : target.GetComponent<T>();
        }

        private static void Set(SerializedObject so, string name, UnityEngine.Object value)
        {
            SerializedProperty property = so.FindProperty(name);
            if (property != null) property.objectReferenceValue = value;
        }
    }
}
#endif
