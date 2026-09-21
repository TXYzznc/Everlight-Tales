using System.Collections.Generic;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;

namespace Everlight.Tales.UI
{
    /// <summary>图鉴条目状态（P4-007，D-081）：已拥有 / 已知未拥有 / 完全未知，只正向推进。</summary>
    public enum CodexState
    {
        Owned = 0,
        Known = 1,
        Unknown = 2,
    }

    /// <summary>图鉴条目类别（P4-007）：通用零件 P / 怪谈形态 M。</summary>
    public enum CodexCategory
    {
        Part = 0,
        Form = 1,
    }

    /// <summary>图鉴条目视图模型（P4-007）：编号 + 名称 + 类别 + 宿主零件 + 状态 + 来源提示 + 详情。</summary>
    public sealed class CodexEntry
    {
        public string Id;
        public string Name;          // 真实名称；Unknown 状态显示层应显示「？？？」（见 DisplayName）
        public CodexCategory Category;
        public PartType Part;        // P = 零件本身；M = 宿主零件
        public CodexState State;
        public string SourceHint;    // 来源方向（Unknown 不显示）
        public string Stage;         // P 专属：首次投放阶段
        public string Description;   // M 专属：工作方式一句话

        public CodexEntry(string id, string name, CodexCategory category, PartType part,
            CodexState state, string sourceHint, string stage = "", string description = "")
        {
            Id = id;
            Name = name;
            Category = category;
            Part = part;
            State = state;
            SourceHint = sourceHint ?? "";
            Stage = stage ?? "";
            Description = description ?? "";
        }
    }

    /// <summary>
    /// 零件图鉴布局（P4-007，D-081）：P（通用零件）与 M（怪谈形态）三态只读查阅 + 收集进度。
    /// 纯逻辑落 UI（引全层，触达 Meta 的 WorldState/FormService 与 Data 的 PartCodexCatalog/FormCatalog）。
    /// 图鉴只读：不改动任何世界状态，状态由永久解锁记录（OwnedParts/UnlockedForms/Blueprints/KnownParts）推导。
    /// 两处共用同一数据源：M 条目直接引用 FormCatalog，P 条目引用 PartCodexCatalog（战斗数值另读 PartCatalog）。
    /// </summary>
    public static class CodexLayout
    {
        /// <summary>图鉴总条数：20 通用零件 + 15 怪谈形态。</summary>
        public const int TotalCount = 35;

        public const int PartCount = 20;
        public const int FormCount = 15;

        /// <summary>通用零件页：20 条，按 P-ID 升序。</summary>
        public static IReadOnlyList<CodexEntry> Parts(WorldState world)
        {
            var result = new List<CodexEntry>();
            foreach (PartCodexConfig p in PartCodexCatalog.All())
            {
                result.Add(new CodexEntry(p.Id, p.Name, CodexCategory.Part, p.Type,
                    ClassifyPart(world, p.Type), p.SourceHint, stage: p.Stage));
            }

            return result;
        }

        /// <summary>怪谈形态页：15 条，按 M-ID 升序（FormCatalog 内 M 段已按 ID 序）。</summary>
        public static IReadOnlyList<CodexEntry> Forms(WorldState world)
        {
            var result = new List<CodexEntry>();
            foreach (FormConfig f in FormCatalog.All())
            {
                if (f.Kind != FormKind.M)
                {
                    continue;
                }

                result.Add(new CodexEntry(f.Id, f.Name, CodexCategory.Form, f.HostPart,
                    ClassifyForm(world, f), f.SourceHint, description: f.Description));
            }

            return result;
        }

        // ---- 三态分类 ----

        public static CodexState ClassifyPart(WorldState world, PartType part)
        {
            if (world == null || part == PartType.None)
            {
                return CodexState.Unknown;
            }

            if (world.OwnedParts.Contains(part))
            {
                return CodexState.Owned;
            }

            if (world.KnownParts.Contains(part))
            {
                return CodexState.Known;
            }

            return CodexState.Unknown;
        }

        public static CodexState ClassifyForm(WorldState world, FormConfig form)
        {
            if (world == null || form == null)
            {
                return CodexState.Unknown;
            }

            if (FormService.IsUnlocked(world, form.Id))
            {
                return CodexState.Owned;
            }

            if (FormService.HasBlueprint(world, form.BlueprintId))
            {
                return CodexState.Known;
            }

            return CodexState.Unknown;
        }

        // ---- 收集进度 ----

        public static int OwnedPartCount(WorldState world)
        {
            if (world == null)
            {
                return 0;
            }

            int n = 0;
            foreach (PartCodexConfig p in PartCodexCatalog.All())
            {
                if (world.OwnedParts.Contains(p.Type))
                {
                    n++;
                }
            }

            return n;
        }

        public static int OwnedFormCount(WorldState world)
        {
            if (world == null)
            {
                return 0;
            }

            int n = 0;
            foreach (FormConfig f in FormCatalog.All())
            {
                if (f.Kind == FormKind.M && FormService.IsUnlocked(world, f.Id))
                {
                    n++;
                }
            }

            return n;
        }

        public static int OwnedCount(WorldState world)
        {
            return OwnedPartCount(world) + OwnedFormCount(world);
        }

        /// <summary>网格显示名：完全未知显示「？？？」。</summary>
        public static string DisplayName(CodexEntry entry)
        {
            if (entry == null)
            {
                return "";
            }

            return entry.State == CodexState.Unknown ? "？？？" : entry.Name;
        }
    }
}
