using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>关卡固定元素行ID关系，不能把类型参数或运行时ID混入实例绑定。</summary>
    public sealed class FixedElementRelations
    {
        public IReadOnlyList<int> LinkedElementIds { get; }
        public int ProtectedElementId { get; }
        public int GrowthRootElementId { get; }
        public int InitialClampedElementId { get; }
        public bool StabilizesSettling { get; }
        public bool HasBindings => LinkedElementIds.Count != 0 || ProtectedElementId != 0
            || GrowthRootElementId != 0 || InitialClampedElementId != 0;

        public FixedElementRelations(ObstacleType type, bool controlNode, string text, string owner, bool repairNode = false)
        {
            ExtraParamSet values = ExtraParamSet.Parse(text, owner);
            var schema = new Dictionary<string, ExtraParamKind>();
            if (controlNode || type == ObstacleType.LinkedGate || type == ObstacleType.PressurePillar)
                schema.Add("LinkedElementIds", ExtraParamKind.Text);
            if (type == ObstacleType.PressurePillar) schema.Add("ProtectedElementId", ExtraParamKind.Int);
            if (type == ObstacleType.ProliferatingSeal) schema.Add("GrowthRootElementId", ExtraParamKind.Int);
            if (type == ObstacleType.ClampingSeat) schema.Add("InitialClampedElementId", ExtraParamKind.Int);
            if (repairNode) schema.Add("StabilizesSettling", ExtraParamKind.Bool);
            values.Validate(owner, schema);
            var linked = new List<int>(); var seen = new HashSet<int>();
            string list = values.GetText("LinkedElementIds", "0");
            if (list != "0")
                foreach (string item in list.Split(','))
                {
                    if (!int.TryParse(item.Trim(), out int id) || id <= 0 || !seen.Add(id))
                        throw new ArgumentException(owner + " 的LinkedElementIds含非法或重复行ID。");
                    linked.Add(id);
                }
            LinkedElementIds = linked.AsReadOnly();
            ProtectedElementId = values.GetInt("ProtectedElementId", 0);
            GrowthRootElementId = values.GetInt("GrowthRootElementId", 0);
            InitialClampedElementId = values.GetInt("InitialClampedElementId", 0);
            StabilizesSettling = values.GetBool("StabilizesSettling", false);
            if (ProtectedElementId < 0 || GrowthRootElementId < 0 || InitialClampedElementId < 0)
                throw new ArgumentException(owner + " 的实例行ID不能为负数。");
        }
    }
}
