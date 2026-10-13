using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>
    /// 一次配置加载得到的纯领域快照。它不包含 Unity 对象、存档状态或当前盘面状态。
    /// </summary>
    public sealed class GameConfigSnapshot
    {
        public IReadOnlyList<PartConfig> Parts { get; }
        public IReadOnlyList<PartCodexConfig> PartCodex { get; }
        public IReadOnlyList<MaterialConfig> Materials { get; }
        public IReadOnlyList<FormConfig> Forms { get; }
        public IReadOnlyList<BuffConfig> Buffs { get; }
        public IReadOnlyList<AnomalyConfig> Anomalies { get; }
        public IReadOnlyList<ReleaseEntry> Releases { get; }
        public IReadOnlyList<PlaceConfig> Places { get; }
        public IReadOnlyList<SupplyCandidate> Supplies { get; }
        public IReadOnlyList<TrialConfig> Trials { get; }
        public IReadOnlyList<TaskConfig> Tasks { get; }
        public IReadOnlyList<NormalEventConfig> NormalEvents { get; }
        public IReadOnlyList<RewardMaterialEntry> Rewards { get; }
        public IReadOnlyList<CaseConfig> Cases { get; }
        public IReadOnlyList<InvestigationConfig> Investigations { get; }
        public IReadOnlyList<CopyEntry> Copies { get; }
        public IReadOnlyList<PortraitConfig> Portraits { get; }
        public IReadOnlyList<TutorialHintConfig> TutorialHints { get; }
        public IReadOnlyList<CalibrationBound> Calibrations { get; }
        public IReadOnlyList<ObstacleConfig> Obstacles { get; }
        public IReadOnlyList<AudioCueConfig> AudioCues { get; }
        public IReadOnlyList<BgmConfig> Bgms { get; }
        public IReadOnlyList<AssetSlot> AssetBindings { get; }
        public IReadOnlyList<NarrativeEntry> Narratives { get; }
        public IReadOnlyList<CaseEventConfig> CaseEvents { get; }
        public IReadOnlyList<BatchConfig> Batches { get; }
        public IReadOnlyList<CaseRewardOptionConfig> CaseRewardOptions { get; }
        public IReadOnlyList<ChallengeConfig> Challenges { get; }
        public GameConfigSnapshot(IReadOnlyList<PartConfig> parts, IReadOnlyList<PartCodexConfig> partCodex, IReadOnlyList<MaterialConfig> materials, IReadOnlyList<FormConfig> forms, IReadOnlyList<BuffConfig> buffs, IReadOnlyList<AnomalyConfig> anomalies, IReadOnlyList<ReleaseEntry> releases, IReadOnlyList<PlaceConfig> places, IReadOnlyList<SupplyCandidate> supplies, IReadOnlyList<TrialConfig> trials, IReadOnlyList<TaskConfig> tasks, IReadOnlyList<NormalEventConfig> normalEvents, IReadOnlyList<RewardMaterialEntry> rewards, IReadOnlyList<CaseConfig> cases, IReadOnlyList<InvestigationConfig> investigations, IReadOnlyList<CopyEntry> copies, IReadOnlyList<PortraitConfig> portraits, IReadOnlyList<TutorialHintConfig> tutorialHints, IReadOnlyList<CalibrationBound> calibrations, IReadOnlyList<ObstacleConfig> obstacles, IReadOnlyList<AudioCueConfig> audioCues, IReadOnlyList<BgmConfig> bgms, IReadOnlyList<AssetSlot> assetBindings, IReadOnlyList<NarrativeEntry> narratives, IReadOnlyList<CaseEventConfig> caseEvents, IReadOnlyList<BatchConfig> batches, IReadOnlyList<CaseRewardOptionConfig> caseRewardOptions, IReadOnlyList<ChallengeConfig> challenges)
        {
            Parts = parts ?? throw new ArgumentNullException(nameof(parts));
            PartCodex = partCodex ?? throw new ArgumentNullException(nameof(partCodex));
            Materials = materials ?? throw new ArgumentNullException(nameof(materials));
            Forms = forms ?? throw new ArgumentNullException(nameof(forms));
            Buffs = buffs ?? throw new ArgumentNullException(nameof(buffs));
            Anomalies = anomalies ?? throw new ArgumentNullException(nameof(anomalies));
            Releases = releases ?? throw new ArgumentNullException(nameof(releases));
            Places = places ?? throw new ArgumentNullException(nameof(places));
            Supplies = supplies ?? throw new ArgumentNullException(nameof(supplies));
            Trials = trials ?? throw new ArgumentNullException(nameof(trials));
            Tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            NormalEvents = normalEvents ?? throw new ArgumentNullException(nameof(normalEvents));
            Rewards = rewards ?? throw new ArgumentNullException(nameof(rewards));
            Cases = cases ?? throw new ArgumentNullException(nameof(cases));
            Investigations = investigations ?? throw new ArgumentNullException(nameof(investigations));
            Copies = copies ?? throw new ArgumentNullException(nameof(copies));
            Portraits = portraits ?? throw new ArgumentNullException(nameof(portraits));
            TutorialHints = tutorialHints ?? throw new ArgumentNullException(nameof(tutorialHints));
            Calibrations = calibrations ?? throw new ArgumentNullException(nameof(calibrations));
            Obstacles = obstacles ?? throw new ArgumentNullException(nameof(obstacles));
            AudioCues = audioCues ?? throw new ArgumentNullException(nameof(audioCues));
            Bgms = bgms ?? throw new ArgumentNullException(nameof(bgms));
            AssetBindings = assetBindings ?? throw new ArgumentNullException(nameof(assetBindings));
            Narratives = narratives ?? throw new ArgumentNullException(nameof(narratives));
            CaseEvents = caseEvents ?? throw new ArgumentNullException(nameof(caseEvents));
            Batches = batches ?? throw new ArgumentNullException(nameof(batches));
            CaseRewardOptions = caseRewardOptions ?? throw new ArgumentNullException(nameof(caseRewardOptions));
            Challenges = challenges ?? throw new ArgumentNullException(nameof(challenges));
        }
    }
}
