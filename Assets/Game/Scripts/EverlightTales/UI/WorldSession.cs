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

        /// <summary>序章字幕步骤（P3-005）：新档首进播放，可跳过。</summary>
        public static IReadOnlyList<PrologueStepConfig> OpeningSteps { get; } = new[]
        {
            new PrologueStepConfig("旁白", "夜雨落在旧城区的铁皮屋顶上。", 2.5f),
            new PrologueStepConfig("旁白", "你推开「永昼修理铺」的门，灯亮了起来。", 3f),
            new PrologueStepConfig("红舞鞋", "……那台节拍器，又开始响了。", 2.5f),
        };

        private const string KeyDay = "et.world.day";
        private const string KeyPeriod = "et.world.period";
        private const string KeyRepairFee = "et.world.repairFee";
        private const string KeyBatch = "et.world.batch";
        private const string KeyTutorial = "et.world.tutorial";
        private const string KeyOwned = "et.world.owned";
        private const string KeyKnown = "et.world.known";
        private const string KeyMaterials = "et.world.materials";
        private const string KeyBlueprints = "et.world.blueprints";
        private const string KeyForms = "et.world.forms";
        private const string KeyCurrentForms = "et.world.currentForms";
        private const string KeyTasks = "et.world.tasks";
        private const string KeyDisplay = "et.world.display";

        /// <summary>
        /// 四条改装支线任务模板（P4-013 接线处）：F 类四形态的图样任务，
        /// 任务页领奖发 T-G01~T-G04 图样。总步数 1、初始即进行中（Accepted），
        /// 步骤推进来源由 b30 委托/怪谈接线，本批只接「已存在 + 领奖」链路。
        /// </summary>
        public static IReadOnlyList<TaskConfig> ModTaskConfigs { get; } = new[]
        {
            new TaskConfig("TASK-MOD-001", "改装·贯通撞锤", 40, new[] { "T-G01" }, false, "周衡", "为惯性撞锤打造贯通形态：力从另一头出来。", 1),
            new TaskConfig("TASK-MOD-002", "改装·横推撞锤", 40, new[] { "T-G02" }, false, "周衡", "为惯性撞锤打造横推形态：一边输入，两边接应。", 1),
            new TaskConfig("TASK-MOD-003", "改装·轴向线圈", 40, new[] { "T-G03" }, false, "周衡", "为爆破线圈打造轴向形态：隔着空隙传过去。", 1),
            new TaskConfig("TASK-MOD-004", "改装·定时线圈", 40, new[] { "T-G04" }, false, "周衡", "为爆破线圈打造定时形态：把这一拍留到下一拍。", 1),
        };

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

        /// <summary>新档序章是否已播放完成（避免每次进主页都重播）。</summary>
        public bool OpeningDone { get; set; }

        /// <summary>是否有待展示的结算结果（结算事务完成后置位，主页壳读取后清除）。</summary>
        public bool HasPendingSettlement { get; set; }

        /// <summary>四选一奖励累计的机械臂次数（下次开始事件时注入 SessionState）。</summary>
        public int PendingArmMoves { get; private set; }

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
            SeedModTasks(s.World);
            s.RefreshSupply();
            Current = s;
            return s;
        }

        /// <summary>有存档则恢复，无存档则新档。</summary>
        public static WorldSession LoadOrNew(int seed)
        {
            return HasSave() ? Restore(seed) : NewGame(seed);
        }

        /// <summary>
        /// 把当前世界状态写 PlayerPrefs。b44 由最简 6 键扩展到成长闭环状态
        /// （已知零件/材料/图样/已解锁形态/当前形态/改装支线任务），仍走 PlayerPrefs。
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetInt(KeyDay, World.Day);
            PlayerPrefs.SetInt(KeyPeriod, (int)World.Period);
            PlayerPrefs.SetInt(KeyRepairFee, World.RepairFee);
            PlayerPrefs.SetInt(KeyBatch, World.BatchNumber);
            PlayerPrefs.SetInt(KeyTutorial, World.TutorialComplete ? 1 : 0);

            PlayerPrefs.SetString(KeyOwned, JoinParts(World.OwnedParts));
            PlayerPrefs.SetString(KeyKnown, JoinParts(World.KnownParts));

            var materials = new List<string>();
            foreach (MaterialStack stack in World.Materials.Stacks)
            {
                materials.Add(stack.MaterialId + ":" + stack.Count + ":" + (stack.SourceCase ?? ""));
            }

            PlayerPrefs.SetString(KeyMaterials, string.Join(";", materials));
            PlayerPrefs.SetString(KeyBlueprints, string.Join(",", World.Blueprints));
            PlayerPrefs.SetString(KeyForms, string.Join(",", World.UnlockedForms));

            var currentForms = new List<string>();
            foreach (var pair in World.CurrentForms)
            {
                if (!string.IsNullOrEmpty(pair.Value))
                {
                    currentForms.Add(((int)pair.Key) + ":" + pair.Value);
                }
            }

            PlayerPrefs.SetString(KeyCurrentForms, string.Join(";", currentForms));

            var tasks = new List<string>();
            foreach (TaskState task in World.Tasks)
            {
                tasks.Add(task.Config.Id + ":" + (int)task.Kind + ":" + task.CurrentStep);
            }

            PlayerPrefs.SetString(KeyTasks, string.Join(";", tasks));

            var displays = new List<string>();
            foreach (DisplayItem item in World.DisplayItems)
            {
                displays.Add(item.Id + ":" + item.Name + ":" + (int)item.Kind + ":" + item.Source + ":" + item.ObtainedDay);
            }

            PlayerPrefs.SetString(KeyDisplay, string.Join(";", displays));
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

            // 注入四选一累计的机械臂次数奖励（跨事件保留，注入后清零）。
            if (PendingArmMoves > 0)
            {
                CurrentEvent.Level.Session.ArmMoves += PendingArmMoves;
                PendingArmMoves = 0;
            }

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

            // 成功时发放材料奖励（如卷帘门 MT-002 精密齿轮 ×1）。
            if (reward != null && reward.Materials != null)
            {
                foreach (string materialId in reward.Materials)
                {
                    World.Materials.Add(materialId, 1);
                }
            }

            // 成长闭环演示接线：成功完成一次维修事件，推进「改装·贯通撞锤」支线一步
            // （1 步任务 → 可领奖），贯通「事件 → 领图样 → 加工台解锁形态」闭环。
            // 其余三条改装支线的步骤推进来源由 b30 委托/怪谈接线。
            if (success)
            {
                AdvanceModTask("TASK-MOD-001");

                // 陈列物（P4-009）：普通维修成功结算后留下「修好物件」只读回顾，按 Id 去重。
                AddDisplayItem("EV-N01", "卷帘门（已修复）", DisplayKind.RepairCompletion, "EV-N01");
            }

            LastReward = reward;
            HasPendingSettlement = true;
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

            ParseParts(PlayerPrefs.GetString(KeyOwned, ""), s.World.OwnedParts);
            ParseParts(PlayerPrefs.GetString(KeyKnown, ""), s.World.KnownParts);

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

            foreach (string item in PlayerPrefs.GetString(KeyMaterials, "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = item.Split(':');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int count))
                {
                    s.World.Materials.Add(parts[0], count, parts.Length >= 3 ? parts[2] : "");
                }
            }

            foreach (string id in PlayerPrefs.GetString(KeyBlueprints, "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                s.World.Blueprints.Add(id);
            }

            foreach (string id in PlayerPrefs.GetString(KeyForms, "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                s.World.UnlockedForms.Add(id);
            }

            foreach (string item in PlayerPrefs.GetString(KeyCurrentForms, "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = item.Split(':');
                if (parts.Length >= 2 && int.TryParse(parts[0], out int host) && host != 0)
                {
                    s.World.CurrentForms[(PartType)host] = parts[1];
                }
            }

            RestoreTasks(s.World);

            foreach (string item in PlayerPrefs.GetString(KeyDisplay, "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = item.Split(':');
                if (parts.Length >= 5 && int.TryParse(parts[2], out int kind) && int.TryParse(parts[4], out int obtainedDay))
                {
                    s.World.DisplayItems.Add(new DisplayItem(parts[0], parts[1], (DisplayKind)kind, parts[3], obtainedDay));
                }
            }

            // 旧档（b43 仅存 6 键）升级后补齐四条改装支线。
            SeedModTasks(s.World);

            s.RefreshSupply();
            Current = s;
            return s;
        }

        /// <summary>抽四选一奖励（Buff / 零件补给 / 机械臂次数）。</summary>
        public IReadOnlyList<RewardOption> DrawRewardChoice()
        {
            return RewardChoiceService.Draw(Rng, BuildRewardPool(), HeldBuffs, 4);
        }

        /// <summary>应用一个四选一奖励（Buff 入本关持有集、零件入世界、机械臂累计到下次事件）。</summary>
        public void ApplyReward(RewardOption option)
        {
            if (option == null)
            {
                return;
            }

            switch (option.Kind)
            {
                case RewardKind.Buff:
                    if (option.BuffConfig != null)
                    {
                        HeldBuffs.Add(option.BuffConfig);
                    }

                    break;

                case RewardKind.Part:
                    PartType type = PartTypeFromId(option.Id);
                    if (type != PartType.None && !World.OwnedParts.Contains(type))
                    {
                        World.OwnedParts.Add(type);
                    }

                    break;

                case RewardKind.ArmMove:
                    PendingArmMoves++;
                    break;
            }

            Save();
        }

        private IReadOnlyList<RewardOption> BuildRewardPool()
        {
            var pool = new List<RewardOption>();

            foreach (BuffConfig buff in BuffCatalog.All)
            {
                pool.Add(new RewardOption(RewardKind.Buff, buff.Id, "Buff·" + buff.Name, buff));
            }

            foreach (PartCodexConfig part in PartCodexCatalog.All())
            {
                // 无能力零件（新占位值）与已拥有零件不进补给池。
                if (PartCatalog.Get(part.Type) == null || World.OwnedParts.Contains(part.Type))
                {
                    continue;
                }

                pool.Add(new RewardOption(RewardKind.Part, part.Id, "零件·" + part.Name, null));
            }

            pool.Add(new RewardOption(RewardKind.ArmMove, "arm", "机械臂+1", null));
            return pool;
        }

        private static PartType PartTypeFromId(string id)
        {
            foreach (PartCodexConfig part in PartCodexCatalog.All())
            {
                if (part.Id == id)
                {
                    return part.Type;
                }
            }

            return PartType.None;
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

        /// <summary>推进指定改装支线任务一步（仅进行中任务）。</summary>
        private void AdvanceModTask(string id)
        {
            foreach (TaskState task in World.Tasks)
            {
                if (task.Config.Id == id)
                {
                    TaskService.Advance(task, Time.Day);
                    break;
                }
            }
        }

        /// <summary>追加陈列物（按 Id 去重，重复完成同一事件只记一条）。</summary>
        private void AddDisplayItem(string id, string name, DisplayKind kind, string source)
        {
            foreach (DisplayItem item in World.DisplayItems)
            {
                if (item.Id == id)
                {
                    return;
                }
            }

            World.DisplayItems.Add(new DisplayItem(id, name, kind, source, Time.Day));
        }

        /// <summary>补齐四条改装支线任务（按 Id 判重，缺则加入）。</summary>
        private static void SeedModTasks(WorldState world)
        {
            foreach (TaskConfig config in ModTaskConfigs)
            {
                if (!HasTask(world, config.Id))
                {
                    world.Tasks.Add(new TaskState(config));
                }
            }
        }

        /// <summary>从存档恢复改装支线任务（用完整配置，保留图样列表；旧档无 key 则跳过）。</summary>
        private static void RestoreTasks(WorldState world)
        {
            foreach (string item in PlayerPrefs.GetString(KeyTasks, "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = item.Split(':');
                if (parts.Length < 3)
                {
                    continue;
                }

                TaskConfig config = FindModTaskConfig(parts[0]);
                if (config == null || !int.TryParse(parts[1], out int kind) || !int.TryParse(parts[2], out int step))
                {
                    continue;
                }

                world.Tasks.Add(new TaskState(config)
                {
                    Kind = (TaskStateKind)kind,
                    CurrentStep = step,
                });
            }
        }

        private static TaskConfig FindModTaskConfig(string id)
        {
            foreach (TaskConfig config in ModTaskConfigs)
            {
                if (config.Id == id)
                {
                    return config;
                }
            }

            return null;
        }

        private static bool HasTask(WorldState world, string id)
        {
            foreach (TaskState task in world.Tasks)
            {
                if (task.Config.Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static string JoinParts(IEnumerable<PartType> parts)
        {
            var ids = new List<string>();
            foreach (PartType part in parts)
            {
                ids.Add(((int)part).ToString());
            }

            return string.Join(",", ids);
        }

        private static void ParseParts(string raw, List<PartType> target)
        {
            foreach (string part in raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part, out int value) && value != 0)
                {
                    target.Add((PartType)value);
                }
            }
        }
    }
}
