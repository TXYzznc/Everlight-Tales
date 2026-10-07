## 1. 全量清单与审计基线

- [x] 1.1 扫描全部 UI 运行时代码，登记路径查找、全局查找、序列化字段和组件契约。
- [x] 1.2 扫描全部 UI Editor 工具，登记读取/写入层级的逻辑和迁移边界。
- [x] 1.3 扫描全部 UI 页面、弹窗和 Item Prefab，建立逐资源绑定清单。
- [x] 1.4 输出基线审计报告，并确认清单中的资源数量与 AssetDatabase 结果一致。

## 2. 通用契约和验证基础设施

- [x] 2.1 建立页面 View、局部 View、Item View 的引用校验约定。
- [x] 2.2 增加必选/可选依赖诊断和 `OnValidate`/Editor 验证入口。
- [x] 2.3 增加节点重排、缺失必选引用和可选详情缺失的验证场景。

## 3. 页面逐个迁移

- [x] 3.1 迁移 `HomePage`、`WorkbenchPage` 及其 `Panel_Materials`、`Panel_Ledger`。
- [x] 3.2 迁移 `GuestPage`、`ServicePage`、`ArchivePage`、`CodexPage`。
- [x] 3.3 迁移 `JournalPage`、`MapPage`、`InvestigationPage`。
- [x] 3.4 迁移 `BoardPage`、`PreparationPage`、`SettlementPage`、`RecoveryPage`。
- [x] 3.5 迁移 `DialoguePage`、`DialogView`、`FeedbackPage`、`Settings`、`SaveSlotPage`、`ProloguePage`、`MainPageShell` 和其余 UI 页面。

## 4. Item 和 Editor 工具逐个迁移

- [x] 4.1 逐个迁移全部 `UI/Item` Prefab 与对应 Item 组件的内部引用。
- [x] 4.2 修改资源绑定器、生成器、布局迁移器，使其写入序列化引用并提供明确错误。
- [x] 4.3 修改深度审计、交互审计、单页审查和验证入口，覆盖新契约。

## 5. 逐界面验证

- [x] 5.1 每个页面完成编译和 Prefab 校验后运行独立烟雾测试。
- [x] 5.2 验证节点移动后绑定继续有效。
- [x] 5.3 验证缺失必选引用时能准确失败，缺失可选字段时不阻断其他功能。
- [x] 5.4 运行全量 UI 审计、编译反馈、运行时控制台检查和 `git diff --check`。
- [x] 5.5 更新 OpenSpec 清单与验收证据，确认无遗漏资源。

## 验收证据（2026-10-06）

- 18 份页面契约逐页菜单刷新，全部输出“未解析 0”。
- `审计契约与UI注册` 输出：页面契约 18、页面 Prefab 18、UI Prefab 总数 41、缺失脚本 0、UITable 注册匹配 18。
- `验证契约场景` 输出：节点重排场景通过；必选引用统计未解析 0；GuestPanel 可选详情缺失不阻断。
- 运行时烟雾任务 `763ce6d3` 完成，`debug_get_errors` 为 0，Unity diagnose healthy=true、consoleErrorCount=0、consoleWarningCount=0。
