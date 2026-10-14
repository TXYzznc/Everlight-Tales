using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    public sealed class PersistenceResult<T>
    {
        public PersistenceStatus Status;
        public T Value;
        public string Diagnostic;
        public bool Succeeded => Status == PersistenceStatus.Success || Status == PersistenceStatus.RecoveredBackup;
    }

    /// <summary>分槽JSON文件协议。根路径由拥有者注入，测试不得使用玩家目录。</summary>
    public sealed class SlotSaveStore
    {
        private sealed class Document
        {
            public int Version;
            public string Kind;
            public AttemptIdentity Identity;
            public string SavedAtUtc;
            public string Payload;
            public string Hash;
        }

        public const int Version = 1;
        private const int MaxFileBytes = 8 * 1024 * 1024;
        private readonly string _root;
        /// <summary>测试专用故障注入点；生产中保持null。</summary>
        public Action<string> FaultInjector { get; set; }

        public SlotSaveStore(string root)
        {
            _root = Path.GetFullPath(root ?? throw new ArgumentNullException(nameof(root)));
        }

        public static string Hash(string text)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "");
        }

        public string DirectoryFor(AttemptIdentity identity)
        {
            identity.Validate();
            return Path.Combine(_root, "slots", identity.Slot.ToString(), identity.GenerationId);
        }

        public bool HasFiles(AttemptIdentity identity, string kind)
        {
            string path = PathFor(identity, kind);
            return File.Exists(path) || File.Exists(path + ".bak");
        }

        public PersistenceResult<CheckpointRecord> ReadCheckpoint(AttemptIdentity identity, string fingerprint, Action<CheckpointRecord> validateState = null)
        {
            return Read<CheckpointRecord>(identity, "attempt", record =>
            {
                if (record?.Identity == null || !record.Identity.SameAttempt(identity)
                    || record.Identity.Sequence < 1 || string.IsNullOrEmpty(record.StateJson)
                    || string.IsNullOrEmpty(record.EventId) || string.IsNullOrEmpty(record.LevelKey)
                    || record.RoundIndex < 0 || record.TimeCost < 0)
                    throw new ArgumentException("Invalid checkpoint header.");
                if (fingerprint != null && record.ConfigFingerprint != fingerprint) throw new NotSupportedException("Configuration changed.");
                validateState?.Invoke(record);
            });
        }

        public PersistenceResult<SettlementJournal> ReadJournal(AttemptIdentity identity)
        {
            return Read<SettlementJournal>(identity, "settlement", journal =>
            {
                if (journal == null) throw new ArgumentException("Missing journal.");
                journal.Validate();
                if (!journal.Identity.SameAttempt(identity)) throw new ArgumentException("Journal identity mismatch.");
            });
        }

        public PersistenceResult<CheckpointRecord> WriteCheckpoint(CheckpointRecord record)
        {
            return Write(record.Identity, "attempt", record, false);
        }

        public PersistenceResult<SettlementJournal> WriteJournal(SettlementJournal journal)
        {
            journal.Validate();
            var existing = ReadJournal(journal.Identity);
            if (existing.Succeeded && Json.ToJson(existing.Value) != Json.ToJson(journal))
                return new PersistenceResult<SettlementJournal> { Status = PersistenceStatus.Conflict,
                    Diagnostic = "A prepared settlement result is immutable." };
            return Write(journal.Identity, "settlement", journal, true);
        }

        public void Cleanup(AttemptIdentity identity)
        {
            // 日志最后删除；任一步失败时凭据/日志仍可阻止旧盘面复活。
            DeletePair(PathFor(identity, "attempt"));
            DeletePair(PathFor(identity, "settlement"));
        }

        public void DeleteSlot(int slot)
        {
            if (slot < 1 || slot > 3) throw new ArgumentOutOfRangeException(nameof(slot));
            string slotsRoot = Path.GetFullPath(Path.Combine(_root, "slots")) + Path.DirectorySeparatorChar;
            string target = Path.GetFullPath(Path.Combine(slotsRoot, slot.ToString()));
            if (!target.StartsWith(slotsRoot, StringComparison.OrdinalIgnoreCase)) throw new IOException("Unsafe slot path.");
            if (!Directory.Exists(target)) return;
            ValidateDirectoryTree(target);
            Directory.Delete(target, true);
        }

        private static void ValidateDirectoryTree(string directory)
        {
            if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Linked save directory refused.");
            foreach (string child in Directory.EnumerateDirectories(directory)) ValidateDirectoryTree(child);
        }

        public void Quarantine(AttemptIdentity identity, string kind, string reason)
        {
            string path = PathFor(identity, kind);
            foreach (string suffix in new[] { "", ".bak", ".tmp" })
            {
                if (!File.Exists(path + suffix)) continue;
                string quarantine = Path.Combine(_root, "slots", identity.Slot.ToString(), "quarantine");
                Directory.CreateDirectory(quarantine);
                File.Move(path + suffix, Path.Combine(quarantine,
                    identity.GenerationId + "-" + identity.AttemptId + "-" + kind + suffix + "-" + Guid.NewGuid().ToString("N")));
            }
        }

        private PersistenceResult<T> Write<T>(AttemptIdentity identity, string kind, T value, bool redundant)
        {
            try
            {
                identity.Validate();
                string path = PathFor(identity, kind);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var document = new Document
                {
                    Version = Version, Kind = kind, Identity = identity,
                    SavedAtUtc = DateTime.UtcNow.ToString("O"), Payload = Json.ToJson(value)
                };
                document.Hash = Digest(document);
                string text = Json.ToJson(document);
                if (Encoding.UTF8.GetByteCount(text) > MaxFileBytes) throw new ArgumentException("Save exceeds size limit.");
                WriteValidated(path + ".tmp", text, identity, kind);
                FaultInjector?.Invoke("before-replace");
                // 不允许坏主文件替换上一份有效备份。
                if (File.Exists(path) && ReadDocument(path, identity, kind).Succeeded)
                    File.Replace(path + ".tmp", path, path + ".bak");
                else if (File.Exists(path))
                {
                    string bad = path + ".corrupt-" + Guid.NewGuid().ToString("N");
                    File.Move(path, bad);
                    File.Move(path + ".tmp", path);
                }
                else File.Move(path + ".tmp", path);
                FaultInjector?.Invoke("after-replace");
                if (redundant)
                {
                    WriteValidated(path + ".bak.tmp", text, identity, kind);
                    if (File.Exists(path + ".bak")) File.Replace(path + ".bak.tmp", path + ".bak", null);
                    else File.Move(path + ".bak.tmp", path + ".bak");
                    FaultInjector?.Invoke("after-redundancy");
                }
                var verification = ReadDocument(path, identity, kind);
                if (!verification.Succeeded) throw new IOException("Written save failed validation.");
                return new PersistenceResult<T> { Status = PersistenceStatus.Success, Value = value };
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
            {
                return new PersistenceResult<T> { Status = exception is ArgumentException ? PersistenceStatus.Corrupt : PersistenceStatus.IoError,
                    Diagnostic = exception.GetType().Name + ": " + exception.Message };
            }
        }

        private void WriteValidated(string path, string text, AttemptIdentity identity, string kind)
        {
            FaultInjector?.Invoke("before-write");
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            FaultInjector?.Invoke("after-flush");
            if (!ReadDocument(path, identity, kind).Succeeded) throw new IOException("Temporary save failed validation.");
        }

        private PersistenceResult<T> Read<T>(AttemptIdentity identity, string kind, Action<T> validate)
        {
            string path = PathFor(identity, kind);
            PersistenceStatus failure = PersistenceStatus.NotFound;
            string diagnostic = null;
            foreach (string suffix in new[] { "", ".bak" })
            {
                var result = ReadDocument(path + suffix, identity, kind);
                if (!result.Succeeded)
                {
                    if (result.Status != PersistenceStatus.NotFound) { failure = result.Status; diagnostic = result.Diagnostic; }
                    continue;
                }
                try
                {
                    T value = Json.ToObject<T>(result.Value.Payload);
                    validate(value);
                    return new PersistenceResult<T>
                    {
                        Status = suffix.Length == 0 ? PersistenceStatus.Success : PersistenceStatus.RecoveredBackup, Value = value
                    };
                }
                catch (NotSupportedException e) { failure = PersistenceStatus.Incompatible; diagnostic = e.Message; }
                catch (Exception e) { failure = PersistenceStatus.Corrupt; diagnostic = e.GetType().Name + ": " + e.Message; }
            }
            return new PersistenceResult<T> { Status = failure, Diagnostic = diagnostic };
        }

        private PersistenceResult<Document> ReadDocument(string path, AttemptIdentity identity, string kind)
        {
            try
            {
                if (!File.Exists(path)) return new PersistenceResult<Document> { Status = PersistenceStatus.NotFound };
                if (new FileInfo(path).Length > MaxFileBytes) throw new ArgumentException("Oversized save.");
                Document document = Json.ToObject<Document>(File.ReadAllText(path, Encoding.UTF8));
                if (document?.Identity == null) throw new ArgumentException("Missing save identity.");
                document.Identity.Validate();
                if (!document.Identity.SameAttempt(identity) || document.Kind != kind) throw new ArgumentException("Save identity mismatch.");
                if (document.Version != Version) return new PersistenceResult<Document> { Status = PersistenceStatus.Incompatible };
                if (string.IsNullOrEmpty(document.Payload) || document.Hash != Digest(document)) throw new ArgumentException("Save checksum mismatch.");
                return new PersistenceResult<Document> { Status = PersistenceStatus.Success, Value = document };
            }
            catch (IOException e) { return new PersistenceResult<Document> { Status = PersistenceStatus.IoError, Diagnostic = e.Message }; }
            catch (UnauthorizedAccessException e) { return new PersistenceResult<Document> { Status = PersistenceStatus.IoError, Diagnostic = e.Message }; }
            catch (Exception e) { return new PersistenceResult<Document> { Status = PersistenceStatus.Corrupt, Diagnostic = e.GetType().Name + ": " + e.Message }; }
        }

        private static string Digest(Document document)
        {
            return Hash(document.Version + "\n" + document.Kind + "\n" + Json.ToJson(document.Identity)
                + "\n" + document.SavedAtUtc + "\n" + document.Payload);
        }

        private string PathFor(AttemptIdentity identity, string kind)
        {
            if (kind != "attempt" && kind != "settlement") throw new ArgumentException("Unknown save kind.");
            return Path.Combine(DirectoryFor(identity), kind + ".json");
        }

        private void DeletePair(string path)
        {
            foreach (string suffix in new[] { "", ".bak", ".tmp", ".bak.tmp" })
            {
                FaultInjector?.Invoke("before-delete");
                if (File.Exists(path + suffix)) File.Delete(path + suffix);
            }
        }

        private static NewtonsoftJsonHelper Json => new NewtonsoftJsonHelper();
    }

    public static class SettlementJournalCommit
    {
        public static PersistenceStatus Apply(SettlementJournal journal, IWorldImageStore world)
        {
            journal.Validate();
            world.ValidateImage(journal.Before);
            world.ValidateImage(journal.After);
            if (world.Revision == journal.TargetRevision && world.AppliedAttemptId == journal.Identity.AttemptId)
            {
                if (!world.Matches(journal.After) || world.AppliedImageHash != ImageHash(journal.After))
                {
                    world.Apply(journal.After);
                    world.CommitReceipt(journal.TargetRevision, journal.Identity.AttemptId);
                }
                return world.Matches(journal.After) && world.AppliedImageHash == ImageHash(journal.After)
                    ? PersistenceStatus.Success : PersistenceStatus.Conflict;
            }
            if (world.Revision != journal.ExpectedRevision) return PersistenceStatus.Conflict;
            // 此分支允许上次部分写入；绝对映像可安全重复，不重新运行奖励或时间逻辑。
            world.Apply(journal.After);
            if (!world.Matches(journal.After)) throw new IOException("World image verification failed.");
            world.CommitReceipt(journal.TargetRevision, journal.Identity.AttemptId);
            if (world.Revision != journal.TargetRevision || world.AppliedAttemptId != journal.Identity.AttemptId
                || world.AppliedImageHash != ImageHash(journal.After) || !world.Matches(journal.After))
                throw new IOException("World receipt verification failed.");
            return PersistenceStatus.Success;
        }

        public static string ImageHash(WorldImage image)
        {
            var normalized = new System.Collections.Generic.List<WorldValue>();
            foreach (WorldValue value in image.Values)
                normalized.Add(new WorldValue { Key = value.Key, Kind = value.Kind,
                    Integer = value.Kind == WorldValueKind.Integer ? value.Integer : 0,
                    Text = value.Kind == WorldValueKind.String ? value.Text : null });
            normalized.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return SlotSaveStore.Hash(new NewtonsoftJsonHelper().ToJson(normalized));
        }
    }
}
