# 通用 UI 列表行：调查与决策检查点

## 已确认

- 用户要求横向列表行统一使用 SHR-005 图片，减少重复 Item 预制体；初始化/绑定参数控制节点显示。
- 第 1 轮范围决策：检查并统一以下 16 类，包含对话选项和奖励选项；保留各自业务交互。
- 第 2 轮实现方式决策：单预制体＋通用显示脚本；页面传数据、显隐参数和回调。迁移后删除被替代的旧 Item 资源与专用显示脚本。
- 第 3 轮布局决策：单行高 64、显示副文本时高 96；隐藏节点释放空间，长文本省略显示并保留详情交互。
- 保留特殊结构：WorkbenchHostItem、MapNodeItem、InvestigationHotspotItem、SaveSlotItem、CarryAvailableItem。
- 按页面逐项迁移、核对引用和行为，沿用此前用户要求，不采用未经审阅的全局资源重写。

## 约束与验收

- 保留原有详情、执行操作、工作台选择、对话选择和奖励领取语义，不在通用组件内增加业务判断。
- 编译、引用检查及逐页面运行日志验收，不截图。
- 三轮关键决策已经收敛；本变更已按下面的检查点实施，最终状态见 tasks.md 与 verification.md。

## 16 类资源与使用位置

| 资源 | 页面 | 当前背景 | 当前根节点高度 |
|---|---|---|---|
| ArchiveDisplayItem | ArchivePage | SHR-003 | 96 |
| ArchiveOwnedItem | ArchivePage | SHR-005 | 96 |
| ArchiveMaterialItem | ArchivePage | SHR-005 | 96 |
| ArchiveBlueprintItem | ArchivePage | SHR-005 | 96 |
| ArchiveSummaryItem | ArchivePage | SHR-003 | 96 |
| CodexPartItem | CodexPage | SHR-005 | 96 |
| CodexFormItem | CodexPage | SHR-005 | 96 |
| CodexCaseItem | CodexPage | SHR-003 | 96 |
| WorkbenchFormItem | WorkbenchPage | SHR-005 | 76 |
| WorkbenchMaterialItem | WorkbenchPage | SHR-005 | 76 |
| WorkbenchLedgerItem | WorkbenchPage | SHR-005 | 76 |
| GuestDelegationItem | GuestPage | SHR-005 | 56 |
| JournalItem | JournalPage | SHR-005 | 96 |
| MapEventItem | MapPage | SHR-005 | 96 |
| DialogueChoiceItem | BoardPage | SHR-003 | 96 |
| RewardChoiceItem | SettlementPage | SHR-003 | 76 |

此表来自当前 `UI/Item` 下 21 个预制体及页面模板 GUID 引用的只读检查；实现前继续核对代码、其他资源引用与注册表，不把本表当成完整迁移验收。

## 实现必须处理的现有约束

- `UIFormBase.GetItemPoolId` 当前以页面实例和模板实例生成池名，池类型还由 `UIItemObject` 泛型决定。
- `UnspawnAllChildItem<T>(template)` 回收整个对应池，不区分列表容器。Archive 同页五列表合并模板/类型后，现有逐列表清理会影响其他列表，必须调整条目持有和回收。
- 对象池复用要求每次 Bind 完整重置文本、颜色、图标、节点显隐、选中/禁用状态、详情与操作回调；只在首次初始化隐藏节点不够。
- Journal 和 Guest 含独立 Action 按钮；根行详情点击与操作按钮点击需保留为独立语义。
- WorkbenchHostItem 当前与三个 Workbench 列表行共用 WorkbenchItem 逻辑；迁移列表行时不能误删宿主仍需的组件。
- Codex 的 Id、State、Source、StateFrame，Archive 的 Value，以及 Journal/Guest 的 Action 是显隐参数的候选输入，具体契约待后续决策。
- SHR-005 当前只有 normal、hover、selected 三张状态图片，没有独立 pressed/disabled；现有 pressed 复用 selected，disabled 复用 normal。
- 资源生成器、引用绑定器、验收工具须同步迁移，避免重新生成旧 Item 或重新绑定旧类型。

## 当前阶段

实现与验收完成，全部证据见 verification.md。

## 全量引用检查点

- 资源：通过旧 16 个 Prefab GUID 检索全部 Assets 下 prefab、unity、asset、json、txt、csv；通过 AssetDatabase 递归依赖检查所有 74 个序列化资源。
- 正式使用页面：ArchivePage（5）、CodexPage（3）、WorkbenchPage（3）、GuestPage、JournalPage、MapPage、BoardPage、SettlementPage。对话旧兼容组件 DialoguePanel 同步使用通用行；正式 DialoguePage 的 DialogView 固定确认按钮不属于列表行模板。
- 额外引用：用户保存的 Assets/HomePage(Clone).prefab 快照包含旧模板与 3 个已保存的运行时 GuestDelegationItem(Clone)。通过 PrefabUtility 更新模板并清除这 3 个临时克隆，保留快照布局及其他节点。
- 表、注册：UITable 注册的是页面而非 Item，无需修改 Item 注册；资源目录由正式 SpriteCatalog 供给图标，通用行使用直接序列化的 SHR-005 状态图。
- Editor：EverlightUiItemPrefabGenerator、HomeSubPanelsContractMigration、UiItemContractMigration、UiItemLayoutNormalizer、EverlightFormalUiResourceBinder、UiLayoutPrefabMigration、MapPanelContractMigration、WorkbenchPageContractMigration 已同步。旧专用行构造器已移除，页面生成菜单都取得 CommonRow；特殊 Item 构造器保留。
- 清理映射：16 个旧 Prefab 均映射到 UI/Item/ListRowItem.prefab；ArchiveItem、CodexItem、GuestDelegationItem、JournalItem、MapEventItem、DialogueChoiceItem、RewardChoiceItem 映射到 ListRowItem / ListRowItemObject 并删除。WorkbenchItem / WorkbenchItemObject 继续服务 WorkbenchHostItem。
