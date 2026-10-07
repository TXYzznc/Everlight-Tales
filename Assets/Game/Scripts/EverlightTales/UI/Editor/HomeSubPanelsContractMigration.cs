#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI.Editor
{
    public static class HomeSubPanelsContractMigration
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/HomePage.prefab";
        private const string CodexPagePath = "Assets/Game/Prefabs/UI/CodexPage.prefab";
        private const string ArchivePagePath = "Assets/Game/Prefabs/UI/ArchivePage.prefab";
        private const string ServicePagePath = "Assets/Game/Prefabs/UI/ServicePage.prefab";

        [MenuItem("Game Framework/EverlightTales/UI/绑定 CodexPage 序列化引用")]
        public static void BindCodexPage() => Run(CodexPagePath, BindCodexImpl, "CodexPanel(CodexPage)");
        [MenuItem("Game Framework/EverlightTales/UI/绑定 ArchivePage 序列化引用")]
        public static void BindArchivePage() => Run(ArchivePagePath, BindArchiveImpl, "ArchivePanel(ArchivePage)");
        [MenuItem("Game Framework/EverlightTales/UI/绑定 ServicePage 序列化引用")]
        public static void BindServicePage() => Run(ServicePagePath, BindServiceImpl, "ServicePanel(ServicePage)");

        [MenuItem("Game Framework/EverlightTales/UI/修复通用列表行按钮")]
        public static void RepairItemButtons()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("请先停止 Play Mode。");
            GameObject root = PrefabUtility.LoadPrefabContents(ListRowPrefabMigration.RowPath);
            try
            {
                foreach (Button button in root.GetComponentsInChildren<Button>(true))
                    ConfigureItemSprites(root, button, "ListRowItem");
                PrefabUtility.SaveAsPrefabAsset(root, ListRowPrefabMigration.RowPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
        }

        public static void ConfigureItemSprites(GameObject root, Button button, string item)
        {
            if (item != "ListRowItem") return;
            Sprite normal = LoadItemSprite("SHR-005-normal列表行");
            Sprite selected = LoadItemSprite("SHR-005-selected列表行");
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = LoadItemSprite("SHR-005-hover列表行"),
                pressedSprite = selected, selectedSprite = selected, disabledSprite = normal
            };
            Image image = button.targetGraphic as Image;
            if (image != null) { image.sprite = normal; image.type = Image.Type.Sliced; image.raycastTarget = true; }
        }

        private static Sprite LoadItemSprite(string name)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Sprites/UI/九宫格/" + name + ".png");
            if (sprite == null) throw new System.InvalidOperationException("缺少列表行图片：" + name);
            return sprite;
        }

        private static void Run(string prefabPath, System.Action<GameObject> action, string label)
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("请先停止 Play Mode。");
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try { action(root); PrefabUtility.SaveAsPrefabAsset(root, prefabPath); Debug.Log("[HomeSubPanelsContractMigration] " + label + " 序列化引用绑定完成。"); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        }

        private static void BindCodexImpl(GameObject root)
        {
            CodexPanel panel = root.GetComponentInChildren<CodexPanel>(true);
            if (panel == null) throw new System.InvalidOperationException("HomePage.prefab 中缺少 CodexPanel。");
            Transform sr = panel.transform.name == "Panel_Codex" ? panel.transform : panel.transform.Find("Panel_Codex");
            Transform top = Find(sr, "Panel_CodexTop"); Transform list = Find(sr, "Panel_CodexList");
            SerializedObject so = new SerializedObject(panel); Set(so, "_staticRoot", sr); Set(so, "_topRoot", top); Set(so, "_listRoot", list);
            Set(so, "_progressLabel", Component<TextMeshProUGUI>(top, "Txt_Progress"));
            SerializedProperty buttons = so.FindProperty("_subButtons"); buttons.arraySize = 3;
            for (int i = 0; i < 3; i++) buttons.GetArrayElementAtIndex(i).objectReferenceValue = Component<Button>(top, "Btn_Sub_" + i);
            RectTransform part = Find(list, "PartScroll") as RectTransform, form = Find(list, "FormScroll") as RectTransform, cases = Find(list, "CaseScroll") as RectTransform;
            Set(so, "_partViewport", part); Set(so, "_formViewport", form); Set(so, "_caseViewport", cases);
            Set(so, "_partRoot", Content(part, "PartContent")); Set(so, "_formRoot", Content(form, "FormContent")); Set(so, "_caseRoot", Content(cases, "CaseContent")); so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindArchiveImpl(GameObject root)
        {
            ArchivePanel panel = root.GetComponentInChildren<ArchivePanel>(true);
            if (panel == null) throw new System.InvalidOperationException("HomePage.prefab 中缺少 ArchivePanel。");
            Transform sr = panel.transform.name == "Panel_Archive" ? panel.transform : panel.transform.Find("Panel_Archive");
            Transform top = Find(sr, "Panel_ArchiveTop"); Transform list = Find(sr, "Panel_ArchiveList");
            SerializedObject so = new SerializedObject(panel); Set(so, "_staticRoot", sr); Set(so, "_topRoot", top); Set(so, "_listRoot", list);
            SerializedProperty buttons = so.FindProperty("_subButtons"); buttons.arraySize = 5;
            for (int i = 0; i < 5; i++) buttons.GetArrayElementAtIndex(i).objectReferenceValue = Component<Button>(top, "Btn_Sub_" + i);
            SerializedProperty views = so.FindProperty("_subViews"); views.arraySize = 5;
            Set(so, "_sectionHeader", Component<TextMeshProUGUI>(list, "Header_Title"));
            SerializedProperty emptyLabels = so.FindProperty("_emptyLabels"); emptyLabels.arraySize = 5;
            string[] names = { "Content_Display", "Content_Owned", "Content_Material", "Content_Blueprint", "Content_Summary" };
            string[] fields = { "_displayRoot", "_ownedRoot", "_materialRoot", "_blueprintRoot", "_summaryRoot" };
            for (int i = 0; i < names.Length; i++)
            {
                Transform view = Find(list, names[i]);
                views.GetArrayElementAtIndex(i).objectReferenceValue = view;
                Set(so, fields[i], Content(view, "Items_" + names[i].Substring("Content_".Length)));
                emptyLabels.GetArrayElementAtIndex(i).objectReferenceValue = Component<TextMeshProUGUI>(view, "EmptyState");
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindServiceImpl(GameObject root)
        {
            ServicePanel panel = root.GetComponentInChildren<ServicePanel>(true);
            if (panel == null) throw new System.InvalidOperationException("HomePage.prefab 中缺少 ServicePanel。");
            Transform sr = panel.transform.name == "Panel_Service" ? panel.transform : panel.transform.Find("Panel_Service"); Transform content = Find(sr, "Panel_ServiceContent");
            SerializedObject so = new SerializedObject(panel); Set(so, "m_Root", content); Set(so, "_title", Component<TMP_Text>(content, "Txt_Title")); Set(so, "_hint", Component<TMP_Text>(content, "Txt_Hint")); Set(so, "_loadout", Component<Button>(content, "Btn_Loadout")); Set(so, "_recover", Component<Button>(content, "Btn_Recover")); Set(so, "_settings", Component<Button>(content, "Btn_Settings")); so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform Find(Transform root, string path) => root == null ? null : root.Find(path);
        private static Transform Content(Transform root, string name) => Find(root, name) ?? root;
        private static T Component<T>(Transform root, string path) where T : Component { Transform target = Find(root, path); return target == null ? null : target.GetComponent<T>(); }
        private static void Set(SerializedObject so, string name, Object value) { SerializedProperty p = so.FindProperty(name); if (p == null) throw new System.InvalidOperationException("缺少序列化字段：" + name); p.objectReferenceValue = value; }
    }
}
#endif
