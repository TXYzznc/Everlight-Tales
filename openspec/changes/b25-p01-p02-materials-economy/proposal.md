# 变更提案：b25 材料体系与背包 + 维修费与收支记账

覆盖任务：P4-001（材料体系与背包）、P4-002（维修费与收支记账）。

## 动机

G3 一天闭环关闭后进入 G4 成长闭环。成长闭环的根基是「夜里收获变成白天成长」：材料（加工素材）与维修费（唯一货币）是加工台解锁形态的两种消耗。b25 先立住经济基础——六种材料、三档稀有度、异常纹样按来源怪谈类型化、背包拥有，以及维修费唯一货币 + 三通道发放 + 收支记账。

## 关键决定

- 材料体系与背包（D-163）：`MaterialRarity`（普通/精良/稀有 三档）+ `MaterialConfig`（MT-001~006，异常纹样 `IsTypedByCase`）+ `MaterialCatalog.All`（六种：铜芯线/精密齿轮/玻璃镜片/校准簧片/定势残晶/异常纹样）。`MaterialStack`（一条拥有：材料ID+来源怪谈+数量，Key 区分纹样来源）+ `MaterialBackpack`（纯数据容器：Add/Remove/GetCount，不同来源纹样互不通用）。
- 维修费与收支记账（D-164）：维修费是唯一货币（余额即 `WorldState.RepairFee`），`EconomyService`（Meta）统一 `Grant`（三通道：结算即时 Settlement/任务页 Task/回访 Revisit）与 `Spend`（加工 Craft），每笔记一条 `LedgerEntry`（方向/通道/金额/事由/时间序号），余额不足拒绝支出。
- 记账贯通（D-165）：`TaskService.Claim`（任务页）与 `WorldSettlementService.Settle`（结算即时）改走 `EconomyService.GrantByTask/GrantBySettlement`，使三通道发放统一记账。
- 存档扩展（D-166）：`WorldState` 增 `Materials`（背包）+ `Ledger`（账目），`WorldSave`/`WorldSaveService` 同步持久化，材料与账目随世界存档往返。

## 变更范围

- Data：新增 `MaterialCatalog.cs`、`MaterialBackpack.cs`、`LedgerEntry.cs`；`WorldSave.cs` 增 Materials/Ledger。
- Meta：`WorldState` 增 Materials/Ledger；新增 `EconomyService.cs`；`TaskService.Claim`/`WorldSettlementService.Settle` 改走统一记账；`WorldSaveService` 持久化 Materials/Ledger。

## 验收

- UnitySkills Play Mode 综合探针（29/29，验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。

## 边界（本批有意留出）

- 加工台界面与配方消耗（P4-003/P4-004）在 b26；材料在事件结算中的具体掉落配置在事件库批。
- 材料/纹样的图标与加工台 UI 显示留美术批。
