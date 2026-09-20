#!/usr/bin/env python3
"""Rebuild GameData DataTable Excel sources from the generated .txt DataTables.

The framework pipeline is:

    GameData/DataTables/<rel>.xlsx  --[GameDataGenerator]-->  Assets/Game/DataTable/<rel>.txt
                                    --[GameDataGenerator]-->  Assets/Game/Scripts/DataTable/<rel>.cs

Only the generated ``.txt`` and ``.cs`` files are committed for the Core tables, so the Excel
sources cannot be reproduced and the pipeline cannot be re-run. This tool reconstructs an Excel
source from a committed ``.txt`` using the exact same sheet layout the framework writes:

    A1 "#"   B1 "<logical table name>"
    A2 "#"   B2 "ID"   ""     D2 "<Field>" ...
    A3 "#"   B3 "int"  ""     D3 "<type>"  ...
    A4 "#"   B4 ""     C4 "<table comment>"   D4 "<field note>" ...
    A5 ""    B5.. "<row values>" ...     (row leads with "" meaning "enabled")

Layout facts that are easy to get wrong — all of them are asserted by the round-trip check, and
all were established by reading the generator plus an empirical calibration run:

* **Raw column C (index 2) is a spacer.** The framework template (``CreateDataTableExcel``) writes
  ``#`` / ``ID`` / ``int`` there and puts the human-facing ``备注`` header in C4. The generator
  never reads it for fields, and row 4's C cell is echoed verbatim into the .txt, so C4 is written
  as pure data (the table comment) with no template decoration.
* **The table comment lives in C4 (index 2), not B4.** B4 is empty. The class-level doc comment of
  the generated type comes from ``GetValue(0, 1)`` — cell B1 — not from row 4 at all.
* **Field notes start at D4 (index 3)** and are read by *raw column index*
  (``DataTableProcessor.GetComment(rawColumn)``). The note row must therefore stay **full width**:
  trimming its trailing cells shifts every field's doc comment onto the previous field.
* A row whose first cell is "#" is a commented-out (disabled) row, matching
  ``DataTableProcessor.CommentLineSeparator``.

Usage:
    python tools/datatable_excel_source.py --check
    python tools/datatable_excel_source.py --write
    python tools/datatable_excel_source.py --write --only Core/UIGroupTable
    python tools/datatable_excel_source.py --create            # 依据 SCHEMAS 新建业务表源
"""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
TXT_ROOT = REPO_ROOT / "Assets" / "Game" / "DataTable"
EXCEL_ROOT = REPO_ROOT / "GameData" / "DataTables"
CS_ROOT = REPO_ROOT / "Assets" / "Game" / "Scripts" / "DataTable"

COMMENT_MARKER = "#"
SHEET_NAME = "Sheet 1"

# Table-level header rows. Row 0 carries the table name in column B, row 4 carries the note row.
HEADER_ROWS = 4
TABLE_NAME_ROW = 0
TABLE_NAME_COLUMN = 1
TABLE_COMMENT_ROW = 3
# Row 4 of the sheet is the note row (DataTableGenerator passes commentRow: 3, zero based).
# Calibrated against the framework with every note cell replaced by a unique marker:
#
#     raw col 0 -> not read      raw col 1 -> the Id member
#     raw col 2 -> not read      raw col 3 -> first field after Id
#     raw col 4 -> second field  ...          1:1 from then on
#
# The table comment the generated class carries comes from ``GetValue(0, 1)`` (cell B1), not from
# the note row. The layout therefore mirrors the framework's own template
# (``GameDataGenerator.CreateDataTableExcel`` writes "#" in A4, 备注 in C4):
#   raw col 0  marker "#" (makes the note row a comment line, not a data row)
#   raw col 1  the Id note
#   raw col 2  table comment (human reference; the generator never reads this cell)
#   raw col 3+ per-field note, one per field, starting at the first field after Id
# This row must stay full width: the generator indexes it by raw column, so trailing cells have to
# exist (empty) rather than be trimmed.
NOTE_COLUMN = 1
TABLE_COMMENT_COLUMN = 2
FIELD_NOTE_COLUMN = 3

# ---------------------------------------------------------------------------
# 业务表骨架定义（b02 / P0-005）
#
# 每张业务表只落"最小可用字段"：ID + 名称 + 1~2 个已在设计文档中确认的字段。
# 字段的最终清单不由本文件决定，而由 P0-011（配置表字段冻结）登记草案、再由各表的归属任务
# （关卡表列的归属是 P1-021）冻结。因此这里的骨架刻意保持最小：它的目的是证明
# Excel → .txt → 生成 C# → AppConfigs 注册 → 运行时加载 这条链路可跑通，而不是锁定字段。
#
# 字段名首字母大写、无下划线（与框架模板提示"请添加字段, 字段名首字母大写"一致）。
# 类型限定 int / string / bool / float，不引入枚举与自定义 JSON 类型。
# ---------------------------------------------------------------------------

TABLE_SCHEMAS: list[dict] = [
    {
        "key": "Board/PartTable",
        "comment": "可装配零件的基础定义",
        "fields": [
            ("Name", "string", "零件显示名称"),
            ("PartKind", "int", "零件大类（占位，取值域待 P0-011 冻结）"),
            ("EnergyCost", "int", "驱动该零件消耗的能量"),
        ],
        # P0-005 的完成定义是"一张示例表走通生成与运行时加载"，因此这张表需要至少一行真实数据，
        # 才能验证"加载后能读到行"。其余表只需骨架，不填数据。
        "sample_rows": [
            ("1", "铜制齿轮", "1", "2"),
        ],
    },
    {
        "key": "Board/LevelTable",
        "comment": "维修关卡的骨架定义",
        "fields": [
            ("Name", "string", "关卡显示名称"),
            ("Radius", "int", "盘面半径档位 n，合法格数 3n(n-1)+1"),
            ("RoundCount", "int", "本关轮数"),
            ("TargetScore", "int", "本关目标分数"),
        ],
    },
    {
        "key": "Board/BuffTable",
        "comment": "局内 Buff 的骨架定义",
        "fields": [
            ("Name", "string", "Buff 显示名称"),
            ("StackRule", "int", "叠加规则（占位，取值域待 P0-011 冻结）"),
            ("Duration", "int", "持续轮数，0 表示本关常驻"),
        ],
    },
    {
        "key": "Meta/EventTable",
        "comment": "事件模板的骨架定义",
        "fields": [
            ("Name", "string", "事件显示名称"),
            ("EventType", "int", "事件类型（地图交互／调查／普通维修／怪谈阶段等）"),
            ("TimeCost", "int", "完成后消耗的时间格"),
        ],
    },
    {
        "key": "Meta/MaterialTable",
        "comment": "材料的基础定义",
        "fields": [
            ("Name", "string", "材料显示名称"),
            ("MaterialKind", "int", "材料大类（占位，取值域待 P0-011 冻结）"),
            ("StackLimit", "int", "单格堆叠上限，0 表示不限"),
        ],
    },
    {
        "key": "Meta/QuestTable",
        "comment": "任务的骨架定义",
        "fields": [
            ("Name", "string", "任务显示名称"),
            ("QuestKind", "int", "任务类型（主线／支线／日常等）"),
            ("RequireCount", "int", "达成条件所需的事件完成数量"),
        ],
    },
]

# Table comments are pinned here rather than read back from the .txt. The .txt carries whatever
# the note row held when it was generated, so reading it back would make the Excel source depend
# on the previous Excel state: every regeneration would move the comment one cell further right.
# These are the committed table comments, transcribed once.
TABLE_COMMENTS: dict[str, str] = {
    "Core/EntityGroupTable": "Framework default entity groups",
    "Core/LanguagesTable": "Framework fallback language",
    "Core/SoundGroupTable": "Framework default sound groups",
    "Core/UIGroupTable": "Framework default UI groups",
    "Core/UITable": "Project UI entries are intentionally empty in the framework baseline.",
}

try:
    from openpyxl import Workbook, load_workbook
    from openpyxl.styles import Font
except ImportError:  # pragma: no cover - tooling dependency
    print("openpyxl is required: python -m pip install openpyxl", file=sys.stderr)
    raise SystemExit(3)


def parse_txt(path: Path) -> list[list[str]]:
    """Split a DataTable .txt into rows of cells, dropping trailing empty cells."""
    rows: list[list[str]] = []
    for raw in path.read_text(encoding="utf-8").splitlines():
        if raw == "":
            continue
        cells = raw.split("\t")
        while cells and cells[-1] == "":
            cells.pop()
        rows.append(cells)
    return rows


# Row 4 of every table is the note row. The canonical column layout is:
#   index 0 marker "#"  |  index 1 empty ("ID" comment slot)  |  index 2 "备注"
#   index 3 table comment  |  index 4+ per-field note (one per field)
# The framework echoes these cells verbatim into the generated .cs as XML doc comments, so the
# note row is what makes the generated code readable. Tables reconstructed from a .txt that
# predates this convention will have the note row shifted; FIELD_NOTES below restores it.
NOTE_HEADER = "备注"
# key = "<relative path without extension>", value = index 4+ field notes in field order.
FIELD_NOTES: dict[str, list[str]] = {
    "Core/EntityGroupTable": [
        "分组名称",
        "自动释放间隔（秒），0 表示不自动释放",
        "对象池容量，0 表示不限制",
        "对象过期时间（秒），0 表示不过期",
        "优先级，数值越大越晚被释放",
    ],
    "Core/LanguagesTable": [
        "语言键，与运行时语言标识一致",
        "语言资源名，用于加载语言表",
        "界面显示名称",
        "语言图标资源名",
    ],
    "Core/SoundGroupTable": [
        "声音分组名称",
        "声音代理数量，决定同组可同时播放的数量",
        "是否避免被同优先级的音频替换",
        "是否静音",
        "音量，1 为原始音量",
    ],
    "Core/UIGroupTable": [
        "界面分组名称",
        "分组深度，数值越大越靠前显示",
    ],
    "Core/UITable": [
        "同分组内的显示排序",
        "界面预制体名称",
        "是否暂停被覆盖的界面",
        "所属界面分组 Id",
        "是否允许返回键关闭",
    ],
}


def apply_canonical_notes(
    rows: list[list[str]], key: str, span: int
) -> list[list[str]]:
    """Return rows whose note row follows the canonical column layout.

    The generated code reads the note row by raw column index (``GetComment(i)``), so this row
    must stay **full width**: a note sits in the cell whose raw column matches its field, and
    every other cell is present but empty. Trimming trailing empties would shift the row and
    silently mis-document the generated properties, so the row is padded to the table's width.

    Every value written here comes from the pinned NOTE/COMMENT/FIELD tables, never from the
    incoming row, so the result depends only on the table key and its width.
    """
    notes = FIELD_NOTES.get(key)
    if notes is None:
        return rows

    normalized = [list(row) for row in rows]
    required = max(span, FIELD_NOTE_COLUMN + len(notes))
    while len(normalized[TABLE_COMMENT_ROW]) < required:
        normalized[TABLE_COMMENT_ROW].append("")

    note_row = normalized[TABLE_COMMENT_ROW]
    table_comment = TABLE_COMMENTS.get(key, "")
    id_note = note_row[NOTE_COLUMN] if len(note_row) > NOTE_COLUMN else ""

    for index in range(len(note_row)):
        note_row[index] = ""
    while len(note_row) < required:
        note_row.append("")
    # The note row is a comment line: the marker in column A keeps the parser from reading it as
    # a data row, matching the framework template and every other Core table.
    note_row[0] = COMMENT_MARKER
    note_row[NOTE_COLUMN] = id_note
    note_row[TABLE_COMMENT_COLUMN] = table_comment
    for offset, note in enumerate(notes):
        note_row[FIELD_NOTE_COLUMN + offset] = note
    return normalized


def column_span(rows: list[list[str]]) -> int:
    return max((len(row) for row in rows), default=0)


def to_cell(value: str, type_name: str):
    """Convert a .txt cell to the value EPPlus would have written to the sheet.

    Only types whose string form round-trips differently need special handling; the caller
    verifies the result by regenerating the .txt and diffing it against the committed file.
    """
    if value == "":
        return None
    if type_name in ("int", "long"):
        try:
            return int(value)
        except ValueError:
            return value
    if type_name in ("float", "double", "decimal"):
        try:
            return float(value)
        except ValueError:
            return value
    if type_name == "bool":
        lowered = value.lower()
        if lowered in ("true", "false"):
            return lowered == "true"
        return value
    return value


def build_workbook(rows: list[list[str]], table_name: str) -> Workbook:
    span = column_span(rows)
    workbook = Workbook()
    sheet = workbook.active
    sheet.title = SHEET_NAME

    # Row 3 holds the type names; they drive cell typing for the data rows.
    type_row = rows[2] if len(rows) > 2 else []
    types = {index: value for index, value in enumerate(type_row)}

    for row_index in range(HEADER_ROWS):
        source = rows[row_index] if row_index < len(rows) else []
        for col_index in range(span):
            value = source[col_index] if col_index < len(source) else ""
            if row_index == TABLE_NAME_ROW and col_index == TABLE_NAME_COLUMN and value == "":
                value = table_name
            sheet.cell(row=row_index + 1, column=col_index + 1, value=value or None)

    # Bold the marker column so the reconstructed sheet keeps the framework template's
    # visual convention. No other decoration is added: any cell this tool writes is echoed
    # back into the generated .txt by the framework.
    for row_index in range(1, HEADER_ROWS + 1):
        sheet.cell(row=row_index, column=1).font = Font(bold=True)

    for row_index in range(HEADER_ROWS, len(rows)):
        source = rows[row_index]
        for col_index in range(span):
            raw = source[col_index] if col_index < len(source) else ""
            value = to_cell(raw, types.get(col_index, ""))
            sheet.cell(row=row_index + 1, column=col_index + 1, value=value)

    return workbook


def schema_rows(schema: dict) -> list[list[str]]:
    """Build the four header rows for a business table from its schema definition.

    Layout (width = ``FIELD_NOTE_COLUMN + len(fields)``)::

        col 0      marker "#"
        col 1      Id (field name / type; its note slot is left empty)
        col 2      spacer; row 4 carries the table comment here
        col 3..    the declared fields, one per column

    Field notes land 1:1 at ``FIELD_NOTE_COLUMN + index`` — exactly what the generator reads by
    raw column index.
    """
    fields = schema["fields"]
    width = FIELD_NOTE_COLUMN + len(fields)

    marker = [""] * width
    marker[0] = COMMENT_MARKER
    marker[TABLE_NAME_COLUMN] = schema["key"].split("/")[-1]

    name_row = [""] * width
    name_row[0] = COMMENT_MARKER
    name_row[1] = "ID"
    type_row = [""] * width
    type_row[0] = COMMENT_MARKER
    type_row[1] = "int"

    note_row = [""] * width
    note_row[0] = COMMENT_MARKER
    note_row[TABLE_COMMENT_COLUMN] = schema["comment"]

    for index, (field_name, field_type, field_note) in enumerate(fields):
        column = FIELD_NOTE_COLUMN + index
        name_row[column] = field_name
        type_row[column] = field_type
        note_row[column] = field_note

    # 数据行：首列为空表示"启用"（框架以 "#" 标记被注释禁用的行）。
    data_rows: list[list[str]] = []
    for sample in schema.get("sample_rows", []):
        row = [""] * width
        row[1] = str(sample[0])  # Id
        for index, value in enumerate(sample[1:]):
            row[FIELD_NOTE_COLUMN + index] = str(value)
        data_rows.append(row)

    return [marker, name_row, type_row, note_row] + data_rows


def create_tables(only: str | None = None, overwrite: bool = False) -> list[str]:
    """Create the business table Excel sources declared in ``TABLE_SCHEMAS``.

    ``overwrite=False`` (the default) never clobbers an existing source: this is a scaffolding
    step, and destroying a source a designer has since edited would be silent data loss.
    ``overwrite=True`` is used by ``--seed`` to (re)write the declared sample rows.
    """
    written: list[str] = []
    for schema in TABLE_SCHEMAS:
        key = schema["key"]
        if only and key != only:
            continue
        target = EXCEL_ROOT / f"{key}.xlsx"
        if target.exists() and not overwrite:
            continue
        target.parent.mkdir(parents=True, exist_ok=True)
        rows = schema_rows(schema)
        build_workbook(rows, schema["key"].split("/")[-1]).save(target)
        written.append(key)
    return written


def iter_tables(only: str | None):
    for txt in sorted(TXT_ROOT.rglob("*.txt")):
        relative = txt.relative_to(TXT_ROOT).with_suffix("")
        key = relative.as_posix()
        if only and key != only:
            continue
        yield key, txt, EXCEL_ROOT / relative.with_suffix(".xlsx"), CS_ROOT / relative.with_suffix(".cs")


def inspect_workbook(path: Path, table_name: str) -> tuple[bool, str]:
    """Return (readable, detail) for an Excel source, checking structure only.

    ``table_name`` is the logical name carried by the .txt (cell B1 of row 1), which is not
    always the file name - e.g. ``SoundGroupTable.xlsx`` declares ``SoundGroup``.

    Cell-by-cell comparison is deliberately not attempted here: openpyxl in ``data_only`` mode
    reports absent cells differently from EPPlus (``False`` for bool columns where the framework
    sees empty), so a Python-level diff produces false alarms. Content correctness is proven by
    the framework round trip instead:

        python tools/datatable_excel_source.py --write
        Unity.exe -batchmode -nographics -projectPath <project> \\
                  -executeMethod DSHDataTableRoundTrip.Run -logFile <log> -dshResult <json>
        git diff -- Assets/Game/DataTable Assets/Game/Scripts/DataTable   # must be empty

    That check is what the Core tables were verified with; this function only catches a source
    that is missing, unreadable, or has lost its header rows.
    """
    if not path.exists():
        return False, "missing"
    try:
        book = load_workbook(path, data_only=True)
    except Exception as error:  # pragma: no cover - corrupt workbook
        return False, f"unreadable: {error}"
    if not book.sheetnames:
        return False, "no worksheet"
    sheet = book[book.sheetnames[0]]
    if sheet.max_row < HEADER_ROWS or sheet.max_column < 2:
        return False, f"header rows/columns missing (max_row={sheet.max_row}, max_col={sheet.max_column})"
    declared = str(sheet.cell(row=1, column=2).value or "")
    if declared != table_name:
        return False, f"cell B1 declares '{declared}', expected '{table_name}'"
    return True, f"{sheet.max_row} rows x {sheet.max_column} cols"


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--write", action="store_true", help="create or refresh the Excel sources")
    parser.add_argument("--check", action="store_true", help="report whether the Excel sources exist and match")
    parser.add_argument("--only", default=None, help="limit to one table, e.g. Core/UIGroupTable")
    parser.add_argument(
        "--create",
        action="store_true",
        help="create the business table sources declared in TABLE_SCHEMAS (existing files are kept)",
    )
    parser.add_argument(
        "--seed",
        action="store_true",
        help="(re)write the business table sources declared in TABLE_SCHEMAS, including sample_rows",
    )
    parser.add_argument(
        "--calibrate",
        action="store_true",
        help="write note rows whose every cell is a unique marker cell0..cellN, so the framework's "
        "generated .cs doc comments reveal the true note-row -> property mapping",
    )
    args = parser.parse_args()

    if args.create or args.seed:
        created = create_tables(args.only, overwrite=args.seed)
        for key in created:
            print(f"[WROTE]   {key} -> {(EXCEL_ROOT / (key + '.xlsx')).relative_to(REPO_ROOT).as_posix()}")
        if not args.seed:
            kept = [s["key"] for s in TABLE_SCHEMAS if (args.only is None or s["key"] == args.only)]
            for key in [k for k in kept if k not in created]:
                print(f"[KEPT]    {key} (already exists)")
        print(f"[OK] business table sources written: {len(created)}")
        return 0

    if args.calibrate:
        for key, txt, excel, cs in iter_tables(args.only):
            rows = parse_txt(txt)
            if len(rows) < HEADER_ROWS:
                continue
            span = column_span(rows)
            normalized = [list(row) for row in rows]
            while len(normalized[TABLE_COMMENT_ROW]) < span:
                normalized[TABLE_COMMENT_ROW].append("")
            for index in range(len(normalized[TABLE_COMMENT_ROW])):
                normalized[TABLE_COMMENT_ROW][index] = f"cell{index}"
            excel.parent.mkdir(parents=True, exist_ok=True)
            build_workbook(normalized, excel.stem).save(excel)
            print(f"[CALIBRATE] {key} note row = " + ",".join(f"cell{i}" for i in range(span)))
        return 0

    if not args.write and not args.check:
        args.check = True

    missing: list[str] = []
    written: list[str] = []
    matched: list[str] = []

    for key, txt, excel, cs in iter_tables(args.only):
        rows = parse_txt(txt)
        if len(rows) < HEADER_ROWS:
            print(f"[SKIP] {key}: fewer than {HEADER_ROWS} header rows")
            continue
        # The .txt row 1 carries the logical table name, which is not always the file name.
        declared_name = (
            rows[TABLE_NAME_ROW][TABLE_NAME_COLUMN]
            if len(rows[TABLE_NAME_ROW]) > TABLE_NAME_COLUMN
            else excel.stem
        )

        if args.check:
            readable, detail = inspect_workbook(excel, declared_name)
            if readable:
                matched.append(key)
                print(f"[OK]      {key} ({detail})")
            else:
                missing.append(key)
                print(f"[BROKEN]  {key}: {detail}")

        if args.write:
            span = column_span(rows)
            corrected = apply_canonical_notes(rows, key, span)
            workbook = build_workbook(corrected, declared_name)
            excel.parent.mkdir(parents=True, exist_ok=True)
            workbook.save(excel)
            written.append(key)
            note_count = len(FIELD_NOTES.get(key, []))
            print(
                f"[WROTE]   {key} -> {excel.relative_to(REPO_ROOT).as_posix()}"
                f" (canonical field notes: {note_count})"
            )

    if args.check and not args.write:
        if missing:
            print(f"[FAIL] Excel sources missing or broken: {len(missing)}")
            return 1
        print(f"[OK] Excel sources present and structurally valid: {len(matched)}")
        print("     content correctness is proven by the framework round trip, not this check")

    if args.write:
        print(f"[OK] Excel sources written: {len(written)}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
