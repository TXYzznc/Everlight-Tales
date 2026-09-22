# b28 设计

## 家园五区映射

| 区 | 实现 | 说明 |
|---|---|---|
| 来客 | 占位文本 | 首版不经营客流（D-026） |
| 加工 | WorkbenchPanel（复用 b44） | 加工 / 材料 / 账目三段 |
| 收藏 | CodexPanel（复用 b44） | 零件 P / 形态 M 三态图鉴 |
| 保管 | ArchivePanel（新增） | 陈列物列表 + 拥有物五类概览 |
| 服务 | 占位文本 | 首版暂无服务 |

## 陈列物模型（DisplayItem）

- `DisplayKind`：RepairCompletion=1 / LifeGift=2 / Exhibition=3 / CaseMemento=4。
- `DisplayItem{Id, Name, Kind, Source, ObtainedDay}`；按 `Id` 去重（同一事件重复完成只记一条）。
- 持久化 `et.world.display`，格式 `id:name:kind:source:day` 分号分隔。
- 本批只发放 RepairCompletion（卷帘门成功 →「卷帘门（已修复）」）；其余三类留 b30+ 接线。

## 主页壳接线

- `MainPageShell` Tab_2（图鉴→家园）：构建 `HomePanel`，默认落在「收藏」区（保留原图鉴用途）。
- Tab_3（工作台）：`HomePanel` 快捷入口，直接落到「加工」区（复用同一实例，避免工作台面板重复构建导致解锁后状态不同步）。
- 来客 / 服务两区为只读占位文本，不挂面板。

## 分层

- `DisplayItem` / `DisplayKind` 落 **Data**（纯 DTO，同 MaterialStack / LedgerEntry）。
- `WorldState.DisplayItems` 落 **Meta**（Meta 仅引 Data）。
- `HomePanel` / `ArchivePanel` / `WorldSession` 落 **UI**（UI 引全层）。
