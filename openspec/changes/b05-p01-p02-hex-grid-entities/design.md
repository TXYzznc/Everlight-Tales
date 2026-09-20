# 设计：蜂窝格 + 实体占用层（b05）

## 决策记录

### D-091：网格以「边长 n」参数化，半径 = n − 1

盘面尺寸沿用 D-088 的「边长 n」（n = 每边格数），与 `第一版开发任务表` P1-001 的「各 n 档位」一致。坐标半径（最大轴向距离）= n − 1，合法格满足 `max(|q|, |r|, |q + r|) ≤ n − 1`，即 `DistanceFromCenter(coord) < n`；总格数 = `3n(n − 1) + 1`。

| 档位 | 边长 n | 半径 | 总格数 |
|---|---:|---:|---:|
| 教学 | 5 | 4 | 61 |
| 试机 | 5～7 | 4～6 | 61～127 |
| 普通 | 6～8 | 5～7 | 91～169 |
| 怪谈 | 10 | 9 | 271 |

说明：`HexGrid` 原桩代码参数命名为 `radius` 但 `CountCells` 使用边长公式、`IsValid` 使用半径判定，二者不自洽；本批按 D-088 统一为「边长 n」语义并修复 `IsValid` 为 `< n`。

### D-092：实体用「类别枚举 + 固定标志」建模

`BoardEntity` 为单一基类，携带 `EntityKind`（Part／Facility／TaskMarker／RepairTarget）与 `IsFixed` 标志。`IsMovable = !IsFixed`：零件默认可移动；固定设施锁盘面不可移动；任务标记默认固定。后续批次（能力、维修进度）在基类上扩展，不在此批引入子类膨胀。

### D-093：盘面状态用「三层模型」承载

`BoardState` 持有边长 n 与三层结构：

- 实体占用层：`Dictionary<HexCoord, BoardEntity>`，每格最多一个实体，ID 唯一。
- 地形层：`Dictionary<HexCoord, TerrainKind>`（轨道／区域），与实体层可共存。
- 端点标签：`Dictionary<HexCoord, int>`（目标 ID），附着格子、不占实体容量。

放置与移动通过结果枚举（`PlaceResult`／`MoveResult`）返回，遵守「每格单实体」「固定锁盘面」「不越界」。

## 类型清单

| 类型 | 文件 | 状态 |
|---|---|---|
| `HexCoord` | Board/HexCoord.cs | 已有，沿用 |
| `HexDirection` + `HexDirections` | Board/HexDirection.cs | 已有，沿用 |
| `HexGrid` | Board/HexGrid.cs | 修复参数语义 + 新增 `RadiusOf`／`Enumerate` |
| `EntityKind` | Board/EntityKind.cs | 新增 |
| `TerrainKind` | Board/TerrainKind.cs | 新增 |
| `BoardEntity` | Board/BoardEntity.cs | 新增 |
| `BoardState` | Board/BoardState.cs | 新增 |
