## 1. 调查与决策

- [x] 1.1 盘点 UI/Item 下 21 个资源及直接页面引用。
- [x] 1.2 三轮确认范围、组件共用方式和 64/96 两档布局，记录检查点。
- [x] 1.3 核实对象池回收、多容器、操作按钮、宿主共用脚本等迁移风险。
- [x] 1.4 扩展核对所有资源引用、类型调用、注册表和 Editor 工具，建立完整旧→新映射。

## 2. 通用组件与回收

- [x] 2.1 实现 ListRowData、ListRowItem 和 ListRowItemObject，显式绑定可选节点及两个点击语义。
- [x] 2.2 创建 ListRowItem.prefab，使用 SHR-005 Sprite Swap，隐藏节点释放空间，行高 64/96。
- [x] 2.3 实现容器级行持有与逐实例回收；如需公开 API，仅封装通用 UIFormBase 中已有能力。
- [x] 2.4 验证完整 Bind 重置、真正回收释放、关闭分类保留回调，以及关闭页面重开不会重复回收。

## 3. 逐页面、逐类型迁移

- [x] 3.1 ArchiveDisplayItem：名称、副文本、详情点击。
- [x] 3.2 ArchiveOwnedItem：名称、副文本、右侧数值/拥有状态及详情。
- [x] 3.3 ArchiveMaterialItem：图标、数量、缺库存颜色及详情。
- [x] 3.4 ArchiveBlueprintItem：图样名称、解锁状态及详情。
- [x] 3.5 ArchiveSummaryItem：汇总数量与详情；验证五类共模板且回收互不影响。
- [x] 3.6 CodexPartItem：编号、图标、可选状态框、已知/未知状态及点击。
- [x] 3.7 CodexFormItem：形态字段、图标、状态框和点击。
- [x] 3.8 CodexCaseItem：怪谈字段和详情，不显示状态框。
- [x] 3.9 WorkbenchFormItem：持久选择、可加工/锁定状态；保留六边形宿主。
- [x] 3.10 WorkbenchMaterialItem：列表选择刷新原有详情面板。
- [x] 3.11 WorkbenchLedgerItem：方向、金额、渠道与原因。
- [x] 3.12 GuestDelegationItem：来客各区域数据、根详情和独立操作按钮。
- [x] 3.13 JournalItem：三个分类与各状态分组，根详情和独立执行操作。
- [x] 3.14 MapEventItem：地图事件数据与原回调。
- [x] 3.15 DialogueChoiceItem：对话选项及原选择行为。
- [x] 3.16 RewardChoiceItem：奖励展示、领取和防重复领取行为。

每个条目完成页面参数转换、Prefab 引用更新、编译和运行日志核对后才标记完成，不按资源名称一次性改写全部页面。

## 4. 工具与旧资源清理

- [x] 4.1 更新生成器、Item/页面绑定器、资源注册和运行验收工具；不得重建旧行资源。
- [x] 4.2 全量检查旧 GUID、路径、类型和字段引用，逐个删除 16 个旧 Prefab 与 meta。
- [x] 4.3 删除仅服务被替代列表行的旧显示脚本；保留 WorkbenchHostItem 仍需逻辑。
- [x] 4.4 验证 5 类特殊 Item 及相关页面未受影响。

## 5. 最终验收

- [x] 5.1 验证同模板多容器隔离、跨用途复用重置、分类切换及页面重开。
- [x] 5.2 逐页面记录行数、分类、按钮状态、点击回调和控制台异常。
- [x] 5.3 完成编译、Prefabs 引用审计与 diff 检查，更新全部迁移证据。
- [x] 5.4 所有实现与验收完成后按 OpenSpec 流程同步和归档。
