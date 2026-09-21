using System.Collections.Generic;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;

namespace Everlight.Tales.UI
{
    /// <summary>形态卡四状态（P4-003，D-079）。</summary>
    public enum FormCardState
    {
        /// <summary>已解锁·当前使用。</summary>
        Current = 0,

        /// <summary>已解锁·非当前使用。</summary>
        Unlocked = 1,

        /// <summary>有图样·未解锁（可加工）。</summary>
        Craftable = 2,

        /// <summary>未取得图样。</summary>
        Unknown = 3,
    }

    /// <summary>材料持有视图（P4-003）：配方需求 + 背包持有。</summary>
    public sealed class MaterialHolding
    {
        public string MaterialId;
        public string MaterialName;
        public string SourceCase;
        public int Required;
        public int Held;

        public bool Enough => Held >= Required;
    }

    /// <summary>形态卡视图模型（P4-003）：基础形态 + 各形态卡。</summary>
    public sealed class FormCardEntry
    {
        public string Id;          // 空 = 基础形态
        public string Name;
        public FormKind Kind;
        public PartType HostPart;
        public FormCardState State;
        public string Description;
        public int UnlockFee;
        public bool RequiresBlueprint;
        public bool HasBlueprint;
        public string SourceHint;
        public IReadOnlyList<MaterialHolding> Materials;
        public bool IsBase;
    }

    /// <summary>
    /// 加工台布局（P4-003 纯逻辑，D-079 三段界面）：
    /// 上方 = 已拥有零件种类横向列表（有形态的宿主），中部 = 选中种类的形态卡（基础 + 全部形态），
    /// 下方 = 选中形态详情（工作方式/解锁条件/材料持有量）。四状态分类与材料持有量在此统一计算。
    /// 查看与切换不消耗资源、不耗时；仅解锁需要支付维修费与材料。
    /// </summary>
    public static class WorkbenchLayout
    {
        /// <summary>已拥有且存在形态的宿主零件（上方横向列表）。</summary>
        public static IReadOnlyList<PartType> OwnedHostParts(WorldState world)
        {
            var result = new List<PartType>();
            if (world == null)
            {
                return result;
            }

            foreach (PartType host in FormCatalog.HostParts())
            {
                if (world.OwnedParts.Contains(host))
                {
                    result.Add(host);
                }
            }

            return result;
        }

        /// <summary>某宿主的形态卡列表（中部横排）：基础形态在前，其余按目录顺序。</summary>
        public static IReadOnlyList<FormCardEntry> FormCards(WorldState world, PartType hostPart)
        {
            var result = new List<FormCardEntry>();
            string current = FormService.GetCurrent(world, hostPart);

            result.Add(new FormCardEntry
            {
                Id = FormService.BaseFormId,
                Name = "基础形态",
                Kind = FormKind.F,
                HostPart = hostPart,
                State = current == FormService.BaseFormId ? FormCardState.Current : FormCardState.Unlocked,
                Description = "该零件的默认工作方式。",
                IsBase = true,
                Materials = System.Array.Empty<MaterialHolding>(),
            });

            foreach (FormConfig form in FormCatalog.OfHost(hostPart))
            {
                result.Add(BuildCard(world, form));
            }

            return result;
        }

        /// <summary>形态卡四状态分类（D-079）：当前使用 / 已解锁 / 可加工（有图样或无需图样）/ 未取得图样。</summary>
        public static FormCardState Classify(WorldState world, FormConfig form)
        {
            if (form == null)
            {
                return FormCardState.Unknown;
            }

            if (FormService.IsUnlocked(world, form.Id))
            {
                return FormService.GetCurrent(world, form.HostPart) == form.Id
                    ? FormCardState.Current
                    : FormCardState.Unlocked;
            }

            if (!form.RequiresBlueprint || FormService.HasBlueprint(world, form.BlueprintId))
            {
                return FormCardState.Craftable;
            }

            return FormCardState.Unknown;
        }

        private static FormCardEntry BuildCard(WorldState world, FormConfig form)
        {
            var entry = new FormCardEntry
            {
                Id = form.Id,
                Name = form.Name,
                Kind = form.Kind,
                HostPart = form.HostPart,
                Description = form.Description,
                UnlockFee = form.UnlockFee,
                RequiresBlueprint = form.RequiresBlueprint,
                HasBlueprint = FormService.HasBlueprint(world, form.BlueprintId),
                SourceHint = form.SourceHint,
                IsBase = false,
            };

            entry.State = Classify(world, form);

            var holdings = new List<MaterialHolding>();
            foreach (FormMaterialCost m in form.Materials)
            {
                MaterialConfig mc = MaterialCatalog.Get(m.MaterialId);
                holdings.Add(new MaterialHolding
                {
                    MaterialId = m.MaterialId,
                    MaterialName = mc == null ? m.MaterialId : mc.Name,
                    SourceCase = m.SourceCase,
                    Required = m.Count,
                    Held = world == null ? 0 : world.Materials.GetCount(m.MaterialId, m.SourceCase),
                });
            }

            entry.Materials = holdings;
            return entry;
        }
    }
}
