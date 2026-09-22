using System;
using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 世界会话（b43）：一天闭环的运行时编排器。持有世界状态／时间／随机源／开场服务／
    /// 当前盘面事件，提供新档／继续／开始事件／结算事件与 PlayerPrefs 最简持久化。
    /// 落在 UI 层（需同时触达 Board／Events／Meta／Data）；纯 C# 静态单例，非 MonoBehaviour。
    /// </summary>
    public sealed class WorldSession
    {
        public const int DemoSeed = 20260922;

        private const string KeyDay = "et.world.day";
        private const string KeyPeriod = "et.world.period";
        private const string KeyRepairFee = "et.world.repairFee";
        private const string KeyBatch = "et.world.batch";
        private const string KeyTutorial = "et.world.tutorial";
        private const string KeyOwned = "et.world.owned";

        public static WorldSession Current { get; private set; }

        public WorldState World { get; private set; }

        public TimeState Time { get; private set; }

        public RandomService Rng { get; private set; }

        public RedShoeIntroService Intro { get; private set; }

        public BuffSet HeldBuffs { get; private set; }

        /// <summary>本次进入是否为新档（未读到存档）。</summary>
        public bool IsNewGame { get; private set; }

        /// <summary>当前盘面事件（卷帘门 EV-N01）。</summary>
        public RepairEventInstance CurrentEvent { get; private set; }

        /// <summary>当前盘面事件的盘面配置。</summary>
        public LevelBoardConfig CurrentBoardConfig { get; private set; }

        /// <summary>最近一次结算事务结果。</summary>
        public SettlementTransactionResult LastSettlement { get; private set; }

        /// <summary>最近一次事件奖励（失败/撤退为 null）。</summary>
        public EventReward LastReward { get; private set; }

        private List<SupplyInstance> _supply = new List<SupplyInstance>();

        /// <summary>当前昼/夜的一批普通供给。</summary>
        public IReadOnlyList<SupplyInstance> Supply => _supply;

        /// <summary>新档：建世界 + 开局地图 + 初始零件 + 刷新供给。</summary>
        public static WorldSession NewGame(int seed)
        {
            var s = new WorldSession
            {
                World = new WorldState(),
                Time = new TimeState(1, TimeOfDay.Morning, TimeState.CellsPerPeriod),
                Rng = new RandomService(seed),
                Intro = new RedShoeIntroService(),
                HeldBuffs = new BuffSet(),
                IsNewGame = true,
            };
            s.RegisterMap();
            s.World.OwnedParts.AddRange(new[]
            {
                PartType.InertiaHammer,
                PartType.MeteringRatchet,
                PartType.BlastCoil,
                PartType.ReversalGear,
            });
            s.RefreshSupply();
            Current = s;
            return s;
        }

        /// <summary>有存档则恢复，无存档则新档。</summary>
        public static WorldSession LoadOrNew(int seed)
        {
            return HasSave() ? Restore(seed) : NewGame(seed);
        }

        /// <summary>把当前世界状态写 PlayerPrefs（最简持久化；完整 DTO 往返在 b22/b24 已验证）。</summary>
        public void Save()
        {
            PlayerPrefs.SetInt(KeyDay, World.Day);
            PlayerPrefs.SetInt(KeyPeriod, (int)World.Period);
            PlayerPrefs.SetInt(KeyRepairFee, World.RepairFee);
            PlayerPrefs.SetInt(KeyBatch, World.BatchNumber);
            PlayerPrefs.SetInt(KeyTutorial, World.TutorialComplete ? 1 : 0);

            var owned = new List<string>();
            foreach (PartType part in World.OwnedParts)
            {
                owned.Add(((int)part).ToString());
            }

            PlayerPrefs.SetString(KeyOwned, string.Join(",", owned));
            PlayerPrefs.Save();
        }

        /// <summary>开始卷帘门事件（EV-N01）：构建盘面与关卡运行时，进入准备完成态。</summary>
        public RepairEventInstance StartRollerDoor()
        {
            LevelBoardConfig boardConfig = RollerDoorEvent.CreateBoardConfig();
            RepairEventConfig eventConfig = RollerDoorEvent.CreateConfig();

            var keyParts = new List<PartType>();
            foreach (KeyPieceConfig key in boardConfig.KeyPieces)
            {
                keyParts.Add(key.PartType);
            }

            CarrySelection carry = new CarrySelection(World.OwnedParts, boardConfig.BorrowedParts, keyParts);
            BoardState board = InitialBoardBuilder.Build(boardConfig, carry.SelectedParts(), Rng);

            CurrentBoardConfig = boardConfig;
            CurrentEvent = RepairEventShell.Begin(eventConfig, board);
            return CurrentEvent;
        }

        /// <summary>
        /// 结算当前事件：终局判定（由调用方传 success）→ 结算事务推进时间 →
        /// 世界结算发放奖励 → 写档 → 刷新供给。返回成功奖励（失败为 null）。
        /// </summary>
        public EventReward SettleEvent(bool success)
        {
            if (CurrentEvent == null)
            {
                return null;
            }

            EventResultKind outcome = success ? EventResultKind.Success : EventResultKind.Failure;
            EventReward reward = RepairEventShell.Resolve(CurrentEvent, outcome);

            var transaction = new SettlementTransaction();
            LastSettlement = transaction.Execute(
                Time,
                CurrentEvent.Config.TimeCost,
                success ? SettlementOutcomeKind.Success : SettlementOutcomeKind.Failure);

            var worldSettle = new WorldSettlementService();
            worldSettle.Settle(World, Time.Day, Time.Period, reward != null ? reward.RepairFee : 0);

            LastReward = reward;
            Save();
            RefreshSupply();
            return reward;
        }

        /// <summary>卷帘门门轴标记是否已被普通推移送入校正格。</summary>
        public bool IsRollerDoorGatePushedIn()
        {
            return CurrentEvent != null && RollerDoorEvent.IsGatePushedIn(CurrentEvent.Board);
        }

        private static bool HasSave()
        {
            return PlayerPrefs.HasKey(KeyDay);
        }

        private static WorldSession Restore(int seed)
        {
            int day = PlayerPrefs.GetInt(KeyDay, 1);
            var period = (TimeOfDay)PlayerPrefs.GetInt(KeyPeriod, (int)TimeOfDay.Morning);

            var s = new WorldSession
            {
                World = new WorldState(day, period)
                {
                    RepairFee = PlayerPrefs.GetInt(KeyRepairFee, 0),
                    BatchNumber = PlayerPrefs.GetInt(KeyBatch, 1),
                    TutorialComplete = PlayerPrefs.GetInt(KeyTutorial, 0) != 0,
                },
                Time = new TimeState(day, period, TimeState.CellsPerPeriod),
                Rng = new RandomService(seed),
                Intro = new RedShoeIntroService(),
                HeldBuffs = new BuffSet(),
                IsNewGame = false,
            };

            s.RegisterMap();

            string owned = PlayerPrefs.GetString(KeyOwned, "");
            foreach (string part in owned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part, out int value) && value != 0)
                {
                    s.World.OwnedParts.Add((PartType)value);
                }
            }

            if (s.World.OwnedParts.Count == 0)
            {
                s.World.OwnedParts.AddRange(new[]
                {
                    PartType.InertiaHammer,
                    PartType.MeteringRatchet,
                    PartType.BlastCoil,
                    PartType.ReversalGear,
                });
            }

            s.RefreshSupply();
            Current = s;
            return s;
        }

        private void RegisterMap()
        {
            foreach (PlaceConfig config in PlaceCatalog.FirstBatch())
            {
                PlaceNodeStatus status;
                if (config.UnlockSource == PlaceUnlockSource.Start)
                {
                    status = PlaceNodeStatus.Unlocked;
                }
                else if (config.UnlockSource == PlaceUnlockSource.Story)
                {
                    status = PlaceNodeStatus.KnownLocked;
                }
                else
                {
                    status = PlaceNodeStatus.Undiscovered;
                }

                World.Map.Register(config, status);
            }
        }

        private void RefreshSupply()
        {
            bool night = TimePeriod.IsNight(World.Period);
            _supply = SupplyService.Refresh(SupplyCatalog.FirstBatch(), night, 1, Rng);
        }
    }
}
