# 设计：盘面根节点预制化与 GravitySeat 视觉清理

## D-1：预制体提供盘面根节点

在 `BoardPage` 预制体下增加 `board_root`。`HexBoardView` 优先使用该节点作为运行时 `board_tiles`、盘面描边和盘面特效的父节点；旧预制体缺少该节点时保留运行时兜底创建，避免历史资源无法加载。

`board_root` 的位置由 `RectTransform.anchoredPosition` 控制，整体大小由 `RectTransform.localScale` 控制。盘面内部仍使用现有 `m_CellSize` 和 `HexLayout` 坐标，不引入新的响应式重排算法。

## D-2：GravitySeat 只停用视觉生成

现有 `GravitySeat` 代码同时承担外围视觉座与方向箭头的显示，`SetGravity` 还承担更新重力方向表现。按确认结果，将生成循环和更新调用保留为注释，不生成这些节点；核心旋转逻辑仍依赖 `SettleState.GravityDirection`，数据、资源和文档保持不变。
