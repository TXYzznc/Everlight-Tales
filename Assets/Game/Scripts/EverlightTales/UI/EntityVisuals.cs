using Everlight.Tales.Board;
using Everlight.Tales.Data;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 棋子／设施／标记的表现配色（P1-015）。按实体类别与零件类型给出可辨识颜色，
    /// 供 <see cref="HexBoardView"/> 与后续零件卡复用；表现层细节，不参与结算。
    /// </summary>
    public static class EntityVisuals
    {
        public static Color GetColor(BoardEntity entity)
        {
            switch (entity.Kind)
            {
                case EntityKind.Facility:
                    return new Color(0.22f, 0.24f, 0.28f, 1f); // 深灰：固定设施
                case EntityKind.TaskMarker:
                    return new Color(0.95f, 0.80f, 0.20f, 1f); // 黄：任务标记
                case EntityKind.RepairTarget:
                    return new Color(0.62f, 0.40f, 0.92f, 1f); // 紫：维修对象
                default:
                    return GetPartColor(entity.PartType);
            }
        }

        public static Color GetPartColor(PartType partType)
        {
            switch (partType)
            {
                case PartType.InertiaHammer:
                    return new Color(0.95f, 0.55f, 0.20f, 1f); // 橙：撞锤
                case PartType.MeteringRatchet:
                    return new Color(0.20f, 0.72f, 0.68f, 1f); // 青：棘轮
                case PartType.BlastCoil:
                    return new Color(0.90f, 0.28f, 0.32f, 1f); // 红：线圈
                default:
                    return new Color(0.62f, 0.64f, 0.68f, 1f); // 灰：普通件
            }
        }
    }
}
