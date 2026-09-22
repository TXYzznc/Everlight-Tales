# 提案：一天闭环流程接入（b43，INT-001 第三步）

覆盖任务：INT-001 流程接入第三步「一天闭环」——把 b13～b24 已实现（且仅探针验证）的
序章、五区地图、事件、结算事务、世界推进、存档恢复、四选一、时段条串成 Editor 可玩的
完整一天循环。

## 目标

把「纯逻辑 + 无头探针」状态提升为「接入正式 UIForm 流程 + Play Mode 运行验收通过」：

1. **世界会话（WorldSession）**：UI 层运行时编排器，持有 WorldState + TimeState + 随机源 +
   开场服务 + 当前盘面事件，并提供新档／继续／刷新供给／开始事件／结算事件。
2. **序章 → 地图**：新档播放序章字幕（可跳过），完成后进入五区地图。
3. **五区地图**：CityMapView 摆五节点 + TimePeriodBar 时段条 + 地点事件列表；点地点看事件、点事件进盘面。
4. **盘面接入事件**：BoardPageForm 从 WorldSession 当前事件（卷帘门 EV-N01）加载真实盘面，
   替换 b42 的教学样张直开。
5. **结算 → 世界推进 → 四选一 → 回地图**：拍击过关后结算事务推进时间、世界结算发放奖励、
   四选一奖励选择、回地图刷新时段条与供给。

## 前置

- b42 已完成（BoardPage 接 UIFormBase + 输入驱动 + 盘面特效，提交 5f09d30 及后续）。
- b13～b24 已完成（地图／事件／结算／存档／四选一／时段条全部纯逻辑）。

## 验收边界

- Play Mode 下：新档 → 序章（可跳过）→ 五区地图（5 节点）→ 点地点 → 事件列表 → 进盘面 →
  过关 → 结算推进时间 → 四选一 → 回地图时段条刷新。
- 编译 0 错 0 警；`verify_project_assemblies.py` / `audit_framework_purity.py` 通过。
- 全部通过 UIForm 正式流程触发（无 `[RuntimeInitializeOnLoadMethod]` 业务程序集触发）。

## 关键决定

- **世界会话落 UI 层**：需同时触达 Board（TimeState/RandomService/SettlementTransaction）、
  Events（SupplyService/RewardChoiceService/RepairEventShell/BuffSet）、Meta（WorldState/
  WorldSaveService/WorldSettlementService/RecoveryService/RedShoeIntroService）与 Data，UI 是唯一
  同时引用四者的业务层（沿用 b03/b41 结论）。
- **地图宿主 = MainPageShell 地图页签**：不新建独立 UIForm（避免 UITable 增项与预制体登记），
  在主壳地图页签程序化装配 CityMapView + TimePeriodBar + 地点事件面板（复用 BoardPage 的程序化
  装配模式）。
- **序章/结算/四选一 = 程序化覆盖层**：全屏覆盖层组件挂在 MainPageShell，按需显示/隐藏，
  不占用独立 UIForm。
- **存档**：WorldSaveService.Capture/Restore 已就绪，b43 用 PlayerPrefs 存最简世界存档
  （时间/维修费/批次/教学/已拥有零件），继续时恢复；完整 DTO 往返已在 b22/b24 验证。
- **演示事件 = 卷帘门 EV-N01**：RollerDoorEvent 有完整盘面配置（CreateBoardConfig）与
  关卡配置（CreateConfig），是「地图 → 事件 → 盘面 → 结算」闭环的最小可玩实例。

## 非目标（Non-Goals）

- 工作台／图鉴／背包 UI 接世界状态（b44）。
- 四条改装支线接任务页（b44）。
- 事件/任务/怪谈三页的完整列表渲染（本批只做地图→事件→盘面主链路；三页列表在 b44 与任务页收口）。
- 美术立绘/播片/音频（美术批挂接）。
