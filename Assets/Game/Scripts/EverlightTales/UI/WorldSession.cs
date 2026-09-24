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
        private const string KeyJobs = "et.world.jobs";
        private const string KeyTutorialStage = "et.world.tutorialStage";
        private const string KeySlotMetaPrefix = "et.world.meta.";

        /// <summary>存档位（1~3，1=原单存档键，向后兼容老档）。</summary>
        public int Slot { get; private set; } = 1;

        /// <summary>把存档键按存档位加后缀：slot&lt;=1 用原键，slot&gt;1 换成 et.world.{slot}.* 前缀。</summary>
        private static string SlotKey(string key, int slot)
        {
            return slot <= 1 ? key : key.Replace("et.world.", "et.world." + slot + ".");
        }

        private static string SlotMetaKey(int slot) => KeySlotMetaPrefix + slot;

        /// <summary>
        /// 四条改装支线任务模板（P4-013）：F 类四形态的图样任务，任务页领奖发 T-G01~T-G04 图样。
        /// 投放门槛（D-061）：T-G01=L-01 已完成 + 0 次成功普通维修；T-G02/T-G03=L-01 已完成 + 1 次；
        /// T-G04=L-01 已完成 + 2 次。门槛满足才播种（RefreshModTasks），试机成功推进、领奖发图样。
        /// </summary>
        public static IReadOnlyList<TaskConfig> ModTaskConfigs { get; } = new[]
        {
            new TaskConfig("TASK-MOD-001", "改装·贯通撞锤", 40, new[] { "T-G01" }, false, "周衡", "为惯性撞锤打造贯通形态：力从另一头出来。", 1, "L-01", 0, "home"),
            new TaskConfig("TASK-MOD-002", "改装·横推撞锤", 40, new[] { "T-G02" }, false, "周衡", "为惯性撞锤打造横推形态：一边输入，两边接应。", 1, "L-01", 1, "home"),
            new TaskConfig("TASK-MOD-003", "改装·轴向线圈", 40, new[] { "T-G03" }, false, "周衡", "为爆破线圈打造轴向形态：隔着空隙传过去。", 1, "L-01", 1, "home"),
            new TaskConfig("TASK-MOD-004", "改装·定时线圈", 40, new[] { "T-G04" }, false, "周衡", "为爆破线圈打造定时形态：把这一拍留到下一拍。", 1, "L-01", 2, "home"),
        };

        public static WorldSession Current { get; private set; }

        public WorldState World { get; private set; }

        public TimeState Time { get; private set; }

        public RandomService Rng { get; private set; }

        public RedShoeIntroService Intro { get; private set; }

        /// <summary>新档开场流程（P5-004）：回城接管→师父引导→教学衔接。</summary>
        public OpeningService Opening { get; private set; }

        public BuffSet HeldBuffs { get; private set; }

        /// <summary>本次进入是否为新档（未读到存档）。</summary>
        public bool IsNewGame { get; private set; }

        /// <summary>当前跟踪任务（定位/跟踪，P3-011），空=未跟踪。</summary>
        public string TrackedTaskId { get; private set; }

        /// <summary>当前盘面事件（卷帘门 EV-N01）。</summary>
        public RepairEventInstance CurrentEvent { get; private set; }

        /// <summary>当前盘面事件的盘面配置。</summary>
        public LevelBoardConfig CurrentBoardConfig { get; private set; }

        /// <summary>准备阶段暂存的携带选择（P2-007，确认后建盘面并清空）。</summary>
        public CarrySelection PendingCarry { get; private set; }

        /// <summary>准备阶段暂存的事件配置（P2-007，确认后 Begin 并清空）。</summary>
        public RepairEventConfig PendingEventConfig { get; private set; }

        /// <summary>当前试机事件（T-G01~04，借用样机）。</summary>
        public TrialInstance CurrentTrial { get; private set; }

        /// <summary>最近一次结算事务结果。</summary>
        public SettlementTransactionResult LastSettlement { get; private set; }

        /// <summary>最近一次事件奖励（失败/撤退为 null）。</summary>
        public EventReward LastReward { get; private set; }

        /// <summary>新档序章是否已播放完成（避免每次进主页都重播）。</summary>
        public bool OpeningDone { get; set; }

        /// <summary>四选一奖励累计的机械臂次数（下次开始事件时注入 SessionState）。</summary>
        public int PendingArmMoves { get; private set; }

        private List<SupplyInstance> _supply = new List<SupplyInstance>();

        /// <summary>当前昼/夜的一批普通供给。</summary>
        public IReadOnlyList<SupplyInstance> Supply => _supply;

        /// <summary>是否有未完成的维修尝试存档（首版 PlayerPrefs 最简存档未持久化尝试，暂恒 false；恢复面板预留）。</summary>
        public bool HasAttemptSave => false;

        /// <summary>新档：建世界 + 开局地图 + 初始零件 + 刷新供给。</summary>
        public static WorldSession NewGame(int seed, int slot = 1)
        {
            var s = new WorldSession
            {
                World = new WorldState(),
                Time = new TimeState(1, TimeOfDay.Morning, TimeState.CellsPerPeriod),
                Rng = new RandomService(seed),
                Intro = new RedShoeIntroService(),
                Opening = new OpeningService(),
                HeldBuffs = new BuffSet(),
                Slot = slot,
                IsNewGame = true,
            };
            s.RegisterMap();
            // S0 新档不预置零件：P-001~P-003 由三段教学逐步解锁（P5-003），
            // P-004 换向齿轮等由 S1 红鞋首案现场引入。
            RefreshModTasks(s.World);
            s.RefreshSupply();
            Current = s;
            return s;
        }

        /// <summary>有存档则恢复，无存档则新档（slot 默认 1，兼容原单存档入口）。</summary>
        public static WorldSession LoadOrNew(int seed, int slot = 1)
        {
            return HasSave(slot) ? Restore(seed, slot) : NewGame(seed, slot);
        }

        /// <summary>
        /// 把当前世界状态写 PlayerPrefs。b44 由最简 6 键扩展到成长闭环状态
        /// （已知零件/材料/图样/已解锁形态/当前形态/改装支线任务），仍走 PlayerPrefs。
        /// </summary>
        /// <summary>设置跟踪任务（P3-011），空 id 取消跟踪。</summary>
        public void TrackTask(string taskId)
        {
            TrackedTaskId = taskId;
        }

        /// <summary>当前跟踪任务的目标地点名（未跟踪返回 null）。</summary>
        public string TrackedTaskPlaceName
        {
            get
            {
                if (string.IsNullOrEmpty(TrackedTaskId))
                {
                    return null;
                }

                foreach (TaskState task in World.Tasks)
                {
                    if (task.Config.Id == TrackedTaskId)
                    {
                        PlaceConfig place = PlaceCatalog.Get(task.Config.PlaceId);
                        return place != null ? place.Name : "长明修理铺";
                    }
                }

                return null;
            }
        }

        public void Save()
        {
            int slot = Slot;
            PlayerPrefs.SetInt(SlotKey(KeyDay, slot), World.Day);
            PlayerPrefs.SetInt(SlotKey(KeyPeriod, slot), (int)World.Period);
            PlayerPrefs.SetInt(SlotKey(KeyRepairFee, slot), World.RepairFee);
            PlayerPrefs.SetInt(SlotKey(KeyBatch, slot), World.BatchNumber);
            PlayerPrefs.SetInt(SlotKey(KeyTutorial, slot), World.TutorialComplete ? 1 : 0);

            PlayerPrefs.SetString(SlotKey(KeyOwned, slot), JoinParts(World.OwnedParts));
            PlayerPrefs.SetString(SlotKey(KeyKnown, slot), JoinParts(World.KnownParts));

            var materials = new List<string>();
            foreach (MaterialStack stack in World.Materials.Stacks)
            {
                materials.Add(stack.MaterialId + ":" + stack.Count + ":" + (stack.SourceCase ?? ""));
            }

            PlayerPrefs.SetString(SlotKey(KeyMaterials, slot), string.Join(";", materials));
            PlayerPrefs.SetString(SlotKey(KeyBlueprints, slot), string.Join(",", World.Blueprints));
            PlayerPrefs.SetString(SlotKey(KeyForms, slot), string.Join(",", World.UnlockedForms));

            var currentForms = new List<string>();
            foreach (var pair in World.CurrentForms)
            {
                if (!string.IsNullOrEmpty(pair.Value))
                {
                    currentForms.Add(((int)pair.Key) + ":" + pair.Value);
                }
            }

            PlayerPrefs.SetString(SlotKey(KeyCurrentForms, slot), string.Join(";", currentForms));

            var tasks = new List<string>();
            foreach (TaskState task in World.Tasks)
            {
                tasks.Add(task.Config.Id + ":" + (int)task.Kind + ":" + task.CurrentStep);
            }

            PlayerPrefs.SetString(SlotKey(KeyTasks, slot), string.Join(";", tasks));

            var displays = new List<string>();
            foreach (DisplayItem item in World.DisplayItems)
            {
                displays.Add(item.Id + ":" + item.Name + ":" + (int)item.Kind + ":" + item.Source + ":" + item.ObtainedDay);
            }

            PlayerPrefs.SetString(SlotKey(KeyDisplay, slot), string.Join(";", displays));
            PlayerPrefs.SetInt(SlotKey(KeyJobs, slot), World.SuccessfulJobs);
            PlayerPrefs.SetInt(SlotKey(KeyTutorialStage, slot), World.TutorialStage);
            PlayerPrefs.SetString(SlotMetaKey(slot), DateTime.UtcNow.ToString("O"));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 准备卷帘门事件（EV-N01，P2-007）：创建盘面配置 + 事件配置 + 携带选择并暂存，
        /// 供准备页展示与调整；不建盘面、不进盘面。确认后调用 <see cref="ConfirmRollerDoor"/>。
        /// </summary>
        public LevelBoardConfig PrepareRollerDoor()
        {
            LevelBoardConfig boardConfig = RollerDoorEvent.CreateBoardConfig();
            RepairEventConfig eventConfig = RollerDoorEvent.CreateConfig();

            var keyParts = new List<PartType>();
            foreach (KeyPieceConfig key in boardConfig.KeyPieces)
            {
                keyParts.Add(key.PartType);
            }

            PendingCarry = new CarrySelection(World.OwnedParts, boardConfig.BorrowedParts, keyParts);
            PendingEventConfig = eventConfig;
            CurrentBoardConfig = boardConfig;
            return boardConfig;
        }

        /// <summary>解析本关形态映射（D-060）：当前形态 + 试机借用形态覆盖（借用覆盖宿主当前形态）。</summary>
        private Dictionary<PartType, string> ResolveBoardForms(TrialConfig trial)
        {
            var forms = new Dictionary<PartType, string>();
            foreach (var pair in World.CurrentForms)
            {
                if (!string.IsNullOrEmpty(pair.Value))
                {
                    forms[pair.Key] = pair.Value;
                }
            }

            if (trial != null && !string.IsNullOrEmpty(trial.BorrowedFormId))
            {
                forms[trial.HostPart] = trial.BorrowedFormId;
            }

            return forms;
        }

        /// <summary>
        /// 确认卷帘门事件（P2-007）：用准备页调整后的携带选择建盘面并 Begin 事件。
        /// 未先 <see cref="PrepareRollerDoor"/> 或已确认过则返回 null。
        /// </summary>
        public RepairEventInstance ConfirmRollerDoor()
        {
            if (CurrentBoardConfig == null || PendingCarry == null || PendingEventConfig == null)
            {
                return null;
            }

            BoardState board = InitialBoardBuilder.Build(CurrentBoardConfig, PendingCarry.SelectedParts(), Rng, ResolveBoardForms(null));
            CurrentEvent = RepairEventShell.Begin(PendingEventConfig, board);

            // 注入四选一累计的机械臂次数奖励（跨事件保留，注入后清零）。
            if (PendingArmMoves > 0)
            {
                CurrentEvent.Level.Session.ArmMoves += PendingArmMoves;
                PendingArmMoves = 0;
            }

            PendingCarry = null;
            PendingEventConfig = null;
            return CurrentEvent;
        }

        /// <summary>开始卷帘门事件（EV-N01）：准备 + 确认一步到位（快捷路径，供无准备页调用）。</summary>
        public RepairEventInstance StartRollerDoor()
        {
            PrepareRollerDoor();
            return ConfirmRollerDoor();
        }

        /// <summary>
        /// 结算当前事件：终局判定（由调用方传 success）→ 结算事务推进时间 →
        /// 世界结算发放奖励 → 写档 → 刷新供给。返回成功奖励（失败为 null）。
        /// </summary>
        public EventReward SettleEvent(SettlementOutcomeKind outcome)
        {
            if (CurrentEvent == null)
            {
                return null;
            }

            EventResultKind eventResult = outcome switch
            {
                SettlementOutcomeKind.Success => EventResultKind.Success,
                SettlementOutcomeKind.Retreat => EventResultKind.Retreat,
                _ => EventResultKind.Failure,
            };
            EventReward reward = RepairEventShell.Resolve(CurrentEvent, eventResult);

            // 失败时计算失败原因（分数差额 / 未完成特殊目标 / 即时失败），供结算页展示。
            FailureOutcome failure = null;
            if (outcome == SettlementOutcomeKind.Failure)
            {
                failure = FailureReason.Compute(
                    CurrentEvent.Level.Session.Score,
                    CurrentEvent.Level.Round.TargetScore,
                    CurrentEvent.Level.Round.Goals);
            }

            var transaction = new SettlementTransaction();
            LastSettlement = transaction.Execute(Time, CurrentEvent.Config.TimeCost, outcome, failure);

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

            // 成功普通维修/临时处置实例累计（P4-013 门槛来源；档案重放/试机不计入）。
            // 计数后刷新改装支线播种：满足 L-01 已完成 + 计数门槛的支线才出现。
            if (outcome == SettlementOutcomeKind.Success)
            {
                World.SuccessfulJobs++;
                RefreshModTasks(World);

                // 陈列物（P4-009）：普通维修成功结算后留下「修好物件」只读回顾，按 Id 去重。
                AddDisplayItem("EV-N01", "卷帘门（已修复）", DisplayKind.RepairCompletion, "EV-N01");
            }

            LastReward = reward;
            Save();
            RefreshSupply();
            return reward;
        }

        /// <summary>结算当前事件（bool 重载）：true=成功、false=失败。</summary>
        public EventReward SettleEvent(bool success)
        {
            return SettleEvent(success ? SettlementOutcomeKind.Success : SettlementOutcomeKind.Failure);
        }

        /// <summary>卷帘门门轴标记是否已被普通推移送入校正格。</summary>
        public bool IsRollerDoorGatePushedIn()
        {
            return CurrentEvent != null && RollerDoorEvent.IsGatePushedIn(CurrentEvent.Board);
        }

        /// <summary>开始一次试机（P4-012）：按改装支线图样定位试机关卡，借用样机、进入准备完成态。</summary>
        public TrialInstance StartTrial(string taskId)
        {
            TaskConfig task = FindModTaskConfig(taskId);
            if (task == null)
            {
                return null;
            }

            TrialConfig trial = FindTrialForTask(task);
            if (trial == null)
            {
                return null;
            }

            LevelBoardConfig boardConfig = TrialEvent.CreateBoardConfig(trial);

            var keyParts = new List<PartType>();
            foreach (KeyPieceConfig key in boardConfig.KeyPieces)
            {
                keyParts.Add(key.PartType);
            }

            CarrySelection carry = new CarrySelection(World.OwnedParts, boardConfig.BorrowedParts, keyParts);
            BoardState board = InitialBoardBuilder.Build(boardConfig, carry.SelectedParts(), Rng, ResolveBoardForms(trial));

            CurrentTrial = TrialEventShell.Begin(trial, board);
            return CurrentTrial;
        }

        /// <summary>结算试机：成功推进对应改装支线（不发直接奖励、不解锁永久形态）。</summary>
        public bool SettleTrial(bool success)
        {
            if (CurrentTrial == null)
            {
                return false;
            }

            bool succeeded = TrialEventShell.Resolve(CurrentTrial, success ? EventResultKind.Success : EventResultKind.Failure);
            if (succeeded)
            {
                string taskId = FindTaskForTrial(CurrentTrial.Config.Id);
                if (taskId != null)
                {
                    AdvanceModTask(taskId);
                }
            }

            Save();
            return succeeded;
        }

        /// <summary>试机标记是否已由普通推移送入端点格。</summary>
        public bool IsTrialSpecialGoalMet()
        {
            return CurrentTrial != null && TrialEvent.IsSpecialGoalMet(CurrentTrial.Board);
        }

        /// <summary>回访交付（P4-011）：已解决→待回访→已回访，一次性发图样/材料/维修费。</summary>
        public RevisitResult RevisitCase(string caseId)
        {
            CaseState state = FindCase(caseId);
            if (state == null)
            {
                return RevisitResult.NotAvailable;
            }

            if (state.Kind == CaseStateKind.Resolved)
            {
                RevisitService.OpenRevisit(state);
            }

            RevisitResult result = RevisitService.Deliver(state, World, Time.Day);
            if (result.Success)
            {
                Save();
                ShowRevisitUnlock(state.Config.RevisitBlueprints);
            }

            return result;
        }

        private static void ShowRevisitUnlock(IReadOnlyList<string> blueprints)
        {
            if (blueprints == null || blueprints.Count == 0)
            {
                return;
            }

            var names = new List<string>();
            foreach (string blueprint in blueprints)
            {
                FormConfig form = FormCatalog.FindByBlueprint(blueprint);
                names.Add(form != null ? form.Name : blueprint);
            }

            GlobalUI.ShowUnlock("获得图样", string.Join("、", names));
        }

        /// <summary>演示入口：确保红舞鞋 L-01 已解决（待回访）并配置回访奖励（D-085）。</summary>
        public CaseState EnsureRedShoeResolved()
        {
            CaseState existing = FindCase("L-01");
            if (existing != null)
            {
                if (existing.Kind == CaseStateKind.Resolved || existing.Kind == CaseStateKind.AwaitingRevisit || existing.Kind == CaseStateKind.Revisited)
                {
                    return existing;
                }

                WalkToResolved(existing);
                RefreshModTasks(World);
                return existing;
            }

            CaseState created = Intro.Confirm();
            if (created == null)
            {
                return null;
            }

            created.Config.RevisitBlueprints = new[] { "L-01" };
            created.Config.RevisitMaterials = new FormMaterialCost[]
            {
                new FormMaterialCost("MT-002", 2),
                new FormMaterialCost("MT-004", 2),
                new FormMaterialCost("MT-005", 1),
                new FormMaterialCost("MT-006", 1, "L-01"),
            };
            created.Config.RevisitFee = 120;

            WalkToResolved(created);
            World.Cases.Add(created);
            RefreshModTasks(World);
            return created;
        }

        /// <summary>把一条未结案案件沿状态机推进到已解决（调查中→待维修→维修中→已解决）。</summary>
        private static void WalkToResolved(CaseState state)
        {
            if (state.Kind == CaseStateKind.Investigating)
            {
                CaseStateMachine.ConfirmAnomaly(state);
            }

            if (state.Kind == CaseStateKind.AwaitingRepair)
            {
                CaseStateMachine.StartRepair(state);
            }

            if (state.Kind == CaseStateKind.Repairing)
            {
                CaseStateMachine.ResolveSuccess(state, true);
            }
        }

        /// <summary>按 ID 查案件状态（供来客区等 UI 读取，不改变状态）。</summary>
        public CaseState GetCase(string caseId)
        {
            return FindCase(caseId);
        }

        private CaseState FindCase(string caseId)
        {
            foreach (CaseState state in World.Cases)
            {
                if (state.Config.Id == caseId)
                {
                    return state;
                }
            }

            return null;
        }

        /// <summary>通过某段 S0 教学（P5-001/P5-003）：解锁对应零件 + 推进进度 + 写档。</summary>
        public bool CompleteTutorialStage(int stageIndex)
        {
            bool ok = TutorialService.CompleteStage(World, stageIndex);
            if (ok)
            {
                PartType part = TutorialService.StagePart(stageIndex);
                PartCodexConfig codex = PartCodexCatalog.Get(part);
                GlobalUI.ShowUnlock("新零件", codex != null ? codex.Name : part.ToString());
                Save();
            }

            return ok;
        }

        /// <summary>新档开场推进一步（P5-004）：回城接管→师父引导→教学衔接。</summary>
        public void AdvanceOpening()
        {
            if (Opening == null)
            {
                return;
            }

            if (Opening.Stage == OpeningStage.None)
            {
                Opening.ReturnToShop();
            }
            else if (Opening.Stage == OpeningStage.ReturnedToShop)
            {
                Opening.ReceiveMasterGuidance();
            }
            else if (Opening.Stage == OpeningStage.MasterGuidance)
            {
                Opening.StartTutorial();
            }
        }

        /// <summary>回归/演示入口（P5-011）：一键完成新档开场 + S0 三段教学（解锁 P-001~P-003）。</summary>
        public void CompleteOpeningAndTutorial()
        {
            while (Opening != null && Opening.Stage != OpeningStage.TutorialReady)
            {
                AdvanceOpening();
            }

            for (int i = 0; i < TutorialService.StageCount; i++)
            {
                CompleteTutorialStage(i);
            }
        }

        private static TrialConfig FindTrialForTask(TaskConfig task)
        {
            if (task.Blueprints == null || task.Blueprints.Count == 0)
            {
                return null;
            }

            return TrialCatalog.Get(task.Blueprints[0]);
        }

        private static string FindTaskForTrial(string trialId)
        {
            foreach (TaskConfig config in ModTaskConfigs)
            {
                if (config.Blueprints != null && config.Blueprints.Count > 0 && config.Blueprints[0] == trialId)
                {
                    return config.Id;
                }
            }

            return null;
        }

        private static bool HasSave(int slot)
        {
            return PlayerPrefs.HasKey(SlotKey(KeyDay, slot));
        }

        private static WorldSession Restore(int seed, int slot)
        {
            int day = PlayerPrefs.GetInt(SlotKey(KeyDay, slot), 1);
            var period = (TimeOfDay)PlayerPrefs.GetInt(SlotKey(KeyPeriod, slot), (int)TimeOfDay.Morning);

            var s = new WorldSession
            {
                World = new WorldState(day, period)
                {
                    RepairFee = PlayerPrefs.GetInt(SlotKey(KeyRepairFee, slot), 0),
                    BatchNumber = PlayerPrefs.GetInt(SlotKey(KeyBatch, slot), 1),
                    TutorialComplete = PlayerPrefs.GetInt(SlotKey(KeyTutorial, slot), 0) != 0,
                },
                Time = new TimeState(day, period, TimeState.CellsPerPeriod),
                Rng = new RandomService(seed),
                Intro = new RedShoeIntroService(),
                Opening = new OpeningService(),
                HeldBuffs = new BuffSet(),
                Slot = slot,
                IsNewGame = false,
            };

            s.RegisterMap();

            ParseParts(PlayerPrefs.GetString(SlotKey(KeyOwned, slot), ""), s.World.OwnedParts);
            ParseParts(PlayerPrefs.GetString(SlotKey(KeyKnown, slot), ""), s.World.KnownParts);

            foreach (string item in PlayerPrefs.GetString(SlotKey(KeyMaterials, slot), "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = item.Split(':');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int count))
                {
                    s.World.Materials.Add(parts[0], count, parts.Length >= 3 ? parts[2] : "");
                }
            }

            foreach (string id in PlayerPrefs.GetString(SlotKey(KeyBlueprints, slot), "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                s.World.Blueprints.Add(id);
            }

            foreach (string id in PlayerPrefs.GetString(SlotKey(KeyForms, slot), "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                s.World.UnlockedForms.Add(id);
            }

            foreach (string item in PlayerPrefs.GetString(SlotKey(KeyCurrentForms, slot), "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = item.Split(':');
                if (parts.Length >= 2 && int.TryParse(parts[0], out int host) && host != 0)
                {
                    s.World.CurrentForms[(PartType)host] = parts[1];
                }
            }

            RestoreTasks(s.World);
            s.World.SuccessfulJobs = PlayerPrefs.GetInt(SlotKey(KeyJobs, slot), 0);
            s.World.TutorialStage = PlayerPrefs.GetInt(SlotKey(KeyTutorialStage, slot), 0);

            foreach (string item in PlayerPrefs.GetString(SlotKey(KeyDisplay, slot), "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = item.Split(':');
                if (parts.Length >= 5 && int.TryParse(parts[2], out int kind) && int.TryParse(parts[4], out int obtainedDay))
                {
                    s.World.DisplayItems.Add(new DisplayItem(parts[0], parts[1], (DisplayKind)kind, parts[3], obtainedDay));
                }
            }

            // 门槛满足但尚未播种的改装支线补种（旧档升级路径）。
            RefreshModTasks(s.World);

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

        /// <summary>等待到下一时段：推进剩余时间格 + 刷新供给 + 保存（P3-004 等待按钮）。</summary>
        public TimeAdvanceResult WaitToNextPeriod()
        {
            TimeAdvanceResult result = Time.Advance(Time.RemainingCells);
            RefreshSupply();
            Save();
            return result;
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

        /// <summary>
        /// 按投放门槛补齐改装支线任务（P4-013）：只播种满足「前置案件已解决 + 成功计数达标」
        /// 且尚未加入的支线，按 Id 判重。门槛不满足的支线不出现（直到后续刷新）。
        /// </summary>
        private static void RefreshModTasks(WorldState world)
        {
            foreach (TaskConfig config in ModTaskConfigs)
            {
                if (!HasTask(world, config.Id) && ModTaskGateService.IsAvailable(config, world))
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
