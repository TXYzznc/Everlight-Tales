using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 零件能力解释器（P1-007）：把一次触发（碰撞／冲击）结算为「触发分 + 自身效果 + 公共维修能量」。
    /// 数值来自 <see cref="PartCatalog"/>（Data 层），能力行为在此解释（Board 层负责盘面变化）。
    /// 触发分与效果成功无关、不消耗能量；自身效果按实时零件能量检查并消耗。
    /// 本类型不引用引擎。
    /// </summary>
    public static class PartAbility
    {
        // D-060 首批通用零件形态（F 类）ID：撞锤两态 + 线圈两态。
        private const string FormPierceHammer = "P-001-F01";
        private const string FormSweepHammer = "P-001-F02";
        private const string FormAxialCoil = "P-003-F01";
        private const string FormFuseCoil = "P-003-F02";

        private readonly struct Outcome
        {
            public readonly int EffectScore;
            public readonly int PublicEnergyDelta;

            public Outcome(int effectScore, int publicEnergyDelta)
            {
                EffectScore = effectScore;
                PublicEnergyDelta = publicEnergyDelta;
            }
        }

        /// <summary>
        /// 结算一次触发：发放触发分，再按零件类型执行自身效果，最后记录结算日志。
        /// 目标须为在盘零件且具有能力配置；否则静默返回（不记录触发）。
        /// </summary>
        public static void ResolveTrigger(SettlementContext context, int sourceId, int targetId, string kind, HexDirection incoming)
        {
            if (!context.Board.TryGetEntity(targetId, out BoardEntity target))
            {
                return;
            }

            if (target.Kind != EntityKind.Part)
            {
                return;
            }

            PartConfig config = PartCatalog.Get(target.PartType);
            if (config == null)
            {
                return;
            }

            int triggerScore = config.TriggerScore + context.Bonuses.TriggerBonus(target.PartType);
            context.Tap.TriggerScore += triggerScore;

            Outcome outcome;
            switch (target.PartType)
            {
                case PartType.InertiaHammer:
                    outcome = HammerForm(context, target, sourceId, kind == TriggerKinds.Collision, incoming, config);
                    break;

                case PartType.MeteringRatchet:
                    outcome = Ratchet(context, target, config);
                    break;

                case PartType.BlastCoil:
                    outcome = CoilForm(context, target, config);
                    break;

                case PartType.ReversalGear:
                    outcome = ReversalGear(context, target, sourceId, kind == TriggerKinds.Collision, incoming, config);
                    break;

                case PartType.RivetPliers:
                    outcome = RivetPliers(context, target);
                    break;

                case PartType.SpringLauncher:
                    outcome = SpringLauncher(context, target, sourceId, kind == TriggerKinds.Collision, incoming, config);
                    break;

                case PartType.SplitMold:
                    outcome = SplitMold(context, target, sourceId, kind == TriggerKinds.Collision, config);
                    break;

                case PartType.MaterialFurnace:
                    outcome = MaterialFurnace(context, target, sourceId, kind == TriggerKinds.Collision, config);
                    break;

                case PartType.SwapFork:
                    outcome = SwapFork(context, target, config);
                    break;

                case PartType.VortexRotor:
                    outcome = VortexRotor(context, target, config);
                    break;

                case PartType.EnergyFlywheel:
                    outcome = EnergyFlywheel(context, target, config);
                    break;

                case PartType.MagneticTractor:
                    outcome = MagneticTractor(context, target, config);
                    break;

                case PartType.RelayBattery:
                    outcome = RelayBattery(context, target, config);
                    break;

                case PartType.StorageStomach:
                    outcome = StorageStomach(context, target, sourceId, kind == TriggerKinds.Collision, config);
                    break;

                case PartType.LightingPrism:
                    outcome = LightingPrism(context, target, config);
                    break;

                case PartType.DrainImpeller:
                    outcome = DrainImpeller(context, target, config);
                    break;

                case PartType.CalibrationProbe:
                    outcome = CalibrationProbe(context, target, config);
                    break;

                case PartType.ConductiveBridge:
                    outcome = ConductiveBridge(context, target, config);
                    break;

                case PartType.TuningFork:
                    outcome = TuningFork(context, target, config);
                    break;

                case PartType.BufferBladder:
                    outcome = BufferBladder(context, target, config);
                    break;

                default:
                    outcome = default;
                    break;
            }

            context.Tap.EffectScore += outcome.EffectScore;
            context.Session.PublicRepairEnergy += outcome.PublicEnergyDelta;

            context.Log.Add(new SettlementEvent(
                SettlementEventKind.TriggerResolved,
                entityId: sourceId,
                targetId: targetId,
                message: kind,
                scoreDelta: triggerScore + outcome.EffectScore,
                energyDelta: outcome.PublicEnergyDelta));
        }

        /// <summary>撞锤：有实体入射且能量足够时，消耗 1 点把来撞实体沿入射反方向推移 1 格，每格 8 效果分。</summary>
        private static Outcome Hammer(SettlementContext context, BoardEntity target, int sourceId, bool hasIncoming, HexDirection incoming, PartConfig config)
        {
            // 冲击等无实体入射：只触发分，不推移、不耗能。
            if (!hasIncoming)
            {
                return default;
            }

            if (target.Energy < config.EffectCost)
            {
                return default; // 能量不足：不耗能、不推移。
            }

            target.Energy -= config.EffectCost;

            if (!context.Board.TryGetEntity(sourceId, out BoardEntity source) || !source.IsMovable)
            {
                return default;
            }

            // BF-016 长程撞锤：反推距离 +N 格；逐格执行，遇阻挡停止本次移动请求。
            int distance = 1 + context.Bonuses.DistanceBonus(target.PartType);
            int effectPerCell = config.EffectScorePerCell + context.Bonuses.EffectUnitBonus(target.PartType);
            int moved = 0;
            for (int step = 0; step < distance; step++)
            {
                HexCoord destination = source.Coord.Neighbor(HexDirections.Opposite(incoming));
                if (!context.Board.IsValid(destination) || context.Board.IsOccupied(destination))
                {
                    break;
                }

                context.Board.Move(source, destination, MoveSource.Push);
                moved++;
            }

            return moved > 0 ? new Outcome(moved * effectPerCell, 0) : default;
        }

        /// <summary>撞锤形态分派（D-060）：贯通 / 横推 / 基础。</summary>
        private static Outcome HammerForm(SettlementContext context, BoardEntity target, int sourceId, bool hasIncoming, HexDirection incoming, PartConfig config)
        {
            if (target.FormId == FormPierceHammer)
            {
                return PierceHammer(context, target, sourceId, hasIncoming, incoming, config);
            }

            if (target.FormId == FormSweepHammer)
            {
                return SweepHammer(context, target, sourceId, hasIncoming, incoming, config);
            }

            return Hammer(context, target, sourceId, hasIncoming, incoming, config);
        }

        /// <summary>
        /// 贯通撞锤（P-001-F01，D-060）：被实体撞击时推移输出侧对象（被撞零件沿入射方向相邻格）1 格，
        /// 来撞件留原位。输出侧无对象／不可动时已支付能量不退还。
        /// </summary>
        private static Outcome PierceHammer(SettlementContext context, BoardEntity target, int sourceId, bool hasIncoming, HexDirection incoming, PartConfig config)
        {
            if (!hasIncoming)
            {
                return default; // 冲击无实体入射：只触发分。
            }

            if (target.Energy < config.EffectCost)
            {
                return default; // 能量不足：不耗能、不推移。
            }

            target.Energy -= config.EffectCost;

            BoardEntity outputEntity = context.Board.EntityAt(target.Coord.Neighbor(incoming));
            if (outputEntity == null || !outputEntity.IsMovable)
            {
                return default; // 输出侧无对象／固定：已支付能量不退还。
            }

            int distance = 1 + context.Bonuses.DistanceBonus(target.PartType);
            int effectPerCell = config.EffectScorePerCell + context.Bonuses.EffectUnitBonus(target.PartType);
            int moved = 0;
            for (int step = 0; step < distance; step++)
            {
                HexCoord destination = outputEntity.Coord.Neighbor(incoming);
                if (!context.Board.IsValid(destination) || context.Board.IsOccupied(destination))
                {
                    break;
                }

                context.Board.Move(outputEntity, destination, MoveSource.Push);
                moved++;
            }

            return moved > 0 ? new Outcome(moved * effectPerCell, 0) : default;
        }

        /// <summary>
        /// 横推撞锤（P-001-F02，D-060）：被实体撞击时把入射方向的顺／逆时针相邻两路对象各推 1 格，
        /// 两路合计费 1，缺一路照常结算另一路。
        /// </summary>
        private static Outcome SweepHammer(SettlementContext context, BoardEntity target, int sourceId, bool hasIncoming, HexDirection incoming, PartConfig config)
        {
            if (!hasIncoming)
            {
                return default;
            }

            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            target.Energy -= config.EffectCost;

            int effectPerCell = config.EffectScorePerCell + context.Bonuses.EffectUnitBonus(target.PartType);
            int totalMoved = 0;
            totalMoved += PushSide(context, target, HexDirections.Rotate(incoming, 1));
            totalMoved += PushSide(context, target, HexDirections.Rotate(incoming, -1));
            return totalMoved > 0 ? new Outcome(totalMoved * effectPerCell, 0) : default;
        }

        /// <summary>把撞锤某侧相邻格对象沿该方向推 1 格，返回是否成功推移。</summary>
        private static int PushSide(SettlementContext context, BoardEntity target, HexDirection direction)
        {
            BoardEntity entity = context.Board.EntityAt(target.Coord.Neighbor(direction));
            if (entity == null || !entity.IsMovable)
            {
                return 0;
            }

            HexCoord destination = entity.Coord.Neighbor(direction);
            if (!context.Board.IsValid(destination) || context.Board.IsOccupied(destination))
            {
                return 0;
            }

            context.Board.Move(entity, destination, MoveSource.Push);
            return 1;
        }

        /// <summary>棘轮：能量足够时消耗 1 点、产出 1 点公共维修能量；本拍第 N 次及以后成功产能另得效果分。</summary>
        private static Outcome Ratchet(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default; // 能量不足：只触发分。
            }

            target.Energy -= config.EffectCost;
            int successCount = context.Tap.BumpEffectCount(target.Id);
            int bonus = successCount >= config.BonusNth ? config.BonusScoreFromNth : 0;
            int energy = config.PublicEnergyPerEffect + context.Bonuses.PublicEnergyBonus(target.PartType);
            return new Outcome(bonus, energy);
        }

        /// <summary>线圈：能量足够时消耗 1 点、向六邻格发冲击（每个实际接受的零件 6 效果分）并移除本体。</summary>
        private static Outcome Coil(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default; // 能量不足：只触发分，不爆炸、不移除。
            }

            target.Energy -= config.EffectCost;

            HexCoord center = target.Coord;
            var hitTargets = new List<BoardEntity>();
            for (int i = 0; i < HexDirections.Count; i++)
            {
                BoardEntity occupant = context.Board.EntityAt(center.Neighbor(HexDirections.All[i]));
                if (occupant == null)
                {
                    continue;
                }

                if (occupant.Kind == EntityKind.Part && occupant.PartType != PartType.None)
                {
                    hitTargets.Add(occupant);
                }
                else if (occupant.ObstacleType != ObstacleType.None && occupant.MaxDurability > 0)
                {
                    // D-118：爆破冲击扣耐久障碍（O-002 脆裂隔板／O-008 承压支柱／O-007 封条）。
                    ObstacleService.Damage(context, occupant);
                }
            }

            // 起爆后移除本体（移除后仍按快照向邻件发冲击）。
            context.Board.Remove(target);

            for (int i = 0; i < hitTargets.Count; i++)
            {
                context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock, target.Id, hitTargets[i].Id));
            }

            int effectPerTarget = config.EffectScorePerTarget + context.Bonuses.EffectUnitBonus(target.PartType);
            return new Outcome(hitTargets.Count * effectPerTarget, 0);
        }

        /// <summary>线圈形态分派（D-060）：轴向 / 定时 / 基础。</summary>
        private static Outcome CoilForm(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.FormId == FormAxialCoil)
            {
                return AxialCoil(context, target, config);
            }

            if (target.FormId == FormFuseCoil)
            {
                return FuseCoil(context, target, config);
            }

            return Coil(context, target, config);
        }

        /// <summary>
        /// 轴向线圈（P-003-F01，D-060）：受触发时沿 D0、D3 各发最长 2 格射线，命中首个实体即停
        /// （零件发冲击、耐久障碍扣耐久），然后移除本体。
        /// </summary>
        private static Outcome AxialCoil(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default; // 能量不足：只触发分，不发射线、不移除。
            }

            target.Energy -= config.EffectCost;

            var hitTargets = new List<BoardEntity>();
            ScanRay(context, target.Coord, HexDirection.D0, 2, hitTargets);
            ScanRay(context, target.Coord, HexDirection.D3, 2, hitTargets);

            // 起爆后移除本体（移除后仍按快照向命中件发冲击）。
            context.Board.Remove(target);

            for (int i = 0; i < hitTargets.Count; i++)
            {
                context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock, target.Id, hitTargets[i].Id));
            }

            int effectPerTarget = config.EffectScorePerTarget + context.Bonuses.EffectUnitBonus(target.PartType);
            return new Outcome(hitTargets.Count * effectPerTarget, 0);
        }

        /// <summary>从 center 沿 direction 扫描最多 range 格，命中首个实体即停（零件计入命中、耐久障碍扣耐久）。</summary>
        private static void ScanRay(SettlementContext context, HexCoord center, HexDirection direction, int range, List<BoardEntity> hits)
        {
            HexCoord cell = center;
            for (int i = 0; i < range; i++)
            {
                cell = cell.Neighbor(direction);
                if (!context.Board.IsValid(cell))
                {
                    break;
                }

                BoardEntity occupant = context.Board.EntityAt(cell);
                if (occupant == null)
                {
                    continue; // 空格继续延伸。
                }

                if (occupant.Kind == EntityKind.Part && occupant.PartType != PartType.None)
                {
                    hits.Add(occupant);
                }
                else if (occupant.ObstacleType != ObstacleType.None && occupant.MaxDurability > 0)
                {
                    ObstacleService.Damage(context, occupant);
                }

                break; // 命中首个实体即停。
            }
        }

        /// <summary>
        /// 定时线圈（P-003-F02，D-060）：首次触发支付 1 进入待爆（到期拍 = 当前拍 + 1），
        /// 待爆再触发只加触发分不支付不累积；到期爆破由 FuseCoilService 在到期拍扣额度后执行。
        /// </summary>
        private static Outcome FuseCoil(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.FuseDueTap > 0)
            {
                return default; // 已待爆：本次触发只加触发分（开头已加）。
            }

            if (target.Energy < config.EffectCost)
            {
                return default; // 能量不足：不启动待爆。
            }

            target.Energy -= config.EffectCost;
            target.SetFuseDueTap(context.Session.TapSerial + 1);
            return default; // 启动时效果分 0。
        }

        /// <summary>换向齿轮（P-004）：有实体入射且能量足够时，消耗 1 点把来撞实体沿入射方向顺时针偏转 60° 推进 1 格。</summary>
        private static Outcome ReversalGear(SettlementContext context, BoardEntity target, int sourceId, bool hasIncoming, HexDirection incoming, PartConfig config)
        {
            if (!hasIncoming)
            {
                return default; // 信号无入射实体：不换向不耗能。
            }

            if (target.Energy < config.EffectCost)
            {
                return default; // 能量不足：只触发分。
            }

            target.Energy -= config.EffectCost;

            if (!context.Board.TryGetEntity(sourceId, out BoardEntity source) || !source.IsMovable)
            {
                return default;
            }

            HexDirection deflected = HexDirections.Rotate(incoming, 1); // 顺时针偏转 60°（D0→D1）。
            HexCoord destination = source.Coord.Neighbor(deflected);
            if (!context.Board.IsValid(destination) || context.Board.IsOccupied(destination))
            {
                return default; // 落点非法：耗能但不移动。
            }

            context.Board.Move(source, destination);
            return new Outcome(config.EffectScorePerCell + context.Bonuses.EffectUnitBonus(target.PartType), 0);
        }

        /// <summary>铆合钳（P-014）：受击／信号时检查 D0 邻格可修复节点，公共维修能量足够时入队一次维修。</summary>
        private static Outcome RivetPliers(SettlementContext context, BoardEntity target)
        {
            BoardEntity node = context.Board.EntityAt(target.Coord.Neighbor(HexDirection.D0));
            if (node == null || node.Kind != EntityKind.RepairTarget || node.RepairCompleted)
            {
                return default; // 无合法目标：只触发分。
            }

            if (node.RepairConfig == null || !node.RepairConfig.Accepts(target.PartType))
            {
                return default;
            }

            if (context.Session.PublicRepairEnergy < node.RepairConfig.EnergyCost)
            {
                return default; // 公共储备不足：只触发分，不扣能量。
            }

            context.Queue.Enqueue(new RepairEffect(target.PartType, node.Id));
            return default; // 维修效果分由 RepairEffect 记入。
        }

        /// <summary>弹射簧（P-005）：反向逐格推进最多 PushDistance 格，遇障停在首碰撞处并发碰撞。</summary>
        private static Outcome SpringLauncher(SettlementContext context, BoardEntity target, int sourceId, bool hasIncoming, HexDirection incoming, PartConfig config)
        {
            if (!hasIncoming)
            {
                return default; // 纯冲击不触发弹射。
            }

            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            target.Energy -= config.EffectCost;

            if (!context.Board.TryGetEntity(sourceId, out BoardEntity source) || !source.IsMovable)
            {
                return default;
            }

            int distance = config.PushDistance;
            int effectPerCell = config.EffectScorePerCell + context.Bonuses.EffectUnitBonus(target.PartType);
            int moved = 0;
            for (int step = 0; step < distance; step++)
            {
                HexCoord destination = source.Coord.Neighbor(HexDirections.Opposite(incoming));
                if (!context.Board.IsValid(destination))
                {
                    break;
                }

                if (context.Board.IsOccupied(destination))
                {
                    BoardEntity blocker = context.Board.EntityAt(destination);
                    if (blocker != null && blocker.Kind == EntityKind.Part && blocker.PartType != PartType.None)
                    {
                        context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Collision, source.Id, blocker.Id, HexDirections.Opposite(incoming)));
                    }

                    break;
                }

                context.Board.Move(source, destination, MoveSource.Push);
                moved++;
            }

            return moved > 0 ? new Outcome(moved * effectPerCell, 0) : default;
        }

        /// <summary>分裂铸模（P-006）：复制来撞可复制普通件到六邻格首个空位，副本不可再作源。</summary>
        private static Outcome SplitMold(SettlementContext context, BoardEntity target, int sourceId, bool hasIncoming, PartConfig config)
        {
            if (!hasIncoming)
            {
                return default;
            }

            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            if (!context.Board.TryGetEntity(sourceId, out BoardEntity source) || source.Kind != EntityKind.Part)
            {
                return default;
            }

            if (source.PartType == PartType.None || source.IsCopy)
            {
                return default; // 惰性件或复制体不能作源。
            }

            if (source.PartType == PartType.SplitMold)
            {
                return default; // 不复制铸模自身（防铸模链式复制）。
            }

            HexCoord spawn = default;
            bool found = false;
            for (int i = 0; i < HexDirections.Count; i++)
            {
                HexCoord candidate = target.Coord.Neighbor(HexDirections.All[i]);
                if (context.Board.IsValid(candidate) && !context.Board.IsOccupied(candidate))
                {
                    spawn = candidate;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return default; // 无空位：不复制、不消耗。
            }

            target.Energy -= config.EffectCost;

            int newId = context.Board.AllocateEntityId();
            BoardEntity copy = BoardEntity.Part(newId, source.PartType);
            copy.SetEnergy(source.Energy); // 副本充能=源当前充能。
            copy.SetCopy();
            context.Board.Place(copy, spawn);

            return new Outcome(config.EffectScorePerTarget, 0);
        }

        /// <summary>吞料炉（P-008）：吞入（移除）来撞可回收普通件，得 12+剩余能量×2 分并产 2 公共能量。</summary>
        private static Outcome MaterialFurnace(SettlementContext context, BoardEntity target, int sourceId, bool hasIncoming, PartConfig config)
        {
            if (!hasIncoming)
            {
                return default;
            }

            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            if (!context.Board.TryGetEntity(sourceId, out BoardEntity source) || source.Kind != EntityKind.Part)
            {
                return default;
            }

            if (source.PartType == PartType.None || source.PartType == PartType.MaterialFurnace || source.PartType == PartType.StorageStomach)
            {
                return default; // 惰性件／另一吞料炉／胃袋不可吞。
            }

            target.Energy -= config.EffectCost;

            int remainingEnergy = source.Energy;
            context.Board.Remove(source);

            int score = config.EffectScorePerTarget + remainingEnergy * config.EffectScorePerEnergy;
            int energy = config.PublicEnergyPerEffect + context.Bonuses.PublicEnergyBonus(target.PartType);
            return new Outcome(score, energy);
        }

        /// <summary>交换拨叉（P-009）：原子交换 D0/D3 邻格两个可移动普通实体，一空则移动另一件。</summary>
        private static Outcome SwapFork(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            target.Energy -= config.EffectCost; // 失败也消耗。

            HexCoord c0 = target.Coord.Neighbor(HexDirection.D0);
            HexCoord c3 = target.Coord.Neighbor(HexDirection.D3);
            if (!context.Board.IsValid(c0) || !context.Board.IsValid(c3))
            {
                return default;
            }

            BoardEntity e0 = context.Board.EntityAt(c0);
            BoardEntity e3 = context.Board.EntityAt(c3);

            if (IsSwapBlocked(e0) || IsSwapBlocked(e3))
            {
                return default;
            }

            if (e0 != null && e3 != null)
            {
                context.Board.Remove(e0);
                context.Board.Remove(e3);
                context.Board.Place(e0, c3);
                context.Board.Place(e3, c0);
                return new Outcome(2 * config.EffectScorePerTarget, 0);
            }

            if (e0 != null)
            {
                context.Board.Move(e0, c3, MoveSource.Push);
                return new Outcome(config.EffectScorePerTarget, 0);
            }

            if (e3 != null)
            {
                context.Board.Move(e3, c0, MoveSource.Push);
                return new Outcome(config.EffectScorePerTarget, 0);
            }

            return default; // 两空不操作。
        }

        private static bool IsSwapBlocked(BoardEntity e)
        {
            return e != null && (e.IsLocked || e.Kind == EntityKind.TaskMarker || e.IsFixed);
        }

        /// <summary>旋涡转子（P-010）：六邻格 D0→D1→…→D5→D0 旋移一位，环上有固定/任务对象则整体取消。</summary>
        private static Outcome VortexRotor(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            var ring = new BoardEntity[HexDirections.Count];
            for (int i = 0; i < HexDirections.Count; i++)
            {
                HexCoord c = target.Coord.Neighbor(HexDirections.All[i]);
                if (!context.Board.IsValid(c))
                {
                    return default; // 邻格越界：取消（不耗能）。
                }

                ring[i] = context.Board.EntityAt(c);
                if (ring[i] != null && (ring[i].IsFixed || ring[i].Kind == EntityKind.TaskMarker))
                {
                    return default; // 环上有固定物/任务标记：整体取消。
                }
            }

            target.Energy -= config.EffectCost;

            for (int i = 0; i < HexDirections.Count; i++)
            {
                if (ring[i] != null)
                {
                    context.Board.Remove(ring[i]);
                }
            }

            int moved = 0;
            for (int i = 0; i < HexDirections.Count; i++)
            {
                if (ring[i] != null)
                {
                    HexCoord dest = target.Coord.Neighbor(HexDirections.All[(i + 1) % HexDirections.Count]);
                    context.Board.Place(ring[i], dest);
                    moved++;
                }
            }

            return moved > 0 ? new Outcome(moved * config.EffectScorePerTarget, 0) : default;
        }

        /// <summary>蓄能飞轮（P-011）：受击蓄量+1，达阈值后清蓄并六向射线（最远 RayRange 格）。</summary>
        private static Outcome EnergyFlywheel(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            target.Energy -= config.EffectCost;
            target.AddCharge(1);

            if (target.Charge < config.ChargeThreshold)
            {
                return default; // 未达阈值：只蓄。
            }

            target.ResetCharge();

            var hitTargets = new List<BoardEntity>();
            for (int i = 0; i < HexDirections.Count; i++)
            {
                HexDirection dir = HexDirections.All[i];
                for (int dist = 1; dist <= config.RayRange; dist++)
                {
                    HexCoord coord = target.Coord;
                    for (int s = 0; s < dist; s++)
                    {
                        coord = coord.Neighbor(dir);
                    }

                    if (!context.Board.IsValid(coord))
                    {
                        break;
                    }

                    BoardEntity occupant = context.Board.EntityAt(coord);
                    if (occupant == null)
                    {
                        continue; // 空格：继续延伸。
                    }

                    if (occupant.Kind == EntityKind.Part && occupant.PartType != PartType.None)
                    {
                        hitTargets.Add(occupant);
                    }

                    break; // 实体或设施终止射线。
                }
            }

            for (int i = 0; i < hitTargets.Count; i++)
            {
                context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock, target.Id, hitTargets[i].Id));
            }

            int effectPerTarget = config.EffectScorePerTarget + context.Bonuses.EffectUnitBonus(target.PartType);
            return new Outcome(hitTargets.Count * effectPerTarget, 0);
        }

        /// <summary>磁吸牵引器（P-019）：D0 射线最远 RayRange 找首个可磁吸实体，朝自身拉近最多 PushDistance 格。</summary>
        private static Outcome MagneticTractor(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            target.Energy -= config.EffectCost; // 无目标仍消耗。

            BoardEntity found = null;
            for (int dist = 1; dist <= config.RayRange; dist++)
            {
                HexCoord coord = target.Coord;
                for (int s = 0; s < dist; s++)
                {
                    coord = coord.Neighbor(HexDirection.D0);
                }

                if (!context.Board.IsValid(coord))
                {
                    break;
                }

                if (context.Board.IsWall(coord))
                {
                    break; // 墙体阻断搜索。
                }

                BoardEntity occupant = context.Board.EntityAt(coord);
                if (occupant != null)
                {
                    found = occupant;
                    break;
                }
            }

            if (found == null || found.Kind != EntityKind.Part || !found.IsMovable)
            {
                return default;
            }

            int moved = 0;
            for (int step = 0; step < config.PushDistance; step++)
            {
                HexCoord destination = found.Coord.Neighbor(HexDirections.Opposite(HexDirection.D0));
                if (destination == target.Coord)
                {
                    break; // 不拉进自身格。
                }

                if (!context.Board.IsValid(destination) || context.Board.IsOccupied(destination))
                {
                    break;
                }

                context.Board.Move(found, destination, MoveSource.Push);
                moved++;
            }

            return moved > 0 ? new Outcome(moved * config.EffectScorePerCell, 0) : default;
        }

        /// <summary>接力电池（P-020）：六邻格按顺序补零件能量，实际转出多少扣多少，电量归零移除本体。</summary>
        private static Outcome RelayBattery(SettlementContext context, BoardEntity target, PartConfig config)
        {
            int transferred = 0;
            for (int i = 0; i < HexDirections.Count; i++)
            {
                BoardEntity occupant = context.Board.EntityAt(target.Coord.Neighbor(HexDirections.All[i]));
                if (occupant == null || occupant.Kind != EntityKind.Part)
                {
                    continue;
                }

                if (occupant.PartType == PartType.None || occupant.PartType == PartType.RelayBattery)
                {
                    continue; // 惰性件／电池本身不补。
                }

                PartConfig oc = PartCatalog.Get(occupant.PartType);
                if (oc == null)
                {
                    continue;
                }

                while (target.Energy > 0 && occupant.Energy < occupant.EnergyCapacity)
                {
                    occupant.SetEnergy(occupant.Energy + 1);
                    target.Energy -= 1;
                    transferred++;
                }

                if (target.Energy <= 0)
                {
                    break;
                }
            }

            if (target.Energy <= 0)
            {
                context.Board.Remove(target); // 电量归零移除本体。
            }

            return transferred > 0 ? new Outcome(transferred * config.EffectScorePerCell, 0) : default;
        }

        /// <summary>储料胃袋（P-007）：收纳模式吞入来撞件，释放模式受击吐出到 D0 出口。</summary>
        private static Outcome StorageStomach(SettlementContext context, BoardEntity target, int sourceId, bool hasIncoming, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            if (!target.ReleaseMode)
            {
                // 收纳模式：碰撞时吞入来撞普通件。
                if (!hasIncoming)
                {
                    return default;
                }

                if (!context.Board.TryGetEntity(sourceId, out BoardEntity source) || source.Kind != EntityKind.Part)
                {
                    return default;
                }

                if (source.PartType == PartType.None || source.PartType == PartType.StorageStomach)
                {
                    return default; // 惰性件／另一胃袋不可收纳。
                }

                if (target.Stored.Count >= 2)
                {
                    return default; // 内部已满：按普通阻挡，不收纳。
                }

                target.Energy -= config.EffectCost;
                context.Board.Remove(source);
                target.Store(source);
                return new Outcome(config.EffectScorePerTarget, 0); // 收纳 6 分。
            }

            // 释放模式：受击释放最早收纳者到 D0 出口。
            if (target.Stored.Count == 0)
            {
                return default; // 无内部物。
            }

            HexCoord exit = target.Coord.Neighbor(HexDirection.D0);
            if (!context.Board.IsValid(exit) || context.Board.IsOccupied(exit))
            {
                return default; // 无空格：保留内部物，释放状态保留。
            }

            BoardEntity stored = target.TakeStored();
            target.Energy -= config.EffectCost;
            context.Board.Place(stored, exit);
            target.ReleaseMode = false; // 释放成功后回到收纳状态。
            return new Outcome(config.SecondaryEffectScore, 0); // 释放 10 分。
        }

        /// <summary>照明棱镜（P-013）：照明自身六邻格，每照明实体 2 分，每揭一层伪装 6 分。</summary>
        private static Outcome LightingPrism(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            target.Energy -= config.EffectCost;

            int lit = 0;
            int disguised = 0;
            for (int i = 0; i < HexDirections.Count; i++)
            {
                BoardEntity neighbor = context.Board.EntityAt(target.Coord.Neighbor(HexDirections.All[i]));
                if (neighbor == null)
                {
                    continue;
                }

                if (neighbor.Kind == EntityKind.Part && neighbor.PartType != PartType.None)
                {
                    lit++; // 每照明一个不同实体。
                }

                if (neighbor.RemoveDisguiseLayer())
                {
                    disguised++; // 揭一层伪装。
                }
            }

            int score = lit * config.EffectScorePerTarget + disguised * config.SecondaryEffectScore;
            return score > 0 ? new Outcome(score, 0) : default;
        }

        /// <summary>排水叶轮（P-016）：从 D0 邻格水负荷容器排掉最多 2 单位，每单位 8 分 + 1 公共能量。</summary>
        private static Outcome DrainImpeller(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            BoardEntity container = context.Board.EntityAt(target.Coord.Neighbor(HexDirection.D0));
            if (container == null || container.WaterLoad <= 0)
            {
                return default; // 无水：不扣充能。
            }

            target.Energy -= config.EffectCost;
            int drained = container.DrainWater(2);

            int score = drained * config.EffectScorePerTarget;
            int energy = drained * config.PublicEnergyPerEffect;
            return new Outcome(score, energy);
        }

        /// <summary>校准探针（P-018）：向 D0 邻格发校准事件，仅对声明接受校准的节点生效得 8 分。</summary>
        private static Outcome CalibrationProbe(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            target.Energy -= config.EffectCost; // 无效目标该次充能也已使用。

            BoardEntity node = context.Board.EntityAt(target.Coord.Neighbor(HexDirection.D0));
            if (node == null || !node.AcceptsCalibration)
            {
                return default;
            }

            return new Outcome(config.EffectScorePerTarget, 0);
        }

        /// <summary>导电桥（P-012）：受击沿 D3 端口转发脉冲到桥或声音端点，转发一次得 8 分（本拍去重防环路）。</summary>
        private static Outcome ConductiveBridge(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            // 本拍已转发过则不重复转发（简化环路去重；完整按根信号去重待信号系统批）。
            if (context.Tap.EffectCounts.ContainsKey(target.Id))
            {
                return default;
            }

            HexCoord next = target.Coord.Neighbor(HexDirection.D3);
            BoardEntity peer = context.Board.EntityAt(next);
            if (peer == null)
            {
                return default; // 无可发送邻端：不消耗、不记为已转发。
            }

            if (peer.Kind == EntityKind.Part && peer.PartType == PartType.ConductiveBridge)
            {
                target.Energy -= config.EffectCost;
                context.Tap.BumpEffectCount(target.Id);
                context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock, target.Id, peer.Id));
                return new Outcome(config.EffectScorePerTarget, 0);
            }

            if (peer.SoundEndpoint != null)
            {
                target.Energy -= config.EffectCost;
                context.Tap.BumpEffectCount(target.Id);
                return new Outcome(config.EffectScorePerTarget, 0);
            }

            return default;
        }

        /// <summary>叩击音叉（P-015）：受击产生根声音，沿 D0 端口传播，端点接收得 4 分。</summary>
        private static Outcome TuningFork(SettlementContext context, BoardEntity target, PartConfig config)
        {
            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            target.Energy -= config.EffectCost;

            HexCoord next = target.Coord.Neighbor(HexDirection.D0);
            BoardEntity peer = context.Board.EntityAt(next);
            if (peer == null)
            {
                return default;
            }

            if (peer.SoundEndpoint != null)
            {
                return new Outcome(config.EffectScorePerTarget, 0); // 端点接收 4 分。
            }

            if (peer.Kind == EntityKind.Part && peer.PartType == PartType.ConductiveBridge)
            {
                context.Queue.Enqueue(new PartTriggerEvent(TriggerKinds.Shock, target.Id, peer.Id));
                return default; // 桥转发得分由桥记。
            }

            return default;
        }

        /// <summary>缓冲囊（P-017）：接收模式被动吸负荷（0 费），排放模式主动交负荷到处置端（1 费，8 分/单位）。</summary>
        private static Outcome BufferBladder(SettlementContext context, BoardEntity target, PartConfig config)
        {
            HexCoord d0 = target.Coord.Neighbor(HexDirection.D0);
            BoardEntity peer = context.Board.EntityAt(d0);

            if (!target.DischargeMode)
            {
                // 接收模式：从 D0 邻格负荷源吸收（被动，0 费）。
                if (peer == null || peer.LoadAmount <= 0)
                {
                    return default;
                }

                if (target.LoadAmount >= 3)
                {
                    return default; // 已满：剩余交回原负荷端（简化：本次不吸收）。
                }

                peer.DrainLoad(1);
                target.AddLoad(1);
                return default; // 容纳不另发效果分。
            }

            // 排放模式：主动交出负荷（1 费）。
            if (target.Energy < config.EffectCost)
            {
                return default;
            }

            if (target.LoadAmount <= 0 || peer == null || !peer.AcceptsLoad)
            {
                return default; // 无兼容处置端：保持储存，不销毁负荷。
            }

            target.Energy -= config.EffectCost;
            int delivered = target.DrainLoad(1);
            peer.AddLoad(delivered); // 处置端接收（转移保持负荷守恒）。
            return new Outcome(delivered * config.EffectScorePerTarget, 0);
        }
    }
}
