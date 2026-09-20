## Why

G0 剩下的四项任务不构成一条功能链，但共享同一个性质：**它们都是后续所有涉及"状态"的功能必须提前定型的底座**，而且定型之后很难改。

- **存档**（P0-009）：`P2-010` 的维修尝试存档、`G3` 的世界存档恢复、`P6` 的新档自走全部建立在双存档文件格式与版本号之上。存档格式一旦被大量读写点依赖，改版本号就是全量迁移。
- **随机**（P0-010）：`P1-012` 的同种子复现与 `P2-017` 的批量演算校准是**可复现性验收**，它们要求"同种子同序列"并可计数随机消费次数。随机源若散落在各处直接调用 `UnityEngine.Random`，这条验收永远拿不到。
- **配置字段冻结**（P0-011）：`P1` 与 `P2` 共约 20 项任务读配置表，字段口径不先落文档就会各自发明列名。
- **诊断与演算日志**（P0-012）：盘面演算的验证（b09 起）依赖可复现的演算日志，而不是靠人眼看画面。

四项任务的前置是 `P0-002`（b01 已完成的业务程序集），因此本批次**与 b02 写域不重叠、可并行**（计划第 64 行原文口径）。唯一的交叉是 `P0-011` 需要 `P0-005` 的表骨架作为登记对象。

## What Changes

- 新增存档服务骨架：世界存档与尝试存档**双文件**、独立版本号、读写时机接口；序列化与落盘方式沿用框架既有约定（框架自带 `NewtonsoftJsonHelper`，GF 自身的 `DefaultSettingHelper` 已示范 `persistentDataPath` + JSON 的落盘范式）。
- 新增可配置随机源：关卡固定种子与随机混合的统一入口，**记录随机消费次数**；提供"完全固定"与"完全随机"两个可插拔来源，使同种子同序列可被验证，也使"随机"本身可被测试替代。
- 新增配置字段草案登记：事件模板／关卡／零件／Buff 四类表的字段清单落文档，明确各项字段的归属任务（关卡表列的最终归属是 `P1-021`，本批次只登记草案）。
- 新增诊断与盘面演算日志开关：接入框架既有诊断机制（`GFDiagnosticScenario`／`GFDiagnosticReport` 运行期模式），并提供供关卡验证使用的演算日志开关与可复现记录。

## Capabilities

### New Capabilities

- `project-save-service`: 定义世界存档与尝试存档的双文件契约、版本号策略、序列化与落盘位置、读写时机接口，以及"损坏或版本不符时的处理"。
- `project-random-source`: 定义固定种子、随机混合与随机消费计数的统一入口，以及"同种子同序列"的可验证边界。
- `project-config-field-registry`: 定义首批配置表字段草案的登记位置与字段归属口径。
- `project-diagnostics-logging`: 定义诊断接入方式、演算日志开关与可复现记录契约。

### Modified Capabilities

无。本批次只新增业务侧能力，不修改任何既有框架规格。

## Impact

- 新增：`Assets/Game/Scripts/EverlightTales/Meta/` 下的存档服务、随机源、诊断接入（`Meta` 已引用 `Builtin.Runtime`，可直接使用框架工具）。
- 新增：`Assets/Game/Scripts/EverlightTales/Board/` 下的演算日志开关（保持 **Board 零引擎依赖**，日志只依赖 `System`）。
- 新增：配置字段草案文档（落在 `Docs/GameDesign/07-设计参考/` 口径文档，不新建权威副本）。
- 运行时产物：存档文件写入 `Application.persistentDataPath`，**不进版本库**。
- 不影响：`Assets/Game/ScriptsBuiltin/**`、既有框架 Procedure 顺序、b02 的写域（表源／UI／音频）。
- 验证入口：`python tools/audit_framework_purity.py`、`python tools/verify_project_assemblies.py`（确认 `Board` 仍是零引擎）、Unity 批处理编译、同种子序列断言、存档读写往返断言。
- 已知依赖：`P0-011` 需要 `P0-005` 的表骨架作为登记对象；若 b02 尚未落地，`P0-011` 只能登记字段草案而不能对照实际表列。
