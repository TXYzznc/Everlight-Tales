

## Codely Structured Memories

### User

### Feedback

### Project
- [2026-10-13 17:57:19] 本机（用户 Windows 环境）常同时打开多个 Unity 实例（b5_client/Basketball5_Client ×3、stellar-frontier-auto-era、Everlight-Tales），Codely CLI 的 Unity 连接会因此出现"未连接"的假象，原因是 CLI 侧 TCP 会话失效而 Unity 桥接进程仍活着。原因是每个项目各写自己的握手文件 <项目>/Temp/.com-unity-codely.json（端口随实例不同，不在项目根目录），且全局 Editor.log 会被别的实例覆盖。修复：调用 unity_refresh 重读握手文件即恢复；不要删 Temp/.com-unity-codely.json。
- [2026-10-13 18:22:18] Everlight-Tales DataTable 关键点：新增配置表必须登记进 GameData/DataTables 对应 xlsx 之外的「Assets/Game/ScriptableAssets/Core/AppConfigs.asset → mDataTables 清单」，才可能生成 Assets/Game/Scripts/DataTable/**/*.cs（GameDataGenerator 只遍历该清单，目录里已有的 .txt 不会自动生成代码，且运行时 GF.DataTable 加载也依赖该清单）；.txt 则由全部 xlsx 生成。改表后须退出 Play Mode，经 Unity 菜单「Game Framework/GameTools/Refresh All Excels【刷新所有数据表】」重生成；b57 曾因漏登记导致 ChallengeTable.txt 有、ChallengeTable.cs 无。
- [2026-10-13 18:22:18] Everlight-Tales 重生成配置表会产生无关 churn：上述菜单是全局重生成，而仓库存在既有漂移——7 个 Meta .cs（Batch/CaseEvent/CaseRewardOption/AssetBinding/AudioCue/Bgm/NarrativeTable）被手工压缩成无注释单行形式、与生成器输出格式不符，且导出器会给数据行补行尾制表符。收敛做法（b57 采用）：先备份本批产物 → `git checkout -- Assets/Game/DataTable Assets/Game/Scripts/DataTable` → 回填本批文件。2026-10-13 已把 CalibrationTable（曾列错位）/CaseEventTable/CaseRewardOptionTable/NarrativeTable 的 xlsx 对齐到已提交 .txt（NarrativeTable 浮点列按文本写回以保住 `3.0`）；7 个 Meta .cs 的格式漂移仍未处理，决定前不要提交这些再生变更。
- [2026-10-13 18:39:00] Everlight-Tales 提交口径（2026-10-13 确认）：Packages/manifest.json 与 packages-lock.json 中的 cn.tuanjie.codely.bridge / cn.tuanjie.ai.generators（file: 指向 .codely-cli 的本地 AI 集成）由用户决定**不提交**，改动常驻工作区；.codely-cli/（约 126MB 工具缓存）与 Docs/GameDesign/11-美术规范 - 副本/（手工备份目录）已写入 .gitignore 不入库。提交按任务分批，提交信息用「标题 + 正文 + 验证：」简体中文三段式。

### Reference
- [2026-10-13 17:57:19] Everlight-Tales 权威入口索引：设计文档入口 Docs/GameDesign/目录.md（产品总纲 00、核心循环 01-核心框架/00）；开发路线与批次 Docs/GameDesign/10-开发计划/（分批开发执行计划.md 记录 b01~b56 推进）；配置表源 GameData/DataTables/{Board,Core,Meta}/*.xlsx，导出快照 Assets/Game/DataTable/**/*.txt；中大型变更走 openspec/changes/；框架纯度审计 python tools/audit_framework_purity.py。注意：第一版开发任务表.xlsx 由用户本人维护，AI 只读、不回写。
