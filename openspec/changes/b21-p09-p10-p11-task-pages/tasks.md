# 任务分解：b21 任务系统三页

覆盖任务：P3-009、P3-010、P3-011。

- [x] 1. Data：`TaskConfig` 增 IsMain/Client/Description/TotalSteps；`CaseConfig` 增 Source/FirstPlace
- [x] 2. Meta：`TaskState` 增 CurrentStep/TotalSteps/LastUpdated
- [x] 3. Meta：`TaskService.cs`（Advance/CanClaim/Claim + TaskClaimResult）
- [x] 4. UI：`EventPageLayout.cs`（EventGroup/EventEntry/EventPageLayout + NextOpen）
- [x] 5. UI：`TaskPageLayout.cs`、`CasePageLayout.cs`
- [x] 6. UI：`JournalPages.cs`（EventListPage/TaskListPage/CaseListPage 表现层壳）
- [x] 7. 探针 `B21AcceptanceRunner` 验收（26/26，验收后删除）
- [x] 8. 静态检查（meta/程序集布局/纯度）+ 编译 0 错 0 警
