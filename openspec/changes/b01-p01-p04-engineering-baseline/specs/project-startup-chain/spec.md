## ADDED Requirements

### Requirement: 业务启动 Procedure 唯一注册

`AppConfigs` 的已启用流程列表 MUST 只新增一个实现 `IFrameworkStartupProcedure` 的业务 Procedure `EverlightTalesProcedure`。MUST NOT 注册第二个启动 Procedure，MUST NOT 修改框架既有 Procedure 的注册项与顺序。

#### Scenario: 注册表包含两个启动 Procedure
- **WHEN** `AppConfigs.Procedures` 中出现两个 `IFrameworkStartupProcedure` 实现
- **THEN** 框架 MUST 抛出 `InvalidOperationException` 报告两个类型的全名

#### Scenario: 检查注册结果
- **WHEN** 审计读取 `Assets/Game/ScriptableAssets/Core/AppConfigs.asset`
- **THEN** Procedures 列表中恰好存在一个业务启动 Procedure，全名为 `Everlight.Tales.Procedure.EverlightTalesProcedure`

### Requirement: 启动链从 Launch 进入空业务场景

启动链 MUST 为 `Launch → PreloadProcedure → FrameworkReadyProcedure → EverlightTalesProcedure → ChangeSceneProcedure(Home)`，并在进入业务场景后不残留加载进度界面。

#### Scenario: 从 Launch 启动
- **WHEN** 在 Play Mode 从 `Launch` 场景启动
- **THEN** 日志 MUST 依次出现框架预加载完成、框架就绪与业务 Procedure 进入记录，且最终加载 `Home` 场景

#### Scenario: 业务 Procedure 被框架发现
- **WHEN** `FrameworkReadyProcedure` 按类型名解析启动 Procedure
- **THEN** MUST 成功解析到 `Everlight.Tales.Procedure.EverlightTalesProcedure`，MUST NOT 记录"failed to select a startup procedure"

### Requirement: 业务启动 Procedure 只承担启动职责

`EverlightTalesProcedure` MUST 只承担启动职责：登记业务入口日志、初始化项目输入模块、转入场景加载。MUST NOT 在该 Procedure 内实现玩法、表加载、存档或 UI 逻辑。

#### Scenario: 启动 Procedure 进入时
- **WHEN** `EverlightTalesProcedure.OnEnter` 执行
- **THEN** 输入模块 MUST 已初始化，且流程 MUST 转入 `ChangeSceneProcedure` 加载业务场景，MUST NOT 停留在该状态等待外部事件

### Requirement: 业务空场景可独立加载

`Assets/Game/Scene/Home.unity` MUST 是一个可加载的空业务场景，MUST NOT 依赖任何业务内容资源。它作为后续页面的挂载点，MUST NOT 在本批次包含业务 Prefab 或数据。

#### Scenario: 加载业务空场景
- **WHEN** `ChangeSceneProcedure` 以场景名 `Home` 加载
- **THEN** 场景 MUST 加载成功且无缺失引用、无 Console 报错
