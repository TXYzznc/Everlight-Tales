# b03 派发单：存档＋随机＋配置字段＋诊断

| 项 | 内容 |
|---|---|
| 派发 ID | `b03-p09-p12-save-random-diagnostics` |
| 覆盖任务 | P0-009、P0-010、P0-011、P0-012 |
| 里程碑 | G0 工程基线 |
| 前置批次 | b01（已完成，提交 `f0dbb06`） |
| 并行能力 | 与 b02 写域不重叠，可并行（计划第 64 行口径） |
| 预计工时 | 14～28 小时 |
| 状态 | 方案待确认 |

## 覆盖任务原文（据《第一版开发任务表》，只读引用）

| 任务ID | 模块 | 任务名称 | 前置 | 完成定义(DoD) |
|---|---|---|---|---|
| P0-009 | 存档底层 | 存档服务骨架 | P0-002 | 双存档接口与版本号就绪，读写可测 |
| P0-010 | 随机 | 可配置随机源 | P0-002 | 同种子同序列，随机消费可计数 |
| P0-011 | 配置管线 | 配置表字段冻结 | **P0-005（b02）** | 首批字段清单落文档 |
| P0-012 | 诊断 | 诊断与盘面演算日志 | P0-002 | 日志可开关，演算可复现 |

**依赖差异**：P0-009／P0-010／P0-012 只依赖 b01，可立即开工；**P0-011 依赖 b02 的 P0-005**。因此 b03 与 b02 虽可并行，但 b03 的最后一项任务需要 b02 的表骨架作为登记对象。

## 框架侧已有设施（已实地清点）

| 设施 | 位置 | 用途 |
|---|---|---|
| `NewtonsoftJsonHelper` | `ScriptsBuiltin/Runtime/GFHelper/` | 存档序列化，不引入新依赖 |
| `DefaultSettingHelper` | `Plugins/UnityGameFramework/.../Utility/` | GF 自身的落盘范式：`persistentDataPath` + JSON |
| `GFDiagnosticScenario` / `GFDiagnosticReport` | `ScriptsBuiltin/Runtime/Diagnostics/` | 诊断以 scenario 产出报告，直接沿用 |

## 写域边界

**可写**

- `Assets/Game/Scripts/EverlightTales/Meta/`（存档服务、诊断场景）
- `Assets/Game/Scripts/EverlightTales/Board/`（随机源、演算日志开关 —— **必须保持零引擎依赖**）
- `Docs/GameDesign/07-设计参考/`（配置字段草案）
- `Docs/Framework/Development/Dispatch/`、`openspec/`

**只读**

- `Assets/Game/ScriptsBuiltin/**`（框架核心）
- `Docs/GameDesign/10-开发计划/第一版开发任务表.xlsx`（用户维护）
- b02 的写域：`GameData/DataTables/`、`Assets/Game/DataTable/`、`Assets/Game/Scripts/DataTable/`、UI 与音频相关文件

**禁写**

- 任何业务代码进入 `Assets/Game/ScriptsBuiltin/`
- 运行时存档文件进入版本库

## 待确认决策（方案门禁）

| # | 决策点 | 建议 |
|---|---|---|
| D1 | 双存档组织 | 世界存档与尝试存档为**两个独立文件**，互不阻塞；各自带版本号。合成单文件会让高频覆写的尝试存档威胁世界进度 |
| D2 | 序列化与落盘 | 复用框架 JSON 工具 + `persistentDataPath`（沿用 `DefaultSettingHelper` 范式）；三段信封 `{version, savedAtUtc, payload}`；**不加密不压缩**。版本高于支持范围时拒绝且不静默降级 |
| D3 | 随机源结构 | 固定种子默认 + 可插拔来源（固定／固定＋随机混合）；**消费计数在服务内部集中记账**，供同种子复现与批量演算校准直接读取 |
| D4 | 随机源放哪一层 | 放 **`Board`**（零引擎）。理由：同种子复现必须在无引擎环境可验证，放 `Meta` 会让盘面演算被迫依赖带引擎程序集，破坏 b01 刻意建立的离线演算能力 |
| D5 | 配置字段登记口径 | 登记四类表草案 + **标注每类字段的最终归属任务**（关卡类明确归属 `P1-021`），文档开头声明"草案非冻结"。理由：P0-011 与 P1-021 存在真实口径重叠，硬在 G0"冻结"关卡字段会与 G1 冲突 |
| D6 | 诊断与演算日志 | 诊断沿用框架 `GFDiagnosticScenario` 模式；演算日志开关放 `Board`，关闭时**零开销**，开启时记录种子／初始盘面／每步动作与随机消费数，使"同一记录可重放出同一结果"可被断言 |

## 验收证据计划

| 项 | 命令／方式 |
|---|---|
| 框架纯度 | `python tools/audit_framework_purity.py` |
| 程序集布局 | `python tools/verify_project_assemblies.py`（重点确认 `Board` 仍零引擎） |
| 存档往返 | 写入→读回等价、重复往返稳定、损坏可辨识、版本不符被拒绝 |
| 随机确定性 | 同种子同序列、不同种子不同序列、计数正确、重置归零 |
| 演算日志 | 关闭零记录、开启记录完整、按记录重放结果一致 |
| Unity 编译 | `-batchmode -nographics -executeMethod`（无头探针，验证后删除） |

## 未决与边界

- 存档槽位与多档位：任务表未要求；`G5` 新档自走若需要，届时扩展文件名约定。
- 世界存档的具体写入时机集合：由 `G3` 世界状态任务定义，本批次只提供接口。
- 演算日志是否落文件供跨进程比对：`P2-017` 批量演算校准时确认；本批次先做内存态 + 可导出。
- `P0-011` 的字段稳定性弱于任务表字面（只登记草案），这是与 `P1-021` 共存的必要妥协，已显式记录。
