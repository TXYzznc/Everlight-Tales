# board-effect-provenance

## Purpose

定义盘面能力在结算中的来源、派生和轮次边界，确保同根限制不会误伤合法互动，回声不会递归扩散，且相同输入在中断恢复前后得到确定且可解释的结果。

## ADDED Requirements

### Requirement: Root identity follows derived effects

系统 SHALL 区分同拍内的独立根效果，派生效果保留根来源；仅对规则声明受限的目标效果按根去重，不抑制普通合法重复互动。

#### Scenario: One explosion reaches the same partition twice

- **WHEN** 同一爆破根经不同传播路径命中同一隔板
- **THEN** 隔板最多扣一次耐久；同拍另一个独立爆破根仍可扣减

#### Scenario: Ordinary interactions are not globally suppressed

- **WHEN** 两个合法普通碰撞拥有相同源与目标但不同触发
- **THEN** 按既有碰撞规则处理，不因目标曾受击而统一丢弃

### Requirement: Echo is emitted after original propagation

系统 SHALL 只为区域内成功发出的原始声音登记回声，在原始传播完成后按相同发出位置和端口派生一次；回声在任何区域均不得被再次复制。

#### Scenario: Echo enters another region

- **WHEN** 原始声音被复制且回声经过另一回声区
- **THEN** 第二个区域不再复制，不消耗其额度

#### Scenario: Suppressed sound

- **WHEN** 声音因静默或尚未释放的封存而未真正发出
- **THEN** 不产生回声，不消费额度

### Requirement: Echo quota and overlap are deterministic

系统 SHALL 按配置保存跨拍回声额度，成功登记一次消耗一次；合法复位恢复配置值。重叠区按优先级再稳定ID选择唯一处理者，额度耗尽不转交其它区。

#### Scenario: Quota exhaustion and reset

- **WHEN** 初始额度3连续登记三次原始声音后再次触发，再执行合法复位
- **THEN** 第四次无回声；复位后恢复3；不通过重叠区绕过限额

### Requirement: End of tap and round effects have stable order

系统 SHALL 在主连锁结束后、最终失败判定前完成拍末生长；同轮重绘只执行一次并在下一轮恢复点前完成。

#### Scenario: Repeated round completion callback

- **WHEN** 同一轮结束通知重复到达
- **THEN** 重绘与轮后状态只变化一次，下一轮恢复点包含同一最终状态

### Requirement: Recovery preserves observable effect state

系统 SHALL 从轮起点恢复相同能力来源规则和持久状态；恢复重演与未中断运行在相同输入和随机序列下产生相同结果。

#### Scenario: Replay after interruption

- **WHEN** 含回声和重绘的轮内中断后从轮起点重放相同操作
- **THEN** 额度、层数、目标结果及事件顺序一致，不残留上次事务去重记录
