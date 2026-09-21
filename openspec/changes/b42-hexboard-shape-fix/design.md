# 设计：盘面形状复刻参考项目（b42 修订）

## 决策记录

### D-088 修订：参数化从「边长 n」改为「boardRadius」，格子数按几何推导

原 D-088 以「边长 n」（每边格数）参数化，总格 = `3n(n−1)+1`，合法格 = 轴向半径簇
`max(|q|,|r|,|q+r|) ≤ n−1`（**平顶大六边形**）。用户确认该口径有误，应改为参考项目的
「棋盘高度 = 若干小六边形」语义，最终定为 `boardRadius`（可玩区半径）：

- `OuterRadius = boardRadius + 1`（含外层残缺墙的轮廓半径）。
- 大六边形轮廓为**点顶**（顶点朝上下），边心距 `apothem = √3 · OuterRadius · CellSize`
  （CellSize 为格子外接圆半径）。
- 点包含判定（SDF 三投影）：`max(|x|, |0.5x+√3/2·y|, |0.5x−√3/2·y|) ≤ apothem`。
- 三分类（几何依据，非中心距离）：
  - **Normal**：6 顶点（90°+i·60°，半径 CellSize）全部 SDF ≤ apothem + ε。
  - **Wall**：中心 SDF ≤ apothem + margin（margin=CellSize，候选筛选），但任一顶点越界。
  - **Outside**：中心 SDF > apothem + margin（候选外，不生成格子、不参与逻辑）。

| 档位 | boardRadius | OuterRadius | 正常格 | 墙 | 总格 |
|---|---:|---:|---:|---:|---:|
| 教学 | 5 | 6 | 133 | 48 | 181 |
| 试机 | 6~8 | 7~9 | 181~307 | 60~72 | 241~379 |
| 普通 | 6~10 | 7~11 | 181~463 | 60~90 | 241~553 |
| 怪谈 | 12 | 13 | 649 | 108 | 757 |

> 注：旧 sideLength=5 的 61 格（轴向半径簇）在 boardRadius=5 下全部仍为 Normal（0 越界），
> 红舞鞋 (-2,-2)、教程 (3,0) 等关键坐标全部 Normal，现有关卡坐标无需迁移。

### D-新：`HexBoardShape` 承载 SDF 三分类（Board 层零引擎）

新增 `Board/HexBoardShape.cs`：以 `boardRadius` 构造，预计算或即时计算三分类，暴露：

- `BoardRadius`／`OuterRadius`／`Apothem`（CellSize 归一化为 1，几何与像素解耦）。
- `CellKind Classify(HexCoord)` → Normal／Wall／Outside。
- `bool IsNormal(coord)`／`bool IsWall(coord)`。
- `IEnumerable<HexCoord> EnumerateNormal()`／`EnumerateWall()`／`EnumerateInside()`（Normal+Wall）。
- `int NormalCount`／`WallCount`。

轴向局部坐标沿用现有 `HexLayout.AxialToPixel` 的约定（`x=√3(q+r/2)·size, y=1.5·r·size`），
顶点角度 90°+i·60°（点顶）。SDF 判定用归一化 CellSize=1，与像素无关，供离线演算与 UI 共用。

### D-新：墙格 = `IsValid == false` 但非 Outside，逻辑阻挡复用现有链路

`BoardState` 参数从 `SideLength` 改为 `BoardRadius`，内部持有 `HexBoardShape`：

- `IsValid(coord)` = `shape.IsNormal(coord)`（正常格可放置、可移动）。
- 新增 `IsWall(coord)` = `shape.IsWall(coord)`。
- 新增 `EnumerateNormal()`／`EnumerateWall()` 供视觉层与构建器使用。

由于墙格 `IsValid == false`，现有所有 `board.IsValid(next)` 判定链路（TapSettlement 下落、
PreviewService 预览、PartAbility 推移、ObstacleService 生长、MFormService 候选、RedShoe 步进）
**自动**把墙格当边界阻挡，无需逐个改动。视觉层通过 `EnumerateWall()` 单独渲染残缺墙。

### D-新：视觉层——正常格 + 残缺墙 + 点顶轮廓

`HexBoardView`：

- 正常格：`HexagonGraphic` 完整点顶六边形（现有）。
- 残缺墙：`HexagonGraphic` 加「裁剪」模式，用大六边形 SDF 裁剪六边形顶点，绘制被裁切
  的半格（uGUI 无 SpriteMask 直接裁切 Graphic，改用多边形顶点裁剪，视觉等价）。
- 大六边形轮廓：点顶，`outlineRadius = 2 · OuterRadius · CellSize`（顶点到中心），旋转后屏幕平顶。
- 旋转角沿用 `RotationAngleForGravity`（重力恒指向屏幕下方），点顶格子+点顶大六边形+点顶
  轮廓三者本地朝向一致，旋转后屏幕全部平顶，消除当前 30° 错位。

## 类型清单

| 类型 | 文件 | 状态 |
|---|---|---|
| `HexBoardShape` + `HexCellKind` | Board/HexBoardShape.cs | 新增 |
| `BoardState` | Board/BoardState.cs | 参数 SideLength→BoardRadius，接 HexBoardShape，加墙格接口 |
| `HexGrid` | Board/HexGrid.cs | 废弃轴向半径簇，改由 HexBoardShape 承担（或删除） |
| `LevelBoardConfig` | Board/LevelBoardConfig.cs | SideLength→BoardRadius |
| `RedShoeLevelConfig` | Board/RedShoeLevel.cs | SideLength→BoardRadius |
| `SeededBoardBuilder` / `InitialBoardBuilder` | Board/*.cs | 枚举改用 shape 正常格 |
| `RepairSaveService` | Events/RepairSaveService.cs | sideLength→boardRadius |
| `PreviewModel` | Board/PreviewModel.cs | SideLength→BoardRadius |
| `HexagonGraphic` | UI/HexagonGraphic.cs | 加裁剪模式 |
| `HexBoardView` | UI/HexBoardView.cs | 正常格+残缺墙+点顶轮廓 |
