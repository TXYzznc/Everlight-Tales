# 设计：首批零件 + 公共维修能量与维修对象 + 计分体系（b08）

## 决策记录

### D-100：零件配置在 Data，能力行为在 Board

- 数值（触发分、能量容量、单次消耗、效果分）作为**配置**落在 Data 层 `PartCatalog`（`PartConfig` + `PartType`），后续校准只改配置。
- 能力**行为**（撞锤推移、棘轮产能、线圈爆破）落在 Board 层 `PartAbility`，因为它要改变 `BoardState` 并在结算事务内执行，而 Board 不能引用 Events。
- 首批三件数值：撞锤 4 触发分/2 容量/每格 8；棘轮 10 触发分/5 容量/每成功产 1 公共能量/本拍第 3 次起各 +5；线圈 6 触发分/1 容量/每命中对象 6/起爆移除本体。

### D-101：触发统一为 `PartTriggerEvent`（collision/shock）

- 碰撞触发 `TriggerKinds.Collision`：带入射方向（来撞实体→被撞零件），撞锤据此反向推移；冲击触发 `TriggerKinds.Shock`：无方向，撞锤不推移。
- 结算事务在 `FallAll` 产出 `Collision` 事实时，入队 `PartTriggerEvent(collision, 移动方, 被撞方, 重力方向)`；线圈起爆后向六邻格零件入队 `PartTriggerEvent(shock, 线圈, 邻件)`。
- 触发分与效果成功无关、不消耗能量；自身效果按实时零件能量检查并消耗（SR-003/SR-006）。

### D-102：能力事件改为纯 FIFO，去掉按 `(SourceId, Kind)` 去重；`IBoardEvent` 增加 `TargetId`

- b07 的 `(SourceId, Kind)` 去重会错误合并两类合法场景：①同一线圈冲击多个不同邻件；②撞锤「被推开再落回」产生的同源同目标重发触发（SR-002 明确「可再次触发」）。任何基于 (SourceId, Kind[, TargetId]) 的去重都无法区分「重复」与「合法重发」。
- 改为：结算日志逐条触发都是独立事实，`FifoAbilityEventQueue` 只做 FIFO、不去重；「同一根信号在同一接收件只记一次触发分」的信号级去重推迟到声音／控制信号（b13+）按根事件规则实现。
- `IBoardEvent`／`AbilityEvent` 增加 `TargetId` 承载接收件，供回放与后续信号去重使用。

### D-103：结算日志记录分数/能量增量，Events 层解析实际收益

- `SettlementEvent` 增加 `ScoreDelta`／`EnergyDelta` 字段，并新增 `TriggerResolved`（零件触发）与 `RepairApplied`（有效维修）两种日志类别。
- `PartAbility` 每结算一次触发追加一条 `TriggerResolved`（EntityId=触发源、TargetId=被触发零件、Message=触发类别、ScoreDelta=触发分+效果分、EnergyDelta=公共能量增量）。
- `CollisionTriggerResolver` 改读 `TriggerResolved`（取代 b07 的 `Collision` 事实），产出 `CollisionTrigger`（collision/shock）＋`AbilityEvent`（TargetId 入队）＋`BoardEffect`（实际 ScoreDelta/EnergyDelta）。
- 维修效果走 `RepairEffect`，记 `RepairApplied`；它无「触发源」，不进入触发回放流（分数/能量由 `SessionState` 直接承载）。

## 数据模型

| 层 | 类型 | 文件 | 状态 |
|---|---|---|---|
| Data | `PartType` | Data/PartType.cs | 新增 |
| Data | `PartConfig` + `PartCatalog` | Data/PartConfig.cs | 新增 |
| Data | `RepairTargetConfig` + `RepairRules` | Data/RepairTargetConfig.cs | 新增 |
| Board | `BoardEntity`（+PartType/Energy/RepairConfig） | Board/BoardEntity.cs | 修改 |
| Board | `SessionState` | Board/SessionState.cs | 新增 |
| Board | `TapScoreState` | Board/TapScoreState.cs | 新增 |
| Board | `SettlementContext` | Board/SettlementContext.cs | 新增 |
| Board | `ISettlementEvent`（Apply(SettlementContext)） | Board/ISettlementEvent.cs | 修改 |
| Board | `SettlementEvent`（+ScoreDelta/EnergyDelta/TriggerResolved/RepairApplied） | Board/SettlementEvent.cs | 修改 |
| Board | `PartTriggerEvent` + `TriggerKinds` | Board/PartTriggerEvent.cs | 新增（替换 CollisionTriggerEvent） |
| Board | `PartAbility` | Board/PartAbility.cs | 新增 |
| Board | `RepairEffect` | Board/RepairEffect.cs | 新增 |
| Board | `TapSettlement`（+SessionState 接入） | Board/TapSettlement.cs | 修改 |
| Board | `SettlementResult`（+触发分/效果分） | Board/SettlementResult.cs | 修改 |
| Events | `IBoardEvent`（+TargetId） | Events/IAbilityEventQueue.cs | 修改 |
| Events | `AbilityEvent`（+TargetId） | Events/AbilityEvent.cs | 修改 |
| Events | `FifoAbilityEventQueue`（三元组去重） | Events/FifoAbilityEventQueue.cs | 修改 |
| Events | `CollisionTrigger`（+ShockKind/FromShock） | Events/CollisionTrigger.cs | 修改 |
| Events | `CollisionTriggerResolver`（读 TriggerResolved） | Events/CollisionTriggerResolver.cs | 修改 |

## 边界与后续

- **障碍损伤后置**：线圈冲击对可破坏障碍的损伤（O-002 等）在 b13（P2-002）接入，本批冲击只命中零件。
- **非重力推移后置**：重力-only 结算下撞锤推移总有合法退位格；「无合法落点仍耗能」在非重力推移（后续能力）出现时生效。
- **纯结算化后置**：`TapSettlement` 直接改 `SessionState`/`BoardState`；同种子复现的「演算纯函数 + 结果回放」由 b09（P1-012）收口。
- 维修型零件（铆合钳 P-014）在 b13+ 接入，本批 `RepairEffect` 为机制定义，验收时以探针直接驱动。
