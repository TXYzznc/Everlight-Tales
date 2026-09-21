# 设计：b25 材料体系与背包 + 维修费与收支记账

## 1. 材料体系（D-163）

```csharp
// Data/MaterialCatalog.cs
MaterialRarity { Common=0, Refined=1, Rare=2 }
MaterialConfig { Id, Name, Rarity, IsTypedByCase }
MaterialCatalog.All(): MT-001 铜芯线(普通) / MT-002 精密齿轮(普通) / MT-003 玻璃镜片(普通)
                       MT-004 校准簧片(普通) / MT-005 定势残晶(精良) / MT-006 异常纹样(稀有,按来源类型化)

// Data/MaterialBackpack.cs
MaterialStack { MaterialId, SourceCase, Count, Key=MaterialId 或 MaterialId:SourceCase }
MaterialBackpack { Add(materialId,count,sourceCase), GetCount, Remove, Stacks }
```

- 材料是顶层统称、物资条目是具体物品、图样与维修费不属于材料（口径）。
- 异常纹样（MT-006）按来源怪谈类型化：`MT-006:红舞鞋` 与 `MT-006:画皮` 是不同条目、互不通用、互不替代。

## 2. 维修费与收支记账（D-164）

```csharp
// Data/LedgerEntry.cs
LedgerDirection { Income, Expense }
PayChannel { Settlement=0, Task=1, Revisit=2, Craft=3 }   // 三通道发放 + 加工支出
LedgerEntry { Direction, Channel, Amount, Reason, Tick }

// Meta/EconomyService.cs
Balance(world) = world.RepairFee                            // 维修费唯一货币
Grant(world, amount, channel, reason, tick)                 // 收入 + 记账
Spend(world, amount, channel, reason, tick)                 // 支出；余额不足 false
GrantBySettlement / GrantByTask / GrantByRevisit            // 三通道便捷入口
```

## 3. 记账贯通（D-165）

| 通道 | 接线点 |
|---|---|
| 结算即时 Settlement | `WorldSettlementService.Settle` → `GrantBySettlement` |
| 任务页 Task | `TaskService.Claim` → `GrantByTask` |
| 回访 Revisit | `EconomyService.GrantByRevisit`（入口已备，回访交付在 b29 接剧情） |
| 加工 Craft | `EconomyService.Spend`（b26 加工台调用） |

## 4. 存档扩展（D-166）

- `WorldState` 增 `Materials`（MaterialBackpack）+ `Ledger`（List<LedgerEntry>）。
- `WorldSave` 增 `Materials`（List<MaterialStack>）+ `Ledger`（List<LedgerEntry>）。
- `WorldSaveService.Capture/Restore` 同步快照与重建（材料栈与账目逐条拷贝）。

## 5. 层关系

- Data：`MaterialCatalog`/`MaterialBackpack`/`LedgerEntry`/`WorldSave`（零引擎）。
- Meta：`EconomyService`/`WorldState`/`WorldSaveService`/`TaskService`/`WorldSettlementService`（仅引用 Data）。

## 已知限制

- 材料只从事件结算结果发放，首版不可用维修费购买（D-024）。
- 加工配方与材料消耗在 b26（加工台与形态解锁）；材料掉落具体配置在事件库批。
