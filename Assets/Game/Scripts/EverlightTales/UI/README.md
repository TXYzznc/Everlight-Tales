# Everlight.Tales.UI

业务 UI 层。程序集：`Everlight.Tales.UI`。

## 职责

- 全部业务 `UIForm`／`UIItem` 与竖屏页面壳（盘面、准备、结算、三页、地图、加工台、图鉴、家园）。
- 事件队列驱动的表现播放：滑动、碰撞、触发、飘字、连击计数。
- 页面壳与通用组件的复用入口。

## 边界

- 可引用：`Everlight.Tales.Data`、`Everlight.Tales.Board`、`Everlight.Tales.Events`、`Everlight.Tales.Meta`，以及框架 UI／TMP／UniTask／DOTween。
- 禁止放入：模拟器演算逻辑、存档读写、底层输入读取。
- 页面不得各自实现一套互不兼容的导航；统一复用页面壳与通用组件。

## 后续任务

| 任务 | 内容 |
|---|---|
| P0-006 | 通用页面壳与安全区 |
| P0-007 | 通用弹窗／Toast／确认 |
| P1-014 起 | 盘面视图、HUD、操作循环 |
| P2-007 起 | 准备页、预览、结算与失败面板 |
| P3-003 起 | 地图、任务系统三页、对话 |
