# project-save-service

## ADDED Requirements

### Requirement: 双存档文件

存档服务 SHALL 提供世界存档与尝试存档两个**独立文件**，任一文件不可读 SHALL NOT 影响另一文件的读写。

#### Scenario: 两个存档相互独立

- **WHEN** 尝试存档文件缺失或损坏
- **THEN** 世界存档仍可正常读取
- **AND** 尝试存档的失败被单独报告，不导致世界存档被判定为损坏

#### Scenario: 文件名与位置确定

- **WHEN** 存档服务写入任一存档
- **THEN** 文件位于 `Application.persistentDataPath` 下的存档目录内
- **AND** 两个存档使用不同的文件名

### Requirement: 独立版本号

每个存档文件 SHALL 独立携带版本号。读取时 SHALL 依据版本号决定接受、拒绝或迁移，SHALL NOT 在版本不匹配时静默按当前格式解析。

#### Scenario: 版本高于支持范围时拒绝

- **WHEN** 读取一个版本号高于当前支持范围的存档
- **THEN** 读取被拒绝并给出可辨识的错误
- **AND** 不尝试按当前格式解析

#### Scenario: 版本号随写入更新

- **WHEN** 写入存档
- **THEN** 文件内携带当前版本号

### Requirement: 读写往返一致

存档服务 SHALL 支持"写入后读回得到等价内容"，SHALL 对同一份数据重复往返保持稳定。

#### Scenario: 往返一致

- **WHEN** 写入一份已知内容并立即读回
- **THEN** 读回的内容与写入内容等价

#### Scenario: 重复往返稳定

- **WHEN** 对同一份数据连续执行多次"写入—读回"
- **THEN** 每次读回的内容一致

### Requirement: 损坏存档的可辨识处理

存档文件损坏时，存档服务 SHALL 给出可辨识的失败结果，SHALL NOT 把损坏静默当作"无存档"。

#### Scenario: 损坏文件不被当作空存档

- **WHEN** 存档文件内容被截断或替换为非法内容
- **THEN** 读取返回失败并标明是损坏
- **AND** 调用方可以区分"损坏"与"从未存在过存档"

### Requirement: 读写时机接口

存档服务 SHALL 暴露读写时机接口，使调用方能在明确的时机请求读或写，SHALL NOT 依赖隐式的每帧自动写盘。

#### Scenario: 写入由调用方时机驱动

- **WHEN** 调用方在指定时机请求写入
- **THEN** 该次写入发生在请求时刻
- **AND** 未请求写入时不产生额外的写盘

### Requirement: 存档产物不进版本库

运行时存档文件 SHALL NOT 出现在版本库的受版本控制路径中。

#### Scenario: 存档不会污染仓库

- **WHEN** 游戏运行并产生存档文件
- **THEN** 版本库状态不因存档产物而变化
