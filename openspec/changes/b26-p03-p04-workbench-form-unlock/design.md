# 设计：b26 加工台 UI 与流程 + 形态解锁与当前使用

## 分层

沿用 D-089 五层布局，本批全部类型落位如下（Meta 仅引用 Data，UI 引全层）：

| 类型 | 层 | 说明 |
|---|---|---|
| `FormKind` / `FormMaterialCost` / `FormConfig` / `FormCatalog` | Data | 形态静态配置与首批目录 |
| `CurrentFormSave` | Data | 当前使用形态存档 DTO |
| `FormService` / `UnlockCheck` / `UnlockResult` | Meta | 解锁/切换/当前使用/图样（游戏逻辑，仅引 Data） |
| `WorkbenchLayout` / `FormCardEntry` / `MaterialHolding` / `FormCardState` | UI | 三段布局与四状态分类（纯逻辑，触达 Meta+Data） |

## 形态目录（D-084）

| 形态 | 宿主 | 维修费 | 图样 | 材料 |
|---|---|---:|---|---|
| P-001-F01 贯通撞锤 | P-001 惯性撞锤 | 80 | T-G01 | — |
| P-001-F02 横推撞锤 | P-001 惯性撞锤 | 120 | T-G02 | 精密齿轮×2 + 铜芯线×1 |
| P-003-F01 轴向线圈 | P-003 爆破线圈 | 140 | T-G03 | — |
| P-003-F02 定时线圈 | P-003 爆破线圈 | 180 | T-G04 | 铜芯线×2 + 校准簧片×1 |
| M-012 借力改道夹 | P-004 换向齿轮 | 240 | L-01 | 精密齿轮×2 + 校准簧片×2 + 定势残晶×1 + 异常纹样·红舞鞋×1 |

## 解锁流程（一次性支付）

1. `CanUnlock(world, form)`：已解锁→Already；否则分解缺口（`FeeShort` / `MissingBlueprint` / `MissingMaterials`）。
2. `Unlock(world, form, tick)`：条件齐→`EconomyService.Spend(Craft)` 扣维修费 + 按 `MaterialBackpack.Remove` 消耗材料（图样保留）→ 入 `UnlockedForms` → `AutoSetCurrent` 时写入 `CurrentForms[HostPart]`。
3. 重复解锁返回 `AlreadyUnlocked`，不再收费/耗材（每个形态只加工一次）。

## 切换与当前使用（P4-004 边界）

- `SwitchCurrent(world, hostPart, formId)`：`""`=基础形态；否则须为已解锁且宿主匹配。免费、立即生效。
- `GetCurrent(world, hostPart)`：返回当前使用（空=基础）。
- `ResolveForm(world, hostPart, tempOverride)`：准备页临时切换优先且只影响本关；非法或空时回退 `GetCurrent`。绝不改动 `CurrentForms`，保证「当前使用 / 临时切换不冲突」。

## 形态卡四状态

| 状态 | 判定 |
|---|---|
| Current 已解锁·当前使用 | 已解锁 且 `GetCurrent == Id` |
| Unlocked 已解锁·非当前使用 | 已解锁 且非当前 |
| Craftable 有图样·未解锁 | 未解锁 且（无需图样 或 有图样） |
| Unknown 未取得图样 | 未解锁 且需图样 且 无图样 |

## 存档

`WorldSaveService.Capture` 序列化 `Blueprints`/`UnlockedForms`/`CurrentForms`（跳过基础形态空值）；`Restore` 重建三集合。图样随世界存档永久保留，符合「条件不过期、可后补」口径。
