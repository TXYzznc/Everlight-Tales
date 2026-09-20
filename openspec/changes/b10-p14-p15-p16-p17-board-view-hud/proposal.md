# 变更提案：b10 盘面渲染视图 + 棋子/设施/标记表现 + 拍击操作循环 UI + 六分区 HUD

覆盖任务：P1-014、P1-015、P1-016、P1-017。

## 动机

b05～b09 已把盘面核心逻辑（网格、实体、旋转、拍击结算、事件队列、零件、能量、计分、机械臂、轮关、种子、预演）落到纯逻辑层。b10 是 G1 的第一个表现层批次：把盘面核心渲染成可看的竖屏盘面，并接上操作循环与 HUD。

## 关键决定

- 盘面视图程序化构建（不依赖预制体 YAML），布局由 `HexLayout`（点顶六边形轴向→屏幕）换算（D-108）。
- 表现配色集中在 `EntityVisuals`（EntityKind/PartType → 颜色），供盘面视图与后续零件卡复用（D-109）。
- 操作循环编排在 Board 层 `BoardGame`（旋转/搬动/预演/拍击/过轮），UI 只绑定不结算（D-110）。
- 六分区 HUD 由 `BoardHUD` 绑定会话与轮次状态（D-111）。

## 变更范围

- 新增 Board：`BoardGame.cs`。
- 新增 UI：`HexLayout.cs`、`EntityVisuals.cs`、`HexBoardView.cs`、`BoardHUD.cs`、`BoardPage.cs`。

## 验收

- UnitySkills Play Mode 探针（验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。
