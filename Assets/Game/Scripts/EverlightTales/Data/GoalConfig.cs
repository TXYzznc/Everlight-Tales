using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>特殊目标判定类型（P2-004）。</summary>
    public enum GoalKind : byte
    {
        None = 0,

        /// <summary>到达：指定实体到达锚点格即达成。</summary>
        Arrive = 1,

        /// <summary>保持：指定实体在锚点格连续保持指定拍数。</summary>
        Hold = 2,

        /// <summary>顺序：指定实体按顺序到访多个锚点格。</summary>
        Order = 3,

        /// <summary>推移进格：指定实体必须由普通推移送入锚点格并锁定（机械臂／下落不计）。</summary>
        PushedInto = 4,
    }

    /// <summary>
    /// 特殊目标静态配置（P2-004）：现场锚点（锚点标签）→ 盘面节点（端点标签），
    /// 判定类型与参数。锚点标签在 BoardState 端点层登记，判定由 Board 层 GoalEvaluator 执行。
    /// </summary>
    public sealed class GoalConfig
    {
        /// <summary>目标 ID。</summary>
        public string Id { get; }

        /// <summary>判定类型。</summary>
        public GoalKind Kind { get; }

        /// <summary>被追踪的实体 ID（0 = 任意实体）。</summary>
        public int EntityId { get; }

        /// <summary>锚点标签（对应端点标签 targetId，到达/保持用）。</summary>
        public int AnchorLabel { get; }

        /// <summary>达成所需的量：Hold=保持拍数，Order=顺序节点数，Arrive 忽略。</summary>
        public int Required { get; }

        /// <summary>顺序目标的锚点标签序列（Kind==Order）。</summary>
        public IReadOnlyList<int> OrderLabels { get; }

        public GoalConfig(string id, GoalKind kind, int entityId, int anchorLabel = 0, int required = 1, IReadOnlyList<int> orderLabels = null)
        {
            Id = id;
            Kind = kind;
            EntityId = entityId;
            AnchorLabel = anchorLabel;
            Required = required;
            OrderLabels = orderLabels ?? System.Array.Empty<int>();
        }
    }
}
