# project-datatable-pipeline

## ADDED Requirements

### Requirement: 业务表的源与生成物路径

业务表的 Excel 源 SHALL 位于 `GameData/DataTables/<领域>/<表名>.xlsx`，表数据生成物 SHALL 位于 `Assets/Game/DataTable/<领域>/<表名>.txt`，代码生成物 SHALL 位于 `Assets/Game/Scripts/DataTable/<领域>/<表名>.cs`。三处路径的相对部分 SHALL 一致。

领域目录 SHALL 取 `Board` 或 `Meta`，与 b01 建立的业务程序集分层保持一致。

#### Scenario: 新增表的三处路径一致

- **WHEN** 新增表 `Board/PartTable`
- **THEN** Excel 源位于 `GameData/DataTables/Board/PartTable.xlsx`
- **AND** 表数据生成物位于 `Assets/Game/DataTable/Board/PartTable.txt`
- **AND** 代码生成物位于 `Assets/Game/Scripts/DataTable/Board/PartTable.cs`

#### Scenario: 表名与生成类名一致

- **WHEN** 表文件名为 `PartTable`
- **THEN** 生成的代码类型名为 `PartTable`
- **AND** 该类型继承框架的 `DataRowBase`

### Requirement: Excel 行约定

表源的四个表头行 SHALL 遵循框架约定：第 1 行 A 列为 `#`、B 列为逻辑表名；第 2 行为字段名；第 3 行为字段类型；第 4 行为字段备注。第 5 行起为数据行，A 列为 `#` 表示该行被注释禁用。

第 4 行 SHALL 保持满宽：字段备注 SHALL 写在与其字段相同的列上，其余列为空但必须存在。第 4 行 SHALL NOT 出现占据字段备注列的表头文字。

#### Scenario: 备注行与字段一一对应

- **WHEN** 第 2 行的字段依次为 `ID`、`Name`、`Depth`
- **THEN** 第 4 行对应列依次给出这三个字段的备注
- **AND** 生成的代码中每个属性带有与其字段匹配的 XML 文档注释

#### Scenario: 备注行被裁剪即视为缺陷

- **WHEN** 第 4 行尾部的空单元格被裁掉
- **THEN** 该表 SHALL 被判为不符合本规格，因为生成器按原列号读取备注行，裁剪会让注释整体错位

### Requirement: 运行时注册契约

每张需要运行时加载的业务表 SHALL 在 `Assets/Game/ScriptableAssets/Core/AppConfigs.asset` 的 `mDataTables` 列表中登记，登记项为**不含扩展名的相对路径**。

#### Scenario: 登记项格式

- **WHEN** 表 `Board/PartTable` 需要运行时加载
- **THEN** `mDataTables` 中出现一项 `Board/PartTable`
- **AND** 该项不含 `.txt` 扩展名、不含 `Assets/` 前缀

#### Scenario: 已登记的表可在运行时读到数据

- **WHEN** 启动链进入业务场景并完成表加载
- **THEN** 已登记的表可通过框架数据表组件按表名读取
- **AND** 表中已配置的行数、字段值与 Excel 源一致

### Requirement: 管线可重跑且往返收敛

表管线 SHALL 可重复执行：以 Excel 源为输入重新生成后，已提交的 `.txt` 与 `.cs` 生成物 SHALL 与重新生成的结果一致。首次生成会修正历史生成物的漂移，此后每次重跑 SHALL 不再产生差异。

#### Scenario: 二次重跑无差异

- **WHEN** 连续两次以 Excel 源为输入执行生成
- **THEN** 第二次执行不产生任何文件差异
- **AND** 生成过程报成功且无编译错误

#### Scenario: Excel 源本身是确定性产物

- **WHEN** 连续多次从已提交的生成物重建 Excel 源
- **THEN** 每次产出的源文件内容一致

### Requirement: 业务表的字段骨架尺度

本批次新增的业务表 SHALL 只包含其任务完成定义所需的最小字段，字段类型 SHALL 限于 `int`、`string`、`bool`、`float`。字段的最终清单不在本批次承诺范围内。

#### Scenario: 骨架字段类型受限

- **WHEN** 检查本批次新增的业务表
- **THEN** 字段类型仅出现 `int`、`string`、`bool`、`float`
- **AND** 不出现枚举类型与自定义 JSON 类型

### Requirement: 业务侧数据表访问入口

业务代码 SHALL 经项目侧入口访问数据表，SHALL NOT 直接引用框架数据表组件的内部实现细节。

#### Scenario: 业务层不出现框架内部调用

- **WHEN** 扫描 `Assets/Game/Scripts/EverlightTales/` 下的代码
- **THEN** 数据表访问集中在项目侧入口
- **AND** 其余业务代码不直接持有框架数据表组件实例
