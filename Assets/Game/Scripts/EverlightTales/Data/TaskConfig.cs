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

    /// <summary>任务模板（P3-001/P3-010）：持续推进的目标与步骤的外层记录定义。</summary>
    public sealed class TaskConfig
    {
        public string Id;
        public string Name;
        public int RewardFee;
        public IReadOnlyList<string> Blueprints;
        public bool IsMain;
        public string Client;
        public string Description;
        public int TotalSteps;

        // 投放门槛（P4-013）：前置案件已解决（空=无）+ 成功普通维修/处置最低次数。
        public string RequiredCaseDone;
        public int RequiredSuccessCount;

        // 目标地点（P3-011 定位/跟踪）：任务推进要去的据点，空=修理铺 home。
        public string PlaceId;

        public TaskConfig(string id, string name, int rewardFee, IReadOnlyList<string> blueprints = null, bool isMain = false, string client = "", string description = "", int totalSteps = 1,
            string requiredCaseDone = "", int requiredSuccessCount = 0, string placeId = "")
        {
            Id = id;
            Name = name;
            RewardFee = rewardFee;
            Blueprints = blueprints ?? System.Array.Empty<string>();
            IsMain = isMain;
            Client = client;
            Description = description;
            TotalSteps = totalSteps;
            RequiredCaseDone = requiredCaseDone ?? "";
            RequiredSuccessCount = requiredSuccessCount;
            PlaceId = placeId ?? "";
        }
    }
}
