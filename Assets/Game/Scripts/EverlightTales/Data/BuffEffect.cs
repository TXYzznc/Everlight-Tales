using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>Buff 效果类型（P2-005）。每项改变一个主要参数或在明确时点执行一次资源操作。</summary>
    public enum BuffEffectKind : byte
    {
        None = 0,

        /// <summary>触发分 +N（BF-001 精密计量／BF-002 全盘校准）。</summary>
        TriggerScore = 1,

        /// <summary>自身效果分每单位 +N（BF-003～BF-010，各目标一专属零件）。</summary>
        EffectScorePerUnit = 2,

        /// <summary>每次成功产能 +N 公共维修能量（BF-011 增产棘轮）。</summary>
        PublicEnergyPerTrigger = 3,

        /// <summary>有能量槽零件容量与当前能量 +N（BF-012 扩容补能）。</summary>
        CapacityEnergy = 4,

        /// <summary>新补给生成件初始能量与容量 +N（BF-013 充足备件）。</summary>
        NewSpawnCapacity = 5,

        /// <summary>每拍末产公共维修能量 +N（BF-014 回收余振）。</summary>
        TapEndEnergy = 6,

        /// <summary>下一轮开始场上有能量槽零件各恢复 +N（BF-015 轮间保养）。</summary>
        RoundStartEnergy = 7,

        /// <summary>位移／搜索距离 +N（BF-016～BF-019，各目标一专属零件）。</summary>
        Distance = 8,

        /// <summary>下一轮开始机械臂次数 +N（BF-020 轮间调臂）。</summary>
        RoundStartArm = 9,
    }

    /// <summary>
    /// 从已持有 Buff 集解析出的结算加成（P2-005）。Board 层只读，Data 类型供 Board 引用。
    /// 零件专属加成按 PartType 查询；TriggerScore 的 TargetPart==None 表示作用于全部零件。
    /// </summary>
    public sealed class BuffBonuses
    {
        public static readonly BuffBonuses Empty = new BuffBonuses();

        private readonly Dictionary<PartType, int> _trigger = new Dictionary<PartType, int>();
        private readonly Dictionary<PartType, int> _effectUnit = new Dictionary<PartType, int>();
        private readonly Dictionary<PartType, int> _energy = new Dictionary<PartType, int>();
        private readonly Dictionary<PartType, int> _distance = new Dictionary<PartType, int>();

        /// <summary>作用于全部零件的触发分加成（BF-002）。</summary>
        public int AllTrigger { get; set; }

        /// <summary>有能量槽零件容量加成（BF-012）。</summary>
        public int CapacityBonus { get; set; }

        /// <summary>每拍末公共能量加成（BF-014）。</summary>
        public int TapEndEnergy { get; set; }

        /// <summary>轮间能量恢复加成（BF-015）。</summary>
        public int RoundStartEnergy { get; set; }

        /// <summary>轮间机械臂加成（BF-020）。</summary>
        public int RoundStartArm { get; set; }

        /// <summary>新补给生成件容量加成（BF-013）。</summary>
        public int NewSpawnCapacity { get; set; }

        public void AddTrigger(PartType part, int value)
        {
            if (part == PartType.None)
            {
                AllTrigger += value;
                return;
            }

            _trigger[part] = Get(_trigger, part) + value;
        }

        public void AddEffectUnit(PartType part, int value)
        {
            if (part == PartType.None)
            {
                return; // 效果分专属类不含「全部零件」语义。
            }

            _effectUnit[part] = Get(_effectUnit, part) + value;
        }

        public void AddEnergy(PartType part, int value)
        {
            _energy[part] = Get(_energy, part) + value;
        }

        public void AddDistance(PartType part, int value)
        {
            _distance[part] = Get(_distance, part) + value;
        }

        public int TriggerBonus(PartType part)
        {
            return AllTrigger + Get(_trigger, part);
        }

        public int EffectUnitBonus(PartType part)
        {
            return Get(_effectUnit, part);
        }

        public int PublicEnergyBonus(PartType part)
        {
            return Get(_energy, part);
        }

        public int DistanceBonus(PartType part)
        {
            return Get(_distance, part);
        }

        private static int Get(Dictionary<PartType, int> map, PartType part)
        {
            map.TryGetValue(part, out int value);
            return value;
        }
    }
}
