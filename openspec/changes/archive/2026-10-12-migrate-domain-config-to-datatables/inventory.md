# 配置与数据盘点基线

本文件记录迁移开始时的基线。静态配置由 DataTable 承载，账号、存档和当前尝试仍由运行时模型承载。

## 静态配置（迁移目标）

| 领域 | 当前来源 | 目标表 | 稳定键 | 当前状态 |
|---|---|---|---|---|
| 零件能力 | `Data/PartConfig.cs` 的 `PartCatalog` | `Board/PartTable` | `P-001`~`P-020` | 表驱动安装，正式条目已清理 |
| 零件图鉴 | `Data/PartCodex.cs` 的 `PartCodexCatalog` | `Board/PartTable` | `P-001`~`P-020` | 与能力共用表行，正式条目已清理 |
| 形态与配方 | `Data/FormConfig.cs`、`MFormConfig.cs` | `Board/FormTable`、`Board/FormMaterialTable` | `P-xxx-Fxx` / `M-xxx` | 表驱动安装，正式条目已清理 |
| 材料 | `Data/MaterialCatalog.cs` | `Meta/MaterialTable` | `MT-001`~`MT-006` | 表驱动安装，正式条目已清理 |
| Buff、异常、投放 | `BuffConfig.cs`、`AnomalyType.cs`、`StageRelease.cs` | `Board/BuffTable`、`Board/AnomalyTable`、`Meta/StageReleaseTable` | `BF-*`、`AN-*`、内容稳定键 | 表驱动安装，正式条目已清理 |
| 事件、关卡、任务 | `Events/*`、`TrialConfig.cs`、`WorldSession` 内容数组 | `Meta/*`、`Board/Level*`、`Board/RedShoe*` | `EV-*`、`L-*`、`T-*` | 事件、试机、盘面关系、红舞鞋规则、任务和奖励关系已迁移；运行时状态仍由会话/存档承载 |

## 运行时/账号数据（不迁移为配置）

- `WorldState`：已拥有零件、材料数量、蓝图、已解锁形态、当前形态、任务状态和案件状态。
- `WorldSave` / `PlayerPrefs`：存档槽位、天数、时间段、资源和任务进度。
- `RepairAttemptSave`、`BoardState`：当前盘面的实体、能量、连锁、轮次和特殊目标进度。
- `CurrentEvent`、`PendingCarry`、`PendingEventConfig`：一次进入流程中的临时状态。

## 当前覆盖基线

- `PartCatalog`：20 个条目；包含能力数值 15 列。
- `PartCodexCatalog`：20 个条目；与零件能力共享同一个稳定键。
- `MaterialCatalog`：6 个条目；包含稀有度、来源类型化标记和风味文案，旧目录初始条目已清理。
- `FormCatalog`：19 个条目；配方拆为 54 条 `FormMaterialTable` 关系行。
- `BuffCatalog`：20 个条目；效果类型、数值、目标零件和叠加上限已进入 `Board/BuffTable`。
- `AnomalyCatalog`：8 个条目；偏转、方向、阈值、芯数量和反转周期已进入 `Board/AnomalyTable`。
- `ReleaseTable`：39 个条目；阶段、首次出现提示和永久解锁条件已进入 `Meta/StageReleaseTable`。
- `PlaceCatalog`：5 个地点；名称、描述、解锁来源、地图坐标和家园标记进入 `Meta/PlaceTable`，旧目录初始条目已清理。
- `SupplyCatalog` / `NormalEventCatalog`：6 个事件条目；开放时段拆为 19 条 `Meta/EventOpenPeriodTable` 关系，权重、耗时、奖励、失败收尾、材料和地点绑定进入 `Meta/EventTable`，旧目录初始条目已清理。
- `TrialCatalog`：4 个试机条目；借用形态、宿主零件、轮次目标和特殊目标进入 `Meta/TrialTable`，旧目录初始条目已清理。
- `TaskCatalog`：4 个改装任务模板；任务字段进入 `Meta/TaskTable`，图样奖励拆为 4 条 `Meta/TaskBlueprintTable` 关系，旧目录初始条目已清理。
- `RewardMaterialCatalog`：5 条事件/首案回访材料奖励关系进入 `Meta/RewardMaterialTable`，首案回访配置已改为读取该目录。
- `CaseCatalog` / `InvestigationCatalog`：`L-01` 案件模板与 `IV-R1` 调查入口进入 `Meta/CaseTable`、`Meta/InvestigationTable`，红鞋调查创建案件时读取配置模板。
- `CopyCatalog`：12 条统一文案进入 `Meta/CopyTable`，运行时从配置快照安装。
- `PortraitCatalog`：3 个人物立绘目录条目进入 `Meta/PortraitTable`，对话立绘服务只消费配置键。
- `TutorialHintCatalog`：3 段教学提示进入 `Meta/TutorialHintTable`，教学页通过快照获取引导和解锁零件。
- `CalibrationCatalog`：18 条校准口径进入 `Meta/CalibrationTable`，运行时校验值域后安装；普通事件费率和默认维修效果分也从校准项读取。
- `ObstacleCatalog`：8 种障碍参数进入 `Board/ObstacleTable`，障碍行为只读取表驱动参数。
- `AudioCueCatalog` / `BgmCatalog`：6 个音效位和 2 条昼夜 BGM 进入 `Meta/AudioCueTable`、`Meta/BgmTable`。
- `AssetBindingCatalog`：9 个资源槽位进入 `Meta/AssetBindingTable`，素材路径仍是配置字段，账号状态不写入。
- `NarrativeCatalog` / `CaseEventCatalog`：首案序章、三段对话图、调查热点、维修入口、回访费、回访图样和结案解锁零件进入 `Meta/NarrativeTable`、`Meta/CaseEventTable`。
- `CaseRewardOptionCatalog`：首案局内 Buff、零件和机械臂奖励池进入 `Meta/CaseRewardOptionTable`。
- `BatchCatalog`：五个怪谈批次的名称、顺序和开放完成数量进入 `Meta/BatchTable`。
- `LevelPresetCatalog`：9 个关卡预设（卷帘门、4 个试机、3 段教学、红舞鞋）；`Board/LevelTable`、`LevelRoundTable`、`LevelGoalTable`、`LevelKeyPieceTable`、`LevelCarryPartTable`、`LevelFixedElementTable` 共 6 张关系表驱动盘面装配。
- `RedShoePresetCatalog`：1 个红舞鞋规则预设，导流格 6 行、转向格 1 行；`Board/RedShoeTable`、`Board/RedShoeGuideTable`、`Board/RedShoeTurnTable` 驱动红鞋专属规则装配。
- `AppConfigs.asset` 已注册零件、形态、材料、Buff、异常、投放、事件、地点、案件、调查、试机、任务、奖励、盘面、红舞鞋、文案、立绘、教学、校准、障碍、音频、资源槽位、首案叙事、批次和案件奖励相关的 43 张表；已迁移表均补齐正式行并扩展字段。
- 当前配置表生成链已覆盖 43 张业务表并包含已迁移领域的正式行；剩余硬编码仅需继续区分 UI 展示文案、系统构建口径和测试夹具，不把存档状态写入表。
- `tools/check_migrated_relations.py` 对生成后的 `.txt` 做独立外键探针，覆盖试机、任务、关卡、事件开放时段和红舞鞋关系；该探针不能替代 Unity PlayMode 行为回归。

## 双读规则

加载表后先将已迁移表转换为纯 DTO，再进行行键、外键、枚举、数量和关系完整性校验；通过后由 `GameConfigSnapshot` 安装到目录。已完成领域不再依赖旧字面量作为运行时权威来源。
