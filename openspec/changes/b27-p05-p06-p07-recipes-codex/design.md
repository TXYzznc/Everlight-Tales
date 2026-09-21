# 设计：b27 F 类配方 + M 类图样来源与配方 + 零件图鉴

## 分层

沿用 D-089 五层布局（Meta 仅引用 Data，UI 引全层）：

| 类型 | 层 | 说明 |
|---|---|---|
| `PartType`（20 值）/ `PartCodexConfig` / `PartCodexCatalog` | Data | 零件枚举与图鉴元数据 |
| `FormConfig` / `FormCatalog`（19 形态 = 4F+15M）/ `FormKind` / `FormMaterialCost` | Data | 形态配置与完整配方目录 |
| `KnownParts`（WorldState/WorldSave/WorldSaveService） | Meta/Data | 零件来源已出现追踪 |
| `CodexState` / `CodexCategory` / `CodexEntry` / `CodexLayout` | UI | 图鉴三态只读布局与收集进度 |

## 零件枚举（P4-007）

`PartType` 数值 0~5 保持（None/惯性撞锤/计量棘轮/爆破线圈/换向齿轮/铆合钳），追加 15 值（弹射簧/分裂铸模/储料胃袋/吞料炉/交换拨叉/旋涡转子/蓄能飞轮/导电桥/照明棱镜/叩击音叉/排水叶轮/缓冲囊/校准探针/磁吸牵引器/接力电池）。新值行为未实现，`PartCatalog.Get` 返 null、`PartAbility` 走默认惰性分支，不影响已有结算。

## 形态配方目录（P4-005/P4-006，D-084）

F 类四形态（b26 已落，本批确认）：

| 形态 | 宿主 | 维修费 | 图样 | 材料 |
|---|---|---:|---|---|
| P-001-F01 贯通撞锤 | P-001 | 80 | T-G01 | — |
| P-001-F02 横推撞锤 | P-001 | 120 | T-G02 | 精密齿轮×2 + 铜芯线×1 |
| P-003-F01 轴向线圈 | P-003 | 140 | T-G03 | — |
| P-003-F02 定时线圈 | P-003 | 180 | T-G04 | 铜芯线×2 + 校准簧片×1 |

M 类十五形态（本批补 14 + 修正 M-012）：宿主/维修费/图样（=来源怪谈 L-XX）/材料按 D-084 逐条落；五个代表性形态带类型化异常纹样。图样保留不消耗，解锁一次支付维修费 + 消耗材料（复用 b26 `FormService.Unlock`，无需改机制）。

## 零件图鉴（P4-007，D-081）

- 收录 20 P + 15 M = 35 条（O/A/C 与材料不进入图鉴）。
- 顶部两标签页「通用零件」「怪谈形态」，各自按 ID 升序；顶部右收集进度「已拥有 n/35」。
- 三态（只正向推进）：
  - 已拥有：P=OwnedParts；M=UnlockedForms（图样已取得且加工完成）。
  - 已知未拥有：P=KnownParts（来源已出现）；M=Blueprints 含该形态图样（L-XX）。
  - 完全未知：默认。
- 条目视图模型 `CodexEntry{Id,Name,Category,Part,State,SourceHint,Stage,Description}`；`DisplayName` 对完全未知返回「？？？」。
- 不新增独立数据：M 条目引用 `FormCatalog`、P 条目引用 `PartCodexCatalog`（已拥有详情战斗数值另读 `PartCatalog`），符合「两处共用同一套配置表」。
- 图鉴只读：`CodexLayout` 不写任何世界状态。

## 存档

`WorldSave.KnownParts`（`List<PartType>`）+ `WorldSaveService` Capture/Restore 逐条往返，与 OwnedParts/Blueprints/UnlockedForms 同轨。
