# b61 挑战装配与正式一局回归

2026-10-14，实施中，最终验收项以tasks为准。前置b60：112项EditMode规则、36项协议、65项世界恢复、99项正式GF.UI回归通过。下述新装配仍须当前版本复验。

## 参数权威与启用边界

| 输入 | 权威来源 | 当前值/用途 |
|---|---|---|
| 案件→挑战 | Board/ChallengeTable C-001 | CaseKey=L-01、LevelKey=REDSHOE、RuleId=CR-001、Stage=S1 |
| 投放条件 | Meta/StageReleaseTable C-001 | ReleaseKind=Challenge、S1；世界入口仍要求教学完成、调查完成和夜间 |
| 关卡/轮次 | Board/LevelTable REDSHOE；LevelRoundTable行9～11；LevelGoalTable | 半径5、固定关键件6、初始机械臂2、耗时4；三轮各3拍，30/100/170分 |
| 规则 | Board/RedShoeTable行1 | 起点(0,-2)、方向D5、匣(2,0)、维修2、入口(3,0)、导流6、束缚上限6、StepsPerTap=1 |
| 导流/转向 | Board/RedShoeGuideTable、RedShoeTurnTable | 按显式SortOrder形成区域/方向序列 |
| 固定实体 | LevelFixedElementTable、LevelKeyPieceTable | 原顺序保留稳定运行ID；通用设施关系由b60解析 |
| 校准报告 | Meta/CalibrationTable | 记录范围/试配结果，不覆写运行参数 |

历史C001正文中的每拍3步与当前表中1步存在冲突。本批保留1步；1/3候选对照归b66，未经实际校准选定不改主配置。ChallengeTable、StageReleaseTable及红鞋/轮目标表没有修改或重新生成。

## 实施说明

ChallengeAssemblyCatalog.Prepare对候选快照执行完整绑定检查，再由GameConfigRuntime发布。显式CR-001注册仅绑定L-01；未启用条目不装配，不通过未知规则退回默认红鞋。可空LevelKey保持后置元数据；可选ExtraData `Enabled=false`明确禁用，`Enabled=true`必须有合法关卡。重复挑战/案件、错误投放类型/阶段、缺案件/关卡/专属规则、非法扩展参数/必需数值均在运行对象创建前拒绝。

WorldSession准备/正式创建/轮起快照/冷恢复及配置指纹都消费同一装配。先创建完整盘面再改变世界案件状态；每次CreateRuntime独立创建实体、Session、鞋状态和可变转向DTO。已有世界案件实例和数值不被替换。CaseEventTable的案件/耗时须与LevelTable.EventKey和Duration匹配。ExtraParamSet.Validate补齐类型错误的owner上下文，避免只报参数而缺表项键。

## 验证状态

- 八张绑定/规则相关表定向表头检查通过；只读核对，不执行全量生成。
- 普通Unity编译ee3a9afa、b57bf74a通过；类型错误诊断修订后待新编译/装配负例和正式流程。
- 新菜单“验证挑战配置装配”使用已加载正式配置的候选副本，不修改已发布目录或玩家世界；输出Library/ChallengeAssemblyValidation/runtime.txt及baseline.json。
- 三教学、红鞋、卷帘门、三槽/旧世界档及中断恢复将在新装配版本下重新运行；不以b60记录替代。

暂缓：b57全量round-trip漂移、第4音频、第7设备/构建/发布、第8纯度/全量生成治理，真人试玩和G6最终验收均未关闭。

## 装配验证增量

Library/ChallengeAssemblyValidation/runtime.txt：23项PASS ALL；baseline.json记录实际参数及来源。未知规则/缺关卡/缺专属规则/缺案件、启用无关卡、重复挑战/案件、ReleaseKind/阶段错误、未知/重复/类型错误/语法错误ExtraData、必需StepsPerTap=0均在候选装配时失败。失败不替换当前目录；显式禁用后不装配，后置14案直接请求被拒绝；两份运行实体/Session/鞋/转向DTO隔离。首次负例发现ExtraParamSet布尔错误缺少owner，已修复并在普通编译e4c792da后重跑通过。

进一步接入世界/地图禁用门槛：未装配挑战关闭线索和维修入口，保留已有案件状态；已结案的回访交付仍按历史世界状态执行。当前普通编译fc4e9c14通过，正式流程正复验禁用请求与准备保护。

## 最终运行结果与移交（2026-10-14 03:24）

普通Unity编译fc4e9c14：0错误；b60规则112项再次PASS ALL。装配23项PASS ALL。新装配正式GF.UI流程runtime.txt于03:24:17完成102项PASS ALL（额外核对C001→CR001→REDSHOE、1步/14案后置、禁用入口不改变案件）。protocol 36项、world 65项均PASS ALL，隔离备份均已归还。固定回归输入沿用[首案路径源码](../../../Assets/Game/Scripts/EverlightTales/UI/FirstCaseRuntimeValidation.cs)：三教学实际按钮、红鞋相位5/1/2/1/5/0/5/5/5、第5/6拍合法机械臂，自然候选BF014；未注入分数或Buff。卷帘门正式路线记录seed=20260922、consumed=59、phases=0/0/3；真实完成、一次中断、失败和撤退均单独验证。世界资源/耗时防重、三槽、旧档和每轮恢复在本版复验。

参数基线见baseline.json；正式结论仅覆盖已记录路线，未形成全种子可解率或真人试玩结论。八张表定向表头检查、5程序集无环/引擎隔离和OpenSpec严格检查通过。

b57对应关系：3.1b/3.2/3.3/3.4→b60的112项纯规则EditMode状态断言；4.3→ChallengeAssemblyCatalog及正式入口；5.3→b60规则与本批23项配置负例（Editor纯规则/PlayMode配置验证，不冒充独立无头Player）；5.4→普通编译及本批102项正式运行；5.5→本文件与b60证据索引。5.2全量round-trip漂移仍未处理，不关闭b57全部遗留/G6。b61全部11项完成，下一批为b62样张与制作清单。
