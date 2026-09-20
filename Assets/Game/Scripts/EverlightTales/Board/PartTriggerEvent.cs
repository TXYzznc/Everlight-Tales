namespace Everlight.Tales.Board
{
    /// <summary>触发语义标签（Board 侧）。</summary>
    public static class TriggerKinds
    {
        /// <summary>碰撞触发：来撞实体指向被撞零件，带入射方向。</summary>
        public const string Collision = "collision";

        /// <summary>冲击触发：线圈爆破等其他对象效果，无实体入射方向。</summary>
        public const string Shock = "shock";
    }

    /// <summary>
    /// 零件触发事件：一次碰撞或冲击触发目标零件的能力。
    /// 碰撞（Collision）带入射方向（来撞实体→被撞零件），冲击（Shock）无方向（不触发撞锤推移）。
    /// 应用时由 <see cref="PartAbility"/> 结算触发分与自身效果，可能改变盘面并入队新触发。
    /// </summary>
    public sealed class PartTriggerEvent : ISettlementEvent
    {
        /// <summary>触发语义标签（collision／shock）。</summary>
        public string Kind { get; }

        /// <summary>触发源实体 ID（碰撞移动方／冲击来源线圈）。</summary>
        public int SourceId { get; }

        /// <summary>被触发目标实体 ID（零件）。</summary>
        public int TargetId { get; }

        /// <summary>入射方向（仅碰撞有效；冲击用 D0 占位）。</summary>
        public HexDirection Incoming { get; }

        public string Name => Kind;

        public PartTriggerEvent(string kind, int sourceId, int targetId, HexDirection incoming = HexDirection.D0)
        {
            Kind = kind;
            SourceId = sourceId;
            TargetId = targetId;
            Incoming = incoming;
        }

        public void Apply(SettlementContext context)
        {
            PartAbility.ResolveTrigger(context, SourceId, TargetId, Kind, Incoming);
        }

        public override string ToString()
        {
            return Kind + " #" + SourceId + "->#" + TargetId;
        }
    }
}
