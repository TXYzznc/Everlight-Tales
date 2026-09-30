#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Everlight.Tales.UI;

namespace Everlight.Tales.Editor
{
    /// <summary>
    /// Everlight UI 验收台：集中管理验证数据、页面打开、截图、20 页烟雾测试与 Prefab 审计。
    /// 这是 Editor 工具，不参与运行时构建。
    /// </summary>
    public sealed class EverlightUiValidationWindow : EditorWindow
    {
        private const string WindowTitle = "Everlight UI 验收台";
        private static readonly string[] Pages =
        {
            "MainPageShell", "DialogView", "BoardPage", "PreparationPage", "SettlementPage",
            "Settings", "SaveSlotPage", "MapPage", "HomePage", "JournalPage", "WorkbenchPage",
            "CodexPage", "ArchivePage", "GuestPage", "ServicePage", "DialoguePage",
            "ProloguePage", "InvestigationPage", "RecoveryPage", "FeedbackPage"
        };

        private int _selectedPage;
        private int _width = 1080;
        private int _height = 1920;
        private string _folder = "Assets/Screenshots/AllPages";
        private bool _capture = true;
        private bool _showAdvanced;
        private Vector2 _scroll;
        private string _lastAction = "等待操作";

        [MenuItem("Game Framework/EverlightTales/UI/打开 UI 验收台", priority = 1980)]
        [MenuItem("EverlightTales/工具箱/UI 验收台", priority = 100)]
        public static void Open()
        {
            EverlightUiValidationWindow window = GetWindow<EverlightUiValidationWindow>(WindowTitle);
            window.minSize = new Vector2(440f, 600f);
            window.Show();
        }

        private void OnEnable()
        {
            _width = UIValidationHarness.CaptureWidth;
            _height = UIValidationHarness.CaptureHeight;
            _folder = UIValidationHarness.CaptureFolder;
            _capture = UIValidationHarness.CaptureEnabled;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            Repaint();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(WindowTitle, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("用验证数据打开页面，直接在 1080×1920 画布中检查正式资源、动态 Item 和文字布局。所有操作只作用于 Editor PlayMode。", MessageType.Info);

            DrawCaptureSettings();
            DrawPagePicker();
            DrawRuntimeActions();
            DrawAuditActions();
            DrawReports();
            EditorGUILayout.EndScrollView();
        }

        private void DrawCaptureSettings()
        {
            EditorGUILayout.LabelField("截图与运行参数", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _width = EditorGUILayout.IntField("宽", _width);
                    _height = EditorGUILayout.IntField("高", _height);
                }
                _folder = EditorGUILayout.TextField("截图目录", _folder);
                _capture = EditorGUILayout.ToggleLeft("运行时自动截图", _capture);
                if (GUILayout.Button("保存参数")) SaveSettings();
            }
        }

        private void DrawPagePicker()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("验收页面", EditorStyles.boldLabel);
            _selectedPage = GUILayout.SelectionGrid(_selectedPage, Pages, 4, EditorStyles.miniButton, GUILayout.MinHeight(96f));
            EditorGUILayout.LabelField("当前页面: " + Pages[_selectedPage], EditorStyles.miniLabel);
        }

        private void DrawRuntimeActions()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("运行验证", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("打开当前页面", GUILayout.Height(32f))) RunSelectedPage();
                if (GUILayout.Button("截图当前页面", GUILayout.Height(32f))) CaptureCurrentPage();
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("20 页烟雾测试", GUILayout.Height(32f))) RunSmokeTest();
                if (GUILayout.Button("停止并关闭验证", GUILayout.Height(32f))) StopValidation();
            }
            EditorGUILayout.LabelField(_lastAction, EditorStyles.miniLabel);
        }

        private void DrawAuditActions()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("资源与结构审计", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("审计 20 个 Prefab")) RunPrefabAudit();
                if (GUILayout.Button("验证主页路由")) RunMainShellRoutingCheck();
            }
            _showAdvanced = EditorGUILayout.Foldout(_showAdvanced, "高级工具菜单", true);
            if (_showAdvanced)
            {
                EditorGUILayout.HelpBox("正式资源接入和 Item 生成仍由 UI 菜单中的现有生成器负责。本窗口只负责验收与报告，不会自动重建 Prefab。", MessageType.None);
                if (GUILayout.Button("打开 UI 生成器菜单说明"))
                    EditorUtility.DisplayDialog("UI 生成器", "请使用 Game Framework/EverlightTales/UI 下的生成与接入菜单。", "确定");
            }
        }

        private void DrawReports()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("报告与资产", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("打开烟雾报告")) OpenFile("Library/UIAllPagesSmokeTest.txt");
                if (GUILayout.Button("打开 Prefab 审计")) OpenFile("Library/AllUiPrefabAudit.txt");
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("打开截图目录")) EditorUtility.RevealInFinder(Path.GetFullPath(_folder));
                if (GUILayout.Button("打开 Item 目录")) EditorUtility.RevealInFinder(Path.GetFullPath("Assets/Game/Prefabs/UI/Item"));
            }
            int itemCount = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/Prefabs/UI/Item" }).Length;
            EditorGUILayout.LabelField("当前 Item Prefab 数量: " + itemCount);
        }

        private void SaveSettings()
        {
            _width = Mathf.Clamp(_width, 320, 7680);
            _height = Mathf.Clamp(_height, 320, 7680);
            UIValidationHarness.ConfigureCapture(_width, _height, _folder, _capture);
            _lastAction = "参数已保存: " + _width + "×" + _height;
        }

        private void RunSelectedPage()
        {
            SaveSettings();
            UIValidationHarness.Configure(Pages[_selectedPage]);
            _lastAction = "已配置 " + Pages[_selectedPage] + "，正在进入 PlayMode";
            if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
            else UIValidationHarness.Start();
        }

        private void CaptureCurrentPage()
        {
            SaveSettings();
            if (!EditorApplication.isPlaying)
            {
                _lastAction = "请先打开当前页面并进入 PlayMode";
                return;
            }
            UIValidationHarness.CaptureCurrentScreenshot(Pages[_selectedPage]);
            _lastAction = "已请求当前页面截图";
        }

        private void RunSmokeTest()
        {
            SaveSettings();
            UIValidationHarness.ConfigureSmokeTest(_width, _height, _folder);
            _lastAction = "已配置 20 页烟雾测试";
            if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
            else UIValidationHarness.StartAllPagesSmokeTest();
        }

        private void StopValidation()
        {
            UIValidationHarness.Disable();
            _lastAction = "验证开关已关闭";
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        }

        private static void RunPrefabAudit()
        {
            EverlightUiItemPrefabGenerator.NormalizeAllUiPrefabs();
        }

        [MenuItem("Game Framework/EverlightTales/UI/验证主页路由", priority = 2026)]
        public static void RunMainShellRoutingCheck()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("需要 PlayMode", "请先点击“打开当前页面”进入 MainPageShell，再执行路由验证。", "确定");
                return;
            }

            MonoBehaviour shell = null;
            foreach (MonoBehaviour candidate in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
            {
                if (candidate != null && candidate.GetType().Name == "MainPageShell" && candidate.gameObject.scene.IsValid()) { shell = candidate; break; }
            }
            if (shell == null)
            {
                EditorUtility.DisplayDialog("未找到 MainPageShell", "当前 PlayMode 场景没有已打开的 MainPageShell。", "确定");
                return;
            }

            var report = new List<string> { "playing=true", "shell=" + shell.name };
            Button homeTab = FindDescendant(shell.transform, "Tab_1")?.GetComponent<Button>();
            if (homeTab != null) homeTab.onClick.Invoke();
            Transform map = FindDescendant(shell.transform, "Panel_Map");
            Transform journal = FindDescendant(shell.transform, "Panel_Journal");
            Transform home = FindDescendant(shell.transform, "Panel_Home");
            report.Add("afterJournalTab;mapActive=" + IsActive(map) + ";journalActive=" + IsActive(journal) + ";homeActive=" + IsActive(home));

            Transform top = journal == null ? null : journal.Find("Panel_JournalTop");
            FieldInfo subTab = typeof(JournalPanel).GetField("m_SubTab", BindingFlags.Instance | BindingFlags.NonPublic);
            JournalPanel journalPanel = journal == null ? null : journal.GetComponent<JournalPanel>();
            for (int i = 0; i < 3; i++)
            {
                Button button = top == null ? null : top.Find("Btn_Sub_" + i)?.GetComponent<Button>();
                if (button != null) button.onClick.Invoke();
                object value = journalPanel == null || subTab == null ? "missing" : subTab.GetValue(journalPanel);
                report.Add("journalButton=" + i + ";listenerInvoked=" + (button != null) + ";subTab=" + value);
            }

            // 重复执行“任务 → 家园 → 任务 → 地图 → 任务”，覆盖 GF 面板复用与子面板显隐生命周期。
            Button mapTab = FindDescendant(shell.transform, "Tab_0")?.GetComponent<Button>();
            Button journalTab = FindDescendant(shell.transform, "Tab_1")?.GetComponent<Button>();
            Button homeTabAgain = FindDescendant(shell.transform, "Tab_2")?.GetComponent<Button>();
            for (int cycle = 0; cycle < 3; cycle++)
            {
                report.Add("cycle=" + cycle + ";beforeJournal"); WriteRoutingCheckpoint(report);
                journalTab?.onClick.Invoke();
                report.Add("cycle=" + cycle + ";afterJournal"); WriteRoutingCheckpoint(report);
                homeTabAgain?.onClick.Invoke();
                report.Add("cycle=" + cycle + ";afterHome"); WriteRoutingCheckpoint(report);
                journalTab?.onClick.Invoke();
                report.Add("cycle=" + cycle + ";afterJournalAgain"); WriteRoutingCheckpoint(report);
                mapTab?.onClick.Invoke();
                report.Add("cycle=" + cycle + ";afterMap"); WriteRoutingCheckpoint(report);
                journalTab?.onClick.Invoke();
                report.Add("cycle=" + cycle + ";mapActive=" + IsActive(map) + ";journalActive=" + IsActive(journal) + ";homeActive=" + IsActive(home));
                WriteRoutingCheckpoint(report);
            }

            WriteRoutingCheckpoint(report);
            Debug.Log("[EverlightUiValidationWindow] MainPageShell 路由验证完成。");
        }

        private static void WriteRoutingCheckpoint(List<string> report)
        {
            File.WriteAllLines("Library/UIMainPageShellRouting.txt", report);
        }

        private static bool IsActive(Transform node)
        {
            return node != null && node.gameObject.activeSelf && node.gameObject.activeInHierarchy;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform hit = FindDescendant(root.GetChild(i), name);
                if (hit != null) return hit;
            }
            return null;
        }

        private static void OpenFile(string path)
        {
            if (File.Exists(path)) EditorUtility.OpenWithDefaultApp(path);
            else EditorUtility.DisplayDialog("报告不存在", path, "确定");
        }
    }
}
#endif
