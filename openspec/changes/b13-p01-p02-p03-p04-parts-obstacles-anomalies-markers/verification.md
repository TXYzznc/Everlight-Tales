# 验证记录（b13 盘面内容件批量接入）

覆盖任务：P2-001、P2-002、P2-003、P2-004。

## 验证环境

- Unity 2022.3.62f3（Editor 常驻，UnitySkills REST `http://localhost:8090`，`projectName=GameDesinger`，`currentMode=bypass`）
- 验收方式：UnitySkills Play Mode 综合探针（`B13AcceptanceRunner`，验收后已删除）+ Python 静态检查

## 静态检查

| 检查 | 命令 | 结果 |
|---|---|---|
| 程序集布局 | `python tools/verify_project_assemblies.py` | ✅ 5 程序集无环；Data/Board/Events 零引擎 |
| 框架纯度 | `python tools/audit_framework_purity.py` | ✅ 通过 |

## Unity 编译

- 探针删除后 `debug_force_recompile` → `success=true, errorCount=0, warningCount=0`。

## Play Mode 验收（探针日志 `[B13ACCEPT]`）

`SUMMARY passed=38 failed=0`：

| 任务 | 断言 | 结果 |
|---|---|---|
| P2-001 换向齿轮 | 能量4→3、偏转后滑到(4,-1)、12分 | ✅ |
| P2-001 铆合钳 | 完成一次维修、24分、公共能量2→0 | ✅ |
| P2-002 障碍 | O-001~O-008 固定/耐久/闸门/轨道/夹持/百叶/封条/支柱各自行为 | ✅ |
| P2-003 异常框架 | A-001/A-002/A-004/A-005/A-006/A-008 注册与触发 | ✅ |
| P2-004 特殊目标 | 到达/保持/顺序判定可配置 | ✅ |

## 设计说明

- D-118：零件（换向齿轮/铆合钳）+ 障碍（8 种）配置化，耐久障碍在碰撞/爆破冲击扣耐久。
- D-119：棋盘异常 `AnomalyRegion` 注册 + `AnomalyService` 触发解释框架。
- D-120：特殊目标 `GoalConfig`/`GoalEvaluator`/`AnchorGoalState` 到达/保持/顺序判定。

## 边界（本批有意留出）

- 地形层障碍与实体共存的真正通道、夹持/轨道/闸门与结算管线深度联动（下落改向/待关闭落闸/封条拍末挂钩）留 b15/b16 一局链路接入。
- O-008 附属隔板拆除、A-003/A-007（文档待补）仅登记类型。

## 结论

b13 通过：两零件、八障碍、异常框架、特殊目标判定全部验收通过，程序集布局与框架纯度审计通过。可进入 b14（Buff库+M类）。
