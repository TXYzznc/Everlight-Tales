using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>任务状态（P3-001 对象分层；完整任务系统三页在 b21）。</summary>
    public enum TaskStateKind : byte
    {
        /// <summary>已接受：任务写入任务页，调查/处理进行中。</summary>
        Accepted = 0,

        /// <summary>已完成：全部步骤完成，等待任务页领取。</summary>
        Completed = 1,

        /// <summary>已领奖：任务页领取过一次奖励。</summary>
        Rewarded = 2,
    }

    /// <summary>任务模板（P3-001）：持续推进的目标与步骤的外层记录定义。</summary>
    public sealed class TaskConfig
    {
        public string Id;
        public string Name;
        public int RewardFee;
        public IReadOnlyList<string> Blueprints;

        public TaskConfig(string id, string name, int rewardFee, IReadOnlyList<string> blueprints = null)
        {
            Id = id;
            Name = name;
            RewardFee = rewardFee;
            Blueprints = blueprints ?? System.Array.Empty<string>();
        }
    }
}
