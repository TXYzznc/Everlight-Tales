using System;
using System.Collections.Generic;
using System.IO;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;
using UnityEditor;
using UnityEngine;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>真实目录配置上的快照往返和持久化故障验证；文件仅写Library测试目录。</summary>
    public static class RepairPersistenceValidation
    {
        private static readonly List<string> Lines = new List<string>();
        private static NewtonsoftJsonHelper Json => new NewtonsoftJsonHelper();

        [MenuItem("Game Framework/EverlightTales/Save/验证快照与故障恢复")]
        public static void Run()
        {
            if (!Application.isPlaying || !GameConfigRuntime.EnsureLoaded())
                throw new InvalidOperationException("从Launch进入Play Mode并等待配置加载后运行。");
            Lines.Clear();
            try
            {
                SnapshotRoundTrip();
                FileProtocol();
                JournalReplay();
                Lines.Add("PASS ALL");
            }
            catch (Exception exception) { Lines.Add("FAIL " + exception); throw; }
            finally
            {
                Directory.CreateDirectory("Library/RepairPersistenceValidation");
                File.WriteAllLines("Library/RepairPersistenceValidation/protocol.txt", Lines);
                Debug.Log("[RepairPersistence][Validation] " + string.Join("; ", Lines));
            }
        }

        private static void SnapshotRoundTrip()
        {
            RedShoeLevel shoe = RedShoeLevelBuilder.Build(RedShoeLevelConfig.Tutorial());
            var rng = new RandomService(413);
            rng.NextInt(0, 19); rng.NextFloat();
            var buffs = new BuffSet(); buffs.Add(BuffCatalog.Get("BF-014"));
            var settle = new SettleState(HexDirection.D3);
            var entity = BoardEntity.Part(shoe.Board.AllocateEntityId(), PartType.StorageStomach);
            foreach (HexCoord cell in shoe.Board.EnumerateNormal())
                if (!shoe.Board.IsOccupied(cell)) { Check(shoe.Board.Place(entity, cell) == PlaceResult.Ok, "rich entity placed"); break; }
            entity.AddCharge(2); entity.SetCopy(); entity.ReleaseMode = true; entity.DischargeMode = true;
            entity.AddWater(2); entity.AddLoad(1); entity.SetAcceptsLoad(); entity.SetAcceptsCalibration();
            entity.SetDisguiseLayers(2); entity.SetSoundEndpoint("test-endpoint"); entity.SetFuseDueTap(8);
            var stored = BoardEntity.Part(shoe.Board.AllocateEntityId(), PartCatalog.All()[1].Type);
            bool placed = false;
            foreach (HexCoord cell in shoe.Board.EnumerateNormal())
            {
                if (shoe.Board.IsOccupied(cell)) continue;
                if (!placed) { shoe.Board.Place(stored, cell); placed = true; }
                else { shoe.Board.Move(stored, cell, MoveSource.Arm); break; }
            }
            shoe.Board.Remove(stored);
            entity.Store(stored);
            var timer = BoardEntity.Part(shoe.Board.AllocateEntityId(), PartType.BlastCoil, "P-003-F02");
            timer.SetFuseDueTap(8);
            foreach (HexCoord cell in shoe.Board.EnumerateNormal())
                if (!shoe.Board.IsOccupied(cell)) { shoe.Board.Place(timer, cell); break; }
            shoe.Board.SetTerrain(entity.Coord, (TerrainKind)Enum.GetValues(typeof(TerrainKind)).GetValue(0));
            shoe.Board.SetEndpointLabel(entity.Coord, 801);
            var region = new AnomalyRegion(AnomalyType.OverloadField, new[] { entity.Coord });
            region.AddHeat(); region.AddHeat(); shoe.Board.RegisterAnomaly(region);
            shoe.RedShoe.Coord = shoe.Config.RedShoe.GuideCells[0];
            shoe.RedShoe.RestoreCountedGuides(new[] { shoe.Config.RedShoe.GuideCells[0] });
            shoe.RedShoe.DiversionProgress = 1; shoe.RedShoe.RestraintLoad = 1;
            shoe.Level.AdvanceRound();
            shoe.Level.Session.Score = 40; shoe.Level.Session.ArmMoves = 2;
            shoe.Level.Session.PublicRepairEnergy = 3; shoe.Level.Session.TapSerial = 3;
            foreach (SpecialGoalState goal in shoe.Level.Round.Goals) goal.Progress(1);
            RepairAttemptSave saved = RepairSaveService.Capture("attempt", "REDSHOE", shoe.Board, shoe.Level, rng, buffs, settle, shoe.RedShoe);
            var decoded = Json.ToObject<RepairAttemptSave>(Json.ToJson(saved));
            RestoredAttempt restored = RepairSaveService.Restore(decoded, shoe.Board.BoardRadius, shoe.Level.Config);
            var again = RepairSaveService.Capture("attempt", "REDSHOE", restored.Board, restored.Level,
                restored.Rng, restored.Buffs, restored.Settle, restored.RedShoe);
            Check(Json.ToJson(saved) == Json.ToJson(again), "complete snapshot JSON round trip including stored entities and shoe");
            Check(rng.NextInt(0, 9999) == restored.Rng.NextInt(0, 9999), "RNG continuation");
            var restoredShoe = new RedShoeLevel(restored.Board, restored.RedShoe, restored.Level, shoe.Config);
            var originalGame = new BoardGame(shoe.Board, settle, shoe.Level.Session, shoe.Level, shoe);
            var restoredGame = new BoardGame(restored.Board, restored.Settle, restored.Level.Session, restored.Level, restoredShoe);
            originalGame.Bonuses = BuffEffectService.Resolve(buffs); restoredGame.Bonuses = BuffEffectService.Resolve(restored.Buffs);
            originalGame.Tap(); restoredGame.Tap();
            var originalAfter = RepairSaveService.Capture("attempt", "REDSHOE", shoe.Board, shoe.Level, rng, buffs, settle, shoe.RedShoe);
            var restoredAfter = RepairSaveService.Capture("attempt", "REDSHOE", restored.Board, restored.Level,
                restored.Rng, restored.Buffs, restored.Settle, restored.RedShoe);
            Check(Json.ToJson(originalAfter) == Json.ToJson(restoredAfter), "same next tap produces identical complete state");
            var invalid = Json.ToObject<RepairAttemptSave>(Json.ToJson(saved));
            invalid.Entities[1].Id = invalid.Entities[0].Id;
            Reject(() => RepairSaveService.Restore(invalid, saved.BoardRadius, shoe.Level.Config), "duplicate entity rejected");
            invalid = Json.ToObject<RepairAttemptSave>(Json.ToJson(saved));
            invalid.Buffs.Add(new SavedBuff { Id = "unknown-buff", Stacks = 1 });
            Reject(() => RepairSaveService.Restore(invalid, saved.BoardRadius, shoe.Level.Config), "unknown buff rejected");
            invalid = Json.ToObject<RepairAttemptSave>(Json.ToJson(saved));
            invalid.Entities[0].Coord = new HexCoord(999, 999);
            Reject(() => RepairSaveService.Restore(invalid, saved.BoardRadius, shoe.Level.Config), "invalid placement rejected");
            invalid = Json.ToObject<RepairAttemptSave>(Json.ToJson(saved)); invalid.Version++;
            Reject(() => RepairSaveService.Restore(invalid, saved.BoardRadius, shoe.Level.Config), "future schema rejected");
        }

        private static void FileProtocol()
        {
            var identity = Identity();
            string root = Path.GetFullPath("Library/RepairPersistenceValidation/files-" + Guid.NewGuid().ToString("N"));
            var store = new SlotSaveStore(root);
            var record = Record(identity, 0);
            Check(store.WriteCheckpoint(record).Succeeded, "initial atomic checkpoint");
            string firstPath = Path.Combine(store.DirectoryFor(identity), "attempt.json");
            string firstDocument = File.ReadAllText(firstPath);
            using (var locked = new FileStream(firstPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                Check(store.ReadCheckpoint(identity, "rules").Status == PersistenceStatus.IoError, "read IO is distinguished from corrupt data");
            foreach (bool changeSlot in new[] { true, false })
            {
                var wrong = new AttemptIdentity { Slot = changeSlot ? 2 : identity.Slot,
                    GenerationId = changeSlot ? identity.GenerationId : Guid.NewGuid().ToString("N"),
                    AttemptId = identity.AttemptId, Sequence = 1 };
                Directory.CreateDirectory(store.DirectoryFor(wrong));
                File.WriteAllText(Path.Combine(store.DirectoryFor(wrong), "attempt.json"), firstDocument);
                Check(store.ReadCheckpoint(wrong, "rules").Status == PersistenceStatus.Corrupt,
                    changeSlot ? "wrong slot payload rejected" : "wrong generation payload rejected");
            }
            File.WriteAllText(firstPath, System.Text.RegularExpressions.Regex.Replace(firstDocument,
                @"""Version""\s*:\s*1", "\"Version\":2"));
            Check(store.ReadCheckpoint(identity, "rules").Status == PersistenceStatus.Incompatible, "future file envelope rejected");
            File.WriteAllText(firstPath, firstDocument);
            identity.Sequence = 2; record = Record(identity, 1);
            Check(store.WriteCheckpoint(record).Succeeded, "next checkpoint with backup");
            string path = Path.Combine(store.DirectoryFor(identity), "attempt.json");
            File.WriteAllText(path, "broken");
            var loaded = store.ReadCheckpoint(identity, "rules");
            Check(loaded.Status == PersistenceStatus.RecoveredBackup && loaded.Value.RoundIndex == 0,
                "corrupt main falls back to actual earlier round");
            Check(store.ReadCheckpoint(identity, "different-rules").Status == PersistenceStatus.Incompatible, "configuration mismatch rejected");
            var other = Identity(); other.Slot = identity.Slot; other.GenerationId = identity.GenerationId;
            Check(!store.ReadCheckpoint(other, "rules").Succeeded, "other attempt backup rejected");
            Check(store.WriteCheckpoint(record).Succeeded, "replace corrupt main without destroying valid backup");
            foreach (string stage in new[] { "before-write", "after-flush", "before-replace", "after-replace" })
            {
                store.FaultInjector = point => { if (point == stage) throw new IOException("injected-" + stage); };
                var attempt = Record(identity, 2);
                Check(!store.WriteCheckpoint(attempt).Succeeded, "write failure reported " + stage);
                store.FaultInjector = null;
                Check(store.ReadCheckpoint(identity, "rules").Succeeded, "valid checkpoint preserved " + stage);
            }
            File.WriteAllText(path, "bad"); File.WriteAllText(path + ".bak", "bad");
            Check(store.ReadCheckpoint(identity, "rules").Status == PersistenceStatus.Corrupt, "both corrupted classified");
            store.Quarantine(identity, "attempt", "test");
            Check(!store.HasFiles(identity, "attempt"), "corrupt pair quarantined");
            var journal = Journal(identity);
            Check(store.WriteJournal(journal).Succeeded, "journal primary and redundant copy durable");
            File.WriteAllText(Path.Combine(store.DirectoryFor(identity), "settlement.json"), "bad");
            Check(store.ReadJournal(identity).Status == PersistenceStatus.RecoveredBackup, "journal backup recovery");
            journal.After.Values[0].Integer++;
            Check(store.WriteJournal(journal).Status == PersistenceStatus.Conflict, "prepared result immutable");
        }

        private static void JournalReplay()
        {
            SettlementJournal journal = Journal(Identity());
            var world = new MemoryWorld();
            world.FailAfterFirstValue = true;
            try { SettlementJournalCommit.Apply(journal, world); } catch (IOException) { }
            Check(world.Revision == 0, "partial world write has no receipt");
            world.FailAfterFirstValue = false;
            Check(SettlementJournalCommit.Apply(journal, world) == PersistenceStatus.Success, "partial write replay finishes");
            Check(SettlementJournalCommit.Apply(journal, world) == PersistenceStatus.Success
                && world.Values["fee"] == 40 && world.Values["cells"] == 2 && world.Revision == 1, "replay never accumulates");
            world.Values["fee"] = 0;
            Check(SettlementJournalCommit.Apply(journal, world) == PersistenceStatus.Success && world.Values["fee"] == 40,
                "receipt with incomplete fields repaired");
            world.CommitReceipt(2, "newer-attempt");
            Check(SettlementJournalCommit.Apply(journal, world) == PersistenceStatus.Conflict && world.Revision == 2,
                "stale journal cannot overwrite newer revision");
        }

        private static AttemptIdentity Identity() => new AttemptIdentity
        { Slot = 3, GenerationId = Guid.NewGuid().ToString("N"), AttemptId = Guid.NewGuid().ToString("N"), Sequence = 1 };

        private static CheckpointRecord Record(AttemptIdentity identity, int round) => new CheckpointRecord
        { Identity = identity, EventId = "test", LevelKey = "test", ConfigFingerprint = "rules",
            RoundIndex = round, StateJson = "{}", TimeCost = 2, StartedDay = 1, StartedPeriod = 0 };

        private static SettlementJournal Journal(AttemptIdentity identity) => new SettlementJournal
        {
            Identity = identity, ExpectedRevision = 0, TargetRevision = 1, Outcome = 0,
            Before = new WorldImage { Values = new List<WorldValue> {
                new WorldValue { Key = "fee", Kind = WorldValueKind.Integer, Integer = 0 },
                new WorldValue { Key = "cells", Kind = WorldValueKind.Integer, Integer = 4 } } },
            After = new WorldImage { Values = new List<WorldValue> {
                new WorldValue { Key = "fee", Kind = WorldValueKind.Integer, Integer = 40 },
                new WorldValue { Key = "cells", Kind = WorldValueKind.Integer, Integer = 2 } } }
        };

        private sealed class MemoryWorld : IWorldImageStore
        {
            public readonly Dictionary<string, int> Values = new Dictionary<string, int>();
            public bool FailAfterFirstValue;
            public long Revision { get; private set; }
            public string AppliedAttemptId { get; private set; }
            public string AppliedImageHash { get; private set; }
            public void ValidateImage(WorldImage image) { if (image.Values.Count != 2) throw new ArgumentException(); }
            public void Apply(WorldImage image)
            {
                int count = 0;
                foreach (WorldValue value in image.Values)
                {
                    Values[value.Key] = value.Integer;
                    if (++count == 1 && FailAfterFirstValue) throw new IOException("partial write");
                }
            }
            public void CommitReceipt(long revision, string id)
            {
                Revision = revision; AppliedAttemptId = id;
                var image = new WorldImage();
                foreach (var pair in Values) image.Values.Add(new WorldValue { Key = pair.Key, Kind = WorldValueKind.Integer, Integer = pair.Value });
                AppliedImageHash = SettlementJournalCommit.ImageHash(image);
            }
            public bool Matches(WorldImage image)
            {
                foreach (WorldValue value in image.Values)
                    if (!Values.TryGetValue(value.Key, out int actual) || actual != value.Integer) return false;
                return true;
            }
        }

        private static void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException(name);
            Lines.Add("PASS " + name);
        }

        private static void Reject(Action action, string name)
        {
            bool rejected = false;
            try { action(); } catch (ArgumentException) { rejected = true; }
            Check(rejected, name);
        }
    }
}
