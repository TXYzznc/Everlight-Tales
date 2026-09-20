# 实现任务（b02 数据＋UI＋音频骨架）

覆盖原始任务：P0-005、P0-006、P0-007、P0-008。

前置（本批次开工前已完成并验证，不计入本批次任务）：重建 5 张 Core 表的 Excel 源，恢复表管线的可重跑性。证据见 `Docs/Framework/Development/Dispatch/Active/b02-datatable-excel-source.md`。

## 1. 首批表骨架与生成链路（P0-005）

- [x] 1.1 新建 6 张业务表的 Excel 源：`Board/PartTable`、`Board/LevelTable`、`Board/BuffTable`、`Meta/EventTable`、`Meta/MaterialTable`、`Meta/QuestTable`；语言链复用既有 `Core/LanguagesTable`，不新建
- [x] 1.2 每表只落最小字段（ID + 名称 + 1～2 个已确认字段），类型限定 `int`／`string`／`bool`／`float`
- [x] 1.3 每表第 4 行写满宽中文备注，确认生成的 `.cs` 中每个属性带有匹配的 XML 文档注释
- [x] 1.4 经框架生成器导出 6 张表的 `.txt` 与 `.cs`，确认生成物路径与 `design.md` D1 表一致
- [x] 1.5 在 `AppConfigs.asset` 的 `mDataTables` 登记 6 张表（不含扩展名的相对路径），既有 5 项与顺序不变
- [x] 1.6 提供业务侧数据表访问入口（项目侧薄封装），业务代码不直接持有框架数据表组件实例
- [x] 1.7 以 `Board/PartTable` 走完整端到端验证：Excel → `.txt` → `.cs` → 注册 → 运行时加载并读到已配置行
- [x] 1.8 复跑生成管线两轮，确认第二轮无文件差异（管线可重跑且收敛）

## 2. 通用页面壳（P0-006）

- [x] 2.1 建立竖屏页面壳 UIForm（基于框架 `UIFormBase`），基准 1080×1920，锚点适配其它竖屏比例
- [x] 2.2 安全区适配：顶部与底部内容避开不安全区，返回按钮与页签落在安全区内
- [x] 2.3 返回入口：复用 `UIParams.AllowEscapeClose` 与 `UIFormBase.CanCloseByInputModule`，走框架关闭流程
- [x] 2.4 页签：可切换且当前选中项有可辨识表现
- [x] 2.5 登记页面壳到 `GameData/DataTables/Core/UITable.xlsx`，重新生成 `Assets/Game/Scripts/UI/Core/UIViews.cs`，确认枚举包含该页面
- [x] 2.6 分组归属：挂载到预期 UI 分组，渲染层级与 `UIGroupTable` 的 `Depth` 一致
- [x] 2.7 反复开关验证：关闭后实例与 item 池被释放，场景无残留

## 3. 全局弹窗 / Toast / 加载 / 确认（P0-007）

- [x] 3.1 项目侧统一入口，调用方不引用框架组件具体类型
- [x] 3.2 弹窗：基于 `UIFormBase` + `UIDialogTemplate`，经 UI 生成链路产出，挂 `Dialog(200)`
- [x] 3.3 确认：同上，结果经 `UIParams.ButtonClickCallback` 回传可区分的确认／取消
- [x] 3.4 Toast：项目侧自建（不派生 `UIFormBase`），带排队与池化，挂 `Overlay(500)`
- [x] 3.5 加载：项目侧自建，全屏屏蔽下层交互，挂 `Overlay(500)`
- [x] 3.6 验证 Toast 显示在弹窗之上、加载遮挡下层交互
- [x] 3.7 按钮反馈：复用 `ClickUIButton` 与可覆写的 `PlayClickSound`，补齐连点不重复触发保护
- [x] 3.8 重复与并发调用验证：实例不泄漏，同帧多个 Toast 全部展示且顺序确定

## 4. 音频分组与音量静音（P0-008）

- [x] 4.1 在 `SoundGroupTable` 增补 `UISound` 分组，音乐与音效沿用框架既有分组
- [x] 4.2 接入音乐／音效／UI 音三分组的播放通路
- [x] 4.3 音量与静音设置入口，设置立即影响对应分组的实际输出且不影响其它分组
- [x] 4.4 用占位音验证通路，不依赖正式音频素材
- [x] 4.5 登记持久化边界：本批次仅内存态，跨启动保留留给存档服务批次

## 5. 验证

- [x] 5.1 `python tools/audit_framework_purity.py` 通过
- [x] 5.2 `python tools/verify_project_assemblies.py` 通过，未破坏 b01 的五层布局与引用方向
- [x] 5.3 Unity 编译通过（退出码 0，无 `error CS`）
- [x] 5.4 表管线往返：重跑生成后 `git diff -- Assets/Game/DataTable Assets/Game/Scripts/DataTable` 为空
- [x] 5.5 `python tools/datatable_excel_source.py --check` 与 `python tools/check_datatable_schema.py` 均通过；后者守住"备注行满宽、字段与备注逐列对齐"，避免注释整体错位
- [x] 5.6 Play Mode 走查：业务场景内表加载可读到数据、页面壳可开可关、四类全局组件可拉起、三分组可播放且音量静音生效
- [x] 5.7 记录本批次验证证据（命令、退出码、关键日志），交回传
