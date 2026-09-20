## ADDED Requirements

### Requirement: 业务程序集按五层划分

业务代码 MUST 组织为五个程序集：`Everlight.Tales.Data`、`Everlight.Tales.Board`、`Everlight.Tales.Meta`、`Everlight.Tales.Events`、`Everlight.Tales.UI`，对应目录 `Assets/Game/Scripts/EverlightTales/` 下的 `Data/`、`Board/`、`Meta/`、`Events/`、`UI/`。MUST NOT 把业务代码放入单一未分层的程序集。

#### Scenario: 检查程序集清单
- **WHEN** 审计读取 `Assets/Game/Scripts/EverlightTales/` 下的全部 asmdef
- **THEN** 恰好存在上列五个程序集，且每个程序集的根目录与其目录名一一对应

#### Scenario: 新增业务代码时选择归属
- **WHEN** 新增一个业务类型
- **THEN** 该类型 MUST 落在上列五层之一，MUST NOT 落在 `Assets/Game/ScriptsBuiltin/`

### Requirement: 程序集引用方向单向

程序集引用 MUST 保持单向：`Data` 不引用其它业务程序集；`Board`、`Meta` 只引用 `Data`；`Events` 只引用 `Data` 与 `Board`；`UI` 可引用其余四个业务程序集。任何构成环的引用 MUST NOT 被接受。

#### Scenario: 检查引用方向
- **WHEN** 审计读取五个 asmdef 的 references
- **THEN** 不存在反向引用，且不存在两个业务程序集互相引用的环

### Requirement: 盘面程序集零引擎依赖

`Everlight.Tales.Board` MUST 设置 `noEngineReferences: true`，且 MUST NOT 引用任何 Unity 引擎程序集。该约束用于保证盘面模拟器可在无引擎环境离线演算。

#### Scenario: 盘面层误用引擎类型
- **WHEN** `Board/` 下出现引用 `UnityEngine` 命名空间的代码
- **THEN** Unity 编译 MUST 失败，因为该程序集不提供引擎引用

#### Scenario: 审计确认零引擎设置
- **WHEN** 审计读取 `Board` 的 asmdef
- **THEN** `noEngineReferences` MUST 为 `true`

### Requirement: 业务代码不进入框架核心

业务类型、业务常量与业务资源 MUST NOT 出现在 `Assets/Game/ScriptsBuiltin/`。框架纯度审计 MUST 在新增业务程序集后仍然通过。

#### Scenario: 业务代码写入框架核心
- **WHEN** 有人在 `Assets/Game/ScriptsBuiltin/` 下新增业务类型
- **THEN** `python tools/audit_framework_purity.py` MUST 以非零退出码报告该命中

#### Scenario: 新增业务程序集后运行审计
- **WHEN** 在业务程序集已建立的工作树上执行 `python tools/audit_framework_purity.py`
- **THEN** 审计 MUST 以零退出码通过
