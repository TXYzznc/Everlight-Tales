using System;
using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>先完整验证关卡行图，再将稳定行ID解析为运行实体ID。验证不消耗随机数。</summary>
    public static class FixedElementBinding
    {
        public static void Validate(LevelBoardConfig level, IReadOnlyList<ObstacleConfig> obstacleConfigs = null)
        {
            if (level.BoardRadius < 1 || level.BoardRadius > 32) throw new ArgumentException(level.LevelName + " 半径不在可恢复范围内。");
            var shape = new HexBoardShape(level.BoardRadius);
            var rows = new Dictionary<int, FixedElementConfig>();
            var fixtures = new HashSet<HexCoord>(); var occupied = new HashSet<HexCoord>();
            var labels = new HashSet<int>(); var clampTargets = new HashSet<int>();
            foreach (FixedElementConfig element in level.FixedElements)
            {
                if (element == null || !shape.IsNormal(element.Position)) throw new ArgumentException(level.LevelName + " 固定元素坐标无效。");
                if (!Enum.IsDefined(typeof(FixedElementKind), element.Kind)
                    || !Enum.IsDefined(typeof(ObstacleType), element.ObstacleType)
                    || (element.Kind == FixedElementKind.Obstacle) != (element.ObstacleType != ObstacleType.None))
                    Fail(element, "固定元素种类与障碍类型不匹配");
                if (element.ElementId != 0 && !rows.TryAdd(element.ElementId, element)) Fail(element, "稳定行ID重复");
                if (element.Kind == FixedElementKind.EndpointLabel)
                {
                    if (element.Label <= 0 || !labels.Add(element.Label)) Fail(element, "端点标签无效或重复");
                    continue;
                }
                bool fixture = IsFixture(element);
                if (!(fixture ? fixtures : occupied).Add(element.Position)) Fail(element, "同层固定元素重叠");
                if (element.Kind == FixedElementKind.Obstacle) Config(element, obstacleConfigs);
            }
            foreach (KeyPieceConfig key in level.KeyPieces)
                if (!shape.IsNormal(key.Position) || !occupied.Add(key.Position))
                    throw new ArgumentException(level.LevelName + " 关键件坐标无效或重叠：" + key.Position);
            foreach (FixedElementConfig element in level.FixedElements)
            {
                FixedElementRelations relation = element.Relations;
                if (relation.StabilizesSettling && (element.RepairTarget == null
                    || !element.RepairTarget.Accepts(PartType.RivetPliers))) Fail(element, "沉降稳定节点必须接受铆合钳维修");
                ObstacleConfig config = element.Kind == FixedElementKind.Obstacle ? Config(element, obstacleConfigs) : null;
                bool repairRequired = element.ObstacleType == ObstacleType.PressurePillar && config.Repairable
                    || element.ObstacleType == ObstacleType.ProliferatingSeal
                        && (relation.GrowthRootElementId == 0 || relation.GrowthRootElementId == element.ElementId);
                if (repairRequired && (element.RepairTarget == null || !element.RepairTarget.Accepts(PartType.RivetPliers)
                    || element.RepairTarget.RequiredProgress <= 0 || element.RepairTarget.EnergyCost < 0 || element.RepairTarget.RepairScore < 0))
                    Fail(element, "可维修支柱/封条根须在现有维修列声明适配件、费用和效果分");
                if (element.ObstacleType == ObstacleType.LinkedGate && occupied.Contains(element.Position) && !config.InitialOpen)
                    Fail(element, "关闭闸门初始格已有占用者");
                foreach (int id in relation.LinkedElementIds)
                {
                    FixedElementConfig target = Target(element, id, rows);
                    bool valid = element.ObstacleType == ObstacleType.PressurePillar
                        ? target.ObstacleType == ObstacleType.BrittlePartition
                        : target.ObstacleType == ObstacleType.LinkedGate || target.ObstacleType == ObstacleType.ClampingSeat
                            || target.ObstacleType == ObstacleType.OneWayShutter;
                    if (id == element.ElementId || !valid) Fail(element, "联动/附属目标类型错误：" + id);
                }
                if (element.ObstacleType == ObstacleType.PressurePillar
                    && relation.LinkedElementIds.Count != config.AttachedPartitions) Fail(element, "附属隔板数量与类型配置不符");
                if (relation.ProtectedElementId != 0)
                {
                    FixedElementConfig target = Target(element, relation.ProtectedElementId, rows);
                    if (!config.ProtectedObject || target.Kind != FixedElementKind.TaskMarker && target.Kind != FixedElementKind.MovableMarker
                        && target.Kind != FixedElementKind.RepairTarget) Fail(element, "保护绑定目标不受支持");
                }
                if (relation.GrowthRootElementId != 0)
                {
                    FixedElementConfig target = Target(element, relation.GrowthRootElementId, rows);
                    if (target.ObstacleType != ObstacleType.ProliferatingSeal
                        || target.Relations.GrowthRootElementId != 0 && target.Relations.GrowthRootElementId != target.ElementId)
                        Fail(element, "生长根指向非根封条");
                }
                if (relation.InitialClampedElementId != 0)
                {
                    FixedElementConfig target = Target(element, relation.InitialClampedElementId, rows);
                    if (target.Kind != FixedElementKind.MovableMarker || !config.AllowsTag("task")
                        || target.Position != element.Position || !clampTargets.Add(target.ElementId))
                        Fail(element, "初始夹持目标类型、标签、位置或唯一性错误");
                }
            }
        }

        internal static void Resolve(BoardState board, LevelBoardConfig level, IReadOnlyDictionary<int, int> ids)
        {
            foreach (FixedElementConfig element in level.FixedElements)
            {
                if (element.ElementId == 0 || element.Kind == FixedElementKind.EndpointLabel) continue;
                board.TryGetEntity(ids[element.ElementId], out BoardEntity entity);
                foreach (int id in element.Relations.LinkedElementIds) entity.LinkedElementIds.Add(ids[id]);
                if (element.Kind == FixedElementKind.Facility && entity.LinkedElementIds.Count != 0) entity.SetSoundEndpoint("control");
                if (element.Relations.ProtectedElementId != 0) entity.ProtectedElementId = ids[element.Relations.ProtectedElementId];
                if (element.Relations.GrowthRootElementId != 0)
                {
                    entity.GrowthRootEntityId = ids[element.Relations.GrowthRootElementId];
                    if (entity.GrowthRootEntityId != entity.Id) entity.ConfigureSealSegment(entity.GrowthRootEntityId);
                }
                if (element.Relations.InitialClampedElementId != 0)
                {
                    board.TryGetEntity(ids[element.Relations.InitialClampedElementId], out BoardEntity target);
                    if (entity.ClampedEntityId != target.Id && !ObstacleService.TryClamp(entity, target))
                        Fail(element, "初始夹持装配失败");
                }
            }
        }

        private static bool IsFixture(FixedElementConfig element) => element.Kind == FixedElementKind.Obstacle
            && (element.ObstacleType == ObstacleType.LinkedGate || element.ObstacleType == ObstacleType.ClampingSeat
                || element.ObstacleType == ObstacleType.TurningRail || element.ObstacleType == ObstacleType.OneWayShutter);
        private static ObstacleConfig Config(FixedElementConfig element, IReadOnlyList<ObstacleConfig> configs)
        {
            if (configs != null) foreach (ObstacleConfig config in configs) if (config.Type == element.ObstacleType) return config;
            if (configs == null && ObstacleCatalog.Get(element.ObstacleType) is ObstacleConfig installed) return installed;
            Fail(element, "障碍类型未配置"); return null;
        }
        private static FixedElementConfig Target(FixedElementConfig source, int id, IReadOnlyDictionary<int, FixedElementConfig> rows)
        {
            if (rows.TryGetValue(id, out FixedElementConfig target) && target.Kind != FixedElementKind.EndpointLabel) return target;
            Fail(source, "引用不存在、跨关或不产生实体的固定元素行：" + id); return null;
        }
        private static void Fail(FixedElementConfig element, string reason) => throw new ArgumentException(element.OwnerKey + "：" + reason);
    }
}
