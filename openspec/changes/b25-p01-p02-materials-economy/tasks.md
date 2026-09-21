# 任务分解：b25 材料体系与背包 + 维修费与收支记账

覆盖任务：P4-001、P4-002。

- [x] 1. Data：`MaterialCatalog.cs`（MaterialRarity/MaterialConfig/六种材料）
- [x] 2. Data：`MaterialBackpack.cs`（MaterialStack/MaterialBackpack 拥有与显示）
- [x] 3. Data：`LedgerEntry.cs`（LedgerDirection/PayChannel/LedgerEntry）
- [x] 4. Meta：`EconomyService.cs`（Grant/Spend/Balance + 三通道）
- [x] 5. Meta：`WorldState` 增 Materials/Ledger；`WorldSave`/`WorldSaveService` 持久化
- [x] 6. Meta：`TaskService.Claim`/`WorldSettlementService.Settle` 改走统一记账
- [x] 7. 探针 `B25AcceptanceRunner` 验收（29/29，验收后删除）
- [x] 8. 静态检查（meta/程序集布局/纯度）+ 编译 0 错 0 警
