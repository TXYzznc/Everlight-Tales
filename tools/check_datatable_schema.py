#!/usr/bin/env python3
"""DataTable .txt 表头结构校验（秒级，不需要 Unity）。

背景：这套表头约定是靠实测标定的，且**任何一处偏差都不会报错**，只会静默改变生成结果：

- `DataTableProcessor.GetComment(rawColumn)` 按**原列号**读取备注行，因此备注行必须满宽。
  尾部空单元格被裁掉会让整行左移，每个字段的文档注释落到前一个字段上。
- 生成器为每个非注释、非 ID 列产出一个属性，读取的列序是 raw 3、4、5…，
  因此 raw 列 2 是**分隔列**，应当在字段名行与类型行为空。
- 数据行以首列 `#` 标记禁用，注释行同样以 `#` 开头。

这些约定此前只在文档里、靠人眼守住；本工具把它变成可执行检查。

用法：

    python tools/check_datatable_schema.py                # 检查全部表
    python tools/check_datatable_schema.py --path Assets/Game/DataTable/Core/UITable.txt
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
TXT_ROOT = ROOT / "Assets" / "Game" / "DataTable"

COMMENT_MARKER = "#"
HEADER_ROWS = 4
TABLE_NAME_ROW = 0
TABLE_NAME_COLUMN = 1
NAME_ROW = 1
TYPE_ROW = 2
NOTE_ROW = 3
SEPARATOR_COLUMN = 2
FIRST_FIELD_COLUMN = 3
ID_COLUMN = 1

KNOWN_TYPES = {
    "int", "long", "float", "double", "bool", "string", "datetime",
    "int[]", "long[]", "float[]", "double[]", "bool[]", "string[]",
    "vector2", "vector3", "vector4", "color", "quaternion",
}


class Problem:
    def __init__(self, key: str, kind: str, detail: str) -> None:
        self.key = key
        self.kind = kind
        self.detail = detail

    def __str__(self) -> str:
        return f"[{self.kind}] {self.key}: {self.detail}"


def read_rows(path: Path) -> list[list[str]]:
    text = path.read_text(encoding="utf-8")
    return [line.split("\t") for line in text.replace("\r\n", "\n").split("\n") if line != ""]


def cell(row: list[str], index: int) -> str:
    return row[index] if index < len(row) else ""


def table_key(path: Path) -> str:
    """可读的表标识；树外文件（自测用）退化为文件名。"""
    try:
        return path.relative_to(TXT_ROOT).with_suffix("").as_posix()
    except ValueError:
        return path.name


def check_table(path: Path) -> list[Problem]:
    key = table_key(path)
    problems: list[Problem] = []

    rows = read_rows(path)
    if len(rows) < HEADER_ROWS:
        return [Problem(key, "缺失", f"只有 {len(rows)} 行，至少需要 {HEADER_ROWS} 行表头")]

    width = max(len(row) for row in rows)
    name_row, type_row, note_row = rows[NAME_ROW], rows[TYPE_ROW], rows[NOTE_ROW]

    declared_name = cell(rows[TABLE_NAME_ROW], TABLE_NAME_COLUMN)
    if not declared_name:
        problems.append(
            Problem(
                key,
                "表名缺失",
                "第 1 行 B 列为空，生成的类注释会为空",
            )
        )
    # 注意：B1 是自由文本，**只**用于生成的类级注释，不决定类名。
    # 已核实：类名恒等于文件名（EntityGroupTable.txt -> class EntityGroupTable），
    # 而 B1 可以是 'EntityGroup'、'SoundGroup'、'UI table' 这类展示名。
    # 因此这里只检查非空，不要求与文件名一致。

    if cell(rows[TABLE_NAME_ROW], 0) != COMMENT_MARKER:
        problems.append(Problem(key, "注释标记", f"第 1 行 A 列应为 '{COMMENT_MARKER}'"))

    if cell(name_row, ID_COLUMN).strip() == "":
        problems.append(Problem(key, "Id 字段缺失", f"第 2 行第 {ID_COLUMN + 1} 列（Id）为空"))

    # 分隔列应当在字段名行与类型行为空：生成器读取字段的列序从 raw 3 起。
    for row_index, row in ((NAME_ROW, name_row), (TYPE_ROW, type_row)):
        value = cell(row, SEPARATOR_COLUMN)
        if value.strip() != "":
            problems.append(
                Problem(
                    key,
                    "分隔列被占用",
                    f"第 {row_index + 1} 行第 {SEPARATOR_COLUMN + 1} 列为 '{value}'；"
                    "该列是分隔列，占用它会把后续字段整体右移一位",
                )
            )

    # 逐字段：字段名、类型、备注三者按同一列对齐。
    field_columns = [
        index for index in range(FIRST_FIELD_COLUMN, width) if cell(name_row, index).strip() != ""
    ]
    if not field_columns:
        problems.append(Problem(key, "无字段", f"从第 {FIRST_FIELD_COLUMN + 1} 列起没有任何字段名"))

    # 备注行必须覆盖到最后一个字段列。**这是本工具存在的核心理由**：
    # 生成器按原列号读备注行，备注行被裁短会让每个字段的注释左移错位。
    # 反过来，字段名行与类型行的尾部空单元格被裁掉是无害的（框架把缺失单元格当空处理），
    # 因此这里只比较"是否覆盖到字段列"，不比较各行宽度是否相等。
    if field_columns:
        last_field = max(field_columns)
        if len(note_row) <= last_field:
            problems.append(
                Problem(
                    key,
                    "备注行被裁短",
                    f"第 4 行只有 {len(note_row)} 个单元格，但最后一个字段在第 {last_field + 1} 列；"
                    "生成器按原列号读取备注行，裁短会让字段注释整体左移错位",
                )
            )
        if len(type_row) <= last_field:
            problems.append(
                Problem(
                    key,
                    "类型行被裁短",
                    f"第 3 行只有 {len(type_row)} 个单元格，但最后一个字段在第 {last_field + 1} 列",
                )
            )

    for index in field_columns:
        field_name = cell(name_row, index)
        field_type = cell(type_row, index).strip()
        if field_type == "":
            problems.append(
                Problem(key, "类型缺失", f"字段 '{field_name}'（第 {index + 1} 列）在第 3 行没有类型")
            )
        elif field_type.lower() not in KNOWN_TYPES:
            problems.append(
                Problem(
                    key,
                    "未知类型",
                    f"字段 '{field_name}' 的类型 '{field_type}' 不在已知类型集合中；"
                    "若是新增类型，请同时更新本工具的 KNOWN_TYPES",
                )
            )
        if cell(note_row, index).strip() == "":
            problems.append(
                Problem(
                    key,
                    "备注缺失",
                    f"字段 '{field_name}'（第 {index + 1} 列）没有备注，生成的属性会缺少文档注释",
                )
            )

    # 备注行不得出现落在任何字段列之外的非空单元格（会污染下一个字段的注释）。
    # 例外：raw 列 0（注释标记）与 raw 列 2（表注释）不是字段列，生成器从不读取它们，
    # 因此允许非空。raw 列 1 是 Id 的备注位，已核实生成器从不读取（Id 属性没有文档注释）。
    never_read = {0, SEPARATOR_COLUMN}
    for index in range(width):
        if cell(note_row, index).strip() == "":
            continue
        if index in never_read or index == ID_COLUMN or index in field_columns:
            continue
        problems.append(
            Problem(
                key,
                "备注列错位",
                f"第 4 行第 {index + 1} 列有备注 '{cell(note_row, index)}'，但该列没有对应字段；"
                "它会被读成下一个字段的注释",
            )
        )

    # 数据行：首列必须是注释标记或空，且列数不得超过表宽。
    for row_index in range(HEADER_ROWS, len(rows)):
        marker = cell(rows[row_index], 0)
        if marker not in (COMMENT_MARKER, ""):
            problems.append(
                Problem(
                    key,
                    "数据行标记",
                    f"第 {row_index + 1} 行 A 列为 '{marker}'；数据行应以 '{COMMENT_MARKER}' 禁用或留空",
                )
            )
        if len(rows[row_index]) > width:
            problems.append(
                Problem(key, "数据行过宽", f"第 {row_index + 1} 行有 {len(rows[row_index])} 个单元格，超过表宽 {width}")
            )

    return problems


def main() -> int:
    parser = argparse.ArgumentParser(description="DataTable .txt 表头结构校验")
    parser.add_argument("--path", action="append", default=None, help="只检查指定文件，可重复")
    args = parser.parse_args()

    if args.path:
        targets = [Path(p) if Path(p).is_absolute() else ROOT / p for p in args.path]
    else:
        targets = sorted(TXT_ROOT.rglob("*.txt"))

    if not targets:
        print(f"[FAIL] 未找到任何表：{TXT_ROOT}")
        return 1

    problems: list[Problem] = []
    for path in targets:
        if not path.exists():
            problems.append(Problem(str(path), "缺失", "文件不存在"))
            continue
        problems.extend(check_table(path))

    for problem in problems:
        print(problem)

    print()
    if problems:
        print(f"[FAIL] 表头结构校验未通过：{len(problems)} 个问题，涉及 {len(targets)} 个表")
        return 1

    print(f"[OK] 表头结构校验通过：{len(targets)} 个表，备注行满宽、字段与备注逐列对齐")
    return 0


if __name__ == "__main__":
    sys.exit(main())
