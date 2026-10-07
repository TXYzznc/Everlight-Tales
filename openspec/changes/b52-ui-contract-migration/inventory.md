# UI 契约迁移清单

生成日期：2026-10-06。此清单只记录扫描结果；每项必须逐界面审阅、迁移和验收。

## 统计

- 运行时代码：85 个
- 含层级/全局查找的运行时代码：30 个
- Editor UI 工具：16 个
- UI Prefab：41 个

## 含绑定查找的运行时代码

- [x] `Assets/Game/Scripts/EverlightTales/UI/ArchivePageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ArchivePanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/BoardHUD.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/BoardPage.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/CityMapView.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/CodexPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/CodexPanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/DialoguePageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/DialogView.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/FeedbackPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/GuestPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/GuestPanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/HomePageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/HomePanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/InvestigationPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/InvestigationView.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/JournalPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/JournalPanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/MapPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/MapPanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/OpeningOverlay.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ProloguePageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/RecoveryPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ServicePageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ServicePanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/TimePeriodBar.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/UIFactory.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/UIValidationHarness.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/WorkbenchPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/WorkbenchPanel.cs`

## 全部运行时代码

- [x] `Assets/Game/Scripts/EverlightTales/UI/ArchiveItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ArchivePageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ArchivePanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/BoardHexPulseEffect.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/BoardHUD.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/BoardImpactFX.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/BoardPage.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/BoardPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ButtonParticlesEffect.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/CarryAvailableItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/CasePageLayout.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/CityMapView.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/CodexItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/CodexLayout.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/CodexPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/CodexPanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Diagnostics/GameDiagnosticScenario.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/DialogueChoiceItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/DialoguePageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/DialoguePanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/DialogView.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/EntityVisuals.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/EventPageLayout.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/EverlightTalesUILayer.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/FeedbackPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/GlobalUI.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/GlobalUIRoot.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/GuestDelegationItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/GuestPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/GuestPanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/HexagonGraphic.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/HexBoardView.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/HexLayout.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/HomePageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/HomePanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/InvestigationHotspotItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/InvestigationPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/InvestigationView.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/JournalItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/JournalPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/JournalPages.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/JournalPanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/MainPageShell.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/MapCanvasInteraction.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/MapEventItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/MapNodeItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/MapPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/MapPanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/OpeningOverlay.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/OpeningPage.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/OpeningSequence.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/PauseMenuView.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/PreparationPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/PreparationPageForm.Fields.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/PreviewBoardView.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ProloguePageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/RecoveryPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/RewardChoiceItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/RotationPreviewRenderer.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/SafeAreaFitter.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/SaveSlotItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/SaveSlotPage.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/SaveSlotPage.Fields.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/SaveSlotService.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ScorePopup.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ScreenShakeEffect.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ServicePageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ServicePanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/SettingsForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/SettingsForm.Fields.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/SettlementPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/SettlementPageForm.Fields.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ShockwaveRingEffect.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/TaskPageLayout.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/TimePeriodBar.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/ToastItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/UIFactory.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/UIFormalButtonLibrary.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/UIFormalSpriteCatalog.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/UIValidationHarness.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/WorkbenchItem.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/WorkbenchLayout.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/WorkbenchPageForm.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/WorkbenchPanel.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/WorldSession.cs`

## Editor UI 工具

- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/CardPaperGUI.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/CardPaperPreviewTool.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightButtonSpriteBinder.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightFormalUiResourceBinder.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiItemPrefabGenerator.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiPrefabGenerator.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiRegistrationValidator.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/EverlightUiValidationWindow.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/JournalSectionLayoutMigration.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/UiInteractiveRaycastAudit.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/UiItemLayoutNormalizer.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/UiLayoutPrefabMigration.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/UiPrefabDeepAudit.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/UiScrollingLayoutMigration.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/UiSinglePageReview.cs`
- [x] `Assets/Game/Scripts/EverlightTales/UI/Editor/WorkbenchMaterialsLayoutMigration.cs`

## 全部 UI Prefab

- [x] `Assets/Game/Prefabs/UI/ArchivePage.prefab`
- [x] `Assets/Game/Prefabs/UI/BoardPage.prefab`
- [x] `Assets/Game/Prefabs/UI/CodexPage.prefab`
- [x] `Assets/Game/Prefabs/UI/DialoguePage.prefab`
- [x] `Assets/Game/Prefabs/UI/DialogView.prefab`
- [x] `Assets/Game/Prefabs/UI/FeedbackPage.prefab`
- [x] `Assets/Game/Prefabs/UI/GuestPage.prefab`
- [x] `Assets/Game/Prefabs/UI/HomePage.prefab`
- [x] `Assets/Game/Prefabs/UI/InvestigationPage.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/ArchiveBlueprintItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/ArchiveDisplayItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/ArchiveMaterialItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/ArchiveOwnedItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/ArchiveSummaryItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/CarryAvailableItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/CodexCaseItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/CodexFormItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/CodexPartItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/DialogueChoiceItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/GuestDelegationItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/InvestigationHotspotItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/JournalItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/MapEventItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/MapNodeItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/RewardChoiceItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/SaveSlotItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/WorkbenchFormItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/WorkbenchHostItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/WorkbenchLedgerItem.prefab`
- [x] `Assets/Game/Prefabs/UI/Item/WorkbenchMaterialItem.prefab`
- [x] `Assets/Game/Prefabs/UI/JournalPage.prefab`
- [x] `Assets/Game/Prefabs/UI/MainPageShell.prefab`
- [x] `Assets/Game/Prefabs/UI/MapPage.prefab`
- [x] `Assets/Game/Prefabs/UI/PreparationPage.prefab`
- [x] `Assets/Game/Prefabs/UI/ProloguePage.prefab`
- [x] `Assets/Game/Prefabs/UI/RecoveryPage.prefab`
- [x] `Assets/Game/Prefabs/UI/SaveSlotPage.prefab`
- [x] `Assets/Game/Prefabs/UI/ServicePage.prefab`
- [x] `Assets/Game/Prefabs/UI/Settings.prefab`
- [x] `Assets/Game/Prefabs/UI/SettlementPage.prefab`
- [x] `Assets/Game/Prefabs/UI/WorkbenchPage.prefab`

## 逐项验收记录格式

每个页面/组件完成后补充：序列化引用、必选/可选诊断、Prefab 校验、编译、运行烟雾、节点移动回归、控制台结果。

## 验收状态

- 41 个 UI Prefab 已由 `UiSerializedReferenceInventory` 逐资产扫描，缺失脚本为 0。
- 18 个页面契约逐个执行刷新菜单，全部 `未解析 0`。
- 页面验证器覆盖 18 个页面契约、41 个 UI Prefab 与 UITable 注册；节点重排场景已通过。
