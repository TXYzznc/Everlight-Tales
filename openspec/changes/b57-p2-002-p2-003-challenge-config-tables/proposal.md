## Why

G6 已关闭、程序侧 112 项任务完成，但盘面内容件在**配置层**仍有三处真实缺口（2026-10-13 核对）：

- `Board/ObstacleTable` 只有 4 列（Key／ObstacleKind／MaxDurability／PassDirection），O-003 初始开闭、O-005 允许夹持标签、O-008 归零拆除附属隔板数等逐项参数目前散落在实体默认值与关卡实例里，无法只靠改表调整。
- `Board/AnomalyTable` 8 行参数完全相同（占位值 1／2／3／3／2）；且 **A-003 回声区域、A-007 重绘帷幕** 在代码中仍是 `ReservedA003`／`ReservedA007`，而 `05-挑战库` 中两份设计文档已存在。
- 怪谈专属挑战 C-001~C-015 **完全没有配置表**；只有 `RedShoeTable`／`RedShoeGuideTable`／`RedShoeTurnTable` 覆盖 C-001，`ReleaseKind.Challenge` 标注为 2 却无任何行使用，`StageReleaseTable` 中也没有 C-xxx 行。

## What Changes

- 覆盖原任务 **P2-002**（障碍设施 8 种 O-001~O-008）、**P2-003**（棋盘异常 8 种 A-001~A-008）；C-xxx 表无独立任务 ID，按 **G6 后内容批**口径对应 **P1-021**（关卡盘面配置表+教学样张）。用户开发任务工作簿保持只读，不写入。
- `Board/ObstacleTable` 新增 `ExtraData` 列，O-003／O-005／O-007／O-008 的逐项参数改由表驱动。
- `Board/AnomalyTable` 的占位值改为逐项真实值，新增 `ExtraData` 列；A-003／A-007 从「保留」改为正式类型并补齐行为。
- 新增 `Board/ChallengeTable`（C-001~C-015）与 Data 层 `ChallengeConfig`／`ChallengeCatalog`，接入关卡装配与投放。
- `Meta/StageReleaseTable` 补 C-001~C-015 行（`ReleaseKind=Challenge`）。
- `ExtraData` 统一采用 `k=v;k=v` 文本编码，解析收敛到 Data 层单个解析器；非法键值抛错，与既有 `ObstacleCatalog.Install`／`AnomalyCatalog.Install` 的严格校验风格一致。
- `tools/datatable_excel_source.py` 的 `TABLE_SCHEMAS` 同步新增表与新列，保持 Excel 源可重建。

## Capabilities

### New Capabilities
- `board-challenge-config`: 障碍设施、棋盘异常与怪谈专属挑战的逐项配置、编码约定与运行时装配。

### Modified Capabilities
无。拍击／连锁／结算／计分等既有规则不变；C-001 数值沿用已确认的 D-051，不在本批重开。

## Impact

修改 `Assets/Game/Scripts/EverlightTales/` 的 `Data`（Config／Catalog／ExtraData 解析）、`Board`（AnomalyService／ObstacleService）与 `UI`（`GameConfigRuntime` 装载）三层；新增 `GameData/DataTables/Board/ChallengeTable.xlsx` 及其生成的 `.txt`／`.cs`；同步 `tools/datatable_excel_source.py`。不修改 `Assets/Game/ScriptsBuiltin/`，不新增包、美术、音频或后续十四案内容，不推送、不改写历史。
