# 任务清单（b43 一天闭环）

- [ ] 新增 `UI/WorldSession.cs`：世界会话编排器（新档/继续/刷新供给/开始事件/结算事件 + PlayerPrefs 存档）
- [ ] 新增 `UI/MapPanel.cs`：地图页签（TimePeriodBar + CityMapView + PlaceEventPanel）
- [ ] 新增 `UI/OpeningOverlay.cs`：序章覆盖层（字幕 + 跳过）
- [ ] 新增 `UI/SettlementOverlay.cs`：结算覆盖层（结果 + 四选一 + 回地图）
- [ ] 改 `UI/MainPageShell.cs`：OnOpen 初始化世界会话 + 地图页签装配 MapPanel + 覆盖层调度
- [ ] 改 `UI/BoardPageForm.cs`：从 WorldSession 当前事件（卷帘门 EV-N01）加载盘面，替换教学样张直开
- [ ] 编译验证 + Play Mode 运行验收（新档→序章→地图→事件→盘面→结算→四选一→回地图）
- [ ] 静态检查（verify_project_assemblies / audit_framework_purity）+ 提交
