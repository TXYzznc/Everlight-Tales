#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>将旧的运行时 UI 布局落实到正式页面预制体。可重复执行，不覆盖已有节点的布局。</summary>
    public static class UiLayoutPrefabMigration
    {
        private const string Root = "Assets/Game/Prefabs/UI/";

        [MenuItem("Game Framework/EverlightTales/UI/补齐静态布局", priority = 2090)]
        public static void Run()
        {
            Edit("WorkbenchPage", AddWorkbench);
            Edit("GuestPage", AddGuest);
            Edit("ServicePage", AddService);
            Edit("ArchivePage", AddArchiveEmptyStates);
            Edit("CodexPage", AddCodexEmptyStates);
            Edit("MapPage", AddMapTimeBar);
            MergeHome("MainPageShell");
            BindBoardChoiceItem();
            AssetDatabase.SaveAssets();
            Debug.Log("[UiLayoutPrefabMigration] 静态布局已补齐。");
        }

        private static void BindBoardChoiceItem()
        {
            string page = Root + "BoardPage.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(page);
            try
            {
                Component form = root.GetComponent("BoardPageForm");
                var item = AssetDatabase.LoadAssetAtPath<GameObject>(ListRowPrefabMigration.RowPath);
                if (form != null && item != null) { SerializedObject so = new SerializedObject(form); so.FindProperty("_formChoiceItemTemplate").objectReferenceValue = item; so.ApplyModifiedPropertiesWithoutUndo(); }
                PrefabUtility.SaveAsPrefabAsset(root, page);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void Edit(string name, Action<GameObject> edit)
        {
            string path = Root + name + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                edit(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void AddWorkbench(GameObject root)
        {
            Transform panel = root.transform.Find("Panel_Workbench/Panel_WorkbenchBody/Panel_Workbench");
            if (panel == null) throw new InvalidOperationException("WorkbenchPage 缺少内部 Panel_Workbench");
            EnsureText(panel, "Txt_Detail", new Vector2(0, -560), new Vector2(-40, 140), 18, TextAlignmentOptions.TopLeft);
            EnsureText(panel, "Txt_ActionHint", new Vector2(0, -720), new Vector2(-40, 40), 18, TextAlignmentOptions.Center);
            EnsureButton(panel, "Btn_Action", "加工", new Vector2(0, -820), new Vector2(-40, 64));
            Transform body = root.transform.Find("Panel_Workbench/Panel_WorkbenchBody");
            EnsureText(body.Find("Panel_Materials"), "EmptyState", new Vector2(0, -18), new Vector2(-40, 52), 26, TextAlignmentOptions.Left).gameObject.SetActive(false);
            EnsureText(body.Find("Panel_Ledger"), "EmptyState", new Vector2(0, -18), new Vector2(-40, 52), 26, TextAlignmentOptions.Left).gameObject.SetActive(false);
        }

        private static void AddGuest(GameObject root)
        {
            Transform content = root.transform.Find("Panel_Guest/Panel_GuestContent");
            if (content == null) throw new InvalidOperationException("GuestPage 缺少 Panel_GuestContent");
            EnsureText(content, "Txt_Title", new Vector2(0, 400), new Vector2(600, 50), 30, TextAlignmentOptions.Center);
            EnsureText(content, "Txt_Thanks", new Vector2(0, 320), new Vector2(720, 40), 26, TextAlignmentOptions.Left);
            EnsureButton(content, "Btn_Claim", "领取感谢", new Vector2(260, 250), new Vector2(220, 56));
            EnsureText(content, "Txt_DelegationHeader", new Vector2(0, 150), new Vector2(720, 36), 24, TextAlignmentOptions.Left);
            EnsureContainer(content, "DelegationContent", new Vector2(0, 0), new Vector2(720, 200));
            EnsureText(content, "Txt_DelegationEmpty", new Vector2(0, 100), new Vector2(720, 40), 22, TextAlignmentOptions.Left);
            EnsureText(content, "Txt_ModHeader", new Vector2(0, -120), new Vector2(720, 36), 24, TextAlignmentOptions.Left);
            EnsureText(content, "Txt_ModHint", new Vector2(0, -170), new Vector2(560, 40), 22, TextAlignmentOptions.Left);
            EnsureButton(content, "Btn_Mod", "任务页", new Vector2(300, -170), new Vector2(180, 52));
        }

        private static void AddService(GameObject root)
        {
            Transform content = root.transform.Find("Panel_Service/Panel_ServiceContent");
            if (content == null) throw new InvalidOperationException("ServicePage 缺少 Panel_ServiceContent");
            EnsureText(content, "Txt_Title", new Vector2(0, 400), new Vector2(600, 50), 30, TextAlignmentOptions.Center);
            EnsureText(content, "Txt_Hint", new Vector2(0, 340), new Vector2(720, 36), 22, TextAlignmentOptions.Center);
            EnsureButton(content, "Btn_Loadout", "配装（携带选择）", new Vector2(0, 260), new Vector2(420, 64));
            EnsureButton(content, "Btn_Recover", "恢复面板", new Vector2(0, 180), new Vector2(420, 64));
            EnsureButton(content, "Btn_Settings", "设置", new Vector2(0, 100), new Vector2(420, 64));
        }

        private static void AddArchiveEmptyStates(GameObject root)
        {
            Transform list = root.transform.Find("Panel_Archive/Panel_ArchiveList");
            foreach (string name in new[] { "Content_Display", "Content_Owned", "Content_Material", "Content_Blueprint", "Content_Summary" })
            {
                Transform content = list != null ? list.Find(name) : null;
                if (content == null) throw new InvalidOperationException("ArchivePage 缺少 " + name);
                EnsureText(content, "EmptyState", new Vector2(0, -18), new Vector2(-40, 64), 24, TextAlignmentOptions.Center).gameObject.SetActive(false);
            }
        }

        private static void AddCodexEmptyStates(GameObject root)
        {
            Transform list = root.transform.Find("Panel_Codex/Panel_CodexList");
            foreach (string name in new[] { "PartScroll", "FormScroll", "CaseScroll" })
            {
                Transform content = list != null ? list.Find(name) : null;
                if (content == null) throw new InvalidOperationException("CodexPage 缺少 " + name);
                EnsureText(content, "EmptyState", new Vector2(0, -18), new Vector2(-40, 64), 24, TextAlignmentOptions.Center).gameObject.SetActive(false);
            }
        }

        private static void AddMapTimeBar(GameObject root)
        {
            GameObject main = PrefabUtility.LoadPrefabContents(Root + "MainPageShell.prefab");
            try
            {
                Transform source = main.transform.Find("SafeArea/Content/Panel_Map/Panel_TimeBar");
                Transform target = root.transform.Find("Panel_Map");
                if (source == null || target == null) throw new InvalidOperationException("地图时段条源节点或目标节点缺失");
                if (target.Find("Panel_TimeBar") == null) UnityEngine.Object.Instantiate(source.gameObject, target).name = "Panel_TimeBar";
            }
            finally { PrefabUtility.UnloadPrefabContents(main); }
        }

        private static void MergeHome(string targetName)
        {
            Edit(targetName, targetRoot =>
            {
                Transform home = targetName == "HomePage"
                    ? targetRoot.transform.Find("Panel_Home")
                    : targetRoot.transform.Find("SafeArea/Content/Panel_Home");
                if (home == null) throw new InvalidOperationException(targetName + " 缺少 Panel_Home");
                Transform area = home.Find("Panel_HomeArea");
                foreach (string panelName in new[] { "Panel_Workbench", "Panel_Codex", "Panel_Archive", "Panel_Guest", "Panel_Service" })
                {
                    string sourceName = panelName.Substring("Panel_".Length) + "Page";
                    GameObject sourceRoot = PrefabUtility.LoadPrefabContents(Root + sourceName + ".prefab");
                    try
                    {
                        Transform sourcePanel = sourceRoot.transform.Find(panelName);
                        Transform targetPanel = area != null ? area.Find(panelName) : null;
                        if (sourcePanel == null || targetPanel == null) throw new InvalidOperationException(targetName + " 缺少 " + panelName);
                        MergeChildren(sourcePanel, targetPanel);
                        CopyPanelScript(sourcePanel, targetPanel);
                    }
                    finally { PrefabUtility.UnloadPrefabContents(sourceRoot); }
                }
                if (targetName == "MainPageShell")
                {
                    GameObject mapRoot = PrefabUtility.LoadPrefabContents(Root + "MapPage.prefab");
                    try
                    {
                        Transform sourceMap = mapRoot.transform.Find("Panel_Map");
                        Transform targetMap = targetRoot.transform.Find("SafeArea/Content/Panel_Map");
                        CopyPanelScript(sourceMap, targetMap);
                    }
                    finally { PrefabUtility.UnloadPrefabContents(mapRoot); }
                }
            });
        }

        private static void MergeChildren(Transform source, Transform target)
        {
            foreach (Transform child in source)
            {
                Transform existing = target.Find(child.name);
                if (existing == null)
                {
                    UnityEngine.Object.Instantiate(child.gameObject, target).name = child.name;
                }
                else MergeChildren(child, existing);
            }
        }

        private static void CopyPanelScript(Transform source, Transform target)
        {
            foreach (MonoBehaviour component in source.GetComponents<MonoBehaviour>())
            {
                Type type = component.GetType();
                if (type.Namespace != "Everlight.Tales.UI") continue;
                MonoBehaviour dest = target.GetComponent(type) as MonoBehaviour;
                if (dest == null) dest = target.gameObject.AddComponent(type) as MonoBehaviour;
                if (dest != null) EditorUtility.CopySerialized(component, dest);
            }
        }

        private static RectTransform EnsureContainer(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing as RectTransform;
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            return rect;
        }

        private static TextMeshProUGUI EnsureText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, TextAlignmentOptions align)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<TextMeshProUGUI>();
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Button EnsureButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.GetComponent<Button>();
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.55f, 1f);
            Button button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.SpriteSwap;
            TextMeshProUGUI text = EnsureText(go.transform, "label", Vector2.zero, size, 24, TextAlignmentOptions.Center);
            text.text = label;
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }
    }
}
#endif
