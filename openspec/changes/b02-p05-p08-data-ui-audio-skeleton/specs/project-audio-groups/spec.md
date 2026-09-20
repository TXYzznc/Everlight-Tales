# project-audio-groups

## ADDED Requirements

### Requirement: 三个音频分组可用

项目 SHALL 提供音乐、音效、UI 音三个音频分组，分组 SHALL 在 `SoundGroupTable` 中登记并可在运行时按分组播放。

#### Scenario: 按分组播放

- **WHEN** 分别向音乐、音效、UI 音分组请求播放
- **THEN** 三者均从各自分组播放
- **AND** 不互相抢占同一分组的播放名额

#### Scenario: 分组登记与配置一致

- **WHEN** 检查 `SoundGroupTable`
- **THEN** 音乐与音效沿用框架既有分组
- **AND** UI 音分组已登记且带有合理的代理数量

### Requirement: 音量与静音生效

项目 SHALL 提供音量与静音的设置入口，设置 SHALL 立即影响对应分组的实际输出。

#### Scenario: 静音后无输出

- **WHEN** 对某分组开启静音
- **THEN** 该分组不再产生可听输出
- **AND** 其它分组不受影响

#### Scenario: 音量调整立即生效

- **WHEN** 调整某分组的音量
- **THEN** 该分组的输出响度随之改变
- **AND** 正在播放的声音在同一次调整后即受影响

### Requirement: 音频通路先于素材

本批次 SHALL 只验证通路可用，SHALL NOT 依赖正式音频素材。占位音或框架自带音 SHALL 足以验证分组、音量与静音三项行为。

#### Scenario: 无正式素材也能验证通路

- **WHEN** 工程中尚无正式音乐与音效素材
- **THEN** 仍可完成分组播放、音量与静音的验证

### Requirement: 音频设置的持久化边界

音量与静音设置在本批次 SHALL 允许仅存在于内存态，其持久化 SHALL 由存档服务批次承担。

#### Scenario: 本批次不要求跨启动保留

- **WHEN** 重启应用
- **THEN** 本批次不要求音频设置被保留
- **AND** 该要求在存档服务批次中补齐
