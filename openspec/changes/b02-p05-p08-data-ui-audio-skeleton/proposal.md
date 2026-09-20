## Why

G0 工程准备在 b01 立住了「业务代码边界」与「输入抽象边界」两条不可逆地基后，剩下三条**互不阻塞的基础设施链**必须在盘面开发（G1）之前可用：**数据**（表管线）、**UI**（页面壳与全局组件）、**音频**（分组与音量）。任务表把它们归为 P0-005～P0-008，并给出共同验收边界："页面壳与表加载在业务场景内可用"。

这三条链的共同风险是**返工成本随任务量放大**：后续 100+ 项任务全部要读写配置表、100+ 个界面全部要复用页面壳与弹窗组件、所有关卡与事件都要播放音效。若表管线不能重跑、页面壳不定型、音频分组缺失，这些成本会在 G1～G4 逐层放大。

本批次的前置阻塞项已在开工前解决：`GameData/DataTables/` 此前为空，Excel 源从未进入工程，`.txt` 与 `.cs` 生成物无法重新生成。已重建 Core 表 Excel 源并验证框架往返收敛（见《b02 DataTable 管线可重跑化》派发单）。因此本批次现在可以真正"打通生成链路"，而不是继承一份不可重跑的配置。

## What Changes

- 新增首批 7 张业务表的表骨架（事件／零件／关卡／Buff／材料／任务／语言），每表只落最小可用字段，走完整 `Excel → .txt → 生成 C# → AppConfigs 注册 → 运行时加载` 链路；字段最终冻结留给 `P0-011`。
- 7 张表的 Excel 源落 `GameData/DataTables/`，生成物落 `Assets/Game/DataTable/` 与 `Assets/Game/Scripts/DataTable/`，注册项写入 `Assets/Game/ScriptableAssets/Core/AppConfigs.asset` 的 `mDataTables`。
- 新增竖屏通用页面壳：1080×1920 基准、安全区适配、返回与页签，验证 GF.UI 的分组与 `UIFormBase`／`UIItemBase` 用法。页面壳是项目**第一个**注册界面，需同时走通「登记 `UITable` → 重新生成空的 `UIViews` 枚举 → 按枚举打开」这条此前从未在本项目跑通的链路。
- 新增全局组件：通用弹窗、Toast、加载、确认。实现基础按框架实际能力区分——弹窗与确认基于框架 `UIFormBase` 与 `UIDialogTemplate`（走 UI 生成链路），**Toast 与加载由项目侧自建**（框架不存在现成实现），挂到 `Dialog` / `Overlay` 分组并保证层级正确；这些组件在 b01 已建的 `Everlight.Tales.UI` 程序集内实现。
- 新增音频接入：音乐／音效／UI 音三个分组接入 `GF.Sound`，音量与静音设置占位，落 `SoundGroupTable` 已有分组之上。
- 新增项目侧的数据表加载路径：业务不直接引用框架 `DataTableComponent` 的内部实现，走一层项目侧入口，便于后续替换与测试。

## Capabilities

### New Capabilities

- `project-datatable-pipeline`: 定义业务表的 Excel 源位置与命名、字段行约定（表名／字段名／类型／备注行的列契约）、生成物路径、`AppConfigs.mDataTables` 注册契约，以及"管线可重跑且往返收敛"的可验证边界。
- `project-page-shell`: 定义竖屏页面壳的基准分辨率、安全区适配、返回/页签导航契约，以及 UI 分组与层级归属。
- `project-global-ui-components`: 定义通用弹窗／Toast／加载／确认组件的挂载分组、层级与可复用契约。
- `project-audio-groups`: 定义音乐／音效／UI 音分组、音量与静音设置的生效契约。

### Modified Capabilities

无。本批次只新增业务侧能力，不修改任何既有框架规格。

## Impact

- 新增：`GameData/DataTables/{Board,Meta}/**.xlsx`、`Assets/Game/DataTable/{Board,Meta}/**.txt`、`Assets/Game/Scripts/DataTable/{Board,Meta}/**.cs`。
- 新增：业务 UI 壳与全局组件（位于 `Assets/Game/Scripts/EverlightTales/UI/`）、音频分组接入与音量静音设置占位（位于 `Meta/` 与 `UI/`）。
- 修改：`Assets/Game/ScriptableAssets/Core/AppConfigs.asset`（`mDataTables` 列表新增 7 项）。
- 不影响：`Assets/Game/ScriptsBuiltin/**`、既有框架 Procedure 顺序、框架纯度审计口径。
- 验证入口：`python tools/audit_framework_purity.py`、`python tools/datatable_excel_source.py --check`、Unity 批处理编译、数据表往返 `git diff` 为空、Play Mode 表加载与页面开关走查。
- 已知未决（不阻塞本批次）：7 张表的最终字段清单由 `P0-011` 冻结；正式音频素材由 ART 路线提供，本批次只验证通路。
