# 变更提案：b24 G3 一天闭环验收门

覆盖任务：P3-020（G3 一天闭环验收）。

## 动机

G3 里程碑（阶段3 一天闭环）要求「新档能完整描述并保存一天」：城市地图五区与地点解锁、四时段与时间推进、昼夜刷新、任务系统三页、世界存档与维修尝试存档双轨、启动恢复面板，且结算与中断不重复扣时/发奖。b01～b23 已逐批落地全部构成件，本批做全链路集成验收并关闭 G3 里程碑。

## 关键决定

- 集成验收探针 `G3AcceptanceRunner`（验收后删除）：按 P3-020 验收描述「新档→白天事件→夜晚怪谈入口→盘面→结算→跨天→存档恢复」串联 b13～b23 全部构成件：
  - 新档：`WorldState` + `TimeState` 第 1 天上午。
  - 地图五区 + 解锁：五地点（家/街区小店/舞蹈教室/裁缝铺/社区活动中心）注册与剧情解锁。
  - 四时段与时间推进：上午→下午→夜晚→深夜→次日清晨跨天。
  - 昼夜刷新：`SupplyService.Refresh` 每昼夜各 1～4 个普通实例。
  - 夜晚怪谈入口：`InvestigationService` 创建 L-01 红舞鞋。
  - 盘面→结算：`InitialBoardBuilder` + `RepairEventShell`（卷帘门）成功发奖；`SettlementTransaction` 五步推进时间 + 防重；`WorldSettlementService` 写档 + 防重不重复发奖。
  - 跨天：深夜→第 2 天上午。
  - 双存档 + 恢复：`WorldSaveService` 往返 + `RecoveryService` 恢复面板（继续/各只一次）。
  - 任务三页：`EventPageLayout`/`TaskPageLayout`/`CasePageLayout` 分组排序。
- 地点目录补第五区（`PlaceCatalog` 增社区活动中心），满足「五区」口径。

## 变更范围

- Data：`PlaceCatalog.FirstBatch()` 增「社区活动中心」第五区。
- 探针 `G3AcceptanceRunner.cs`（验收后删除）。

## 验收

- UnitySkills Play Mode 集成探针（23/23，验收后删除）+ 程序集布局/纯度静态检查 + 编译 0 错 0 警。
- G3 里程碑关闭。

## 边界（本批有意留出）

- G3 仅验收纯逻辑一天闭环；触控/分辨率适配、正式 UI/立绘/音频等表现与教学自走验收在 G5/G6。
- 成长闭环（加工/经济/家园/图鉴）自 b25（G4）起。
