using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.EditorTools
{
    /// <summary>
    /// 将首批没有 JSON 契约的 UIForm 绑定到正式 UI Sprite。
    /// 规则按节点语义匹配，结果写回 Prefab；运行时不做资源查找。
    /// </summary>
    public static class EverlightFormalUiResourceBinder
    {
        private const string UiRoot = "Assets/Game/Sprites/UI/";

        [MenuItem("Game Framework/EverlightTales/UI/接入首批正式资源", priority = 2010)]
        public static void BindFirstBatch()
        {
            BindPrefab("Assets/Game/Prefabs/UI/MainPageShell.prefab");
            BindPrefab("Assets/Game/Prefabs/UI/JournalPage.prefab");
            BindPrefab("Assets/Game/Prefabs/UI/DialogView.prefab");
            // BoardPage 的主要控件目前由 BoardPageForm/BoardPage 运行时装配，
            // 本轮不向领域逻辑注入资源查找；下一批迁移到契约后再绑定。
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightFormalUiResourceBinder] 首批正式资源绑定完成。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/补齐 Journal 筛选数量文本", priority = 2011)]
        public static void EnsureJournalFilterNumbersInPrefab()
        {
            const string path = "Assets/Game/Prefabs/UI/JournalPage.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                Transform list = root.transform.Find("Panel_Journal/Panel_JournalList");
                EnsureJournalListContainers(list);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightFormalUiResourceBinder] Journal 筛选数量文本已补齐。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/整理 HomePage 来客布局", priority = 2012)]
        public static void NormalizeHomePageLayout()
        {
            const string homePath = "Assets/Game/Prefabs/UI/HomePage.prefab";

            GameObject home = PrefabUtility.LoadPrefabContents(homePath);
            try
            {
                EnsureHomeLayout(home);
                // HomePage 的 Panel_HomeArea 只作为运行时挂载容器，不能重新写入静态五区。

                Transform delegation = FindDescendant(home.transform, "DelegationContent");
                if (delegation != null) ConfigureDelegationContent(delegation);
                Transform guestContent = FindDescendant(home.transform, "Panel_GuestContent");
                if (guestContent != null)
                {
                    if (FindDescendant(guestContent, "ThanksScrollRect") == null)
                    {
                        EnsureGuestScrollLayout(guestContent);
                    }
                    ConfigureGuestContent(guestContent);
                }
                PrefabUtility.SaveAsPrefabAsset(home, homePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(home); }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightFormalUiResourceBinder] HomePage 来客布局整理完成。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/清理 HomePage 来客旧节点", priority = 2013)]
        public static void CleanupHomePageGuestLegacyNodes()
        {
            const string homePath = "Assets/Game/Prefabs/UI/HomePage.prefab";
            GameObject home = PrefabUtility.LoadPrefabContents(homePath);
            try
            {
                Transform expectedHome = home.transform.Find("Panel_Home");
                foreach (Transform panel in FindAllDescendants(home.transform, "Panel_Home"))
                {
                    if (panel != expectedHome)
                    {
                        UnityEngine.Object.DestroyImmediate(panel.gameObject);
                    }
                }

                Transform guestContent = expectedHome != null ? FindDescendant(expectedHome, "Panel_GuestContent") : null;
                if (guestContent != null)
                {
                    foreach (string legacyName in new[] { "Btn_Claim", "Txt_ModHint", "Btn_Mod" })
                    {
                        foreach (Transform legacy in FindAllDescendants(guestContent, legacyName))
                        {
                            UnityEngine.Object.DestroyImmediate(legacy.gameObject);
                        }
                    }
                    SplitGuestSections(guestContent);
                }

                PrefabUtility.SaveAsPrefabAsset(home, homePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(home); }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightFormalUiResourceBinder] HomePage 来客旧节点清理完成。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/拆分 HomePage 来客滚动列表", priority = 2014)]
        public static void SplitHomePageGuestScrollLists()
        {
            const string homePath = "Assets/Game/Prefabs/UI/HomePage.prefab";
            GameObject home = PrefabUtility.LoadPrefabContents(homePath);
            try
            {
                Transform panelHome = home.transform.Find("Panel_Home");
                Transform guestContent = panelHome != null ? FindDescendant(panelHome, "Panel_GuestContent") : null;
                if (guestContent == null)
                {
                    Debug.LogError("[EverlightFormalUiResourceBinder] 找不到 Panel_Home/Panel_GuestContent，无法拆分来客滚动列表。");
                    return;
                }

                SplitGuestSections(guestContent);
                PrefabUtility.SaveAsPrefabAsset(home, homePath);
            }
            finally { PrefabUtility.UnloadPrefabContents(home); }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightFormalUiResourceBinder] HomePage 来客三个独立滚动列表拆分完成。");
        }

        private static void SplitGuestSections(Transform guestContent)
        {
            HorizontalLayoutGroup horizontalLayout = guestContent.GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayout != null) UnityEngine.Object.DestroyImmediate(horizontalLayout);
            VerticalLayoutGroup verticalLayout = guestContent.GetComponent<VerticalLayoutGroup>();
            if (verticalLayout != null) UnityEngine.Object.DestroyImmediate(verticalLayout);

            Transform oldScroll = guestContent.Find("GuestScrollRect");
            Transform oldContent = oldScroll != null ? FindDescendant(oldScroll, "Content") : null;
            if (oldContent == null)
            {
                Debug.LogWarning($"[EverlightFormalUiResourceBinder] 来客拆分跳过：GuestScrollRect={oldScroll != null}, Content={oldContent != null}。");
                return;
            }

            Transform thanksHeader = FindDescendant(oldContent, "Txt_Thanks");
            Transform thanksContent = FindDescendant(oldContent, "ThanksContent");
            Transform delegationHeader = FindDescendant(oldContent, "Txt_DelegationHeader");
            Transform delegationContent = FindDescendant(oldContent, "DelegationContent");
            Transform delegationEmpty = FindDescendant(oldContent, "Txt_DelegationEmpty");
            Transform modHeader = FindDescendant(oldContent, "Txt_ModHeader");
            Transform modContent = FindDescendant(oldContent, "ModContent");

            if (thanksHeader == null || thanksContent == null || delegationHeader == null || delegationContent == null || modHeader == null || modContent == null)
            {
                Debug.LogWarning($"[EverlightFormalUiResourceBinder] 来客拆分跳过：ThanksHeader={thanksHeader != null}, ThanksContent={thanksContent != null}, DelegationHeader={delegationHeader != null}, DelegationContent={delegationContent != null}, ModHeader={modHeader != null}, ModContent={modContent != null}。");
                return;
            }

            thanksHeader.SetParent(guestContent, false);
            delegationHeader.SetParent(guestContent, false);
            modHeader.SetParent(guestContent, false);
            if (delegationEmpty != null) delegationEmpty.SetParent(delegationContent, false);

            RectTransform thanksScroll = CreateGuestSectionScroll(guestContent, "ThanksScrollRect", thanksContent, new Vector2(0f, -120f), 210f);
            RectTransform delegationScroll = CreateGuestSectionScroll(guestContent, "DelegationScrollRect", delegationContent, new Vector2(0f, -390f), 340f);
            RectTransform modScroll = CreateGuestSectionScroll(guestContent, "ModScrollRect", modContent, new Vector2(0f, -805f), 330f);
            SetGuestTopRect(thanksHeader, new Vector2(720f, 36f), new Vector2(0f, -78f));
            SetGuestTopRect(delegationHeader, new Vector2(720f, 36f), new Vector2(0f, -350f));
            SetGuestTopRect(modHeader, new Vector2(720f, 36f), new Vector2(0f, -765f));

            if (oldScroll != null) UnityEngine.Object.DestroyImmediate(oldScroll.gameObject);
        }

        private static RectTransform CreateGuestSectionScroll(Transform parent, string name, Transform sectionContent, Vector2 position, float height)
        {
            Transform existing = parent.Find(name);
            GameObject scrollObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(ScrollRect));
            if (existing == null) scrollObject.transform.SetParent(parent, false);
            RectTransform scrollRect = scrollObject.transform as RectTransform;
            scrollRect.anchorMin = new Vector2(0.5f, 1f);
            scrollRect.anchorMax = new Vector2(0.5f, 1f);
            scrollRect.pivot = new Vector2(0.5f, 1f);
            scrollRect.anchoredPosition = position;
            scrollRect.sizeDelta = new Vector2(720f, height);

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>() ?? scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic; scroll.scrollSensitivity = 10f;
            scroll.inertia = true;

            Transform viewportTransform = scrollObject.transform.Find("Viewport");
            GameObject viewportObject = viewportTransform != null ? viewportTransform.gameObject : new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            if (viewportTransform == null) viewportObject.transform.SetParent(scrollObject.transform, false);
            RectTransform viewport = viewportObject.transform as RectTransform;
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            RectMask2D mask = viewportObject.GetComponent<RectMask2D>() ?? viewportObject.AddComponent<RectMask2D>();
            mask.enabled = true;

            sectionContent.SetParent(viewportObject.transform, false);
            RectTransform contentRect = sectionContent as RectTransform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);
            ConfigureSectionContent(sectionContent);
            scroll.content = contentRect;
            scroll.viewport = viewport;
            return scrollRect;
        }

        private static void ConfigureDelegationContent(Transform delegation)
        {
            RectTransform rect = delegation as RectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(720f, 200f);
            VerticalLayoutGroup layout = delegation.GetComponent<VerticalLayoutGroup>() ?? delegation.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

    private static void ConfigureGuestContent(Transform content)
    {
            if (content != null && content.Find("Section_0") != null) return;
            // 来客页按顶部向下排列，避免原先大量正向 top-anchor 坐标把内容推到页面外。
            SetGuestTopRect(FindDescendant(content, "Txt_Title"), new Vector2(720f, 40f), new Vector2(0f, -28f));
            RectTransform scroll = FindDescendant(content, "GuestScrollRect") as RectTransform;
            if (scroll != null)
            {
                scroll.anchorMin = Vector2.zero;
                scroll.anchorMax = Vector2.one;
                scroll.offsetMin = new Vector2(24f, 24f);
                scroll.offsetMax = new Vector2(-24f, -72f);
            }
        }

        private static void EnsureGuestScrollLayout(Transform content)
        {
            if (content.Find("Section_0") != null) return;
            Transform scrollTransform = content.Find("GuestScrollRect");
            GameObject scrollObject = scrollTransform != null ? scrollTransform.gameObject : new GameObject("GuestScrollRect", typeof(RectTransform), typeof(ScrollRect));
            if (scrollTransform == null) scrollObject.transform.SetParent(content, false);
            RectTransform scrollRect = scrollObject.transform as RectTransform;
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(24f, 24f);
            scrollRect.offsetMax = new Vector2(-24f, -72f);

            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>() ?? scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic; scroll.scrollSensitivity = 10f;
            scroll.inertia = true;

            Transform viewportTransform = scrollObject.transform.Find("Viewport");
            GameObject viewportObject = viewportTransform != null ? viewportTransform.gameObject : new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            if (viewportTransform == null) viewportObject.transform.SetParent(scrollObject.transform, false);
            RectTransform viewport = viewportObject.transform as RectTransform;
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewportObject.GetComponent<RectMask2D>().enabled = true;

            Transform contentTransform = viewportObject.transform.Find("Content");
            GameObject contentObject = contentTransform != null ? contentTransform.gameObject : new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            if (contentTransform == null) contentObject.transform.SetParent(viewportObject.transform, false);
            RectTransform scrollContent = contentObject.transform as RectTransform;
            scrollContent.anchorMin = new Vector2(0f, 1f);
            scrollContent.anchorMax = new Vector2(1f, 1f);
            scrollContent.pivot = new Vector2(0.5f, 1f);
            scrollContent.anchoredPosition = Vector2.zero;
            scrollContent.sizeDelta = new Vector2(0f, 0f);

            VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>() ?? contentObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.padding = new RectOffset(12, 12, 12, 24);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>() ?? contentObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Transform title = FindDescendant(content, "Txt_Title");
            Transform thanksHeader = FindDescendant(content, "Txt_Thanks");
            Transform delegationHeader = FindDescendant(content, "Txt_DelegationHeader");
            Transform delegationContent = FindDescendant(content, "DelegationContent");
            Transform delegationEmpty = FindDescendant(content, "Txt_DelegationEmpty");
            Transform modHeader = FindDescendant(content, "Txt_ModHeader");
            Transform modHint = FindDescendant(content, "Txt_ModHint");
            Transform[] children = { thanksHeader, delegationHeader, delegationContent, delegationEmpty, modHeader };
            foreach (Transform child in children)
            {
                if (child == null || child == title || child == scrollObject.transform || child.IsChildOf(contentObject.transform)) continue;
                child.SetParent(contentObject.transform, false);
                LayoutElement element = child.GetComponent<LayoutElement>() ?? child.gameObject.AddComponent<LayoutElement>();
                if (child.name == "Txt_Thanks") element.preferredHeight = 40f;
                else if (child.name == "Btn_Claim") element.preferredHeight = 56f;
                else if (child.name == "Txt_DelegationHeader" || child.name == "Txt_ModHeader") element.preferredHeight = 36f;
                else if (child.name == "Txt_ModHint") element.preferredHeight = 40f;
                else if (child.name == "Btn_Mod") element.preferredHeight = 52f;
                else if (child.name == "Txt_DelegationEmpty") element.preferredHeight = 40f;
                element.flexibleHeight = 0f;
            }

            Transform thanksContent = EnsureSectionContent(contentObject.transform, "ThanksContent", thanksHeader, 1);
            Transform modContent = EnsureSectionContent(contentObject.transform, "ModContent", modHeader, 3);
            ConfigureSectionContent(thanksContent);
            ConfigureSectionContent(modContent);
            ConfigureSectionContent(delegationContent);
            if (delegationContent != null)
            {
                LayoutElement element = delegationContent.GetComponent<LayoutElement>() ?? delegationContent.gameObject.AddComponent<LayoutElement>();
                element.minHeight = 0f;
                element.preferredHeight = -1f;
                ContentSizeFitter delegationFitter = delegationContent.GetComponent<ContentSizeFitter>() ?? delegationContent.gameObject.AddComponent<ContentSizeFitter>();
                delegationFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            if (modHint != null) modHint.gameObject.SetActive(false);
            Transform claimButton = FindDescendant(content, "Btn_Claim");
            if (claimButton != null) claimButton.gameObject.SetActive(false);
            Transform modButton = FindDescendant(content, "Btn_Mod");
            if (modButton != null) modButton.gameObject.SetActive(false);
            scroll.content = scrollContent;
            scroll.viewport = viewport;
        }

        private static Transform EnsureSectionContent(Transform parent, string name, Transform anchor, int fallbackIndex)
        {
            Transform existing = FindDescendant(parent, name);
            GameObject sectionObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            if (existing == null) sectionObject.transform.SetParent(parent, false);
            Transform section = sectionObject.transform;
            if (anchor != null)
            {
                int index = anchor.GetSiblingIndex() + 1;
                section.SetSiblingIndex(Mathf.Clamp(index, 0, parent.childCount - 1));
            }
            else section.SetSiblingIndex(Mathf.Clamp(fallbackIndex, 0, parent.childCount - 1));
            RectTransform rect = section as RectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 0f);
            return section;
        }

        private static void ConfigureSectionContent(Transform section)
        {
            if (section == null) return;
            VerticalLayoutGroup layout = section.GetComponent<VerticalLayoutGroup>() ?? section.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = section.GetComponent<ContentSizeFitter>() ?? section.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static void SetGuestTopRect(Transform target, Vector2 size, Vector2 position)
        {
            if (target == null) return;
            RectTransform rect = target as RectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void BindPrefab(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (prefabPath.EndsWith("MainPageShell.prefab", StringComparison.Ordinal)
                    || prefabPath.EndsWith("JournalPage.prefab", StringComparison.Ordinal))
                {
                    if (prefabPath.EndsWith("MainPageShell.prefab", StringComparison.Ordinal))
                    {
                        EnsureMapLayout(root);
                        EnsureHomeLayout(root);
                        EnsureOpeningLayout(root);
                        EnsureTabIcons(root);
                    }
                    EnsureJournalLayout(root);
                }

                int count = 0;
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    Image image = child.GetComponent<Image>();
                    if (image == null)
                    {
                        continue;
                    }

                    string asset = ResolveAsset(child.name, prefabPath);
                    if (string.IsNullOrEmpty(asset))
                    {
                        continue;
                    }

                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + asset);
                    if (sprite == null)
                    {
                        Debug.LogError($"[EverlightFormalUiResourceBinder] 找不到 Sprite：{UiRoot + asset}（{prefabPath}/{child.name}）");
                        continue;
                    }

                    image.sprite = sprite;
                    image.type = Image.Type.Sliced;
                    image.color = Color.white;
                    count++;
                }

                Everlight.Tales.UI.Editor.FormalResourceMigration.BuildCatalog();
                Everlight.Tales.UI.Editor.FormalResourceMigration.Upgrade(root,prefabPath);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"[EverlightFormalUiResourceBinder] {prefabPath} 绑定 {count} 个 Sprite。");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void EnsureMapLayout(GameObject root)
        {
            Transform content = FindDescendant(root.transform, "Content");
            if (content == null) return;
            Transform existingPanel = FindDescendant(content, "Panel_Map");
            if (existingPanel != null)
            {
                EnsureMapTimeBar(existingPanel);
                EnsureMapNodeTemplates(existingPanel.Find("Panel_MapView"));
                Transform existingList = existingPanel.Find("Panel_Place/List_Event");
                if (existingList != null) EnsureEventCardTemplate(existingList);
                return;
            }

            GameObject panel = CreateRect("Panel_Map", content);
            Stretch(panel.GetComponent<RectTransform>());
            panel.AddComponent<CanvasGroup>().blocksRaycasts = true;

            GameObject timeBar = CreateRect("Panel_TimeBar", panel.transform);
            SetRect(timeBar.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 120f), Vector2.zero);
            var timeImage = timeBar.AddComponent<Image>();
            timeImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png");
            timeImage.type = Image.Type.Sliced;
            timeImage.color = new Color(0.08f, 0.10f, 0.14f, 0.90f);
            timeImage.raycastTarget = false;
            timeBar.AddComponent<Everlight.Tales.UI.TimePeriodBar>();
            AddText(timeBar.transform, "Txt_DayPeriod", "第1天 · 清晨", 26f, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-20f, 40f), new Vector2(20f, 10f), new Color(0.94f, 0.95f, 0.96f, 1f));
            Transform dayText = timeBar.transform.Find("Txt_DayPeriod");
            dayText.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Left;
            AddText(timeBar.transform, "Txt_Remaining", "剩4/4", 26f, new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 40f), new Vector2(-20f, 10f), new Color(0.94f, 0.95f, 0.96f, 1f));
            Transform remainingText = timeBar.transform.Find("Txt_Remaining");
            remainingText.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Right;
            for (int i = 0; i < 4; i++)
            {
                GameObject cell = CreateRect("Cell_" + i, timeBar.transform);
                SetRect(cell.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(56f, 14f), new Vector2(80f + i * 70f, 6f));
                var cellImage = cell.AddComponent<Image>();
                cellImage.color = TimePeriodBar.DayIdle;
                cellImage.raycastTarget = false;
            }

            GameObject track = CreateRect("Txt_Track", panel.transform);
            SetRect(track.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(920f, 40f), new Vector2(0f, -150f));
            var trackText = track.AddComponent<TMPro.TextMeshProUGUI>();
            trackText.font = TMPro.TMP_Settings.defaultFontAsset;
            trackText.fontSize = 20f;
            trackText.alignment = TMPro.TextAlignmentOptions.Center;
            trackText.raycastTarget = false;
            trackText.color = new Color(0.88f, 0.66f, 0.35f, 1f);

            GameObject map = CreateRect("Panel_MapView", panel.transform);
            SetRect(map.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-40f, -360f), Vector2.zero);
            var mapImage = map.AddComponent<Image>();
            mapImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png");
            mapImage.type = Image.Type.Sliced;
            mapImage.color = new Color(0.10f, 0.12f, 0.16f, 0.35f);
            mapImage.raycastTarget = false;
            EnsureMapNodeTemplates(map.transform);

            GameObject place = CreateRect("Panel_Place", panel.transform);
            SetRect(place.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(-40f, 360f), new Vector2(0f, 0f));
            var placeImage = place.AddComponent<Image>();
            placeImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png");
            placeImage.type = Image.Type.Sliced;
            placeImage.color = Color.white;
            placeImage.raycastTarget = false;

            AddText(place.transform, "Txt_PlaceLabel", "点击地图节点查看地点", 28f, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(920f, 40f), new Vector2(0f, -28f));
            AddText(place.transform, "Txt_PlaceDesc", "", 20f, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(920f, 56f), new Vector2(0f, -72f));

            GameObject list = CreateRect("List_Event", place.transform);
            SetRect(list.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-40f, 30f), new Vector2(0f, 120f));
            EnsureEventCardTemplate(list.transform);

            GameObject wait = CreateRect("Btn_Wait", place.transform);
            SetRect(wait.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(360f, 52f), new Vector2(0f, 24f));
            var waitImage = wait.AddComponent<Image>();
            waitImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-021-normal主按钮.png");
            waitImage.type = Image.Type.Sliced;
            waitImage.color = Color.white;
            var waitButton = wait.AddComponent<Button>();
            waitButton.targetGraphic = waitImage;
            AddText(wait.transform, "Txt_WaitLabel", "等待到下一时段", 24f, Vector2.zero, Vector2.one, new Vector2(-16f, -8f), Vector2.zero, Color.white);
        }

        private static void EnsureMapTimeBar(Transform panel)
        {
            if (panel.Find("Panel_TimeBar") != null) return;
            GameObject timeBar = CreateRect("Panel_TimeBar", panel);
            SetRect(timeBar.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 120f), Vector2.zero);
            var image = timeBar.AddComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png");
            image.type = Image.Type.Sliced; image.color = new Color(0.08f, 0.10f, 0.14f, 0.90f); image.raycastTarget = false;
            timeBar.AddComponent<Everlight.Tales.UI.TimePeriodBar>();
            AddText(timeBar.transform, "Txt_DayPeriod", "第1天 · 清晨", 26f, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-20f, 40f), new Vector2(20f, 10f));
            timeBar.transform.Find("Txt_DayPeriod").GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Left;
            AddText(timeBar.transform, "Txt_Remaining", "剩4/4", 26f, new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 40f), new Vector2(-20f, 10f));
            timeBar.transform.Find("Txt_Remaining").GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Right;
            for (int i = 0; i < 4; i++)
            {
                GameObject cell = CreateRect("Cell_" + i, timeBar.transform);
                SetRect(cell.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0f), new Vector2(56f, 14f), new Vector2(80f + i * 70f, 6f));
                var cellImage = cell.AddComponent<Image>(); cellImage.color = TimePeriodBar.DayIdle; cellImage.raycastTarget = false;
            }
        }

        private static void EnsureMapNodeTemplates(Transform mapRoot)
        {
            if (mapRoot == null) return;
            string[] ids = { "home", "street", "dance", "tailor", "community" };
            string[] sprites =
            {
                "图标/地点/ICO-071-unlocked长明修理铺.png",
                "图标/地点/ICO-072-unlocked街区小店.png",
                "图标/地点/ICO-073-unlocked舞蹈教室.png",
                "图标/地点/ICO-074-unlocked裁缝铺.png",
                "图标/地点/ICO-075-unlocked社区活动中心.png"
            };
            for (int i = 0; i < ids.Length; i++)
            {
                if (mapRoot.Find("NodeTemplate_" + ids[i]) != null) continue;
                GameObject node = CreateRect("NodeTemplate_" + ids[i], mapRoot);
                node.SetActive(false);
                SetRect(node.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(44f, 44f), Vector2.zero);
                Image image = node.AddComponent<Image>();
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + sprites[i]);
                image.color = Color.white;
                image.raycastTarget = true;
                Button button = node.AddComponent<Button>();
                button.targetGraphic = image;
            }
        }

        private static void EnsureEventCardTemplate(Transform list)
        {
            if (list.Find("EventCardTemplate") != null) return;
            GameObject card = CreateRect("EventCardTemplate", list);
            card.SetActive(false);
            SetRect(card.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 56f), Vector2.zero);
            var image = card.AddComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-003-normal卡片.png");
            image.type = Image.Type.Sliced; image.color = Color.white;
            var button = card.AddComponent<Button>(); button.targetGraphic = image;
            AddText(card.transform, "Txt_EventName", "事件名称", 22f, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 30f), new Vector2(0f, 10f));
            Transform name = card.transform.Find("Txt_EventName"); name.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Left;
            AddText(card.transform, "Txt_EventMeta", "类型 · 耗时", 16f, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 24f), new Vector2(0f, -16f), new Color(0.68f, 0.71f, 0.75f, 1f));
            Transform meta = card.transform.Find("Txt_EventMeta"); meta.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Left;
        }

        private static void EnsureHomeLayout(GameObject root)
        {
            Transform content = FindDescendant(root.transform, "Content");
            if (content == null) return;
            Transform home = FindDescendant(content, "Panel_Home");
            if (home == null)
            {
                GameObject panel = CreateRect("Panel_Home", content);
                Stretch(panel.GetComponent<RectTransform>());
                panel.AddComponent<CanvasGroup>().blocksRaycasts = true;
                home = panel.transform;
            }
            if (home.Find("Panel_HomeTop") == null)
            {
                GameObject top = CreateRect("Panel_HomeTop", home);
                SetRect(top.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 96f), Vector2.zero);
                var topImage = top.AddComponent<Image>(); topImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png"); topImage.type = Image.Type.Sliced; topImage.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
                string[] labels = { "来客", "收藏", "保管", "服务" };
                int[] buttonIds = { 0, 2, 3, 4 };
                for (int i = 0; i < labels.Length; i++)
                {
                    GameObject button = CreateRect("Btn_Zone_" + buttonIds[i], top.transform);
                    SetRect(button.GetComponent<RectTransform>(), new Vector2(i * 0.25f, 0f), new Vector2((i + 1) * 0.25f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-16f, -32f), Vector2.zero);
                    var image = button.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-027-normal页签4.png"); image.type = Image.Type.Sliced; image.color = Color.white;
                    var buttonComp = button.AddComponent<Button>(); buttonComp.targetGraphic = image;
                    AddText(button.transform, "Txt_Label", labels[i], 24f, Vector2.zero, Vector2.one, new Vector2(-12f, -8f), Vector2.zero, Color.white);
                }
            }
            if (home.Find("Panel_HomeArea") == null)
            {
                GameObject area = CreateRect("Panel_HomeArea", home);
                SetRect(area.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, -96f), new Vector2(0f, -48f));
            }
            Transform homeArea = home.Find("Panel_HomeArea");
            // HomePage 的四个内容区由 HomePanel 从独立页面预制体按需实例化。
        }

        private static void EnsureContentRoot(Transform parent, string name)
        {
            GameObject content = CreateRect(name, parent);
            Stretch(content.GetComponent<RectTransform>());
            var image = content.AddComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png");
            image.type = Image.Type.Sliced;
            image.color = new Color(0.08f, 0.10f, 0.14f, 0.40f);
            image.raycastTarget = false;
        }

        private static void EnsureOpeningLayout(GameObject root)
        {
            if (FindDescendant(root.transform, "Panel_OpeningOverlay") != null) return;
            GameObject panel = CreateRect("Panel_OpeningOverlay", root.transform);
            Stretch(panel.GetComponent<RectTransform>());
            panel.SetActive(false);
            var image = panel.AddComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-002对话框.png");
            image.type = Image.Type.Sliced; image.color = new Color(0.02f, 0.03f, 0.05f, 0.97f);
            GameObject subtitle = CreateRect("Txt_OpeningSubtitle", panel.transform);
            SetRect(subtitle.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(940f, 140f), new Vector2(0f, -160f));
            var subtitleText = subtitle.AddComponent<TMPro.TextMeshProUGUI>(); subtitleText.font = TMPro.TMP_Settings.defaultFontAsset; subtitleText.fontSize = 30f; subtitleText.alignment = TMPro.TextAlignmentOptions.Center; subtitleText.color = Color.white; subtitleText.raycastTarget = false;
            GameObject button = CreateRect("Btn_OpeningAction", panel.transform);
            SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(260f, 56f), new Vector2(0f, -360f));
            var buttonImage = button.AddComponent<Image>(); buttonImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-021-normal主按钮.png"); buttonImage.type = Image.Type.Sliced;
            var buttonComponent = button.AddComponent<Button>(); buttonComponent.targetGraphic = buttonImage;
            AddText(button.transform, "Txt_ActionLabel", "跳过", 24f, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        }

        private static void EnsureTabIcons(GameObject root)
        {
            string[] assets = { "图标/ICO-001地图.png", "图标/ICO-002任务.png", "图标/ICO-003家园.png", "图标/ICO-004工作台.png", "图标/ICO-012设置.png" };
            for (int i = 0; i < assets.Length; i++)
            {
                Transform tab = FindDescendant(root.transform, "Tab_" + i);
                if (tab == null || tab.Find("Img_Icon") != null) continue;
                GameObject icon = CreateRect("Img_Icon", tab);
                SetRect(icon.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(38f, 38f), new Vector2(0f, -10f));
                var image = icon.AddComponent<Image>();
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + assets[i]);
                image.preserveAspect = true; image.raycastTarget = false;
            }
        }

        private static void EnsureCodexLayout(Transform root)
        {
            GameObject top = CreateRect("Panel_CodexTop", root);
            SetRect(top.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 96f), Vector2.zero);
            var image = top.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png"); image.type = Image.Type.Sliced; image.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
            AddText(top.transform, "Txt_Progress", "已拥有 0/0", 26f, new Vector2(0.5f, 0.5f), Vector2.one, new Vector2(-40f, 36f), new Vector2(-24f, -24f));
            top.transform.Find("Txt_Progress").GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Right;
            string[] labels = { "零件 P", "形态 M", "怪谈" };
            for (int i = 0; i < labels.Length; i++)
            {
                GameObject button = CreateRect("Btn_Sub_" + i, top.transform);
                SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(140f, 56f), new Vector2(-430f + i * 150f, -24f));
                var btnImage = button.AddComponent<Image>(); btnImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-026-normal页签3.png"); btnImage.type = Image.Type.Sliced; btnImage.color = Color.white;
                var btn = button.AddComponent<Button>(); btn.targetGraphic = btnImage;
                AddText(button.transform, "Txt_Label", labels[i], 24f, Vector2.zero, Vector2.one, new Vector2(-12f, -8f), Vector2.zero, Color.white);
            }
            GameObject list = CreateRect("Panel_CodexList", root);
            SetRect(list.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, -96f), new Vector2(0f, -48f));
        }

        private static void EnsureArchiveLayout(Transform root)
        {
            GameObject top = CreateRect("Panel_ArchiveTop", root);
            SetRect(top.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 96f), Vector2.zero);
            var image = top.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png"); image.type = Image.Type.Sliced; image.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
            AddText(top.transform, "Txt_Title", "陈列 / 档案", 28f, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 36f), new Vector2(24f, -24f), new Color(1f, 0.85f, 0.35f, 1f));
            GameObject list = CreateRect("Panel_ArchiveList", root);
            SetRect(list.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, -96f), new Vector2(0f, -48f));
        }

        private static void EnsureWorkbenchLayout(Transform root)
        {
            GameObject top = CreateRect("Panel_WorkbenchTop", root);
            SetRect(top.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 96f), Vector2.zero);
            var topImage = top.AddComponent<Image>(); topImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png"); topImage.type = Image.Type.Sliced; topImage.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
            AddText(top.transform, "Txt_Fee", "维修费 0", 26f, new Vector2(0.5f, 0.5f), Vector2.one, new Vector2(-40f, 36f), new Vector2(-24f, -24f));
            top.transform.Find("Txt_Fee").GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Right;
            string[] labels = { "加工", "材料", "账目" };
            for (int i = 0; i < labels.Length; i++)
            {
                GameObject button = CreateRect("Btn_Sub_" + i, top.transform);
                SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(140f, 56f), new Vector2(-430f + i * 150f, -24f));
                var image = button.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-026-normal页签3.png"); image.type = Image.Type.Sliced; image.color = Color.white;
                var buttonComp = button.AddComponent<Button>(); buttonComp.targetGraphic = image;
                AddText(button.transform, "Txt_Label", labels[i], 24f, Vector2.zero, Vector2.one, new Vector2(-12f, -8f), Vector2.zero, Color.white);
            }
            GameObject body = CreateRect("Panel_WorkbenchBody", root);
            SetRect(body.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, -96f), new Vector2(0f, -48f));
            string[] bodies = { "Panel_Workbench", "Panel_Materials", "Panel_Ledger" };
            foreach (string bodyName in bodies)
            {
                GameObject child = CreateRect(bodyName, body.transform);
                Stretch(child.GetComponent<RectTransform>());
                child.AddComponent<CanvasGroup>().blocksRaycasts = true;
                child.SetActive(false);
            }
        }

        private static void EnsureJournalLayout(GameObject root)
        {
            Transform content = FindDescendant(root.transform, "Content");
            Transform journal = content != null
                ? FindDescendant(content, "Panel_Journal")
                : FindDescendant(root.transform, "Panel_Journal");
            if (journal == null)
            {
                if (content == null) return;
                GameObject panel = CreateRect("Panel_Journal", content);
                Stretch(panel.GetComponent<RectTransform>());
                panel.AddComponent<CanvasGroup>().blocksRaycasts = true;
                journal = panel.transform;
            }
            if (journal.Find("Panel_JournalTop") == null)
            {
                GameObject top = CreateRect("Panel_JournalTop", journal);
                SetRect(top.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 96f), Vector2.zero);
                var topImage = top.AddComponent<Image>(); topImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png"); topImage.type = Image.Type.Sliced; topImage.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
                AddText(top.transform, "Txt_Balance", "维修费 0", 26f, new Vector2(0.5f, 0.5f), Vector2.one, new Vector2(-40f, 36f), new Vector2(-24f, -24f));
                top.transform.Find("Txt_Balance").GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Right;
                string[] labels = { "事件", "任务", "怪谈" };
                for (int i = 0; i < labels.Length; i++)
                {
                    GameObject button = CreateRect("Btn_Sub_" + i, top.transform);
                    SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(140f, 56f), new Vector2(-430f + i * 150f, -24f));
                    var image = button.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-026-normal页签3.png"); image.type = Image.Type.Sliced; image.color = Color.white;
                    var buttonComp = button.AddComponent<Button>(); buttonComp.targetGraphic = image;
                    AddText(button.transform, "Txt_Label", labels[i], 24f, Vector2.zero, Vector2.one, new Vector2(-12f, -8f), Vector2.zero, Color.white);
                    AddText(top.transform, "Txt_Badge_" + i, "", 20f, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(36f, 26f), new Vector2(-376f + i * 150f, -6f), new Color(0.88f, 0.66f, 0.35f, 1f));
                }
            }
            Transform list = journal.Find("Panel_JournalList");
            if (list == null)
            {
                GameObject listObject = CreateRect("Panel_JournalList", journal);
                SetRect(listObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, -96f), new Vector2(0f, -48f));
                list = listObject.transform;
            }
            EnsureJournalListContainers(list);
        }

        private static void EnsureJournalListContainers(Transform list)
        {
            if (list == null) return;
            Transform filterRoot = list.Find("JournalFilterRoot");
            if (filterRoot == null)
            {
                GameObject filter = CreateRect("JournalFilterRoot", list);
                SetRect(filter.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 88f), Vector2.zero);
                filterRoot = filter.transform;
            }

            string[] names = { "全部", "维修", "处置", "生活", "调查", "怪谈" };
            for (int i = 0; i < names.Length; i++)
            {
                string buttonName = "Btn_Filter_" + i;
                Transform existingButton = filterRoot.Find(buttonName);
                if (existingButton != null)
                {
                    EnsureJournalFilterNumber(existingButton);
                    continue;
                }
                GameObject button = CreateRect(buttonName, filterRoot);
                SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(84f, 44f), new Vector2(-430f + i * 90f, -12f));
                Image image = button.AddComponent<Image>();
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-029-normal筛选片.png");
                image.type = Image.Type.Sliced;
                image.color = Color.white;
                Button buttonComponent = button.AddComponent<Button>();
                buttonComponent.targetGraphic = image;
                AddText(button.transform, "Txt_Label", names[i], 20f, Vector2.zero, Vector2.one, new Vector2(-8f, -8f), Vector2.zero, Color.white);
                EnsureJournalFilterNumber(button.transform);
            }

            Transform entriesRoot = list.Find("JournalEntriesRoot");
            if (entriesRoot == null)
            {
                GameObject entries = CreateRect("JournalEntriesRoot", list);
                SetRect(entries.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(0f, -104f));
                entriesRoot = entries.transform;
            }
            EnsureJournalSections(entriesRoot);
        }

        private static void EnsureJournalSections(Transform root)
        {
            if (root == null) return;
            // 固定三分类布局，避免资源重新接入时生成旧的两段事件专用结构。
            Everlight.Tales.UI.Editor.JournalSectionLayoutMigration.EnsureSections(root);
        }

        private static void EnsureJournalFilterNumber(Transform button)
        {
            if (button == null || button.Find("Txt_Num") != null) return;
            AddText(button, "Txt_Num", "", 14f, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(24f, 22f), new Vector2(-5f, -3f), new Color(1f, 0.84f, 0.38f, 1f));
        }

        private static void EnsureJournalHeader(Transform root, string name, string value, float y)
        {
            Transform existing = root.Find(name);
            GameObject header = existing != null ? existing.gameObject : CreateRect(name, root);
            SetRect(header.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 44f), new Vector2(0f, y));
            LayoutElement layout = header.GetComponent<LayoutElement>() ?? header.AddComponent<LayoutElement>();
            layout.preferredHeight = 44f;
            if (header.transform.Find("Txt_Label") == null) AddText(header.transform, "Txt_Label", value, 26f, Vector2.zero, Vector2.one, new Vector2(-24f, 0f), Vector2.zero, new Color(1f, 0.85f, 0.35f, 1f));
            TMPro.TextMeshProUGUI label = header.transform.Find("Txt_Label")?.GetComponent<TMPro.TextMeshProUGUI>();
            if (label != null) { label.enableWordWrapping = false; label.overflowMode = TMPro.TextOverflowModes.Ellipsis; }
        }

        private static void EnsureJournalScroll(Transform root, string scrollName, string contentName, float y)
        {
            Transform existing = root.Find(scrollName);
            GameObject scrollObject = existing != null ? existing.gameObject : CreateRect(scrollName, root);
            SetRect(scrollObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 270f), new Vector2(0f, y));
            LayoutElement scrollLayout = scrollObject.GetComponent<LayoutElement>() ?? scrollObject.AddComponent<LayoutElement>();
            scrollLayout.preferredHeight = 270f;
            scrollLayout.flexibleWidth = 1f;
            ScrollRect scroll = scrollObject.GetComponent<ScrollRect>() ?? scrollObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic; scroll.scrollSensitivity = 10f;

            Transform existingViewport = scrollObject.transform.Find("Viewport");
            GameObject viewport = existingViewport != null ? existingViewport.gameObject : CreateRect("Viewport", scrollObject.transform);
            SetRect(viewport.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            if (viewport.GetComponent<RectMask2D>() == null) viewport.AddComponent<RectMask2D>();
            Transform existingContent = viewport.transform.Find(contentName);
            GameObject content = existingContent != null ? existingContent.gameObject : CreateRect(contentName, viewport.transform);
            SetRect(content.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 0f), Vector2.zero);
            VerticalLayoutGroup contentLayout = content.GetComponent<VerticalLayoutGroup>() ?? content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 6f;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>() ?? content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = content.GetComponent<RectTransform>();
        }

        private static GameObject CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            SetRect(rect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot, Vector2 size, Vector2 position)
        {
            rect.anchorMin = min; rect.anchorMax = max; rect.pivot = pivot; rect.sizeDelta = size; rect.anchoredPosition = position;
        }

        private static void AddText(Transform parent, string name, string value, float size, Vector2 min, Vector2 max, Vector2 dimensions, Vector2 position, Color? color = null)
        {
            GameObject go = CreateRect(name, parent);
            RectTransform rect = go.GetComponent<RectTransform>();
            SetRect(rect, min, max, new Vector2(0.5f, 0.5f), dimensions, position);
            var text = go.AddComponent<TMPro.TextMeshProUGUI>();
            text.font = TMPro.TMP_Settings.defaultFontAsset; text.fontSize = size; text.text = value;
            text.alignment = TMPro.TextAlignmentOptions.Center; text.raycastTarget = false;
            text.color = color ?? new Color(0.94f, 0.95f, 0.96f, 1f);
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform hit = FindDescendant(child, name);
                if (hit != null) return hit;
            }
            return null;
        }

        private static List<Transform> FindAllDescendants(Transform root, string name)
        {
            var result = new List<Transform>();
            if (root == null) return result;
            if (root.name == name) result.Add(root);
            for (int i = 0; i < root.childCount; i++) result.AddRange(FindAllDescendants(root.GetChild(i), name));
            return result;
        }

        private static string ResolveAsset(string nodeName, string prefabPath)
        {
            if (prefabPath.EndsWith("MainPageShell.prefab", StringComparison.Ordinal))
            {
                if (nodeName.StartsWith("Tab_", StringComparison.Ordinal))
                {
                    return "控件/SHR-027-normal页签4.png";
                }

                if (nodeName == "TopBar")
                {
                    return "九宫格/SHR-006-night顶部条.png";
                }
            }

            if (prefabPath.EndsWith("DialogView.prefab", StringComparison.Ordinal))
            {
                switch (nodeName)
                {
                    case "Panel":
                        return "九宫格/SHR-002对话框.png";
                    case "Button_0":
                        return "控件/SHR-021-normal主按钮.png";
                    case "Button_1":
                        return "控件/SHR-022-normal次按钮.png";
                }
            }

            return null;
        }
    }
}
