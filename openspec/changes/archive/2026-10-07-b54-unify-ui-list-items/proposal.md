## Why

当前横向列表行按业务复制成多份 Item 预制体；16 类条目主要差异是文本、图标、可见字段与回调，而非独立的视觉结构。重复资产造成布局、按钮状态、引用契约和生成工具反复修补，部分列表行还混用 SHR-003 卡片与 SHR-005 列表行背景。

## What Changes

- 建立一个 `ListRowItem.prefab` 和通用 `ListRowItem` / `ListRowItemObject`，所有纳入范围的页面使用同一正式资产。
- 根行背景及 Sprite Swap 状态统一 SHR-005。单行高 64；显示副文本时高 96；隐藏字段释放布局空间。
- 通过一份绑定参数传入标题、副文本、编号、图标、可选状态框、右侧文本、操作文字、显隐、选中/禁用状态与两个点击回调。
- 页面持有业务数据与行为，通用组件仅显示数据和发出回调；完整重置对象池复用状态。
- 建立按容器持有和回收条目的方式，防止多个列表共享一个模板时互相回收。
- 逐页面迁移 16 类条目、更新 Prefab 引用及 Editor 生成/绑定/验证工具，再删除无引用旧资产和仅服务旧行的专用脚本。

## Capabilities

### New Capabilities

- `common-ui-list-row`：统一列表行外观、字段绑定、布局、交互及对象池隔离。

### Modified Capabilities

- 页面及 Item View 契约沿用现有显式序列化引用原则；新增通用行契约，不改变世界数据、存档或业务规则。

## Impact

- 16 类条目清单与页面映射见 `discovery.md`。
- 影响 ArchivePage、CodexPage、WorkbenchPage、GuestPage、JournalPage、MapPage、BoardPage、SettlementPage 的行模板引用和调用脚本。
- 影响 `EverlightUiItemPrefabGenerator`、Item/页面绑定器、深度审计和验证入口，以及其他扫描发现的模板注册与资源清单。
- 若需要公开已有逐实例回收能力，仅在 `Assets/Game/Scripts/UI/Core/UIFormBase.cs` 的通用扩展中增加对现有回收方法的薄封装，不修改 `ScriptsBuiltin` 框架核心。

## Non-Goals

- 不合并 WorkbenchHostItem、MapNodeItem、InvestigationHotspotItem、SaveSlotItem、CarryAvailableItem。
- 不改变页面容器、美术导航、详情面板、业务流程、奖励规则和数据模型。
- 不在页面中继续保留 16 个 Prefab Variant；目标是一个正式列表行资源。
- 不使用未经逐项审阅的全局资源重写，不通过运行时节点名称查找补偿缺失引用。
