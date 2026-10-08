## ADDED Requirements

### Requirement: Four transient feedback types do not block input

项目全局短提示 SHALL 使用SHR-041 info/success/warning/danger，自动消失、不生成DialogView遮罩、不接收射线，避开当前页面底部操作区。信息/成功 SHALL 停留2.5秒，警告/错误4秒，计时不受游戏暂停影响。

#### Scenario: Warning appears during paused UI interaction

- **WHEN** 条件不足产生warning短提示且游戏处于暂停状态
- **THEN** 提示显示4秒后消失，仍可操作底层允许的暂停菜单控件
- **AND** 不显示全屏Dimed或确认按钮

### Requirement: Bounded queue and duplicate merging

短提示 SHALL 同时最多3条、间距12，1秒内同类同文合并刷新并显示次数，队列最多10条；满时 SHALL 先丢弃最早info/success；无低优先级消息时，新danger替换最早warning或最早danger，新warning仅替换最早warning（没有则丢弃新warning），新info/success丢弃。剩余消息保持原次序，已展示消息不截短。

#### Scenario: Repeated insufficient material warning

- **WHEN** 同一warning在1秒内重复产生
- **THEN** 合并为一条显示次数与重置停留，不叠满三条重复消息

### Requirement: Decisions and loading retain distinct lifecycles

需要确认/选择/阅读后处理的信息 SHALL 使用原DialogView或专用流程；Loading SHALL 保留原输入阻挡与关闭生命周期、使用独立SHR-042装饰和纯色阻挡底，不按短提示时长自动关闭。

#### Scenario: User requests deletion

- **WHEN** 删除存档需要确认
- **THEN** 使用SHR-002弹窗及SHR-024危险确认/SHR-022取消，不使用自动消失提示执行决定

#### Scenario: Loading completes or fails

- **WHEN** 原加载流程完成或失败并通知结束
- **THEN** 阻挡层正确关闭，后续结果按短提示或操作弹窗显示，不遗留永久遮罩
