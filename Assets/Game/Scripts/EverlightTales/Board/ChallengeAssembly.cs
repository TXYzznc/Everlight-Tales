using System;
using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>只读装配描述；每次创建新的专属状态，不在目录中共享运行实体。</summary>
    public sealed class ChallengeAssembly
    {
        public ChallengeConfig Challenge { get; }
        public LevelPreset Level { get; }
        private readonly RedShoePreset _redShoe;
        internal ChallengeAssembly(ChallengeConfig challenge, LevelPreset level, RedShoePreset redShoe)
        { Challenge = challenge; Level = level; _redShoe = redShoe; }
        public RedShoeLevelConfig CreateConfig() => RedShoeLevelConfig.From(Level, _redShoe);
        public RedShoeLevel CreateRuntime() => RedShoeLevelBuilder.Build(CreateConfig());
    }

    /// <summary>受控RuleId注册。后置挑战没有装配项，不能借用默认红鞋或反射任意类型。</summary>
    public static class ChallengeAssemblyCatalog
    {
        private static readonly Dictionary<string, string> RuleCases = new Dictionary<string, string>(StringComparer.Ordinal)
        { { "CR-001", "L-01" } };
        private static IReadOnlyList<ChallengeAssembly> _all = Array.Empty<ChallengeAssembly>();
        public static IReadOnlyList<ChallengeAssembly> All => _all;
        public static bool IsEnabled(ChallengeConfig challenge) => challenge != null && challenge.Extra.GetBool("Enabled", challenge.HasLevel);

        public static IReadOnlyList<ChallengeAssembly> Prepare(IReadOnlyList<ChallengeConfig> challenges,
            IReadOnlyList<CaseConfig> cases, IReadOnlyList<ReleaseEntry> releases,
            IReadOnlyList<LevelPreset> levels, IReadOnlyList<RedShoePreset> redShoes)
        {
            var result = new List<ChallengeAssembly>();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            var caseKeys = new HashSet<string>(StringComparer.Ordinal);
            var boundLevels = new HashSet<string>(StringComparer.Ordinal);
            var schema = new Dictionary<string, ExtraParamKind> { { "Enabled", ExtraParamKind.Bool } };
            foreach (ChallengeConfig challenge in challenges)
            {
                if (challenge == null || string.IsNullOrEmpty(challenge.Key) || string.IsNullOrEmpty(challenge.CaseKey)
                    || !keys.Add(challenge.Key) || !caseKeys.Add(challenge.CaseKey)) Fail(challenge, "挑战键/案件键为空或重复绑定");
                challenge.Extra.Validate("挑战 " + challenge.Key, schema);
                StageLevel stage = default;
                if (challenge.Kind != ChallengeKind.CaseExclusive || !Enum.TryParse(challenge.Stage, out stage)
                    || !Enum.IsDefined(typeof(StageLevel), stage)) Fail(challenge, "挑战类型或投放阶段非法");
                ReleaseEntry release = null;
                foreach (ReleaseEntry entry in releases) if (entry.Id == challenge.Key)
                { if (release != null) Fail(challenge, "投放引用重复"); release = entry; }
                if (release == null || release.Kind != ReleaseKind.Challenge || release.Stage != stage)
                    Fail(challenge, "StageReleaseTable缺少匹配Challenge类型/阶段的投放记录");
                if (!IsEnabled(challenge)) continue;
                if (!challenge.HasLevel) Fail(challenge, "启用挑战未指定LevelKey");
                bool hasCase = false;
                foreach (CaseConfig entry in cases) if (entry.Id == challenge.CaseKey) hasCase = true;
                if (!hasCase) Fail(challenge, "CaseTable未配置案件 " + challenge.CaseKey);
                if (!RuleCases.TryGetValue(challenge.RuleId ?? "", out string ruleCase) || ruleCase != challenge.CaseKey)
                    Fail(challenge, "RuleId未注册或与案件不匹配：" + challenge.RuleId);
                LevelPreset level = null; RedShoePreset shoe = null;
                foreach (LevelPreset entry in levels) if (entry.Key == challenge.LevelKey)
                { if (level != null) Fail(challenge, "LevelKey重复"); level = entry; }
                foreach (RedShoePreset entry in redShoes) if (entry.LevelKey == challenge.LevelKey)
                { if (shoe != null) Fail(challenge, "专属规则LevelKey重复"); shoe = entry; }
                if (level == null || shoe == null || !boundLevels.Add(challenge.LevelKey)) Fail(challenge, "关卡/专属规则引用缺失或重复：" + challenge.LevelKey);
                ValidateRedShoe(challenge, level, shoe);
                result.Add(new ChallengeAssembly(challenge, level, shoe));
            }
            return result.AsReadOnly();
        }

        private static void ValidateRedShoe(ChallengeConfig challenge, LevelPreset level, RedShoePreset shoe)
        {
            var shape = new HexBoardShape(level.Board.BoardRadius);
            if (!shape.IsNormal(shoe.StartCoord) || !shape.IsNormal(shoe.BoxCoord) || !shape.IsNormal(shoe.EntryCoord)
                || !Enum.IsDefined(typeof(HexDirection), shoe.StartDirection) || shoe.BoxRequired <= 0
                || shoe.RequiredDiversion <= 0 || shoe.RestraintLimit <= 0 || shoe.StepsPerTap <= 0
                || level.Level.Rounds.Count != 3 || level.Board.InitialPartCount != level.Board.KeyPieces.Count) Fail(challenge, "红鞋规则坐标/必需数值/轮数非法");
            var guides = new HashSet<HexCoord>();
            foreach (HexCoord coord in shoe.GuideCells) if (!shape.IsNormal(coord) || !guides.Add(coord)) Fail(challenge, "导流格越界或重复");
            if (guides.Count < shoe.RequiredDiversion) Fail(challenge, "导流格数量不足");
            var turns = new HashSet<HexCoord>();
            foreach (RedShoeTurnCell turn in shoe.TurnCells)
                if (!shape.IsNormal(turn.Coord) || !turns.Add(turn.Coord) || !Enum.IsDefined(typeof(HexDirection), turn.Direction))
                    Fail(challenge, "转向格越界/重复/方向非法");
            int boxes = 0;
            foreach (FixedElementConfig element in level.Board.FixedElements)
                if (element.Kind == FixedElementKind.RepairTarget && element.Position == shoe.BoxCoord
                    && element.RepairTarget != null && element.RepairTarget.RequiredProgress == shoe.BoxRequired) boxes++;
            if (boxes != 1) Fail(challenge, "封存匣位置/维修进度与固定元素表不一致");
            foreach (RoundConfig round in level.Level.Rounds)
            {
                if (round.TapCount <= 0 || round.TargetScore < 0) Fail(challenge, "轮数值非法");
                var goals = new HashSet<string>();
                foreach (SpecialGoalConfig goal in round.Goals)
                    if (!goals.Add(goal.Id) || goal.Required < 0 || goal.Id != "diversion" && goal.Id != "box_repaired" && goal.Id != "sealed")
                        Fail(challenge, "专属轮目标未知或重复：" + goal.Id);
            }
        }
        public static void Install(IReadOnlyList<ChallengeAssembly> prepared) => _all = prepared ?? throw new ArgumentNullException(nameof(prepared));
        public static ChallengeAssembly GetByCase(string caseKey)
        {
            foreach (ChallengeAssembly assembly in _all) if (assembly.Challenge.CaseKey == caseKey) return assembly;
            return null;
        }
        public static ChallengeAssembly RequireCase(string caseKey) => GetByCase(caseKey)
            ?? throw new InvalidOperationException("案件挑战未启用或未完成装配：" + caseKey);
        private static void Fail(ChallengeConfig challenge, string reason) => throw new InvalidOperationException(
            "Board/ChallengeTable " + (challenge?.Key ?? "<null>") + " (" + challenge?.CaseKey + ")：" + reason);
    }
}
