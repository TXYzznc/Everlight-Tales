# 设计：六相旋转与定势 + 拍击结算事务（b06）

## 决策记录

### D-094：六相旋转用 `SettleState` 承载，重力方向 = 对准定势座的相位边

六相定势盘的六条相位边一一对应六方向 D0~D5。`SettleState` 持有一个 `GravityDirection`：对准底部定势座的那条边即最低势位，也就是重力方向（SR-005／D-037）。左旋／右旋各转一个相位并吸附（`HexDirections.Rotate(±1)`）；旋转只改重力方向，不移动棋子、不触发能力、不累计代价（D-078）。

| 操作 | 语义 |
|---|---|
| `RotateLeft()` | 重力方向相位 −1 并吸附 |
| `RotateRight()` | 重力方向相位 +1 并吸附 |
| `SetGravity(d)` | 直接设定（配置／恢复） |

左旋＝−1、右旋＝+1 是数据层约定；可视化方向映射（左旋＝逆时针等）由盘面表现层（b10）落地。

### D-095：拍击结算事务用 `TapSettlement` 编排，演算与播放分离

`TapSettlement.Settle(board, settle, context)` 严格按 SR-002 顺序结算：

1. 扣拍数（`TapQuota - 1`）。
2. 到期效果（`TapContext.DueEffects`，先于基础定势）。
3. 基础定势下落 + 碰撞 + FIFO 排空 + 重力接续，直到稳定。
4. 拍末效果（`TapContext.TapEndEffects`，逐项执行、每项后接重力稳定）。
5. 稳定。

结算只产出 `SettlementResult`（确定性 `SettlementEvent` 日志 + 剩余拍击额度），不直接驱动表现；表现层按日志回放。这与「风险与对策」第 7 节「b06 定义时就分离演算结果与播放时间轴」一致。

### D-096：重力接续（D-067）用批式「下落 → 排空队列 → 重查」循环实现

`SettleToStable` 反复执行「`FallAll`（沿重力让全部可移动实体滑至边缘或首次碰撞）→ `DrainAll`（FIFO 排空事件队列）」，直到既无实体移动、队列也为空。队列排空后重查可下落实体，即同拍重力持续接续（D-067），不消耗额外拍数。

- 排序：每次处理可行动实体时，以实时位置按当前重力「前到后」排序（轴向投影大者在前），投影相同按 q、r、ID 升序（SR-002）。
- 碰撞规则：实体**移动至少一步后**受阻才算新碰撞并触发；静止贴着同一阻挡不因重查队列重复触发（SR-002）。`CollisionTriggerEvent` 只记录来源／目标，能力由后续批次接入。

### D-097：`BoardState` 增加实体枚举与移除

为支撑下落扫描与回收／开门／销毁（重力接续的来源之一），`BoardState` 新增：

- `Entities`：在盘实体实时视图（无序，遍历前先快照）。
- `Remove(entity)`／`RemoveAt(coord)`：移除实体。

## 类型清单

| 类型 | 文件 | 状态 |
|---|---|---|
| `SettleState` | Board/SettleState.cs | 新增 |
| `SettlementEventKind` + `SettlementEvent` | Board/SettlementEvent.cs | 新增 |
| `ISettlementEvent` | Board/ISettlementEvent.cs | 新增 |
| `SettlementEventQueue` | Board/SettlementEventQueue.cs | 新增 |
| `CollisionTriggerEvent` | Board/CollisionTriggerEvent.cs | 新增 |
| `TapContext` | Board/TapContext.cs | 新增 |
| `SettlementResult` | Board/SettlementResult.cs | 新增 |
| `TapSettlement` | Board/TapSettlement.cs | 新增 |
| `BoardState` | Board/BoardState.cs | 扩展：`Entities`／`Remove`／`RemoveAt` |

## 边界与后续

- **碰撞触发为占位**：b06 只记录来源／目标，不解释能力；「碰撞与触发语义」（单次触发、推落回新触发、原子事件）由 P1-005、FIFO 能力事件队列契约（`Everlight.Tales.Events.IAbilityEventQueue`）由 P1-006 接入（b07）。
- **到期／拍末效果为插槽**：b06 通过 `ISettlementEvent` 提供插槽，具体效果（零件能力、轮规则）由 P1-007 起与关卡规则接入。
- Board 层不引用 Events 层（依赖方向 Events → Board），事务自持 FIFO；b07 将把事务的碰撞触发映射到 Events 层的 `IAbilityEventQueue`。
