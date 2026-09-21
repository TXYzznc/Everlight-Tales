using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>
    /// 零件图鉴条目元数据（P4-007，D-081）：20 种通用零件的显示信息。
    /// 只读参考数据——名称/阶段/来源提示；战斗数值（触发分/能量等）在已拥有时另读 PartCatalog。
    /// 不新增战斗配置，与 PartCatalog/FormCatalog 共用同一零件与形态标识（PartType / FormConfig）。
    /// </summary>
    public sealed class PartCodexConfig
    {
        public string Id;          // "P-001"
        public string Name;        // "惯性撞锤"
        public PartType Type;
        public string Stage;       // "S0 开店教学"
        public string SourceHint;  // 来源方向一句话

        public PartCodexConfig(string id, string name, PartType type, string stage, string sourceHint)
        {
            Id = id;
            Name = name;
            Type = type;
            Stage = stage;
            SourceHint = sourceHint;
        }
    }

    /// <summary>20 种通用零件图鉴目录（按 P-ID 排序，P4-007）。</summary>
    public static class PartCodexCatalog
    {
        private static readonly IReadOnlyList<PartCodexConfig> _parts = new[]
        {
            new PartCodexConfig("P-001", "惯性撞锤", PartType.InertiaHammer, "S0 开店教学", "工作台第一段：先完成一次移动碰撞。"),
            new PartCodexConfig("P-002", "计量棘轮", PartType.MeteringRatchet, "S0 开店教学", "工作台第二段：在撞锤之后展示能量收益。"),
            new PartCodexConfig("P-003", "爆破线圈", PartType.BlastCoil, "S0 开店教学", "工作台最后一段：接上爆破与脆裂隔板。"),
            new PartCodexConfig("P-004", "换向齿轮", PartType.ReversalGear, "S1 红鞋首案", "红鞋首关现场演示后永久解锁。"),
            new PartCodexConfig("P-005", "弹射簧", PartType.SpringLauncher, "S3 城市中期", "第二章普通弹簧维修。"),
            new PartCodexConfig("P-006", "分裂铸模", PartType.SplitMold, "S4 画壁筹备", "第三章加工台进阶委托。"),
            new PartCodexConfig("P-007", "储料胃袋", PartType.StorageStomach, "S3 城市中期", "穿墙案完成后的回收试机。"),
            new PartCodexConfig("P-008", "吞料炉", PartType.MaterialFurnace, "S3 城市中期", "第二章仓库整理委托。"),
            new PartCodexConfig("P-009", "交换拨叉", PartType.SwapFork, "S4 画壁筹备", "风月案完成后工作台演示。"),
            new PartCodexConfig("P-010", "旋涡转子", PartType.VortexRotor, "S4 画壁筹备", "交换拨叉试机后六邻格重排。"),
            new PartCodexConfig("P-011", "蓄能飞轮", PartType.EnergyFlywheel, "S2 街区初期", "扫帚初次处理后的工作台试机。"),
            new PartCodexConfig("P-012", "导电桥", PartType.ConductiveBridge, "S3 城市中期", "猴爪案调查准备。"),
            new PartCodexConfig("P-013", "照明棱镜", PartType.LightingPrism, "S2 街区初期", "画皮案识别环节。"),
            new PartCodexConfig("P-014", "铆合钳", PartType.RivetPliers, "S1 红鞋首案", "红鞋准备阶段由周衡借出。"),
            new PartCodexConfig("P-015", "叩击音叉", PartType.TuningFork, "S2 街区初期", "裂口女案白天准备后段。"),
            new PartCodexConfig("P-016", "排水叶轮", PartType.DrainImpeller, "S2 街区初期", "扫帚案前段现场提供。"),
            new PartCodexConfig("P-017", "缓冲囊", PartType.BufferBladder, "S4 画壁筹备", "画像案前段现场提供。"),
            new PartCodexConfig("P-018", "校准探针", PartType.CalibrationProbe, "S3 城市中期", "穿墙案白天调查。"),
            new PartCodexConfig("P-019", "磁吸牵引器", PartType.MagneticTractor, "S2 街区初期", "裂口女案白天车灯委托。"),
            new PartCodexConfig("P-020", "接力电池", PartType.RelayBattery, "S2 街区初期", "画皮完成后的白天工具维修。"),
        };

        public static IReadOnlyList<PartCodexConfig> All() => _parts;

        public static PartCodexConfig Get(PartType type)
        {
            foreach (PartCodexConfig p in _parts)
            {
                if (p.Type == type)
                {
                    return p;
                }
            }

            return null;
        }
    }
}
