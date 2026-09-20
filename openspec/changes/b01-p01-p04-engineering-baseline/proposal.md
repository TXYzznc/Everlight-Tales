## Why

《Everlight-Tales》当前处于框架基线状态：`Assets/Game/Scripts/` 只有框架侧的启动链与热更入口，没有任何业务程序集、业务场景或业务启动 Procedure，`Assets/Game/Scripts/EverlightTales/` 为空目录。任务表 G0 工程准备的第一组任务（P0-001～P0-004）要求在进入盘面开发之前先立住两件不可逆的地基：**业务代码的物理边界**与**输入的抽象边界**。这两项一旦被后续 108 项程序任务绕过，返工成本会随任务量线性上升（业务代码渗入 `ScriptsBuiltin` 会破坏框架升级与纯度审计；直接读 `UnityEngine.Input` 会让移动端触控、编辑器调试与自动化验证同时失去统一入口）。

## What Changes

- 新增业务根目录 `Assets/Game/Scripts/EverlightTales/`，按 `Data` / `Board` / `Meta` / `Events` / `UI` 五个程序集与同名子目录分层，每个程序集独立 `asmdef`。
- `Board` 程序集保持**零引擎依赖**（不引用 `UnityEngine` 与 `noEngineReferences: false`），使盘面模拟器可在无引擎环境离线演算，服务后续 P1-012（同种子复现）与 P2-017（批量演算校准工具）。
- 新增业务启动 Procedure `EverlightTalesProcedure`，实现既有 `IFrameworkStartupProcedure`，在 `AppConfigs` 的 Procedure 列表中注册；`Launch → PreloadProcedure → FrameworkReadyProcedure → EverlightTalesProcedure` 全链打通并进入空业务场景。
- 新增项目输入抽象：`IInputProvider` 契约 + `InputModule` 单例入口 + 触摸默认实现与编辑器鼠标回退实现；业务代码不直接引用 `UnityEngine.Input`。
- 新增业务空场景 `Assets/Game/Scene/Home.unity`，作为业务侧场景入口与后续页面挂载点。
- `P0-001` 的范围基线**不新建文档副本**：任务表工作簿与《分批开发执行计划》已足够，范围冻结只以"确认一致"为验收，不产生新的权威载体。

## Capabilities

### New Capabilities

- `project-assembly-layout`: 定义业务程序集的划分、命名、引用方向与目录职责，以及"业务不得进入 `ScriptsBuiltin`"的可验证边界。
- `project-startup-chain`: 定义业务启动 Procedure 在 `AppConfigs` 中的注册契约、启动链顺序与空业务场景的进入条件。
- `project-input-abstraction`: 定义 `IInputProvider` / `InputModule` 的输入事件契约、Provider 替换规则，以及"业务不直读 `UnityEngine.Input`"的可验证边界。

### Modified Capabilities

无。本批次只新增业务侧能力，不修改任何既有框架规格。

## Impact

- 新增：`Assets/Game/Scripts/EverlightTales/**`（5 个 asmdef + 契约代码 + 各层 README）、`Assets/Game/Scene/Home.unity`。
- 修改：`Assets/Game/ScriptableAssets/Core/AppConfigs.asset`（仅 Procedures 列表新增一项）。
- 不影响：`Assets/Game/ScriptsBuiltin/**`、既有框架 Procedure 顺序、`Hotfix.asmdef`、`scripts/` 与 `tools/` 下的框架工具。
- 验证入口：`python tools/audit_framework_purity.py`、Unity 编译、`rg "UnityEngine\.Input"` 业务程序集扫描、Unity Play Mode 启动链走查。
- 已知未决（不阻塞本批次）：业务程序集在 HybridCLR 热更包中的最终归属由 `P6-010` 构建链路批次定型。
