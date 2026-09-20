# 任务清单（b05）

- [x] 修复 `HexGrid` 参数语义（边长 n，半径 n−1，`IsValid` 改为 `< n`）
- [x] 新增 `HexGrid.RadiusOf` 与 `HexGrid.Enumerate`（生成全部合法格）
- [x] 新增 `EntityKind`（Part／Facility／TaskMarker／RepairTarget）
- [x] 新增 `TerrainKind`（Track／Area）
- [x] 新增 `BoardEntity` 实体基类（ID、类别、固定标志、可移动）
- [x] 新增 `BoardState`（实体占用层 + 地形层 + 端点标签 + 放置／移动限制）
- [x] UnitySkills Play Mode 验收探针（网格生成 + 坐标方向换算 + 占用与移动限制）
- [x] `verify_project_assemblies.py` 与 `audit_framework_purity.py` 通过
