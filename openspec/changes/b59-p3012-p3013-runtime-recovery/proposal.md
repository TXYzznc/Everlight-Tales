# 正式世界存档提交与启动恢复

## Why

`WorldSession.HasAttemptSave` 恒为 false，恢复页继续/放弃仅是占位动作，红鞋维修中读档会退回待维修。需要把 b58 的真实快照与结算结果日志接入选档、世界写入和 GF.UI，使玩家能够继续同一次维修且不重复结算。

## What Changes

- 覆盖原任务 **P3-012、P3-013**（G3 修复收尾）；原依赖为 P3-001/P2-011、P3-012；新增实施依赖 b58。P6-004 仅作为后续整体回归，不跨 G6 纳入本批。
- 保留现有三个槽位与 PlayerPrefs 世界档格式，补结算绝对结果写入、世代/修订身份与提交凭据。
- 选档后先恢复未完成结算，再检查维修快照，最后进入世界或 GF.UI 恢复页；接通继续/放弃真实行为。
- 快照及备份均不可用的未结算尝试保留世界进度并无罚重开；旧档无快照按既有规则进入，不伪造续轮。
- 对槽位切换、删除、新建、页面关闭与旧回调加隔离；Editor 正式流程验证继续、撤退、结算中断、损坏及三槽隔离。

### 状态与边界

方案依据D-324；用户已授权整套方案开始执行，本批恢复接线实施中，实际结果以tasks及verification为准。本批不创建新事件/试机/档案入口，不扩展后续十四案，不修改红鞋数值，不做音频、真机发布、纯度审计修复或数据生成一致性治理。全部收尾范围已按最新要求形成 b58～b67，见[统一审阅入口](../../../Docs/GameDesign/10-开发计划/收尾批次方案审阅.md)，本批不替代其它批次的具体设计。

## Capabilities

### New Capabilities

- `repair-recovery-flow`：按槽位启动恢复、无罚降级、真实继续/撤退及生命周期隔离。
- `world-settlement-persistence`：PlayerPrefs 世界结果提交、修订校验与结算日志重放。

### Modified Capabilities

无。主 specs 尚无对应业务能力；依赖 b58 新能力但不重复声明其所有权。

## Impact

拟修改 UI/WorldSession 及其 FirstCase/Rewards 分部、SaveSlotService/SaveSlotPage、RecoveryPageForm、ServicePanel、正式页面导航和必要的 Meta 恢复状态。业务层五程序集引用方向保持不变；不改 ScriptsBuiltin、依赖包、任务工作簿或正式配置数值。Prefab 如需改引用，使用 Unity API 保留 GUID。

方案见 [design.md](design.md)，上游见 [b58](../b58-p2010-p2011-p2012-attempt-checkpoints/proposal.md)。
