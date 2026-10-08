#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Everlight.Tales.UI;
using Everlight.Tales.UI.Editor;

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
            string rewardItem = CommonRow();
            string carryItem = CreateCarryAvailableItem();
            BindSaveSlotPage(saveItem);
            BindSettlementPage(rewardItem);
            BindPreparationPage(carryItem);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightUiItemPrefabGenerator] 已生成并接入 SaveSlotItem、通用奖励行、CarryAvailableItem。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/生成并接入地图节点 UIItem", priority = 2021)]
        public static void GenerateAndBindMapNodeItem()
        {
            EnsureFolder(ItemDir);
            string path = ItemDir + "/MapNodeItem.prefab";
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            CreateItemVisual(path, "MapNodeItem", "SHR-003-normal卡片", (root, background) =>
            {
                if (root.GetComponent<Button>() == null) root.AddComponent<Button>();
                foreach (string childName in new[] { "Txt_Id", "Icon", "StateFrame", "Txt_Detail", "Txt_State", "Txt_Source", "Txt_Value" })
                {
                    Transform child = root.transform.Find(childName); if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
                RectTransform title = root.transform.Find("Txt_Title") as RectTransform;
                if (title != null) { title.anchorMin = Vector2.zero; title.anchorMax = Vector2.one; title.offsetMin = Vector2.zero; title.offsetMax = Vector2.zero; title.GetComponent<TMP_Text>().fontSize = 16f; title.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center; }
                (root.transform as RectTransform).sizeDelta = new Vector2(64f, 64f);
                Component logic = AddComponentByName(root, "Everlight.Tales.UI.MapNodeItem");
                SetRef(logic, "_label", FindComponent<TMP_Text>(root, "Txt_Title")); SetRef(logic, "_button", root.GetComponent<Button>());
            });
            BindMapNodeTemplate(path);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("[EverlightUiItemPrefabGenerator] 已生成并接入 MapNodeItem。");
        }

        private static Component AddComponentByName(GameObject root, string fullTypeName)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(candidate => candidate.FullName == fullTypeName);
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                throw new InvalidOperationException("无法找到 UIItem 组件类型: " + fullTypeName);
            }
            return root.AddComponent(type);
        }

        private static void BindMapNodeTemplate(string itemPath)
        {
            // 地图节点是 MapContent 中显式绑定地点 ID 的静态节点；不重写旧模板字段。
            Debug.Log("[MapNodeItem] 已生成特殊节点资源；地图节点由 MapPage 手工配置地点 ID。");
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDescendant(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        [MenuItem("Game Framework/EverlightTales/UI/生成并接入 Archive-Codex-Workbench UIItem", priority = 2022)]
        public static void GenerateAndBindCollectionPages()
        {
            CommonRow();
            ListRowPrefabMigration.BindPage("Assets/Game/Prefabs/UI/ArchivePage.prefab");
            ListRowPrefabMigration.BindPage("Assets/Game/Prefabs/UI/CodexPage.prefab");
            ListRowPrefabMigration.BindPage("Assets/Game/Prefabs/UI/WorkbenchPage.prefab");
            Debug.Log("[ListRow][Generator] ArchivePage, CodexPage, WorkbenchPage 已接入通用行，保留页面布局。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/修复 Workbench 布局与 Item", priority = 2023)]
        public static void RebuildWorkbenchOnly()
        {
            CommonRow();
            ListRowPrefabMigration.BindPage("Assets/Game/Prefabs/UI/WorkbenchPage.prefab");
            Debug.Log("[ListRow][Generator] WorkbenchPage 已接入通用行，保留页面布局。");
        }

        /// <summary>
        /// 修复工作台对通用行的引用及图片过渡，保留宿主与页面的美术布局。
        /// </summary>
        [MenuItem("Game Framework/EverlightTales/UI/修复 Workbench 交互与布局", priority = 2024)]
        public static void RepairWorkbenchInteractionAndLayout()
        {
            CommonRow();
            HomeSubPanelsContractMigration.RepairItemButtons();
            ListRowPrefabMigration.BindPage(WorkbenchPage);
        }







        [MenuItem("Game Framework/EverlightTales/UI/修复 Codex 布局与 Item", priority = 2025)]
        public static void RebuildCodexOnly()
        {
            CommonRow();
            ListRowPrefabMigration.BindPage("Assets/Game/Prefabs/UI/CodexPage.prefab");
            Debug.Log("[ListRow][Generator] CodexPage 已接入通用行，保留页面布局。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/修复 Archive 布局与 Item", priority = 2026)]
        public static void RebuildArchiveOnly()
        {
            CommonRow();
            ListRowPrefabMigration.BindPage("Assets/Game/Prefabs/UI/ArchivePage.prefab");
            Debug.Log("[ListRow][Generator] ArchivePage 已接入通用行，保留页面布局。");
        }

        private static void AddCodexTabLabels()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(CodexPage);
            try
            {
                string[] labels = { "零件", "形态", "怪谈" };
                for (int i = 0; i < labels.Length; i++)
                {
                    Transform button = root.GetComponentsInChildren<Transform>(true)
                        .FirstOrDefault(node => node.name == "Btn_Sub_" + i);
                    if (button == null) continue;
                    TMP_Text label = button.Find("Txt_Label")?.GetComponent<TMP_Text>();
                    if (label == null)
                    {
                        GameObject labelObject = CreateTextChild(button, "Txt_Label", Vector2.zero, Vector2.zero, 22, TextAlignmentOptions.Center);
                        label = labelObject.GetComponent<TMP_Text>();
                        RectTransform rect = labelObject.transform as RectTransform;
                        rect.anchorMin = Vector2.zero;
                        rect.anchorMax = Vector2.one;
                        rect.offsetMin = new Vector2(8f, 4f);
                        rect.offsetMax = new Vector2(-8f, -4f);
                    }
                    label.font = UIFactory.BuiltinFont;
                    label.fontSize = 22f;
                    label.text = labels[i];
                    label.color = Color.white;
                    label.alignment = TextAlignmentOptions.Center;
                    label.raycastTarget = false;
                    EditorUtility.SetDirty(button.gameObject);
                }
                PrefabUtility.SaveAsPrefabAsset(root, CodexPage);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void EnsureArchiveTabButtons(Transform pageRoot, string[] labels)
        {
            Transform top = Find(pageRoot, "Panel_ArchiveTop");
            if (top == null) return;
            RectTransform topRect = top as RectTransform;
            topRect.sizeDelta = new Vector2(topRect.sizeDelta.x, 168f);
            for (int i = 0; i < labels.Length; i++)
            {
                Transform existing = top.Find("Btn_Sub_" + i);
                GameObject button = existing != null ? existing.gameObject : CreateRect(top, "Btn_Sub_" + i).gameObject;
                RectTransform rect = button.transform as RectTransform;
                rect.anchorMin = new Vector2(0.5f, 1f); rect.anchorMax = rect.anchorMin; rect.pivot = new Vector2(0.5f, 1f);
                rect.sizeDelta = new Vector2(132f, 52f); rect.anchoredPosition = new Vector2(-300f + i * 150f, -104f);
                Image image = button.GetComponent<Image>() ?? button.AddComponent<Image>();
                image.sprite = FindSprite("SHR-028-normal页签5"); image.type = Image.Type.Sliced; image.color = Color.white;
                Button buttonComponent = button.GetComponent<Button>() ?? button.AddComponent<Button>();
                buttonComponent.targetGraphic = image;
                TMP_Text text = button.transform.Find("Txt_Label")?.GetComponent<TMP_Text>();
                if (text == null)
                {
                    GameObject label = CreateTextChild(button.transform, "Txt_Label", Vector2.zero, Vector2.zero, 20, TextAlignmentOptions.Center);
                    text = label.GetComponent<TMP_Text>();
                    RectTransform labelRect = label.transform as RectTransform; labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one; labelRect.offsetMin = new Vector2(6f, 3f); labelRect.offsetMax = new Vector2(-6f, -3f);
                }
                text.text = labels[i]; text.color = Color.white; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            }
        }

        [MenuItem("Game Framework/EverlightTales/UI/修复 HomePage 加工 Item 引用", priority = 2024)]
        public static void BindHomeWorkbenchItems()
        {
            GameObject root = PrefabUtility.LoadPrefabContents("Assets/Game/Prefabs/UI/HomePage.prefab");
            try
            {
                Component home = FindComponentByType(root, "HomePanel");
                PrefabUtility.SaveAsPrefabAsset(root, "Assets/Game/Prefabs/UI/HomePage.prefab");
                Debug.Log("[EverlightUiItemPrefabGenerator] HomePage 不再持有加工 Item 引用，工作台统一使用 WorkbenchPage。");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [MenuItem("Game Framework/EverlightTales/UI/第二批动态 Item/生成并接入地图事件 Item", priority = 2026)]
        public static void GenerateAndBindMapEventItem()
        {
            CommonRow();
            ListRowPrefabMigration.BindPage("Assets/Game/Prefabs/UI/MapPage.prefab");
            Debug.Log("[ListRow][Generator] MapPage 已接入通用行，保留页面布局。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/第二批动态 Item/生成并接入日志条目 Item", priority = 2027)]
        public static void GenerateAndBindJournalItem()
        {
            CommonRow();
            ListRowPrefabMigration.BindPage("Assets/Game/Prefabs/UI/JournalPage.prefab");
            ListRowPrefabMigration.BindPage("Assets/Game/Prefabs/UI/MainPageShell.prefab");
            Debug.Log("[ListRow][Generator] JournalPage, MainPageShell 已接入通用行，保留页面布局。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/第二批动态 Item/生成对话选项 Item", priority = 2028)]
        public static void GenerateDialogueChoiceItem()
        {
            EnsureFolder(ItemDir);
            CommonRow();
            ListRowPrefabMigration.BindPage("Assets/Game/Prefabs/UI/BoardPage.prefab");
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
            CommonRow();
            ListRowPrefabMigration.BindPage("Assets/Game/Prefabs/UI/GuestPage.prefab");
            Debug.Log("[ListRow][Generator] GuestPage 已接入通用行，保留页面布局。");
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

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/HomePage", priority = 2030)]
        public static void EnableHomeValidation() => UIValidationHarness.Configure("HomePage");

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/CodexPage", priority = 2031)]
        public static void EnableCodexValidation() => UIValidationHarness.Configure("CodexPage");

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/WorkbenchPage", priority = 2032)]
        public static void EnableWorkbenchValidation() => UIValidationHarness.Configure("WorkbenchPage");

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/JournalPage", priority = 2032)]
        public static void EnableJournalValidation() => UIValidationHarness.Configure("JournalPage");

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/关闭", priority = 2033)]
        public static void DisableValidation() => UIValidationHarness.Disable();

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/启用正常流程测试", priority = 2033)]
        public static void EnableNormalFlowValidation() => UIValidationHarness.ConfigureNormalFlow(true);

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/PlayMode立即注入", priority = 2034)]
        public static void InjectValidationNow()
        {
            if (Application.isPlaying) UIValidationHarness.Start();
            else Debug.LogWarning("请先进入 PlayMode，再执行此菜单。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/验证运行时数据/正常流程测试-从存档进入主页", priority = 2034)]
        public static void EnterMainShellFromSaveSlotForTest()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("请先进入 PlayMode，再执行此菜单。");
                return;
            }

            System.Type pageType = System.Type.GetType("Everlight.Tales.UI.SaveSlotPage, Everlight.Tales.UI");
            UnityEngine.Object page = pageType == null
                ? null
                : Resources.FindObjectsOfTypeAll(pageType)
                    .FirstOrDefault(item => item != null && ((Component)item).gameObject.scene.IsValid());
            if (page == null)
            {
                Debug.LogError("[UI诊断][测试] 当前 PlayMode 未找到 SaveSlotPage。");
                return;
            }

            var method = pageType.GetMethod("EnterSlot", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (method == null)
            {
                Debug.LogError("[UI诊断][测试] SaveSlotPage.EnterSlot 不存在。");
                return;
            }

            Debug.Log("[UI诊断][测试] 调用 SaveSlotPage.EnterSlot(1)，模拟从存档进入主页。");
            method.Invoke(page, new object[] { 1 });
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
            return Everlight.Tales.UI.Editor.FormalResourceMigration.BuildCatalog();
        }

        private static string CommonRow()
        {
            ListRowPrefabMigration.CreatePrefab();
            return ListRowPrefabMigration.RowPath;
        }

        private static string CreateWorkbenchItem(string prefabName, string backgroundPrefix)
        {
            string path = ItemDir + "/" + prefabName + ".prefab";
            // 增量升级，保留已被 Home/Workbench 引用的 Prefab GUID。
            if (File.Exists(path))
            {
                FormalResourceMigration.BuildCatalog();
                GameObject existing = PrefabUtility.LoadPrefabContents(path);
                try { FormalResourceMigration.Upgrade(existing,path); PrefabUtility.SaveAsPrefabAsset(existing,path); }
                finally { PrefabUtility.UnloadPrefabContents(existing); }
                return path;
            }
            return CreateItemVisual(path, prefabName, backgroundPrefix, (root, background) =>
            {
                // WorkbenchPanel binds selection listeners to the item root.  Keep
                // the interaction contract in the prefab instead of relying on a
                // runtime component lookup that silently returns null.
                background.raycastTarget = true;
                Button button = root.GetComponent<Button>() ?? root.AddComponent<Button>();
                button.transition = Selectable.Transition.ColorTint;
                button.targetGraphic = background;
                ConfigureWorkbenchHostVisual(root);
                Component logic = AddComponentByName(root, "Everlight.Tales.UI.WorkbenchItem");
                SetRef(logic, "_background", root.GetComponent<Image>());
                SetRef(logic, "_icon", FindComponent<Image>(root, "Icon"));
                SetRef(logic, "_title", FindComponent<TMP_Text>(root, "Txt_Title"));
                SetRef(logic, "_detail", FindComponent<TMP_Text>(root, "Txt_Detail"));
                SetRef(logic, "_value", FindComponent<TMP_Text>(root, "Txt_Value"));
                FormalResourceMigration.BuildCatalog();
                FormalResourceMigration.Upgrade(root,path);
            });
        }




        private static string CreateInvestigationHotspotItem()
        {
            Everlight.Tales.UI.Editor.SpecialItemPrefabMigration.Investigation();
            return ItemDir + "/InvestigationHotspotItem.prefab";
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
                Component logic = root.GetComponent("SaveSlotItem") ?? AddComponentByName(root, "Everlight.Tales.UI.SaveSlotItem");
                SetRef(logic, "_summary", FindComponent<TMP_Text>(root, "Txt_Summary"));
                SetRef(logic, "_actionLabel", FindComponent<TMP_Text>(root, "Txt_ActionLabel"));
                SetRef(logic, "_actionButton", FindComponent<Button>(root, "Btn_Action"));
                SetRef(logic, "_deleteButton", FindComponent<Button>(root, "Btn_Delete"));
            });
        }


        private static string CreateCarryAvailableItem()
        {
            Everlight.Tales.UI.Editor.SpecialItemPrefabMigration.Carry();
            return ItemDir + "/CarryAvailableItem.prefab";
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
// Codex review menu registration touch
