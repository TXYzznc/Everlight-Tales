# 变更提案：盘面根节点预制化与 GravitySeat 视觉清理

## 动机

当前 `HexBoardView` 在运行时创建全屏 `board_root`，盘面整体的位置与缩放缺少预制体级控制；同时盘面外围会生成六个 `GravitySeat` 视觉节点，而当前设计倾向于不显示这组节点。

## 已确认决策检查点（第 1～2 轮）

- `board_root` 继续挂在 `BoardPage` 下，保留现有坐标体系。
- `board_root` 改为预制体中的静态 `RectTransform`，供 Inspector 调整 `anchoredPosition`。
- 盘面整体大小使用 `board_root.localScale` 控制，保持现有格子坐标与交互逻辑。

## 已确认决策检查点（第 3 轮）

- 仅注释掉 `GravitySeat` 与 `DirectionArrow` 的视觉生成和更新调用。
- 保留 `SettleState.GravityDirection`、左右旋转逻辑、资源和设计文档，便于后续重新启用视觉表现。
