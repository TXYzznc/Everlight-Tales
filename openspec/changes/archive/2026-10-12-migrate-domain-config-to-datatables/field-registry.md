# 字段登记与表覆盖

本登记以 `design.md` 第 2 节的字段方向为冻结口径，以 Excel 表头和生成行类为实际实现。字段只在表源登记一次；`.txt` 与 `Assets/Game/Scripts/DataTable/**Table.cs` 由生成链再生，不作为第二份字段定义。

## 业务表清单

| 分层 | 表 |
|---|---|
| Core | `EntityGroupTable`, `LanguagesTable`, `SoundGroupTable`, `UIGroupTable`, `UITable` |
| Board 基础 | `PartTable`, `FormTable`, `FormMaterialTable`, `BuffTable`, `AnomalyTable`, `ObstacleTable` |
| Board 关卡关系 | `LevelTable`, `LevelRoundTable`, `LevelGoalTable`, `LevelKeyPieceTable`, `LevelCarryPartTable`, `LevelFixedElementTable` |
| Board 红舞鞋 | `RedShoeTable`, `RedShoeGuideTable`, `RedShoeTurnTable` |
| Meta 事件与世界 | `EventTable`, `EventOpenPeriodTable`, `PlaceTable`, `StageReleaseTable`, `TrialTable`, `TaskTable`, `TaskBlueprintTable`, `RewardMaterialTable`, `QuestTable` |
| Meta 案件 | `CaseTable`, `InvestigationTable`, `CaseEventTable`, `CaseRewardOptionTable`, `BatchTable` |
| Meta 文案与资源 | `CopyTable`, `PortraitTable`, `TutorialHintTable`, `CalibrationTable`, `AudioCueTable`, `BgmTable`, `AssetBindingTable`, `NarrativeTable` |
| Meta 材料 | `MaterialTable` |

共 43 张表，均已注册到 `Assets/Game/ScriptableAssets/Core/AppConfigs.asset`。表之间的主键、外键、枚举、排序、坐标和空值语义见 `design.md`；关系完整性由 `GameConfigRuntime` 和 `tools/check_migrated_relations.py` 双重校验。

## 字段归属规则

- 基础目录字段放在对应 `*Table`；一对多内容拆为关系表，以稳定字符串键建立外键。
- 关卡坐标、轮次、特殊目标、携带池和固定元素不编码在单个字符串字段中，分别登记在 `Level*Table` 关系表。
- 红舞鞋专属起点、导流格、转向格、封存匣和限制登记在 `RedShoe*Table`，不回写运行时状态。
- 文案、立绘、音频、BGM、资源槽位和首案叙事使用稳定资源键；路径和文本是配置字段，实际资源仍由资源系统管理。
- `WorldState`、`WorldSave`、`RepairAttemptSave`、`BoardState` 和 PlayerPrefs 字段属于运行时/存档数据，不登记到业务配置表。

## 生成一致性

`python tools/datatable_excel_source.py --check` 对 43 张 Excel 源逐表通过；生成 `.txt` 和 C# 行类的列顺序与表头一致，重复刷新不产生无关差异。
