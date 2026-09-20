# 设计：b11 四选一框架 + Buff 数据结构与首批条目 + 暂停与撤退菜单 + 关卡盘面配置表+教学样张

## D-112：Buff 配置在 Data、运行时持有状态在 Events

- `BuffConfig{Id,Name,Stage,Stackable,MaxStacks,Description}` 与 `BuffCatalog`（BF-001～BF-020，见 13-Buff库.md）落 Data。
- `BuffState{Config,Stacks,IsMaxed,AddStack}` 与 `BuffSet{Add,TryGet,IsExcluded}` 落 Events：不可叠加已持有或满层时 `AddStack` 失败；`Add` 满层／不可叠加重复返回 null。
- Buff 效果接入结算留待 b14（Buff 库+M 类）；本批只落地数据结构与首批条目。

## D-113：四选一框架在 Events

- `RewardOption{Kind,Id,Label,BuffConfig}`、`RewardKind{Buff,Part,ArmMove}`、`RewardChoiceService.Draw(rng, pool, held, count=4)`。
- 生成前移除满层 Buff 与已持有不可叠加 Buff（`BuffSet.IsExcluded`），无放回抽 `count` 个按 Id 互不相同的选项；零件奖励不因曾抽取被移出池（D-058）。

## D-114：暂停与撤退菜单在 UI

- `PauseMenuView{Resume/OpenSettings/ShowRules/Retreat}` 分发动作并置 `LastAction`、`RetreatRequested` 标记。
- 时间结算、现场回退、怪谈/普通事件撤退分流留待 b15/b16（一局链路）；本批只做菜单动作骨架。

## D-115：关卡盘面配置在 Board

- `LevelBoardConfig{LevelName,SideLength,KeyPieces,CarryPool,InitialPartCount,InitialArmMoves,InitialPublicRepairEnergy}` 与 `KeyPieceConfig{PartType,Position}`。
- 因含盘面坐标 HexCoord，配置落 Board 层（Board refs Data 取 PartType）；`TutorialLevelConfig` 提供 S0 教学样张（边长 5、关键件固定落位、携带池、初盘 6 件、机械臂 2 次、关初公共维修能量 0）。

## 边界

- 本批不接入 Buff 效果到结算（b14）、不做奖励应用与轮间四选一流程（b14/b15）、不做撤退完整流程（b15/b16）。
- 教学样张为示例配置，正式 S0 三段教学在 b32 定稿。
