using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;
using UnityEditor;
using UnityEngine;

namespace Everlight.Tales.UI.Editor
{
    public static class RepairWorldPersistenceValidation
    {
        private const string RootKey = "Everlight.RepairPersistence.ValidationRoot";
        private const string BackupPath = "Library/RepairPersistenceValidation/world-prefs-backup.json";
        private static readonly List<string> Lines = new List<string>();

        [Serializable] private sealed class Backup
        {
            public string PreviousRoot;
            public List<FirstCaseRuntimeValidation.PrefEntry> Entries = new List<FirstCaseRuntimeValidation.PrefEntry>();
        }

        [MenuItem("Game Framework/EverlightTales/Save/验证世界提交与槽位恢复")]
        public static void Run()
        {
            if (!Application.isPlaying || !GameConfigRuntime.EnsureLoaded())
                throw new InvalidOperationException("从Launch进入Play Mode并等待配置加载后运行。");
            if (UnityEngine.Object.FindObjectOfType<FirstCaseRuntimeValidation>() != null)
                throw new InvalidOperationException("不能和正式UI流程验收同时运行。");
            WorldSession previous = WorldSession.Current;
            Preserve();
            Lines.Clear();
            try
            {
                EntryAndRecovery();
                foreach (string stage in new[] { "after-world-value", "after-world-save", "before-receipt", "after-receipt", "before-delete" })
                    InterruptedSettlement(stage);
                BadDataAndLifetime();
                SupplyLifetime();
                CorruptJournal();
                IncompatibleCheckpoint();
                TerminalReward();
                WorldFieldsAndLegacy();
                Lines.Add("PASS ALL");
            }
            catch (Exception exception) { Lines.Add("FAIL " + exception); throw; }
            finally
            {
                Restore();
                typeof(WorldSession).GetProperty("Current", BindingFlags.Public | BindingFlags.Static).SetValue(null, previous);
                File.WriteAllLines("Library/RepairPersistenceValidation/world.txt", Lines);
                Debug.Log("[RepairWorldPersistence][Validation] " + string.Join("; ", Lines));
            }
        }

        private static void Preserve()
        {
            if (File.Exists(BackupPath)) throw new InvalidOperationException("先恢复未完成的世界存档验证备份。");
            Directory.CreateDirectory(Path.GetDirectoryName(BackupPath));
            var backup = new Backup { PreviousRoot = UnityEditor.SessionState.GetString(RootKey, "") };
            for (int slot = 1; slot <= 3; slot++)
                foreach (string key in PlayerPrefsWorldImageStore.EnumerateKeys(slot, true))
                {
                    bool integer = PlayerPrefsWorldImageStore.IsIntegerKey(slot, key);
                    backup.Entries.Add(new FirstCaseRuntimeValidation.PrefEntry
                    {
                        Key = key, Exists = PlayerPrefs.HasKey(key), IsInt = integer,
                        Number = integer ? PlayerPrefs.GetInt(key) : 0, Text = integer ? null : PlayerPrefs.GetString(key)
                    });
                }
            File.WriteAllText(BackupPath, JsonUtility.ToJson(backup));
            UnityEditor.SessionState.SetString(RootKey, Path.GetFullPath("Library/RepairPersistenceValidation/world-files-" + Guid.NewGuid().ToString("N")));
        }

        [MenuItem("Game Framework/EverlightTales/Save/恢复世界存档验证备份")]
        public static void Restore()
        {
            if (!File.Exists(BackupPath)) return;
            Backup backup = JsonUtility.FromJson<Backup>(File.ReadAllText(BackupPath));
            foreach (var entry in backup.Entries)
                if (!entry.Exists) PlayerPrefs.DeleteKey(entry.Key);
                else if (entry.IsInt) PlayerPrefs.SetInt(entry.Key, entry.Number);
                else PlayerPrefs.SetString(entry.Key, entry.Text);
            PlayerPrefs.Save();
            if (string.IsNullOrEmpty(backup.PreviousRoot)) UnityEditor.SessionState.EraseString(RootKey);
            else UnityEditor.SessionState.SetString(RootKey, backup.PreviousRoot);
            File.Delete(BackupPath);
        }

        private static WorldSession Fresh(int seed = 613)
        {
            WorldSession session = WorldSession.NewGame(seed, 3);
            session.Save();
            // 通过真实教学演算取得首批可携带件，不用空新档跳过准备资格。
            for (int i = 0; i < 3; i++)
            {
                TutorialStage stage = i == 0 ? TutorialSample.RotateAndCollide()
                    : i == 1 ? TutorialSample.EnergyAndRepair() : TutorialSample.BlastAndClear();
                var game = new BoardGame(stage.Board, stage.Settle, stage.Session, stage.Level);
                game.Tap();
                if (game.Tap() != RoundPassResult.Passed || !session.CompleteTutorialStage(i))
                    throw new InvalidOperationException("Teaching setup did not pass through actual taps.");
            }
            return session;
        }

        private static void EntryAndRecovery()
        {
            WorldSession session = Fresh();
            WorldImage original = PlayerPrefsWorldImageStore.CapturePersisted(3);
            session.AttemptStore.FaultInjector = point => { if (point == "before-write") throw new IOException("initial-save"); };
            Check(session.StartRollerDoor() == null && session.PersistenceBlocked, "first checkpoint failure blocks entry");
            string attempt = session.ActiveAttemptId; int consumed = session.Rng.ConsumedCount;
            session.AttemptStore.FaultInjector = null;
            Check(session.ConfirmPreparedEvent() != null && session.ActiveAttemptId == attempt
                && session.Rng.ConsumedCount == consumed, "entry retry preserves same attempt and random draw");
            var json = new NewtonsoftJsonHelper();
            string initial = json.ToJson(Everlight.Tales.Events.RepairSaveService.Capture(attempt, "EV-N01",
                session.CurrentEvent.Board, session.CurrentEvent.Level, session.Rng, session.HeldBuffs, session.CurrentSettle));
            var game = new BoardGame(session.CurrentEvent.Board, session.CurrentSettle,
                session.CurrentEvent.Level.Session, session.CurrentEvent.Level);
            game.RotateRight(); game.Tap();
            session = WorldSession.LoadOrNew(613, 3);
            Check(session.HasPendingRecovery && session.RecoveryRound == 1 && session.ContinueAttempt(), "roller door resumes real round start");
            string restored = json.ToJson(Everlight.Tales.Events.RepairSaveService.Capture(attempt, "EV-N01",
                session.CurrentEvent.Board, session.CurrentEvent.Level, session.Rng, session.HeldBuffs, session.CurrentSettle));
            Check(initial == restored, "roller marker endpoints and forms restored exactly");
            Check(new PlayerPrefsWorldImageStore(3, session.GenerationId).Matches(original), "continue does not charge or reward world");
            session.AttemptStore.FaultInjector = point => { if (point == "before-write") throw new IOException("journal-save"); };
            int cells = session.Time.RemainingCells;
            Check(!session.TrySettleEvent(SettlementOutcomeKind.Retreat, out _) && session.Time.RemainingCells == cells
                && new PlayerPrefsWorldImageStore(3, session.GenerationId).Matches(original), "journal failure leaves both world representations unchanged");
            bool blocked = false;
            try { session.WaitToNextPeriod(); } catch (InvalidOperationException) { blocked = true; }
            Check(blocked && session.Time.RemainingCells == cells, "pending settlement blocks time mutation before it happens");
            session.AttemptStore.FaultInjector = null;
            Check(session.TrySettleEvent(SettlementOutcomeKind.Retreat, out _) && session.Time.RemainingCells == cells - 2,
                "journal retry charges retreat once");
            Check(session.TrySettleEvent(SettlementOutcomeKind.Retreat, out _) && session.Time.RemainingCells == cells - 2,
                "repeated retreat is idempotent");
        }

        private static void InterruptedSettlement(string stage)
        {
            WorldSession session = Fresh();
            Check(session.StartRollerDoor() != null, "start for " + stage);
            var expected = new TimeState(session.Time.Day, session.Time.Period, session.Time.RemainingCells);
            expected.Advance(session.CurrentEvent.Config.TimeCost);
            if (stage == "before-delete")
                session.AttemptStore.FaultInjector = point => { if (point == stage) throw new IOException(stage); };
            else session.WorldPersistenceFaultInjector = point => { if (point == stage) throw new IOException(stage); };
            Check(!session.TrySettleEvent(SettlementOutcomeKind.Retreat, out _) && session.PersistenceBlocked,
                "interruption remains recoverable " + stage);
            session = WorldSession.LoadOrNew(613, 3);
            Check(!session.HasAttemptSave && session.Time.Day == expected.Day && session.Time.Period == expected.Period
                && session.Time.RemainingCells == expected.RemainingCells && session.World.RepairFee == 0,
                "startup replays absolute result once " + stage);
            uint rng = session.Rng.State; string supply = new NewtonsoftJsonHelper().ToJson(session.Supply);
            session = WorldSession.LoadOrNew(613, 3);
            Check(session.Rng.State == rng && new NewtonsoftJsonHelper().ToJson(session.Supply) == supply,
                "supply and random state do not redraw " + stage);
        }

        private static void BadDataAndLifetime()
        {
            WorldSession session = Fresh();
            session.StartRollerDoor();
            string attempt = session.ActiveAttemptId;
            var identity = new AttemptIdentity { Slot = 3, GenerationId = session.GenerationId, AttemptId = attempt, Sequence = 1 };
            string file = Path.Combine(session.AttemptStore.DirectoryFor(identity), "attempt.json");
            File.Copy(file, file + ".bak", true); File.WriteAllText(file, "bad-main");
            session = WorldSession.LoadOrNew(613, 3);
            Check(session.HasPendingRecovery && session.RecoveryNotice.Contains("备份"), "valid backup offered with recovery notice");
            File.WriteAllText(file + ".bak", "bad-backup");
            session = WorldSession.LoadOrNew(613, 3);
            Check(!session.HasAttemptSave && session.Time.RemainingCells == 4 && session.World.RepairFee == 0,
                "unrecoverable checkpoint ends without cost or reward");
            session.StartRollerDoor();
            WorldSession stale = session;
            WorldImage before = PlayerPrefsWorldImageStore.CapturePersisted(3);
            WorldSession other = WorldSession.NewGame(901, 2); other.Save();
            Check(!stale.TrySettleEvent(SettlementOutcomeKind.Retreat, out _)
                && new PlayerPrefsWorldImageStore(3, stale.GenerationId).Matches(before)
                && other.World.RepairFee == 0, "old session cannot settle either slot");
            session = WorldSession.LoadOrNew(613, 3);
            SaveSlotService.Delete(3);
            Check(!session.ContinueAttempt() && SaveSlotService.HasSlot(2), "deleted generation rejects old callback and preserves other slot");
            session = WorldSession.NewGame(613, 3); session.Save(); session.StartRollerDoor();
            PlayerPrefs.SetInt(PlayerPrefsWorldImageStore.Key(3, "deleting"), 1); PlayerPrefs.Save();
            session = WorldSession.LoadOrNew(613, 3);
            Check(!session.HasAttemptSave && session.IsNewGame && SaveSlotService.HasSlot(2), "interrupted deletion completes before fresh slot loads");
            var adapter = new PlayerPrefsWorldImageStore(3, session.GenerationId);
            var invalid = session.CaptureWorldImage();
            invalid.Values[0].Key = PlayerPrefsWorldImageStore.Key(2, "day");
            bool rejected = false;
            try { adapter.Apply(invalid); } catch (ArgumentException) { rejected = true; }
            Check(rejected && SaveSlotService.HasSlot(2), "cross-slot image rejected before writing");
        }

        private static void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException(name);
            Lines.Add("PASS " + name);
        }

        private static void SupplyLifetime()
        {
            WorldSession session = Fresh();
            SupplyInstance source = null;
            // 使用正式供给生成器取得 EV-N01；最多检查四天，不构造供给实例。
            for (int period = 0; period < 20 && source == null; period++)
            {
                foreach (SupplyInstance supply in session.Supply)
                    if (supply.Template.TemplateId == "EV-N01" && !supply.Processed) { source = supply; break; }
                if (source == null) session.WaitToNextPeriod();
            }
            Check(source != null && session.PrepareRollerDoor(source) != null, "real generated supply can prepare matching repair");
            foreach (PartType part in session.PendingCarry.Available) session.PendingCarry.Fill(part);
            Check(session.ConfirmPreparedEvent() != null, "generated supply confirms with frozen source identity");
            string id = source.InstanceId; uint rng = session.Rng.State;
            Check(session.TrySettleEvent(SettlementOutcomeKind.Retreat, out _), "generated supply retreat commits");
            SupplyInstance processed = null;
            foreach (SupplyInstance supply in session.Supply) if (supply.InstanceId == id) processed = supply;
            Check(processed != null && processed.Processed && session.Rng.State == rng,
                "same halfday settlement retains processed instance without redraw");
            session = WorldSession.LoadOrNew(613, 3);
            processed = null;
            foreach (SupplyInstance supply in session.Supply) if (supply.InstanceId == id) processed = supply;
            Check(processed != null && processed.Processed && session.PrepareRollerDoor(processed) == null,
                "processed source survives restart and cannot prepare again");
            // 同半日再等一时段不刷新；跨半日/跨日必须产生新的实例 ID，重启保持它们。
            session.WaitToNextPeriod();
            Check(session.Rng.State == rng, "same halfday period change does not redraw supply");
            session.WaitToNextPeriod();
            foreach (SupplyInstance supply in session.Supply) Check(supply.InstanceId != id, "new halfday never reuses processed instance ID");
            uint nightRng = session.Rng.State; string nightSupply = new NewtonsoftJsonHelper().ToJson(session.Supply);
            session = WorldSession.LoadOrNew(613, 3);
            Check(session.Rng.State == nightRng && new NewtonsoftJsonHelper().ToJson(session.Supply) == nightSupply,
                "halfday batch and RNG survive restart");
            int day = session.Time.Day;
            while (session.Time.Day == day) session.WaitToNextPeriod();
            string nextDaySupply = new NewtonsoftJsonHelper().ToJson(session.Supply); uint nextDayRng = session.Rng.State;
            session = WorldSession.LoadOrNew(613, 3);
            Check(session.Rng.State == nextDayRng && new NewtonsoftJsonHelper().ToJson(session.Supply) == nextDaySupply,
                "new day supply and RNG survive restart");
        }

        private static void CorruptJournal()
        {
            WorldSession session = Fresh(); session.StartRollerDoor();
            var identity = new AttemptIdentity { Slot = 3, GenerationId = session.GenerationId,
                AttemptId = session.ActiveAttemptId, Sequence = 1 };
            session.WorldPersistenceFaultInjector = point => { if (point == "after-world-value") throw new IOException("partial"); };
            Check(!session.TrySettleEvent(SettlementOutcomeKind.Retreat, out _), "prepare redundant journal before corrupt-pair probe");
            string file = Path.Combine(session.AttemptStore.DirectoryFor(identity), "settlement.json");
            File.WriteAllText(file, "bad-journal"); File.WriteAllText(file + ".bak", "bad-journal-backup");
            WorldImage partial = PlayerPrefsWorldImageStore.CapturePersisted(3);
            session = WorldSession.LoadOrNew(613, 3);
            Check(!session.HasAttemptSave && session.RecoveryNotice.Contains("无法确认")
                && ImageWithoutMeta(partial) == ImageWithoutMeta(PlayerPrefsWorldImageStore.CapturePersisted(3)),
                "both corrupt journals preserve current world without guessing reward or reactivating checkpoint");
            Check(Directory.GetFiles(Path.Combine(WorldSession.PersistenceRoot, "slots", "3", "quarantine")).Length >= 3,
                "corrupt result and old checkpoint retained as isolated evidence");
        }

        private static void IncompatibleCheckpoint()
        {
            foreach (bool future in new[] { false, true })
            {
                WorldSession session = Fresh(); session.StartRollerDoor();
                var identity = new AttemptIdentity { Slot = 3, GenerationId = session.GenerationId,
                    AttemptId = session.ActiveAttemptId, Sequence = 1 };
                CheckpointRecord checkpoint = session.AttemptStore.ReadCheckpoint(identity, null).Value;
                if (future)
                {
                    RepairAttemptSave saved = new NewtonsoftJsonHelper().ToObject<RepairAttemptSave>(checkpoint.StateJson);
                    saved.Version = RepairAttemptSave.CurrentVersion + 1;
                    checkpoint.StateJson = new NewtonsoftJsonHelper().ToJson(saved);
                }
                else checkpoint.ConfigFingerprint = "incompatible-rule-set";
                session.AttemptStore.WriteCheckpoint(checkpoint);
                File.Delete(Path.Combine(session.AttemptStore.DirectoryFor(identity), "attempt.json.bak"));
                session = WorldSession.LoadOrNew(613, 3);
                Check(!session.HasAttemptSave && session.Time.RemainingCells == 4 && session.World.RepairFee == 0,
                    future ? "future snapshot schema permits free restart" : "changed configuration permits free restart");
            }
        }

        private static void TerminalReward()
        {
            WorldSession session = null; RepairRouteValidation.Route route = null;
            for (int seed = 613; seed < 623 && route == null; seed++)
            {
                session = Fresh(seed);
                session.Time.Advance(3); session.World.Day = session.Time.Day; session.World.Period = session.Time.Period;
                session.Save(); session.StartRollerDoor(); route = RepairRouteValidation.FindRoller(session);
            }
            Check(route != null, "bounded legal policy finds a roller completion route");
            Lines.Add("ROUTE seed=" + session.Rng.Seed + " arm=" + route.ArmEntityId + " target=" + route.ArmTarget
                + " phases=" + string.Join(",", route.Phases) + " evaluated=" + route.EvaluatedStates);
            var game = new BoardGame(session.CurrentEvent.Board, session.CurrentSettle,
                session.CurrentEvent.Level.Session, session.CurrentEvent.Level);
            if (route.ArmEntityId != 0)
            {
                game.Board.TryGetEntity(route.ArmEntityId, out BoardEntity entity);
                Check(game.ArmMove(entity, route.ArmTarget) == ArmMoveResult.Ok, "legal roller arm input");
            }
            foreach (int phase in route.Phases)
            {
                while ((int)game.Settle.GravityDirection != phase) game.RotateRight();
                game.Tap();
            }
            Check(game.Level.Round.TapQuotaRemaining == 0 && session.IsCurrentSpecialGoalMet()
                && game.Session.Score >= game.Level.Round.TargetScore, "real roller taps complete score and pushed marker");
            var identity = new AttemptIdentity { Slot = 3, GenerationId = session.GenerationId,
                AttemptId = session.ActiveAttemptId, Sequence = 1 };
            session.WorldPersistenceFaultInjector = point => { if (point == "after-world-save") throw new IOException("success-save"); };
            Check(!session.TrySettleEvent(SettlementOutcomeKind.Success, out _) && session.PersistenceBlocked,
                "successful result journal survives interruption before receipt");
            string file = Path.Combine(session.AttemptStore.DirectoryFor(identity), "attempt.json");
            File.WriteAllText(file, "bad-success-checkpoint"); File.WriteAllText(file + ".bak", "bad-success-checkpoint-backup");
            session = WorldSession.LoadOrNew(613, 3);
            Check(!session.HasAttemptSave && session.HasPendingSettlementReward && session.World.RepairFee == 40
                && session.Time.Period == TimeOfDay.Afternoon && session.Time.RemainingCells == 3,
                "valid successful journal takes precedence over corrupt checkpoint and preserves cross-period result once");
            string choices = ChoiceIds(session); uint rng = session.Rng.State;
            session = WorldSession.LoadOrNew(613, 3);
            Check(session.HasPendingSettlementReward && ChoiceIds(session) == choices && session.Rng.State == rng,
                "terminal choices survive restart without reroll");
            var option = session.DrawRewardChoice()[0];
            WorldImage before = PlayerPrefsWorldImageStore.CapturePersisted(3);
            session.AttemptStore.FaultInjector = point => { if (point == "before-write") throw new IOException("choice-save"); };
            Check(!session.TryApplyReward(option) && new PlayerPrefsWorldImageStore(3, session.GenerationId).Matches(before),
                "terminal choice failure does not grant early reward");
            session.AttemptStore.FaultInjector = null;
            Check(session.TryApplyReward(option) && !session.HasPendingSettlementReward, "terminal choice retries same absolute result");
            before = PlayerPrefsWorldImageStore.CapturePersisted(3);
            Check(!session.TryApplyReward(option) && new PlayerPrefsWorldImageStore(3, session.GenerationId).Matches(before),
                "terminal choice cannot be claimed twice");
            session = WorldSession.LoadOrNew(613, 3);
            Check(!session.HasPendingSettlementReward && session.DrawRewardChoice().Count == 0,
                "claimed terminal reward does not reopen after restart");
        }

        private static string ChoiceIds(WorldSession session)
        {
            var values = new List<string>();
            foreach (var option in session.DrawRewardChoice()) values.Add(option.Kind + ":" + option.Id);
            return string.Join(",", values);
        }

        private static void WorldFieldsAndLegacy()
        {
            // 字段往返夹具，不作为玩法通关证据。
            WorldSession session = Fresh();
            session.Time.Advance(1); session.World.RepairFee = 137;
            session.World.Materials.Add("MT-006", 3, "L-01");
            session.World.KnownParts.Add(PartCatalog.All()[3].Type);
            FormConfig form = FormCatalog.All()[0];
            if (!session.World.OwnedParts.Contains(form.HostPart)) session.World.OwnedParts.Add(form.HostPart);
            session.World.UnlockedForms.Add(form.Id); session.World.CurrentForms[form.HostPart] = form.Id;
            if (!string.IsNullOrEmpty(form.BlueprintId)) session.World.Blueprints.Add(form.BlueprintId);
            session.World.Tasks.Add(new Everlight.Tales.Meta.TaskState(TaskCatalog.All[0]) { CurrentStep = 1 });
            session.World.Map.Unlock("dance"); session.World.HomeState = "field-roundtrip";
            session.World.Ledger.Add(new LedgerEntry { Amount = 137, Reason = "field\\nroundtrip", Tick = 1 });
            session.TrackTask(TaskCatalog.All[0].Id);
            session.Save();
            WorldImage persisted = PlayerPrefsWorldImageStore.CapturePersisted(3);
            string materials = new NewtonsoftJsonHelper().ToJson(session.World.Materials);
            session = WorldSession.LoadOrNew(999, 3);
            Check(ImageWithoutMeta(persisted) == ImageWithoutMeta(session.CaptureWorldImage()),
                "all controlled world fields roundtrip including material source forms tasks ledger places and RNG");
            Check(session.Time.RemainingCells == 3 && session.World.RepairFee == 137,
                "remaining cells and world currency restored exactly");
            WorldSession legacy = WorldSession.NewGame(123, 1); legacy.Save();
            var legacyImage = new WorldImage();
            foreach (WorldValue value in persisted.Values)
                legacyImage.Values.Add(new WorldValue
                {
                    Key = value.Key == "et.world.meta.3" ? "et.world.meta.1" : value.Key.Replace("et.world.3.", "et.world."),
                    Kind = value.Kind, Integer = value.Integer, Text = value.Text
                });
            new PlayerPrefsWorldImageStore(1, legacy.GenerationId).Apply(legacyImage);
            foreach (string key in new[] { "generation", "commit", "runtime" }) PlayerPrefs.DeleteKey("et.world." + key);
            PlayerPrefs.Save();
            legacy = WorldSession.LoadOrNew(123, 1);
            Check(legacy.Time.RemainingCells == 3 && legacy.World.RepairFee == 137
                && legacy.World.CurrentForms[form.HostPart] == form.Id
                && new NewtonsoftJsonHelper().ToJson(legacy.World.Materials) == materials,
                "legacy slot one keys retain time assets material source and forms without identity or runtime fields");
            Check(ImageWithoutMeta(persisted) == ImageWithoutMeta(PlayerPrefsWorldImageStore.CapturePersisted(3)),
                "legacy upgrade does not change slot three");
        }

        private static string ImageWithoutMeta(WorldImage image)
        {
            var filtered = new WorldImage();
            foreach (WorldValue value in image.Values)
                if (!value.Key.StartsWith("et.world.meta.", StringComparison.Ordinal)) filtered.Values.Add(value);
            return SettlementJournalCommit.ImageHash(filtered);
        }
    }
}
