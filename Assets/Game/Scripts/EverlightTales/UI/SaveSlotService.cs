using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using System;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>存档位摘要（标题页展示）。</summary>
    public sealed class SaveSlotInfo
    {
        public int Slot;
        public bool HasSave;
        public int Day;
        public string PeriodName;
        public int RepairFee;
        public string SavedAtUtc;

        public string Summary => HasSave
            ? "第 " + Day + " 天 · " + PeriodName + " · " + RepairFee + " 维修费"
            : "空存档位";
    }

    /// <summary>
    /// 多存档位服务（收尾批）：读/删 PlayerPrefs 的存档位摘要，落 UI 层（需触达 UnityEngine）。
    /// 键约定与 WorldSession 一致：slot&lt;=1 用原键 et.world.*，slot&gt;1 换成 et.world.{slot}.*。
    /// </summary>
    public static class SaveSlotService
    {
        public const int MaxSlots = 3;

        private const string KeyDay = "et.world.day";
        private const string KeyPeriod = "et.world.period";
        private const string KeyRepairFee = "et.world.repairFee";
        private const string KeyMetaPrefix = "et.world.meta.";

        public static string SlotKey(string key, int slot)
        {
            return slot <= 1 ? key : key.Replace("et.world.", "et.world." + slot + ".");
        }

        public static bool HasSlot(int slot)
        {
            ValidateSlot(slot);
            if (PlayerPrefs.GetInt(PlayerPrefsWorldImageStore.Key(slot, "deleting"), 0) != 0) return false;
            return PlayerPrefs.HasKey(SlotKey(KeyDay, slot));
        }

        public static string EnsureGeneration(int slot)
        {
            ValidateSlot(slot);
            if (PlayerPrefs.GetInt(PlayerPrefsWorldImageStore.Key(slot, "deleting"), 0) != 0) Delete(slot);
            string key = PlayerPrefsWorldImageStore.Key(slot, "generation");
            string generation = PlayerPrefs.GetString(key, "");
            if (!Guid.TryParseExact(generation, "N", out _))
            {
                generation = Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(key, generation); PlayerPrefs.Save();
            }
            return generation;
        }

        private static void ValidateSlot(int slot)
        {
            if (slot < 1 || slot > MaxSlots) throw new ArgumentOutOfRangeException(nameof(slot));
        }

        public static SaveSlotInfo Read(int slot)
        {
            var info = new SaveSlotInfo { Slot = slot };
            if (!HasSlot(slot))
            {
                return info;
            }

            info.HasSave = true;
            info.Day = PlayerPrefs.GetInt(SlotKey(KeyDay, slot), 1);
            info.PeriodName = TimePeriod.DisplayName((TimeOfDay)PlayerPrefs.GetInt(SlotKey(KeyPeriod, slot), 0));
            info.RepairFee = PlayerPrefs.GetInt(SlotKey(KeyRepairFee, slot), 0);
            info.SavedAtUtc = PlayerPrefs.GetString(KeyMetaPrefix + slot, "");
            return info;
        }

        public static void Delete(int slot)
        {
            ValidateSlot(slot);
            // 先持久失效世代和删除意图，中断后EnsureGeneration会继续清理。
            PlayerPrefs.SetString(PlayerPrefsWorldImageStore.Key(slot, "generation"), Guid.NewGuid().ToString("N"));
            PlayerPrefs.SetInt(PlayerPrefsWorldImageStore.Key(slot, "deleting"), 1);
            PlayerPrefs.Save();
            new SlotSaveStore(WorldSession.PersistenceRoot).DeleteSlot(slot);
            foreach (string key in PlayerPrefsWorldImageStore.EnumerateKeys(slot, true))
                if (key != PlayerPrefsWorldImageStore.Key(slot, "deleting")) PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            PlayerPrefs.DeleteKey(PlayerPrefsWorldImageStore.Key(slot, "deleting")); PlayerPrefs.Save();
        }
    }
}
