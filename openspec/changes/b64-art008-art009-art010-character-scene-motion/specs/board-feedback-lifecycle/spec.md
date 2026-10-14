# board-feedback-lifecycle

## Purpose

为旋转、咬合、震波、连锁和目标反馈建立一致的播放生命周期，使玩家看得清结算顺序，并在跳过、关闭页面、重新进入和恢复存档时保持同一逻辑结果和干净的视觉状态。

## ADDED Requirements

### Requirement: Feedback reflects resolved gameplay

盘面表现 SHALL 按结算结果顺序展示反馈，不通过动画回调重复执行伤害、发奖或世界结算。

#### Scenario: Animation finishes twice

- **WHEN** 反馈完成通知重复或快进与完成同时发生
- **THEN** 逻辑结算和奖励仍只执行原有一次

### Requirement: Skipping reaches the same final view

播放完成与跳过 SHALL 得到相同最终视图，输入恢复须等待当前逻辑事务与表现收束，不能由倍率绕过逻辑锁。

#### Scenario: Skip during chain

- **WHEN** 玩家在连锁反馈期间快进
- **THEN** 表现收束到已确定结果，未完成逻辑前不能发起下一次输入

### Requirement: Closing cancels previous playback

关闭、重开、恢复 SHALL 使旧页面或尝试的表现失效并释放临时实例与订阅，旧回调不能修改新视图。

#### Scenario: Close and immediately reopen

- **WHEN** 盘面震波播放中关闭后立即重进
- **THEN** 无旧粒子或飘字残留，旧完成回调不解锁或写入新页面

### Requirement: Recovery reconstructs current state

恢复 SHALL 直接显示已恢复轮起点，不重播已完成的奖励/上一轮结算表现。

#### Scenario: Recover at next round

- **WHEN** 读取已含奖励选择的下一轮快照
- **THEN** 显示该轮当前状态，既有奖励动画和结算不再次触发
