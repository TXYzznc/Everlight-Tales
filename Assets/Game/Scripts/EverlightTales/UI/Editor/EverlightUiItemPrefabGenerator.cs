#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Everlight.Tales.UI;

namespace Everlight.Tales.Editor
{
    /// <summary>将页面内重复对象抽成 GF UIItem Prefab，并替换页面中的静态重复实例。</summary>
    public static class EverlightUiItemPrefabGenerator
    {
        private const string ItemDir = "Assets/Game/Prefabs/UI/Item";
        private const string SavePage = "Assets/Game/Prefabs/UI/SaveSlotPage.prefab";
        private const string SettlementPage = "Assets/Game/Prefabs/UI/SettlementPage.prefab";
        private const string PreparationPage = "Assets/Game/Prefabs/UI/PreparationPage.prefab";
        private const string ArchivePage = "Assets/Game/Prefabs/UI/ArchivePage.prefab";
        private const string CodexPage = "Assets/Game/Prefabs/UI/CodexPage.prefab";
        private const string WorkbenchPage = "Assets/Game/Prefabs/UI/WorkbenchPage.prefab";
        private const string MapPage = "Assets/Game/Prefabs/UI/MapPage.prefab";
        private const string JournalPage = "Assets/Game/Prefabs/UI/JournalPage.prefab";
        private const string GuestPage = "Assets/Game/Prefabs/UI/GuestPage.prefab";
        private const string CatalogPath = "Assets/Game/Data/UI/UIFormalSpriteCatalog.asset";

        [MenuItem("Game Framework/EverlightTales/UI/生成并接入 UIItem 预制体", priority = 2020)]
        public static void GenerateAndBind()
        {
            EnsureFolder(ItemDir);
            string saveItem = CreateSaveSlotItem();
            string rewardItem = CreateRewardChoiceItem();
            string carryItem = CreateCarryAvailableItem();
            BindSaveSlotPage(saveItem);
            BindSettlementPage(rewardItem);
            BindPreparationPage(carryItem);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightUiItemPrefabGenerator] 已生成并接入 SaveSlotItem、RewardChoiceItem、CarryAvailableItem。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/生成并接入 Archive-Codex-Workbench UIItem", priority = 2022)]
        public static void GenerateAndBindCollectionPages()
        {
            EnsureFolder("Assets/Game/Data");
            EnsureFolder("Assets/Game/Data/UI");
            EnsureFolder(ItemDir);
            UIFormalSpriteCatalog catalog = CreateFormalSpriteCatalog();
            string archiveDisplay = CreateArchiveItem("ArchiveDisplayItem", "SHR-003-normal卡片");
            string archiveOwned = CreateArchiveItem("ArchiveOwnedItem", "SHR-005-normal列表行");
            string archiveMaterial = CreateArchiveItem("ArchiveMaterialItem", "SHR-005-normal列表行");
            string archiveBlueprint = CreateArchiveItem("ArchiveBlueprintItem", "SHR-005-normal列表行");
            string archiveSummary = CreateArchiveItem("ArchiveSummaryItem", "SHR-003-normal卡片");
            string codexPart = CreateCodexItem("CodexPartItem", "SHR-005-normal列表行");
            string codexForm = CreateCodexItem("CodexFormItem", "SHR-005-normal列表行");
            string codexCase = CreateCodexItem("CodexCaseItem", "SHR-003-normal卡片");
            string workbenchHost = CreateWorkbenchItem("WorkbenchHostItem", "SHR-046-normal六边形框");
            string workbenchForm = CreateWorkbenchItem("WorkbenchFormItem", "SHR-005-normal列表行");
            string workbenchMaterial = CreateWorkbenchItem("WorkbenchMaterialItem", "SHR-005-normal列表行");
            string workbenchLedger = CreateWorkbenchItem("WorkbenchLedgerItem", "SHR-005-normal列表行");
            BindArchivePage(archiveDisplay, archiveOwned, archiveMaterial, archiveBlueprint, archiveSummary, catalog);
            BindCodexPage(codexPart, codexForm, codexCase, catalog);
            BindWorkbenchPage(workbenchHost, workbenchForm, workbenchMaterial, workbenchLedger, catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightUiItemPrefabGenerator] 已生成并接入 Archive/Codex/Workbench UIItem。 ");
        }

        [MenuItem("Game Framework/EverlightTales/UI/修复 Workbench 布局与 Item", priority = 2023)]
        public static void RebuildWorkbenchOnly()
        {
            EnsureFolder(ItemDir);
            UIFormalSpriteCatalog catalog = CreateFormalSpriteCatalog();
            string host = CreateWorkbenchItem("WorkbenchHostItem", "SHR-046-normal六边形框");
            string form = CreateWorkbenchItem("WorkbenchFormItem", "SHR-005-normal列表行");
            string material = CreateWorkbenchItem("WorkbenchMaterialItem", "SHR-005-normal列表行");
            string ledger = CreateWorkbenchItem("WorkbenchLedgerItem", "SHR-005-normal列表行");
            BindWorkbenchPage(host, form, material, ledger, catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightUiItemPrefabGenerator] Workbench 布局与 Item 已重建。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/第二批动态 Item/生成并接入地图事件 Item", priority = 2026)]
        public static void GenerateAndBindMapEventItem()
        {
            EnsureFolder(ItemDir);
            string item = CreateMapEventItem();
            BindMapPage(item);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightUiItemPrefabGenerator] 已生成并接入 MapEventItem。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/第二批动态 Item/生成并接入日志条目 Item", priority = 2027)]
        public static void GenerateAndBindJournalItem()
        {
            EnsureFolder(ItemDir);
            string item = CreateJournalItem();
            BindJournalPage(item);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightUiItemPrefabGenerator] 已生成并接入 JournalItem。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/第二批动态 Item/生成对话选项 Item", priority = 2028)]
        public static void GenerateDialogueChoiceItem()
        {
            EnsureFolder(ItemDir);
            CreateDialogueChoiceItem();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightUiItemPrefabGenerator] 已生成对话选项 Item。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/第二批动态 Item/生成调查热点 Item", priority = 2029)]
        public static void GenerateInvestigationHotspotItem()
        {
            EnsureFolder(ItemDir); CreateInvestigationHotspotItem();
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("[EverlightUiItemPrefabGenerator] 已生成调查热点 Item。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/第二批动态 Item/生成并接入来客委托 Item", priority = 2030)]
        public static void GenerateAndBindGuestDelegationItem()
        {
            EnsureFolder(ItemDir); string item = CreateGuestDelegationItem(); BindGuestPage(item); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("[EverlightUiItemPrefabGenerator] 已生成并接入 GuestDelegationItem。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/全量20界面/规范化节点并生成审计报告", priority = 2024)]
        public static void NormalizeAllUiPrefabs()
        {
            var report = new System.Collections.Generic.List<string> { "resolution=1080x1920", "pages=20" };
            string[] pages = { "MainPageShell", "DialogView", "BoardPage", "PreparationPage", "SettlementPage", "Settings", "SaveSlotPage", "MapPage", "HomePage", "JournalPage", "WorkbenchPage", "CodexPage", "ArchivePage", "GuestPage", "ServicePage", "DialoguePage", "ProloguePage", "InvestigationPage", "RecoveryPage", "FeedbackPage" };
            foreach (string view in pages)
            {
                string path = "Assets/Game/Prefabs/UI/" + view + ".prefab";
                if (!File.Exists(path)) { report.Add(view + ";missingPrefab"); continue; }
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    RectTransform rootRect = root.GetComponent<RectTransform>();
                    int texts = 0, images = 0, layouts = 0, missing = 0;
                    foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.GetComponent<TMP_Text>() is TMP_Text text)
                        {
                            texts++;
                            text.raycastTarget = false;
                            text.overflowMode = TextOverflowModes.Ellipsis;
                            text.enableAutoSizing = false;
                        }
                        if (t.GetComponent<Image>() != null) images++;
                        if (t.GetComponent<LayoutGroup>() != null) layouts++;
                        foreach (Component c in t.GetComponents<Component>()) if (c == null) missing++;
                    }
                    if (rootRect != null) rootRect.localScale = Vector3.one;
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    report.Add(string.Format("{0};texts={1};images={2};layouts={3};missing={4}", view, texts, images, layouts, missing));
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            File.WriteAllLines("Library/AllUiPrefabAudit.txt", report);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightUiItemPrefabGenerator] 已规范化全部 20 个界面并写入 Library/AllUiPrefabAudit.txt。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/ArchivePage", priority = 2030)]
        public static void EnableArchiveValidation() => UIValidationHarness.Configure("ArchivePage");

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/MainPageShell", priority = 2029)]
        public static void EnableMainPageShellValidation() => UIValidationHarness.Configure("MainPageShell");

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/CodexPage", priority = 2031)]
        public static void EnableCodexValidation() => UIValidationHarness.Configure("CodexPage");

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/WorkbenchPage", priority = 2032)]
        public static void EnableWorkbenchValidation() => UIValidationHarness.Configure("WorkbenchPage");

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/JournalPage", priority = 2032)]
        public static void EnableJournalValidation() => UIValidationHarness.Configure("JournalPage");

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/关闭", priority = 2033)]
        public static void DisableValidation() => UIValidationHarness.Disable();

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/PlayMode立即注入", priority = 2034)]
        public static void InjectValidationNow()
        {
            if (Application.isPlaying) UIValidationHarness.Start();
            else Debug.LogWarning("请先进入 PlayMode，再执行此菜单。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/状态转储", priority = 2035)]
        public static void DumpValidationState()
        {
            var session = WorldSession.Current;
            var lines = new System.Collections.Generic.List<string>
            {
                "playing=" + Application.isPlaying,
                "enabled=" + UIValidationHarness.Enabled,
                "session=" + (session != null),
                "fee=" + (session != null ? session.World.RepairFee : -1),
                "parts=" + (session != null ? session.World.OwnedParts.Count : -1),
                "hosts=" + (session != null ? WorkbenchLayout.OwnedHostParts(session.World).Count : -1)
            };
            foreach (WorkbenchPanel panel in Resources.FindObjectsOfTypeAll<WorkbenchPanel>())
            {
                if (!panel.gameObject.scene.IsValid()) continue;
                lines.Add("panel=" + panel.name + " active=" + panel.gameObject.activeInHierarchy + " path=" + panel.transform.parent?.name);
                var panelType = typeof(WorkbenchPanel);
                foreach (string fieldName in new[] { "_hostItemTemplate", "_formItemTemplate", "_materialItemTemplate", "_ledgerItemTemplate", "_spriteCatalog", "_form", "m_HostRow", "m_FormList", "m_WorkbenchRoot" })
                {
                    var field = panelType.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    object value = field == null ? null : field.GetValue(panel);
                    lines.Add(fieldName + "=" + (value == null ? "null" : value.ToString()));
                }
                foreach (Transform child in panel.GetComponentsInChildren<Transform>(true))
                    if (child.parent == panel.transform || child.name == "HostScroll" || child.name == "FormScroll" || child.name == "host_row" || child.name == "form_list") lines.Add(child.name + " children=" + child.childCount);
            }
            File.WriteAllLines("Library/UIValidationState.txt", lines);
        }

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/验证 MainPageShell 路由", priority = 2037)]
        public static void ValidateMainPageShellRouting()
        {
            EverlightUiValidationWindow.RunMainShellRoutingCheck();
        }

        [MenuItem("Game Framework/EverlightTales/UI/全量20界面/PlayMode 烟雾测试", priority = 2025)]
        public static void StartAllPagesSmokeTest()
        {
            UIValidationHarness.StartAllPagesSmokeTest();
        }

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/Workbench全部页签", priority = 2036)]
        public static void ValidateWorkbenchTabs()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("请先进入 Play Mode 并启用 UI 验证 harness。");
                return;
            }

            var lines = new System.Collections.Generic.List<string>
            {
                "playing=" + Application.isPlaying,
                "resolution=1080x1920"
            };
            foreach (WorkbenchPanel panel in Resources.FindObjectsOfTypeAll<WorkbenchPanel>())
            {
                if (panel == null || !panel.gameObject.scene.IsValid()) continue;
                var select = typeof(WorkbenchPanel).GetMethod("SelectSubTab", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (select == null) continue;
                lines.Add("panel=" + panel.name);
                for (int tab = 0; tab < 3; tab++)
                {
                    select.Invoke(panel, new object[] { tab });
                    var type = typeof(WorkbenchPanel);
                    RectTransform host = GetPrivateRect(type, panel, "m_HostRow");
                    RectTransform forms = GetPrivateRect(type, panel, "m_FormList");
                    RectTransform materials = GetPrivateRect(type, panel, "m_MaterialsRoot");
                    RectTransform ledger = GetPrivateRect(type, panel, "m_LedgerRoot");
                    lines.Add(string.Format("tab={0};hostActive={1};hostChildren={2};formActive={3};formChildren={4};materialsActive={5};materialsChildren={6};ledgerActive={7};ledgerChildren={8}",
                        tab,
                        host != null && host.gameObject.activeSelf, host != null ? host.childCount : -1,
                        forms != null && forms.gameObject.activeSelf, forms != null ? forms.childCount : -1,
                        materials != null && materials.gameObject.activeSelf, materials != null ? materials.childCount : -1,
                        ledger != null && ledger.gameObject.activeSelf, ledger != null ? ledger.childCount : -1));
                }
            }
            File.WriteAllLines("Library/UIValidationWorkbenchTabs.txt", lines);
            Debug.Log("Workbench tabs validation written to Library/UIValidationWorkbenchTabs.txt");
        }

        private static RectTransform GetPrivateRect(Type type, object target, string fieldName)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(target) as RectTransform;
        }

        private static void BindArchivePage(string display, string owned, string material, string blueprint, string summary, UIFormalSpriteCatalog catalog)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(ArchivePage);
            try
            {
                Transform list = Find(root.transform, "Panel_ArchiveList");
                if (list == null) throw new InvalidOperationException("找不到 ArchivePage/Panel_ArchiveList");
                ConfigureContainerLayout(list);
                string[] names = { "Content_Display", "Content_Owned", "Content_Material", "Content_Blueprint", "Content_Summary" };
                foreach (Transform child in list.GetComponentsInChildren<Transform>(true))
                    if (child != list && Array.IndexOf(names, child.name) >= 0) UnityEngine.Object.DestroyImmediate(child.gameObject);
                RectTransform[] roots = new RectTransform[names.Length];
                for (int i = 0; i < names.Length; i++)
                {
                    roots[i] = CreateRect(list, names[i]);
                    roots[i].anchorMin = new Vector2(0f, 1f); roots[i].anchorMax = new Vector2(1f, 1f); roots[i].pivot = new Vector2(0.5f, 1f);
                    roots[i].anchoredPosition = new Vector2(0f, -i * 230f); roots[i].sizeDelta = new Vector2(0f, i == 4 ? 180f : 210f);
                    AddVerticalLayout(roots[i], 12f);
                    CreateSectionHeader(roots[i], new[] { "陈列物", "拥有物", "材料", "图样", "汇总" }[i]);
                }
                Component panel = FindComponentByType(root, "ArchivePanel");
                SetRef(panel, "_displayItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(display)); SetRef(panel, "_displayRoot", roots[0]);
                SetRef(panel, "_ownedItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(owned)); SetRef(panel, "_ownedRoot", roots[1]);
                SetRef(panel, "_materialItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(material)); SetRef(panel, "_materialRoot", roots[2]);
                SetRef(panel, "_blueprintItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(blueprint)); SetRef(panel, "_blueprintRoot", roots[3]);
                SetRef(panel, "_summaryItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(summary)); SetRef(panel, "_summaryRoot", roots[4]); SetRef(panel, "_spriteCatalog", catalog);
                PrefabUtility.SaveAsPrefabAsset(root, ArchivePage);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BindCodexPage(string part, string form, string @case, UIFormalSpriteCatalog catalog)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(CodexPage);
            try
            {
                Transform list = Find(root.transform, "Panel_CodexList");
                if (list == null) throw new InvalidOperationException("找不到 CodexPage/Panel_CodexList");
                ConfigureContainerLayout(list);
                string[] names = { "PartScroll", "FormScroll", "CaseScroll" };
                foreach (Transform child in list.GetComponentsInChildren<Transform>(true))
                    if (child != list && Array.IndexOf(names, child.name) >= 0) UnityEngine.Object.DestroyImmediate(child.gameObject);
                RectTransform[] roots = new RectTransform[3];
                for (int i = 0; i < names.Length; i++)
                {
                    roots[i] = CreateRect(list, names[i]); roots[i].anchorMin = Vector2.zero; roots[i].anchorMax = Vector2.one; roots[i].sizeDelta = Vector2.zero; roots[i].gameObject.SetActive(i == 0); AddVerticalLayout(roots[i], 8f);
                }
                Component panel = FindComponentByType(root, "CodexPanel");
                SetRef(panel, "_partItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(part)); SetRef(panel, "_formItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(form)); SetRef(panel, "_caseItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(@case));
                SetRef(panel, "_partRoot", roots[0]); SetRef(panel, "_formRoot", roots[1]); SetRef(panel, "_caseRoot", roots[2]); SetRef(panel, "_spriteCatalog", catalog);
                PrefabUtility.SaveAsPrefabAsset(root, CodexPage);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BindWorkbenchPage(string host, string form, string material, string ledger, UIFormalSpriteCatalog catalog)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(WorkbenchPage);
            try
            {
                Transform body = Find(root.transform, "Panel_WorkbenchBody");
                Transform workbench = body != null ? Find(body, "Panel_Workbench") : null;
                if (workbench == null) throw new InvalidOperationException("找不到 WorkbenchPage/Panel_Workbench");
                foreach (Transform child in workbench.GetComponentsInChildren<Transform>(true))
                    if (child != workbench && (child.name == "HostScroll" || child.name == "FormScroll")) UnityEngine.Object.DestroyImmediate(child.gameObject);
                RectTransform hostRoot = CreateRect(workbench, "HostScroll"); hostRoot.anchorMin = new Vector2(0f, 1f); hostRoot.anchorMax = new Vector2(1f, 1f); hostRoot.pivot = new Vector2(0.5f, 1f); hostRoot.anchoredPosition = Vector2.zero; hostRoot.sizeDelta = new Vector2(0f, 104f);
                hostRoot.gameObject.AddComponent<RectMask2D>();
                RectTransform hostContent = CreateRect(hostRoot, "HostContent"); hostContent.anchorMin = new Vector2(0f, 1f); hostContent.anchorMax = new Vector2(0f, 1f); hostContent.pivot = new Vector2(0f, 1f); hostContent.anchoredPosition = Vector2.zero; hostContent.sizeDelta = new Vector2(184f * 11f + 12f * 10f, 104f); AddHorizontalLayout(hostContent, 12f);
                ScrollRect hostScroll = hostRoot.gameObject.AddComponent<ScrollRect>(); hostScroll.content = hostContent; hostScroll.viewport = hostRoot; hostScroll.horizontal = true; hostScroll.vertical = false; hostScroll.movementType = ScrollRect.MovementType.Clamped;
                RectTransform formRoot = CreateRect(workbench, "FormScroll"); formRoot.anchorMin = new Vector2(0f, 1f); formRoot.anchorMax = new Vector2(1f, 1f); formRoot.pivot = new Vector2(0.5f, 1f); formRoot.anchoredPosition = new Vector2(0f, -116f); formRoot.sizeDelta = new Vector2(0f, 300f); AddVerticalLayout(formRoot, 8f);
                Component panel = FindComponentByType(root, "WorkbenchPanel");
                GameObject hostAsset = AssetDatabase.LoadAssetAtPath<GameObject>(host);
                GameObject formAsset = AssetDatabase.LoadAssetAtPath<GameObject>(form);
                SetRef(panel, "_hostItemTemplate", hostAsset);
                SetRef(panel, "_formItemTemplate", formAsset); SetRef(panel, "_materialItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(material)); SetRef(panel, "_ledgerItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(ledger)); SetRef(panel, "_spriteCatalog", catalog);
                PrefabUtility.SaveAsPrefabAsset(root, WorkbenchPage);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BindMapPage(string eventItem)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(MapPage);
            try
            {
                Component panel = FindComponentByType(root, "MapPanel");
                SetRef(panel, "_eventItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(eventItem));
                PrefabUtility.SaveAsPrefabAsset(root, MapPage);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BindJournalPage(string item)
        {
            BindJournalPrefab("Assets/Game/Prefabs/UI/MainPageShell.prefab", item);
            BindJournalPrefab(JournalPage, item);
        }

        private static void BindJournalPrefab(string prefabPath, string item)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Component shell = FindComponentByType(root, "MainPageShell");
                if (shell != null)
                {
                    SetRef(shell, "m_JournalItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(item));
                }
                Component panel = FindComponentByType(root, "JournalPanel");
                if (panel != null)
                {
                    SetRef(panel, "_itemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(item));
                }
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BindGuestPage(string item)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(GuestPage);
            try
            {
                Component panel = FindComponentByType(root, "GuestPanel");
                SetRef(panel, "_delegationItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(item));
                PrefabUtility.SaveAsPrefabAsset(root, GuestPage);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static Component FindComponentByType(GameObject root, string typeName)
        {
            foreach (MonoBehaviour candidate in root.GetComponentsInChildren<MonoBehaviour>(true)) if (candidate != null && candidate.GetType().Name == typeName) return candidate;
            throw new InvalidOperationException("找不到组件 " + typeName);
        }

        private static void AddVerticalLayout(RectTransform root, float spacing)
        {
            VerticalLayoutGroup layout = root.gameObject.GetComponent<VerticalLayoutGroup>() ?? root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing; layout.childControlWidth = true; layout.childControlHeight = false; layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
        }

        private static void AddHorizontalLayout(RectTransform root, float spacing)
        {
            HorizontalLayoutGroup layout = root.gameObject.GetComponent<HorizontalLayoutGroup>() ?? root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing; layout.childControlWidth = false; layout.childControlHeight = true; layout.childForceExpandWidth = false; layout.childForceExpandHeight = false;
        }

        private static void ConfigureContainerLayout(Transform container)
        {
            VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup>();
            if (layout == null) layout = container.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
        }

        private static UIFormalSpriteCatalog CreateFormalSpriteCatalog()
        {
            UIFormalSpriteCatalog catalog = AssetDatabase.LoadAssetAtPath<UIFormalSpriteCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<UIFormalSpriteCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            SerializedObject serialized = new SerializedObject(catalog);
            SerializedProperty entries = serialized.FindProperty("_entries");
            entries.ClearArray();
            string[,] assets =
            {
                { "ICO-008保管", "图标/ICO-008保管.png" },
                { "ICO-051维修费", "图标/ICO-051维修费.png" },
                { "ICO-052铜芯线", "图标/ICO-052铜芯线.png" },
                { "ICO-053精密齿轮", "图标/ICO-053精密齿轮.png" },
                { "ICO-054玻璃镜片", "图标/ICO-054玻璃镜片.png" },
                { "ICO-055校准簧片", "图标/ICO-055校准簧片.png" },
                { "ICO-056定势残晶", "图标/ICO-056定势残晶.png" },
                { "ICO-057异常纹样", "图标/ICO-057异常纹样.png" },
                { "ICO-058图样", "图标/ICO-058图样.png" },
                { "ICO-060零件", "图标/ICO-060零件.png" },
                { "ICO-061形态", "图标/ICO-061形态.png" },
                { "SHR-005-normal", "九宫格/SHR-005-normal列表行.png" },
                { "SHR-005-selected", "九宫格/SHR-005-selected列表行.png" },
                { "SHR-003-normal", "九宫格/SHR-003-normal卡片.png" },
                { "SHR-046-normal", "控件/SHR-046-normal六边形框.png" },
                { "SHR-046-selected", "控件/SHR-046-selected六边形框.png" },
                { "SHR-048-empty-list", "插画/SHR-048-empty-list.png" }
            };
            for (int i = 0; i < assets.GetLength(0); i++)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Sprites/UI/" + assets[i, 1]);
                if (sprite == null) continue;
                entries.InsertArrayElementAtIndex(entries.arraySize);
                SerializedProperty entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                entry.FindPropertyRelative("Key").stringValue = assets[i, 0];
                entry.FindPropertyRelative("Sprite").objectReferenceValue = sprite;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static string CreateArchiveItem(string prefabName, string backgroundPrefix)
        {
            string path = ItemDir + "/" + prefabName + ".prefab";
            return CreateItemVisual(path, prefabName, backgroundPrefix, (root, background) =>
            {
                Component logic = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(root, "Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs (116,33)", "ArchiveItem");
                SetRef(logic, "_icon", FindComponent<Image>(root, "Icon"));
                SetRef(logic, "_title", FindComponent<TMP_Text>(root, "Txt_Title"));
                SetRef(logic, "_detail", FindComponent<TMP_Text>(root, "Txt_Detail"));
                SetRef(logic, "_value", FindComponent<TMP_Text>(root, "Txt_Value"));
                if (prefabName == "WorkbenchHostItem" || prefabName == "WorkbenchFormItem")
                {
                    LayoutElement element = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
                    element.minWidth = 200f; element.preferredWidth = 200f; element.minHeight = 80f; element.preferredHeight = 80f;
                }
            });
        }

        private static string CreateCodexItem(string prefabName, string backgroundPrefix)
        {
            string path = ItemDir + "/" + prefabName + ".prefab";
            return CreateItemVisual(path, prefabName, backgroundPrefix, (root, background) =>
            {
                Component logic = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(root, "Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs (129,32)", "CodexItem");
                SetRef(logic, "_icon", FindComponent<Image>(root, "Icon"));
                SetRef(logic, "_stateFrame", FindComponent<Image>(root, "StateFrame"));
                SetRef(logic, "_id", FindComponent<TMP_Text>(root, "Txt_Id"));
                SetRef(logic, "_title", FindComponent<TMP_Text>(root, "Txt_Title"));
                SetRef(logic, "_state", FindComponent<TMP_Text>(root, "Txt_State"));
                SetRef(logic, "_source", FindComponent<TMP_Text>(root, "Txt_Source"));
            });
        }

        private static string CreateWorkbenchItem(string prefabName, string backgroundPrefix)
        {
            string path = ItemDir + "/" + prefabName + ".prefab";
            // Workbench Item 的布局契约发生变化时必须重建模板，避免旧版宽文本节点残留。
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            return CreateItemVisual(path, prefabName, backgroundPrefix, (root, background) =>
            {
                if (prefabName == "WorkbenchHostItem") ConfigureWorkbenchHostVisual(root);
                else ConfigureWorkbenchRowVisual(root);
                Component logic = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(root, "Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs (144,36)", "WorkbenchItem");
                SetRef(logic, "_background", root.GetComponent<Image>());
                SetRef(logic, "_icon", FindComponent<Image>(root, "Icon"));
                SetRef(logic, "_title", FindComponent<TMP_Text>(root, "Txt_Title"));
                SetRef(logic, "_detail", FindComponent<TMP_Text>(root, "Txt_Detail"));
                SetRef(logic, "_value", FindComponent<TMP_Text>(root, "Txt_Value"));
            });
        }

        private static string CreateMapEventItem()
        {
            string path = ItemDir + "/MapEventItem.prefab";
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            return CreateItemVisual(path, "MapEventItem", "SHR-005-normal列表行", (root, background) =>
            {
                if (root.GetComponent<Button>() == null) root.AddComponent<Button>();
                foreach (string childName in new[] { "Txt_Id", "Icon", "StateFrame", "Txt_State", "Txt_Source", "Txt_Value" })
                {
                    Transform child = root.transform.Find(childName);
                    if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
                RectTransform title = root.transform.Find("Txt_Title") as RectTransform;
                if (title != null) { title.anchoredPosition = new Vector2(20f, 12f); title.sizeDelta = new Vector2(760f, 30f); title.GetComponent<TMP_Text>().fontSize = 20f; }
                RectTransform detail = root.transform.Find("Txt_Detail") as RectTransform;
                if (detail != null) { detail.anchoredPosition = new Vector2(20f, -16f); detail.sizeDelta = new Vector2(760f, 24f); detail.GetComponent<TMP_Text>().fontSize = 15f; }
                Component logic = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(root, "Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs (465,36)", "MapEventItem");
                SetRef(logic, "_title", FindComponent<TMP_Text>(root, "Txt_Title"));
                SetRef(logic, "_meta", FindComponent<TMP_Text>(root, "Txt_Detail"));
                SetRef(logic, "_button", root.GetComponent<Button>());
            });
        }

        private static string CreateJournalItem()
        {
            string path = ItemDir + "/JournalItem.prefab";
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            return CreateItemVisual(path, "JournalItem", "SHR-005-normal列表行", (root, background) =>
            {
                if (root.GetComponent<Button>() == null) root.AddComponent<Button>();
                background.pixelsPerUnitMultiplier = 2f;
                foreach (string childName in new[] { "Txt_Id", "Icon", "StateFrame", "Txt_State", "Txt_Source", "Txt_Value" })
                {
                    Transform child = root.transform.Find(childName);
                    if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
                RectTransform title = root.transform.Find("Txt_Title") as RectTransform;
                if (title != null) { title.anchoredPosition = new Vector2(18f, 10f); title.sizeDelta = new Vector2(680f, 30f); title.GetComponent<TMP_Text>().fontSize = 20f; }
                RectTransform detail = root.transform.Find("Txt_Detail") as RectTransform;
                if (detail != null) { detail.anchoredPosition = new Vector2(18f, -17f); detail.sizeDelta = new Vector2(680f, 24f); detail.GetComponent<TMP_Text>().fontSize = 14f; }
                GameObject action = new GameObject("Action", typeof(RectTransform), typeof(Image), typeof(Button));
                action.transform.SetParent(root.transform, false);
                RectTransform actionRect = action.transform as RectTransform;
                actionRect.anchorMin = new Vector2(1f, 0.5f); actionRect.anchorMax = actionRect.anchorMin; actionRect.pivot = new Vector2(1f, 0.5f); actionRect.anchoredPosition = new Vector2(-16f, 0f); actionRect.sizeDelta = new Vector2(130f, 40f);
                TextMeshProUGUI actionText = CreateTextChild(action.transform, "Label", Vector2.zero, Vector2.zero, 18, TextAlignmentOptions.Center).GetComponent<TextMeshProUGUI>();
                (actionText.transform as RectTransform).anchorMin = Vector2.zero; (actionText.transform as RectTransform).anchorMax = Vector2.one; (actionText.transform as RectTransform).sizeDelta = Vector2.zero;
                Component logic = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(root, "Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs (505,36)", "JournalItem");
                SetRef(logic, "_title", FindComponent<TMP_Text>(root, "Txt_Title")); SetRef(logic, "_detail", FindComponent<TMP_Text>(root, "Txt_Detail")); SetRef(logic, "_actionLabel", actionText); SetRef(logic, "_button", root.GetComponent<Button>());
            });
        }

        private static string CreateDialogueChoiceItem()
        {
            string path = ItemDir + "/DialogueChoiceItem.prefab";
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            return CreateItemVisual(path, "DialogueChoiceItem", "SHR-003-normal卡片", (root, background) =>
            {
                if (root.GetComponent<Button>() == null) root.AddComponent<Button>();
                foreach (string childName in new[] { "Txt_Id", "Icon", "StateFrame", "Txt_Detail", "Txt_State", "Txt_Source", "Txt_Value" })
                {
                    Transform child = root.transform.Find(childName);
                    if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
                RectTransform title = root.transform.Find("Txt_Title") as RectTransform;
                if (title != null) { title.anchorMin = Vector2.zero; title.anchorMax = Vector2.one; title.offsetMin = new Vector2(18f, 0f); title.offsetMax = new Vector2(-18f, 0f); title.GetComponent<TMP_Text>().fontSize = 22f; title.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Left; }
                Component logic = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(root, "Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs (560,36)", "DialogueChoiceItem");
                SetRef(logic, "_label", FindComponent<TMP_Text>(root, "Txt_Title")); SetRef(logic, "_button", root.GetComponent<Button>());
            });
        }

        private static string CreateInvestigationHotspotItem()
        {
            string path = ItemDir + "/InvestigationHotspotItem.prefab";
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            return CreateItemVisual(path, "InvestigationHotspotItem", "SHR-003-normal卡片", (root, background) =>
            {
                if (root.GetComponent<Button>() == null) root.AddComponent<Button>();
                foreach (string childName in new[] { "Txt_Id", "Icon", "StateFrame", "Txt_Detail", "Txt_State", "Txt_Source", "Txt_Value" })
                {
                    Transform child = root.transform.Find(childName); if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
                RectTransform title = root.transform.Find("Txt_Title") as RectTransform;
                if (title != null) { title.anchorMin = Vector2.zero; title.anchorMax = Vector2.one; title.offsetMin = Vector2.zero; title.offsetMax = Vector2.zero; title.GetComponent<TMP_Text>().fontSize = 18f; title.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center; }
                RectTransform rt = root.transform as RectTransform; rt.sizeDelta = new Vector2(140f, 140f);
                Component logic = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(root, "Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs (615,36)", "InvestigationHotspotItem");
                SetRef(logic, "_ring", root.GetComponent<Image>()); SetRef(logic, "_label", FindComponent<TMP_Text>(root, "Txt_Title")); SetRef(logic, "_button", root.GetComponent<Button>());
            });
        }

        private static string CreateGuestDelegationItem()
        {
            string path = ItemDir + "/GuestDelegationItem.prefab";
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            return CreateItemVisual(path, "GuestDelegationItem", "SHR-005-normal列表行", (root, background) =>
            {
                if (root.GetComponent<Button>() == null) root.AddComponent<Button>();
                foreach (string childName in new[] { "Txt_Id", "Icon", "StateFrame", "Txt_Detail", "Txt_State", "Txt_Source", "Txt_Value" })
                {
                    Transform child = root.transform.Find(childName); if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
                RectTransform title = root.transform.Find("Txt_Title") as RectTransform;
                if (title != null) { title.anchoredPosition = new Vector2(18f, 0f); title.sizeDelta = new Vector2(560f, 50f); title.GetComponent<TMP_Text>().fontSize = 20f; }
                GameObject action = new GameObject("Action", typeof(RectTransform), typeof(Image), typeof(Button)); action.transform.SetParent(root.transform, false);
                RectTransform actionRect = action.transform as RectTransform; actionRect.anchorMin = new Vector2(1f, 0.5f); actionRect.anchorMax = actionRect.anchorMin; actionRect.pivot = new Vector2(1f, 0.5f); actionRect.anchoredPosition = new Vector2(-16f, 0f); actionRect.sizeDelta = new Vector2(150f, 44f);
                TMP_Text actionText = CreateTextChild(action.transform, "Label", Vector2.zero, Vector2.zero, 18, TextAlignmentOptions.Center).GetComponent<TMP_Text>(); RectTransform ar = actionText.transform as RectTransform; ar.anchorMin = Vector2.zero; ar.anchorMax = Vector2.one; ar.sizeDelta = Vector2.zero; actionText.text = "去处理";
                Component logic = UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(root, "Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs (680,36)", "GuestDelegationItem"); SetRef(logic, "_title", FindComponent<TMP_Text>(root, "Txt_Title")); SetRef(logic, "_button", action.GetComponent<Button>()); SetRef(logic, "_buttonLabel", actionText);
            });
        }

        private static void ConfigureWorkbenchHostVisual(GameObject root)
        {
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f); rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f); rt.sizeDelta = new Vector2(184f, 96f);
            LayoutElement layout = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
            layout.minWidth = 184f; layout.preferredWidth = 184f; layout.minHeight = 96f; layout.preferredHeight = 96f;
            foreach (string name in new[] { "Txt_Id", "Txt_Detail", "Txt_State", "Txt_Source", "Txt_Value", "StateFrame" })
            {
                Transform child = root.transform.Find(name);
                if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            RectTransform icon = root.transform.Find("Icon") as RectTransform;
            if (icon != null) { icon.anchorMin = new Vector2(0.5f, 1f); icon.anchorMax = icon.anchorMin; icon.pivot = new Vector2(0.5f, 1f); icon.anchoredPosition = new Vector2(0f, -10f); icon.sizeDelta = new Vector2(42f, 42f); }
            RectTransform title = root.transform.Find("Txt_Title") as RectTransform;
            if (title != null)
            {
                title.anchorMin = new Vector2(0f, 0f); title.anchorMax = new Vector2(1f, 0f); title.pivot = new Vector2(0.5f, 0f);
                title.anchoredPosition = new Vector2(0f, 8f); title.sizeDelta = new Vector2(-12f, 28f);
                TMP_Text text = title.GetComponent<TMP_Text>(); text.fontSize = 16f; text.alignment = TextAlignmentOptions.Center; text.enableWordWrapping = false; text.overflowMode = TextOverflowModes.Ellipsis;
            }
        }

        private static void ConfigureWorkbenchRowVisual(GameObject root)
        {
            RectTransform rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f); rt.sizeDelta = new Vector2(-24f, 76f);
            LayoutElement layout = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
            layout.minHeight = 76f; layout.preferredHeight = 76f; layout.flexibleWidth = 1f;
            foreach (string name in new[] { "Txt_Id", "Txt_State", "Txt_Source", "StateFrame" })
            {
                Transform child = root.transform.Find(name);
                if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
            RectTransform icon = root.transform.Find("Icon") as RectTransform;
            if (icon != null) { icon.anchorMin = new Vector2(0f, 0.5f); icon.anchorMax = icon.anchorMin; icon.pivot = new Vector2(0f, 0.5f); icon.anchoredPosition = new Vector2(18f, 0f); icon.sizeDelta = new Vector2(42f, 42f); }
            RectTransform title = root.transform.Find("Txt_Title") as RectTransform;
            if (title != null) { title.anchoredPosition = new Vector2(76f, 14f); title.sizeDelta = new Vector2(620f, 30f); title.GetComponent<TMP_Text>().fontSize = 20f; title.GetComponent<TMP_Text>().enableWordWrapping = false; title.GetComponent<TMP_Text>().overflowMode = TextOverflowModes.Ellipsis; }
            RectTransform detail = root.transform.Find("Txt_Detail") as RectTransform;
            if (detail != null) { detail.anchoredPosition = new Vector2(76f, -18f); detail.sizeDelta = new Vector2(620f, 24f); detail.GetComponent<TMP_Text>().fontSize = 15f; detail.GetComponent<TMP_Text>().enableWordWrapping = false; detail.GetComponent<TMP_Text>().overflowMode = TextOverflowModes.Ellipsis; }
            RectTransform value = root.transform.Find("Txt_Value") as RectTransform;
            if (value != null) { value.anchorMin = new Vector2(1f, 0.5f); value.anchorMax = value.anchorMin; value.pivot = new Vector2(1f, 0.5f); value.anchoredPosition = new Vector2(-18f, 0f); value.sizeDelta = new Vector2(220f, 30f); value.GetComponent<TMP_Text>().fontSize = 18f; value.GetComponent<TMP_Text>().enableWordWrapping = false; value.GetComponent<TMP_Text>().overflowMode = TextOverflowModes.Ellipsis; }
        }

        private static string CreateItemVisual(string path, string prefabName, string backgroundPrefix, Action<GameObject, Image> configure)
        {
            if (File.Exists(path)) return path;
            GameObject root = new GameObject(prefabName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = new Vector2(0f, 1f); rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f); rootRect.sizeDelta = new Vector2(-24f, 96f);
            Image background = root.GetComponent<Image>();
            background.sprite = FindSprite(backgroundPrefix); background.type = Image.Type.Sliced; background.raycastTarget = false;
            CreateTextChild(root.transform, "Txt_Id", new Vector2(18f, 0f), new Vector2(100f, 0f), 18, TextAlignmentOptions.Left);
            CreateImageChild(root.transform, "Icon", new Vector2(128f, 0f), new Vector2(40f, 40f));
            CreateImageChild(root.transform, "StateFrame", new Vector2(128f, 0f), new Vector2(40f, 40f));
            CreateTextChild(root.transform, "Txt_Title", new Vector2(184f, 14f), new Vector2(500f, 34f), 22, TextAlignmentOptions.Left);
            CreateTextChild(root.transform, "Txt_Detail", new Vector2(184f, -20f), new Vector2(500f, 28f), 16, TextAlignmentOptions.Left);
            CreateTextChild(root.transform, "Txt_State", new Vector2(700f, 14f), new Vector2(220f, 34f), 18, TextAlignmentOptions.Right);
            CreateTextChild(root.transform, "Txt_Source", new Vector2(184f, -20f), new Vector2(500f, 24f), 16, TextAlignmentOptions.Left);
            CreateTextChild(root.transform, "Txt_Value", new Vector2(-20f, 0.5f), new Vector2(180f, 34f), 20, TextAlignmentOptions.Right, true);
            configure(root, background);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return path;
        }

        private static GameObject CreateImageChild(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform; rect.anchorMin = new Vector2(0f, 0.5f); rect.anchorMax = new Vector2(0f, 0.5f); rect.pivot = new Vector2(0f, 0.5f); rect.anchoredPosition = pos; rect.sizeDelta = size;
            go.GetComponent<Image>().preserveAspect = true; go.GetComponent<Image>().raycastTarget = false;
            return go;
        }

        private static GameObject CreateTextChild(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, TextAlignmentOptions alignment, bool rightAnchored = false)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = rightAnchored ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f); rect.anchorMax = rect.anchorMin; rect.pivot = rightAnchored ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f); rect.anchoredPosition = pos; rect.sizeDelta = size;
            TMP_Text text = go.GetComponent<TMP_Text>(); text.font = UIFactory.BuiltinFont; text.fontSize = fontSize; text.alignment = alignment; text.raycastTarget = false; text.text = string.Empty;
            return go;
        }

        private static void CreateSectionHeader(RectTransform parent, string title)
        {
            GameObject go = CreateTextChild(parent, "Header_" + title, Vector2.zero, new Vector2(0f, 44f), 28, TextAlignmentOptions.Center);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f); rect.anchoredPosition = Vector2.zero;
            go.GetComponent<TMP_Text>().text = title;
            go.GetComponent<TMP_Text>().color = new Color(1f, 0.72f, 0.2f, 1f);
            LayoutElement element = go.AddComponent<LayoutElement>();
            element.minHeight = 44f; element.preferredHeight = 44f;
        }

        private static string CreateSaveSlotItem()
        {
            if (File.Exists(ItemDir + "/SaveSlotItem.prefab")) return ItemDir + "/SaveSlotItem.prefab";
            return CreateFromChild(SavePage, "Panel_Slot1", ItemDir + "/SaveSlotItem.prefab", root =>
            {
                root.name = "SaveSlotItem";
                Rename(root, "Txt_Slot1", "Txt_Summary");
                Rename(root, "Btn_Slot1", "Btn_Action");
                Rename(root, "Txt_Slot1Label", "Txt_ActionLabel");
                Rename(root, "Btn_Delete1", "Btn_Delete");
                Rename(root, "Txt_Delete1Label", "Txt_DeleteLabel");
                Component logic = root.GetComponent("SaveSlotItem") ?? UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(root, "Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs (43,72)", "SaveSlotItem");
                SetRef(logic, "_summary", FindComponent<TMP_Text>(root, "Txt_Summary"));
                SetRef(logic, "_actionLabel", FindComponent<TMP_Text>(root, "Txt_ActionLabel"));
                SetRef(logic, "_actionButton", FindComponent<Button>(root, "Btn_Action"));
                SetRef(logic, "_deleteButton", FindComponent<Button>(root, "Btn_Delete"));
            });
        }

        private static string CreateRewardChoiceItem()
        {
            if (File.Exists(ItemDir + "/RewardChoiceItem.prefab")) return ItemDir + "/RewardChoiceItem.prefab";
            return CreateFromChild(SettlementPage, "Btn_Choice0", ItemDir + "/RewardChoiceItem.prefab", root =>
            {
                root.name = "RewardChoiceItem";
                Rename(root, "Txt_Choice0Label", "Txt_Label");
                Component logic = root.GetComponent("RewardChoiceItem") ?? UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(root, "Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs (57,76)", "RewardChoiceItem");
                SetRef(logic, "_label", FindComponent<TMP_Text>(root, "Txt_Label"));
                SetRef(logic, "_button", root.GetComponent<Button>());
            });
        }

        private static string CreateCarryAvailableItem()
        {
            if (File.Exists(ItemDir + "/CarryAvailableItem.prefab")) return ItemDir + "/CarryAvailableItem.prefab";
            return CreateFromChild(PreparationPage, "Item_CarryAvailableTemplate", ItemDir + "/CarryAvailableItem.prefab", root =>
            {
                root.name = "CarryAvailableItem";
                Rename(root, "Txt_CarryAvailableItemLabel", "Txt_Label");
                Component logic = root.GetComponent("CarryAvailableItem") ?? UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(root, "Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs (69,78)", "CarryAvailableItem");
                SetRef(logic, "_label", FindComponent<TMP_Text>(root, "Txt_Label"));
                SetRef(logic, "_button", root.GetComponent<Button>());
            });
        }

        private static void BindSaveSlotPage(string itemPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(SavePage);
            try
            {
                foreach (string name in new[] { "Panel_Slot1", "Panel_Slot2", "Panel_Slot3" })
                {
                    Transform old = root.transform.Find(name);
                    if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                }
                // 旧版生成器可能已经创建过重复容器；重新绑定时先清理，保证契约只有一个 SlotsRoot。
                Transform[] oldSlotRoots = root.GetComponentsInChildren<Transform>(true);
                for (int i = oldSlotRoots.Length - 1; i >= 0; i--)
                {
                    if (oldSlotRoots[i] != null && oldSlotRoots[i].name == "SlotsRoot")
                        UnityEngine.Object.DestroyImmediate(oldSlotRoots[i].gameObject);
                }
                RectTransform slots = CreateRect(root.transform, "SlotsRoot");
                slots.anchorMin = new Vector2(0f, 1f);
                slots.anchorMax = new Vector2(1f, 1f);
                slots.pivot = new Vector2(0.5f, 1f);
                slots.anchoredPosition = new Vector2(0f, -380f);
                slots.sizeDelta = new Vector2(0f, 600f);
                Component logic = root.GetComponent("SaveSlotPage");
                SetRef(logic, "_slotItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(itemPath));
                SetRef(logic, "_slotsRoot", slots);
                PrefabUtility.SaveAsPrefabAsset(root, SavePage);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BindSettlementPage(string itemPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(SettlementPage);
            try
            {
                Transform[] all = root.GetComponentsInChildren<Transform>(true);
                for (int i = all.Length - 1; i >= 0; i--)
                {
                    if (all[i] != null && all[i].name.StartsWith("Btn_Choice", StringComparison.Ordinal))
                        UnityEngine.Object.DestroyImmediate(all[i].gameObject);
                }
                Component logic = root.GetComponent("SettlementPageForm");
                SetRef(logic, "_rewardChoiceItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(itemPath));
                PrefabUtility.SaveAsPrefabAsset(root, SettlementPage);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BindPreparationPage(string itemPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(PreparationPage);
            try
            {
                Transform old = Find(root.transform, "Item_CarryAvailableTemplate");
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
                Component logic = root.GetComponent("PreparationPageForm");
                SetRef(logic, "_availableItemTemplate", AssetDatabase.LoadAssetAtPath<GameObject>(itemPath));
                PrefabUtility.SaveAsPrefabAsset(root, PreparationPage);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static string CreateFromChild(string sourcePath, string childName, string outputPath, Action<GameObject> configure)
        {
            GameObject sourceRoot = PrefabUtility.LoadPrefabContents(sourcePath);
            try
            {
                Transform source = Find(sourceRoot.transform, childName);
                if (source == null) throw new InvalidOperationException($"找不到 {sourcePath} 内的 {childName}");
                GameObject clone = UnityEngine.Object.Instantiate(source.gameObject);
                clone.name = childName;
                clone.transform.SetParent(null, false);
                configure(clone);
                PrefabUtility.SaveAsPrefabAsset(clone, outputPath);
                UnityEngine.Object.DestroyImmediate(clone);
                return outputPath;
            }
            finally { PrefabUtility.UnloadPrefabContents(sourceRoot); }
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Transform Find(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform found = Find(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private static void Rename(GameObject root, string oldName, string newName)
        {
            Transform found = Find(root.transform, oldName);
            if (found != null) found.name = newName;
        }

        private static T FindComponent<T>(GameObject root, string name) where T : Component
        {
            Transform found = Find(root.transform, name);
            return found != null ? found.GetComponent<T>() : null;
        }

        private static void SetRef(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            if (target == null) throw new InvalidOperationException($"无法设置 {propertyName}：目标为空");
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null) throw new InvalidOperationException($"找不到序列化字段 {propertyName}");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite FindSprite(string prefix)
        {
            string[] guids = AssetDatabase.FindAssets(prefix + " t:Sprite", new[] { "Assets/Game/Sprites/UI" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null && Path.GetFileNameWithoutExtension(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return sprite;
            }
            return null;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
