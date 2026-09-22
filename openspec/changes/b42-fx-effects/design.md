# 设计：盘面表现特效（uGUI 适配）

## 分层

所有特效均为 UI 层（`Everlight.Tales.UI`）纯表现组件，不参与结算；Board 层只新增一个
只读暴露点 `BoardGame.LastSettlement`（结算结果已在 `Tap()` 内产生，仅补一个属性引用，
零引擎依赖、不改变结算语义）。

```
BoardPage（装配）
├── HexBoardView（盘面渲染，board_root / board_tiles）
│   ├── board_outline（HexagonGraphic 描边，已存在）
│   ├── board_edge_glow（HexagonGraphic 描边 + CanvasGroup，已存在）
│   └── board_tiles（格 + 实体）
├── BoardHUD
└── BoardImpactFX（新增，拍击冲击总入口）
    ├── ScreenShakeEffect（震 board_root）
    ├── BoardHexPulseEffect（三层六边环，挂 board_root 下）
    ├── ShockwaveRingEffect（冲击波，挂 board_root 下）
    ├── ButtonParticlesEffect（按钮粒子，挂页面根）
    └── ScorePopup（得分弹窗，挂页面根，文字不随盘面旋转）
```

## 组件设计

### 1. ScreenShakeEffect（屏幕震动）

- 挂 `HexBoardView` 同一 GameObject，持有 `board_root` 引用（由 `HexBoardView` 暴露）。
- `Shake()`：`board_root.DOShakeAnchorPos(duration, strength, vibrato, randomness, fadeOut: true)`。
- 复用现有 `DOTween.Extension`（`DOTweenModuleUI.DOShakeAnchorPos` 已确认存在）。
- 强度/时长/振动频率按像素尺度调参（参考 world 单位 strength 0.18 → 本项目约 8~14px）。

### 2. BoardHexPulseEffect（连击脉冲）

- 挂 `board_root` 下，三层 `HexagonGraphic` 描边环（`color=clear` + `StrokeColor`），
  半径分别 = 轮廓半径 × {1, 0.72, 0.48}，延迟 {0, 0.07, 0.14}s。
- `Pulse(int combo)`：combo 越大颜色从橙 → 深红（`Color.Lerp`），alpha 峰值从 min→max。
- 每帧/每拍用 `DOTween` 驱动：环 scale 从基准扩张 `expand×(0.25+comboT)`，alpha 从峰值衰减到 0。
- 轮廓半径由 `HexBoardView` 暴露的 `outlineRadius = 2f*(boardRadius+1)*cellSize` 传入。

### 3. RotationPreviewRenderer（旋转预览）

- 挂 `board_root` 下，每次预览 `Refresh(previewMoves, board, gravityOffset)`：
  - 对每条 `PreviewMove`：在 `To` 画 ghost（半透明实体色填充块）。
  - hit ring：在 `To + gravityOffset` 处若有实体（被撞目标），画橙色描边环。
- `Clear()` 销毁全部临时对象；`Setup(root, cellSize)` 注入容器与尺寸。

### 4. ScorePopup（得分弹窗）

- 挂页面根（`HexBoardView.transform`，不随 board_root 旋转）。
- `Pop(score, boardLocalPos)`：把 `board_root` 本地坐标经旋转+平移换算到页面本地坐标，
  创建 `Text`「+N」黄色加粗，`DOAnchorPosY` 上飘 + `CanvasGroup.DOFade` 淡出，结束销毁。
- 坐标换算由 `HexBoardView.BoardToLocal(boardLocal)` 提供（旋转后文字仍正向）。

### 5. ShockwaveRingEffect（拍击冲击波）

- 挂 board_root 下（随盘面旋转），`Setup(boardRoot, outerRadius)` 注入轮廓半径。
- `Play(boardLocalPos)`：以盘面中心为原点扩散，二层描边六边环（主环暖白 + 回环暖橙，
  延迟 0/0.05s）+ 若干填充六边火花向外飞散；缩放扩张 + CanvasGroup 淡出，结束销毁瞬态 host。
- 每次拍击都触发（与是否得分无关），代表「物理冲击感」。

### 6. ButtonParticlesEffect（按钮粒子爆发）

- 挂页面根（不随盘面旋转），`Burst(pageLocalPosition)` 在按钮位置向外飞散暖色小六边火花，
  DOTween 位移 + 缩放 + 淡出，结束销毁。

### 7. BoardImpactFX（总入口 / Facade）

- `Setup(boardView, boardRadius)` 装配以上组件与注入依赖。
- `PlayTapImpact(settlementResult, board, tapButton)`：每次拍击先触发 `shockwave.Play(盘面中心)`
  与 `buttonParticles.Burst(按钮位置)`；再统计 `TotalScore`：`TotalScore>0` 时
  `screenShake.Shake()` + `comboPulse.Pulse(++combo)`，否则 `combo=0`；遍历 `Events` 中
  `ScoreDelta>0` 的事件，在其 `To`（或 target 坐标）弹得分弹窗。
- `RefreshPreview()`：调 `RotationPreviewRenderer.Refresh(...)`。
- `ClearPreview()`：调 `RotationPreviewRenderer.Clear()`。

## Board 层改动

`BoardGame` 增加只读属性：

```csharp
public SettlementResult LastSettlement { get; private set; }
```

`Tap()` 内 `LastSettlement = result;`（原 `result` 本就局部存在，仅补一行赋值）。

## 触发接线（BoardPage）

- `OnTap()`：`Tap()` → `m_ImpactFx.PlayTapImpact(Game.LastSettlement, Game.Board)`。
- `OnRotateLeft/Right()`：旋转后 `m_ImpactFx.RefreshPreview()`（用 `Game.Preview()`）。
- `Refresh()`：重建盘面后 `m_ImpactFx.ClearPreview()`。

## 坐标换算细节

- `board_root` 的 `anchoredPosition=0`（锚点全拉伸），格坐标在 board_root 本地系。
- 盘面旋转 = `board_root.localRotation`（z 角 = `RotationAngleForGravity`）。
- `BoardToLocal(boardLocal)` = `(Vector2)(board_root.localRotation * boardLocal)`，
  供弹窗落在页面本地系（文字不受盘面旋转影响）。
- 实体/格子的像素坐标仍走 `HexLayout.AxialToPixel(coord, cellSize)`（Board 层不引用引擎，
  换算留在 UI 层 `HexLayout`，不新增 Board 依赖）。
