using Everlight.Tales.Board;
using System.Linq;
using Everlight.Tales.Data;
using Everlight.Tales.Events;
using Everlight.Tales.Meta;

namespace Everlight.Tales.UI.Diagnostics
{
    /// <summary>
    /// 业务诊断场景：产出表加载状态、随机消费计数与演算日志摘要。
    /// 接入框架既有 GFDiagnosticScenario 模式（经 TypeCache 自动发现）。
    /// 注意：本场景位于 UI 层而非 Meta——按程序集布局 Meta 仅引用 Data，无法访问 Board 的随机源；
    /// UI 同时引用 Board 与 Builtin.Runtime，是承载该跨层诊断的正确落点。
    /// </summary>
    public sealed class GameDiagnosticScenario : GFDiagnosticScenarioBase
    {
        public override string Name => "EverlightTales.Runtime";
        public override string Category => "EverlightTales";
        public override GFDiagnosticScenarioMode Mode => GFDiagnosticScenarioMode.PlayMode;

        public override void Run(GFDiagnosticScenarioContext context)
        {
            // 1. 表加载状态
            bool tableReady = GF.DataTable != null && GF.DataTable.HasDataTable<PartTable>();
            context.Assert(tableReady, "PartTable 未加载", "PartTable 已加载");
            bool configReady = GameConfigRuntime.EnsureLoaded();
            context.Assert(configReady, "全局配置快照未安装", "零件、材料、形态与 Buff 配置快照已安装");
            context.Detail("Config.PartCount", PartCatalog.All().Count);
            context.Detail("Config.MaterialCount", MaterialCatalog.All().Count);
            context.Detail("Config.FormCount", FormCatalog.All().Count);
            context.Detail("Config.BuffCount", BuffCatalog.All.Count);
            context.Detail("Config.AnomalyCount", GameConfigRuntime.Current?.Anomalies.Count ?? 0);
            context.Detail("Config.ReleaseCount", ReleaseTable.All().Count);
            context.Detail("Config.PlaceCount", GameConfigRuntime.Current?.Places.Count ?? 0);
            context.Detail("Config.SupplyCount", GameConfigRuntime.Current?.Supplies.Count ?? 0);
            context.Detail("Config.TrialCount", GameConfigRuntime.Current?.Trials.Count ?? 0);
            context.Detail("Config.TaskCount", GameConfigRuntime.Current?.Tasks.Count ?? 0);
            context.Detail("Config.NormalEventCount", GameConfigRuntime.Current?.NormalEvents.Count ?? 0);
            context.Detail("Config.CaseCount", GameConfigRuntime.Current?.Cases.Count ?? 0);
            context.Detail("Config.InvestigationCount", GameConfigRuntime.Current?.Investigations.Count ?? 0);
            context.Detail("Config.CopyCount", GameConfigRuntime.Current?.Copies.Count ?? 0);
            context.Detail("Config.PortraitCount", GameConfigRuntime.Current?.Portraits.Count ?? 0);
            context.Detail("Config.TutorialHintCount", GameConfigRuntime.Current?.TutorialHints.Count ?? 0);
            context.Detail("Config.CalibrationCount", GameConfigRuntime.Current?.Calibrations.Count ?? 0);
            context.Detail("Config.ObstacleCount", GameConfigRuntime.Current?.Obstacles.Count ?? 0);
            context.Detail("Config.AudioCueCount", GameConfigRuntime.Current?.AudioCues.Count ?? 0);
            context.Detail("Config.BgmCount", GameConfigRuntime.Current?.Bgms.Count ?? 0);
            context.Detail("Config.AssetBindingCount", GameConfigRuntime.Current?.AssetBindings.Count ?? 0);
            context.Detail("Config.NarrativeCount", GameConfigRuntime.Current?.Narratives.Count ?? 0);
            context.Detail("Config.CaseEventCount", GameConfigRuntime.Current?.CaseEvents.Count ?? 0);
            context.Detail("Config.BatchCount", GameConfigRuntime.Current?.Batches.Count ?? 0);
            context.Detail("Config.CaseRewardOptionCount", GameConfigRuntime.Current?.CaseRewardOptions.Count ?? 0);
            context.Assert(GameConfigRuntime.Current != null && GameConfigRuntime.Current.Copies.Count == 12 && GameConfigRuntime.Current.Portraits.Count == 3 &&
                GameConfigRuntime.Current.TutorialHints.Count == 3 && GameConfigRuntime.Current.Calibrations.Count == 18 && GameConfigRuntime.Current.Obstacles.Count == 8 &&
                GameConfigRuntime.Current.AudioCues.Count == 6 && GameConfigRuntime.Current.Bgms.Count == 2 && GameConfigRuntime.Current.AssetBindings.Count == 9 &&
                GameConfigRuntime.Current.Narratives.Count == 9,
                "新增全局配置未完整安装", "文案、立绘、教学、校准、障碍、音频、资源槽位和首案叙事配置已安装");
            context.Assert(GameConfigRuntime.Current != null && GameConfigRuntime.Current.CaseEvents.Count == 1,
                "首案事件配置未安装", "首案调查与维修入口配置已安装");
            context.Assert(GameConfigRuntime.Current != null && GameConfigRuntime.Current.Batches.Count == 5,
                "批次配置未安装", "五个怪谈批次配置已安装");
            context.Assert(GameConfigRuntime.Current != null && GameConfigRuntime.Current.CaseRewardOptions.Count == 14,
                "案件奖励配置未安装", "首案局内奖励池配置已安装");
            context.Detail("Config.LevelPresetCount", GameConfigRuntime.Levels.Count);
            context.Detail("Config.RedShoePresetCount", RedShoePresetCatalog.All.Count);
            context.Assert(LevelPresetCatalog.Get("EV-N01") != null && LevelPresetCatalog.Get("T-G01") != null && LevelPresetCatalog.Get("REDSHOE") != null && RedShoePresetCatalog.Get("REDSHOE") != null,
                "关卡关系表未安装完整", "卷帘门、试机、教学与红舞鞋关卡关系表已安装");
            context.Assert(CaseCatalog.Get("L-01") != null && InvestigationCatalog.Get("IV-R1") != null,
                "案件或调查配置未安装", "红舞鞋案件与调查入口配置已安装");
            LevelBoardConfig rollerBoard = RollerDoorEvent.CreateBoardConfig();
            RepairEventConfig rollerEvent = RollerDoorEvent.CreateConfig();
            LevelBoardConfig trialBoard = TrialEvent.CreateBoardConfig(TrialCatalog.Get("T-G01"));
            TutorialStage tutorial = TutorialSample.RotateAndCollide();
            RedShoeLevelConfig redShoe = RedShoeLevelConfig.Tutorial();
            BoardState rollerState = InitialBoardBuilder.Build(rollerBoard, rollerBoard.CarryPool, new RandomService(7));
            BoardState trialState = InitialBoardBuilder.Build(trialBoard, trialBoard.CarryPool, new RandomService(8));
            int trialRuntimeCount = 0;
            bool trialRuntimeReady = true;
            foreach (TrialConfig trial in TrialCatalog.All)
            {
                LevelBoardConfig configuredBoard = TrialEvent.CreateBoardConfig(trial);
                BoardState configuredState = InitialBoardBuilder.Build(configuredBoard, configuredBoard.CarryPool,
                    new RandomService(800 + trialRuntimeCount));
                TrialInstance instance = TrialEventShell.Begin(trial, configuredState);
                trialRuntimeReady &= configuredBoard != null && configuredState != null &&
                    configuredState.Entities.Any() && instance.State == TrialEventState.Prepared &&
                    instance.Level.Session.ArmMoves == 2 &&
                    TrialEventShell.EvaluateOutcome(instance, false) == EventResultKind.Failure;
                trialRuntimeCount++;
            }
            context.Detail("Config.TrialRuntimeCount", trialRuntimeCount);
            context.Detail("Config.RollerEntityCount", rollerState.Entities.Count());
            context.Detail("Config.TrialEntityCount", trialState.Entities.Count());
            context.Detail("Config.RedShoeGoalCount", redShoe.Rounds[0].MinDiversion + "/" + redShoe.Rounds[1].MinDiversion);
            context.Detail("Config.RollerRewardMaterialCount", rollerEvent.SuccessReward.Materials.Count);
            context.Assert(rollerBoard.KeyPieces.Count == 1 && rollerBoard.FixedElements.Count == 3 && rollerState.Entities.Count() == 8 &&
                trialBoard.FixedElements.Count == 3 && trialState.Entities.Count() == 6 && tutorial.Board != null && redShoe.InitialParts.Count == 6 &&
                redShoe.Rounds[0].MinDiversion == 3 && redShoe.Rounds[1].MinDiversion == 6 && redShoe.Rounds[1].RequireBoxRepaired && redShoe.Rounds[2].RequireSealed &&
                rollerEvent.SuccessReward.Materials.Count == 1 && RedShoePresetCatalog.Get("REDSHOE").GuideCells.Count == 6 &&
                RedShoePresetCatalog.Get("REDSHOE").TurnCells.Count == 1 && RedShoePresetCatalog.Get("REDSHOE").RestraintLimit == 6,
                "关卡关系表装配结果不符合预期", "卷帘门、试机、教学和红舞鞋盘面及专属规则装配通过");
            context.Assert(trialRuntimeCount == 4 && trialRuntimeReady,
                "试机配置运行时装配不完整", "四条试机配置均可从 DataTable 生成盘面并进入准备态");
#if UNITY_EDITOR
            WorldSession previous = WorldSession.Current;
            FirstCaseRuntimeValidation.PreservePrefs();
            try
            {
                context.Detail("PersistentProbe.Evidence", "配置/服务边界夹具；不作为正式玩法通关证据");
                VerifyTrialSessionBoundary(context);
                VerifyWorldDataBoundary(context);
            }
            finally
            {
                FirstCaseRuntimeValidation.RestorePrefs();
                WorldSession.RestoreValidationOwner(previous);
            }
#else
            context.Detail("PersistentProbe.Evidence", "持久化写入验证仅在Editor隔离目录运行；Player不修改存档");
#endif
            if (GF.DataTable != null)
            {
                context.Detail("DataTable.PartTable", GF.DataTable.HasDataTable<PartTable>());
                context.Detail("DataTable.SoundGroupTable", GF.DataTable.HasDataTable<SoundGroupTable>());
                context.Detail("DataTable.UITable", GF.DataTable.HasDataTable<UITable>());
            }

            // 2. 随机消费计数（固定种子确定性演示）
            RandomService rng = new RandomService(new FixedSeedSource(12345));
            rng.NextInt(0, 100);
            rng.NextInt(0, 100);
            rng.NextFloat();
            context.Detail("Random.Seed", rng.Seed);
            context.Detail("Random.ConsumedCount", rng.ConsumedCount);
            context.Assert(rng.ConsumedCount == 3, "随机消费计数不等于请求次数");

            // 3. 演算日志开关演示
            BoardLog.SetEnabled(true);
            BoardLog.Begin(12345, "演示初始盘面");
            BoardLog.Step("step-a", 1);
            BoardLog.Step("step-b", 2);
            BoardLogRecord record = BoardLog.End();
            BoardLog.SetEnabled(false);
            context.Assert(record != null && record.Steps.Count == 2, "开启时演算记录不完整");
            if (record != null)
            {
                context.Detail("BoardLog.Steps", record.Steps.Count);
                context.Detail("BoardLog.TotalConsumed", record.TotalRandomConsumed);
                context.Detail("BoardLog.ReplayChecksum", BoardReplay.ReplayChecksum(record));
            }

            context.Pass("业务诊断场景完成");
        }

        private static void VerifyWorldDataBoundary(GFDiagnosticScenarioContext context)
        {
            const int validationSlot = 3;
            try
            {
                SaveSlotService.Delete(validationSlot);
                WorldSession session = WorldSession.NewGame(90731, validationSlot);
                context.Detail("World.SupplyCount", session.Supply.Count);
                context.Detail("World.MapPlaceCount", session.World.Map.Places.Count);
                context.Assert(session.Supply.Count > 0 && session.World.Map.Places.Count > 0,
                    "新档未从配置快照生成供给或地图", "新档供给与地图由配置快照生成");

                WorldSave captured = WorldSaveService.Capture(session.World);
                WorldState restoredState = WorldSaveService.Restore(captured);
                context.Assert(restoredState.Map.Places.Count == session.World.Map.Places.Count &&
                    restoredState.OwnedParts.Count == session.World.OwnedParts.Count &&
                    restoredState.Tasks.Count == session.World.Tasks.Count,
                    "WorldSave 快照恢复丢失配置引用", "WorldSave 只保存状态并按配置目录恢复地图/任务关系");

                session.Save();
                WorldSession loaded = WorldSession.LoadOrNew(90731, validationSlot);
                context.Assert(SaveSlotService.HasSlot(validationSlot) && loaded.World.Map.Places.Count == session.World.Map.Places.Count,
                    "存档槽位恢复失败", "新存档槽位恢复并保留配置关系");
            }
            finally
            {
                SaveSlotService.Delete(validationSlot);
            }
        }

        private static void VerifyTrialSessionBoundary(GFDiagnosticScenarioContext context)
        {
            const int validationSlot = 3;
            WorldSession session = null;
            int taskCount = 0;
            bool allTasksStart = true;
            try
            {
                SaveSlotService.Delete(validationSlot);
                session = WorldSession.NewGame(90732, validationSlot);
                foreach (TaskConfig task in TaskCatalog.All)
                {
                    if (task.Blueprints == null || task.Blueprints.Count == 0)
                    {
                        allTasksStart = false;
                        continue;
                    }

                    TrialInstance instance = session.StartTrial(task.Id);
                    allTasksStart &= instance != null && instance.Config != null &&
                        instance.Config.Id == task.Blueprints[0] && instance.State == TrialEventState.Prepared &&
                        instance.Level.Session.ArmMoves == 2;
                    if (instance != null)
                    {
                        allTasksStart &= !session.SettleTrial(false) && instance.IsSettled && !instance.Succeeded;
                    }
                    taskCount++;
                }

                TaskConfig successTask = TaskCatalog.All.FirstOrDefault(t => t.Blueprints != null && t.Blueprints.Count > 0);
                bool success = false;
                if (successTask != null)
                {
                    TrialInstance instance = session.StartTrial(successTask.Id);
                    instance.Level.Session.Score = instance.Level.Round.TargetScore;
                    bool evaluated = TrialEventShell.EvaluateOutcome(instance, true) == EventResultKind.Success;
                    success = evaluated && session.SettleTrial(evaluated) && instance.Succeeded;
                }

                context.Detail("Config.TrialSessionTaskCount", taskCount);
                context.Assert(taskCount == TrialCatalog.All.Count && allTasksStart && success &&
                    SaveSlotService.HasSlot(validationSlot),
                    "试机会话入口装配失败", "四条试机均可由任务配置启动，失败/成功结算和存档写入均通过");
            }
            finally
            {
                SaveSlotService.Delete(validationSlot);
            }
        }
    }
}
