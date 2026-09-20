# 设计：b10 盘面渲染视图 + 棋子/设施/标记表现 + 拍击操作循环 UI + 六分区 HUD

## D-108：盘面视图程序化构建，HexLayout 点顶六边形布局

- `HexLayout.AxialToPixel(coord, size)`：点顶六边形轴向→屏幕，x=size·√3·(q + r/2)，y=size·1.5·r。
- `HexBoardView` 在 `Refresh(BoardState)` 时清空并按 `HexGrid.Enumerate(sideLength)` 摆放全部合法格（n=6 → 37 格），再摆放实体块；视图只读模型、不做结算。
- 程序化创建 Image/Text，不依赖预制体 YAML；布局数学与视觉细节（六边形描边）本批从简、后续 ART 资源替换。

## D-109：表现配色集中在 EntityVisuals

- `EntityVisuals.GetColor(entity)` 按 `EntityKind`（Facility 深灰／TaskMarker 黄／RepairTarget 紫）与 `PartType`（Hammer 橙／Ratchet 青／Coil 红／None 灰）给出可辨识颜色。
- 只用于表现，不参与结算；后续零件卡、异化标记复用同一映射。

## D-110：操作循环编排在 Board 层 BoardGame

- `BoardGame{Board,Settle,Session,Level,Tap}` 串联 b06～b09 的能力：`RotateLeft/Right` → SettleState，`Preview` → PreviewService，`ArmMove` → ArmService，`Tap` → TapSettlement + LevelState.ResolveAfterTap。
- 纯逻辑、零引擎；UI 只把按钮动作转成 BoardGame 操作并刷新视图。

## D-111：六分区 HUD 由 BoardHUD 绑定

- `BoardHUD` 程序化创建文本：顶部左轮次与拍数、顶部中累计分/目标分、分数下方特殊目标清单、底部左公共维修能量、底部右机械臂剩余次数；暂停按钮与冲击柄在 `BoardPage` 装配。
- `Refresh(session, level)` 从会话与轮次状态取数；HUD 只读、不做判定（判定在 LevelState）。

## 边界

- 本批不做导航栈接入（BoardPage 作为组件先可独立装配，UIForm 宿主与 BuildSettings/UITable 登记在 b11/b12 收口）；不接入真实六边形/立绘/音画资源（ART 路线）。
- 轨道转向与规则步进对象的预演仍待地形规则批次接入（b09 已注明）。
