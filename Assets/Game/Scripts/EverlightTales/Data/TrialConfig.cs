using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>
    /// 试机配置（P4-012，D-062）：借用样机（锁定形态）+ 特殊目标 + 完成后的图样。
    /// 试机成功不直接解锁永久形态、不直接发钱，只推进对应改装任务至可领奖。
    /// 纯配置（Data 层）。
    /// </summary>
    public sealed class TrialConfig
    {
        public string Id;
        public string Name;
        public string BorrowedFormId;   // 借用形态（试机内锁定；不永久解锁）
        public PartType HostPart;       // 宿主零件
        public string BlueprintId;      // 完成试机 → 任务领奖图样（= 形态图样）
        public LevelConfig Level;       // 关卡配置（1 轮 3 拍 + 目标分）
        public string SpecialGoal;      // 特殊目标描述（判定由调用方/关卡实例给出）

        public TrialConfig(string id, string name, string borrowedFormId, PartType hostPart,
            string blueprintId, LevelConfig level, string specialGoal)
        {
            Id = id;
            Name = name;
            BorrowedFormId = borrowedFormId ?? "";
            HostPart = hostPart;
            BlueprintId = blueprintId ?? "";
            Level = level;
            SpecialGoal = specialGoal ?? "";
        }
    }

    /// <summary>四关试机目录（P4-012，D-062）：T-G01~T-G04 绑定四条改装支线图样。</summary>
    public static class TrialCatalog
    {
        private static LevelConfig TrialLevel()
        {
            return new LevelConfig(new[] { new RoundConfig(3, 30) });
        }

        public static readonly IReadOnlyList<TrialConfig> All = new[]
        {
            new TrialConfig("T-G01", "贯通撞锤试机", "P-001-F01", PartType.InertiaHammer,
                "T-G01", TrialLevel(), "输出侧推移到位"),
            new TrialConfig("T-G02", "横推撞锤试机", "P-001-F02", PartType.InertiaHammer,
                "T-G02", TrialLevel(), "同次双路推动"),
            new TrialConfig("T-G03", "轴向线圈试机", "P-003-F01", PartType.BlastCoil,
                "T-G03", TrialLevel(), "射线越过空格命中端点"),
            new TrialConfig("T-G04", "定时线圈试机", "P-003-F02", PartType.BlastCoil,
                "T-G04", TrialLevel(), "待爆后搬位并在下一拍命中端点"),
        };

        public static TrialConfig Get(string id)
        {
            foreach (TrialConfig t in All)
            {
                if (t.Id == id)
                {
                    return t;
                }
            }

            return null;
        }
    }
}
