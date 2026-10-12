using System;
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
        private static IReadOnlyList<FormConfig> _forms = System.Array.Empty<FormConfig>();

        /// <summary>安装已完成外键和配方校验的 DataTable 形态快照。</summary>
        public static void Install(IEnumerable<FormConfig> forms)
        {
            if (forms == null)
            {
                throw new ArgumentNullException(nameof(forms));
            }

            var replacement = new List<FormConfig>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (FormConfig form in forms)
            {
                if (form == null || string.IsNullOrEmpty(form.Id) || !ids.Add(form.Id) || !Enum.IsDefined(typeof(FormKind), form.Kind) || form.HostPart == PartType.None)
                {
                    throw new ArgumentException("形态配置包含空值、重复 ID 或无效枚举。", nameof(forms));
                }

                replacement.Add(form);
            }

            if (replacement.Count == 0)
            {
                throw new ArgumentException("形态配置不能为空。", nameof(forms));
            }

            _forms = replacement;
        }

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

        /// <summary>按图样 ID 反查形态（蓝图 → 形态），找不到返回 null。</summary>
        public static FormConfig FindByBlueprint(string blueprintId)
        {
            if (string.IsNullOrEmpty(blueprintId))
            {
                return null;
            }

            foreach (FormConfig f in _forms)
            {
                if (f.BlueprintId == blueprintId)
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
