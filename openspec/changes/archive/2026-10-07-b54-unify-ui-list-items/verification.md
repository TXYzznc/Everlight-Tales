# 实施与验收记录

## 基础组件

- ListRowItem / ListRowData / ListRowItemObject / ListRowCollection 编译通过。
- ListRowItem.prefab 已通过 PrefabUtility 创建，保留全部可选结构，内部引用显式序列化。
- Play Mode 日志：`[ListRow][Validation] PASS: heights=64/96; optionalFieldsReset; dualCallbacks; hideShow; containerIsolation; staleLeaseAfterClose.`
- 验证使用临时 UIForm 与真正 GF 对象池，检查旧回调重置、隐藏重显保留回调、A 清理不影响 B，以及页面全量回收后旧记录不回收新持有代次。

## ArchivePage（5 类）

- 使用单一 `_rowItemTemplate` 与五个容器级条目集合，原数据、详情行为和用户维护的容器布局保留。
- 编译通过，页面引用通过 PrefabUtility 迁移。
- Play Mode 日志：`[ListRow][Archive] PASS counts=4,13,6,1,2; fiveTabs; repeatedRefresh; independentRebuild; interactions.`
- 确认两次完整刷新、五分类切换、单独重建拥有物均保持各容器正确数量，活跃行可交互。
- 完整控制台结果保存在本地 `Library/ListRowValidation/archive-log.json`，未截图。

## CodexPage（3 类）

- 单一行模板，三个独立持有集合；编号、来源、状态、图标、状态框与已知/未知交互条件由参数提供。
- 编译和引用迁移通过。
- Play Mode 日志：`[ListRow][Codex] PASS counts=20,15,5; threeTabs; repeatedSwitch; caseFrameHidden.`
- 完整控制台结果保存在本地 `Library/ListRowValidation/codex-log.json`。

## WorkbenchPage（3 类）

- 形态、材料、账目使用单模板和独立持有集合；六边形宿主保持原 WorkbenchItem 组件。
- 编译与页面模板引用迁移通过。
- Play Mode 日志：`[ListRow][Workbench] PASS forms=4; materials=6; ledger=3; repeatedTabs; hexHosts; materialClickAndDetail.`
- 实际调用第二条材料的点击事件，确认选择键及原详情面板文字更新。
- 完整控制台结果保存在本地 `Library/ListRowValidation/workbench-log.json`。

## Guest / Journal / Map / Choices / Reward

- Guest：`PASS counts=4,6,5; repeatedRefresh; rootAndAction; navigationCallback.`
- Journal：`PASS groupedCounts=6,0,0 / 2,2,1 / 4,1,0; threeTabs; repeatedSwitch; detailCallbacks.`
- Map：`PASS places=5; events=7; repeatedPlaceSelection; eventCallbacks; retainedMapNodes.`
- Choices：`PASS formOptions=27; allHosts; closeCallback; dialogueCompletion.` 同时覆盖盘面各宿主的形态选择器及旧对话兼容组件。
- Reward：`PASS fourChoices; repeatedBuild; applyOnce; duplicateGuard; noFormalSaveWritten.` 使用隔离的临时存档位 542，保存后删除临时验证键，未改写正式存档。重开 OnOpen 重新初始化 m_Finished。
- 各页完整 JSON 日志位于本地 Library/ListRowValidation/*-log.json；未截图。

## 引用与清理

- 清理前 AssetDatabase 依赖审计：74 个资源，旧行依赖 0，正式 UI 缺失脚本 0，通用行 12 个序列化引用均完整，按钮 SpriteSwap。
- 删除 16 个旧 Prefab 及 meta，删除 7 个旧显示脚本及 meta；保留 WorkbenchItem 宿主共用逻辑。
- 全 Assets 文本 GUID 检索：旧 GUID 引用 0；Item 目录只保留 1 个通用行和 5 个特殊结构 Item。
- 5 个特殊 Item 的 SHA-256 与本次清理前一致。
- 生成器公共构造路径实际调用返回 ListRowItem.prefab，不创建旧资源；旧布局批量同步工具改为只检查通用行。
- `python tools/audit_framework_purity.py`：通过。

## 最终综合验收

资源清理后最终 Play Mode 验收完成。

- `[ListRow][Final] PASS 8 pages x 2 real open/close cycles; recycledRows; callbacks; 5 special items retained; noScreenshots.`
- Archive、Codex、Workbench、Guest、Journal、Map、Board、Settlement 每页均有两条真实 Reopen PASS，包含回收后复用、分类切换、各业务点击回调和奖励单次保护。
- Special：SaveSlotPage count=3；PreparationPage count=12；InvestigationPage count=2，并实际触发调查热点点击回调。六边形宿主与静态地图节点在各自页面验证中检查。
- 基础复用验收额外断言隐藏重显保留选中 Sprite、复用纯标题行时清除旧选中 Sprite，均通过。
- 完整最终日志：Library/ListRowValidation/final-log.json，Error/Exception=0；脚本自动核对了 8×2 的 Reopen 标记。
- 最终资产审计：58 个序列化资源，旧 Prefab 依赖 0，缺失 UI 脚本 0，通用引用和 SpriteSwap 通过。
- 实际执行 Archive/Codex/Workbench 生成接入菜单，以及 Guest、Map、Journal 各自接入菜单：均 templateReferences=0，保持通用模板且不改页面布局。生成器检查确认未再生成旧资源。
- 针对本次改动文件的 git diff --check 通过，Unity 编译通过，框架纯度审计通过。
- 验收后退出 Play Mode、关闭 UIValidationHarness 的验证数据开关；未截图、未重启 Unity、未提交或推送 Git。

### OpenSpec 验证结论

- 完整性：16 类迁移、8 页行为、5 类特殊结构、7 个旧显示脚本与相关工具均覆盖。
- 正确性：条目数量、真实页面重开、跨容器隔离、复用清空、独立操作及领取保护通过运行断言；旧 GUID 与依赖已清零。
- 连贯性：一个正式 SHR-005 行资源，业务只留在页面回调，64/96 参数布局；页面资产经 PrefabUtility 操作，特殊 Item 资源未改动。
- 无待解决的 CRITICAL/WARNING；视觉验收遵循用户约束，本次未截图。

### 验收发现的现有绑定遗漏

InvestigationPage 的 InvestigationView.m_Form 原为 null，生成热点所需的 GF UIForm 不可用。SimplePageContractMigration 补齐对 InvestigationPageForm 的显式引用，使用 PrefabUtility 写回预制体；不修改 InvestigationHotspotItem 的结构或显示脚本。

### 工具收敛

相关生成/修复菜单不再重建 Archive/Codex/Workbench 的容器坐标，统一为创建通用行并写入模板引用；避免旧生成器覆盖用户维护的布局。过时的页面重建函数、专用行构造器与批量节点同步函数已移除。仍保留的特殊结构生成函数不参与本次调用。

## 归档

同步 common-ui-list-row 主规范，所有任务完成。变更归档到 openspec/changes/archive/2026-10-07-b54-unify-ui-list-items。当前环境未提供 OpenSpec CLI，按项目 spec-driven 目录约定直接同步文档并归档，未冒充运行 CLI validate。
