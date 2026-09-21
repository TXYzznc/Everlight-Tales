namespace Everlight.Tales.Data
{
    /// <summary>
    /// 零件种类（P4-007 补齐 20 种通用零件 P-001~P-020）。
    /// 非零件实体（固定设施／任务标记／维修对象）与惰性普通零件用 None。
    /// 数值与行为仅 P-001~P-004、P-014 已实现（PartCatalog/PartAbility）；其余为图鉴登记占位，
    /// 行为与数值待对应内容批次（b30+ 四条改装支线及后续）接入，默认按惰性零件处理。
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

        /// <summary>P-004 换向齿轮：把来撞实体沿入射方向偏转 60° 推进 1 格。</summary>
        ReversalGear = 4,

        /// <summary>P-014 铆合钳：受击／信号时维修 D0 邻格的可修复节点。</summary>
        RivetPliers = 5,

        /// <summary>P-005 弹射簧：把一次向下运动重新送回上游，创造回弹接力。</summary>
        SpringLauncher = 6,

        /// <summary>P-006 分裂铸模：把一枚普通件变成两枚，扩大连锁规模。</summary>
        SplitMold = 7,

        /// <summary>P-007 储料胃袋：先收起零件，等路线合适再吐出，改变连锁时机。</summary>
        StorageStomach = 8,

        /// <summary>P-008 吞料炉：牺牲普通件清理空间，把拥堵转成当前作业能量。</summary>
        MaterialFurnace = 9,

        /// <summary>P-009 交换拨叉：连锁中交换两枚对象，改变后续触发位置。</summary>
        SwapFork = 10,

        /// <summary>P-010 旋涡转子：整体重排六邻格，让一圈零件换到新位置。</summary>
        VortexRotor = 11,

        /// <summary>P-011 蓄能飞轮：把分散小碰撞积成一次大释放，承接跨拍准备。</summary>
        EnergyFlywheel = 12,

        /// <summary>P-012 导电桥：让隔着空隙的设备互相触发，维持通信线路。</summary>
        ConductiveBridge = 13,

        /// <summary>P-013 照明棱镜：用连锁获得可操作情报，处理伪装和视野异常。</summary>
        LightingPrism = 14,

        /// <summary>P-015 叩击音叉：把碰撞转成敲门、呼应和引导，支撑声音类挑战。</summary>
        TuningFork = 15,

        /// <summary>P-016 排水叶轮：把积压水负荷转成排水与动力，为水灾连锁提供核心转换。</summary>
        DrainImpeller = 16,

        /// <summary>P-017 缓冲囊：承接可转移的设备负荷，为集中释放争取空间。</summary>
        BufferBladder = 17,

        /// <summary>P-018 校准探针：低成本试探确定出口或目标关系，承担设备校准。</summary>
        CalibrationProbe = 18,

        /// <summary>P-019 磁吸牵引器：主动把远处对象拉近，接上本来碰不到的连锁。</summary>
        MagneticTractor = 19,

        /// <summary>P-020 接力电池：延长关键器械工作次数，让持续零件支撑更长连锁。</summary>
        RelayBattery = 20,
    }
}
