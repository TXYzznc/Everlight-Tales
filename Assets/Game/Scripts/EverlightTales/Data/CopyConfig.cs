using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>文案分类（P6-006）。</summary>
    public enum CopyCategory : byte
    {
        Dialogue = 0, // 台词
        Hint = 1,     // 提示
        Failure = 2,  // 失败文案
        Ui = 3,       // UI 文案
    }

    /// <summary>一条统一文案（P6-006）：分类 + 文本 + 校准标记。</summary>
    public sealed class CopyEntry
    {
        public string Id;
        public CopyCategory Category;
        public string Text;
        public bool Calibrated;

        public CopyEntry(string id, CopyCategory category, string text, bool calibrated = false)
        {
            Id = id;
            Category = category;
            Text = text;
            Calibrated = calibrated;
        }
    }

    /// <summary>首批统一文案目录（P6-006）：台词/提示/失败文案/UI 文案。</summary>
    public static class CopyCatalog
    {
        private static readonly IReadOnlyList<CopyEntry> _entries = new[]
        {
            new CopyEntry("DLG-OPEN", CopyCategory.Dialogue, "你回到槐江市，代管长明修理铺。", true),
            new CopyEntry("DLG-MASTER", CopyCategory.Dialogue, "师父周衡：店里就交给你了，先跟我学怎么看盘面。", true),
            new CopyEntry("DLG-SHEN", CopyCategory.Dialogue, "沈遥：那台节拍器……它停不下来，帮帮我。", true),
            new CopyEntry("HINT-ROTATE", CopyCategory.Hint, "旋转内盘，让撞锤沿定势方向撞向标记或固定墙。", true),
            new CopyEntry("HINT-ENERGY", CopyCategory.Hint, "把连锁接到棘轮，让它产出公共维修能量。", true),
            new CopyEntry("HINT-BLAST", CopyCategory.Hint, "用爆破线圈炸开脆裂隔板，让目标标记通过校正格。", true),
            new CopyEntry("FAIL-NOT-MET", CopyCategory.Failure, "未达标", true),
            new CopyEntry("FAIL-RETREAT", CopyCategory.Failure, "已撤退，本次尝试不记失败。", true),
            new CopyEntry("UI-TITLE-MAP", CopyCategory.Ui, "槐江市地图", true),
            new CopyEntry("UI-TITLE-CODEX", CopyCategory.Ui, "图鉴", true),
            new CopyEntry("UI-TITLE-WORKBENCH", CopyCategory.Ui, "加工台", true),
            new CopyEntry("UI-UNKNOWN", CopyCategory.Ui, "？？？", true),
        };

        public static IReadOnlyList<CopyEntry> All() => _entries;

        public static CopyEntry Get(string id)
        {
            foreach (CopyEntry e in _entries)
            {
                if (e.Id == id)
                {
                    return e;
                }
            }

            return null;
        }

        /// <summary>校准某条文案。</summary>
        public static void MarkCalibrated(string id)
        {
            CopyEntry e = Get(id);
            if (e != null)
            {
                e.Calibrated = true;
            }
        }
    }
}
