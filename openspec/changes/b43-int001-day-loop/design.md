# 设计：一天闭环流程接入（b43）

## 分层

全部落 UI 层（`Everlight.Tales.UI`），复用已就绪的 Meta／Events／Board 服务，不新增引擎依赖、
不改框架基线。Board 层保持零引擎（本批不改 Board）。

```
MainPageShell（主页壳，UIForm 宿主）
├── 序章覆盖层 OpeningOverlay（字幕 + 跳过，新档首进显示）
├── 地图页签 MapPanel
│   ├── TimePeriodBar（时段条）
│   ├── CityMapView（五节点）
│   └── PlaceEventPanel（地点信息 + 事件列表 + 开始按钮）
├── 结算覆盖层 SettlementOverlay（结果 + 四选一 + 回地图）
└── 其余页签（任务/图鉴/工作台/设置，占位，b44 收口）

BoardPageForm（盘面，UIForm）
└── 从 WorldSession.Current 当前事件加载盘面（卷帘门 EV-N01）
```

## WorldSession（运行时编排器）

静态单例 `WorldSession.Current`，纯 C#（非 MonoBehaviour），持有：

- `WorldState World`、`TimeState Time`（Board）、`RandomService Rng`（Board）、
  `RedShoeIntroService Intro`（Meta）、`BuffSet HeldBuffs`（Events）。
- `RepairEventInstance CurrentEvent` + `LevelBoardConfig CurrentBoardConfig`（当前盘面事件）。
- 方法：`NewGame(seed)`（建世界 + 开局地图 + 初始零件 + 刷新供给）、
  `LoadOrNew(seed)`（有存档恢复、无存档新档）、`Save()`（PlayerPrefs 最简持久化）、
  `StartRollerDoor()`（构建卷帘门盘面 + 事件开始）、
  `SettleEvent(outcome)`（事件结算 → 结算事务推进时间 → 世界结算发放奖励 → 返回奖励）。

## MapPanel（地图页签）

- `Build(WorldSession)`：装配 TimePeriodBar + CityMapView + PlaceEventPanel。
- 点地点节点 → PlaceEventPanel 显示该地点事件（home=卷帘门 EV-N01，其余地点暂「无当前事件」）。
- 点「开始事件」→ `WorldSession.StartRollerDoor()` → `GF.UI.OpenUIForm(UIViews.BoardPage)`。

## OpeningOverlay / SettlementOverlay（覆盖层）

- OpeningOverlay：播放序章字幕（OpeningSequence + OpeningPage 已有），跳过/播完隐藏，进入地图。
- SettlementOverlay：显示结算结果（成功/失败 + 奖励维修费），成功后四选一（RewardChoiceService），
  选完隐藏、回地图刷新。

## 触发接线（MainPageShell）

- `OnOpen`：`WorldSession.LoadOrNew(seed)`；若新档且未过序章，显示 OpeningOverlay。
- `OnTabClicked(0)`（地图）：装配 MapPanel（替换原「直开 BoardPage」）。
- 盘面关闭后：`MapPanel.Refresh()`（刷新时段条 + 供给）；若刚结算过，显示 SettlementOverlay。

## 存档持久化（PlayerPrefs 最简）

键 `et.world.*`：day/period/repairFee/batch/tutorial/ownedParts(逗号分隔)。
`Save()` 写、`LoadOrNew` 读，读不到则新档。完整 WorldSave DTO 往返逻辑已由 b22/b24 覆盖，
本批只做可玩层的轻量持久化。
