using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>清单分类（P6-008）。</summary>
    public enum ChecklistCategory : byte
    {
        CaseList = 0,    // 片单
        NormalEvent = 1, // 普通事件
        Trial = 2,       // T-G 试机
        CaseStory = 3,   // 十五案说明
    }

    /// <summary>一条内容清单条目（P6-008）：分类 + 说明 + 审校标记。</summary>
    public sealed class ChecklistItem
    {
        public string Id;
        public ChecklistCategory Category;
        public string Description;
        public bool Reviewed;

        public ChecklistItem(string id, ChecklistCategory category, string description, bool reviewed = false)
        {
            Id = id;
            Category = category;
            Description = description;
            Reviewed = reviewed;
        }
    }

    /// <summary>首批内容清单（P6-008）：片单 + 普通事件 + T-G 试机 + 十五案说明。</summary>
    public static class ContentChecklist
    {
        private static readonly IReadOnlyList<ChecklistItem> _items = new[]
        {
            // 片单（首个可玩切片）
            new ChecklistItem("CL-SLICE-TUTORIAL", ChecklistCategory.CaseList, "S0 三段工作台教学", true),
            new ChecklistItem("CL-SLICE-REDSHOE", ChecklistCategory.CaseList, "红舞鞋第一关", true),
            new ChecklistItem("CL-SLICE-ROLLERDOOR", ChecklistCategory.CaseList, "卡住的卷帘门", true),

            // 普通事件（首批）
            new ChecklistItem("CL-EV-N01", ChecklistCategory.NormalEvent, "卡住的卷帘门", true),
            new ChecklistItem("CL-EV-N02", ChecklistCategory.NormalEvent, "旧台灯", true),
            new ChecklistItem("CL-EV-N03", ChecklistCategory.NormalEvent, "隔板校准簧片", true),
            new ChecklistItem("CL-EV-D01", ChecklistCategory.NormalEvent, "夜班送还", true),
            new ChecklistItem("CL-EV-D02", ChecklistCategory.NormalEvent, "邻里找物", true),

            // T-G 试机（四条改装支线）
            new ChecklistItem("CL-T-G01", ChecklistCategory.Trial, "贯通撞锤试机", true),
            new ChecklistItem("CL-T-G02", ChecklistCategory.Trial, "横推撞锤试机", true),
            new ChecklistItem("CL-T-G03", ChecklistCategory.Trial, "轴向线圈试机", true),
            new ChecklistItem("CL-T-G04", ChecklistCategory.Trial, "定时线圈试机", true),

            // 十五案说明（仅代表，逐案说明以怪谈库为准）
            new ChecklistItem("CL-CASE-L01", ChecklistCategory.CaseStory, "L-01 红舞鞋", true),
            new ChecklistItem("CL-CASE-L02", ChecklistCategory.CaseStory, "L-02 画皮", true),
            new ChecklistItem("CL-CASE-L03", ChecklistCategory.CaseStory, "L-03 猴爪", true),
            new ChecklistItem("CL-CASE-L04", ChecklistCategory.CaseStory, "L-04 画壁", true),
            new ChecklistItem("CL-CASE-L05", ChecklistCategory.CaseStory, "L-05 聂小倩", true),
            new ChecklistItem("CL-CASE-L06", ChecklistCategory.CaseStory, "L-06 穿墙术", true),
            new ChecklistItem("CL-CASE-L07", ChecklistCategory.CaseStory, "L-07 风月宝鉴", true),
            new ChecklistItem("CL-CASE-L08", ChecklistCategory.CaseStory, "L-08 枕中梦境", true),
            new ChecklistItem("CL-CASE-L09", ChecklistCategory.CaseStory, "L-09 魔法师学徒的扫帚", true),
            new ChecklistItem("CL-CASE-L10", ChecklistCategory.CaseStory, "L-10 道林格雷的画像", true),
            new ChecklistItem("CL-CASE-L11", ChecklistCategory.CaseStory, "L-11 哈梅林吹笛人", true),
            new ChecklistItem("CL-CASE-L12", ChecklistCategory.CaseStory, "L-12 独立的影子", true),
            new ChecklistItem("CL-CASE-L13", ChecklistCategory.CaseStory, "L-13 裂口女", true),
            new ChecklistItem("CL-CASE-L14", ChecklistCategory.CaseStory, "L-14 花子", true),
            new ChecklistItem("CL-CASE-L15", ChecklistCategory.CaseStory, "L-15 如月车站", true),
        };

        public static IReadOnlyList<ChecklistItem> All() => _items;

        public static ChecklistItem Get(string id)
        {
            foreach (ChecklistItem item in _items)
            {
                if (item.Id == id)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>标记某清单条目已审校。</summary>
        public static void MarkReviewed(string id)
        {
            ChecklistItem item = Get(id);
            if (item != null)
            {
                item.Reviewed = true;
            }
        }
    }
}
