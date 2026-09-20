# `b01-p01-p04-engineering-baseline`：工程基线（业务程序集、启动链、输入抽象）

> 本文件用于 OpenSpec 建立前的方案探索派发；它不代表实现方案已确认，也不改变项目权威计划。
> 方案确认状态：**已确认**（2026-09-20，用户确认 5 个决策点），派发单随之转为"可实施"。

## 1. 身份信息

| 字段 | 内容 |
|---|---|
| 派发 ID | `b01-p01-p04-engineering-baseline` |
| 管理批次 | `b01` |
| 覆盖工作 | P0-001、P0-002、P0-003、P0-004 |
| 专业窗口 | `client-unity`（主）、`client-lead`（架构边界复核） |
| 协作状态 | 可实施 |
| 制作人升级入口 | 用户本人（当前个人开发，未启用独立制作人窗口） |
| Git 提交入口 | `git-integration`（当前未启用独立窗口，暂由用户本人提交） |

## 2. 目标、范围与权威输入

### 目标与验收

从 `Launch` 进入一个空的业务场景，并建立后续全部程序阶段的工程骨架：业务程序集分层与目录、可替换的项目输入抽象、业务启动 Procedure 注册。

验收以任务表 DoD 为准：

- `P0-001`：任务表范围与 G0～G6/ART0～ART2 里程碑口径确认一致。
- `P0-002`：asmdef 划分清晰，业务不进入 `ScriptsBuiltin`，编译通过。
- `P0-003`：从 `Launch` 进入空业务场景，`AppConfigs` 只注册本 Procedure。
- `P0-004`：所有输入经 `InputModule` 抽象，无 `UnityEngine.Input` 直读。

### 纳入与排除

- 纳入：`Assets/Game/Scripts/EverlightTales/**`（新建）、`Assets/Game/Scene/` 下业务空场景（新建）、`Assets/Game/ScriptableAssets/Core/AppConfigs.asset` 的 Procedure 列表新增项。
- 排除：DataTable 生成管线与首批表骨架（`b02`）、UI 页面壳（`b02`）、GF.Sound 接入（`b02`）、存档骨架（`b03`）、随机源（`b03`）、诊断日志（`b03`）、任何盘面与玩法逻辑（G1 起）。

### 权威计划和输入边界

- 人类/外部权威计划：《第一版开发任务表.xlsx》（用户本人维护，只读）、[开发执行约束](../../../../GameDesign/10-开发计划/开发执行约束.md)、[第一版开发路线总览](../../../../GameDesign/10-开发计划/第一版开发路线总览.md)。
- AI 可编辑输入与交付物：本派发单、`openspec/changes/b01-p01-p04-engineering-baseline/**`、上述纳入路径。
- 不重新开放的已确认结论：D-088（六边形网格四档位）；框架启动链 `Launch → PreloadProcedure → FrameworkReadyProcedure → 业务 Procedure` 的既有顺序。

## 3. 启动与讨论检查

- [x] 阅读项目入口、角色路由、对应 agent 配置、SKILL 与 OpenSpec 工作流。
- [x] 检查目标项目、依赖、编译/诊断和外部工具状态：`python tools/audit_framework_purity.py` 基线通过。
- [x] 核对目录/文件与工具占用：`Assets/Game/Scripts/EverlightTales/` 为空目录，无其他窗口占用；Git 索引当前无暂存内容。
- [x] 与用户完成关键方案讨论；决策点 D1～D5 已确认并增量归档。
- [x] 在用户确认实现细节和架构后，创建或填写 OpenSpec artifacts。

## 4. 修改权限与占用

### 可写与只读范围

- 可写：`Assets/Game/Scripts/EverlightTales/`、`Assets/Game/Scene/`（仅新增业务空场景）、`Assets/Game/ScriptableAssets/Core/AppConfigs.asset`（仅 Procedures 列表）、`openspec/changes/b01-p01-p04-engineering-baseline/`、本派发单。
- 高冲突单文件：`Assets/Game/ScriptableAssets/Core/AppConfigs.asset`。
- 只读：`Docs/GameDesign/**`、`第一版开发任务表.xlsx`、`Assets/Game/ScriptsBuiltin/**`、既有框架 Procedure 顺序。
- 禁止修改：`Assets/Game/ScriptsBuiltin/**`、任务表工作簿、框架既有 Procedure 与 `Hotfix.asmdef`。

### 工具和 Git 集成

| 资源 | 实例/文件 | 占用者 | 释放条件 |
|---|---|---|---|
| Unity Editor | 2022.3.62f3（主工程 `Everlight-Tales`，Unity Skills 端口 8090 起） | 本批次 | 编译与场景验证完成 |

- 专业窗口不得暂存或提交；所有请求发送给 Git 集成窗口并遵守 [GitIntegration.md](../GitIntegration.md)。
- 请求必须提供派发/OpenSpec ID、精确路径、验证证据、中文提交说明、排除项、资源状态和授权依据。
- Git 集成窗口只能显式暂存请求路径，禁止夹带其他窗口或用户改动、推送和改写历史。

## 5. 实施结果

### 最终程序集引用图（与 `design.md` D1 的差异已登记）

| 程序集 | 引擎 | 引用 |
|---|---|---|
| `Everlight.Tales.Data` | 零引擎 | `GameFramework` |
| `Everlight.Tales.Board` | 零引擎 | `Everlight.Tales.Data` |
| `Everlight.Tales.Events` | 零引擎 | `GameFramework`、`Everlight.Tales.Data`、`Everlight.Tales.Board` |
| `Everlight.Tales.Meta` | 引擎侧 | `GameFramework`、`UnityGameFramework.Runtime`、`Builtin.Runtime`、`Everlight.Tales.Data` |
| `Everlight.Tales.UI` | 引擎侧 | 上述四个 + `GameFramework`、`UnityGameFramework.Runtime`、`UniTask`、`DOTween.Extension` |

**偏差登记**：`Meta` 额外引用 `Builtin.Runtime`，用于取框架诊断日志（`GFTrace`）；`Events` 额外引用 `GameFramework`，为后续事件总线沿用框架事件类型留出空间。两处都保持引用方向单向，不构成环。

### 交付物

- `Assets/Game/Scripts/EverlightTales/`：5 个 asmdef、各层锚点与 README、`Board` 层的 `HexCoord`／`HexDirection`／`HexGrid` 纯逻辑契约、`Events` 层的队列与触发契约、`Meta` 层的输入模块与业务启动 Procedure。
- `Assets/Game/Scene/Home.unity`：无 GameObject 的空业务场景。
- `Assets/Game/ScriptableAssets/Core/AppConfigs.asset`：Procedures 列表新增 `Everlight.Tales.Procedure.EverlightTalesProcedure`，既有三项与顺序不变。
- `tools/unity_meta.py`：补齐新增资源 `.meta` 的确定性工具（只创建缺失项，不覆盖既有 GUID）。
- `tools/verify_project_assemblies.py`：校验五层命名、引用方向、引用可解析性、无环与零引擎约束。

### 验证证据

| 项 | 命令 | 结果 |
|---|---|---|
| 框架纯度 | `python tools/audit_framework_purity.py` | `[OK] framework purity audit passed` |
| 程序集布局 | `python tools/verify_project_assemblies.py` | `[OK] ... 5 business assemblies, acyclic, layout confirmed` |
| 输入边界 | ripgrep `UnityEngine\.Input`（`Assets/Game/Scripts/EverlightTales/**/*.cs`） | 仅 2 个 Provider 实现（8 处）与 1 处文档引用；业务其余层 0 命中 |
| meta 完整性 | `python tools/unity_meta.py --check` | `[OK] meta files missing: 0` |
| OpenSpec | `openspec validate b01-p01-p04-engineering-baseline --strict` | `Change ... is valid` |
| **Unity 编译** | `Unity.exe -batchmode -nographics -executeMethod DSHCompileCheck.Run` | **退出码 0**，`scriptCompilationFailed=false`，五个业务程序集全部 `loaded=true` |
| **注册与场景** | 同上（探针顺带断言） | `appConfigsProcedure=true`（业务 Procedure 已在 AppConfigs 注册）、`entryScene=true`（`Assets/Game/Scene/Home.unity` 存在） |
| **输入与几何契约** | `Unity.exe -batchmode -nographics -executeMethod DSHInputContractCheck.Run` | **退出码 0**，7/7 场景通过 |

### 编译过程中发现并修复的三个真实缺陷

编译不是走形式，第一次跑就抓出三个静态检查发现不了的问题，已全部修复：

1. `Meta` 缺少 `Obfuz.Runtime` 引用 → `ObfuzIgnore` 不可用。
2. `Meta` 缺少 `Hotfix` 引用 → `IFrameworkStartupProcedure` 不可见（该接口定义在 `Hotfix.asmdef`）。
3. `ChangeSceneProcedure.P_SceneName` 是 `internal`，跨程序集不可见 → 改为按框架既有约定直接使用 `"SceneName"` 字符串键，不为此扩大可见性。

### 无头契约检查覆盖的 7 个场景

按 `specs/project-input-abstraction/spec.md` 的 Scenario 逐条验证，全部通过：

| 场景 | 对应 spec 条目 |
|---|---|
| 一次点击产出恰好一个按下与一个抬起 | 输入事件可观测且可消费 |
| 拖动事件携带位移量 | 输入事件可观测且可消费 |
| 长按在阈值后只触发一次 | 输入事件可观测且可消费 |
| 已取出的事件不重复返回 | 队列不被重复消费 |
| 替换来源清空已缓冲事件并释放旧来源 | 替换 Provider |
| 替换后事件只来自新来源 | 替换 Provider |
| 零引擎 Board 层几何正确（n=5→61、n=10→271、合法格与相邻距离） | 盘面程序集零引擎依赖 + 数据正确性 |

> **残留的唯一人工项**：完整 `Launch → Preload → FrameworkReady → EverlightTales → Home` 的 Play Mode 视觉走查（需要真实资源热更链与人工观察）。该项留到批次验收时由用户在 Editor 中确认；已由编译探针确认注册与场景存在、由契约检查确认输入与盘面契约，因此不阻塞后续批次。
>
> 验证用的探针程序集（`Assets/DSHVerification/`）与两个 `-executeMethod` 入口是**一次性**验证工具，验证完成后即删除，不进入交付物。

## 6. 回传、暂停与恢复

整个派发批次完成时，专业窗口向制作人回传决策/确认状态、OpenSpec 阶段、修改范围、验证与人工验收入口、全部提交号、限制、未决项、权威计划手动更新建议和已释放/仍持有的资源。

普通进度、单项用户决定、测试通过、两轮增量归档和独立提交不向制作人回传；跨职能影响由本窗口点对点同步受影响窗口，Git 结果只回传请求窗口。

发生越界需求、公共契约改变、点对点无法解决的占用冲突、工具故障、扩大 OpenSpec 的验收失败，或命中强制升级事件时，停止相关工作、保留现场并按 [PauseAndRecovery.md](../PauseAndRecovery.md) 向制作人请求动作。

## 7. 本批次已知未决项

- **Unity 编译验证未完成**：编辑器当前持有 `Everlight-Tales` 的工程锁（`Temp/UnityLockfile` 存在），批处理模式无法打开同一工程。需要用户在 Editor 中确认 Console 无编译错误，或关闭 Editor 后由 AI 跑一次批处理编译检查并走查启动链。
- 业务程序集在 HybridCLR 热更包中的最终归属（编译进主包 vs 进热更 DLL）属于构建链决策，`P6-010` 构建链路批次再定型；b01 只保证类型可被 `Type.GetType` 解析。
