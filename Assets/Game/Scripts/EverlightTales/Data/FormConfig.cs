using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>形态种类（P4-004）：F 通用零件形态 / M 怪谈形态。</summary>
    public enum FormKind : byte
    {
        F = 0,
        M = 1,
    }

    /// <summary>一条加工材料需求（P4-003）：物资条目 + 数量；异常纹样（MT-006）带来源怪谈。</summary>
    public sealed class FormMaterialCost
    {
        public string MaterialId;
        public int Count;
        public string SourceCase;

        public FormMaterialCost(string materialId, int count, string sourceCase = "")
        {
            MaterialId = materialId;
            Count = count;
            SourceCase = sourceCase ?? "";
        }
    }

    /// <summary>
    /// 形态静态配置（P4-003/P4-004，D-060/D-061/D-079/D-084）：
    /// 宿主零件 + 工作方式说明 + 加工条件（维修费/图样/材料）+ 来源提示。
    /// 图样是永久条件（保留不消耗），维修费必定消耗，材料按配方一次性消耗。
    /// </summary>
    public sealed class FormConfig
    {
        public string Id;
        public string Name;
        public FormKind Kind;
        public PartType HostPart;
        public string Description;
        public int UnlockFee;
        public string BlueprintId;   // 空 = 不需要图样
        public string SourceHint;    // 未取得图样时的来源提示
        public IReadOnlyList<FormMaterialCost> Materials;
        public bool AutoSetCurrent;

        public FormConfig(string id, string name, FormKind kind, PartType hostPart, string description,
            int unlockFee, string blueprintId = "", string sourceHint = "",
            IReadOnlyList<FormMaterialCost> materials = null, bool autoSetCurrent = true)
        {
            Id = id;
            Name = name;
            Kind = kind;
            HostPart = hostPart;
            Description = description;
            UnlockFee = unlockFee;
            BlueprintId = blueprintId ?? "";
            SourceHint = sourceHint ?? "";
            Materials = materials ?? System.Array.Empty<FormMaterialCost>();
            AutoSetCurrent = autoSetCurrent;
        }

        public bool RequiresBlueprint => !string.IsNullOrEmpty(BlueprintId);
    }

    /// <summary>形态目录（P4-003/P4-006，D-084）：F 类四形态 + M 类十五形态（M-001~M-015）。</summary>
    public static class FormCatalog
    {
        private static readonly IReadOnlyList<FormConfig> _forms = new[]
        {
            new FormConfig("P-001-F01", "贯通撞锤", FormKind.F, PartType.InertiaHammer,
                "被 A 撞击后，推动输出侧 B 沿入射方向移动 1 格，A 留在原处。",
                unlockFee: 80, blueprintId: "T-G01", sourceHint: "力从另一头出来"),

            new FormConfig("P-001-F02", "横推撞锤", FormKind.F, PartType.InertiaHammer,
                "沿入射方向左右偏转 60°，分别把邻接对象向外推动 1 格；两路合计消耗 1 点能量。",
                unlockFee: 120, blueprintId: "T-G02", sourceHint: "一边输入，两边接应",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-002", 2),
                    new FormMaterialCost("MT-001", 1),
                }),

            new FormConfig("P-003-F01", "轴向线圈", FormKind.F, PartType.BlastCoil,
                "向 D0、D3 各发送最长 2 格冲击射线，各命中首个实体就停止。",
                unlockFee: 140, blueprintId: "T-G03", sourceHint: "隔着空隙传过去"),

            new FormConfig("P-003-F02", "定时线圈", FormKind.F, PartType.BlastCoil,
                "首次有效触发支付能量，下一拍开始、定势下落前按实时位置爆破。",
                unlockFee: 180, blueprintId: "T-G04", sourceHint: "把这一拍留到下一拍",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-001", 2),
                    new FormMaterialCost("MT-004", 1),
                }),

            new FormConfig("M-001", "护送扣（聂小倩形态）", FormKind.M, PartType.MagneticTractor,
                "替承可护送对象的一步牵引，把该步方向与距离交给相邻承载件。",
                unlockFee: 320, blueprintId: "L-05", sourceHint: "聂小倩",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-002", 3),
                    new FormMaterialCost("MT-001", 2),
                    new FormMaterialCost("MT-005", 2),
                }),

            new FormConfig("M-002", "贯通套筒（穿墙术形态）", FormKind.M, PartType.CalibrationProbe,
                "探针成功校准一个已开启墙口对时，给该口对增加 1 通行额度。",
                unlockFee: 280, blueprintId: "L-06", sourceHint: "穿墙术",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-001", 2),
                    new FormMaterialCost("MT-003", 2),
                    new FormMaterialCost("MT-005", 1),
                }),

            new FormConfig("M-003", "双面标记（风月宝鉴形态）", FormKind.M, PartType.LightingPrism,
                "真实识别后标记目标，使其身份在下一次遮蔽后仍显示至阶段结束。",
                unlockFee: 320, blueprintId: "L-07", sourceHint: "风月宝鉴",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-003", 3),
                    new FormMaterialCost("MT-001", 2),
                    new FormMaterialCost("MT-005", 2),
                    new FormMaterialCost("MT-006", 1, "风月宝鉴"),
                }),

            new FormConfig("M-004", "留梦夹（枕中梦境形态）", FormKind.M, PartType.StorageStomach,
                "收纳一个已接收的现实锚点标记，换层随宿主继承，到归途口释放并重新接入归途线；每作业一次。",
                unlockFee: 340, blueprintId: "L-08", sourceHint: "枕中梦境",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-004", 3),
                    new FormMaterialCost("MT-001", 3),
                    new FormMaterialCost("MT-005", 3),
                }),

            new FormConfig("M-005", "分流泵（魔法师学徒的扫帚形态）", FormKind.M, PartType.DrainImpeller,
                "额外提供两个登记出水端，来水按整数比例分配。",
                unlockFee: 260, blueprintId: "L-09", sourceHint: "魔法师学徒的扫帚",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-002", 2),
                    new FormMaterialCost("MT-001", 2),
                    new FormMaterialCost("MT-005", 1),
                }),

            new FormConfig("M-006", "代偿框（道林格雷的画像形态）", FormKind.M, PartType.BufferBladder,
                "暂存一份可转存设备负荷，下一拍转存至兼容缓冲节点。",
                unlockFee: 420, blueprintId: "L-10", sourceHint: "道林格雷的画像",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-002", 4),
                    new FormMaterialCost("MT-003", 3),
                    new FormMaterialCost("MT-005", 3),
                    new FormMaterialCost("MT-006", 1, "道林格雷的画像"),
                }),

            new FormConfig("M-007", "领奏哨（哈梅林吹笛人形态）", FormKind.M, PartType.TuningFork,
                "指定已登记合法声源，本拍首次激活时优先于后来普通声源。",
                unlockFee: 400, blueprintId: "L-11", sourceHint: "哈梅林吹笛人",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-004", 4),
                    new FormMaterialCost("MT-003", 3),
                    new FormMaterialCost("MT-005", 3),
                }),

            new FormConfig("M-008", "影随扣（独立的影子形态）", FormKind.M, PartType.InertiaHammer,
                "选两个可移动普通件建立主从关系，主件下次位移后从件按轴映射等步长位移一次。",
                unlockFee: 360, blueprintId: "L-12", sourceHint: "独立的影子",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-002", 3),
                    new FormMaterialCost("MT-001", 3),
                    new FormMaterialCost("MT-005", 3),
                }),

            new FormConfig("M-009", "诱导响片（裂口女形态）", FormKind.M, PartType.TuningFork,
                "声学激活后预备响片，使下一次追逐步改向已接通诱导点。",
                unlockFee: 260, blueprintId: "L-13", sourceHint: "裂口女",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-004", 2),
                    new FormMaterialCost("MT-002", 2),
                    new FormMaterialCost("MT-005", 1),
                }),

            new FormConfig("M-010", "回声簧（花子形态）", FormKind.M, PartType.TuningFork,
                "截留宿主下一次真实敲击信号入簧槽，指定门端后下一拍发出。",
                unlockFee: 300, blueprintId: "L-14", sourceHint: "花子",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-004", 3),
                    new FormMaterialCost("MT-003", 2),
                    new FormMaterialCost("MT-005", 2),
                }),

            new FormConfig("M-011", "返程信标（如月车站形态）", FormKind.M, PartType.CalibrationProbe,
                "完成真实凭据核验后保留该出口资格，线路断开重连不需重做。",
                unlockFee: 480, blueprintId: "L-15", sourceHint: "如月车站",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-001", 4),
                    new FormMaterialCost("MT-002", 4),
                    new FormMaterialCost("MT-005", 4),
                    new FormMaterialCost("MT-006", 1, "如月车站"),
                }),

            new FormConfig("M-012", "借力改道夹（红舞鞋形态）", FormKind.M, PartType.ReversalGear,
                "指定强制运动对象，其下一步即将进入危险格或违反安全路线时改由玩家选择合法相邻路线；每次作业一次。",
                unlockFee: 240, blueprintId: "L-01", sourceHint: "红舞鞋",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-002", 2),
                    new FormMaterialCost("MT-004", 2),
                    new FormMaterialCost("MT-005", 1),
                    new FormMaterialCost("MT-006", 1, "红舞鞋"),
                }),

            new FormConfig("M-013", "身份保真签（画皮形态）", FormKind.M, PartType.LightingPrism,
                "真实识别后盖章，后续再被遮蔽仍持续显示真实身份直到任一出口交付；每次作业一次。",
                unlockFee: 260, blueprintId: "L-02", sourceHint: "画皮",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-003", 2),
                    new FormMaterialCost("MT-001", 2),
                    new FormMaterialCost("MT-005", 1),
                    new FormMaterialCost("MT-006", 1, "画皮"),
                }),

            new FormConfig("M-014", "牵连压扣（猴爪形态）", FormKind.M, PartType.RivetPliers,
                "指定已修复牵连节点，下一次新节点形成时暂压待处理，不能免除后续修复；每次作业一次。",
                unlockFee: 300, blueprintId: "L-03", sourceHint: "猴爪",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-002", 3),
                    new FormMaterialCost("MT-004", 2),
                    new FormMaterialCost("MT-005", 2),
                }),

            new FormConfig("M-015", "接应复用哨（画壁形态）", FormKind.M, PartType.TuningFork,
                "临时门户开放后给出口保留接应标记，第二名受困者复用一次；每次作业一次。",
                unlockFee: 340, blueprintId: "L-04", sourceHint: "画壁",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-003", 3),
                    new FormMaterialCost("MT-004", 3),
                    new FormMaterialCost("MT-005", 3),
                }),
        };

        public static IReadOnlyList<FormConfig> All() => _forms;

        public static FormConfig Get(string id)
        {
            foreach (FormConfig f in _forms)
            {
                if (f.Id == id)
                {
                    return f;
                }
            }

            return null;
        }

        /// <summary>某宿主零件的全部形态（不含基础形态）。</summary>
        public static IReadOnlyList<FormConfig> OfHost(PartType hostPart)
        {
            var result = new List<FormConfig>();
            foreach (FormConfig f in _forms)
            {
                if (f.HostPart == hostPart)
                {
                    result.Add(f);
                }
            }

            return result;
        }

        /// <summary>存在形态的宿主零件集合（去重，按目录出现顺序）。</summary>
        public static IReadOnlyList<PartType> HostParts()
        {
            var result = new List<PartType>();
            foreach (FormConfig f in _forms)
            {
                if (!result.Contains(f.HostPart))
                {
                    result.Add(f.HostPart);
                }
            }

            return result;
        }
    }
}
