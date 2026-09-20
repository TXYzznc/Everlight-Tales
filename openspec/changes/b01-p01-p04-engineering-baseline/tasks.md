# 实现任务（b01 工程基线）

覆盖原始任务：P0-001、P0-002、P0-003、P0-004。

## 1. 业务程序集与目录（P0-002）

- [x] 1.1 建立 `Assets/Game/Scripts/EverlightTales/` 与五个分层子目录 `Data/`、`Board/`、`Meta/`、`Events/`、`UI/`
- [x] 1.2 为五层各写 `asmdef`：命名 `Everlight.Tales.<Layer>`，`rootNamespace` 同名，引用方向按 `design.md` D1 表（`Meta` 额外引 `Builtin.Runtime` 取诊断日志，`Events` 额外引 `GameFramework`，已登记偏差）
- [x] 1.3 为 `Everlight.Tales.Board` 设置 `noEngineReferences: true`，确认引擎类型在该程序集内编译不可用（`Data`、`Events` 同样零引擎）
- [x] 1.4 各层放置占位契约类型（`HexCoord`／`HexDirection`／`HexGrid`、事件队列与触发契约、输入模块、各层锚点）
- [x] 1.5 各层写 `README.md` 说明职责、可写边界与引用方向
- [x] 1.6 新增 `tools/verify_project_assemblies.py` 校验命名、引用方向、引用可解析性、无环与零引擎约束

## 2. 业务启动链（P0-003）

- [x] 2.1 实现 `Everlight.Tales.Procedure.EverlightTalesProcedure`：`ProcedureBase` + `IFrameworkStartupProcedure`，带 `ObfuzIgnore`
- [x] 2.2 `OnEnter` 中初始化 `InputModule`，并以 `VarString` 设置 `ChangeSceneProcedure` 的 `SceneName` 参数后切换状态
- [x] 2.3 新建业务空场景 `Assets/Game/Scene/Home.unity`（无 GameObject，沿用 `Launch` 的渲染／光照／导航设置）
- [x] 2.4 在 `AppConfigs.asset` 的 Procedures 列表新增该 Procedure 全名，保持既有项与顺序不变
- [x] 2.5 启动链走查：`Launch → Preload → FrameworkReady → EverlightTales → Home`（无头编译探针已确认业务 Procedure 已在 `AppConfigs` 注册、`Home.unity` 存在且全部程序集加载；完整 Play Mode 视觉走查留待人工验收）

## 3. 项目输入抽象（P0-004）

- [x] 3.1 定义 `InputEventType`（点击按下／点击抬起／拖动／长按／按钮）与 `InputEvent` 结构（类型、屏幕位置、位移、载荷、时间戳）
- [x] 3.2 定义 `IInputProvider`：`Initialize`、`Poll`、`TryDequeue`、`Shutdown`
- [x] 3.3 实现 `TouchInputProvider`（触摸默认实现，跟随稳定手指 ID）
- [x] 3.4 实现 `EditorMouseInputProvider`（编辑器鼠标回退实现，右键产出按钮语义）
- [x] 3.5 实现 `InputModule`：当前 Provider 持有与替换、每帧轮询、单次消费队列；替换时先释放旧来源并清空缓冲
- [x] 3.6 场景驱动载体：`ProjectInputDriver` 由启动 Procedure 创建、跨场景保留、以 `DefaultExecutionOrder(-10000)` 保证先于业务消费

## 4. 范围口径确认（P0-001）

- [x] 4.1 核对《分批开发执行计划》批次边界与本批次覆盖任务一致，且不新建范围副本
- [x] 4.2 在《设计决定》登记 D-089／D-090，在《设计状态》登记实现状态，在《待完善设计》登记未决项

## 5. 验证

- [x] 5.1 `python tools/audit_framework_purity.py` 通过（含新增业务程序集后）
- [x] 5.2 Unity 编译通过（退出码 0，`scriptCompilationFailed=false`），五个 asmdef 全部加载；`Board` 零引擎约束由 `verify_project_assemblies.py` 与无头几何断言共同确认
- [x] 5.3 静态扫描 `Assets/Game/Scripts/EverlightTales/` 下 `UnityEngine.Input.` 命中仅限两个 Provider 实现
- [x] 5.4 无头契约检查 7/7 场景通过（点击成对、拖动位移、长按单次、单次消费、替换清缓冲、替换换源、零引擎几何）；完整 `Launch → … → Home` 的 Play Mode 视觉走查留待人工验收
- [x] 5.5 记录本批次验证证据（命令、退出码、关键日志），交回传
