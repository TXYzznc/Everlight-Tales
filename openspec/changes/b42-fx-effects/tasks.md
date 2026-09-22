# 任务清单（b42 特效）

- [x] 改 `Board/BoardGame.cs`：新增 `LastSettlement` 只读属性，`Tap()` 内赋值
- [x] 改 `UI/HexBoardView.cs`：暴露 `BoardRoot`／`CellSize`／`OutlineRadius`／`BoardToLocal`／`EntityScale`
- [x] 新增 `UI/ScreenShakeEffect.cs`：震 board_root（DOShakeAnchorPos）
- [x] 新增 `UI/BoardHexPulseEffect.cs`：三层 HexagonGraphic 描边环脉冲
- [x] 新增 `UI/RotationPreviewRenderer.cs`：ghost + hit ring
- [x] 新增 `UI/ScorePopup.cs`：得分弹窗（上飘淡出，文字正向）
- [x] 新增 `UI/ShockwaveRingEffect.cs`：拍击冲击波（描边六边环 + 火花，uGUI）
- [x] 新增 `UI/ButtonParticlesEffect.cs`：按钮粒子爆发（填充六边火花，uGUI）
- [x] 新增 `UI/BoardImpactFX.cs`：Facade 装配 + PlayTapImpact + RefreshPreview/ClearPreview
- [x] 改 `UI/BoardPage.cs`：接线（拍击触发 FX、旋转刷新预览、重建清预览）
- [x] OpenSpec proposal/design/tasks
- [x] 编译验证 + 运行验收（Play Mode 触发特效）+ 静态检查 + 提交
