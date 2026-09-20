# 设计：b18 普通维修事件壳 + 卷帘门 + G2 门

## 1. 移动来源与锁定（D-133）

```csharp
// Board/MoveSource.cs
public enum MoveSource : byte { Arm = 0, Gravity = 1, Push = 2, }
```

- `BoardEntity.IsLocked`（internal set，默认 false）；`IsMovable => !IsFixed && !IsLocked`；`Lock()`、`SetLocked(bool)`（存档恢复用）。
- `BoardEntity.MovableTaskMarker(int id, GoalConfig goal = null)`：可移动任务标记（IsFixed=false），携带特殊目标。
- `BoardState.Move(entity, target, MoveSource source = Gravity)`：
  - 移动校验 `IsFixed` 改为 `!IsMovable`（锁定实体不可移动）。
  - 落位后 `_lastMoveSource[entity.Id] = source`；`LastMoveSource(id)` 返回（缺省 Gravity）。
  - 锁定落位：`source==Push && entity.Kind==TaskMarker && entity.Goal?.Kind==GoalKind.PushedInto && TryGetLabelCoord(Goal.AnchorLabel)==target` → `entity.Lock()`。**在结算内 Move 当场锁定**，防重力接续把标记拉回。
- 来源标注：`ArmService.Move` → `MoveSource.Arm`；`PartAbility` 撞锤/换向齿轮推移 → `MoveSource.Push`；`TapSettlement.SlideEntity`（重力）保持默认 `Gravity`；`AnomalyService`（规则传送）保持默认 `Gravity`（不计推移）。

## 2. 推移进格目标（D-133/D-134）

- `GoalKind.PushedInto = 4`（Data/GoalConfig.cs）。
- `GoalConfig(id, PushedInto, entityId, anchorLabel)`：锚点格 = 校正格 G 的端点标签。
- `GoalEvaluator.AnchorGoalState.Tick` 增 `PushedInto` 分支：`AtLabel(anchorLabel, entityId) && 该格 occupant.IsLocked`。
- 卷帘门的目标判定不挂在 SpecialGoalState，由 `RollerDoorEvent.IsGatePushedIn(board)` 直读 `B.IsLocked`（锁定即「被普通推移送入 G」）。

## 3. 事件壳（D-132）

```csharp
// Data/RepairEvent.cs
public enum EventResultKind : byte { None = 0, Success = 1, Failure = 2, Retreat = 3, }
public sealed class EventReward { public int RepairFee; public IReadOnlyList<string> Materials; ... }
public sealed class RepairEventConfig {
    public string Id; public string Name; public int TimeCost;
    public LevelConfig Level; public EventReward SuccessReward; public int InitialArmMoves;
}

// Events/RepairEventShell.cs
public enum RepairEventState : byte { Created, Prepared, InBoard, Settled, }
public sealed class RepairEventInstance { Config/State/Result/Board/Level; IsSettled/Succeeded; }
public static class RepairEventShell {
    Begin(config, board) -> instance（new LevelState(config.Level)+BeginLevel+Session.ArmMoves=config.InitialArmMoves，态=Prepared）
    EvaluateOutcome(instance, specialGoalMet) -> Success/Failure（score>=target && AllGoalsComplete && specialGoalMet）
    Resolve(instance, result) -> EventReward|null（写 Result/态=Settled；成功发 SuccessReward，失败/撤退返 null 轻量结案）
}
```

- 失败/撤退轻量结案 = 结算后无奖励、无永久负面；事件模板不变，同类事件可重现（事件实例层不污染模板）。

## 4. 卷帘门 EV-N01（D-134）

```csharp
// Events/RollerDoorEvent.cs
public static class RollerDoorEvent {
    public const int GateLabel = 1;
    CreateConfig() -> RepairEventConfig("EV-N01","卡住的卷帘门",2, LevelConfig([RoundConfig(3,30)]), EventReward(40,["MT-002"]), initialArmMoves:2)
    CreateBoardConfig() -> LevelBoardConfig(levelName, sideLength:5,
        keyPieces:[KeyPieceConfig(InertiaHammer,(0,0))],        // H
        carryPool:[Hammer,Ratchet,Coil,ReversalGear],
        initialPartCount:6, initialArmMoves:2, initialPublicRepairEnergy:0,
        fixedElements:[
            FixedElementConfig(Obstacle,(-1,0),FixedWall),       // W 传动箱外壳
            FixedElementConfig(MovableMarker,(1,0),goal:GoalConfig("gate",PushedInto,0,GateLabel)), // B
            FixedElementConfig(EndpointLabel,(2,0),label:GateLabel)], // G 校正格
        objective:"把门轴联系标记推入校正格，让卷帘门重新卡回轨道", duration:2)
    IsGatePushedIn(board) / FindGateMarker(board)
}
```

- 参考路线（D3 左倾）：机械臂把 B 从 (1,0) 搬到 (2,0)=G（不计完成）→ 拍击，B 沿 D3 落出 G 撞 H(0,0) → 撞锤沿入射反方向（D0 右）把 B 反推回 (2,0)=G → 锁定。成局判定 = 累计分 ≥30 且 `IsGatePushedIn`。

## 5. 固定元素扩展（D-133/D-136）

- `FixedElementKind` 增 `MovableMarker=3`、`EndpointLabel=4`。
- `FixedElementConfig` 增 `Goal`；`CreateEntity` 处理 `MovableMarker` → `MovableTaskMarker(id, Goal)`。
- `InitialBoardBuilder.Build`：`EndpointLabel` 走 `SetEndpointLabel`（不占实体容量、不占 id）；随机落位的空实体格排除端点标签格（`!board.TryGetEndpointLabel(coord, out _)`）。

## 6. 中断恢复扩展（D-135）

- `SavedEntity` 增 `IsLocked/Movable/AnchorLabel`；`RepairSaveService.Capture` 写这三项；`FromSaved` 对 TaskMarker 按 `Movable` 走 `MovableTaskMarker`（AnchorLabel>0 重建 `GoalConfig("gate",PushedInto, saved.Id, AnchorLabel)`），`RestoreState` 后 `SetLocked(saved.IsLocked)`。
- 红舞鞋 `RedShoeState` 运行时状态存盘留 b19（本批只保证卷帘门门轴标记的锁定/可移动/锚点恢复）。

## 已知限制

- 卷帘门 B 的 GoalConfig.EntityId 取 0（任意实体到格），锁定判定不依赖 EntityId；一般化目标（Order/Hold 的存盘）留后续批次。
