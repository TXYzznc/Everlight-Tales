using System;
using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;

namespace Everlight.Tales.UI
{
    public sealed partial class WorldSession
    {
        private sealed class SavedSupply
        {
            public string InstanceId;
            public string TemplateId;
            public bool Night;
            public bool Processed;
        }

        private sealed class RuntimeSave
        {
            public int Version = 1;
            public int Seed;
            public int RandomConsumed;
            public uint RandomState;
            public bool OpeningDone;
            public int PendingArmMoves;
            public string TrackedTaskId;
            public List<SavedSupply> Supply = new List<SavedSupply>();
            public int SupplyDay;
            public bool SupplyNight;
            public int SupplyEpoch;
            public List<SavedBuff> Buffs = new List<SavedBuff>();
            public List<SavedRewardChoice> Rewards;
            public string Settlement;
            public string SettlementEventId;
        }

        private void CaptureRuntime(WorldImage image)
        {
            var runtime = new RuntimeSave
            {
                Seed = Rng.Seed, RandomConsumed = Rng.ConsumedCount, RandomState = Rng.State,
                OpeningDone = OpeningDone, PendingArmMoves = PendingArmMoves, TrackedTaskId = TrackedTaskId,
                SupplyDay = _supplyDay, SupplyNight = _supplyNight, SupplyEpoch = _supplyEpoch
            };
            foreach (SupplyInstance instance in _supply)
                runtime.Supply.Add(new SavedSupply
                {
                    InstanceId = instance.InstanceId, TemplateId = instance.Template.TemplateId,
                    Night = instance.Night, Processed = instance.Processed
                });
            foreach (BuffState buff in HeldBuffs.All)
                runtime.Buffs.Add(new SavedBuff { Id = buff.Config.Id, Stacks = buff.Stacks });
            if (HasPendingSettlementReward)
            {
                runtime.Rewards = new List<SavedRewardChoice>();
                foreach (RewardOption option in _terminalRewards)
                    runtime.Rewards.Add(new SavedRewardChoice { Kind = (int)option.Kind, Id = option.Id });
            }
            runtime.Settlement = LastSettlement == null ? null : PersistenceJson.ToJson(CreateSettlementDisplay());
            runtime.SettlementEventId = _lastSettlementEventId;
            PlayerPrefsWorldImageStore.AddString(image, SlotKey("et.world.runtime", Slot), PersistenceJson.ToJson(runtime));
            PlayerPrefsWorldImageStore.AddString(image, SlotKey("et.world.ledger", Slot), PersistenceJson.ToJson(World.Ledger));
            PlayerPrefsWorldImageStore.AddString(image, SlotKey("et.world.homeState", Slot), World.HomeState);
        }

        private void RestoreRuntime(WorldImage image)
        {
            string json = PlayerPrefsWorldImageStore.ReadString(image, SlotKey("et.world.runtime", Slot), "");
            if (string.IsNullOrEmpty(json))
            {
                // 旧档没有供给/RNG载荷，仅升级时按既有规则生成一次。
                RefreshSupply();
            }
            else
            {
                RuntimeSave runtime = PersistenceJson.ToObject<RuntimeSave>(json);
                if (runtime == null || runtime.Version != 1 || runtime.Supply == null || runtime.PendingArmMoves < 0)
                    throw new ArgumentException("Invalid world runtime save.");
                Rng.RestoreState(runtime.Seed, runtime.RandomConsumed, runtime.RandomState);
                OpeningDone = runtime.OpeningDone; PendingArmMoves = runtime.PendingArmMoves;
                TrackedTaskId = runtime.TrackedTaskId; _supply.Clear();
                _supplyDay = runtime.SupplyDay > 0 ? runtime.SupplyDay : World.Day;
                _supplyNight = runtime.SupplyDay > 0 ? runtime.SupplyNight : TimePeriod.IsNight(World.Period);
                _supplyEpoch = runtime.SupplyEpoch > 0 ? runtime.SupplyEpoch : 1;
                HeldBuffs = new BuffSet();
                if (runtime.Buffs != null)
                    foreach (SavedBuff buff in runtime.Buffs)
                    {
                        BuffConfig config = BuffCatalog.Get(buff.Id);
                        if (config == null || buff.Stacks < 1 || buff.Stacks > config.MaxStacks)
                            throw new ArgumentException("Invalid saved world buff.");
                        for (int i = 0; i < buff.Stacks; i++)
                            if (HeldBuffs.Add(config) == null) throw new ArgumentException("Invalid saved world buff stacks.");
                    }
                _terminalRewards = null;
                if (runtime.Rewards != null)
                {
                    var options = new List<RewardOption>(); var rewardIds = new HashSet<string>();
                    foreach (SavedRewardChoice choice in runtime.Rewards)
                    {
                        if (!rewardIds.Add(choice.Kind + ":" + choice.Id)) throw new ArgumentException("Duplicate saved reward.");
                        options.Add(RestoreRewardChoice(choice));
                    }
                    if (options.Count == 0) throw new ArgumentException("Empty pending rewards.");
                    _terminalRewards = options;
                }
                _lastSettlementEventId = runtime.SettlementEventId;
                if (!string.IsNullOrEmpty(runtime.Settlement))
                {
                    SettlementDisplay display = PersistenceJson.ToObject<SettlementDisplay>(runtime.Settlement);
                    LastSettlement = display.Transaction;
                    LastReward = display.HasReward ? new EventReward(display.RewardFee, display.Materials) : null;
                }
                var seen = new HashSet<string>();
                foreach (SavedSupply saved in runtime.Supply)
                {
                    SupplyCandidate template = null;
                    foreach (SupplyCandidate candidate in SupplyCatalog.FirstBatch())
                        if (candidate.TemplateId == saved.TemplateId) { template = candidate; break; }
                    if (template == null || string.IsNullOrEmpty(saved.InstanceId) || !seen.Add(saved.InstanceId))
                        throw new ArgumentException("Unknown or duplicate saved supply.");
                    _supply.Add(new SupplyInstance(saved.InstanceId, template, saved.Night) { Processed = saved.Processed });
                }
            }
            json = PlayerPrefsWorldImageStore.ReadString(image, SlotKey("et.world.ledger", Slot), "");
            if (!string.IsNullOrEmpty(json))
                World.Ledger = PersistenceJson.ToObject<List<LedgerEntry>>(json) ?? throw new ArgumentException("Invalid ledger.");
            World.HomeState = PlayerPrefsWorldImageStore.ReadString(image, SlotKey("et.world.homeState", Slot), "");
        }

        private static NewtonsoftJsonHelper PersistenceJson => new NewtonsoftJsonHelper();
    }
}
