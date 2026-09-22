using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>玩家进度阶段（D-014，03-玩家解锁与内容投放）：S 编号不是等级系统，不绑定现实天数。</summary>
    public enum StageLevel : byte
    {
        S0 = 0, // 开店教学
        S1 = 1, // 红鞋首案
        S2 = 2, // 街区初期
        S3 = 3, // 城市中期
        S4 = 4, // 画壁筹备与后期支线
        S5 = 5, // 后期／第五批资格期
    }

    /// <summary>投放条目种类（D-014）：零件 / 形态 / 怪谈挑战。</summary>
    public enum ReleaseKind : byte
    {
        Part = 0,
        Form = 1,
        Challenge = 2,
    }

    /// <summary>投放条目（D-014）：内容 Id + 阶段 + 首次出现 + 永久解锁条件。纯配置。</summary>
    public sealed class ReleaseEntry
    {
        public string Id;              // "P-001" / "P-001-F01" / "M-012" / "C-001"
        public ReleaseKind Kind;
        public StageLevel Stage;
        public string FirstAppearance;
        public string UnlockCondition;

        public ReleaseEntry(string id, ReleaseKind kind, StageLevel stage, string firstAppearance, string unlockCondition)
        {
            Id = id;
            Kind = kind;
            Stage = stage;
            FirstAppearance = firstAppearance ?? "";
            UnlockCondition = unlockCondition ?? "";
        }
    }

    /// <summary>
    /// 解锁投放表（P4-014，D-014）：零件/形态按 S0~S5 逐件投放，可配置可查询。
    /// 阶段越小越早投放；达到阶段不自动塞入携带池，仍按「首次出现 + 解锁条件」逐件激活。
    /// </summary>
    public static class ReleaseTable
    {
        private static readonly IReadOnlyList<ReleaseEntry> _entries = new[]
        {
            // ---- 通用零件 P（20，D-014）----
            new ReleaseEntry("P-001", ReleaseKind.Part, StageLevel.S0, "工作台第一段", "完成移动碰撞演示永久解锁"),
            new ReleaseEntry("P-002", ReleaseKind.Part, StageLevel.S0, "工作台第二段", "完成能量收益演示永久解锁"),
            new ReleaseEntry("P-003", ReleaseKind.Part, StageLevel.S0, "工作台最后一段", "完成爆破与脆裂隔板演示永久解锁"),
            new ReleaseEntry("P-004", ReleaseKind.Part, StageLevel.S1, "红鞋案前段现场提供", "红鞋首关现场演示后永久解锁"),
            new ReleaseEntry("P-014", ReleaseKind.Part, StageLevel.S1, "红鞋准备阶段周衡借出", "红鞋完成后回店试机永久解锁"),
            new ReleaseEntry("P-011", ReleaseKind.Part, StageLevel.S2, "扫帚初次处理后工作台试机", "完成白天演示/加工委托后永久解锁"),
            new ReleaseEntry("P-013", ReleaseKind.Part, StageLevel.S2, "画皮案识别环节", "完成对应怪谈并回访后永久解锁"),
            new ReleaseEntry("P-015", ReleaseKind.Part, StageLevel.S2, "裂口女案白天准备后段", "完成白天演示/加工委托后永久解锁"),
            new ReleaseEntry("P-016", ReleaseKind.Part, StageLevel.S2, "扫帚案前段现场提供", "完成对应怪谈并回访后永久解锁"),
            new ReleaseEntry("P-019", ReleaseKind.Part, StageLevel.S2, "裂口女案白天车灯委托", "完成白天演示/加工委托后永久解锁"),
            new ReleaseEntry("P-020", ReleaseKind.Part, StageLevel.S2, "画皮完成后白天工具维修", "完成白天演示/加工委托后永久解锁"),
            new ReleaseEntry("P-005", ReleaseKind.Part, StageLevel.S3, "第二章普通弹簧维修", "完成白天演示/加工委托后永久解锁"),
            new ReleaseEntry("P-007", ReleaseKind.Part, StageLevel.S3, "穿墙案完成后回收试机", "完成白天演示/加工委托后永久解锁"),
            new ReleaseEntry("P-008", ReleaseKind.Part, StageLevel.S3, "第二章仓库整理委托", "完成白天演示/加工委托后永久解锁"),
            new ReleaseEntry("P-012", ReleaseKind.Part, StageLevel.S3, "猴爪案调查准备", "完成白天演示/加工委托后永久解锁"),
            new ReleaseEntry("P-018", ReleaseKind.Part, StageLevel.S3, "穿墙案白天调查", "完成对应怪谈并回访后永久解锁"),
            new ReleaseEntry("P-006", ReleaseKind.Part, StageLevel.S4, "第三章加工台进阶委托", "完成白天演示/加工委托后永久解锁"),
            new ReleaseEntry("P-009", ReleaseKind.Part, StageLevel.S4, "风月案完成后工作台演示", "完成白天演示/加工委托后永久解锁"),
            new ReleaseEntry("P-010", ReleaseKind.Part, StageLevel.S4, "交换拨叉试机后", "完成白天演示/加工委托后永久解锁"),
            new ReleaseEntry("P-017", ReleaseKind.Part, StageLevel.S4, "画像案前段现场提供", "完成对应怪谈并回访后永久解锁"),

            // ---- F 类形态（4，D-061）----
            new ReleaseEntry("P-001-F01", ReleaseKind.Form, StageLevel.S1, "红舞鞋完成后首次回店", "完成贯通试机领图样后加工解锁"),
            new ReleaseEntry("P-001-F02", ReleaseKind.Form, StageLevel.S2, "红舞鞋后累计 1 次成功普通维修", "完成横推试机领图样后加工解锁"),
            new ReleaseEntry("P-003-F01", ReleaseKind.Form, StageLevel.S2, "红舞鞋后累计 1 次成功普通维修", "完成轴向试机领图样后加工解锁"),
            new ReleaseEntry("P-003-F02", ReleaseKind.Form, StageLevel.S2, "红舞鞋后累计 2 次成功普通维修", "完成定时试机领图样后加工解锁"),

            // ---- M 类形态（15，D-084）----
            new ReleaseEntry("M-012", ReleaseKind.Form, StageLevel.S1, "红舞鞋完成后白天回访", "L-01 回访领图样后加工解锁"),
            new ReleaseEntry("M-005", ReleaseKind.Form, StageLevel.S2, "扫帚完成后白天回访", "L-09 回访领图样后加工解锁"),
            new ReleaseEntry("M-009", ReleaseKind.Form, StageLevel.S2, "裂口女完成后白天回访", "L-13 回访领图样后加工解锁"),
            new ReleaseEntry("M-013", ReleaseKind.Form, StageLevel.S2, "画皮完成后白天回访", "L-02 回访领图样后加工解锁"),
            new ReleaseEntry("M-001", ReleaseKind.Form, StageLevel.S3, "聂小倩完成后白天回访", "L-05 回访领图样后加工解锁"),
            new ReleaseEntry("M-002", ReleaseKind.Form, StageLevel.S3, "穿墙完成后白天回访", "L-06 回访领图样后加工解锁"),
            new ReleaseEntry("M-003", ReleaseKind.Form, StageLevel.S3, "风月完成后白天回访", "L-07 回访领图样后加工解锁"),
            new ReleaseEntry("M-010", ReleaseKind.Form, StageLevel.S3, "花子完成后白天回访", "L-14 回访领图样后加工解锁"),
            new ReleaseEntry("M-014", ReleaseKind.Form, StageLevel.S3, "猴爪完成后白天回访", "L-03 回访领图样后加工解锁"),
            new ReleaseEntry("M-004", ReleaseKind.Form, StageLevel.S4, "梦境完成后白天回访", "L-08 回访领图样后加工解锁"),
            new ReleaseEntry("M-006", ReleaseKind.Form, StageLevel.S4, "画像完成后白天回访", "L-10 回访领图样后加工解锁"),
            new ReleaseEntry("M-007", ReleaseKind.Form, StageLevel.S4, "吹笛完成后白天回访", "L-11 回访领图样后加工解锁"),
            new ReleaseEntry("M-008", ReleaseKind.Form, StageLevel.S4, "影子完成后白天回访", "L-12 回访领图样后加工解锁"),
            new ReleaseEntry("M-015", ReleaseKind.Form, StageLevel.S4, "画壁完成后白天回访", "L-04 回访领图样后加工解锁"),
            new ReleaseEntry("M-011", ReleaseKind.Form, StageLevel.S5, "如月站完成后白天回访", "L-15 回访领图样后加工解锁"),
        };

        public static IReadOnlyList<ReleaseEntry> All() => _entries;

        /// <summary>按条目 Id 查阶段；未登记返回 null。</summary>
        public static StageLevel? StageOf(string id)
        {
            foreach (ReleaseEntry e in _entries)
            {
                if (e.Id == id)
                {
                    return e.Stage;
                }
            }

            return null;
        }

        /// <summary>零件阶段（经图鉴 Id 反查）。</summary>
        public static StageLevel? StageOfPart(PartType type)
        {
            PartCodexConfig codex = PartCodexCatalog.Get(type);
            return codex == null ? null : StageOf(codex.Id);
        }

        /// <summary>形态阶段。</summary>
        public static StageLevel? StageOfForm(string formId)
        {
            return StageOf(formId);
        }
    }
}
