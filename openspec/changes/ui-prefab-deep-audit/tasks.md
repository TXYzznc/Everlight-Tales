## 1. 全量 Prefab 审计

- [x] 1.1 扫描 `Assets/Game/Prefabs/UI` 下 20 个页面和 21 个 Item。
- [x] 1.2 检查根节点锚点、Canvas/Sorting、滚动容器、布局冲突、重复节点名和近似遮挡。
- [x] 1.3 输出 `Library/UiPrefabDeepAudit.md` 并提供可重复执行菜单。

## 2. 结构与交互修复

- [x] 2.1 统一 20 个 UIForm 根节点为 StretchAll。
- [x] 2.2 关闭 44 个装饰引用图的 Raycast Target。
- [x] 2.3 保持 MainPageShell 与独立页面 UIForm 的边界，保留 UIItem/UIDialog 资源。

## 3. 运行时代码优化

- [x] 3.1 将 CityMapView 节点点击改为显式事件，并在 MapPanel 生命周期内订阅/解除。
- [x] 3.2 移除 Item 规范化工具自动添加 ContentSizeFitter 的行为。
- [x] 3.3 检查编译反馈、控制台错误、全量页面烟雾测试和 Raycast 审计。

## 4. 项目验证

- [x] 4.1 运行 `python tools/verify_project_assemblies.py`。
- [x] 4.2 运行 `python tools/audit_framework_purity.py`。
- [x] 4.3 运行 `git diff --check`；全工作区检查仅发现既有 Unity YAML 空值/换行告警，新增范围的 scoped diff check 通过。
