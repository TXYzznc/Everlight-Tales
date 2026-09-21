# 变更提案：b26 加工台 UI 与流程 + 形态解锁与当前使用

覆盖任务：P4-003（加工台 UI 与流程）、P4-004（形态解锁与当前使用）。

## 动机

b25 立住经济基础（六种材料 + 维修费唯一货币 + 收支记账）后，b26 落地成长闭环的核心出口：加工台。加工台承担两件事——解锁形态（一次性支付维修费 + 按配方消耗材料，图样保留不消耗）与切换形态（已解锁形态间免费、立即生效、不耗时）。本批同时确立「当前使用（加工台默认）与准备页临时切换（单关）」的边界。

## 关键决定

- 形态配置与目录（D-167）：`FormKind`（F 通用零件形态 / M 怪谈形态）+ `FormMaterialCost`（物资条目 + 数量 + 来源怪谈，异常纹样类型化）+ `FormConfig`（宿主零件/工作方式说明/维修费/图样 ID/来源提示/材料清单/是否自动设当前）。`FormCatalog` 首批五种：F 类四形态（P-001-F01 贯通撞锤、P-001-F02 横推撞锤、P-003-F01 轴向线圈、P-003-F02 定时线圈，配方按 D-084）+ M-012 借力改道夹（红舞鞋形态，按 D-084 消耗普通材料 + 定势残晶 + 异常纹样·红舞鞋）。
- 形态服务（D-168）：`FormService`（Meta，仅引用 Data）——`CanUnlock`（费用/图样/材料三条件缺口分解）、`Unlock`（一次性 `EconomyService.Spend(Craft)` + `MaterialBackpack.Remove` 消耗材料 + 图样保留 + 解锁 + 自动设当前）、`SwitchCurrent`（免费，仅基础或已解锁形态）、`GetCurrent`（空=基础形态）、`ResolveForm`（临时切换优先且只影响本关、不改 CurrentForms）、`GrantBlueprint`/`HasBlueprint`（图样永久条件幂等）。
- 形态卡四状态与三段布局（D-169）：`WorkbenchLayout`（UI 纯逻辑）——上方=已拥有且有形态的宿主零件，中部=基础形态 + 全部形态卡（四状态：当前使用/已解锁/可加工/未取得图样），下方=详情（工作方式/解锁条件/材料持有量）；`FormCardEntry` + `MaterialHolding`（需求 vs 持有）。
- 存档扩展（D-170）：`WorldState` 增 `Blueprints`/`UnlockedForms`/`CurrentForms`（Dictionary<PartType,string>），`WorldSave` 增对应字段 + `CurrentFormSave` DTO，`WorldSaveService` 往返持久化。
- 图样仓储收口：`TaskService.Claim` 除发维修费外，把 `TaskConfig.Blueprints` 通过 `FormService.GrantBlueprint` 入库（b21 预留的「图样仓储在 b26+」）。

## 变更范围

- Data：新增 `FormConfig.cs`；`WorldSave.cs` 增 `CurrentFormSave` + Blueprints/UnlockedForms/CurrentForms。
- Meta：新增 `FormService.cs`；`WorldState` 增三字段；`TaskService.Claim` 授予图样；`WorldSaveService` 持久化。
- UI：新增 `WorkbenchLayout.cs`（三段布局 + 四状态分类 + 材料持有视图模型）。

## 验收

- UnitySkills Play Mode 综合探针（74/74，验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。

## 边界（本批有意留出）

- F 类/M 类完整配方目录内容（P4-005/P4-006）与零件图鉴（P4-007）在 b27；四条改装成长支线接入任务页（P4-013）在 b30。本批只以 F 四形态 + M-012 作为加工台机制的种子配置。
- 加工台的实际 UIForm 预制体/图标/立绘与素材挂接留美术批；本批交付纯逻辑布局与可测试的解锁/切换流程。
