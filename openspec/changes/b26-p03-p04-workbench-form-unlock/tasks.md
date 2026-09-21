# 任务：b26 加工台 UI 与流程 + 形态解锁与当前使用

- [x] Data：`FormConfig.cs`（FormKind / FormMaterialCost / FormConfig / FormCatalog 五形态）
- [x] Meta：`FormService.cs`（CanUnlock / Unlock / SwitchCurrent / GetCurrent / ResolveForm / GrantBlueprint / HasBlueprint）
- [x] Meta：`WorldState` 增 Blueprints / UnlockedForms / CurrentForms
- [x] Meta：`TaskService.Claim` 授予图样（`FormService.GrantBlueprint`）
- [x] Data：`WorldSave` 增 CurrentFormSave + Blueprints / UnlockedForms / CurrentForms
- [x] Meta：`WorldSaveService` Capture/Restore 持久化形态状态
- [x] UI：`WorkbenchLayout.cs`（三段布局 + 四状态分类 + 材料持有）
- [x] 探针 `B26AcceptanceRunner`（74/74 通过后删除）+ 最终重编译 0 错 0 警
