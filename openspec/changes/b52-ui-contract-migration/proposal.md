## Why

当前 UI 运行时代码、页面表单、Item 组件和 Editor 工具混用固定层级路径、`GetComponentInChildren`、全局查找和序列化引用。节点仅移动父级就可能造成绑定失败；一个可选详情字段缺失还可能阻断整个列表生成。需要把全项目 UI 统一到可校验、可维护的 View 引用契约。

## What Changes

- 覆盖 `Assets/Game/Scripts/EverlightTales/UI` 中所有可能持有 UI 引用的页面、弹窗、局部 View 和 Item 组件。
- 覆盖 `Assets/Game/Scripts/EverlightTales/UI/Editor` 中所有读取或写入 UI 层级的生成器、迁移器、绑定器和验证器。
- 覆盖 `Assets/Game/Prefabs/UI` 下全部页面、弹窗和 Item 预制体，不因当前暂时正常而排除。
- 用序列化 View 引用和局部 View 组件作为运行时契约；固定路径只保留给显式的 Editor 迁移/修复工具。
- 为必选引用、可选引用、条目内部引用建立明确校验，并让编辑器在预制体修改后立即报告缺失。
- 按界面逐个迁移、逐个编译和运行验收，不进行未经审阅的批量自动改写。

## Scope Inventory

### Runtime path-bound files

`ArchivePanel.cs`, `BoardHUD.cs`, `CityMapView.cs`, `CodexPanel.cs`, `DialoguePageForm.cs`, `DialogView.cs`, `FeedbackPageForm.cs`, `GuestPanel.cs`, `HomePanel.cs`, `InvestigationView.cs`, `JournalPanel.cs`, `MapPanel.cs`, `OpeningOverlay.cs`, `RecoveryPageForm.cs`, `ServicePanel.cs`, `TimePeriodBar.cs`, `UIFactory.cs`, `WorkbenchPanel.cs`。

### Runtime serialized/view files requiring unified validation

所有页面表单、局部 View 和 Item 组件，包括 `BoardPageForm`、`PreparationPageForm.Fields`、`SettingsForm.Fields`、`SettlementPageForm.Fields`、`SaveSlotPage.Fields`、`ArchiveItem`、`CodexItem`、`GuestDelegationItem`、`JournalItem`、`MapEventItem`、`MapNodeItem`、`RewardChoiceItem`、`SaveSlotItem`、`WorkbenchItem` 等。

### Editor tools

`EverlightFormalUiResourceBinder`、`EverlightUiItemPrefabGenerator`、`EverlightUiRegistrationValidator`、`UiLayoutPrefabMigration`、`UiScrollingLayoutMigration`、`JournalSectionLayoutMigration`、`WorkbenchMaterialsLayoutMigration`、`UiSinglePageReview`、`UiPrefabDeepAudit`、`UiInteractiveRaycastAudit` 及相关验证入口。

### Prefabs

`Assets/Game/Prefabs/UI` 下全部页面、弹窗和 `UI/Item` 预制体，以 AssetDatabase 扫描结果为准；不得只处理 HomePage 或 WorkbenchPage。

## Non-Goals

- 不改变 UI 的视觉设计、布局尺寸、文案、数据模型或业务流程，除非绑定迁移为保证功能必须调整。
- 不把运行时数据写入 ScriptableObject。
- 不通过一个全局自动脚本重写全部预制体；每个界面必须有可审阅的迁移记录和验证结果。
- 不修改用户维护的任务表。
