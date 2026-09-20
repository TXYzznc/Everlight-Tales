# Everlight.Tales.Data

业务数据层。程序集：`Everlight.Tales.Data`（`noEngineReferences: true`）。

## 职责

- 配置表 DTO、表行结构、枚举与常量。
- 跨层共享的分层清单（`ProjectLayers`）。
- 不承载运行时状态，不承载逻辑演算。

## 边界

- 可引用：`GameFramework`（纯逻辑程序集）。
- 禁止引用：Unity 引擎、其余四个业务程序集。
- 禁止放入：玩法逻辑、UI、存档读写、场景或 MonoBehaviour。

## 后续任务

| 任务 | 内容 |
|---|---|
| P0-005 | DataTable 管线与首批表骨架（事件／零件／关卡／Buff／材料／任务／语言） |
| P0-011 | 配置表字段冻结 |
| P1-001 | 蜂窝格棋盘数据结构（轴向坐标与方向定义已在本层之上的 Board 层） |
