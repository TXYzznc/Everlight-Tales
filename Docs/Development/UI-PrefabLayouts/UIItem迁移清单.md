# Everlight UIItem 迁移清单

## 已完成

| UIItem | 预制体 | 使用页面 | 运行时创建入口 |
|---|---|---|---|
| `SaveSlotItem` | `Assets/Game/Prefabs/UI/Item/SaveSlotItem.prefab` | `SaveSlotPage` | `SaveSlotPage.RefreshSlots()`，3 个存档位由 `SpawnItem` 创建 |
| `RewardChoiceItem` | `Assets/Game/Prefabs/UI/Item/RewardChoiceItem.prefab` | `SettlementPage` | `SettlementPageForm.BuildChoices()`，奖励选项按数据动态创建 |
| `CarryAvailableItem` | `Assets/Game/Prefabs/UI/Item/CarryAvailableItem.prefab` | `PreparationPage` | `PreparationPageForm.BuildAvailableList()`，可携带部件按数据动态创建 |
| `ArchiveDisplayItem` / `ArchiveOwnedItem` / `ArchiveMaterialItem` / `ArchiveBlueprintItem` / `ArchiveSummaryItem` | `Assets/Game/Prefabs/UI/Item/` | `ArchivePage` | `ArchivePanel.Build()`，五个分组分别通过 GF 对象池创建 |
| `CodexPartItem` / `CodexFormItem` / `CodexCaseItem` | `Assets/Game/Prefabs/UI/Item/` | `CodexPage` | `CodexPanel.RebuildList()`，部件、形态、案件按页签创建 |
| `WorkbenchHostItem` / `WorkbenchFormItem` / `WorkbenchMaterialItem` / `WorkbenchLedgerItem` | `Assets/Game/Prefabs/UI/Item/` | `WorkbenchPage` | `WorkbenchPanel.RebuildWorkbench()`、`RebuildMaterials()`、`RebuildLedger()` |
| `MapEventItem` | `Assets/Game/Prefabs/UI/Item/MapEventItem.prefab` | `MapPage` | `MapPanel.AddEventEntry()`，地点事件按当前地点数据动态创建 |
| `JournalItem` | `Assets/Game/Prefabs/UI/Item/JournalItem.prefab` | `JournalPage` | `JournalPanel.AddCard()`，事件/任务/怪谈详情卡按数据动态创建 |
| `DialogueChoiceItem` | `Assets/Game/Prefabs/UI/Item/DialogueChoiceItem.prefab` | `DialoguePage` | `DialoguePanel.AddChoice()`，运行时从 Resources 对应正式 Item 资源实例化 |
| `InvestigationHotspotItem` | `Assets/Game/Prefabs/UI/Item/InvestigationHotspotItem.prefab` | `InvestigationPage` | `InvestigationView.AddHotspot()`，调查热点按场景数据动态创建 |
| `GuestDelegationItem` | `Assets/Game/Prefabs/UI/Item/GuestDelegationItem.prefab` | `GuestPage` | `GuestPanel.BuildDelegations()`，当天委托按供应数据动态创建 |

## 接入约束

- Item 预制体只保留模板结构，页面预制体不再保存这些重复实例。
- 页面通过 `GameObject` 模板字段引用 Item 预制体，运行时使用 GF `UIFormBase.SpawnItem<T>` / `UnspawnAllItem<T>`。
- `UIFormBase` 使用委托保存泛型对象池，避免 `IObjectPool<T>` 的无效强转。
- Item 内部的按钮、文本、正式美术资源随模板统一绑定；页面只负责数据绑定和回调。

## 后续可按同一模式扩展

`BoardPage` 的六边形格、实体块和 HUD 文本属于盘面渲染对象，不按列表 Item 处理；其余重复数据对象已按上表迁移。扩展时分别新增 `Assets/Game/Prefabs/UI/Item` 下的模板和 `UIItemObject` 包装类，不在页面预制体内复制固定数量实例。

## 20 页验收状态

`Library/AllUiPrefabAudit.txt` 记录了 20 个页面的文本、图片、布局组件和缺失引用扫描；`Library/UIAllPagesSmokeTest.txt` 记录了 1080×1920 PlayMode 逐页打开结果。页面级重复数据优先按上表使用 UIItem，其余页面先完成固定结构、正式 Sprite 引用和打开路径验收，再按第二批清单迁移动态对象。
