using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>Buff 静态配置（D-057，P1-019 / P2-005）：叠加资格、上限、效果与玩家可见描述。</summary>
    public sealed class BuffConfig
    {
        public string Id { get; }

        public string Name { get; }

        /// <summary>最早进入内容池的玩家阶段（S1/S2/S3）。</summary>
        public string Stage { get; }

        public string Description { get; }

        public bool Stackable { get; }

        public int MaxStacks { get; }

        /// <summary>效果类型。</summary>
        public BuffEffectKind Effect { get; }

        /// <summary>每层效果数值。</summary>
        public int Value { get; }

        /// <summary>作用零件（None=全部或非专属）。</summary>
        public PartType TargetPart { get; }

        public BuffConfig(string id, string name, string stage, bool stackable, int maxStacks, string description, BuffEffectKind effect = BuffEffectKind.None, int value = 0, PartType targetPart = PartType.None)
        {
            Id = id;
            Name = name;
            Stage = stage;
            Stackable = stackable;
            MaxStacks = maxStacks;
            Description = description;
            Effect = effect;
            Value = value;
            TargetPart = targetPart;
        }
    }

    /// <summary>首批 20 项 Buff 目录（BF-001～BF-020，见 02-系统设计/13-Buff库.md）。</summary>
    public static class BuffCatalog
    {
        private static readonly IReadOnlyList<BuffConfig> AllEntries = new BuffConfig[]
        {
            E("BF-001", "精密计量", "S1", 5, "计量棘轮每次触发分+2（最多叠加5次）。", BuffEffectKind.TriggerScore, 2, PartType.MeteringRatchet),
            E("BF-002", "全盘校准", "S2", 3, "场上所有零件每次触发分+1（最多叠加3次）。", BuffEffectKind.TriggerScore, 1, PartType.None),
            E("BF-003", "重击记录", "S1", 5, "惯性撞锤每实际反推1格，效果分+4（最多叠加5次）。", BuffEffectKind.EffectScorePerUnit, 4, PartType.InertiaHammer),
            E("BF-004", "爆点记录", "S1", 5, "爆破线圈每波及一个有效对象，效果分+3（最多叠加5次）。", BuffEffectKind.EffectScorePerUnit, 3, PartType.BlastCoil),
            E("BF-005", "转向记录", "S1", 5, "换向齿轮每次成功换向推进，效果分+4（最多叠加5次）。", BuffEffectKind.EffectScorePerUnit, 4, PartType.ReversalGear),
            E("BF-006", "飞轮测量", "S2", 5, "蓄能飞轮释放时，每个射线命中对象的效果分+3（最多叠加5次）。", BuffEffectKind.EffectScorePerUnit, 3, PartType.None),
            E("BF-007", "清晰显影", "S2", 5, "照明棱镜每揭露1层伪装，效果分+4（最多叠加5次）。", BuffEffectKind.EffectScorePerUnit, 4, PartType.None),
            E("BF-008", "排水记录", "S2", 5, "排水叶轮每实际排掉1单位水，效果分+4（最多叠加5次）。", BuffEffectKind.EffectScorePerUnit, 4, PartType.None),
            E("BF-009", "牵引记录", "S2", 5, "磁吸牵引器每实际牵引1格，效果分+3（最多叠加5次）。", BuffEffectKind.EffectScorePerUnit, 3, PartType.None),
            E("BF-010", "补能记录", "S2", 5, "接力电池每实际转移1点能量，效果分+4（最多叠加5次）。", BuffEffectKind.EffectScorePerUnit, 4, PartType.None),
            E("BF-011", "增产棘轮", "S2", 3, "计量棘轮每次成功产能，额外产生1点公共维修能量（最多叠加3次）。", BuffEffectKind.PublicEnergyPerTrigger, 1, PartType.MeteringRatchet),
            E("BF-012", "扩容补能", "S2", 3, "有自身能量槽的零件容量+1，场上已有零件立即补能1；后续生成按条目获得对应加成（最多叠加3次）。", BuffEffectKind.CapacityEnergy, 1, PartType.None),
            E("BF-013", "充足备件", "S2", 3, "之后通过补给生成的有能量槽零件，初始能量与容量各+1（最多叠加3次）。", BuffEffectKind.NewSpawnCapacity, 1, PartType.None),
            E("BF-014", "回收余振", "S2", 3, "每拍结束时产生1点公共维修能量（最多叠加3次）。", BuffEffectKind.TapEndEnergy, 1, PartType.None),
            E("BF-015", "轮间保养", "S2", 3, "后续每轮开始时，场上有能量槽的零件各恢复1点能量，不超过容量（最多叠加3次）。", BuffEffectKind.RoundStartEnergy, 1, PartType.None),
            E("BF-016", "长程撞锤", "S2", 3, "惯性撞锤反推距离+1格（最多叠加3次）。", BuffEffectKind.Distance, 1, PartType.InertiaHammer),
            E("BF-017", "加长弹簧", "S3", 3, "弹射簧弹射距离+1格（最多叠加3次）。", BuffEffectKind.Distance, 1, PartType.None),
            E("BF-018", "远距磁吸", "S2", 3, "磁吸牵引器搜索距离+1格，牵引距离保持原值（最多叠加3次）。", BuffEffectKind.Distance, 1, PartType.None),
            E("BF-019", "大口径叶轮", "S2", 3, "排水叶轮单次排水上限+1（最多叠加3次）。", BuffEffectKind.Distance, 1, PartType.None),
            E("BF-020", "轮间调臂", "S2", 3, "后续每轮开始时，机械臂次数+1（最多叠加3次）。", BuffEffectKind.RoundStartArm, 1, PartType.None),
        };

        private static readonly Dictionary<string, BuffConfig> ById = BuildIndex();

        public static IReadOnlyList<BuffConfig> All => AllEntries;

        public static BuffConfig Get(string id)
        {
            ById.TryGetValue(id, out BuffConfig config);
            return config;
        }

        private static Dictionary<string, BuffConfig> BuildIndex()
        {
            var index = new Dictionary<string, BuffConfig>();
            foreach (BuffConfig config in AllEntries)
            {
                index[config.Id] = config;
            }

            return index;
        }

        private static BuffConfig E(string id, string name, string stage, int maxStacks, string description, BuffEffectKind effect = BuffEffectKind.None, int value = 0, PartType targetPart = PartType.None)
        {
            return new BuffConfig(id, name, stage, stackable: true, maxStacks, description, effect, value, targetPart);
        }
    }
}
