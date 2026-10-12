"""Validate cross-table relations in generated GF DataTable text assets."""

from pathlib import Path
import sys


ROOT = Path(__file__).resolve().parents[1]
TABLE_ROOT = ROOT / "Assets" / "Game" / "DataTable"


def read_table(relative: str):
    path = TABLE_ROOT / relative
    lines = path.read_text(encoding="utf-8-sig").splitlines()
    headers = lines[1].split("\t")
    rows = []
    for line in lines[4:]:
        if not line.startswith("\t"):
            continue
        values = line.split("\t")
        row = {}
        for index, name in enumerate(headers):
            if name and index < len(values):
                row[name] = values[index]
        rows.append(row)
    return rows


def keys(rows, field):
    return {row[field] for row in rows if row.get(field)}


def require(condition, message, errors):
    if not condition:
        errors.append(message)


def main() -> int:
    errors = []
    trials = read_table("Meta/TrialTable.txt")
    forms = read_table("Board/FormTable.txt")
    levels = read_table("Board/LevelTable.txt")
    rounds = read_table("Board/LevelRoundTable.txt")
    goals = read_table("Board/LevelGoalTable.txt")
    keys_table = read_table("Board/LevelKeyPieceTable.txt")
    carries = read_table("Board/LevelCarryPartTable.txt")
    fixed = read_table("Board/LevelFixedElementTable.txt")
    tasks = read_table("Meta/TaskTable.txt")
    blueprints = read_table("Meta/TaskBlueprintTable.txt")
    events = read_table("Meta/EventTable.txt")
    case_events = read_table("Meta/CaseEventTable.txt")
    periods = read_table("Meta/EventOpenPeriodTable.txt")
    red_shoe = read_table("Board/RedShoeTable.txt")
    guides = read_table("Board/RedShoeGuideTable.txt")
    turns = read_table("Board/RedShoeTurnTable.txt")

    form_keys = keys(forms, "Key")
    form_blueprints = keys(forms, "BlueprintId")
    level_keys = keys(levels, "Key")
    event_keys = keys(events, "Key")
    case_event_keys = keys(case_events, "Key")
    task_keys = keys(tasks, "Key")
    trial_keys = keys(trials, "Key")

    require(len(trials) == 4, "TrialTable 应包含 4 条试机", errors)
    require(len(tasks) == 4, "TaskTable 应包含 4 条改装任务", errors)
    require(len(blueprints) == len(tasks), "TaskBlueprintTable 与任务数量不一致", errors)

    for row in trials:
        trial = row.get("Key", "")
        require(row.get("BorrowedFormKey") in form_keys, f"试机 {trial} 的借用形态不存在", errors)
        require(row.get("BlueprintKey") in form_blueprints, f"试机 {trial} 的图样不存在", errors)
        require(trial in level_keys, f"试机 {trial} 未绑定 LevelTable", errors)

    for row in blueprints:
        task = row.get("TaskKey", "")
        blueprint = row.get("BlueprintKey", "")
        require(task in task_keys, f"任务图样关系 {task} 引用了不存在的任务", errors)
        require(blueprint in trial_keys or blueprint in form_blueprints,
                f"任务 {task} 的图样 {blueprint} 不存在", errors)

    for row in rounds:
        require(row.get("LevelKey") in level_keys,
                f"轮次引用不存在的关卡 {row.get('LevelKey')}", errors)
    for row in goals:
        require(row.get("LevelKey") in level_keys,
                f"目标引用不存在的关卡 {row.get('LevelKey')}", errors)
    for name, rows in (("关键件", keys_table), ("携带池", carries), ("固定元素", fixed)):
        for row in rows:
            require(row.get("LevelKey") in level_keys,
                    f"{name}引用不存在的关卡 {row.get('LevelKey')}", errors)

    for row in levels:
        event = row.get("EventKey", "")
        require(not event or event in event_keys or event in case_event_keys,
                f"关卡 {row.get('Key')} 引用了不存在的事件 {event}", errors)
    for row in periods:
        require(row.get("EventKey") in event_keys,
                f"开放时段引用不存在的事件 {row.get('EventKey')}", errors)
    for name, rows in (("红舞鞋", red_shoe), ("导流格", guides), ("转向格", turns)):
        for row in rows:
            require(row.get("LevelKey") in level_keys,
                    f"{name}关系引用不存在的关卡 {row.get('LevelKey')}", errors)

    if errors:
        for error in errors:
            print("[FAIL] " + error)
        return 1

    print("[OK] migrated DataTable relations passed: trials=4 tasks=4 levels=%d events=%d" %
          (len(levels), len(events)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
