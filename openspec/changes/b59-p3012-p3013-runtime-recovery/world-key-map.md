# 世界键映像与恢复接线

2026-10-14，已实施并联合验证通过。三个槽位继续沿用PlayerPrefs：槽位1保留et.world.*，槽位2/3插入编号。

| 类型/位置 | 键/内容 |
|---|---|
| int业务键 | day、period、remainingCells、repairFee、batch、tutorial、jobs、tutorialStage |
| string业务键 | owned、known、materials（含SourceCase）、blueprints、forms、currentForms、tasks、display、firstCase、places、runtime、ledger、homeState |
| 另有string元数据 | et.world.meta.槽位：UTC保存时间 |
| runtime JSON | 种子/消费数/内部随机状态、供给批次日/半日/epoch、当前实例身份/模板/昼夜/已处理、开场完成、待注入机械臂、跟踪任务、Buff及层数、冻结终局候选与结算摘要 |
| 世代/活动身份 | generation、activeAttempt；删除前持久deleting=1并失效旧世代，再清理本槽目录与世界键 |
| 单值提交凭据 | commit JSON含Revision、AttemptId、ImageHash；旧revision/receipt/receiptHash读取仅作兼容 |

PlayerPrefsWorldImageStore持有字段类型/键白名单；捕获、校验和删除共用注册表。映像明确Integer/String/Delete，不枚举或覆盖所有PlayerPrefs，拒绝跨槽/未知键/不完整映像。凭据校验使用排序归一后的受控结果摘要。

选档先对账结算日志，再读取世界，再按当前配置校验活动快照。继续重建共享盘面/轮/红鞋，放弃进入正常撤退日志协议；主备均损坏仅隔离尝试并保留世界，结算日志优先。关闭恢复页回选档。普通结算后尚未领取的四选一在重启后继续同一候选，领取本身用独立绝对结果日志防重复。
