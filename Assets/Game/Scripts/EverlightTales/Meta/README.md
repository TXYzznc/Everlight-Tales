# Everlight.Tales.Meta

世界与元游戏层。程序集：`Everlight.Tales.Meta`。

## 职责

- 业务启动 Procedure 与业务入口场景衔接。
- 项目输入模块（`IInputProvider` / `InputModule` / 驱动载体）。
- 世界状态、城市地图与时间、事件与案件。
- 世界存档与维修尝试存档、启动恢复。
- 家园、材料、经济与成长服务。

## 边界

- 可引用：`GameFramework`、`UnityGameFramework.Runtime`、`Builtin.Runtime`、`Everlight.Tales.Data`。
- 禁止引用：`Board`／`Events`／`UI`。
- 输入是业务与底层输入之间的唯一边界：本层实现 Provider，其它层只能经 `InputModule` 消费事件。

## 后续任务

| 任务 | 内容 |
|---|---|
| P0-003 | 业务启动 Procedure 接入（本层 `Procedure/EverlightTalesProcedure.cs`） |
| P0-004 | 项目 InputModule（本层 `Input/`） |
| P0-009 | 存档服务骨架 |
| P0-010 | 可配置随机源 |
| P3-001 起 | 事件架构、时间、存档与成长 |
