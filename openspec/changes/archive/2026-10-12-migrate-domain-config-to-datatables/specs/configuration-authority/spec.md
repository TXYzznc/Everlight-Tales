# Configuration Authority Specification

## ADDED Requirements

### Requirement: Global configuration has one authoritative source

正式玩法内容 SHALL 由 GF DataTable Excel 源定义，并经生成链路进入运行时。代码 SHALL NOT 保留同一正式条目的第二份硬编码来源。

#### Scenario: Load configured part

- **WHEN** 游戏启动并完成 DataTable 加载
- **THEN** 零件名称、数值和显示说明来自配置快照
- **AND** 运行时不读取 `PartCatalog` 中的正式硬编码条目

### Requirement: Configuration is separate from player data

配置表 SHALL 只承载跨账号共享的静态内容。账号进度、库存、解锁、当前形态、当前盘面和维修尝试 SHALL 继续由运行时状态与存档承载。

#### Scenario: Save state does not alter configuration

- **WHEN** 玩家获得零件、消耗材料或改变当前形态
- **THEN** 只改变对应 `WorldState`/存档/维修尝试数据
- **AND** DataTable 配置快照保持不变

### Requirement: Relational content is validated

关系表 SHALL 在运行前检查 ID、外键、枚举、排序、坐标和一对多关系完整性。

#### Scenario: Broken level relation

- **WHEN** 关卡轮次引用不存在的关卡或目标引用不存在的元素
- **THEN** 配置加载失败并报告表名、行 ID、字段和原因
- **AND** 不允许进入该业务流程

### Requirement: Stable identifiers preserve same-version relations

配置迁移 SHALL 为事件、任务、材料、零件、形态和 Buff 保留同一版本内稳定 ID，以维持 DataTable 外键和新存档关系。本批次不要求历史存档迁移。

#### Scenario: New save references migrated content

- **WHEN** 新版本创建的存档包含已拥有零件或已解锁形态 ID
- **THEN** 迁移后的配置能解析这些 ID
- **AND** 存档状态可以在当前版本恢复
- **AND** 历史版本存档不属于本批次验收范围

### Requirement: Generated table pipeline is reproducible

DataTable 源 SHALL 可重复生成 `.txt` 和 C# 行类；生成结果经过重复刷新后 SHALL 无无关差异。

#### Scenario: Refresh generated tables twice

- **WHEN** 对同一批 Excel 源连续运行两次生成
- **THEN** 第二次生成不产生额外差异
- **AND** `AppConfigs` 注册与生成的表类一致
