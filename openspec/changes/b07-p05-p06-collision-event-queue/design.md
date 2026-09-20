# 设计：碰撞与触发语义 + FIFO 能力事件队列（b07）

## 决策记录

### D-098：能力事件队列用 `FifoAbilityEventQueue` 实现，去重键 `(SourceId, Kind)`

`FifoAbilityEventQueue` 实现 Events 层的 `IAbilityEventQueue` 契约：

- FIFO：`Enqueue` 入队尾，`TryDequeue` 取队首，先入先出、不插队。
- 去重「同对象同事件只处理一次」：同一**触发源**（`IBoardEvent.SourceId`）的同一**事件类别**（`IBoardEvent.Kind`）在一次结算内只入队一次，由 `(SourceId, Kind)` 去重集保证。
- `Clear()` 清空队列与去重集，用于开始新一次拍击结算。
- 顺序可复现：入队顺序由结算日志顺序决定，同种子输入产生相同队列。

能力事件以 `AbilityEvent`（`Sequence` + `SourceId` + `Kind`）承载，`Sequence` 由解析时序单调递增，供复现校验。

### D-099：碰撞触发语义用 `CollisionTriggerResolver` 从结算日志 1:1 映射

`CollisionTriggerResolver.Resolve(SettlementResult)` 读取 Board 层 `TapSettlement` 的确定性结算日志，把每条 `Collision` 事实（`EntityId`=移动方、`TargetId`=被撞方）映射为：

- 一个 `CollisionTrigger`（触发源=移动方 → 目标=被撞方，`Kind="collision"`）。
- 一个 `AbilityEvent` 入队 `FifoAbilityEventQueue`。
- 一个 `BoardEffect` 即时结果（能力接入前 `ScoreDelta`/`EnergyDelta` 均为 0）。

四项触发语义如何落地：

| 语义 | 落点 |
|---|---|
| 新碰撞判定 | Board 层（b06）：实体「移动至少一步后受阻」才产出 `Collision` |
| 单次触发 | 每条 `Collision` 恰好一个 `CollisionTrigger` |
| 静止相邻不重复 | Board 层（b06）：静止相邻不产出 `Collision`，故解析器无重复触发 |
| 推落回新触发 | 被推开再落回是角色反转（A→B 再 B→A），触发源不同，`(SourceId, Kind)` 去重不合并 |
| 原子事件 | 每条碰撞对应一个 `BoardEffect`，先完成对象变化再产出即时结果 |

依赖方向 Events → Board，解析器直接消费 `Everlight.Tales.Board.SettlementResult`，Board 层不改动。

## 类型清单

| 类型 | 文件 | 状态 |
|---|---|---|
| `AbilityEvent` | Events/AbilityEvent.cs | 新增（实现 `IBoardEvent`） |
| `CollisionTrigger` | Events/CollisionTrigger.cs | 新增（实现 `IBoardTrigger`） |
| `BoardEffect` | Events/BoardEffect.cs | 新增（实现 `IBoardEffect`） |
| `FifoAbilityEventQueue` | Events/FifoAbilityEventQueue.cs | 新增（实现 `IAbilityEventQueue`） |
| `CollisionTriggerResolver` + `TriggerResolution` | Events/CollisionTriggerResolver.cs | 新增 |

## 边界与后续

- **能力收益为 0**：`BoardEffect` 的分数与维修能量增量由零件能力（P1-007 起）接入，本批只立结构。
- **去重粒度**：`(SourceId, Kind)` 是契约字面口径；若后续需要「同一移动方连续撞不同目标各触发一次」，由能力条目（b08）为不同目标使用更细的 `Kind` 区分。角色反转（不同触发源）天然不被合并。
- Events 层不引用引擎，`verify_project_assemblies.py` 保持 `Events engine-free refs: Board, Data, GameFramework`。
