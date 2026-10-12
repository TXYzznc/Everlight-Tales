# Proposal: 将全局领域配置迁移到 DataTable

## Why

当前项目同时存在两套全局配置来源：`GameData/DataTables` 中的 GF DataTable 骨架，以及 `Assets/Game/Scripts/EverlightTales` 中的静态 Catalog、构造方法和样例配置。运行时实际使用的零件数值、形态、Buff、事件、关卡、任务、材料、投放和奖励大多仍由 C# 提供，导致 Excel 表看起来为空，策划无法通过配置表完整修改内容，也容易出现表与代码不一致。

本变更只处理“游戏全局配置”。玩家账号、存档、解锁、库存、当前形态、当前盘面、维修尝试和运行时状态仍属于数据，由现有 `WorldState`、`WorldSave`、`RepairAttemptSave`、`BoardState` 和 PlayerPrefs/存档链负责，不迁移到配置表。

## What Changes

- 将零件、形态、形态材料、Buff、材料、事件、事件投放、关卡、轮次、特殊目标、盘面固定元素、携带池、试机、任务、地点、阶段投放和异常参数等全局静态内容建立为 GF DataTable 的 Excel 源。
- 采用多张具备稳定 ID 和外键关系的 DataTable 表表达一对多内容，避免把关卡坐标、轮次、目标和奖励压进不可校验的单个字符串。
- 扩展生成的表字段和 `AppConfigs.asset` 注册，新增运行时配置装载快照，把 GF 表行转换为现有领域 DTO。
- 按领域分批替换 `PartCatalog`、`FormCatalog`、`BuffCatalog`、事件创建方法、`TrialCatalog`、`SupplyCatalog`、`PlaceCatalog`、任务与投放目录等硬编码来源。
- 保留领域规则算法和运行时状态代码；代码只解释配置，不再创建正式内容条目。
- 迁移期间进行表数据与旧 Catalog 的双读比对；每个领域通过回归后删除对应硬编码目录和临时兜底。
- 增加配置完整性、外键、重复 ID、枚举值、坐标合法性、轮次引用和资源键引用的编辑器/无头校验。

## Scope

### In scope

- `GameData/DataTables/**.xlsx`、生成的 `Assets/Game/DataTable/**.txt` 和 `Assets/Game/Scripts/DataTable/**.cs`。
- `Assets/Game/ScriptableAssets/Core/AppConfigs.asset` 的 DataTable 注册。
- `Assets/Game/Scripts/EverlightTales/Data` 的配置 DTO、配置快照和读取边界。
- `Assets/Game/Scripts/EverlightTales/Board`、`Events`、`Meta`、`UI` 对静态 Catalog 和关卡构造方法的调用迁移。
- 配置验证、双读差异报告、数据表往返和现有场景回归。

### Out of scope

- 玩家账号、存档槽位、解锁状态、材料库存、当前盘面和维修尝试数据。
- 玩法规则算法、输入、UI 布局、Prefab 层级、Sprite/音频资源制作。
- 通过配置表动态生成 C# 规则代码或把运行时状态写入 ScriptableObject。
- 旧存档兼容、旧存档迁移和历史存档格式重写。

## Acceptance

- DataTable 是全局内容的唯一权威来源；正式运行路径不再依赖领域 Catalog 中的硬编码条目。
- 当前已有内容（首案、红舞鞋、试机、准备页、盘面、结算、图鉴、加工和任务）在迁移后行为与显示保持一致。
- 新配置产生的新存档使用表内稳定 ID；旧存档不纳入本批次验收，也不承诺自动迁移。
- 所有配置表通过 Excel → `.txt` → 生成 C# → `AppConfigs` → 运行时加载链路。
- 配置校验能发现重复 ID、断裂外键、非法枚举、非法盘面坐标、缺失轮次/目标和无效资源键。
- Unity 2022.3 编译无错误，现有无头规则探针、DataTable 诊断和关键 Play Mode 回归通过。

## Risks

- 复杂盘面关系多，表拆分后外键错误会在运行时暴露。通过导入期校验和构建前阻断降低风险。
- 旧存档兼容不在本批次范围内；仍需冻结同一版本内的 ID，避免表内外键和新存档互相失配。
- DataTable 生成类依赖 GF 的运行时类型，而 `Data`/`Board` 保持无引擎约束。通过项目侧读取适配层生成纯 DTO 快照，禁止业务程序集直接引用生成行类。
- 一次全量切换难以定位回归。按领域分批双读、比对、切换和删除硬编码。
