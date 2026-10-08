#if UNITY_EDITOR
using System;
using System.Linq;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    public static class ListRowPrefabMigration
    {
        public const string RowPath = "Assets/Game/Prefabs/UI/Item/ListRowItem.prefab";
        private static readonly string[] OldRows = {
            "ArchiveDisplayItem", "ArchiveOwnedItem", "ArchiveMaterialItem", "ArchiveBlueprintItem", "ArchiveSummaryItem",
            "CodexPartItem", "CodexFormItem", "CodexCaseItem", "WorkbenchFormItem", "WorkbenchMaterialItem", "WorkbenchLedgerItem",
            "GuestDelegationItem", "JournalItem", "MapEventItem", "DialogueChoiceItem", "RewardChoiceItem"
        };

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/创建通用预制体")]
        public static void CreatePrefab()
        {
            StopRequired();
            if (AssetDatabase.LoadAssetAtPath<GameObject>(RowPath) != null)
            {
                GameObject existing = PrefabUtility.LoadPrefabContents(RowPath);
                try { FormalResourceMigration.UpgradeRow(existing.GetComponent<ListRowItem>()); PrefabUtility.SaveAsPrefabAsset(existing, RowPath); }
                finally { PrefabUtility.UnloadPrefabContents(existing); }
                return;
            }
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/Font/Common/SIMHEI SDF.asset");
            if (font == null) throw new InvalidOperationException("缺少正式 UI 字体 SIMHEI SDF。");
            GameObject root = Rect("ListRowItem", null);
            try
            {
                RectTransform rect = (RectTransform)root.transform;
                rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 1f); rect.sizeDelta = new Vector2(-24f, 64f);
                Image background = root.AddComponent<Image>();
                background.sprite = Sprite("SHR-005-normal列表行"); background.type = Image.Type.Sliced;
                Button button = root.AddComponent<Button>(); button.targetGraphic = background;
                ConfigureButton(button);
                root.AddComponent(AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("PersistentButtonVisualState")).First(t => t != null));
                LayoutElement height = root.AddComponent<LayoutElement>(); height.minHeight = height.preferredHeight = 64f;
                HorizontalLayoutGroup row = root.AddComponent<HorizontalLayoutGroup>();
                row.padding = new RectOffset(24, 20, 8, 8); row.spacing = 12f;
                row.childAlignment = TextAnchor.MiddleLeft;
                row.childControlWidth = row.childControlHeight = true;
                row.childForceExpandWidth = row.childForceExpandHeight = false;
                TMP_Text id = Text("Txt_Id", root.transform, font, 16f); Size(id.gameObject, 68f, -1f);
                GameObject iconRoot = Rect("IconRoot", root.transform); Size(iconRoot, 40f, 40f);
                Image icon = ImageChild("Icon", iconRoot.transform), frame = ImageChild("StateFrame", iconRoot.transform);
                GameObject textColumn = Rect("TextColumn", root.transform);
                LayoutElement textWidth = textColumn.AddComponent<LayoutElement>(); textWidth.minWidth = 0; textWidth.flexibleWidth = 1;
                VerticalLayoutGroup textLayout = textColumn.AddComponent<VerticalLayoutGroup>();
                textLayout.spacing = 2; textLayout.childAlignment = TextAnchor.MiddleLeft;
                textLayout.childControlWidth = textLayout.childControlHeight = true;
                textLayout.childForceExpandWidth = true; textLayout.childForceExpandHeight = false;
                TMP_Text title = Text("Txt_Title", textColumn.transform, font, 22); Size(title.gameObject, -1, 30);
                TMP_Text detail = Text("Txt_Detail", textColumn.transform, font, 16); Size(detail.gameObject, -1, 24);
                TMP_Text right = Text("Txt_Right", root.transform, font, 18); right.alignment = TextAlignmentOptions.Right; Size(right.gameObject, 160, -1);
                GameObject actionRoot = Rect("Action", root.transform); Size(actionRoot, 140, 44);
                Image actionImage = actionRoot.AddComponent<Image>(); actionImage.sprite = background.sprite; actionImage.type = Image.Type.Sliced;
                Button action = actionRoot.AddComponent<Button>(); action.targetGraphic = actionImage; ConfigureButton(action);
                TMP_Text actionText = Text("Label", actionRoot.transform, font, 18); Stretch((RectTransform)actionText.transform); actionText.alignment = TextAlignmentOptions.Center;
                Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Everlight.Tales.UI.ListRowItem")).First(t => t != null);
                Component logic = root.AddComponent(type);
                SerializedObject so = new SerializedObject(logic);
                Ref(so, "_button", button); Ref(so, "_actionButton", action);
                Ref(so, "_title", title); Ref(so, "_detail", detail); Ref(so, "_id", id); Ref(so, "_rightText", right); Ref(so, "_actionText", actionText);
                Ref(so, "_icon", icon); Ref(so, "_stateFrame", frame); Ref(so, "_iconRoot", iconRoot.transform); Ref(so, "_height", height); Ref(so, "_rect", rect);
                so.ApplyModifiedPropertiesWithoutUndo();
                id.gameObject.SetActive(false); iconRoot.SetActive(false); detail.gameObject.SetActive(false); right.gameObject.SetActive(false); actionRoot.SetActive(false);
                FormalResourceMigration.UpgradeRow(root.GetComponent<ListRowItem>());
                PrefabUtility.SaveAsPrefabAsset(root, RowPath);
                Debug.Log("[ListRow][Asset] 通用 SHR-005 列表行创建完成。");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
            AssetDatabase.SaveAssets();
        }

        public static void BindPage(string path)
        {
            StopRequired();
            GameObject row = AssetDatabase.LoadAssetAtPath<GameObject>(RowPath);
            if (row == null) throw new InvalidOperationException("请先创建 ListRowItem。");
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                int count = 0;
                foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (component == null) continue;
                    SerializedObject so = new SerializedObject(component);
                    string field = component.GetType().Name == "CodexPanel" || component.GetType().Name == "ArchivePanel" || component.GetType().Name == "WorkbenchPanel" ? "_rowItemTemplate"
                        : component.GetType().Name == "GuestPanel" ? "_delegationItemTemplate" : null;
                    // 旧快照可能保留已失效 fileID，无法通过 objectReferenceValue 读取旧对象。
                    if (field != null)
                    {
                        SerializedProperty template = so.FindProperty(field);
                        if (template != null && template.objectReferenceValue != row) { template.objectReferenceValue = row; count++; }
                    }
                    SerializedProperty prop = so.GetIterator();
                    while (prop.Next(true))
                    {
                        if (prop.propertyType != SerializedPropertyType.ObjectReference) continue;
                        GameObject old = prop.objectReferenceValue as GameObject;
                        if (old == null || !OldRows.Contains(old.name) || !AssetDatabase.Contains(old)) continue;
                        prop.objectReferenceValue = row; count++;
                    }
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log("[ListRow][Bind] " + path + " templateReferences=" + count);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/验证复用与回收")]
        public static void ValidateRuntime()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Everlight.Tales.UI.ListRowRuntimeValidation")).First(t => t != null);
            type.GetMethod("Run").Invoke(null, new object[] { AssetDatabase.LoadAssetAtPath<GameObject>(RowPath) });
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/准备 Archive 验收")]
        public static void PrepareArchive()
        {
            UIValidationHarness.Configure("ArchivePage");
            UIValidationHarness.ConfigureCapture(1080, 1920, "Library/ListRowValidation", false);
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/验证 Archive")]
        public static void ValidateArchive()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Everlight.Tales.UI.ListRowRuntimeValidation")).First(t => t != null);
            type.GetMethod("RunArchive").Invoke(null, null);
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/准备 Codex 验收")]
        public static void PrepareCodex() { UIValidationHarness.Configure("CodexPage"); UIValidationHarness.ConfigureCapture(1080, 1920, "Library/ListRowValidation", false); }
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/验证 Codex")]
        public static void ValidateCodex()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Everlight.Tales.UI.ListRowRuntimeValidation")).First(t => t != null);
            type.GetMethod("RunCodex").Invoke(null, null);
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/准备 Workbench 验收")]
        public static void PrepareWorkbench() { UIValidationHarness.Configure("WorkbenchPage"); UIValidationHarness.ConfigureCapture(1080, 1920, "Library/ListRowValidation", false); }
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/验证 Workbench")]
        public static void ValidateWorkbench()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Everlight.Tales.UI.ListRowRuntimeValidation")).First(t => t != null);
            type.GetMethod("RunWorkbench").Invoke(null, null);
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/准备 Guest 验收")]
        public static void PrepareGuest() { UIValidationHarness.Configure("GuestPage"); UIValidationHarness.ConfigureCapture(1080, 1920, "Library/ListRowValidation", false); }
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/验证 Guest")]
        public static void ValidateGuest()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Everlight.Tales.UI.ListRowRuntimeValidation")).First(t => t != null);
            type.GetMethod("RunGuest").Invoke(null, null);
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/准备 Journal 验收")]
        public static void PrepareJournal() { UIValidationHarness.Configure("JournalPage"); UIValidationHarness.ConfigureCapture(1080, 1920, "Library/ListRowValidation", false); }
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/验证 Journal")]
        public static void ValidateJournal()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Everlight.Tales.UI.ListRowRuntimeValidation")).First(t => t != null);
            type.GetMethod("RunJournal").Invoke(null, null);
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/准备 Map 验收")]
        public static void PrepareMap() { UIValidationHarness.Configure("MapPage"); UIValidationHarness.ConfigureCapture(1080, 1920, "Library/ListRowValidation", false); }
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/验证 Map")]
        public static void ValidateMap()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Everlight.Tales.UI.ListRowRuntimeValidation")).First(t => t != null);
            type.GetMethod("RunMap").Invoke(null, null);
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/准备 Choices 验收")]
        public static void PrepareChoices() { UIValidationHarness.Configure("BoardPage"); UIValidationHarness.ConfigureCapture(1080, 1920, "Library/ListRowValidation", false); }
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/验证 Choices")]
        public static void ValidateChoices() => InvokeValidation("RunChoices");
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/打开 Reward 验收")]
        public static void PrepareReward() => InvokeValidation("PrepareReward");
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/验证 Reward")]
        public static void ValidateReward() => InvokeValidation("RunReward");
        private static void InvokeValidation(string method)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Everlight.Tales.UI.ListRowRuntimeValidation")).First(t => t != null);
            type.GetMethod(method).Invoke(null, null);
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/绑定 ArchivePage")]
        public static void Archive() => BindPage("Assets/Game/Prefabs/UI/ArchivePage.prefab");
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/绑定 CodexPage")]
        public static void Codex() => BindPage("Assets/Game/Prefabs/UI/CodexPage.prefab");
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/绑定 WorkbenchPage")]
        public static void Workbench() => BindPage("Assets/Game/Prefabs/UI/WorkbenchPage.prefab");
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/绑定 GuestPage")]
        public static void Guest() => BindPage("Assets/Game/Prefabs/UI/GuestPage.prefab");
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/绑定 JournalPage")]
        public static void Journal() => BindPage("Assets/Game/Prefabs/UI/JournalPage.prefab");
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/绑定 MapPage")]
        public static void Map() => BindPage("Assets/Game/Prefabs/UI/MapPage.prefab");
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/绑定 BoardPage")]
        public static void Board() => BindPage("Assets/Game/Prefabs/UI/BoardPage.prefab");
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/绑定 SettlementPage")]
        public static void Settlement() => BindPage("Assets/Game/Prefabs/UI/SettlementPage.prefab");

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/迁移 HomePage 快照")]
        public static void Snapshot()
        {
            const string path = "Assets/HomePage(Clone).prefab";
            BindPage(path);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                int removed = 0;
                foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
                    if (component != null && component.GetType().FullName == "Everlight.Tales.UI.GuestDelegationItem")
                    {
                        if (component.name != "GuestDelegationItem(Clone)") throw new InvalidOperationException("快照含手工节点，需单独检查。");
                        UnityEngine.Object.DestroyImmediate(component.gameObject); removed++;
                    }
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log("[ListRow][Snapshot] 更新模板，清除已保存的运行时来客克隆=" + removed);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/审计引用与组件")]
        public static void Audit()
        {
            StopRequired();
            string[] oldPaths = OldRows.Select(n => "Assets/Game/Prefabs/UI/Item/" + n + ".prefab").ToArray();
            string[] assets = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/", StringComparison.Ordinal) &&
                (p.EndsWith(".prefab", StringComparison.Ordinal) || p.EndsWith(".unity", StringComparison.Ordinal) || p.EndsWith(".asset", StringComparison.Ordinal))).ToArray();
            foreach (string path in assets)
            {
                if (oldPaths.Contains(path)) continue;
                string[] dependencies = AssetDatabase.GetDependencies(path, true);
                string old = dependencies.FirstOrDefault(p => oldPaths.Contains(p));
                if (old != null) throw new InvalidOperationException(path + " 仍引用 " + old);
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/Prefabs/UI" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (oldPaths.Contains(path)) continue;
                GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) != 0)
                        throw new InvalidOperationException(path + " 缺少脚本 " + node.name);
            }
            GameObject row = AssetDatabase.LoadAssetAtPath<GameObject>(RowPath);
            Component view = row.GetComponent("ListRowItem");
            if (view == null) throw new InvalidOperationException("通用组件缺失。");
            SerializedObject so = new SerializedObject(view);
            foreach (string field in new[] { "_button", "_actionButton", "_title", "_detail", "_id", "_rightText", "_actionText", "_icon", "_stateFrame", "_iconRoot", "_height", "_rect" })
                if (so.FindProperty(field).objectReferenceValue == null) throw new InvalidOperationException("通用行未绑定 " + field);
            foreach (Button button in row.GetComponentsInChildren<Button>(true))
                if (button.transition != Selectable.Transition.SpriteSwap || button.spriteState.highlightedSprite == null || button.spriteState.selectedSprite == null)
                    throw new InvalidOperationException("通用行图片过渡契约错误。");
            Debug.Log("[ListRow][Audit] PASS assets=" + assets.Length + "; oldPrefabDependencies=0; missingUIScripts=0; serializedFields; SpriteSwap.");
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/清理已迁移旧资源")]
        public static void Cleanup()
        {
            Audit();
            foreach (string name in OldRows)
            {
                string path = "Assets/Game/Prefabs/UI/Item/" + name + ".prefab";
                if (File.Exists(path) && !AssetDatabase.DeleteAsset(path)) throw new InvalidOperationException("无法删除已迁移资源 " + path);
            }
            string[] scripts = { "ArchiveItem", "CodexItem", "GuestDelegationItem", "JournalItem", "MapEventItem", "DialogueChoiceItem", "RewardChoiceItem" };
            foreach (string name in scripts)
            {
                string path = "Assets/Game/Scripts/EverlightTales/UI/" + name + ".cs";
                if (!File.Exists(path)) continue;
                string guid = AssetDatabase.AssetPathToGUID(path);
                foreach (string asset in AssetDatabase.GetAllAssetPaths())
                    if (asset.StartsWith("Assets/", StringComparison.Ordinal) && (asset.EndsWith(".prefab") || asset.EndsWith(".unity") || asset.EndsWith(".asset")) && File.ReadAllText(asset).Contains(guid))
                        throw new InvalidOperationException(asset + " 仍引用旧显示脚本 " + name);
                if (!AssetDatabase.DeleteAsset(path)) throw new InvalidOperationException("无法删除旧显示脚本 " + path);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[ListRow][Cleanup] 完成：16 个旧 Prefab、7 个旧显示脚本；保留宿主共用 WorkbenchItem。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/检查生成器")]
        public static void ValidateFactories()
        {
            StopRequired();
            Type factory = typeof(Everlight.Tales.Editor.EverlightUiItemPrefabGenerator);
            var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
            if ((string)factory.GetMethod("CommonRow", flags).Invoke(null, null) != RowPath)
                throw new InvalidOperationException("生成器未使用通用行。");
            foreach (string name in OldRows)
                if (File.Exists("Assets/Game/Prefabs/UI/Item/" + name + ".prefab")) throw new InvalidOperationException("生成器创建了旧行 " + name);
            Debug.Log("[ListRow][Factories] PASS commonFactory; singlePrefab; noOldAssetsRecreated.");
        }

        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/最终开关页验收")]
        public static void FinalRuntime()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("Everlight.Tales.UI.ListRowFinalValidation")).First(t => t != null);
            type.GetMethod("Run").Invoke(null, null);
        }
        [MenuItem("Game Framework/EverlightTales/UI/通用列表行/关闭验收数据")]
        public static void DisableValidation() => UIValidationHarness.Disable();

        private static void StopRequired() { if (EditorApplication.isPlaying) throw new InvalidOperationException("请先停止 Play Mode。"); }
        private static GameObject Rect(string name, Transform parent) { GameObject go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); go.layer = LayerMask.NameToLayer("UI"); return go; }
        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static Image ImageChild(string name, Transform parent) { GameObject go = Rect(name, parent); Stretch((RectTransform)go.transform); Image image = go.AddComponent<Image>(); image.preserveAspect = true; image.raycastTarget = false; return image; }
        private static TMP_Text Text(string name, Transform parent, TMP_FontAsset font, float size) { GameObject go = Rect(name, parent); TMP_Text text = go.AddComponent<TextMeshProUGUI>(); text.font = font; text.fontSize = size; text.enableWordWrapping = false; text.overflowMode = TextOverflowModes.Ellipsis; text.alignment = TextAlignmentOptions.Left; text.raycastTarget = false; text.text = string.Empty; return text; }
        private static void Size(GameObject go, float width, float height) { LayoutElement element = go.AddComponent<LayoutElement>(); element.preferredWidth = width; element.preferredHeight = height; }
        private static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Sprites/UI/九宫格/" + name + ".png");
        private static void ConfigureButton(Button button) { button.transition = Selectable.Transition.SpriteSwap; button.spriteState = new SpriteState { highlightedSprite = Sprite("SHR-005-hover列表行"), pressedSprite = Sprite("SHR-005-selected列表行"), selectedSprite = Sprite("SHR-005-selected列表行"), disabledSprite = Sprite("SHR-005-normal列表行") }; Navigation nav = button.navigation; nav.mode = Navigation.Mode.None; button.navigation = nav; }
        private static void Ref(SerializedObject so, string field, UnityEngine.Object value) => so.FindProperty(field).objectReferenceValue = value;
    }
}
#endif
