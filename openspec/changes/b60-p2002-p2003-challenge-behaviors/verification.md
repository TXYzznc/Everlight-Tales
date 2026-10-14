# b60 障碍/异常实施证据

2026-10-14，实施中；完成项以tasks为准，最终正式流程回归已通过。前置b58/b59已联合验收：快照Version=2，RulesVersion=2；protocol 36项、world 65项、正式GF.UI 99项均PASS ALL。正式基线为三教学、红鞋每拍1步及卷帘门；没有设备或真人试玩结论。

## 1.1 基线参数消费矩阵

| 类型 | 现有消费 | 待补 |
|---|---|---|
| O001 | 固定/耐久0 | 明确普通损伤/移动拒绝验证 |
| O002 | MaxDurability、普通扣减 | RootBlastOncePerRoot与根传播 |
| O003 | InitialOpen、孤立ToggleGate方法 | ToggleOncePerRoot、DeferCloseWhenOccupied及设施叠层 |
| O004 | PassDirection存在辅助Redirect | 正式步进消费/非默认方向 |
| O005 | ClampedEntityId辅助方法 | AllowTags、实例初始绑定、ReleaseWindowTaps、真实移动限制 |
| O006 | PassDirection辅助阻挡 | ToggleDirection、统一入口方向查询 |
| O007 | 辅助邻格生长 | GrowthPerTap共享根预算、SegmentMaxDurability、稳定顺序与拍末接线 |
| O008 | 基础耐久 | AttachedPartitions显式关系、ProtectedObject绑定事故、Repairable与存活校验 |
| A001 | LocalSettle辅助Deflection | Priority、Controllable、正式定势消费 |
| A002 | 辅助传送/UseCount/Threshold | MaxExpansion、失败不计数、合法出口及正式移动钩子 |
| A003 | 仅类型/参数 | EchoQuota、Resettable、NoEchoOfEcho、成功声音来源和后置复制 |
| A004 | 辅助Silent判断 | SuppressStoryNodes、Controllable、正式能力抑制 |
| A005 | 辅助SettleDirection | StepLength、轮后移动 |
| A006 | Heat/Threshold/CoreCount辅助 | RechargeAmount、正式能力计热/补能/耗芯 |
| A007 | 仅类型/参数 | MaxLayers、PerRoundTargets、登记载体/游标/校准/同轮一次 |
| A008 | TapPeriod辅助TickTap | FlipDegrees、触发重置周期与拍末整理 |

固定元素目前只有实体占位；无InstanceData或稳定行ID关系。BoardState只有一个实体占用表，开闸不能与零件共格，夹持不限制实体IsMovable。AnomalyService辅助方法尚未接正式TapSettlement/BoardGame，不能把方法存在算作实际消费。

## 管线事实与实施约定

Board/SettlementEventQueue驱动实际盘面；Events/FifoAbilityEventQueue由CollisionTriggerResolver从结算事实翻译，供表现和复现。后者不再次执行能力。因此统一来源在Board生成并随SettlementEvent→AbilityEvent保留，不能翻译时生成新根；保留两队列既有职责，不重复执行盘面效果。普通碰撞保留，不全局按源/目标去重。范围内临时集合只在事务内复用/清理，不在Update分配。

后续逐项追加源码/用例/结果。未完成行为、快照扩展、普通编译、EditMode组合及正式回归保持未勾选；b57遗留只有对应证据通过后才同步承接。

## 实施增量

- EffectProvenance/SettlementContext/SettlementEventQueue→CollisionTriggerResolver/AbilityEvent保留根/父/来源；10项纯来源EditMode断言通过，包括同拍独立根、派生分支、合法重复事实、后置及直接发出无队列子项。
- BoardState加入设施登记和统一入口，门占格延迟/同根切换、夹持及机械臂限制、非默认百叶方向与同根爆破检查通过。快照新增设施运行/关系字段，Version及RulesVersion=3，重建不施加进入效果，双向夹持图校验与v2拒绝通过；异常新增字段尚待扩展。
- Library/BoardChallengeValidation/editmode.txt为当前增量证据；阶段性PASS ALL只覆盖已列用例，不代表本批全部规则完成。
- 定向表修改：Artifact Tool包缺index.js无法导入，按Spreadsheets允许的备用方式使用内置Python/openpyxl；原23行A:Q值/类型/格式重开对照一致，只追加R列InstanceData且旧数据为空。Library/spreadsheet保留before/after预览和临时副本；用户任务表未读写。
- FixedElementRelations严格按类型白名单解析，FixedElementBinding先验证本关行图再映射运行ID；GameConfigRuntime在发布目录前验证，InitialBoardBuilder在抽随机前再次保护。正例包含联动开关、支柱附属/保护、初始夹持和封条根；重复/悬空/跨关/数量/类型/环/非法坐标及旧空行均通过EditMode断言。
- 固定元素表经既有Excel2TxtFile/DataTableGenerator公开API定向生成，表头检查及--check --only通过。既有xlsx监听器曾全量重生成C#，已逐差异核对并撤回本次产生的无关BOM/格式变化；当前数据生成物仅LevelFixedElementTable.xlsx/.txt/.cs改动。未改监听器或治理其它表漂移。
- 最新EditMode记录72项状态断言PASS ALL（普通编译e365e359，0错误）。包含非默认轨道剩余步长、原子移动前置拒绝、端点共格、夹持停止步进；多段共享生长预算/配置段耐久/重复拍末/满邻格，真实铆合触发按显式费用切根且既有段保留，完整释放窗口/离开重入，支柱绑定事故与未绑定不误报。测试目录初态为空时以Editor受限反射归还PartCatalog空态，不增加生产清空接口。
- 设施快照新增最大耐久、维修效果分及拍末序号；重建配置并验证，不能把段容量猜成根容量。阶段断言只覆盖已实现障碍，回声/重绘/其余异常、正式组合回归及最终严格检查仍待完成。

## 异常接线与事件契约（第三批增量）

- AnomalyRegion使用单调实例ID；重叠按Priority降序、ID升序选择，已耗尽/停用的选中区域不回退。Cells/ExpansionCells防御性复制，非法格/重复实例在注册时拒绝。
- SoundService.Emit只用于实际释放的逻辑声音，Forward用于中继；未释放缓存不调用Emit。原声传播进入Board FIFO，回声保留发射格/端口，在根的所有原传播子项完成后派生；Origin=Echo永不再登记回声。桥与接收端按根+Original/Echo各一次，门/夹持释放/百叶按根一次；普通碰撞不按该限制去重。
- Board事实经CollisionTriggerResolver翻译到Events队列时保留RootEventId/EventId/ParentEventId及Origin；SoundReceived/ControlApplied也作为事实翻译，不二次改变盘面或重算收益。声音端点通过LinkedElementIds发出明确控制；普通冲击不冒充声音或校准。
- A001进入真实重力路径；路径版本在推移/控制/生成/移除后更新，纯重力环路有事务内访问界限。A002接所有Move/MoveGroup路径，出口先验证、不覆盖占用，成功数按Threshold整倍数扩张并受MaxExpansion限制；每次进入请求的裂隙链有实例访问界限。Preview独立模拟落位、出口和扩张计数，不修改真实状态。
- A004在主动能力前抑制，保留触发分；SuppressStoryNodes区分普通零件与固定端点。A006只对实际支付充能执行的普通零件计热，按RechargeAmount补到容量上限，补能不重新计热；耗芯归零或校准关闭后不再执行，校准不复活芯。
- A005依据AR-005及条目中的“本拍开始/初末判定”：主连锁和常规拍末后检查拍首站位，同格普通可动实体按StepLength推进一次，新增实体没有拍首记录。它属于拍末检查，A007才是轮末；不把文本中的“轮后”误做每轮一次。声明InstanceData `StabilizesSettling=true` 的RepairTarget必须适配RivetPliers，实际铆合完成才关闭所在沉降区；普通校准不替代维修。
- A007明确RegisterCarrier，只处理Part/TaskMarker，按登记顺序轮转，跳过满层/离区/已销毁载体，最多一遍扫描。ApplyRoundEnd是A007与未来LR-PS-03共同入口，相同roundIndex重复调用无变化；目前C002/LR-PS-03未开放，不声称已实现后置案件。真实BoardGame在本轮合法通过后调用，早于下一轮恢复点。
- A008在非终局拍末按TapPeriod计数，达期按FlipDegrees旋转并立即重力整理；计数归零，不额外扣拍。声明接受校准的节点经CalibrationProbe冻结下一次翻转，届时消耗冻结并重置周期。最后一轮最后一拍是终局拍（成功/失败均不追加翻转）；中间轮末正常执行。

## 状态和版本

Version=3、RulesVersion=3，在b58/b59的v2基线上统一增加设施/异常状态。保存NextAnomalyId/LastAnomalyTap、实例ID、回声余量、登记载体、游标、LastRedrawRound、停绘/稳定/冻结标记及原有热/芯/拍计数；稳定器标记与实体配置一起保存。恢复校验额度范围、载体身份/种类、周期、实例ID；恢复时不触发进入或重绘。事务内根/受限集合、队列、访问集合、拍首位置不持久化，恢复重放生成相同来源和顺序。v2无法无损解释时沿用b58备份优先、否则保留世界进度无罚重开策略。

## 当前验证

普通Unity编译a2fe01dc：0错误，未使用FSR。Library/BoardChallengeValidation/editmode.txt：112项状态断言PASS ALL；包括两跳声音/原传播完成后回声/跨区不递归/重叠耗尽不转交/独立根再次转发、控门一次、共享轮末去重、满层有界扫描、揭露后再绘、完整状态与事件顺序恢复重演、裂隙失败与上限、预演无副作用、两枚独立局部重力环同时终止、沉降非默认步长及真实铆合稳定器、非默认120度翻转/冻结/终局跳过、过载补能及静默触发分、非法必需参数。

这些是EditMode人工构造状态夹具，不代表后置案件已可玩。正式Launch入口的三教学、红鞋和卷帘门仍以本次runtime.txt最终结果为准；旧b58结果不能替代新规则的回归。

## 正式回归与交接

2026-10-14 03:06:46，Library/FirstCaseValidation/runtime.txt新规则下99项PASS ALL，覆盖三教学正式按钮、调查、红鞋三轮/自然奖励/合法机械臂与恢复、卷帘门完整成功/中断恢复/失败/撤退及终局奖励。protocol.txt 36项、world.txt 65项也重新运行PASS ALL；隔离测试备份均已归还。所有正式结论仍只针对记录的路线，不外推全奖励可通率。

verify_project_assemblies通过（5个程序集无环，Board/Data/Events引擎隔离）；固定元素表头及源表结构定向检查通过。git diff --check排除原生TSV空末列通过；TSV末列为空必须保留分隔符，不能为了空白检查剥掉InstanceData列。未执行设备/发布验证、未改用户任务工作簿、未暂存/提交。

b60全部15项完成，移交b61的挑战显式装配和当前规则下正式回归；b57全量生成漂移仍属于暂缓第8项，不因此关闭。
