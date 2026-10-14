using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    public enum PersistenceStatus { Success, NotFound, RecoveredBackup, Incompatible, Corrupt, IoError, Conflict }
    public enum WorldValueKind { Integer, String, Delete }

    public sealed class AttemptIdentity
    {
        public int Slot;
        public string GenerationId;
        public string AttemptId;
        public long Sequence;

        public void Validate()
        {
            if (Slot < 1 || Slot > 3 || !Guid.TryParseExact(GenerationId, "N", out _)
                || !Guid.TryParseExact(AttemptId, "N", out _) || Sequence < 1)
                throw new ArgumentException("Invalid attempt identity.");
        }

        public bool SameAttempt(AttemptIdentity other)
        {
            return other != null && Slot == other.Slot && GenerationId == other.GenerationId && AttemptId == other.AttemptId;
        }
    }

    public sealed class CheckpointRecord
    {
        public AttemptIdentity Identity;
        public string EventId;
        public string LevelKey;
        public string ConfigFingerprint;
        public int RoundIndex;
        public int TimeCost;
        public int StartedDay;
        public int StartedPeriod;
        public string FrozenPreparationJson;
        public string SupplyInstanceId;
        public string StateJson;
    }

    public sealed class WorldValue
    {
        public string Key;
        public WorldValueKind Kind;
        public int Integer;
        public string Text;
    }

    public sealed class WorldImage
    {
        public List<WorldValue> Values = new List<WorldValue>();
    }

    public sealed class SettlementJournal
    {
        public AttemptIdentity Identity;
        public long ExpectedRevision;
        public long TargetRevision;
        public int Outcome;
        public string Reason;
        public string DisplayJson;
        public WorldImage Before;
        public WorldImage After;

        public void Validate()
        {
            Identity?.Validate();
            if (Identity == null || ExpectedRevision < 0 || TargetRevision != ExpectedRevision + 1
                || Before?.Values == null || After?.Values == null || Outcome < 0 || Outcome > 2)
                throw new ArgumentException("Invalid settlement journal.");
            ValidateImage(Before); ValidateImage(After);
        }

        private static void ValidateImage(WorldImage image)
        {
            var keys = new HashSet<string>();
            foreach (WorldValue value in image.Values)
                if (value == null || string.IsNullOrEmpty(value.Key) || !keys.Add(value.Key)
                    || !Enum.IsDefined(typeof(WorldValueKind), value.Kind)
                    || (value.Kind == WorldValueKind.String && value.Text == null))
                    throw new ArgumentException("Invalid world image.");
        }
    }

    public interface IWorldImageStore
    {
        long Revision { get; }
        string AppliedAttemptId { get; }
        string AppliedImageHash { get; }
        void ValidateImage(WorldImage image);
        void Apply(WorldImage image);
        void CommitReceipt(long revision, string attemptId);
        bool Matches(WorldImage image);
    }
}
