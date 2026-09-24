using Everlight.Tales.Data;
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

        private static readonly string[] AllKeys =
        {
            "et.world.day", "et.world.period", "et.world.repairFee", "et.world.batch",
            "et.world.tutorial", "et.world.owned", "et.world.known", "et.world.materials",
            "et.world.blueprints", "et.world.forms", "et.world.currentForms", "et.world.tasks",
            "et.world.display", "et.world.jobs", "et.world.tutorialStage",
        };

        public static string SlotKey(string key, int slot)
        {
            return slot <= 1 ? key : key.Replace("et.world.", "et.world." + slot + ".");
        }

        public static bool HasSlot(int slot)
        {
            return PlayerPrefs.HasKey(SlotKey(KeyDay, slot));
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
            foreach (string key in AllKeys)
            {
                PlayerPrefs.DeleteKey(SlotKey(key, slot));
            }

            PlayerPrefs.DeleteKey(KeyMetaPrefix + slot);
            PlayerPrefs.Save();
        }
    }
}
