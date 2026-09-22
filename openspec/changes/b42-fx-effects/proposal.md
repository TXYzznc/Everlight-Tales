# 提案：盘面表现特效系统化实现（参考 2026CIGA 适配 uGUI）

覆盖任务：b42 盘面视觉接线（issue #4）的特效部分——系统化复刻参考项目 2026CIGA 的盘面
表现特效，美术重做、技术栈从「SpriteRenderer + 内置 RP + 世界空间」适配为「uGUI + URP + Overlay」。

## 目标

把参考项目已 inventory 的特效清单按优先级分批落地，首批聚焦四个核心可玩反馈：

1. **屏幕震动**：拍击时盘面震动（参考 `ScreenShakeEffect` 震 Camera → 本项目震 `board_root` RectTransform）。
2. **连击脉冲**：连续得分时棋盘轮廓向内嵌套的三层六边环脉冲（参考 `BoardHexPulseEffect` 的 LineRenderer → 本项目 `HexagonGraphic` 描边环）。
3. **旋转预览**：旋转后显示将移动棋子的落点 ghost 与被撞棋子的橙色 hit ring（参考 `RotationPreviewRenderer`）。
4. **得分弹窗**：得分事件位置弹出「+N」上飘淡出（参考 `ShowScorePop` 的 TMP → 本项目 uGUI Text + DOTween）。

## 前置

- b42-hexboard-shape-fix 已提交（盘面形状正确，含 `HexagonGraphic` 描边六边形能力）。
- b10 已落 `HexBoardView`（board_root / board_tiles 结构）、`BoardHUD`；b10/b11 已落 `BoardGame`、
  `PreviewService`（第一步终点预览）、`SettlementResult.Events`（结算事件日志）。
- 参考项目源码已克隆至 `Temp/2026CIGA_src`（VFX 目录齐全）。

## 验收边界

- 拍击有得分时盘面震动 + 连击脉冲触发，无得分连击归零。
- 旋转后刷新预览：ghost 显示在落点、hit ring 显示在被撞棋子位置，拍击/重建后清除。
- 得分事件位置弹出「+N」并上飘淡出，文字始终正向（不随盘面旋转）。
- 特效均为 UI 层表现，Board 层仅增加 `BoardGame.LastSettlement`（暴露结算结果，零引擎）。
- 编译 0 错 0 警，`verify_project_assemblies.py` / `audit_framework_purity.py` 通过。
- 运行验收（Play Mode 截图/属性探针）：特效在正式流程中触发（接入 BoardPage 按钮链路）。

## 关键决定

- **技术栈适配**：参考的 `LineRenderer`（世界空间）→ 复用已有 `HexagonGraphic` 描边环；`SpriteRenderer`
  ghost → `HexagonGraphic` 半透明填充块；`TMP + 协程` 弹窗 → `uGUI Text + DOTween`。
- **震动对象**：震 `board_root` 的 RectTransform（`DOShakeAnchorPos`），不震整个 Canvas，避免 HUD 抖动。
- **色差/径向模糊（`ChromaticAberrationEffect`/`RadialBlurEffect`）本期跳过**：Overlay Canvas 不经过相机
  后处理（`OnRenderImage` 对 Overlay UI 无效），留待 UI 改 ScreenSpaceCamera 或 URP Volume 方案时再评估。
- **手绘数字（`NumText` sprite tags）本期跳过**：依赖美术数字贴图，用普通 Text 数字，正式美术到位后替换。
- **冲击波环/按钮粒子（`ShockwaveRingEffect`/`SmackButtonParticlesEffect`）本期跳过**：依赖 ParticleSystem +
  世界空间，uGUI 下收益/成本比低，留后续批次。

## 非目标（Non-Goals）

- 色差、径向模糊等相机后处理。
- 手绘数字字体、棋子运动拉伸 shader。
- 冲击波圆环、按钮粒子爆发。
- 运动拉伸 shader（`NumText` 之外的美术/T​​A 类特效）。
