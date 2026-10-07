# Common UI List Row

## Purpose

以一个共享列表行资源和显式参数绑定统一横向列表显示，保证对象池复用、分类切换和页面重开时的数据与交互一致。

## Requirements

### Requirement: One shared row asset for the agreed scope

系统 SHALL 使用一个正式通用列表行预制体和通用显示组件承载已确认的 16 类横向列表行，页面 SHALL 传入显示参数和业务回调。

#### Scenario: Different pages bind the same row asset

- **WHEN** Archive、Codex 或其他纳入范围页面创建列表行
- **THEN** 它们引用同一个通用列表行预制体，不依赖独立业务行预制体或其 Variant
- **AND** WorkbenchHostItem、MapNodeItem、InvestigationHotspotItem、SaveSlotItem、CarryAvailableItem 保留特殊结构

### Requirement: SHR-005 visuals and two layout heights

通用行 SHALL 使用 SHR-005 根背景和 Sprite Swap 状态图片，隐藏字段 SHALL 不占用布局空间；单行高度 SHALL 为 64，显示非空副文本时 SHALL 为 96。

#### Scenario: A title-only row is bound

- **WHEN** 页面仅启用标题且无副文本
- **THEN** 行高为 64，其他可选字段隐藏且释放宽度，标题使用剩余可用空间

#### Scenario: A row with secondary text is bound

- **WHEN** 页面启用并提供非空副文本
- **THEN** 行高为 96，主副文本均单行省略显示，页面原详情回调仍可展示完整内容

### Requirement: Separate row and action interactions

通用行 SHALL 支持独立的行点击和可选操作按钮回调，且 SHALL 不包含世界数据查询或业务流程判断。

#### Scenario: A journal row has two actions

- **WHEN** 用户点击条目主体或右侧操作按钮
- **THEN** 分别调用页面注入的详情回调或执行回调，互不覆盖

### Requirement: Complete state reset on rebind and actual recycle

通用行 SHALL 在每次 Bind 时重置全部可选节点、内容、状态、布局和监听，在真正回收时释放业务回调引用；分类隐藏 SHALL 不清除仍有效的回调。

#### Scenario: A recycled rich row becomes a simple row

- **WHEN** 带编号、图标、数值和 Action 的行回收后被重新绑定为纯标题行
- **THEN** 不显示旧字段、旧图标或旧选中状态，不调用旧回调

#### Scenario: A classification is hidden and shown again

- **WHEN** 页面切换分类而未真正回收其中的条目
- **THEN** 再次显示时原点击回调仍有效

### Requirement: Container-scoped recycling with shared templates

系统 SHALL 在多个列表容器共享模板和 ItemObject 类型时仅回收目标容器持有的实例，并正确处理页面关闭后的全量池回收与重开。

#### Scenario: One Archive classification refreshes

- **WHEN** ArchivePage 五类共用一个模板并重建其中一类
- **THEN** 其他四类条目的活跃数量、显示数据和点击行为保持正确

#### Scenario: A page closes and reopens

- **WHEN** 框架已在关闭页面时回收全部行，随后页面再次打开
- **THEN** 容器持有记录与池状态一致，不重复回收旧实例，不回收其他容器重新持有的实例

### Requirement: Migration coverage and regeneration consistency

迁移 SHALL 逐页面覆盖全部 16 类、更新相关生成绑定验证工具，并在确认无引用后删除旧资源及仅服务旧行的脚本。

#### Scenario: Editor tools run after migration

- **WHEN** 运行正式生成或绑定工具
- **THEN** 使用新的通用列表行契约，不重新创建旧业务行预制体或写回旧组件引用

#### Scenario: Final migration audit

- **WHEN** 宣告迁移完成
- **THEN** 16 类均有引用与运行日志验收证据，旧资源引用无遗留，5 类特殊 Item 未误删，静态核对不会被冒充为运行验收
