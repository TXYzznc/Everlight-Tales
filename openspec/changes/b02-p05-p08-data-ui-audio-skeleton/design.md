## Context

b01 已建立五层业务程序集与输入抽象。G0 剩余的基础设施有三条链，任务表把它们打包为 P0-005～P0-008，并明确共同验收边界是"页面壳与表加载在业务场景内可用"。

三条链的当前事实（已实地探明，非假设）：

| 链 | 现状 |
|---|---|
| 数据 | `GameData/DataTables/` 曾有目录无内容；`.txt` 与 `.cs` 生成物已提交但 Excel 源缺失，管线不可重跑。**已在本批次开工前修复**（重建 5 张 Core 表 Excel 源，框架往返收敛） |
| UI | `Assets/Game/DataTable/Core/UITable.txt` 当前为空表（0 条），生成的 `Assets/Game/Scripts/UI/Core/UIViews.cs` 是空枚举，说明项目还没有任何注册过的界面；`UIGroupTable.txt` 已有 `Default(1)`／`Dialog(200)`／`Overlay(500)` 三个分组 |
| 音频 | `SoundGroupTable.txt` 已有 `Music(1)`／`Sound(4)` 两个框架默认分组，尚无 UI 音分组 |

UI 侧构件的实际清点（决定了 D5 的实现方式）：

| 构件 | 位置 | 用途 |
|---|---|---|
| `UIFormBase` | `Assets/Game/Scripts/UI/Core/` | 页面基类，已含开合动画、子界面管理、item 池、`UIParams`、返回键关闭、按钮派发与点击音 |
| `UIItemBase` / `UIItemObject` | `Assets/Game/Scripts/UI/Core/` | 列表项基类与池化对象 |
| `UIParams` | `Assets/Game/Scripts/UI/Core/` | `AllowEscapeClose`、`SortOrder`、`OpenCallback`、`CloseCallback`、`ButtonClickCallback` |
| `UIExtension.OpenUIForm(UIViews, UIParams)` | `Assets/Game/Scripts/Extension/` | 按枚举打开界面，返回界面 ID |
| `UIFormTemplate.prefab` / `UIItemTemplate.prefab` / `UIDialogTemplate.prefab` | `Assets/Game/ScriptsBuiltin/Editor/UI/Templates/` | UI 生成链路的三份模板 |
| `UIViews` 枚举 | `Assets/Game/Scripts/UI/Core/UIViews.cs` | 由 `UITable` 生成，当前为空 |
| Toast / Loading 组件 | —— | **不存在**，无任何现成实现 |

表管线的事实链（据生成器源码与实测标定）：

- Excel 源：`GameData/DataTables/<相对路径>.xlsx`
- 表数据生成物：`Assets/Game/DataTable/<相对路径>.txt`
- 代码生成物：`Assets/Game/Scripts/DataTable/<相对路径>.cs`
- 运行时注册：`AppConfigs.asset` 的 `mDataTables` 列表，元素是**不含扩展名的相对路径**（如 `Core/UIGroupTable`）
- Excel 行约定：第 1 行 A=`#`、B=逻辑表名；第 2 行字段名；第 3 行类型；第 4 行备注；第 5 行起数据。第 4 行的注释按**原列号**一一对应到字段，因此该行必须满宽。

## Goals / Non-Goals

**Goals**

- 7 张业务表骨架可用，`Excel → .txt → 生成 C# → AppConfigs 注册 → 运行时加载`全链打通，且管线可重跑（重跑后 `git diff` 为空）。
- 竖屏 1080×1920 页面壳可开可关，安全区与分组正确。
- 弹窗／Toast／加载／确认四类组件可复用，层级正确。
- 音乐／音效／UI 音三分组可播放，音量与静音设置生效。
- 框架纯度审计在本批次后仍然通过。

**Non-Goals**

- 不冻结 7 张表的最终字段清单（`P0-011`）。
- 不实现任何盘面、玩法、关卡或事件逻辑（G1 起）。
- 不制作正式 UI 视觉稿与音频素材（ART 路线）。
- 不实现存档、随机源与诊断开关（b03）。
- 不修改 `ScriptsBuiltin` 与既有框架 Procedure 顺序。

## Decisions

### D1：7 张表的目录与命名

表放在 `GameData/DataTables/` 下按领域分子目录，与已有 `Core/` 同级：

| 表 | 相对路径 | 生成类名 |
|---|---|---|
| 事件 | `Meta/EventTable` | `EventTable` |
| 零件 | `Board/PartTable` | `PartTable` |
| 关卡 | `Board/LevelTable` | `LevelTable` |
| Buff | `Board/BuffTable` | `BuffTable` |
| 材料 | `Meta/MaterialTable` | `MaterialTable` |
| 任务 | `Meta/QuestTable` | `QuestTable` |
| 语言 | `Core/LanguagesTable` 之外的业务语言表暂不新建，复用既有 `Core/LanguagesTable` |

表名与类名统一 **PascalCase 英文 + `Table` 后缀**，与既有 `Core/UIGroupTable`、`Core/SoundGroupTable` 完全一致（生成器用文件名派生类名，风格必须统一）。表名**不带业务前缀**：前缀会污染生成的类名（`EVT_EventTable`），且领域已由目录表达。

修订说明：初版曾考虑 `语言` 单列第 7 张表，但 `Core/LanguagesTable` 已存在且承担运行时语言选择，重复建表会制造两个真相来源。第 7 张改为**任务表 `Meta/QuestTable`**，语言链由既有表覆盖──任务表原文列出的"语言"正是这张既有表，不新建。

### D2：字段冻结尺度（本批次只落骨架）

每张表只落 **ID + 名称 + 1～2 个已确认字段**，具体：

- 字段类型一律用已确认的基础类型（`int` / `string` / `bool` / `float`），不引入枚举与自定义 JSON 类型。
- 字段注释行（Excel 第 4 行）写中文说明，让生成的 `.cs` 具备可读文档。
- 字段最终清单与语义由 `P0-011` 冻结，本批次**不**承诺字段稳定性。

理由：`P0-011` 本身就是"配置表字段冻结"任务且以 `P0-005` 为前置，现在冻结全部列会让 G1 必然返工──P1-021（关卡盘面配置表）与 P2-005（Buff 库 20 条）都会大幅扩张字段。骨架阶段证明"链路通"即可。

### D3：7 张表全部建骨，其中 1 张走完整端到端验证

7 张表都建 Excel 源与骨架字段，但**只有 `Board/PartTable` 走完整验证**：生成 `.txt`、生成 `.cs`、写入 `AppConfigs.mDataTables`、并在运行时实际加载读取一行。

理由：任务表 DoD 原文是"**一张**示例表走通生成与运行时加载"。七张全走端到端会让本批次工时冲向 32 小时上限；而只建一张又无法验证"多子目录 + 多表注册"的批量路径。骨架全建 + 单表验证是同时满足两者成本最低的组合。

### D4：页面壳的完成度边界

本批次页面壳只做：**空页面壳可开可关 + 1080×1920 基准 + 安全区适配 + 返回 + 页签 + UIGroup 归属正确**。

导航栈（页面历史、返回拦截链）、页签内容切换动画、页面转场、页面与业务数据的绑定**不在本批次**，推迟到 b10（UI 框架批次）与各业务页面批次。

理由：本批次要验证的是"GF.UI 的分组与 `UIFormBase`／`UIItemBase` 用法正确"，不是导航能力。导航栈与转场会被 G1 的盘面 UI 与 G2 的准备页实际需求驱动定型，现在做会与它们重复实现。

### D5：全局组件走项目侧封装；弹窗复用框架、Toast 与加载自建

**框架侧没有 Toast 与加载组件。** UI 生成链路只提供三份模板（`UIFormTemplate`、`UIItemTemplate`、`UIDialogTemplate`），没有任何 Toast／Loading 实现。因此四类组件分两种实现基础：

| 组件 | 实现基础 | 分组 |
|---|---|---|
| 弹窗 | 基于 `UIFormBase`，用 `UIDialogTemplate` 经 UI 生成链路产出 | `Dialog(200)` |
| 确认 | 同上，按钮结果走 `UIParams.ButtonClickCallback` | `Dialog(200)` |
| Toast | **项目侧自建**：不派生 `UIFormBase`，自行排队与池化 | `Overlay(500)` |
| 加载 | **项目侧自建**：不派生 `UIFormBase`，带全屏交互屏蔽 | `Overlay(500)` |

四者统一由项目侧薄封装入口暴露，调用方不引用框架组件具体类型。

选择理由：弹窗与确认天然是"一次一个、有模态语义"的界面，框架的 `UIFormBase` 已提供开合动画、子界面与子界面排序、返回键关闭与按钮派发，复用它们成本最低。而 Toast 是**高频、可并发、无模态**的临时提示，套用 `UIFormBase` 会强制走界面栈与开合动画，既浪费也难做排队；加载同理。这两者自建反而更简单。

替代方案「Toast 也做成 UIForm」被放弃：会让 Toast 进入界面栈、与模态语义混淆，且并发请求需要绕过界面栈去重规则。

### D5b：页面壳必须先进入 UI 生成链路

页面壳是本项目**第一个**注册界面，`UIViews` 枚举当前为空，因此本批次必须同时走通"登记 `UITable` → 重新生成 `UIViews` → 按枚举打开"这条路径，而不能只做一个孤立预制体。

配套事实：`UIParams.AllowEscapeClose` 与 `UIFormBase.CanCloseByInputModule` 已支持返回键关闭，页面壳的返回能力应复用它们，不另建返回链路。导航栈与转场仍不在本批次（见 D4）。

### D6：音频先做通路，素材留空

`GF.Sound` 分支接入：在 `SoundGroupTable` 增补 `UISound` 分组；音量与静音设置以占位实现（读写 `GF.Sound` 的 group volume 与 mute，持久化留 b03 存档批次）。本批次只验证"分组播放可生效、音量静音可改变输出"。

理由：任务表 DoD 是"分组播放与音量静音生效"，不含素材与持久化；正式素材属 ART 路线预算（b33 起），本批次用占位音或框架自带音验证通路即可。

## Risks / Trade-offs

- **风险**：7 张表骨架在 `P0-011` 冻结字段时被大幅改写，骨架工作部分作废。→ **对策**：骨架只落最小字段且不承诺稳定性，改写成本限制在每表 3～4 列；同时骨架已经验证了多表批量生成路径，这部分价值不随字段变化失效。
- **风险**：直接编辑 `AppConfigs.asset` 的 `mDataTables` 列表出错会破坏表加载。→ **对策**：优先经 Unity Editor 写入；直接编辑时只改纯文本列表项并立即做加载走查；每批次固定跑一次"表加载 + 往返 diff"。
- **风险**：页面壳的安全区适配在编辑器（无刘海）看不出来，真机才发现错位。→ **对策**：在编辑器用 Game 视图的设备模拟（如 iPhone 刘海机型）验证安全区锚点，并把"分组与安全区同时正确"写成可测试场景。
- **风险**：页面壳是本项目第一个注册界面，`UIViews` 枚举为空，UI 生成链路此前从未在本项目跑通过，登记或生成出错会表现为"界面打不开"且不明显。→ **对策**：把"登记 → 生成枚举 → 按枚举打开"写成独立规格场景；失败时先看 `UIViews` 是否包含该项，再看 `UITable` 登记的 `UIPrefab` 名与预制体是否一致。
- **风险**：Toast 与加载自建，容易与框架界面的层级和交互屏蔽语义冲突（例如加载未屏蔽到弹窗上的点击）。→ **对策**：分组与深度以 `UIGroupTable` 为唯一来源，加载按 `Overlay(500)` 全屏屏蔽；把"Toast 覆盖弹窗之上"与"加载屏蔽下层交互"写成可测试场景。
- **风险**：项目侧 UI 封装层与框架能力漂移（框架升级后封装未跟进）。→ **对策**：封装只做转发与分组指定，不复制框架状态机；弹窗与确认直接派生 `UIFormBase` 复用其生命周期，自建部分只限 Toast 与加载。
- **权衡**：本批次把三条链打包在一起，单批次范围偏大（预计 17～32 小时）。保留打包的理由是任务表已把它们标为同一并行组 `A0` 且共同验收边界一致；若开工后发现工时逼近上限，按《开发执行约束》"上限超 18 小时须再次拆分"拆为"数据"与"UI＋音频"两个子批。

## Migration Plan

无迁移：三条链在业务侧均为新增。回滚方式为删除新增的 Excel 源与生成物、还原 `AppConfigs.asset` 的 `mDataTables` 列表、移除 UI 壳与组件、移除音频分组增补。

## Open Questions

- UI 音的正式分组名与数量：本批次只增补一个 `UISound` 分组，最终分组由 ART 路线的音频预算确定。
- 音频音量与静音的持久化载体：与存档服务（`P0-009`，b03）合流，本批次先做内存态。
- 业务语言表是否需要独立于 `Core/LanguagesTable`：`P0-011` 冻结配置字段时一并确认。
