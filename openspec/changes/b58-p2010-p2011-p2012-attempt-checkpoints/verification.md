# b58 实施与验证记录

2026-10-14，已获整套实施授权。实现及联合验收通过，16/16任务完成。与 b59 联合验收，后续 b60 扩展状态必须同步快照版本。

## 实现对应

| 契约 | 实现 |
|---|---|
| 完整显式快照 | Data/RepairAttemptSave、Board/BoardSnapshotCodec、Events/RepairSaveService；逐字段见[snapshot-map](snapshot-map.md) |
| 文件与身份 | Data/RepairPersistence、Meta/Save/SlotSaveStore；槽位/世代/尝试/序号、版本/类型/UTC/SHA256信封、8MiB限制；同卷tmp/Flush(true)/验证/Replace、备份及损坏隔离 |
| 轮边界与拥有者 | UI/WorldSession.Persistence、FirstCaseRewards、准备/盘面/结算页；入场前及选奖/轮初补给后保存，失败冻结重试，不重抽、不重复施加 |
| 确定结算 | 隔离候选计算→冗余日志→绝对世界映像→单值凭据验证→清理→结算UI；终局四选一使用独立提交身份和同一日志协议 |
| 恢复/旧结果 | 启动先日志后快照；身份与修订拒绝旧记录，双日志坏保留当前世界及问题证据，不猜测补奖 |

所有代码位于 Assets/Game/Scripts/EverlightTales。Board/Data/Events无Unity引用；未增加包、修改框架核心或用户任务工作簿。

## 原始证据

Unity 2022.3.62f3c1，Launch，1080×1920 Editor。结构变更停止Play后普通编译，最近job af88cbf3 completed、0错误；不使用FSR作为验收。

| 证据 | 实际结果 |
|---|---|
| Library/RepairPersistenceValidation/protocol.txt | PASS ALL；完整JSON往返、收纳历史/定时形态/红鞋、相同下一拍/RNG；非法ID/Buff/位置/未来schema、IO分类、槽位/世代/信封版本；真实较早备份及四写入故障点；不可变冗余日志、绝对重放、凭据修复、旧修订拒绝 |
| Library/RepairPersistenceValidation/world.txt | PASS ALL；首轮保存失败、五世界/凭据/清理中断点；三槽与删除、坏档/不兼容；真实供给已处理及跨半日/日；成功日志优先坏盘面，跨段最终值=下午剩3格/40维修费；候选与一次领取 |
| Library/FirstCaseValidation/runtime.txt | 正式GF.UI覆盖三教学、调查、冷选档/关闭恢复、红鞋逐轮恢复、写失败/真实三轮成功/回访、卷帘门成功/失败/撤退及候选冷重开；未选奖励退出及重演扩展PASS ALL（00:55:49） |
| GameData/Diagnostics/Reports/gf-diagnostics-run-all_20261014_004745.json | EverlightTales.Runtime success=true、errors=[]；试机/档案既有服务边界通过，仅夹具证据 |
| python tools/verify_project_assemblies.py | PASS，5业务程序集无环，纯层边界通过 |
| git diff --check | PASS，仅CRLF标准化提示 |

重跑：先“首案接入/准备正式流程验收”，Launch进入Play等配置加载，执行“Save/验证快照与故障恢复”和“首案接入/运行正式流程验收”；协程结束恢复数据后才运行“Save/验证世界提交与槽位恢复”。三槽键逐项备份、文件根切到Library独立目录；finally/OnDestroy恢复，另提供两项“恢复…备份”菜单。验收备份已恢复时文件不存在。证据/临时目录不提交。

## 全部Spec场景映射

| capability及场景 | 对应验证 |
|---|---|
| checkpoints：两次尝试、删档新世代 | protocol跨尝试/槽/世代拒绝；world旧会话/删除世代，formal重新入场 |
| 红鞋第二轮、定时形态 | protocol完整状态/相同下一拍；formal第二轮继续到第三轮 |
| 轮间已选/未选、入场/换轮写失败 | formal真实四选一/冻结重试；未选退出恢复完整起点并重演候选 |
| 较早备份、主备不可用 | protocol实际回退轮次；world备份提示、均坏/配置变化/未来schema无罚 |
| journal：记录失败、世界部分写、提交后未清理 | protocol+world故障矩阵，失败不污染、最终值一次 |
| 终局四选一重开 | world失败重试/已领拒绝；formal冷选档返回同一候选 |
| 清理失败、旧世代/旧修订 | world before-delete/世代；protocol旧修订拒绝 |
| 成功/失败/撤退、重复放弃 | formal合法输入/失败原因/完整耗时；world重复撤退和持久失败重试 |
| 坏快照但成功日志有效、双日志坏 | world成功日志优先；双日志隔离保留字段 |

## 修复与交接

修复恢复页关闭清空引用、新壳初始化覆盖恢复盘面、旧任务固定读slot1、收纳实体ID复用及轮奖励旧ID分配。卷帘门推移目标回填同名轮目标，RulesVersion=2入指纹，数值不变。EV-N01动态供给接真实准备流程；同日同半日保留已处理实例，换批用新身份。普通终局候选冻结、领取防重。

现有四选一文字截断和提示短暂跨页残留交b65视觉/反馈验收；其它供给模板仍有占位入口，交b67内容清单，不能声称全部普通事件可玩。有界搜索曾在增加额外撤退后的一种随机盘面未找到路线，交b66记录；不等于证明无解，也不证明全局可解率。

第4音频、第7设备发布、第8纯度/全表生成仍暂缓；纯度审计保留原4失败，无新增失败。没有真人试玩、真机断电或G6关闭结论。未暂存、提交、推送、同步主规范或归档。
