using System;
using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using GameFramework;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 全局静态配置的运行时适配层。DataTable 行先在这里转成 Data 层 DTO，
    /// 再安装到目录；WorldState 和存档数据不经过此类。
    /// </summary>
    public static class GameConfigRuntime
    {
        public static GameConfigSnapshot Current { get; private set; }
        public static IReadOnlyList<LevelPreset> Levels => LevelPresetCatalog.All;
        public static IReadOnlyList<RewardMaterialEntry> Rewards => RewardMaterialCatalog.All;

        private sealed class IndexedMaterialCost
        {
            public int SortOrder;
            public FormMaterialCost Cost;
        }

        /// <summary>
        /// 载入完整全局配置快照。DataTable 尚未完成 GF 预加载时返回 false，
        /// 业务页面应在 Preload 完成后重试，不回退到旧的静态目录。
        /// </summary>
        public static bool EnsureLoaded()
        {
            if (Current != null)
            {
                return true;
            }

            if (GF.DataTable == null || !GF.DataTable.HasDataTable<PartTable>() || !GF.DataTable.HasDataTable<MaterialTable>() ||
                !GF.DataTable.HasDataTable<FormTable>() || !GF.DataTable.HasDataTable<FormMaterialTable>() ||
                !GF.DataTable.HasDataTable<BuffTable>() || !GF.DataTable.HasDataTable<AnomalyTable>() ||
                !GF.DataTable.HasDataTable<StageReleaseTable>() || !GF.DataTable.HasDataTable<PlaceTable>() ||
                !GF.DataTable.HasDataTable<EventTable>() || !GF.DataTable.HasDataTable<EventOpenPeriodTable>() || !GF.DataTable.HasDataTable<TrialTable>() ||
                !GF.DataTable.HasDataTable<TaskTable>() || !GF.DataTable.HasDataTable<TaskBlueprintTable>() || !GF.DataTable.HasDataTable<RewardMaterialTable>() ||
                !GF.DataTable.HasDataTable<CaseTable>() || !GF.DataTable.HasDataTable<InvestigationTable>() ||
                !GF.DataTable.HasDataTable<LevelTable>() || !GF.DataTable.HasDataTable<LevelRoundTable>() ||
                !GF.DataTable.HasDataTable<LevelGoalTable>() || !GF.DataTable.HasDataTable<LevelKeyPieceTable>() ||
                !GF.DataTable.HasDataTable<LevelCarryPartTable>() || !GF.DataTable.HasDataTable<LevelFixedElementTable>() ||
                !GF.DataTable.HasDataTable<RedShoeTable>() || !GF.DataTable.HasDataTable<RedShoeGuideTable>() ||
                !GF.DataTable.HasDataTable<RedShoeTurnTable>() || !GF.DataTable.HasDataTable<CopyTable>() ||
                !GF.DataTable.HasDataTable<PortraitTable>() || !GF.DataTable.HasDataTable<TutorialHintTable>() ||
                !GF.DataTable.HasDataTable<CalibrationTable>() || !GF.DataTable.HasDataTable<ObstacleTable>() ||
                !GF.DataTable.HasDataTable<AudioCueTable>() || !GF.DataTable.HasDataTable<BgmTable>() ||
                !GF.DataTable.HasDataTable<AssetBindingTable>() || !GF.DataTable.HasDataTable<NarrativeTable>() || !GF.DataTable.HasDataTable<CaseEventTable>() || !GF.DataTable.HasDataTable<BatchTable>() || !GF.DataTable.HasDataTable<CaseRewardOptionTable>() || !GF.DataTable.HasDataTable<ChallengeTable>())
            {
                return false;
            }

            var partConfigs = new List<PartConfig>();
            var codexConfigs = new List<PartCodexConfig>();
            var materialConfigs = new List<MaterialConfig>();
            var seenKeys = new HashSet<string>(StringComparer.Ordinal);
            var seenTypes = new HashSet<PartType>();
            foreach (PartTable row in GF.DataTable.GetDataTable<PartTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !seenKeys.Add(row.Key))
                {
                    throw new InvalidOperationException("Board/PartTable 存在空行或重复 Key。");
                }

                PartType type = (PartType)row.PartKind;
                if (!Enum.IsDefined(typeof(PartType), type) || type == PartType.None || !seenTypes.Add(type) || row.Key != string.Format("P-{0:000}", row.Id))
                {
                    throw new InvalidOperationException("Board/PartTable 的 Key、PartKind 或 ID 不符合稳定键约束：" + row.Key);
                }

                partConfigs.Add(new PartConfig(
                    type,
                    row.TriggerScore,
                    row.EnergyCapacity,
                    row.EffectCost,
                    row.EffectScorePerCell,
                    row.EffectScorePerTarget,
                    row.PublicEnergyPerEffect,
                    row.BonusScoreFromNth,
                    row.BonusNth,
                    row.RemovesSelf,
                    row.EffectScorePerEnergy,
                    row.PushDistance,
                    row.RayRange,
                    row.ChargeThreshold,
                    row.SecondaryEffectScore));
                codexConfigs.Add(new PartCodexConfig(row.Key, row.Name, type, row.Stage, row.SourceHint, row.UseHint));
            }

            if (partConfigs.Count != 20)
            {
                throw new InvalidOperationException("Board/PartTable 应包含 20 个正式零件条目，实际为 " + partConfigs.Count + "。");
            }

            var materialKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (MaterialTable row in GF.DataTable.GetDataTable<MaterialTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !materialKeys.Add(row.Key) || row.Key != string.Format("MT-{0:000}", row.Id) || row.Rarity < (int)MaterialRarity.Common || row.Rarity > (int)MaterialRarity.Rare)
                {
                    throw new InvalidOperationException("Meta/MaterialTable 的 Key 或 ID 不符合稳定键约束。");
                }

                materialConfigs.Add(new MaterialConfig(row.Key, row.Name, (MaterialRarity)row.Rarity, row.IsTypedByCase, row.FlavorText));
            }

            if (materialConfigs.Count != 6)
            {
                throw new InvalidOperationException("Meta/MaterialTable 应包含 6 个正式材料条目，实际为 " + materialConfigs.Count + "。");
            }

            var buffConfigs = new List<BuffConfig>();
            var buffKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (BuffTable row in GF.DataTable.GetDataTable<BuffTable>().GetAllDataRows())
            {
                if (row == null)
                {
                    throw new InvalidOperationException("Board/BuffTable 存在空行。");
                }

                BuffEffectKind effect = (BuffEffectKind)row.Effect;
                PartType targetPart = (PartType)row.TargetPartKind;
                if (string.IsNullOrEmpty(row.Key) || !buffKeys.Add(row.Key) || row.Key != string.Format("BF-{0:000}", row.Id) ||
                    string.IsNullOrEmpty(row.Stage) || string.IsNullOrEmpty(row.Description) || row.MaxStacks <= 0 ||
                    !Enum.IsDefined(typeof(BuffEffectKind), effect) || !Enum.IsDefined(typeof(PartType), targetPart))
                {
                    throw new InvalidOperationException("Board/BuffTable 存在空行、重复 Key 或无效枚举/叠加约束。");
                }

                buffConfigs.Add(new BuffConfig(row.Key, row.Name, row.Stage, row.Stackable, row.MaxStacks, row.Description,
                    effect, row.Value, targetPart));
            }

            if (buffConfigs.Count != 20)
            {
                throw new InvalidOperationException("Board/BuffTable 应包含 20 个正式 Buff 条目，实际为 " + buffConfigs.Count + "。");
            }

            var formMaterialRows = new Dictionary<string, List<IndexedMaterialCost>>(StringComparer.Ordinal);
            var relationKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (FormMaterialTable row in GF.DataTable.GetDataTable<FormMaterialTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.FormKey) || !relationKeys.Add(row.FormKey + "|" + row.MaterialKey + "|" + row.SortOrder) ||
                    row.Count <= 0 || row.SortOrder < 0 || !materialKeys.Contains(row.MaterialKey))
                {
                    throw new InvalidOperationException("Board/FormMaterialTable 存在无效外键、数量、排序或重复关系：行 " + (row == null ? "<null>" : row.Id.ToString()));
                }

                if (!formMaterialRows.TryGetValue(row.FormKey, out List<IndexedMaterialCost> costs))
                {
                    costs = new List<IndexedMaterialCost>();
                    formMaterialRows.Add(row.FormKey, costs);
                }

                costs.Add(new IndexedMaterialCost
                {
                    SortOrder = row.SortOrder,
                    Cost = new FormMaterialCost(row.MaterialKey, row.Count, row.SourceCase),
                });
            }

            foreach (List<IndexedMaterialCost> costs in formMaterialRows.Values)
            {
                costs.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
            }

            var formConfigs = new List<FormConfig>();
            var formKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (FormTable row in GF.DataTable.GetDataTable<FormTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !formKeys.Add(row.Key) || row.FormKind < (int)FormKind.F || row.FormKind > (int)FormKind.M ||
                    !Enum.IsDefined(typeof(PartType), (PartType)row.HostPartKind) || row.HostPartKind == (int)PartType.None)
                {
                    throw new InvalidOperationException("Board/FormTable 存在空行、重复 Key 或无效枚举。");
                }

                var costs = new List<FormMaterialCost>();
                if (formMaterialRows.TryGetValue(row.Key, out List<IndexedMaterialCost> configuredCosts))
                {
                    foreach (IndexedMaterialCost configuredCost in configuredCosts)
                    {
                        costs.Add(configuredCost.Cost);
                    }
                }
                formConfigs.Add(new FormConfig(row.Key, row.Name, (FormKind)row.FormKind, (PartType)row.HostPartKind,
                    row.Description, row.UnlockFee, row.BlueprintId, row.SourceHint, costs, row.AutoSetCurrent));
            }

            foreach (string formKey in formMaterialRows.Keys)
            {
                if (!formKeys.Contains(formKey))
                {
                    throw new InvalidOperationException("Board/FormMaterialTable 引用了不存在的形态：" + formKey);
                }
            }

            if (formConfigs.Count != 19 || relationKeys.Count != 54)
            {
                throw new InvalidOperationException("形态配置应包含 19 个形态和 54 条配方关系，实际为 " + formConfigs.Count + " / " + relationKeys.Count + "。");
            }

            var anomalyConfigs = new List<AnomalyConfig>();
            var anomalyTypes = new HashSet<AnomalyType>();
            foreach (AnomalyTable row in GF.DataTable.GetDataTable<AnomalyTable>().GetAllDataRows())
            {
                if (row == null || row.Key != string.Format("AN-{0:000}", row.Id) || row.AnomalyKind != row.Id ||
                    row.AnomalyKind < 1 || row.AnomalyKind > 8 || !anomalyTypes.Add((AnomalyType)row.AnomalyKind) ||
                    row.Deflection < 0 || row.SettleDirection < 0 || row.SettleDirection > 5 ||
                    row.Threshold < 0 || row.CoreCount < 0 || row.TapPeriod < 0)
                {
                    throw new InvalidOperationException("Board/AnomalyTable 存在无效稳定键、方向、数值或重复类型。");
                }

                AnomalyType type = (AnomalyType)row.AnomalyKind;
                if ((type == AnomalyType.LocalSettleDeflection && row.Deflection <= 0) ||
                    (type == AnomalyType.ExpandingRift && row.Threshold <= 0) ||
                    (type == AnomalyType.OverloadField && (row.Threshold <= 0 || row.CoreCount <= 0)) ||
                    (type == AnomalyType.PostTapReversal && row.TapPeriod <= 0))
                {
                    throw new InvalidOperationException("Board/AnomalyTable 的 " + type + " 缺少必需数值。");
                }

                anomalyConfigs.Add(new AnomalyConfig(type, row.Deflection, row.SettleDirection, row.Threshold, row.CoreCount, row.TapPeriod, ExtraParamSet.Parse(row.ExtraData, "异常 " + type)));
            }

            if (anomalyConfigs.Count != 8)
            {
                throw new InvalidOperationException("Board/AnomalyTable 应包含 8 个异常条目，实际为 " + anomalyConfigs.Count + "。");
            }

            var releases = new List<ReleaseEntry>();
            var releaseKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (StageReleaseTable row in GF.DataTable.GetDataTable<StageReleaseTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !releaseKeys.Add(row.Key) || row.ReleaseKind < 0 || row.ReleaseKind > 2 || row.Stage < 0 || row.Stage > 5 ||
                    string.IsNullOrEmpty(row.FirstAppearance) || string.IsNullOrEmpty(row.UnlockCondition))
                {
                    throw new InvalidOperationException("Meta/StageReleaseTable 存在重复 Key、无效阶段、类型或文案。");
                }
                releases.Add(new ReleaseEntry(row.Key, (ReleaseKind)row.ReleaseKind, (StageLevel)row.Stage, row.FirstAppearance, row.UnlockCondition));
            }
            if (releases.Count != 54)
            {
                throw new InvalidOperationException("Meta/StageReleaseTable 应包含 54 个投放条目，实际 " + releases.Count + "。");
            }

            var places = new List<PlaceConfig>();
            var placeKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (PlaceTable row in GF.DataTable.GetDataTable<PlaceTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !placeKeys.Add(row.Key) ||
                    row.UnlockSource < (int)PlaceUnlockSource.Start || row.UnlockSource > (int)PlaceUnlockSource.Event)
                {
                    throw new InvalidOperationException("Meta/PlaceTable 存在空键、重复键或无效解锁来源。");
                }
                places.Add(new PlaceConfig(row.Key, row.Name, row.Description, (PlaceUnlockSource)row.UnlockSource, row.X, row.Y, row.IsHome));
            }
            if (places.Count != 5)
            {
                throw new InvalidOperationException("Meta/PlaceTable 应包含 5 个地点，实际为 " + places.Count + "。");
            }

            var periodMap = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            var periodKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (EventOpenPeriodTable row in GF.DataTable.GetDataTable<EventOpenPeriodTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.EventKey) || row.Period < 0 || row.Period > 3 || row.SortOrder < 0 ||
                    !periodKeys.Add(row.EventKey + "|" + row.Period + "|" + row.SortOrder))
                {
                    throw new InvalidOperationException("Meta/EventOpenPeriodTable 存在无效事件外键、时段或重复排序。");
                }
                if (!periodMap.TryGetValue(row.EventKey, out List<int> periods))
                {
                    periods = new List<int>();
                    periodMap.Add(row.EventKey, periods);
                }
                periods.Add(row.Period);
            }
            var supplies = new List<SupplyCandidate>();
            var supplyKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (EventTable row in GF.DataTable.GetDataTable<EventTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !supplyKeys.Add(row.Key) || row.EventKind < (int)EventKind.Repair || row.EventKind > (int)EventKind.Investigate ||
                    row.MinStage < 1 || row.TimeCost <= 0 || row.RewardFee < 0 || !placeKeys.Contains(row.PlaceKey) || !periodMap.TryGetValue(row.Key, out List<int> periods))
                {
                    throw new InvalidOperationException("Meta/EventTable 存在无效事件字段、地点外键或缺少开放时段。");
                }
                periods.Sort();
                var openPeriods = new TimeOfDay[periods.Count];
                for (int i = 0; i < periods.Count; i++) openPeriods[i] = (TimeOfDay)periods[i];
                supplies.Add(new SupplyCandidate(row.Key, row.Name, row.MinStage, row.DayWeight, row.NightWeight, openPeriods, row.TimeCost, row.RewardFee, (EventKind)row.EventKind, row.PlaceKey));
            }

            var normalEvents = new List<NormalEventConfig>();
            foreach (EventTable row in GF.DataTable.GetDataTable<EventTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || row.RewardMaterialKey != "" && !materialKeys.Contains(row.RewardMaterialKey) ||
                    row.MinStage < 1 || row.TimeCost <= 0 || row.RewardFee < 0 || row.DayWeight < 0 || row.NightWeight < 0)
                {
                    throw new InvalidOperationException("Meta/EventTable 存在无效普通事件字段：" + (row == null ? "<null>" : row.Key));
                }
                if (!periodMap.TryGetValue(row.Key, out List<int> normalPeriods))
                {
                    throw new InvalidOperationException("Meta/EventOpenPeriodTable 缺少普通事件开放时段：" + row.Key);
                }
                normalPeriods.Sort();
                var openPeriods = new TimeOfDay[normalPeriods.Count];
                for (int i = 0; i < normalPeriods.Count; i++) openPeriods[i] = (TimeOfDay)normalPeriods[i];
                normalEvents.Add(new NormalEventConfig(row.Key, row.Name, (EventKind)row.EventKind, row.MinStage, row.TimeCost, row.RewardFee,
                    row.DayWeight, row.NightWeight, openPeriods, row.Repeatable, row.FailureWrapUp, string.IsNullOrEmpty(row.RewardMaterialKey) ? null : row.RewardMaterialKey));
            }
            var trials = new List<TrialConfig>();
            var trialKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (TrialTable row in GF.DataTable.GetDataTable<TrialTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !trialKeys.Add(row.Key) || row.TapCount <= 0 || row.TargetScore < 0 ||
                    !formKeys.Contains(row.BorrowedFormKey) || !Enum.IsDefined(typeof(PartType), (PartType)row.HostPartKind) || row.HostPartKind == (int)PartType.None)
                {
                    throw new InvalidOperationException("Meta/TrialTable 存在无效形态外键、宿主枚举或轮次字段。");
                }
                trials.Add(new TrialConfig(row.Key, row.Name, row.BorrowedFormKey, (PartType)row.HostPartKind, row.BlueprintKey,
                    new LevelConfig(new[] { new RoundConfig(row.TapCount, row.TargetScore) }), row.SpecialGoal));
            }

            var blueprintMap = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            var blueprintKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (TaskBlueprintTable row in GF.DataTable.GetDataTable<TaskBlueprintTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.TaskKey) || string.IsNullOrEmpty(row.BlueprintKey) || row.SortOrder < 0 ||
                    !blueprintKeys.Add(row.TaskKey + "|" + row.BlueprintKey + "|" + row.SortOrder))
                {
                    throw new InvalidOperationException("Meta/TaskBlueprintTable 存在无效任务外键、空图样或重复关系。");
                }
                if (!blueprintMap.TryGetValue(row.TaskKey, out List<string> blueprints))
                {
                    blueprints = new List<string>();
                    blueprintMap.Add(row.TaskKey, blueprints);
                }
                blueprints.Add(row.BlueprintKey);
            }
            var tasks = new List<TaskConfig>();
            var taskKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (TaskTable row in GF.DataTable.GetDataTable<TaskTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !taskKeys.Add(row.Key) || row.RewardFee < 0 || row.TotalSteps <= 0 ||
                    row.RequiredSuccessCount < 0 || !placeKeys.Contains(row.PlaceKey) || !blueprintMap.TryGetValue(row.Key, out List<string> blueprints))
                {
                    throw new InvalidOperationException("Meta/TaskTable 存在无效字段、地点外键或缺少图样关系。");
                }
                tasks.Add(new TaskConfig(row.Key, row.Name, row.RewardFee, blueprints, row.IsMain, row.Client, row.Description, row.TotalSteps,
                    row.RequiredCaseDone, row.RequiredSuccessCount, row.PlaceKey));
            }

            var rewards = new List<RewardMaterialEntry>();
            var rewardKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (RewardMaterialTable row in GF.DataTable.GetDataTable<RewardMaterialTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.OwnerKey) || string.IsNullOrEmpty(row.MaterialKey) || row.Count <= 0 || row.SortOrder < 0 || !rewardKeys.Add(row.OwnerKey + "|" + row.SortOrder) || !materialKeys.Contains(row.MaterialKey))
                    throw new InvalidOperationException("Meta/RewardMaterialTable 存在无效奖励材料、数量或重复顺序。");
                rewards.Add(new RewardMaterialEntry(row.OwnerKey, row.MaterialKey, row.Count, row.SourceCase, row.SortOrder));
            }

            var cases = new List<CaseConfig>();
            var caseKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (CaseTable row in GF.DataTable.GetDataTable<CaseTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !caseKeys.Add(row.Key) || row.TotalStages <= 0 || !placeKeys.Contains(row.FirstPlaceKey))
                    throw new InvalidOperationException("Meta/CaseTable 存在无效案件键、阶段数或地点外键。");
                cases.Add(new CaseConfig(row.Key, row.Name, row.Batch, row.TotalStages, row.Source, row.FirstPlaceKey));
            }

            var investigations = new List<InvestigationConfig>();
            var investigationKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (InvestigationTable row in GF.DataTable.GetDataTable<InvestigationTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !investigationKeys.Add(row.Key) || row.ObservationCount <= 0 || !caseKeys.Contains(row.CaseKey))
                    throw new InvalidOperationException("Meta/InvestigationTable 存在无效调查键、观察数量或案件外键。");
                CaseConfig caseConfig = null;
                foreach (CaseConfig configuredCase in cases)
                    if (configuredCase.Id == row.CaseKey) { caseConfig = configuredCase; break; }
                if (caseConfig == null) throw new InvalidOperationException("Meta/InvestigationTable 案件外键不存在：" + row.CaseKey);
                investigations.Add(new InvestigationConfig(row.Key, row.Name, row.ObservationCount, caseConfig.Id, caseConfig.Name, caseConfig.Batch, caseConfig.TotalStages));
            }

            var copies = new List<CopyEntry>();
            var copyKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (CopyTable row in GF.DataTable.GetDataTable<CopyTable>().GetAllDataRows())
            {
                if (row == null)
                    throw new InvalidOperationException("Meta/CopyTable 存在空行。");
                CopyCategory category = (CopyCategory)row.Category;
                if (string.IsNullOrEmpty(row.Key) || !copyKeys.Add(row.Key) || string.IsNullOrEmpty(row.Text) || !Enum.IsDefined(typeof(CopyCategory), category))
                    throw new InvalidOperationException("Meta/CopyTable 存在空键、重复键、空文案或无效分类。");
                copies.Add(new CopyEntry(row.Key, category, row.Text, row.Calibrated));
            }
            if (copies.Count != 12) throw new InvalidOperationException("Meta/CopyTable 应包含 12 条正式文案，实际为 " + copies.Count + "。");

            var portraits = new List<PortraitConfig>();
            var portraitKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (PortraitTable row in GF.DataTable.GetDataTable<PortraitTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !portraitKeys.Add(row.Key) || string.IsNullOrEmpty(row.Name) || string.IsNullOrEmpty(row.Description))
                    throw new InvalidOperationException("Meta/PortraitTable 存在空键、重复键或空人物说明。");
                portraits.Add(new PortraitConfig(row.Key, row.Name, row.Description));
            }
            if (portraits.Count != 3) throw new InvalidOperationException("Meta/PortraitTable 应包含 3 个正式人物，实际为 " + portraits.Count + "。");

            var tutorialHints = new List<TutorialHintConfig>();
            var hintKeys = new HashSet<string>(StringComparer.Ordinal);
            var hintStages = new HashSet<int>();
            foreach (TutorialHintTable row in GF.DataTable.GetDataTable<TutorialHintTable>().GetAllDataRows())
            {
                if (row == null) throw new InvalidOperationException("Meta/TutorialHintTable 存在空行。");
                PartType unlockPart = (PartType)row.UnlockPartKind;
                if (string.IsNullOrEmpty(row.Key) || !hintKeys.Add(row.Key) || row.StageIndex < 0 || !hintStages.Add(row.StageIndex) ||
                    string.IsNullOrEmpty(row.Title) || string.IsNullOrEmpty(row.TeachGoal) || string.IsNullOrEmpty(row.GuideText) || string.IsNullOrEmpty(row.HighlightTarget) ||
                    unlockPart == PartType.None || !Enum.IsDefined(typeof(PartType), unlockPart))
                    throw new InvalidOperationException("Meta/TutorialHintTable 存在空文案、重复键、重复段序号或无效零件枚举。");
                tutorialHints.Add(new TutorialHintConfig(row.StageIndex, row.Title, row.TeachGoal, row.GuideText, row.HighlightTarget, unlockPart));
            }
            if (tutorialHints.Count != 3) throw new InvalidOperationException("Meta/TutorialHintTable 应包含 3 段教学，实际为 " + tutorialHints.Count + "。");

            var calibrations = new List<CalibrationBound>();
            var calibrationKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (CalibrationTable row in GF.DataTable.GetDataTable<CalibrationTable>().GetAllDataRows())
            {
                if (row == null) throw new InvalidOperationException("Meta/CalibrationTable 存在空行。");
                CalibrationCategory category = (CalibrationCategory)row.Category;
                if (string.IsNullOrEmpty(row.Key) || !calibrationKeys.Add(row.Key) || string.IsNullOrEmpty(row.Name) || string.IsNullOrEmpty(row.Note) ||
                    !Enum.IsDefined(typeof(CalibrationCategory), category) || row.Min > row.Max || row.Value < row.Min || row.Value > row.Max)
                    throw new InvalidOperationException("Meta/CalibrationTable 存在空键、重复键、无效分类或越界数值。");
                calibrations.Add(new CalibrationBound(row.Key, category, row.Name, row.Value, row.Min, row.Max, row.Note));
            }
            if (calibrations.Count != 18) throw new InvalidOperationException("Meta/CalibrationTable 应包含 18 个校准项，实际为 " + calibrations.Count + "。");

            var obstacles = new List<ObstacleConfig>();
            var obstacleTypes = new HashSet<ObstacleType>();
            foreach (ObstacleTable row in GF.DataTable.GetDataTable<ObstacleTable>().GetAllDataRows())
            {
                if (row == null) throw new InvalidOperationException("Board/ObstacleTable 存在空行。");
                ObstacleType type = (ObstacleType)row.ObstacleKind;
                if (string.IsNullOrEmpty(row.Key) || row.Key != string.Format("O-{0:000}", row.ObstacleKind) || !obstacleTypes.Add(type) ||
                    !Enum.IsDefined(typeof(ObstacleType), type) || type == ObstacleType.None || row.ObstacleKind < 1 || row.ObstacleKind > 8 || row.MaxDurability < 0 || row.PassDirection < -1 || row.PassDirection > 5)
                    throw new InvalidOperationException("Board/ObstacleTable 存在无效稳定键、障碍类型、耐久或方向参数。");
                obstacles.Add(new ObstacleConfig(type, row.MaxDurability, row.PassDirection, ExtraParamSet.Parse(row.ExtraData, "障碍 " + type)));
            }
            if (obstacles.Count != 8) throw new InvalidOperationException("Board/ObstacleTable 应包含 8 个障碍类型，实际为 " + obstacles.Count + "。");

            var audioCues = new List<AudioCueConfig>();
            var audioKinds = new HashSet<AudioCueKind>();
            var audioKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (AudioCueTable row in GF.DataTable.GetDataTable<AudioCueTable>().GetAllDataRows())
            {
                if (row == null) throw new InvalidOperationException("Meta/AudioCueTable 存在空行。");
                AudioCueKind kind = (AudioCueKind)row.CueKind;
                if (string.IsNullOrEmpty(row.Key) || !audioKeys.Add(row.Key) || !audioKinds.Add(kind) || !Enum.IsDefined(typeof(AudioCueKind), kind) || string.IsNullOrEmpty(row.Name) || string.IsNullOrEmpty(row.Description))
                    throw new InvalidOperationException("Meta/AudioCueTable 存在空键、重复分类或无效音效位。");
                audioCues.Add(new AudioCueConfig(row.Key, kind, row.Name, row.Description));
            }
            if (audioCues.Count != 6) throw new InvalidOperationException("Meta/AudioCueTable 应包含 6 个音效位，实际为 " + audioCues.Count + "。");

            var bgms = new List<BgmConfig>();
            var bgmTracks = new HashSet<BgmTrack>();
            var bgmKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (BgmTable row in GF.DataTable.GetDataTable<BgmTable>().GetAllDataRows())
            {
                if (row == null) throw new InvalidOperationException("Meta/BgmTable 存在空行。");
                BgmTrack track = (BgmTrack)row.Track;
                if (string.IsNullOrEmpty(row.Key) || !bgmKeys.Add(row.Key) || !bgmTracks.Add(track) || !Enum.IsDefined(typeof(BgmTrack), track) || string.IsNullOrEmpty(row.Name) || string.IsNullOrEmpty(row.Description))
                    throw new InvalidOperationException("Meta/BgmTable 存在空键、重复轨道或无效 BGM。");
                bgms.Add(new BgmConfig(track, row.Name, row.Description));
            }
            if (bgms.Count != 2) throw new InvalidOperationException("Meta/BgmTable 应包含 2 条 BGM，实际为 " + bgms.Count + "。");

            var assetBindings = new List<AssetSlot>();
            var assetKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (AssetBindingTable row in GF.DataTable.GetDataTable<AssetBindingTable>().GetAllDataRows())
            {
                if (row == null) throw new InvalidOperationException("Meta/AssetBindingTable 存在空行。");
                AssetCategory category = (AssetCategory)row.Category;
                if (string.IsNullOrEmpty(row.Key) || !assetKeys.Add(row.Key) || !Enum.IsDefined(typeof(AssetCategory), category) || string.IsNullOrEmpty(row.Name))
                    throw new InvalidOperationException("Meta/AssetBindingTable 存在空键、重复键、无效资源分类或空名称。");
                assetBindings.Add(new AssetSlot(row.Key, category, row.Name, row.Path));
            }
            if (assetBindings.Count != 9) throw new InvalidOperationException("Meta/AssetBindingTable 应包含 9 个资源槽位，实际为 " + assetBindings.Count + "。");

            var narratives = new List<NarrativeEntry>();
            var narrativeKeys = new HashSet<string>(StringComparer.Ordinal);
            var narrativeOrders = new HashSet<string>(StringComparer.Ordinal);
            foreach (NarrativeTable row in GF.DataTable.GetDataTable<NarrativeTable>().GetAllDataRows())
            {
                if (row == null) throw new InvalidOperationException("Meta/NarrativeTable 存在空行。");
                if (string.IsNullOrEmpty(row.Key) || !narrativeKeys.Add(row.Key) || string.IsNullOrEmpty(row.ScriptKey) || !narrativeOrders.Add(row.ScriptKey + "|" + row.SortOrder) || row.SortOrder < 0 || string.IsNullOrEmpty(row.Speaker) || string.IsNullOrEmpty(row.Text) || row.Duration < 0 ||
                    (!string.IsNullOrEmpty(row.ChoiceLabel) && string.IsNullOrEmpty(row.NextNodeKey)))
                    throw new InvalidOperationException("Meta/NarrativeTable 存在空文案、重复顺序或断裂选项关系。");
                narratives.Add(new NarrativeEntry(row.Key, row.ScriptKey, row.SortOrder, row.NodeKey, row.Speaker, row.Text, row.ChoiceLabel, row.NextNodeKey, row.Duration));
            }
            if (narratives.Count != 9) throw new InvalidOperationException("Meta/NarrativeTable 应包含 9 条首案叙事，实际为 " + narratives.Count + "。");

            var caseEvents = new List<CaseEventConfig>();
            var caseEventKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (CaseEventTable row in GF.DataTable.GetDataTable<CaseEventTable>().GetAllDataRows())
            {
                if (row == null) throw new InvalidOperationException("Meta/CaseEventTable 存在空行。");
                if (string.IsNullOrEmpty(row.Key) || !caseEventKeys.Add(row.Key) || !caseKeys.Contains(row.CaseKey) || string.IsNullOrEmpty(row.InvestigationTitle) || string.IsNullOrEmpty(row.InvestigationDescription) || string.IsNullOrEmpty(row.RepairTitle) || string.IsNullOrEmpty(row.RepairDescription) || row.RepairTimeCost <= 0 || row.RevisitFee < 0 || string.IsNullOrEmpty(row.RevisitBlueprintKey) || row.CompletionPartKind <= 0 || string.IsNullOrEmpty(row.InvestigationPageTitle) || string.IsNullOrEmpty(row.Hotspot1Name) || string.IsNullOrEmpty(row.Hotspot2Name) || string.IsNullOrEmpty(row.Hotspot3Name) || row.Hotspot1X < 0 || row.Hotspot1X > 1 || row.Hotspot1Y < 0 || row.Hotspot1Y > 1 || row.Hotspot2X < 0 || row.Hotspot2X > 1 || row.Hotspot2Y < 0 || row.Hotspot2Y > 1 || row.Hotspot3X < 0 || row.Hotspot3X > 1 || row.Hotspot3Y < 0 || row.Hotspot3Y > 1)
                    throw new InvalidOperationException("Meta/CaseEventTable 存在无效案件外键、空文案或耗时。");
                caseEvents.Add(new CaseEventConfig(row.Key, row.CaseKey, row.InvestigationTitle, row.InvestigationDescription, row.RepairTitle, row.RepairDescription, row.RepairTimeCost, row.InvestigationPageTitle, row.Hotspot1Name, row.Hotspot1X, row.Hotspot1Y, row.Hotspot2Name, row.Hotspot2X, row.Hotspot2Y, row.Hotspot3Name, row.Hotspot3X, row.Hotspot3Y, row.RevisitFee, row.RevisitBlueprintKey, row.CompletionPartKind));
            }
            if (caseEvents.Count != 1) throw new InvalidOperationException("Meta/CaseEventTable 应包含 1 个首案事件，实际为 " + caseEvents.Count + "。");

            var batches = new List<BatchConfig>();
            var batchKeys = new HashSet<string>(StringComparer.Ordinal);
            int expectedBatchNumber = 1;
            foreach (BatchTable row in GF.DataTable.GetDataTable<BatchTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !batchKeys.Add(row.Key) || row.BatchNumber != expectedBatchNumber || row.Key != "B" + row.BatchNumber || string.IsNullOrEmpty(row.Name) || row.RequiredCount < 0)
                    throw new InvalidOperationException("Meta/BatchTable 存在空键、重复键、序号或完成条件错误。");
                batches.Add(new BatchConfig(row.Key, row.BatchNumber, row.Name, row.RequiredCount));
                expectedBatchNumber++;
            }
            if (batches.Count != 5) throw new InvalidOperationException("Meta/BatchTable 应包含 5 个批次，实际为 " + batches.Count + "。");

            var caseRewardOptions = new List<CaseRewardOptionConfig>();
            var caseRewardKeys = new HashSet<string>(StringComparer.Ordinal);
            var caseRewardSortOrders = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);
            foreach (CaseRewardOptionTable row in GF.DataTable.GetDataTable<CaseRewardOptionTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || !caseRewardKeys.Add(row.Key) || !caseEventKeys.Contains(row.OwnerKey) || row.RewardKind < 0 || row.RewardKind > 2 || string.IsNullOrEmpty(row.RewardId) || row.SortOrder < 0 || string.IsNullOrEmpty(row.Label))
                    throw new InvalidOperationException("Meta/CaseRewardOptionTable 存在无效案件事件外键、奖励类别、排序或文案。");
                if (!caseRewardSortOrders.TryGetValue(row.OwnerKey, out HashSet<int> sortOrders))
                {
                    sortOrders = new HashSet<int>();
                    caseRewardSortOrders.Add(row.OwnerKey, sortOrders);
                }
                if (!sortOrders.Add(row.SortOrder)) throw new InvalidOperationException("Meta/CaseRewardOptionTable 存在重复奖励排序：" + row.OwnerKey);
                if (row.RewardKind == 0)
                {
                    bool hasBuff = false;
                    foreach (BuffConfig buff in buffConfigs) if (buff.Id == row.RewardId) { hasBuff = true; break; }
                    if (!hasBuff) throw new InvalidOperationException("Meta/CaseRewardOptionTable 引用了不存在的 Buff：" + row.RewardId);
                }
                if (row.RewardKind == 1)
                {
                    if (!Enum.TryParse(row.RewardId, out PartType configuredPart) || configuredPart == PartType.None || !partConfigs.Exists(part => part.Type == configuredPart))
                        throw new InvalidOperationException("Meta/CaseRewardOptionTable 引用了不存在的零件：" + row.RewardId);
                }
                if (row.RewardKind == 2 && row.RewardId != "arm") throw new InvalidOperationException("Meta/CaseRewardOptionTable 的机械臂奖励键必须为 arm。");
                caseRewardOptions.Add(new CaseRewardOptionConfig(row.Key, row.OwnerKey, row.RewardKind, row.RewardId, row.SortOrder, row.Label));
            }
            if (caseRewardOptions.Count != 14) throw new InvalidOperationException("Meta/CaseRewardOptionTable 应包含 14 个首案奖励选项，实际为 " + caseRewardOptions.Count + "。");

            var challenges = new List<ChallengeConfig>();
            var challengeKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (ChallengeTable row in GF.DataTable.GetDataTable<ChallengeTable>().GetAllDataRows())
            {
                if (row == null) throw new InvalidOperationException("Board/ChallengeTable 存在空行。");
                ChallengeKind kind = (ChallengeKind)row.ChallengeKind;
                if (string.IsNullOrEmpty(row.Key) || row.Key != string.Format("C-{0:000}", row.Id) || !challengeKeys.Add(row.Key) ||
                    string.IsNullOrEmpty(row.CaseKey) || string.IsNullOrEmpty(row.Name) || string.IsNullOrEmpty(row.RuleId) ||
                    string.IsNullOrEmpty(row.Stage) || !Enum.IsDefined(typeof(ChallengeKind), kind) || kind == ChallengeKind.None)
                {
                    throw new InvalidOperationException("Board/ChallengeTable 存在无效稳定键、案件键、规则编号或类型。");
                }

                challenges.Add(new ChallengeConfig(row.Key, row.CaseKey, row.Name, row.LevelKey, kind, row.RuleId, row.Stage, row.GoalText, ExtraParamSet.Parse(row.ExtraData, "挑战 " + row.Key)));
            }

            if (challenges.Count != 15)
            {
                throw new InvalidOperationException("Board/ChallengeTable 应包含 15 个专属挑战，实际为 " + challenges.Count + "。");
            }

            var levels = BuildLevelPresets(calibrations);
            var redShoePresets = BuildRedShoePresets();
            var snapshot = new GameConfigSnapshot(partConfigs, codexConfigs, materialConfigs, formConfigs, buffConfigs, anomalyConfigs, releases, places, supplies, trials, tasks, normalEvents, rewards, cases, investigations, copies, portraits, tutorialHints, calibrations, obstacles, audioCues, bgms, assetBindings, narratives, caseEvents, batches, caseRewardOptions, challenges);
            PartCatalog.Install(snapshot.Parts);
            PartCodexCatalog.Install(snapshot.PartCodex);
            MaterialCatalog.Install(snapshot.Materials);
            FormCatalog.Install(snapshot.Forms);
            BuffCatalog.Install(snapshot.Buffs);
            AnomalyCatalog.Install(snapshot.Anomalies);
            ReleaseTable.Install(snapshot.Releases);
            PlaceCatalog.Install(snapshot.Places);
            SupplyCatalog.Install(snapshot.Supplies);
            TrialCatalog.Install(snapshot.Trials);
            TaskCatalog.Install(snapshot.Tasks);
            NormalEventCatalog.Install(snapshot.NormalEvents);
            RewardMaterialCatalog.Install(snapshot.Rewards);
            CaseCatalog.Install(snapshot.Cases);
            InvestigationCatalog.Install(snapshot.Investigations);
            CopyCatalog.Install(snapshot.Copies);
            PortraitCatalog.Install(snapshot.Portraits);
            TutorialHintCatalog.Install(snapshot.TutorialHints);
            CalibrationCatalog.Install(snapshot.Calibrations);
            ObstacleCatalog.Install(snapshot.Obstacles);
            AudioCueCatalog.Install(snapshot.AudioCues);
            BgmCatalog.Install(snapshot.Bgms);
            AssetBindingCatalog.Install(snapshot.AssetBindings);
            NarrativeCatalog.Install(snapshot.Narratives);
            CaseEventCatalog.Install(snapshot.CaseEvents);
            BatchCatalog.Install(snapshot.Batches);
            CaseRewardOptionCatalog.Install(snapshot.CaseRewardOptions);
            ChallengeCatalog.Install(snapshot.Challenges);
            LevelPresetCatalog.Install(levels);
            RedShoePresetCatalog.Install(redShoePresets);
            Current = snapshot;
            Debug.Log("[GameConfigRuntime] 全局 DataTable 配置校验通过，已安装零件 20、材料 6、形态 19、配方 54、Buff 20、异常 8、投放 39、地点 5、供给 6、试机 4、任务 4、普通事件 6、奖励 " + rewards.Count + "、案件 " + cases.Count + "、调查 " + investigations.Count + "、文案 " + copies.Count + "、立绘 " + portraits.Count + "、教学 " + tutorialHints.Count + "、校准 " + calibrations.Count + "、障碍 " + obstacles.Count + "、音效位 " + audioCues.Count + "、BGM " + bgms.Count + "、资源槽位 " + assetBindings.Count + "、叙事 " + narratives.Count + "、案件事件 " + caseEvents.Count + "、关卡 " + levels.Count + "、红舞鞋规则 " + redShoePresets.Count + "、专属挑战 " + challenges.Count + "。");
            return true;
        }

        private static IReadOnlyList<RedShoePreset> BuildRedShoePresets()
        {
            var baseRows = new Dictionary<string, RedShoeTable>(StringComparer.Ordinal);
            foreach (RedShoeTable row in GF.DataTable.GetDataTable<RedShoeTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.LevelKey) || baseRows.ContainsKey(row.LevelKey) ||
                    row.BoxRequired < 0 || row.RequiredDiversion < 0 || row.RestraintLimit <= 0 || row.StepsPerTap <= 0 ||
                    !Enum.IsDefined(typeof(HexDirection), row.StartDirection))
                    throw new InvalidOperationException("Board/RedShoeTable 存在无效关卡键、数值或方向。");
                baseRows.Add(row.LevelKey, row);
            }

            var guides = new Dictionary<string, List<RedShoeGuideTable>>(StringComparer.Ordinal);
            foreach (RedShoeGuideTable row in GF.DataTable.GetDataTable<RedShoeGuideTable>().GetAllDataRows())
            {
                if (row == null || !baseRows.ContainsKey(row.LevelKey) || row.SortOrder < 0)
                    throw new InvalidOperationException("Board/RedShoeGuideTable 存在无效关卡外键或排序。");
                AddRow(guides, row.LevelKey, row);
            }

            var turns = new Dictionary<string, List<RedShoeTurnTable>>(StringComparer.Ordinal);
            foreach (RedShoeTurnTable row in GF.DataTable.GetDataTable<RedShoeTurnTable>().GetAllDataRows())
            {
                if (row == null || !baseRows.ContainsKey(row.LevelKey) || row.SortOrder < 0 || !Enum.IsDefined(typeof(HexDirection), row.Direction))
                    throw new InvalidOperationException("Board/RedShoeTurnTable 存在无效关卡外键、排序或方向。");
                AddRow(turns, row.LevelKey, row);
            }

            var result = new List<RedShoePreset>();
            foreach (RedShoeTable row in baseRows.Values)
            {
                var guideCells = new List<HexCoord>();
                if (guides.TryGetValue(row.LevelKey, out List<RedShoeGuideTable> configuredGuides))
                {
                    configuredGuides.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
                    for (int i = 0; i < configuredGuides.Count; i++)
                    {
                        if (configuredGuides[i].SortOrder != i) throw new InvalidOperationException("Board/RedShoeGuideTable 顺序不连续：" + row.LevelKey);
                        guideCells.Add(new HexCoord(configuredGuides[i].Q, configuredGuides[i].R));
                    }
                }

                var turnCells = new List<RedShoeTurnCell>();
                if (turns.TryGetValue(row.LevelKey, out List<RedShoeTurnTable> configuredTurns))
                {
                    configuredTurns.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
                    for (int i = 0; i < configuredTurns.Count; i++)
                    {
                        if (configuredTurns[i].SortOrder != i) throw new InvalidOperationException("Board/RedShoeTurnTable 顺序不连续：" + row.LevelKey);
                        turnCells.Add(new RedShoeTurnCell(new HexCoord(configuredTurns[i].Q, configuredTurns[i].R), (HexDirection)configuredTurns[i].Direction));
                    }
                }

                result.Add(new RedShoePreset(row.LevelKey, new HexCoord(row.StartQ, row.StartR), (HexDirection)row.StartDirection,
                    guideCells, turnCells, new HexCoord(row.BoxQ, row.BoxR), row.BoxRequired,
                    new HexCoord(row.EntryQ, row.EntryR), row.RequiredDiversion, row.RestraintLimit, row.StepsPerTap));
            }
            return result.AsReadOnly();
        }

        private static IReadOnlyList<LevelPreset> BuildLevelPresets(IReadOnlyList<CalibrationBound> calibrations)
        {
            int defaultRepairScore = GetCalibrationValue(calibrations, "REPAIR-SCORE");
            var levelRows = new Dictionary<string, LevelTable>(StringComparer.Ordinal);
            foreach (LevelTable row in GF.DataTable.GetDataTable<LevelTable>().GetAllDataRows())
            {
                if (row == null || string.IsNullOrEmpty(row.Key) || levelRows.ContainsKey(row.Key) || row.Radius <= 0 || row.InitialPartCount < 0 || row.InitialArmMoves < 0 || row.InitialPublicRepairEnergy < 0 || row.Duration < 0)
                    throw new InvalidOperationException("Board/LevelTable 存在空键、重复键或无效基础字段。");
                levelRows.Add(row.Key, row);
            }

            var rounds = new Dictionary<string, List<LevelRoundTable>>(StringComparer.Ordinal);
            foreach (LevelRoundTable row in GF.DataTable.GetDataTable<LevelRoundTable>().GetAllDataRows())
            {
                if (row == null || !levelRows.ContainsKey(row.LevelKey) || row.RoundIndex < 0 || row.TapCount <= 0 || row.TargetScore < 0)
                    throw new InvalidOperationException("Board/LevelRoundTable 存在无效关卡外键或轮次字段。");
                AddRow(rounds, row.LevelKey, row);
            }

            var goals = new Dictionary<string, List<LevelGoalTable>>(StringComparer.Ordinal);
            foreach (LevelGoalTable row in GF.DataTable.GetDataTable<LevelGoalTable>().GetAllDataRows())
            {
                if (row == null || !levelRows.ContainsKey(row.LevelKey) || string.IsNullOrEmpty(row.GoalKey) || row.RoundIndex < 0 || row.GoalKind < (int)GoalKind.None || row.GoalKind > (int)GoalKind.PushedInto || row.Required < 0)
                    throw new InvalidOperationException("Board/LevelGoalTable 存在无效关卡外键、目标键或枚举值。");
                AddRow(goals, row.LevelKey + "|" + row.RoundIndex, row);
            }

            var keyPieces = new Dictionary<string, List<LevelKeyPieceTable>>(StringComparer.Ordinal);
            foreach (LevelKeyPieceTable row in GF.DataTable.GetDataTable<LevelKeyPieceTable>().GetAllDataRows())
            {
                if (row == null || !levelRows.ContainsKey(row.LevelKey) || row.SortOrder < 0 || !Enum.IsDefined(typeof(PartType), (PartType)row.PartKind))
                    throw new InvalidOperationException("Board/LevelKeyPieceTable 存在无效关卡外键、排序或零件枚举。");
                AddRow(keyPieces, row.LevelKey, row);
            }

            var carries = new Dictionary<string, List<LevelCarryPartTable>>(StringComparer.Ordinal);
            foreach (LevelCarryPartTable row in GF.DataTable.GetDataTable<LevelCarryPartTable>().GetAllDataRows())
            {
                if (row == null || !levelRows.ContainsKey(row.LevelKey) || row.SortOrder < 0 || !Enum.IsDefined(typeof(PartType), (PartType)row.PartKind))
                    throw new InvalidOperationException("Board/LevelCarryPartTable 存在无效关卡外键、排序或零件枚举。");
                AddRow(carries, row.LevelKey, row);
            }

            var fixedElements = new Dictionary<string, List<LevelFixedElementTable>>(StringComparer.Ordinal);
            foreach (LevelFixedElementTable row in GF.DataTable.GetDataTable<LevelFixedElementTable>().GetAllDataRows())
            {
                if (row == null || !levelRows.ContainsKey(row.LevelKey) || row.ElementKind < (int)FixedElementKind.Facility || row.ElementKind > (int)FixedElementKind.RepairTarget || row.ObstacleType < (int)ObstacleType.None || row.ObstacleType > (int)ObstacleType.PressurePillar || row.GoalKind < (int)GoalKind.None || row.GoalKind > (int)GoalKind.PushedInto || row.RepairRequired < 0 || row.RepairEnergyCost < 0)
                    throw new InvalidOperationException("Board/LevelFixedElementTable 存在无效关卡外键、元素枚举或维修参数。");
                AddRow(fixedElements, row.LevelKey, row);
            }

            var result = new List<LevelPreset>();
            foreach (LevelTable row in levelRows.Values)
            {
                var roundRows = rounds.TryGetValue(row.Key, out List<LevelRoundTable> configuredRounds) ? configuredRounds : new List<LevelRoundTable>();
                roundRows.Sort((a, b) => a.RoundIndex.CompareTo(b.RoundIndex));
                var roundConfigs = new List<RoundConfig>();
                for (int i = 0; i < roundRows.Count; i++)
                {
                    if (roundRows[i].RoundIndex != i) throw new InvalidOperationException("Board/LevelRoundTable 轮次不连续：" + row.Key);
                    var roundGoals = new List<SpecialGoalConfig>();
                    if (goals.TryGetValue(row.Key + "|" + i, out List<LevelGoalTable> configuredGoals))
                    {
                        foreach (LevelGoalTable goal in configuredGoals) roundGoals.Add(new SpecialGoalConfig(goal.GoalKey, goal.Required));
                    }
                    roundConfigs.Add(new RoundConfig(roundRows[i].TapCount, roundRows[i].TargetScore, roundGoals));
                }
                if (roundConfigs.Count == 0) throw new InvalidOperationException("Board/LevelTable 缺少轮次关系：" + row.Key);

                var keyConfigs = new List<KeyPieceConfig>();
                if (keyPieces.TryGetValue(row.Key, out List<LevelKeyPieceTable> configuredKeys))
                {
                    configuredKeys.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
                    foreach (LevelKeyPieceTable key in configuredKeys) keyConfigs.Add(new KeyPieceConfig((PartType)key.PartKind, new HexCoord(key.Q, key.R)));
                }
                var carryPool = new List<PartType>();
                var borrowed = new List<PartType>();
                if (carries.TryGetValue(row.Key, out List<LevelCarryPartTable> configuredCarry))
                {
                    configuredCarry.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
                    foreach (LevelCarryPartTable carry in configuredCarry)
                    {
                        PartType part = (PartType)carry.PartKind;
                        if (!carryPool.Contains(part)) carryPool.Add(part);
                        if (carry.IsBorrowed && !borrowed.Contains(part)) borrowed.Add(part);
                    }
                }
                var fixedConfigs = new List<FixedElementConfig>();
                if (fixedElements.TryGetValue(row.Key, out List<LevelFixedElementTable> configuredFixed))
                {
                    foreach (LevelFixedElementTable element in configuredFixed)
                    {
                        GoalConfig goal = string.IsNullOrEmpty(element.GoalKey) ? null : new GoalConfig(element.GoalKey, (GoalKind)element.GoalKind, element.GoalEntityId, element.GoalAnchorLabel, element.GoalRequired);
                        RepairTargetConfig repair = null;
                        if (element.ElementKind == (int)FixedElementKind.RepairTarget)
                            repair = new RepairTargetConfig(ParsePartKinds(element.RepairPartKinds), element.RepairRequired, element.RepairEnergyCost, defaultRepairScore);
                        fixedConfigs.Add(new FixedElementConfig((FixedElementKind)element.ElementKind, new HexCoord(element.Q, element.R), (ObstacleType)element.ObstacleType, element.Label, goal, repair));
                    }
                }
                var board = new LevelBoardConfig(row.Name, row.Radius, keyConfigs, carryPool, row.InitialPartCount, row.InitialArmMoves, row.InitialPublicRepairEnergy, borrowed, fixedElements: fixedConfigs, objective: row.Objective, duration: row.Duration);
                result.Add(new LevelPreset(row.Key, row.EventKey, board, new LevelConfig(roundConfigs)));
            }
            return result.AsReadOnly();
        }

        private static int GetCalibrationValue(IReadOnlyList<CalibrationBound> calibrations, string id)
        {
            foreach (CalibrationBound calibration in calibrations)
            {
                if (calibration.Id == id)
                {
                    return calibration.Value;
                }
            }

            throw new InvalidOperationException("Meta/CalibrationTable 缺少校准项：" + id);
        }

        private static void AddRow<T>(Dictionary<string, List<T>> target, string key, T row)
        {
            if (!target.TryGetValue(key, out List<T> rows))
            {
                rows = new List<T>();
                target.Add(key, rows);
            }
            rows.Add(row);
        }

        private static IReadOnlyList<PartType> ParsePartKinds(string text)
        {
            var parts = new List<PartType>();
            if (string.IsNullOrEmpty(text)) return parts;
            foreach (string token in text.Split(','))
            {
                if (!int.TryParse(token.Trim(), out int value) || !Enum.IsDefined(typeof(PartType), (PartType)value))
                    throw new InvalidOperationException("Board/LevelFixedElementTable 的 RepairPartKinds 无效：" + text);
                parts.Add((PartType)value);
            }
            return parts;
        }

    }
}
