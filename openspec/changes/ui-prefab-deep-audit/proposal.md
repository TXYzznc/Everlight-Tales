## Why

`Assets/Game/Prefabs/UI` 中的页面和列表项已经具备 GF 表单/项目的基础拆分，但缺少一次覆盖全部资源的结构、层级、交互和运行时回归检查。装饰性 Image 的 Raycast Target、根节点锚点和 Map 刷新时重复绑定事件会造成输入遮挡、分辨率漂移或监听器持续累积。

## What Changes

- 为全部 UI Prefab 增加可重复执行的深度审计，检查 UIForm/UIItem 根类型、根节点锚点、Canvas 层级、ScrollRect、布局组件冲突、重复节点名和近似交互遮挡。
- 将 20 个 UIForm 根节点统一为 StretchAll，并关闭 44 个装饰引用图的 Raycast Target。
- 保持独立页面使用 UIForm、重复数据使用 UIItem、操作提示使用 DialogView/UIDialog 的 GF 拆分边界；审计工具只报告问题，不在运行时动态补布局。
- 修复 CityMapView 每次刷新重复注册匿名点击监听器的问题，改为一次订阅、销毁时解除订阅。
- 防止 Item 规范化工具为每个文本节点自动添加 ContentSizeFitter，避免列表批量刷新触发级联布局重建。

## Impact

影响 `Assets/Game/Prefabs/UI`、`Assets/Game/Scripts/EverlightTales/UI` 及其 Editor 审计工具和项目变更文档。GF 内置 LoadingView 的外部缺失 TMP 材质引用、Launch 场景基础设施告警和棋盘运行时绘制不在本次范围内。
