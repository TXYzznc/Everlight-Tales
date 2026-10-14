using System;
using System.Collections.Generic;
using System.IO;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>显式世界键白名单；日志不能写任意PlayerPrefs键。</summary>
    public sealed class PlayerPrefsWorldImageStore : IWorldImageStore
    {
        private readonly int _slot;
        private readonly string _generation;
        public Action<string> FaultInjector { get; set; }
        private sealed class Receipt
        {
            public long Revision;
            public string AttemptId;
            public string ImageHash;
        }
        private static readonly string[] IntegerKeys =
        {
            "day", "period", "remainingCells", "repairFee", "batch", "tutorial", "jobs", "tutorialStage"
        };
        private static readonly string[] StringKeys =
        {
            "owned", "known", "materials", "blueprints", "forms", "currentForms", "tasks", "display",
            "firstCase", "places", "runtime", "ledger", "homeState"
        };

        public PlayerPrefsWorldImageStore(int slot, string generation)
        {
            if (slot < 1 || slot > 3 || !Guid.TryParseExact(generation, "N", out _)) throw new ArgumentException("Invalid world identity.");
            _slot = slot; _generation = generation;
        }

        public static string Key(int slot, string suffix) => SaveSlotService.SlotKey("et.world." + suffix, slot);
        public static IEnumerable<string> EnumerateKeys(int slot, bool includeIdentity)
        {
            foreach (string key in IntegerKeys) yield return Key(slot, key);
            foreach (string key in StringKeys) yield return Key(slot, key);
            yield return "et.world.meta." + slot;
            if (includeIdentity)
                foreach (string key in new[] { "generation", "commit", "revision", "receipt", "receiptHash", "activeAttempt", "deleting" })
                    yield return Key(slot, key);
        }
        public static bool IsIntegerKey(int slot, string key)
        {
            foreach (string suffix in IntegerKeys) if (key == Key(slot, suffix)) return true;
            return key == Key(slot, "deleting");
        }
        public long Revision => ReadReceipt().Revision;
        public string AppliedAttemptId => ReadReceipt().AttemptId;
        public string AppliedImageHash => ReadReceipt().ImageHash;

        private Receipt ReadReceipt()
        {
            string json = PlayerPrefs.GetString(Key(_slot, "commit"), "");
            if (string.IsNullOrEmpty(json))
                return new Receipt
                {
                    Revision = long.TryParse(PlayerPrefs.GetString(Key(_slot, "revision"), "0"), out long revision) ? revision : -1,
                    AttemptId = PlayerPrefs.GetString(Key(_slot, "receipt"), ""),
                    ImageHash = PlayerPrefs.GetString(Key(_slot, "receiptHash"), "")
                };
            Receipt receipt = new NewtonsoftJsonHelper().ToObject<Receipt>(json);
            if (receipt == null || receipt.Revision < 0 || receipt.AttemptId == null || string.IsNullOrEmpty(receipt.ImageHash))
                throw new IOException("Invalid world commit receipt.");
            return receipt;
        }

        public void ValidateImage(WorldImage image)
        {
            if (image?.Values == null) throw new ArgumentException("Missing world image.");
            var allowed = new Dictionary<string, WorldValueKind>(StringComparer.Ordinal);
            foreach (string key in IntegerKeys) allowed.Add(Key(_slot, key), WorldValueKind.Integer);
            foreach (string key in StringKeys) allowed.Add(Key(_slot, key), WorldValueKind.String);
            allowed.Add("et.world.meta." + _slot, WorldValueKind.String);
            var seen = new HashSet<string>();
            foreach (WorldValue value in image.Values)
                if (value == null || !allowed.TryGetValue(value.Key ?? "", out WorldValueKind type)
                    || !seen.Add(value.Key) || (value.Kind != type && value.Kind != WorldValueKind.Delete))
                    throw new ArgumentException("Forbidden or mistyped world key.");
            // 映像必须含完整受控键集，不能用部分载荷冒充已验证结算。
            if (seen.Count != allowed.Count) throw new ArgumentException("Incomplete world image.");
        }

        private void CheckOwner()
        {
            if (PlayerPrefs.GetString(Key(_slot, "generation"), "") != _generation
                || PlayerPrefs.GetInt(Key(_slot, "deleting"), 0) != 0)
                throw new IOException("World generation changed.");
        }

        public void Apply(WorldImage image)
        {
            CheckOwner(); ValidateImage(image);
            foreach (WorldValue value in image.Values)
            {
                if (value.Kind == WorldValueKind.Integer) PlayerPrefs.SetInt(value.Key, value.Integer);
                else if (value.Kind == WorldValueKind.String) PlayerPrefs.SetString(value.Key, value.Text);
                else PlayerPrefs.DeleteKey(value.Key);
                FaultInjector?.Invoke("after-world-value");
            }
            PlayerPrefs.Save();
            FaultInjector?.Invoke("after-world-save");
        }

        public void CommitReceipt(long revision, string attemptId)
        {
            CheckOwner();
            FaultInjector?.Invoke("before-receipt");
            var receipt = new Receipt { Revision = revision, AttemptId = attemptId ?? "",
                ImageHash = SettlementJournalCommit.ImageHash(CapturePersisted(_slot)) };
            // 单个值包含完整凭据，避免修订/尝试/校验三个键间被中断。
            PlayerPrefs.SetString(Key(_slot, "commit"), new NewtonsoftJsonHelper().ToJson(receipt));
            PlayerPrefs.Save();
            FaultInjector?.Invoke("after-receipt");
        }

        public bool Matches(WorldImage image)
        {
            CheckOwner(); ValidateImage(image);
            foreach (WorldValue value in image.Values)
            {
                if (value.Kind == WorldValueKind.Delete) { if (PlayerPrefs.HasKey(value.Key)) return false; }
                else if (!PlayerPrefs.HasKey(value.Key)) return false;
                else if (value.Kind == WorldValueKind.Integer && PlayerPrefs.GetInt(value.Key) != value.Integer) return false;
                else if (value.Kind == WorldValueKind.String && PlayerPrefs.GetString(value.Key) != value.Text) return false;
            }
            return true;
        }

        public static int ReadInt(WorldImage image, string key, int fallback)
        {
            if (image == null) return PlayerPrefs.GetInt(key, fallback);
            foreach (WorldValue value in image.Values)
                if (value.Key == key) return value.Kind == WorldValueKind.Integer ? value.Integer : fallback;
            return fallback;
        }

        public static string ReadString(WorldImage image, string key, string fallback)
        {
            if (image == null) return PlayerPrefs.GetString(key, fallback);
            foreach (WorldValue value in image.Values)
                if (value.Key == key) return value.Kind == WorldValueKind.String ? value.Text : fallback;
            return fallback;
        }

        public static WorldImage CapturePersisted(int slot)
        {
            var image = new WorldImage();
            foreach (string key in IntegerKeys) AddPersisted(image, Key(slot, key), WorldValueKind.Integer);
            foreach (string key in StringKeys) AddPersisted(image, Key(slot, key), WorldValueKind.String);
            AddPersisted(image, "et.world.meta." + slot, WorldValueKind.String);
            return image;
        }

        private static void AddPersisted(WorldImage image, string key, WorldValueKind kind)
        {
            image.Values.Add(new WorldValue
            {
                Key = key, Kind = PlayerPrefs.HasKey(key) ? kind : WorldValueKind.Delete,
                Integer = kind == WorldValueKind.Integer ? PlayerPrefs.GetInt(key, 0) : 0,
                Text = kind == WorldValueKind.String ? PlayerPrefs.GetString(key, "") : null
            });
        }

        public static void AddInt(WorldImage image, string key, int value) =>
            image.Values.Add(new WorldValue { Key = key, Kind = WorldValueKind.Integer, Integer = value });
        public static void AddString(WorldImage image, string key, string value) =>
            image.Values.Add(new WorldValue { Key = key, Kind = WorldValueKind.String, Text = value ?? "" });
    }
}
