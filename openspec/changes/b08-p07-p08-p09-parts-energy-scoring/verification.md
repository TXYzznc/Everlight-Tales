# 验证记录（b08 首批零件 + 公共维修能量与维修对象 + 计分体系）

覆盖任务：P1-007、P1-008、P1-009。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`，`currentMode=bypass`）
- 验收方式：UnitySkills Play Mode 运行时探针（`B08AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；`Data`/`Board`/`Events` 仍零引擎（Board refs Data；Events refs Data/Board/GameFramework） |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过 |

## Unity 编译

- `debug_force_recompile` → `GET /compile/status` 返回 `success=true, errorCount=0, warningCount=0`。
- 探针脚本删除后重新编译，Play Mode 运行零错误、零警告。

## Play Mode 验收（探针日志 `[B08ACCEPT]`）

探针在 Play Mode 内依次断言，最终 `SUMMARY passed=55 failed=0`：

| 分组 | 覆盖点 | 断言数 |
|---|---|---|
| 撞锤 | 碰撞触发分/推动效果分/耗能/入射反方向推移；冲击只触发分不推移不耗能；缺能碰撞不推移 | 11 |
| 棘轮 | 3 次受击触发分/第 3 次起 +5/耗能/产公共能量；缺能只触发分 | 7 |
| 线圈 | 起爆触发分/命中邻件效果分/移除本体/冲击入队；缺能不移除；双线圈连锁 | 12 |
| 维修 | 有效维修进度+1/效果分20/耗公共能量；不适配不维修；能量不足不维修；完成后不重复 | 12 |
| 计分与解析器 | 解析器读 TriggerResolved 产出触发/即时结果/队列，分数与能量增量正确 | 6 |
| 集成 | 重力碰撞撞锤三次受击耗尽能量、本拍累计 28 分、会话分数累加 | 5 |

## 设计说明

- **零件配置在 Data、能力行为在 Board（D-100）**：`PartCatalog` 承载触发分/容量/消耗/效果分，`PartAbility` 解释撞锤推移、棘轮产能、线圈爆破并改变盘面。
- **触发统一为 `PartTriggerEvent`（D-101）**：碰撞带入射方向、冲击无方向；线圈起爆后向六邻格零件入队冲击形成连锁。
- **能力事件纯 FIFO（D-102）**：去掉按 `(SourceId, Kind)` 去重，避免误合并线圈多目标与撞锤「被推开再落回」合法重发；`IBoardEvent` 增加 `TargetId` 供回放与后续信号去重。
- **日志记录分数/能量（D-103）**：`SettlementEvent` 增加 `ScoreDelta`/`EnergyDelta`，新增 `TriggerResolved`/`RepairApplied`；`CollisionTriggerResolver` 读 `TriggerResolved` 产出实际收益。

## 结论

P1-007、P1-008、P1-009 通过验收：三件零件能力（撞锤推移、棘轮产能、线圈爆破连锁）、公共维修能量与维修对象（适配/进度/能量费用/效果分 20）、计分体系（触发分+自身效果分+维修效果分，跨拍累计）与 SR-006 一致；Data/Board/Events 保持零引擎依赖。
