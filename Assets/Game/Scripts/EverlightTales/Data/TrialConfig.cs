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
        public LevelConfig Level;       // 关卡配置（轮次与目标分来自 Meta/TrialTable）
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
        private static IReadOnlyList<TrialConfig> _all = System.Array.Empty<TrialConfig>();

        public static IReadOnlyList<TrialConfig> All => _all;

        public static void Install(IEnumerable<TrialConfig> trials)
        {
            var list = new List<TrialConfig>();
            var ids = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
            foreach (TrialConfig trial in trials)
            {
                if (trial == null || string.IsNullOrEmpty(trial.Id) || !ids.Add(trial.Id))
                {
                    throw new System.InvalidOperationException("试机配置存在空条目或重复 ID。");
                }
                list.Add(trial);
            }
            _all = list.AsReadOnly();
        }

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
