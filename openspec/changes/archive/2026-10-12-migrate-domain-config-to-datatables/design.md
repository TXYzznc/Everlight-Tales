# Design: 全局领域配置 DataTable 化

## 1. 配置与数据的边界

```
GameData/DataTables/*.xlsx       全局配置源（版本库内容）
        │ 生成
Assets/Game/DataTable/*.txt      运行时表资源
Assets/Game/Scripts/DataTable/*  生成的行类型
        │ AppConfigs + GF.DataTable.LoadDataTable
        ▼
配置读取适配层                      只构造只读 DTO 快照
        ▼
Data / Board / Events / Meta / UI  解释规则、展示和改变运行时状态

WorldSave / RepairAttemptSave / PlayerPrefs / BoardState
        └── 玩家数据、存档数据、当前局状态，不进入 DataTable
```

`DataTable` 是配置的唯一权威源。生成的 `.txt` 与 `.cs` 是可再生物，不直接手工维护。运行时配置快照只在启动/换配置时建立，不能被玩法逻辑作为玩家数据写回。

## 2. 表分层

使用基础类型和稳定字符串 ID。ID 用于同一版本的表内外键和新存档引用；本批次不提供历史存档迁移。复杂结构拆成关系表，表之间使用外键；不把坐标列表、轮次列表或奖励列表塞进单个 JSON 字段。

### 基础目录

| 表 | 关键字段方向 | 来源 |
|---|---|---|
| `Board/PartTable` | `Id, Key, Name, PartKind, Stage, SourceHint, UseHint, TriggerScore, EnergyCapacity, EffectCost, EffectScorePerCell, EffectScorePerTarget, PublicEnergyPerEffect, BonusScoreFromNth, BonusNth, RemovesSelf, EffectScorePerEnergy, PushDistance, RayRange, ChargeThreshold, SecondaryEffectScore` | `PartConfig` + `PartCodexConfig` |
| `Board/FormTable` | `Id, Key, Name, FormKind, HostPartKind, Description, UnlockFee, BlueprintId, SourceHint, AutoSetCurrent` | `FormConfig` / `MFormConfig` |
| `Board/FormMaterialTable` | `Id, FormKey, MaterialKey, Count, SourceCase, SortOrder` | `FormMaterialCost` |
| `Board/BuffTable` | `Id, Name, Stage, Stackable, MaxStacks, Description, Effect, Value, TargetPartId` | `BuffConfig` |
| `Meta/MaterialTable` | `Id, Key, Name, Rarity, IsTypedByCase, FlavorText` | `MaterialConfig` |

### 事件和盘面目录

| 表 | 关键字段方向 | 来源 |
|---|---|---|
| `Meta/EventTable` | `Id, Key, Name, EventKind, MinStage, TimeCost, RewardFee, DayWeight, NightWeight, PlaceKey, Repeatable, FailureWrapUp, RewardMaterialKey` | `NormalEventConfig` / `SupplyCandidate` |
| `Meta/EventOpenPeriodTable` | `EventId, Period` | `OpenPeriods` |
| `Board/LevelTable` | `Id, Name, BoardRadius, InitialPartCount, InitialArmMoves, InitialPublicRepairEnergy, Objective, Duration, EventId` | `LevelBoardConfig` |
| `Board/LevelRoundTable` | `LevelId, RoundIndex, TapCount, TargetScore` | `RoundConfig` |
| `Board/LevelGoalTable` | `LevelId, RoundIndex, GoalId, GoalKind, EntityId, Label, Text` | `SpecialGoalConfig` / `GoalConfig` |
| `Board/LevelKeyPieceTable` | `LevelId, PartId, Q, R` | `KeyPieceConfig` |
| `Board/LevelCarryPartTable` | `LevelId, PartId, SortOrder, IsBorrowed` | `CarryPool` / `BorrowedParts` |
| `Board/LevelFixedElementTable` | `LevelId, ElementId, ElementKind, Q, R, ObstacleType, GoalId, Label, IsRandom` | `FixedElementConfig` |
| `Board/RedShoeTable` | `LevelKey, StartQ, StartR, StartDirection, BoxQ, BoxR, BoxRequired, EntryQ, EntryR, RequiredDiversion, RestraintLimit, StepsPerTap` | `RedShoeConfig` |
| `Board/RedShoeGuideTable` | `LevelKey, Q, R, SortOrder` | 红舞鞋导流格关系 |
| `Board/RedShoeTurnTable` | `LevelKey, Q, R, Direction, SortOrder` | 红舞鞋转向格关系 |
| `Board/AnomalyTable` | `Id, Deflection, SettleDirection, Threshold, CoreCount, TapPeriod` | `AnomalyConfig` |

### 世界、任务和试机目录

| 表 | 关键字段方向 | 来源 |
|---|---|---|
| `Meta/TrialTable` | `Id, Name, BorrowedFormId, HostPartId, BlueprintId, LevelId, SpecialGoal` | `TrialConfig` |
| `Meta/TaskTable` | `Id, Name, RewardFee, IsMain, Client, Description, TotalSteps, RequiredCaseDone, RequiredSuccessCount, PlaceId` | `TaskConfig` |
| `Meta/TaskBlueprintTable` | `TaskId, BlueprintId, SortOrder` | `TaskConfig.Blueprints` |
| `Meta/PlaceTable` | `Id, Name, Description, UnlockSource, DayWeight, NightWeight, StartsUnlocked` | `PlaceConfig` |
| `Meta/CaseTable` | `Key, Name, Batch, TotalStages, Source, FirstPlaceKey` | `CaseConfig` |
| `Meta/InvestigationTable` | `Key, Name, ObservationCount, CaseKey` | `InvestigationConfig` |
| `Meta/StageReleaseTable` | `Id, Kind, Stage, FirstSeenText, UnlockCondition` | `ReleaseTable` |
| `Meta/RewardMaterialTable` | `OwnerId, MaterialId, Count, SourceCase` | `EventReward` / 事件奖励关系 |
| `Meta/CopyTable` | `Key, Category, Text, Calibrated` | `CopyEntry` |
| `Meta/PortraitTable` | `Key, Name, Description` | `PortraitConfig` |
| `Meta/TutorialHintTable` | `Key, StageIndex, Title, TeachGoal, GuideText, HighlightTarget, UnlockPartKind` | `TutorialHintConfig` |
| `Meta/CalibrationTable` | `Key, Category, Name, Value, Min, Max, Note` | `CalibrationBound` |
| `Board/ObstacleTable` | `Key, ObstacleKind, MaxDurability, PassDirection` | `ObstacleConfig` |
| `Meta/AudioCueTable` | `Key, CueKind, Name, Description` | `AudioCueConfig` |
| `Meta/BgmTable` | `Key, Track, Name, Description` | `BgmConfig` |
| `Meta/AssetBindingTable` | `Key, Category, Name, Path` | `AssetSlot` |
| `Meta/NarrativeTable` | `Key, ScriptKey, SortOrder, NodeKey, Speaker, Text, ChoiceLabel, NextNodeKey, Duration` | `NarrativeEntry` |
| `Meta/CaseEventTable` | `Key, CaseKey, InvestigationTitle, InvestigationDescription, RepairTitle, RepairDescription, RepairTimeCost, InvestigationPageTitle, Hotspot1~3, RevisitFee, RevisitBlueprintKey, CompletionPartKind` | `CaseEventConfig` |
| `Meta/BatchTable` | `Key, BatchNumber, Name, RequiredCount` | `BatchConfig` |
| `Meta/CaseRewardOptionTable` | `Key, OwnerKey, RewardKind, RewardId, SortOrder, Label` | `CaseRewardOptionConfig` |

## 3. 读取与程序集边界

生成的 DataTable 行类含 GF/Unity 运行时引用，不能被 `Everlight.Tales.Data` 或 `Everlight.Tales.Board` 直接引用。新增项目侧 `GameConfigRuntime`（放在可引用 GF runtime 的接入层）完成以下步骤：

1. 启动流程先确保 `AppConfigs.asset` 注册的表全部加载完成。
2. 按表名读取行，检查重复/缺失 ID、枚举值、外键和关系完整性。
3. 将行转换成 `Data` 层已有或重构后的纯 DTO，构造不可变的 `GameConfigSnapshot`。
4. 将快照注入 `WorldSession`、关卡创建器、`BoardGame` 和各服务；规则层只接收 DTO/快照，不接触生成行类。
5. 在配置加载失败时阻止进入业务页面，输出表名、行 ID、字段和原因。

第一阶段允许通过一个 `GameConfigProvider` 适配现有 `PartCatalog.Get` 等调用，后续按领域改为显式依赖。Provider 初始化后只读；禁止运行时写入配置快照。

## 4. 分批迁移顺序

1. **表结构与导入器**：先扩展 DataTable 字段、生成关系表、补齐当前 C# 中的真实内容，并建立快照/校验器。
2. **零件与形态**：迁移 `PartCatalog`、`PartCodexCatalog`、`FormCatalog`、`MFormCatalog` 和材料配方；验证准备页、图鉴、加工、盘面能力。
3. **Buff 与异常**：迁移 `BuffCatalog`、`AnomalyCatalog`、阶段投放；验证奖励选择、效果计算和投放过滤。
4. **事件与盘面**：迁移 `RollerDoorEvent`、`TrialEvent`、`RedShoeLevel`、`TutorialSample`、普通事件和供给候选；验证轮次、目标、固定元素、携带池和奖励。
5. **任务与世界目录**：迁移 `WorldSession` 中任务、地点、供给、任务链和试机目录；验证地图/准备页/任务页和结算。
6. **清理与阻断**：删除已迁移的静态条目和临时双读代码；保留规则算法和运行时状态；增加扫描阻止新增硬编码目录。

## 5. 双读校验

每个领域切换前，在 Editor/无头验证中同时读取旧 Catalog 和新 DataTable 快照，按稳定 ID 比较。双读只用于迁移期内容等价校验，不代表旧存档兼容：

- 标量数值、名称、描述、枚举和布尔值完全一致。
- 列表按明确的 `SortOrder` 或稳定 ID 比较，不依赖字典遍历顺序。
- 坐标、轮次和目标关系逐项比较。
- 差异报告包含表名、ID、字段、旧值和新值。

通过后将运行时调用切到快照，再删除旧 Catalog 内容。新版本创建的存档使用迁移后的稳定 ID；历史存档不作为本批次验收对象。

## 6. 运行时与编辑器验证

- Unity Editor：生成全部 DataTable，检查 `AppConfigs.asset` 注册和生成物幂等。
- 配置导入：运行完整性检查，阻止断裂外键、非法值、重复 ID 和资源键缺失。
- 规则探针：对零件能力、Buff、目标判定、轮次结算和奖励跑现有无头测试。
- Play Mode：从主页进入准备页、盘面、结算、试机、图鉴、加工、任务和存档读写。
- 构建前扫描：除规则常量和算法阈值外，禁止新增业务条目构造和静态 Catalog 注册。
