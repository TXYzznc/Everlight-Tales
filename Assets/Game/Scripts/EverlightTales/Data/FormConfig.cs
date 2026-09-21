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

    /// <summary>首批形态目录（P4-003，D-084）：F 类四形态 + M-012 借力改道夹。</summary>
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

            new FormConfig("M-012", "借力改道夹（红舞鞋形态）", FormKind.M, PartType.ReversalGear,
                "为指定对象提供合法相邻改道路线。",
                unlockFee: 240, blueprintId: "L-01", sourceHint: "红舞鞋 · 力从另一头出来",
                materials: new FormMaterialCost[]
                {
                    new FormMaterialCost("MT-002", 2),
                    new FormMaterialCost("MT-004", 2),
                    new FormMaterialCost("MT-005", 1),
                    new FormMaterialCost("MT-006", 1, "红舞鞋"),
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
