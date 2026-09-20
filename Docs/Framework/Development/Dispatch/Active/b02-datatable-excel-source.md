# b02 DataTable 管线可重跑化：Excel 源重建

## 背景

b02（P0-005 DataTable 管线）在方案确认阶段发现一个阻塞性事实：

**`Assets/Game/DataTable/Core/*.txt` 只有生成物被提交，Excel 源不在工程内。**

`GameData/DataTables/` 目录存在但为空，而框架管线是

```
GameData/DataTables/<rel>.xlsx --[GameDataGenerator]--> Assets/Game/DataTable/<rel>.txt
                               --[GameDataGenerator]--> Assets/Game/Scripts/DataTable/<rel>.cs
```

也就是说 `.txt`/`.cs` 无法重新生成，管线不可重跑，任何字段调整都只能手改生成物。这一条挡住
了 P0-005 的"DoD：一张示例表走通生成与运行时加载"，所以作为 b02 的前置工作先行解决。

## 交付

- [tools/datatable_excel_source.py](../../../../tools/datatable_excel_source.py)：从已提交的
  `.txt` 重建 Excel 源，`--check` / `--write` / `--calibrate` 三个模式。
- `GameData/DataTables/Core/*.xlsx`：5 张 Core 表的 Excel 源（EntityGroup / Languages /
  SoundGroup / UIGroup / UITable）。

## 表头布局（关键，靠实测标定而非猜测）

Excel 的行约定：

| 行 | 内容 |
|---|---|
| 1 | A=`#`，B=逻辑表名（如 `SoundGroup`，**不等于**文件名 `SoundGroupTable`） |
| 2 | A=`#`，B 起为字段名；ID 固定在第 2 列，第 3 列是分隔列 |
| 3 | A=`#`，B 起为字段类型 |
| 4 | A=`#`，B=ID 注释，C=表注释，D 起为逐字段注释 |
| 5+ | 数据行；A 列为 `#` 表示该行被注释禁用 |

生成器侧的证据链（不是猜的）：

1. `DataTableGenerator.DataTableCodeGenerator` 调用处理器时传 `commentRow: 3`（0 基），
   即 Excel 第 4 行是注释行。
2. `DataTableProcessor.GetComment(rawColumn)` 按**原列号**取注释行，`DataTableGenerator.
   GenerateDataTableProperties` 对每个非注释、非 ID 列一一取用 —— 所以注释行必须是
   **满宽**的，尾部空单元格不能被裁掉，否则整体左移、每个字段的文档注释都会落到别的成员上。
3. 表级注释来自 `GetValue(0, 1)`（B1），**不是**注释行；`CreateDataTableExcel` 模板里
   C4 的 `备注` 只是给人看的表头。

标定手段：`--calibrate` 把注释行每个单元格替换成唯一标记 `cell0..cellN`，跑一次生成后读
`.cs` 的 XML 注释，得到真实映射：

```
raw col 0 -> 不读       raw col 1 -> Id 成员
raw col 2 -> 不读       raw col 3 -> ID 之后的第一个字段
raw col 4 -> 第二个字段  ...            此后 1:1
```

## 发现并修复的两个真实缺陷

1. **注释行整体错位一格。** 提交的 `.txt` 里表注释落在 raw col 3（即首个字段的注释位），
   导致每个字段的注释都对到了**前一个**字段上；`UITable` 因为没有数据行，注释行更短，
   `备注` 与表注释把 5 个字段的文档全部吃掉。修复后 5 张表的字段注释各归其位。
2. **Excel 源不稳定。** 一开始从 `.txt` 回读表注释，而 `.txt` 记录的正是上一次写入的
   结果，于是每跑一次 `--write` 注释就右移一格。改为把表注释与字段注释固定在工具的
   `TABLE_COMMENTS` / `FIELD_NOTES` 常量里，现在连续写三次输出完全一致。

## 验证证据

| 项 | 命令 | 结果 |
|---|---|---|
| 框架往返 | `-executeMethod DSHDataTableRoundTrip.Run` 跑两轮 | 第 1 轮修正注释行；第 2 轮 `changedCount: 0`，即收敛 |
| 生成物编译 | 同上（Unity 编译失败则 `executeMethod` 不会执行） | 日志无 `error CS`，5 张表全部加载并解析 |
| 源稳定性 | 连续 `--write` 三次比对哈希 | 三次输出完全一致 |
| 结构化检查 | `python tools/datatable_excel_source.py --check` | 5/5 通过 |
| 框架纯度 | `python tools/audit_framework_purity.py` | 通过 |
| 程序集布局 | `python tools/verify_project_assemblies.py` | 通过 |
| meta 完整性 | `python tools/unity_meta.py --check` | 0 缺失 |

## 遗留与边界

- 本工具覆盖 **Core** 5 张表。后续 b02 新增的 7 张业务表应直接以 Excel 为源（Excel 是权威），
  再用框架导出 `.txt`/`.cs`，不需要走"从 `.txt` 反向重建"这条路径 —— 该路径只是把历史
  生成物救回可维护状态的过渡手段。
- `--check` 只做存在性与结构校验。逐单元格比对不可靠：openpyxl 的 `data_only` 模式对
  未设置的 bool 列返回 `False`，而 EPPlus 视为空，两者在"空值"上语义不同，硬比会产生
  假告警。内容正确性以框架往返 + `git diff` 为空为准。

## 配套：表头结构静态校验

[check_datatable_schema.py](../../../../tools/check_datatable_schema.py) 把上述表头约定变成秒级可执行检查
（`--path` 可单表检查）：

```powershell
python tools/check_datatable_schema.py
```

它守住的正是本文档记录的真实缺陷：**备注行被裁短**会让生成器按原列号读取时整体错位，
每个字段的文档注释落到前一个字段上。另检查分隔列（raw 列 2）未被字段名占用、字段类型在
已知集合内、每个字段有备注、数据行以 `#` 标记禁用。

工具自身用 6 个构造样本做过自测（1 个干净表 + 5 种缺陷），结论与预期一一对应：

| 样本 | 预期 | 命中的检查 |
|---|---|---|
| 干净表 | 通过 | —— |
| 备注行被裁短 | 失败 | 备注行被裁短、备注缺失 |
| 分隔列被占用 | 失败 | 分隔列被占用、备注列错位 |
| 字段缺类型 | 失败 | 类型行被裁短、类型缺失 |
| 字段缺备注 | 失败 | 备注缺失 |
| 表名缺失 | 失败 | 表名缺失 |

**两条易误判的约定（已核实，不要"修"）**：

1. **B1 不必等于文件名。** 类名恒等于文件名（`EntityGroupTable.txt` → `class EntityGroupTable`），
   而 B1 是自由文本、只作为生成的类级注释，实际取值为 `EntityGroup`、`SoundGroup`、`UI table`
   这类展示名。工具因此只检查 B1 非空。
2. **备注行的 raw 列 0 与列 2 允许非空。** 列 0 是注释标记，列 2 是表注释，生成器从不读取
   这两列（`GetComment` 只对字段列调用）。真正必须对齐的是字段列（raw 列 3 起）。

