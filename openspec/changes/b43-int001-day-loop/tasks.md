# 任务清单（b43 一天闭环）

- [x] 新增 `UI/WorldSession.cs`：世界会话编排器（新档/继续/刷新供给/开始事件/结算事件 + PlayerPrefs 存档 + 四选一奖池）
- [x] 新增 `UI/MapPanel.cs`：地图页签（TimePeriodBar + CityMapView + PlaceEventPanel）
- [x] 新增 `UI/OpeningOverlay.cs`：序章覆盖层（字幕 + 跳过/开始）
- [x] 新增 `UI/SettlementOverlay.cs`：结算覆盖层（成功发奖 + 四选一 / 失败返回）
- [x] 改 `UI/MainPageShell.cs`：OnOpen 初始化世界会话 + 地图页签装配 MapPanel + 序章/结算覆盖层调度
- [x] 改 `UI/BoardPageForm.cs`：从 WorldSession 当前事件（卷帘门 EV-N01）加载盘面，替换教学样张直开；结算返回
- [x] 编译验证 + Play Mode 运行验收（新档→序章→地图→事件→盘面→结算→四选一→回地图）
- [x] 静态检查（verify_project_assemblies / audit_framework_purity）+ 提交

## 验收记录（2026-09-22）

- 序章覆盖层：新档首进显示字幕，跳过/播完进入地图，0 错 0 警。
- 结算覆盖层：失败「维修失败」+ 返回；成功「维修成功！奖励 40 维修费」+ 四选一（Buff/零件/机械臂），
  选择后应用奖励（Buff 入持有集 / 零件入世界 / 机械臂累计下次注入）并回地图。
- 提交：8e394f0（核心链路）+ 04b4f03（序章/结算覆盖层）。

## 非目标（转 b44）

- 事件/任务/怪谈三页完整列表渲染（b44 与任务页收口）。
- 工作台/图鉴/背包 UI 接世界状态（b44）。
