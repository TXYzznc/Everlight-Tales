# Everlight.Tales.Events

事件与触发器层。程序集：`Everlight.Tales.Events`（`noEngineReferences: true`）。

## 职责

- 盘面事件总线与 FIFO 能力事件队列契约。
- 触发语义与即时结果契约（碰撞、到达、保持、顺序）。
- Buff 与每拍四选一的抽取与生效框架。

## 边界

- 可引用：`Everlight.Tales.Data`、`Everlight.Tales.Board`。
- 禁止引用：Unity 引擎程序集、`Meta`／`UI`。
- 结算与播放分离：本层只产出可复现的即时结果序列，播放节奏由 UI 层消费。

## 后续任务

| 任务 | 内容 |
|---|---|
| P1-005 | 碰撞与触发语义 |
| P1-006 | FIFO 能力事件队列 |
| P1-013 | 第一步预演 |
| P1-018／P1-019 | 四选一框架与 Buff 数据结构 |
| P2-004 | 任务标记与锚点映射 |
