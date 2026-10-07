#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>一次仅针对一个明确指定的页面。截图与布局快照分开触发，不遍历页面。</summary>
    public static class UiSinglePageReview
    {
        private static string Page => File.ReadAllText("Library/UiReviewPage.txt").Trim();

        [MenuItem("Game Framework/EverlightTales/UI/逐页复核/补回 HomePage 按钮文字")]
        public static void RepairHomeButtonLabels()
        {
            RepairButtonLabels("HomePage", new[]
            {
                "Panel_Home/Panel_HomeTop/Btn_Zone_0",
                "Panel_Home/Panel_HomeTop/Btn_Zone_2", "Panel_Home/Panel_HomeTop/Btn_Zone_3",
                "Panel_Home/Panel_HomeTop/Btn_Zone_4",
                "Panel_Home/Panel_HomeArea/Panel_Workbench/Panel_WorkbenchTop/Btn_Sub_0",
                "Panel_Home/Panel_HomeArea/Panel_Workbench/Panel_WorkbenchTop/Btn_Sub_1",
                "Panel_Home/Panel_HomeArea/Panel_Workbench/Panel_WorkbenchTop/Btn_Sub_2",
                "Panel_Home/Panel_HomeArea/Panel_Codex/Panel_CodexTop/Btn_Sub_0",
                "Panel_Home/Panel_HomeArea/Panel_Codex/Panel_CodexTop/Btn_Sub_1",
                "Panel_Home/Panel_HomeArea/Panel_Codex/Panel_CodexTop/Btn_Sub_2"
            }, new[] { "来客", "加工", "收藏", "保管", "服务", "加工", "材料", "账目", "零件", "形态", "怪谈" });
        }

        [MenuItem("Game Framework/EverlightTales/UI/逐页复核/补回 MapPage 按钮文字")]
        public static void RepairMapButtonLabels()
        {
            RepairButtonLabels("MapPage", new[]
            {
                "Panel_LocationConfirm/Btn_Confirm", "Panel_LocationConfirm/Btn_Cancel"
            }, new[] { "确认", "取消" });
        }

        [MenuItem("Game Framework/EverlightTales/UI/逐页复核/补回 JournalPage 按钮文字")]
        public static void RepairJournalButtonLabels()
        {
            RepairButtonLabels("JournalPage", new[]
            {
                "Panel_Journal/Panel_JournalTop/Btn_Sub_0",
                "Panel_Journal/Panel_JournalTop/Btn_Sub_1",
                "Panel_Journal/Panel_JournalTop/Btn_Sub_2"
            }, new[] { "事件", "任务", "怪谈" });
        }

        [MenuItem("Game Framework/EverlightTales/UI/逐页复核/补回 WorkbenchPage 按钮文字")]
        public static void RepairWorkbenchButtonLabels()
        {
            RepairButtonLabels("WorkbenchPage", new[]
            {
                "Panel_Workbench/Panel_WorkbenchTop/Btn_Sub_0",
                "Panel_Workbench/Panel_WorkbenchTop/Btn_Sub_1",
                "Panel_Workbench/Panel_WorkbenchTop/Btn_Sub_2"
            }, new[] { "加工", "材料", "账目" });
        }

        // 每个入口仅修改一个页面中明确列出的缺失标签，不重建页面或覆盖已有标签。
        private static void RepairButtonLabels(string page, string[] buttonPaths, string[] captions)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请先停止 Play Mode。");
            if (buttonPaths.Length != captions.Length) throw new ArgumentException("按钮路径与标签数量不一致。");
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null) throw new InvalidOperationException("TMP 默认字体缺失，无法修复按钮文字。");
            string assetPath = "Assets/Game/Prefabs/UI/" + page + ".prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(assetPath);
            int added = 0;
            try
            {
                for (int i = 0; i < buttonPaths.Length; i++)
                {
                    Transform button = root.transform.Find(buttonPaths[i]);
                    if (button == null || button.GetComponent<Button>() == null)
                        throw new InvalidOperationException(page + " 缺少按钮 " + buttonPaths[i]);
                    if (button.GetComponentInChildren<TMP_Text>(true) != null ||
                        button.GetComponentInChildren<Text>(true) != null) continue;
                    var labelObject = new GameObject("Txt_Label", typeof(RectTransform));
                    labelObject.layer = button.gameObject.layer;
                    labelObject.transform.SetParent(button, false);
                    var rect = labelObject.GetComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.offsetMin = new Vector2(6f, 4f);
                    rect.offsetMax = new Vector2(-6f, -4f);
                    var label = labelObject.AddComponent<TextMeshProUGUI>();
                    label.font = font;
                    label.text = captions[i];
                    label.fontSize = 24f;
                    label.alignment = TextAlignmentOptions.Center;
                    label.color = Color.white;
                    label.raycastTarget = false;
                    added++;
                }
                // 包含 inactive 子面板，避免仅检查默认页签而遗漏嵌套页签。
                foreach (Button button in root.GetComponentsInChildren<Button>(true))
                {
                    TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                    Text legacyLabel = button.GetComponentInChildren<Text>(true);
                    if (label == null && legacyLabel == null)
                        throw new InvalidOperationException(page + " 仍有无文字按钮：" + button.name);
                    if (label != null && label.font == null)
                        throw new InvalidOperationException(page + " 按钮文字字体缺失：" + button.name);
                }
                if (added > 0) PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                Debug.Log("[UiSinglePageReview] " + page + " 补回按钮文字 " + added + " 个，全部按钮文字检查通过。");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [MenuItem("Game Framework/EverlightTales/UI/逐页复核/准备指定页面")]
        public static void Prepare()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请先停止当前页面。");
            string page = Page;
            if (!File.Exists("Assets/Game/Prefabs/UI/" + page + ".prefab")) throw new ArgumentException(page);
            UIValidationHarness.Disable();
            UIValidationHarness.Configure(page);
            UIValidationHarness.ConfigureCapture(1080, 1920, "Library/UiVisualReview", true);
            Debug.Log("[UiSinglePageReview] 已准备单页 " + page);
        }

        [MenuItem("Game Framework/EverlightTales/UI/逐页复核/截图当前页面")]
        public static void Capture()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("需要 Play Mode。");
            UIValidationHarness.CaptureCurrentScreenshot(Page);
            var report = new StringBuilder();
            foreach (TMP_Text text in Resources.FindObjectsOfTypeAll<TMP_Text>())
            {
                if (!text.gameObject.scene.IsValid() || !text.gameObject.activeInHierarchy) continue;
                report.AppendLine(text.transform.GetHierarchyPath() + " | " + text.text + " | " + text.alignment + " | " + text.rectTransform.rect);
            }
            Directory.CreateDirectory("Library/UiVisualReview");
            File.WriteAllText("Library/UiVisualReview/" + Page + ".txt", report.ToString());
        }

        [MenuItem("Game Framework/EverlightTales/UI/逐页复核/修复 BoardPage")]
        public static void RepairBoard()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请先停止当前页面。");
            const string path = "Assets/Game/Prefabs/UI/BoardPage.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                root.transform.Find("Txt_HudRound").gameObject.SetActive(false);
                PositionText(root, "Txt_EventTitle", 40, -52, 760, 64, 40);
                PositionText(root, "Txt_HudRoundTap", 40, -136, 600, 48, 28);
                PositionText(root, "Txt_HudScore", 700, -136, 260, 48, 28);
                PositionText(root, "Txt_HudGoals", 40, -200, 960, 100, 26);
                PositionText(root, "Txt_HudArm", 700, -1770, 300, 48, 26);
                foreach (Button button in root.GetComponentsInChildren<Button>(true))
                {
                    TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                    if (label == null) continue;
                    label.alignment = TextAlignmentOptions.Center;
                    label.color = Color.white;
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [MenuItem("Game Framework/EverlightTales/UI/逐页复核/修复 Journal 顶部资源栏")]
        public static void RepairJournalTopBar()
        {
            RepairTopBarText("Assets/Game/Prefabs/UI/JournalPage.prefab", "Txt_Balance");
        }

        [MenuItem("Game Framework/EverlightTales/UI/逐页复核/修复 Workbench 顶部资源栏")]
        public static void RepairWorkbenchTopBar()
        {
            RepairTopBarText("Assets/Game/Prefabs/UI/WorkbenchPage.prefab", "Txt_Fee");
        }

        private static void RepairTopBarText(string path, string textName)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请先停止当前页面。");
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                TMP_Text text = null;
                foreach (TMP_Text candidate in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (candidate.name == textName) { text = candidate; break; }
                }
                if (text == null) throw new InvalidOperationException(path + " 缺少 " + textName);
                RectTransform rect = text.rectTransform;
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -48f);
                rect.sizeDelta = new Vector2(rect.sizeDelta.x, 48f);
                text.alignment = TextAlignmentOptions.Right | TextAlignmentOptions.Midline;
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void PositionText(GameObject root, string name, float x, float y, float width, float height, float fontSize)
        {
            var text = root.transform.Find(name).GetComponent<TMP_Text>();
            var rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.TopLeft;
        }

        private static string GetHierarchyPath(this Transform node)
        {
            string path = node.name;
            while (node.parent != null) { node = node.parent; path = node.name + "/" + path; }
            return path;
        }
    }
}
#endif
