# Tasks: 全局领域配置 DataTable 化

## Phase 0：盘点与字段冻结

- [x] 0.1 建立硬编码清单：静态 Catalog、`CreateConfig`、`WorldSession` 内的内容数组、教学/试机/红鞋关卡样张，并标注配置或运行时状态。
- [x] 0.2 为每个条目确认同版本稳定 ID，并建立旧枚举/字段到 DataTable ID 的映射；本批次不建立历史存档迁移映射。
- [x] 0.3 根据 `design.md` 的关系表草案冻结首批字段、枚举值域、坐标约束、排序规则和空值语义。
- [x] 0.4 输出配置覆盖率基线：每个旧 Catalog 条目、每个关卡目标、每个奖励和每个外键均有目标表行。（43 张表覆盖基础目录、盘面关系、红舞鞋专属规则、文案、立绘、教学、校准、障碍、音频、资源槽位、首案叙事、批次和案件奖励；UI 展示文案、系统构建口径与测试夹具单独标注）

## Phase 1：DataTable 表结构与导入链路

- [x] 1.1 扩展 `GameData/DataTables/Board` 和 `Meta` 的业务 Excel，创建基础表与关系表。（首批完成 `Board/PartTable`，其余关系表按领域推进）
- [x] 1.2 生成对应 `.txt` 和 `Assets/Game/Scripts/DataTable` 行类，保持 Excel → txt → C# 生成可重复。（首批完成 `Board/PartTable`）
- [x] 1.3 更新 `Assets/Game/ScriptableAssets/Core/AppConfigs.asset` 的 `mDataTables` 注册列表。（已有 `Board/PartTable` 注册，字段已扩展）
- [x] 1.4 新增配置加载适配层和 `GameConfigSnapshot`，把生成行转换为纯领域 DTO。（首批覆盖零件和材料）
- [x] 1.5 新增完整性校验：重复 ID、外键、枚举、排序、坐标、轮次、目标、奖励和资源键。（首批覆盖零件键、枚举、数量及字段完整性）
- [x] 1.6 增加迁移期 DataTable 与旧目录双读差异报告。（已完成域切换后保留结构、外键和数量校验）

## Phase 2：基础零件域

- [x] 2.1 迁移 `PartConfig`、`PartCodexConfig` 和 `PartCatalog`。（20 行 `Board/PartTable` 同时提供能力与图鉴字段，正式静态条目已清理）
- [x] 2.2 迁移 `FormConfig`、材料配方和 `FormCatalog`。（19 个形态、54 条配方关系已完成，正式形态条目已清理；`MFormCatalog` 仅保留兼容访问层）
- [x] 2.3 迁移 `MaterialCatalog`。（6 个材料由 `Meta/MaterialTable` 安装，旧目录初始条目已清理）
- [x] 2.4 将准备页、图鉴、加工服务、零件详情和盘面能力改为读取已安装的 FormCatalog 快照。（现有调用点继续通过目录 API 消费）
- [x] 2.5 运行配置快照、编译和准备页/图鉴/加工/盘面回归。（配置快照、最终编译、20 页面烟雾、特殊条目交互和列表行复用验收均通过）

## Phase 3：Buff、异常和投放域

- [x] 3.1 迁移 `BuffCatalog` 和 Buff 效果参数。（20 个 Buff 由 `Board/BuffTable` 安装，正式静态条目已清理）
- [x] 3.2 迁移 `AnomalyCatalog` 和异常阈值参数。（8 个异常由 `Board/AnomalyTable` 安装，正式静态条目已清理）
- [x] 3.3 迁移 `StageRelease`、投放条件和解锁文本。（39 个投放条目由 `Meta/StageReleaseTable` 安装，正式静态条目已清理）
- [x] 3.4 将奖励选择、Buff 效果、阶段过滤和盘面异常改为读取配置快照。（首案奖励选项、Buff、阶段投放和异常目录均由 DataTable 快照安装）
- [x] 3.5 运行无头规则探针和奖励/投放回归。（PlayMode 报告 `gf-diagnostics-run-all_20261012_054921.json` 通过）

## Phase 4：事件、关卡与盘面域

- [x] 4.1 迁移 `RollerDoorEvent` 的事件、轮次、目标、固定元素、关键件、携带池和奖励。（`Board/Level*Table` 关系表与 `Meta/EventTable` 驱动，运行时移除卷帘门正式构造数据）
- [x] 4.2 迁移 `TrialCatalog`、`TrialEvent` 和试机盘面绑定。（4 个试机的目录、轮次、目标和固定元素均由 `Meta/TrialTable` 与 `Board/Level*Table` 装配）
- [x] 4.3 迁移 `RedShoeLevel`、`TutorialSample` 及首案使用的盘面配置。（教学三段、红舞鞋初始零件/轮次、首案盘面以及红舞鞋起点、导流格、转向格、封存匣、入口和束缚限制均由关系表装配）
- [x] 4.4 迁移普通事件、供给候选、开放时段、权重和地点绑定。（6 个事件同时覆盖供给和普通事件目录，19 条开放时段关系进入 `Meta/EventTable` 与 `Meta/EventOpenPeriodTable`，安装前双读校验）
- [x] 4.5 将 `WorldSession`、`BoardPage`、`PreparationPage`、结算和目标判定改为读取快照。（生产入口消费目录快照；正式静态条目已清理）
- [x] 4.6 运行三轮首案、红鞋、试机、教学和特殊目标回归；通过后删除事件/关卡构造方法中的正式内容。（首案 95 项流程断言、四条试机会话边界、关卡关系诊断均通过）

## Phase 5：任务、地点和世界目录域

- [x] 5.1 迁移地点、任务、任务蓝图、奖励材料和前置条件。（地点、案件/调查入口、任务、图样关系和奖励材料均已表驱动并清理正式旧目录）
- [x] 5.2 迁移世界供给、任务链和重复事件目录。（`SupplyCatalog`、`TaskCatalog`、`NormalEventCatalog` 和重复事件入口均由 DataTable 快照提供）
- [x] 5.3 将地图、日志、任务页、准备页和世界会话改为读取配置快照。（生产入口消费地点、供给、任务、关卡和事件目录）
- [x] 5.4 保证 `WorldState`、`WorldSave`、`RepairAttemptSave`、PlayerPrefs 和 `BoardState` 继续只承载运行时/存档数据，并让新配置产生的新存档引用有效 ID。（PlayMode 存档边界探针通过）
- [x] 5.5 运行新存档、三槽隔离、任务推进、地图刷新和结算回归；记录旧存档不在本批次兼容范围内。（首案流程报告 `first-case-runtime-validation-20261012.txt` 通过）
- [x] 5.6 迁移统一文案、人物立绘、教学提示、校准口径、障碍参数、批次口径和首案奖励池；新增 7 张表并接入快照安装与运行时校验。

## Phase 6：清理、阻断和验收

- [x] 6.1 删除已迁移的静态内容数组、正式 `new XxxConfig(...)` 条目和临时双读代码。（基础目录、地点、事件、试机、任务、材料、零件、形态、Buff、异常、投放、文案、立绘、教学、校准、障碍、音频、资源槽位、首案叙事、批次和首案奖励目录已清理；保留规则算法、系统构建口径、UI 展示文案和测试夹具）
- [x] 6.2 为规则层保留必要的常量/算法默认值建立明确注释，避免误删规则逻辑。（行为分派、稳定键和系统构建口径均与领域配置分开标注）
- [x] 6.3 增加静态扫描，阻止新增业务 Catalog 和关卡内容构造；允许测试夹具显式标注。（`tools/check_domain_config_hardcodes.py` 检查首案奖励、回访、教学解锁等生产入口）
- [x] 6.4 运行 `python tools/datatable_excel_source.py --check`、`python tools/check_migrated_relations.py`、数据表往返、配置校验、Unity 编译、无头探针和核心 Play Mode 配置回归。（43 张表、关系探针、编译、静态扫描、存档边界、20 页面 UI 烟雾和 PlayMode 诊断均通过；完整 UI/流程回归仍列在 validation.md）
- [x] 6.5 更新配置文档、字段登记、迁移报告和验证证据；OpenSpec 验证通过后再归档。（`inventory.md`、`field-registry.md`、`migration-report.md` 和 `validation.md` 已更新）

## Definition of Done

- DataTable 是全局领域配置的唯一来源，所有正式内容均可在 Excel 中找到。
- 运行时代码只消费配置快照和规则算法，不创建正式领域条目。
- 账号/存档/当前局数据边界保持不变；新存档可读，旧存档兼容不属于本批次 DoD。
- 配置完整性错误在进入业务页面前可定位到表、行和字段。
- 现有首案、红鞋、试机、教学、准备、盘面、结算、图鉴、加工、任务和存档回归通过。
