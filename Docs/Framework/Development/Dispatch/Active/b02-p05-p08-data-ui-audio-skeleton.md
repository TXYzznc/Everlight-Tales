# b02 派发单：数据＋UI＋音频骨架

| 项 | 内容 |
|---|---|
| 派发 ID | `b02-p05-p08-data-ui-audio-skeleton` |
| 覆盖任务 | P0-005、P0-006、P0-007、P0-008 |
| 里程碑 | G0 工程基线 |
| 前置批次 | b01（已完成，提交 `f0dbb06`） |
| 并行能力 | 与 b03（`b03-p09-p12-save-random-diagnostics`）写域不重叠，可并行 |
| 预计工时 | 17～32 小时（任务表口径）；若逼近 18 小时上限按《开发执行约束》拆分 |
| 状态 | 方案待确认 |

## 覆盖任务原文（据《第一版开发任务表》，只读引用）

| 任务ID | 模块 | 任务名称 | 前置 | 完成定义(DoD) |
|---|---|---|---|---|
| P0-005 | 数据 | DataTable 管线与首批表骨架 | P0-003 | 一张示例表走通生成与运行时加载 |
| P0-006 | UI框架 | 通用页面壳 | P0-003 | 页面壳可开可关，安全区与分组正确 |
| P0-007 | 全局组件 | 通用弹窗/Toast/确认 | P0-006 | 组件可复用，弹窗层级正确 |
| P0-008 | 音频 | GF.Sound 接入 | P0-003 | 分组播放与音量静音生效 |

## 前置阻塞项（已解除）

《第一版开发任务表》的 P0-005 要求"打通 Excel→DataTable→AppConfigs 生成链路"，但开工前探明
`GameData/DataTables/` **目录存在而内容为空**：`.txt` 与 `.cs` 生成物已提交，Excel 源从未进入
工程。这意味着管线**不可重跑**，任何字段调整只能手改生成物。

已重建 5 张 Core 表的 Excel 源并验证框架往返收敛，证据见
[b02-datatable-excel-source.md](./b02-datatable-excel-source.md)。本批次现在可以真正"打通链路"，
而不是继承一份不可维护的配置。

## 三条链的当前事实

| 链 | 现状 | 本批次目标 |
|---|---|---|
| 数据 | `GameData/DataTables/Core/` 5 张 Core 表已可重跑；业务表尚无 | 6 张业务表骨架 + 端到端验证 + 注册 |
| UI | `UITable.txt` 为空表（项目尚无任何注册界面）；`UIGroupTable` 已有 `Default`／`Dialog`／`Overlay` | 页面壳 + 四类全局组件 |
| 音频 | `SoundGroupTable` 已有 `Music`／`Sound` 分组，无 UI 音分组 | 增补 `UISound` + 播放与音量静音通路 |

## 写域边界

**可写**

- `GameData/DataTables/Board/`、`GameData/DataTables/Meta/`
- `Assets/Game/DataTable/Board/`、`Assets/Game/DataTable/Meta/`
- `Assets/Game/Scripts/DataTable/Board/`、`Assets/Game/Scripts/DataTable/Meta/`
- `Assets/Game/Scripts/EverlightTales/UI/`（页面壳与全局组件）
- `Assets/Game/Scripts/EverlightTales/Meta/`（音频设置、数据表访问入口）
- `Assets/Game/ScriptableAssets/Core/AppConfigs.asset`（仅 `mDataTables` 列表追加）
- `Assets/Game/DataTable/Core/SoundGroupTable.txt` 与其 Excel 源（仅追加 `UISound` 分组）
- `Docs/`、`openspec/`

**只读**

- `Assets/Game/ScriptsBuiltin/**`（框架核心，纯度审计硬边界）
- `Docs/GameDesign/10-开发计划/第一版开发任务表.xlsx`（用户维护，只读引用）
- 既有框架 Procedure 顺序、b01 建立的五层 asmdef 引用方向

**禁写**

- 任何业务代码进入 `Assets/Game/ScriptsBuiltin/`
- 正式音频素材与 UI 视觉稿（ART 路线预算）

## 交付物

| 类别 | 路径 |
|---|---|
| 表源 | `GameData/DataTables/{Board,Meta}/*.xlsx`（6 张业务表） |
| 表数据 | `Assets/Game/DataTable/{Board,Meta}/*.txt` |
| 生成代码 | `Assets/Game/Scripts/DataTable/{Board,Meta}/*.cs` |
| 注册 | `Assets/Game/ScriptableAssets/Core/AppConfigs.asset` 的 `mDataTables` |
| 页面壳 | `Assets/Game/Scripts/EverlightTales/UI/` 下页面壳与页面注册 |
| 全局组件 | `Assets/Game/Scripts/EverlightTales/UI/` 下项目侧入口与组件 |
| 音频 | `SoundGroupTable` 增补 + 音量静音设置与分组播放入口 |

## 待确认决策（方案门禁）

| # | 决策点 | 建议 |
|---|---|---|
| D1 | 6 张业务表的目录与命名 | `Board/` 放零件／关卡／Buff，`Meta/` 放事件／材料／任务；表名 PascalCase 英文 + `Table` 后缀，与 Core 表风格一致；语言链复用既有 `Core/LanguagesTable`（见 D6） |
| D2 | 字段冻结尺度 | 只落 ID + 名称 + 1～2 个已确认字段，类型限基础类型；最终冻结留给 `P0-011` |
| D3 | 7 张表 vs 1 张样表 | 6 张业务表全建骨架，只有 `Board/PartTable` 走完整端到端验证（对应 DoD 的"一张示例表"） |
| D4 | 页面壳完成度 | 只做空壳可开可关 + 安全区 + 返回 + 页签 + 分组；导航栈与转场推迟到 b10 |
| D5 | 弹窗/Toast/音频接入方式 | **框架侧无 Toast 与加载组件**（UI 生成链路只有 `UIFormTemplate`／`UIItemTemplate`／`UIDialogTemplate` 三份模板），故：弹窗与确认基于 `UIFormBase` + `UIDialogTemplate` 走生成链路挂 `Dialog(200)`；Toast 与加载由项目侧自建（不派生 `UIFormBase`，自带排队／池化／交互屏蔽）挂 `Overlay(500)`。四者统一由项目侧入口暴露；音频本批次只做通路，素材留 ART 路线 |
| D6 | 语言表处理 | 复用既有 `Core/LanguagesTable`（已注册且承担运行时语言选择），不新建业务语言表，避免两个真相来源。因此任务表原文的"首批 7 张表"落地为 **6 张新表 + 1 张既有语言表** |

## 验收证据计划

| 项 | 命令／方式 |
|---|---|
| 框架纯度 | `python tools/audit_framework_purity.py` |
| 程序集布局 | `python tools/verify_project_assemblies.py` |
| 表源结构 | `python tools/datatable_excel_source.py --check` |
| 管线收敛 | 重跑生成后 `git diff -- Assets/Game/DataTable Assets/Game/Scripts/DataTable` 为空 |
| Unity 编译 | `-batchmode -nographics -executeMethod`（无头探针，验证后删除） |
| 运行时加载 | Play Mode 走查：业务场景内读到已配置行 |
| UI／音频 | Play Mode 走查：页面开关、四类组件、三分组播放与音量静音 |

## 未决与边界

- 7 张表的最终字段清单由 `P0-011` 冻结；本批次不承诺字段稳定性。
- 音频音量与静音的持久化由 `P0-009`（b03）存档服务承担，本批次仅内存态。
- UI 音的最终分组数量由 ART 路线音频预算确定，本批次只增补一个 `UISound`。
- 页面壳的导航栈与转场表现推迟到 b10，避免与 G1 盘面 UI、G2 准备页重复实现。
