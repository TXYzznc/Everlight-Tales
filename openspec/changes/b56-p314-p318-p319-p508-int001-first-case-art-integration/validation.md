# 首案正式美术接入验收

2026-10-09；对应D-091四轮确认及P3-014、P3-018、P3-019、P5-008、INT-001。用户开发任务工作簿保持只读。

## 实现结果

正式Launch存档入口已贯通：新档序章 → 三段工作台教学 → 地图沈遥对话 → 夜间舞蹈教室调查 → 准备预览 → 红舞鞋三轮盘面 → 失败/撤退重试或成功 → 上午回访。

玩家、沈遥和红舞鞋按身份/对象使用已有Sprite；旁白及未知人物不借用其他身份图片。舞蹈教室夜景、六个导流标记、转向器、维修匣及封存入口进入实际盘面，图片不拦截操作。修复开场Canvas排序、盘面不透明衬底遮挡和70处既有TMP字体材质丢失引用。

三个PlayerPrefs存档位保存案件阶段、回访发奖标记及地点解锁；旧档缺少新增字段安全加载，已完成三段但没有完成标记的旧教学进度可进入首案。删除存档清理新增字段，重复调查、结算和回访不会重复处理。

## 完整性与需求对应

本批7项任务完成。4项需求均有实现和运行证据；需求与代码对应如下。

| 需求 | 主要实现 | 验收覆盖 |
| --- | --- | --- |
| 正式首案入口 | `FirstCaseContent`、`FirstCaseFlow`、`MapPanel`、`DialoguePageForm.Flow`、`InvestigationPageForm` | 从正式存档按钮开始；三段真实教学拍击；取消/完成对话；三个调查热点及取消耗时 |
| 精确身份图片 | `FormalPortrait`、`FirstCaseArtMigration`、`FirstCaseBoardArt`、`BoardPageForm` | 开场玩家、沈遥对话、调查图、两侧人物、红鞋预览及真实盘面截图 |
| 首案进度保存 | `WorldSession.FirstCase`、`WorldSession.Restore`、`SaveSlotService` | 对话/调查/成功/回访后重载；三槽隔离；删除；旧档教学；中断维修；重复结算和回访 |
| 三轮接线与运行证据 | `BoardGame`、`RedShoeLevelBuilder`、`WorldSession.FirstCaseRewards`、`SettlementPageForm` | 三轮真实拍击、合法机械臂、四选一奖励；特殊目标失败；暂停撤退；成功封存与回访 |

页面回收清理选项和回调；剧情完成回调检查拥有者会话。运行负例验证旧对话在切换会话后完成，不会推进旧槽或新槽。盘面结算和异步撤退也检查拥有者。

## 编译、资源与正式运行

- Unity 2022.3.62f3c1普通编译完成，最终编译作业`55580cde`，`hasErrors=false`、`errorCount=0`；结构/字段修改均在停止Play Mode后编译，未用FSR代替编译。
- `正式资源规则/验证资产`：`prefabs=26; semanticEntries=522; errors=0`。目录覆盖241张非Placeholder图片；无重复语义键、空Sprite、丢失Prefab引用或测试资源依赖。目录登记与实际首案画面分别验收。
- `首案接入/验证规则与资源`：7个必需Sprite、两个盘面人物挂点通过；真实九拍路线三个轮末累计108/176/176分，通过30/100/170目标并完成导流3/6、6/6、封存匣2/2和红鞋封存。
- `首案接入/运行正式流程验收`：正式GF.UI流程报告`PASS ALL`，最终运行后Console错误数0。验收通过UI按钮和盘面操作API执行，不注入目标完成标记或得分。
- 运行验收会先备份三个存档位相关PlayerPrefs，最后在finally/OnDestroy恢复；验收结束备份文件已清除，Editor停止Play Mode。
- OpenSpec严格验证通过；代码与文档diff检查通过。Unity序列化Prefab的新空字段行保留其默认格式，普通`git diff --check`会报告这些字段行末空格，不手工改写YAML。

开发机可复核的原始证据（Library为可再生、不入库目录）：

- `Library/FirstCaseValidation/rules.txt`
- `Library/FirstCaseValidation/runtime.txt`
- `Library/FormalResourceValidation/assets.txt`
- `Library/FirstCaseValidation/01-opening.png` 至 `06-revisit.png`

复现：停止Play Mode，执行`首案接入/更新首案资源引用`及`验证规则与资源`；打开正式Launch场景，执行`准备正式流程验收`，进入Play Mode后执行`运行正式流程验收`。如验收因外部终止留下备份，停止Play Mode后执行`恢复验收存档备份`。

## 连贯性与实际边界

Data/Board/Events保留无Unity引擎引用；Meta沿用案件状态机；UI使用GF.UI生命周期并编排已有服务。未修改`ScriptsBuiltin`，未新增包、图片或音频。Prefab通过Unity API迁移，GUID保持不变，生成器继续调用同一迁移。

保存范围为案件阶段和地点。维修中断后保留调查结论并回到待维修，从本关初盘重试；未实现逐拍快照或同次维修的中间局面恢复。

通关证据证明存在合法路线，轮间通过实际抽取的BF-014获得每拍维修能量，再执行两次合法机械臂。随机奖励未提供所需选项时，验收通过正式撤退重试，不修改奖励池抽取结果。未证明全部随机奖励组合可解，也未完成数值平衡、连锁手感或真人试玩校准。

本批实际画面在1080×1920 Editor Game View复核；移动端设备、所有宽高比、异步资源加载失败和进程强杀未进行完整测试。未知身份隐藏沿用已验收的FormalPortrait；本批没有新增缺失人物的替代图片。音乐音效、后续十四案和新美术不在范围内。

## 既有框架纯度问题

`python tools/audit_framework_purity.py`仍报告实施前相同的4项失败，没有本批新增失败：

- `.claude/skills/unity-skills/skills/scene/SKILL.md`：sample launch identifier。
- `.claude/skills/unity-skills/skills/ui/SKILL.md`：sample launch identifier。
- `.claude/skills/unity-skills/skills/ui/UI_REFERENCE.md`：sample launch identifier。
- `.claude/skills/unity-skills/skills/sample`：sample artifacts are forbidden。

这些既有Skills改动不属于本批，不予覆盖。工作区既有PNG、包配置和其他Skills修改保留；本批没有Git暂存、提交或推送。
