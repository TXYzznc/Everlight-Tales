## Context

`Assets/Game/Scripts/` 当前承载框架侧启动链（`PreloadProcedure`、`FrameworkReadyProcedure`、`ChangeSceneProcedure`）与热更入口（`HotfixEntry`），全部属于 `Hotfix.asmdef`。`Assets/Game/ScriptsBuiltin/` 是框架核心 `Builtin.Runtime`。业务侧尚不存在：`Assets/Game/Scripts/EverlightTales/` 为空目录。

本批次要立住的地基有两条不可逆边界：

1. **业务代码的物理边界**：后续 108 项程序任务必须落在一个明确的程序集分层中，且永不渗入 `ScriptsBuiltin`。
2. **输入的抽象边界**：移动端触摸、编辑器鼠标与自动化验证必须共用同一入口，业务代码不得直读 `UnityEngine.Input`。

## Goals / Non-Goals

**Goals**

- 五个业务程序集可编译、引用方向单向、`Board` 零引擎依赖。
- `Launch` 启动后经既有框架链进入业务 Procedure 并加载空业务场景，无报错。
- 输入事件经 `InputModule` 统一暴露，触摸与编辑器鼠标两条 Provider 均可触发可观测事件。
- 框架纯度审计在新增业务程序集后仍然通过。

**Non-Goals**

- 不实现任何玩法、盘面、表加载、UI 页面、存档或音频接入（分别在 `b02`～`b03` 与 G1 起）。
- 不决定业务程序集在 HybridCLR 热更包中的归属（`P6-010`）。
- 不引入新第三方依赖，不升级 Unity 版本。
- 不修改 `ScriptsBuiltin` 与既有框架 Procedure 顺序。

## Decisions

### D1：业务程序集拆分为 5 个 asmdef（已确认）

| 程序集 | 目录 | 职责 | 引用 |
|---|---|---|---|
| `Everlight.Tales.Data` | `Data/` | 表结构、配置 DTO、常量、枚举 | `GameFramework` |
| `Everlight.Tales.Board` | `Board/` | 六相定势盘模拟器（纯逻辑） | `Everlight.Tales.Data` |
| `Everlight.Tales.Meta` | `Meta/` | 世界状态、时间、存档、事件／案件、成长服务 | `Everlight.Tales.Data` |
| `Everlight.Tales.Events` | `Events/` | 盘面事件总线、能力事件队列、Buff 与触发器 | `Everlight.Tales.Data`、`Everlight.Tales.Board` |
| `Everlight.Tales.UI` | `UI/` | 全部 UIForm／UIItem 与业务页面 | 以上四个 + 框架 UI／TMP／UniTask／DOTween |

选择理由：`Board` 零引擎依赖可在无引擎环境批量演算，直接服务 P1-012 与 P2-017 的验收；引用方向单向（Data ← 其余全部，Board ← Events/UI/Meta）避免循环依赖；按层分包让编译增量与后续拆分成本可控。

替代方案「单业务 asmdef + 子目录分层」起步更快，但会让盘面逻辑与引擎耦合，后续离线演算与单元验证需要二次拆分，且无法用编译期约束阻止 `Board` 意外依赖引擎类型。已确认放弃。

### D2：业务程序集独立于 Hotfix（已确认）

业务程序集**不合并进** `Hotfix.asmdef`。`Hotfix` 继续承担热更程序集角色；`FrameworkReadyProcedure` 与 `HotfixEntry` 都已通过 `Type.GetType` + `AppDomain.CurrentDomain.GetAssemblies()` 按类型名解析 Procedure，因此业务 Procedure 位于哪个程序集不影响解析。业务程序集归属主包还是热更 DLL 属于构建链决策，留给 `P6-010`。

### D3：输入抽象采用项目侧 Provider + InputModule（已确认）

- `IInputProvider`：定义 `Poll(float deltaTime)` 与 `TryDequeue(out InputEvent)`，产出点击（按下／抬起）、拖动（位移）、长按、按钮语义事件。
- `TouchInputProvider`：默认实现，读取触摸屏。
- `EditorMouseInputProvider`：编辑器回退实现，读取鼠标与键盘。
- `InputModule`：静态入口，持有当前 Provider，向业务暴露 `Update` 与事件消费接口；业务只允许引用 `InputModule` 与 `InputEvent`。

选择理由：满足"业务不直读 `UnityEngine.Input`"的硬约束，同时让真机与编辑器调试走同一条可自动化验证的路径；Provider 可替换也为后续自动化测试（注入脚本化输入）留出入口。

替代方案「包装框架既有输入能力」被放弃：框架当前没有提供面向业务的触摸点击／拖动抽象，包装仍需自建 Provider，反而多一层间接。

### D4：业务空场景位置与命名（已确认）

空业务场景放在 `Assets/Game/Scene/`，命名 `Home`（`Assets/Game/Scene/Home.unity`），与既有 `Launch.unity` 同级。理由：`UtilityBuiltin.AssetsPath.GetScenePath()` 以场景目录为根，`ChangeSceneProcedure` 已按该约定加载场景，业务场景沿用同一约定即可被既有加载链直接寻址。

### D5：P0-001 范围冻结的落点（已确认）

不新建范围说明文档副本。范围与里程碑口径由用户维护的任务表工作簿定义，AI 侧只在《分批开发执行计划》中记录分批归档；`P0-001` 的验收标准是"口径确认一致"，不产生新的权威载体。理由：任务表唯一权威原则（见《开发执行约束》"第一版开发任务表所有权"）禁止创建并行的权威副本。

## Risks / Trade-offs

- **风险**：`Board` 的零引擎约束在后续被无意破坏（例如为了 Vector2 而引用 `UnityEngine`）。→ **对策**：`noEngineReferences: true` 让引擎类型在编译期即不可用；批次验证固定检查该设置存在。
- **风险**：`AppConfigs.asset` 直接编辑 YAML 出错会破坏启动链。→ **对策**：优先经 Unity Editor 写入；直接编辑时只改 `mProcedures` 纯文本数组并即时做启动走查。
- **风险**：编辑器鼠标回退实现被误用于真机。→ **对策**：Provider 选择用条件编译（`UNITY_EDITOR` 优先编辑器实现，其余平台触摸实现），并在 spec 中写成可测试场景。
- **权衡**：5 个程序集比 1 个程序集多出跨程序集引用声明成本，换来盘面逻辑的引擎解耦与编译期边界约束；判定为值得。

## Migration Plan

无迁移：业务侧此前不存在，本批次是纯新增。回滚方式为删除 `Assets/Game/Scripts/EverlightTales/` 与 `Home.unity` 并还原 `AppConfigs.asset` 的 Procedures 列表。

## Open Questions

- 业务程序集编译进主包还是热更 DLL：`P6-010` 决定，本批次两种都能工作。
