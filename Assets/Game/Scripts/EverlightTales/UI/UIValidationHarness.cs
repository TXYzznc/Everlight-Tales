using System.Collections.Generic;
using System.IO;
using System.Collections;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 编辑器 PlayMode 验证数据注入器。只在 Unity Editor 且由 PlayerPrefs 开关启用，
    /// 不写入正式存档，进入指定页面后可直接对照完整的动态列表状态。
    /// </summary>
    public static class UIValidationHarness
    {
        private const string EnabledKey = "et.ui.validation.enabled";
        private const string FormKey = "et.ui.validation.form";
        private const string SmokeKey = "et.ui.validation.smoke";
        private const string WidthKey = "et.ui.validation.width";
        private const string HeightKey = "et.ui.validation.height";
        private const string FolderKey = "et.ui.validation.folder";
        private const string CaptureKey = "et.ui.validation.capture";

        public static int CaptureWidth => Mathf.Max(320, PlayerPrefs.GetInt(WidthKey, 1080));
        public static int CaptureHeight => Mathf.Max(320, PlayerPrefs.GetInt(HeightKey, 1920));
        public static string CaptureFolder => PlayerPrefs.GetString(FolderKey, "Assets/Screenshots/AllPages");
        public static bool CaptureEnabled => PlayerPrefs.GetInt(CaptureKey, 1) == 1;

        public static bool Enabled => Application.isEditor && PlayerPrefs.GetInt(EnabledKey, 0) == 1;

        public static void Configure(string formKey)
        {
            PlayerPrefs.SetInt(EnabledKey, 1);
            PlayerPrefs.SetString(FormKey, formKey ?? "ArchivePage");
            PlayerPrefs.Save();
        }

        public static void ConfigureCapture(int width, int height, string folder, bool capture)
        {
            PlayerPrefs.SetInt(WidthKey, Mathf.Clamp(width, 320, 7680));
            PlayerPrefs.SetInt(HeightKey, Mathf.Clamp(height, 320, 7680));
            PlayerPrefs.SetString(FolderKey, string.IsNullOrWhiteSpace(folder) ? "Assets/Screenshots/AllPages" : folder.Trim());
            PlayerPrefs.SetInt(CaptureKey, capture ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void ConfigureSmokeTest(int width, int height, string folder)
        {
            Configure("MainPageShell");
            ConfigureCapture(width, height, folder, true);
            PlayerPrefs.SetInt(SmokeKey, 1);
            PlayerPrefs.Save();
        }

        public static void Disable()
        {
            PlayerPrefs.SetInt(EnabledKey, 0);
            PlayerPrefs.SetInt(SmokeKey, 0);
            PlayerPrefs.Save();
        }

        /// <summary>在 PlayMode 中依次打开 20 个正式 UIForm，记录每页截图，供逐页人工验收。</summary>
        public static void StartAllPagesSmokeTest()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("请先进入 PlayMode，再启动全量 UI 烟雾测试。");
                return;
            }

            GameObject go = new GameObject("EverlightAllPagesSmokeTest");
            go.AddComponent<UIAllPagesSmokeTest>();
        }

        /// <summary>由 HomePageBootstrap 在 Home 场景加载完成后调用。</summary>
        public static void Start()
        {
            PrepareValidationData();

            if (Application.isPlaying)
            {
                Screen.SetResolution(CaptureWidth, CaptureHeight, false);
            }

            string form = PlayerPrefs.GetString(FormKey, "ArchivePage");
            UIViews view;
            if (!System.Enum.TryParse(form, true, out view)) view = UIViews.ArchivePage;
            GF.UI.OpenUIForm(view);
            RefreshOpenPanels();
            Debug.Log("[UIValidationHarness] 已注入验证数据并打开 " + form + "（不会写入正式存档）。");

            if (PlayerPrefs.GetInt(SmokeKey, 0) == 1)
            {
                PlayerPrefs.SetInt(SmokeKey, 0);
                PlayerPrefs.Save();
                StartAllPagesSmokeTest();
            }
        }

        public static void CaptureCurrentScreenshot(string pageName)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[UIValidationHarness] 当前不在 PlayMode，无法截图。");
                return;
            }

            string folder = UIValidationHarness.CaptureFolder.Replace('\\', '/');
            Directory.CreateDirectory(folder);
            string safeName = string.IsNullOrWhiteSpace(pageName) ? "CurrentPage" : pageName.Trim();
            string path = folder.TrimEnd('/') + "/" + safeName + "_" + CaptureWidth + "x" + CaptureHeight + ".png";
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("[UIValidationHarness] 已请求截图: " + path);
        }

        /// <summary>为逐页烟雾测试注入完整演示数据，但不自动打开页面。</summary>
        public static void PrepareValidationData()
        {
            CanvasScaler scaler = Object.FindObjectOfType<CanvasScaler>();
            if (scaler != null)
            {
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 0.5f;
            }
            WorldSession session = WorldSession.NewGame(WorldSession.DemoSeed);
            Populate(session.World);
        }

        public static void RefreshOpenPanels()
        {
            foreach (ArchivePanel panel in Resources.FindObjectsOfTypeAll<ArchivePanel>())
            {
                if (panel.gameObject.scene.IsValid()) { panel.BindStaticLayout(); panel.Build(); panel.Refresh(); }
            }
            foreach (CodexPanel panel in Resources.FindObjectsOfTypeAll<CodexPanel>())
            {
                if (panel.gameObject.scene.IsValid()) { panel.BindStaticLayout(); panel.Build(); panel.Refresh(); }
            }
            foreach (WorkbenchPanel panel in Resources.FindObjectsOfTypeAll<WorkbenchPanel>())
            {
                if (panel.gameObject.scene.IsValid()) { panel.BindStaticLayout(); panel.Build(); panel.Refresh(); File.WriteAllText("Library/UIValidationWorkbenchTabs.txt", "playing=" + Application.isPlaying + "\nresolution=1080x1920\n" + panel.ValidateTabsReport()); }
            }
        }

        private static void Populate(WorldState world)
        {
            world.Day = 8;
            world.RepairFee = 268;
            world.BatchNumber = 3;
            world.OwnedParts.Add(PartType.InertiaHammer);
            world.OwnedParts.Add(PartType.MeteringRatchet);
            world.OwnedParts.Add(PartType.BlastCoil);
            world.OwnedParts.Add(PartType.ReversalGear);
            world.KnownParts.Add(PartType.RivetPliers);
            world.KnownParts.Add(PartType.SpringLauncher);
            foreach (PartType host in FormCatalog.HostParts())
            {
                if (!world.OwnedParts.Contains(host)) world.OwnedParts.Add(host);
            }
            Debug.Log("[UIValidationHarness] OwnedParts=" + world.OwnedParts.Count + ", FormHosts=" + FormCatalog.HostParts().Count);

            int index = 0;
            foreach (MaterialConfig material in MaterialCatalog.All())
            {
                if (index >= 6) break;
                world.Materials.Add(material.Id, 2 + index, index % 2 == 0 ? "CASE-RED-SHOE" : "");
                index++;
            }

            index = 0;
            foreach (FormConfig form in FormCatalog.All())
            {
                if (form.Kind != FormKind.M) continue;
                if (index == 0)
                {
                    world.UnlockedForms.Add(form.Id);
                    world.CurrentForms[form.HostPart] = form.Id;
                }
                else if (index == 1 && !string.IsNullOrEmpty(form.BlueprintId))
                {
                    world.Blueprints.Add(form.BlueprintId);
                }
                index++;
                if (index >= 4) break;
            }

            world.DisplayItems.Add(new DisplayItem("DISPLAY-001", "雨夜节拍器", DisplayKind.RepairCompletion, "EV-N01", world.Day));
            world.DisplayItems.Add(new DisplayItem("DISPLAY-002", "红舞鞋的回礼", DisplayKind.LifeGift, "CASE-RED-SHOE", world.Day - 1));
            world.DisplayItems.Add(new DisplayItem("DISPLAY-003", "钟楼修复成果", DisplayKind.Exhibition, "EV-CLOCK", world.Day - 2));
            world.DisplayItems.Add(new DisplayItem("DISPLAY-004", "镜中来客纪念物", DisplayKind.CaseMemento, "CASE-MIRROR", world.Day - 3));

            var cases = new[]
            {
                new CaseState(new CaseConfig("CASE-RED-SHOE", "红舞鞋", "B-01", 3, "委托", "旧城区")) { Kind = CaseStateKind.Investigating, CurrentStage = 1 },
                new CaseState(new CaseConfig("CASE-MIRROR", "镜中来客", "B-02", 2, "怪谈", "钟楼街")) { Kind = CaseStateKind.AwaitingRevisit, CurrentStage = 2 },
                new CaseState(new CaseConfig("CASE-CLOCK", "停摆钟楼", "B-03", 1, "回访", "北站")) { Kind = CaseStateKind.Resolved, CurrentStage = 1 },
            };
            world.Cases.AddRange(cases);

            world.Ledger.Add(new LedgerEntry { Direction = LedgerDirection.Income, Channel = PayChannel.Settlement, Amount = 180, Reason = "完成红舞鞋首案", Tick = 6 });
            world.Ledger.Add(new LedgerEntry { Direction = LedgerDirection.Expense, Channel = PayChannel.Craft, Amount = 72, Reason = "解锁贯通形态", Tick = 7 });
            world.Ledger.Add(new LedgerEntry { Direction = LedgerDirection.Income, Channel = PayChannel.Revisit, Amount = 120, Reason = "镜中来客回访", Tick = 8 });
        }
    }

    /// <summary>仅用于 Editor PlayMode 验收，不参与正式游戏流程。</summary>
    public sealed class UIAllPagesSmokeTest : MonoBehaviour
    {
        private static readonly UIViews[] Pages =
        {
            UIViews.MainPageShell, UIViews.DialogView, UIViews.BoardPage, UIViews.PreparationPage,
            UIViews.SettlementPage, UIViews.Settings, UIViews.SaveSlotPage, UIViews.MapPage,
            UIViews.HomePage, UIViews.JournalPage, UIViews.WorkbenchPage, UIViews.CodexPage,
            UIViews.ArchivePage, UIViews.GuestPage, UIViews.ServicePage, UIViews.DialoguePage,
            UIViews.ProloguePage, UIViews.InvestigationPage, UIViews.RecoveryPage, UIViews.FeedbackPage
        };

        private IEnumerator Start()
        {
            UIValidationHarness.PrepareValidationData();
            yield return new WaitForSecondsRealtime(0.5f);
            string folder = UIValidationHarness.CaptureFolder.Replace('\\', '/');
            Directory.CreateDirectory(folder);
            var report = new List<string> { "resolution=" + UIValidationHarness.CaptureWidth + "x" + UIValidationHarness.CaptureHeight, "pages=20" };
            for (int i = 0; i < Pages.Length; i++)
            {
                int serial = 0;
                string openError = null;
                try
                {
                    serial = GF.UI.OpenUIForm(Pages[i]);
                }
                catch (System.Exception ex)
                {
                    openError = ex.GetType().Name + ":" + ex.Message.Replace('\n', ' ');
                }

                yield return new WaitForSecondsRealtime(1.2f);
                // 部分 GF UIForm 在首帧完成资源绑定；强制刷新动态面板，保证截图包含注入数据。
                UIValidationHarness.RefreshOpenPanels();
                yield return new WaitForSecondsRealtime(0.25f);
                if (openError == null)
                {
                    if (UIValidationHarness.CaptureEnabled)
                    {
                        ScreenCapture.CaptureScreenshot(folder + "/" + Pages[i] + "_" + UIValidationHarness.CaptureWidth + "x" + UIValidationHarness.CaptureHeight + ".png");
                    }
                    report.Add(Pages[i] + ";opened=true;serial=" + serial);
                }
                else report.Add(Pages[i] + ";opened=false;error=" + openError);

                yield return new WaitForSecondsRealtime(0.2f);
                if (serial > 0)
                {
                    try { GF.UI.CloseUIForm(serial); }
                    catch (System.Exception ex) { report.Add(Pages[i] + ";closeError=" + ex.GetType().Name); }
                }
                yield return new WaitForSecondsRealtime(0.25f);
            }

            File.WriteAllLines("Library/UIAllPagesSmokeTest.txt", report);
            Debug.Log("[UIValidationHarness] 全量 20 页面烟雾测试完成。");
            Destroy(gameObject);
        }
    }
}
