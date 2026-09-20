namespace Everlight.Tales.Data
{
    /// <summary>
    /// 零件种类。首批三件（P1-007）：惯性撞锤、计量棘轮、爆破线圈。
    /// 非零件实体（固定设施／任务标记／维修对象）与惰性普通零件用 None。
    /// </summary>
    public enum PartType : byte
    {
        /// <summary>无能力：惰性零件或非零件实体。</summary>
        None = 0,

        /// <summary>P-001 惯性撞锤：把碰撞变成对来撞实体的一次反向推移。</summary>
        InertiaHammer = 1,

        /// <summary>P-002 计量棘轮：把受击转化为公共维修能量与连锁分。</summary>
        MeteringRatchet = 2,

        /// <summary>P-003 爆破线圈：受击起爆，向六邻格发冲击后移除本体。</summary>
        BlastCoil = 3,
    }
}
