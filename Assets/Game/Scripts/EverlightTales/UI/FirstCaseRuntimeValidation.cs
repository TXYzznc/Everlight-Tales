#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>正式GF页面的首案回归。临时测试槽位与PlayerPrefs在finally及OnDestroy恢复。</summary>
    public sealed class FirstCaseRuntimeValidation : MonoBehaviour
    {
        [Serializable] public sealed class PrefEntry { public string Key, Text; public int Number; public bool Exists, IsInt; }
        [Serializable] public sealed class PrefBackup { public List<PrefEntry> Entries = new List<PrefEntry>(); }
        private const string Folder = "Library/FirstCaseValidation";
        private static readonly string[] IntKeys = { "day", "period", "remainingCells", "repairFee", "batch", "tutorial", "jobs", "tutorialStage" };
        private static readonly string[] StringKeys = { "owned", "known", "materials", "blueprints", "forms", "currentForms", "tasks", "display", "firstCase", "places" };
        private readonly List<string> _report = new List<string>();
        private WorldSession _previous;
        private bool _restored;

        public static void PreservePrefs()
        {
            Directory.CreateDirectory(Folder);
            if (File.Exists(Folder + "/prefs-backup.json")) throw new InvalidOperationException("存在未恢复的验收备份，请先恢复。");
            var backup = new PrefBackup();
            for (int slot = 1; slot <= 3; slot++)
            {
                foreach (string key in IntKeys) Capture(backup, SaveSlotService.SlotKey("et.world." + key, slot), true);
                foreach (string key in StringKeys) Capture(backup, SaveSlotService.SlotKey("et.world." + key, slot), false);
                Capture(backup, "et.world.meta." + slot, false);
            }
            foreach (string key in new[] { "enabled", "normalFlow", "smoke" }) Capture(backup, "et.ui.validation." + key, true);
            File.WriteAllText(Folder + "/prefs-backup.json", JsonUtility.ToJson(backup));
        }

        private static void Capture(PrefBackup backup, string key, bool integer)
        {
            backup.Entries.Add(new PrefEntry { Key = key, Exists = PlayerPrefs.HasKey(key), IsInt = integer,
                Number = integer ? PlayerPrefs.GetInt(key) : 0, Text = integer ? null : PlayerPrefs.GetString(key) });
        }

        public static void RestorePrefs()
        {
            string path = Folder + "/prefs-backup.json";
            if (!File.Exists(path)) return;
            var backup = JsonUtility.FromJson<PrefBackup>(File.ReadAllText(path));
            foreach (PrefEntry entry in backup.Entries)
                if (!entry.Exists) PlayerPrefs.DeleteKey(entry.Key);
                else if (entry.IsInt) PlayerPrefs.SetInt(entry.Key, entry.Number);
                else PlayerPrefs.SetString(entry.Key, entry.Text);
            PlayerPrefs.Save(); File.Delete(path);
        }

        public static void Run()
        {
            if (!Application.isPlaying || !File.Exists(Folder + "/prefs-backup.json")) throw new InvalidOperationException("先备份，再从Launch进入Play Mode。");
            new GameObject("FirstCaseRuntimeValidation").AddComponent<FirstCaseRuntimeValidation>();
        }

        private IEnumerator Start()
        {
            _previous = WorldSession.Current;
            var stack = new Stack<IEnumerator>(); stack.Push(Test());
            try
            {
                while (stack.Count > 0)
                {
                    bool next; object current;
                    try { next = stack.Peek().MoveNext(); current = next ? stack.Peek().Current : null; }
                    catch (Exception exception) { _report.Add("FAIL " + exception); Debug.LogError("[FirstCase][Runtime] " + exception); break; }
                    if (!next) { stack.Pop(); if (stack.Count == 0) _report.Add("PASS ALL"); continue; }
                    if (current is IEnumerator child) { stack.Push(child); continue; }
                    yield return current;
                }
            }
            finally
            {
                Cleanup(); File.WriteAllLines(Folder + "/runtime.txt", _report);
                Debug.Log("[FirstCase][Runtime] " + _report.Last()); Destroy(gameObject);
            }
        }

        private IEnumerator Test()
        {
            for (int slot = 1; slot <= 3; slot++) SaveSlotService.Delete(slot);
            yield return WaitFor<SaveSlotPage>();
            SaveSlotItem first = Active<SaveSlotItem>().First(i => Field<int>(i, "_slot") == 1); Field<Button>(first, "_actionButton").onClick.Invoke();
            yield return WaitFor<MainPageShell>(); yield return new WaitForSecondsRealtime(3.5f);
            Canvas openingCanvas = Active<OpeningOverlay>().First().GetComponent<Canvas>();
            Require(openingCanvas != null && openingCanvas.overrideSorting && openingCanvas.sortingOrder > Active<MainPageShell>().First().SortOrder,
                "opening canvas is above hosted map");
            yield return CaptureImage("01-opening");
            OpeningOverlay opening = Active<OpeningOverlay>().First(); Field<Button>(opening, "m_ActionButton").onClick.Invoke();
            yield return Delay();
            for (int stage = 0; stage < 3; stage++)
            {
                yield return OpenMapEvent("home", "工作台教学"); yield return WaitFor<BoardPageForm>();
                BoardPage page = Active<BoardPage>().First(); page.Tap(); page.Tap();
                Require(page.LastRoundResult == RoundPassResult.Passed, "S0 stage " + stage + " real taps");
                Field<Button>(Active<BoardPageForm>().First(), "_settleButton").onClick.Invoke();
                yield return Delay(); CloseDialogs(); yield return Delay();
            }
            Require(WorldSession.Current.CanStartFirstCase, "S0 unlocks first case");
            yield return OpenMapEvent("home", "沈遥的红舞鞋线索"); yield return WaitFor<DialoguePageForm>();
            Field<Button[]>(Active<DialoguePageForm>().First(), "m_Buttons")[2].onClick.Invoke(); yield return Delay();
            Require(WorldSession.Current.GetCase(FirstCaseContent.CaseId) == null, "dialogue cancel does not advance");
            yield return OpenMapEvent("home", "沈遥的红舞鞋线索"); yield return WaitFor<DialoguePageForm>();
            WorldSession staleOwner = WorldSession.Current;
            WorldSession.NewGame(902, 2); yield return CompleteDialogue();
            Require(staleOwner.GetCase(FirstCaseContent.CaseId) == null && WorldSession.Current.GetCase(FirstCaseContent.CaseId) == null,
                "stale dialogue completion does not mutate either slot");
            WorldSession.LoadOrNew(WorldSession.DemoSeed, 1); FirstCaseFlow.RefreshMaps();
            yield return OpenMapEvent("home", "沈遥的红舞鞋线索"); yield return WaitFor<DialoguePageForm>();
            yield return CaptureImage("02-shen-dialogue"); yield return CompleteDialogue();
            WorldSession owner = WorldSession.Current; owner.Save();
            owner = WorldSession.LoadOrNew(WorldSession.DemoSeed, 1); FirstCaseFlow.RefreshMaps();
            Require(owner.GetCase(FirstCaseContent.CaseId)?.Kind == CaseStateKind.Investigating && owner.World.Map.Get("dance").Status == PlaceNodeStatus.Unlocked, "reload request and place");
            yield return Night(); yield return OpenMapEvent("dance", "没有结束的排练"); yield return WaitFor<InvestigationPageForm>();
            int before = owner.Time.RemainingCells;
            Active<InvestigationPageForm>().First().OnClickClose(); yield return Delay();
            Require(owner.GetCase(FirstCaseContent.CaseId).Kind == CaseStateKind.Investigating && owner.Time.RemainingCells == before - 1, "investigation cancellation costs one cell");
            yield return OpenMapEvent("dance", "没有结束的排练"); yield return WaitFor<InvestigationPageForm>(); yield return CaptureImage("03-investigation");
            var hotspots = Active<InvestigationHotspotItem>(); Require(hotspots.Length == 3, "three real hotspots");
            Field<Button>(hotspots[0], "_button").onClick.Invoke(); Field<Button>(hotspots[0], "_button").onClick.Invoke();
            Require(Active<InvestigationView>().First().ConfirmedCount == 1, "duplicate hotspot does not count");
            Field<Button>(hotspots[1], "_button").onClick.Invoke(); Field<Button>(hotspots[2], "_button").onClick.Invoke();
            yield return WaitFor<DialoguePageForm>(); yield return CompleteDialogue();
            Require(owner.GetCase(FirstCaseContent.CaseId).Kind == CaseStateKind.AwaitingRepair, "investigation completes");
            for (int slot = 2; slot <= 3; slot++) { var other = WorldSession.NewGame(900 + slot, slot); other.Save(); }
            owner = WorldSession.LoadOrNew(WorldSession.DemoSeed, 1); FirstCaseFlow.RefreshMaps();
            Require(owner.GetCase(FirstCaseContent.CaseId).Kind == CaseStateKind.AwaitingRepair && SaveSlotService.HasSlot(2) && SaveSlotService.HasSlot(3), "three slots isolated");
            // 旧档缺少增量字段：仍安全加载，不伪造案件完成。
            PlayerPrefs.DeleteKey("et.world.2.firstCase"); PlayerPrefs.DeleteKey("et.world.2.places");
            PlayerPrefs.SetInt("et.world.2.tutorialStage", 3); PlayerPrefs.SetInt("et.world.2.tutorial", 0);
            var old = WorldSession.LoadOrNew(902, 2);
            Require(old.GetCase(FirstCaseContent.CaseId) == null && old.CanStartFirstCase, "old/default progress and completed tutorial compatible");
            owner = WorldSession.LoadOrNew(WorldSession.DemoSeed, 1); FirstCaseFlow.RefreshMaps();
            yield return StartRepair();
            GF.UI.CloseUIForm(Active<BoardPageForm>().First().Id); yield return Delay();
            owner = WorldSession.LoadOrNew(WorldSession.DemoSeed, 1); FirstCaseFlow.RefreshMaps();
            Require(owner.GetCase(FirstCaseContent.CaseId).Kind == CaseStateKind.AwaitingRepair
                && owner.CurrentEvent == null && owner.CurrentRedShoeLevel == null, "interrupted repair reloads initial attempt with investigation retained");
            yield return StartRepair();
            BoardPage failurePage = Active<BoardPage>().First(); failurePage.Tap(); failurePage.Tap(); failurePage.Tap();
            Require(failurePage.LastRoundResult == RoundPassResult.Failed, "failed special goal"); CloseDialogs();
            Field<Button>(Active<BoardPageForm>().First(), "_settleButton").onClick.Invoke(); yield return WaitFor<SettlementPageForm>();
            Active<SettlementPageForm>().First().BackToMap.onClick.Invoke(); yield return Delay();
            Require(owner.GetCase(FirstCaseContent.CaseId).Kind == CaseStateKind.AwaitingRepair, "failure retains investigation");
            yield return StartRepair(); yield return Retreat();
            Require(owner.GetCase(FirstCaseContent.CaseId).Kind == CaseStateKind.AwaitingRepair, "retreat permits retry");
            bool won = false;
            for (int attempt = 0; attempt < 30 && !won; attempt++)
            {
                yield return StartRepair();
                var page = Active<BoardPage>().First(); yield return CaptureImage("04-board");
                int[] directions = { 5, 1, 2, 1, 5, 0, 5, 5, 5 };
                bool retry = false;
                for (int tap = 0; tap < 9; tap++)
                {
                    while ((int)owner.CurrentRedShoeLevel.RedShoe.Direction != directions[tap]) page.RotateRight();
                    if (tap == 4 || tap == 5)
                    {
                        owner.CurrentEvent.Board.TryGetEntity(6, out BoardEntity pliers);
                        Require(page.Game.ArmMove(pliers, tap == 4 ? new HexCoord(0, 0) : new HexCoord(0, 2)) == ArmMoveResult.Ok, "real mechanical arm");
                    }
                    RoundPassResult result = page.Tap();
                    Require(result != RoundPassResult.Failed, "winning tap " + tap);
                    if (tap == 2 || tap == 5)
                    {
                        yield return WaitFor<SettlementPageForm>();
                        var options = owner.PendingFirstCaseRewards;
                        Require(options.Count == 4 && options.Select(o => o.Id).Distinct().Count() == 4, "four unique round choices");
                        int index = tap == 2 ? options.ToList().FindIndex(o => o.Id == "BF-014") : 0;
                        if (index < 0) { index = 0; retry = true; }
                        Active<SettlementPageForm>().First().RewardChoices.GetComponentsInChildren<ListRowItem>().Where(i => i.gameObject.activeInHierarchy).ToArray()[index].Button.onClick.Invoke();
                        yield return Delay();
                        if (retry) break;
                    }
                }
                if (retry) { yield return Retreat(); continue; }
                CloseDialogs(); Field<Button>(Active<BoardPageForm>().First(), "_settleButton").onClick.Invoke();
                yield return WaitFor<SettlementPageForm>(); yield return CaptureImage("05-success");
                Active<SettlementPageForm>().First().BackToMap.onClick.Invoke(); yield return Delay(); won = true;
            }
            Require(won && owner.CurrentRedShoeLevel.RedShoe.IsSealed && owner.World.OwnedParts.Contains(PartType.RivetPliers), "actual three-round success");
            int day = owner.Time.Day, cells = owner.Time.RemainingCells, fee = owner.World.RepairFee;
            owner.SettleEvent(SettlementOutcomeKind.Success);
            Require(owner.Time.Day == day && owner.Time.RemainingCells == cells && owner.World.RepairFee == fee, "duplicate settlement safe");
            owner = WorldSession.LoadOrNew(WorldSession.DemoSeed, 1); FirstCaseFlow.RefreshMaps();
            Require(owner.GetCase(FirstCaseContent.CaseId).Kind == CaseStateKind.AwaitingRevisit, "success reload keeps revisit");
            while (owner.Time.Period != TimeOfDay.Morning) { owner.WaitToNextPeriod(); yield return Delay(); CloseDialogs(); }
            FirstCaseFlow.RefreshMaps(); yield return OpenMapEvent("home", "沈遥的回访"); yield return WaitFor<DialoguePageForm>();
            yield return CaptureImage("06-revisit"); yield return CompleteDialogue(); CloseDialogs();
            Require(owner.World.RepairFee == fee + 120 && owner.GetCase(FirstCaseContent.CaseId).RewardDelivered, "revisit delivers once");
            owner.RevisitCase(FirstCaseContent.CaseId); Require(owner.World.RepairFee == fee + 120, "duplicate revisit safe");
            owner = WorldSession.LoadOrNew(WorldSession.DemoSeed, 1);
            Require(owner.GetCase(FirstCaseContent.CaseId).Kind == CaseStateKind.Revisited && owner.World.RepairFee == fee + 120, "revisit survives reload");
            SaveSlotService.Delete(1); Require(!PlayerPrefs.HasKey("et.world.firstCase") && !PlayerPrefs.HasKey("et.world.places") && SaveSlotService.HasSlot(2), "delete clears progress and isolates slots");
        }

        private IEnumerator StartRepair()
        {
            yield return Night(); yield return OpenMapEvent("dance", "停不下来的排练"); yield return WaitFor<PreparationPageForm>();
            Require(WorldSession.Current.PendingCarry.SelectedCount == 4, "four fixed carried types");
            Require(Active<PreviewBoardView>().First().transform.GetComponentsInChildren<Image>().Any(i => i.name == "RedShoes" && i.sprite != null), "preview red shoes art");
            Active<PreparationPageForm>().First().ConfirmButton.onClick.Invoke(); yield return WaitFor<BoardPageForm>();
            var boardForm = Active<BoardPageForm>().First();
            Image background = Field<Image>(boardForm, "_sceneBackground");
            var catalog = Field<UIFormalSpriteCatalog>(boardForm, "_spriteCatalog");
            Require(background != null && background.enabled && background.sprite == catalog.Get("SCR-07-08-dance-night") && background.color == Color.white,
                "live dance night scene background");
            Transform backing = background.transform.parent.Find("OpaquePageBacking");
            Require(backing == null || backing.GetSiblingIndex() < background.transform.GetSiblingIndex(), "scene is above opaque backing");
        }

        private IEnumerator Retreat()
        {
            var form = Active<BoardPageForm>().First(); Field<Button>(form, "_pauseButton").onClick.Invoke();
            Field<Button>(form, "_pauseRetreat").onClick.Invoke(); yield return WaitFor<DialogView>(); ClickDialog();
            yield return WaitFor<SettlementPageForm>(); Active<SettlementPageForm>().First().BackToMap.onClick.Invoke(); yield return Delay();
        }

        private IEnumerator Night()
        {
            CloseDialogs();
            while (!TimePeriod.IsNight(WorldSession.Current.Time.Period))
            {
                WorldSession.Current.WaitToNextPeriod(); FirstCaseFlow.RefreshMaps(); yield return Delay(); CloseDialogs();
            }
        }

        private IEnumerator OpenMapEvent(string place, string title)
        {
            CloseDialogs(); FirstCaseFlow.RefreshMaps(); Active<CityMapView>().First().Select(place); yield return null;
            var row = Active<ListRowItem>().First(i => Field<TMPro.TMP_Text>(i, "_title").text.Contains(title));
            row.Button.onClick.Invoke(); yield return WaitFor<DialogView>(); ClickDialog(); yield return Delay();
        }

        private IEnumerator CompleteDialogue()
        {
            var form = Active<DialoguePageForm>().First();
            Field<Button[]>(form, "m_Buttons")[0].onClick.Invoke(); yield return null;
            Field<Button[]>(form, "m_Buttons")[0].onClick.Invoke(); yield return Delay();
        }
        private static T[] Active<T>() where T : Component => UnityEngine.Object.FindObjectsOfType<T>().Where(x => x.gameObject.activeInHierarchy).ToArray();
        private static T Field<T>(object obj, string name) => (T)obj.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
        private static IEnumerator WaitFor<T>() where T : Component
        {
            float limit = Time.realtimeSinceStartup + 12f;
            while (Active<T>().Length == 0) { if (Time.realtimeSinceStartup > limit) throw new InvalidOperationException("Timeout " + typeof(T).Name); yield return null; }
            yield return new WaitForSecondsRealtime(.15f);
        }
        private static void ClickDialog() { var dialog = Active<DialogView>().First(i => i.UIForm != null); Field<Button[]>(dialog, "m_Buttons")[0].onClick.Invoke(); }
        private static void CloseDialogs() { foreach (var dialog in Active<DialogView>()) if (dialog.UIForm != null) GF.UI.CloseUIForm(dialog.Id); }
        private static IEnumerator Delay() { yield return new WaitForSecondsRealtime(.35f); }
        private void Require(bool condition, string text) { if (!condition) throw new InvalidOperationException(text); _report.Add("PASS " + text); }
        private static IEnumerator CaptureImage(string name)
        {
            yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(Folder + "/" + name + ".png");
            yield return new WaitForEndOfFrame(); yield return null;
        }
        private void Cleanup()
        {
            if (_restored) return; _restored = true;
            foreach (UIFormBase form in Active<UIFormBase>()) if (form.UIForm != null) GF.UI.CloseUIForm(form.Id);
            typeof(WorldSession).GetProperty("Current").SetValue(null, _previous);
            RestorePrefs();
        }
        private void OnDestroy() { Cleanup(); }
    }
}
#endif
