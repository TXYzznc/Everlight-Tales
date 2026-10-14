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

# 逐项参数列（ExtraData）的统一备注。键值编码 `k=v;k=v`，空表示用代码默认值；
# 解析与严格校验落在 Data 层 ExtraParamSet，不在本工具里解释内容。
EXTRA_DATA_NOTE = "逐项参数，k=v;k=v 分号分隔；空表示用代码默认值"

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
        "comment": "可装配零件的完整静态定义",
        "fields": [
            ("Key", "string", "稳定键（P-001~P-020）"),
            ("Name", "string", "零件显示名称"),
            ("PartKind", "int", "PartType 数值"),
            ("TriggerScore", "int", "每次成立触发的触发分"),
            ("EnergyCapacity", "int", "零件能量容量"),
            ("EffectCost", "int", "单次自身效果消耗的能量"),
            ("EffectScorePerCell", "int", "每实际推动一格的效果分"),
            ("EffectScorePerTarget", "int", "每个受冲击对象的效果分"),
            ("PublicEnergyPerEffect", "int", "每次效果产出的公共维修能量"),
            ("BonusScoreFromNth", "int", "达到次数门槛后的额外效果分"),
            ("BonusNth", "int", "额外效果分的次数门槛"),
            ("RemovesSelf", "bool", "起爆后是否移除本体"),
            ("EffectScorePerEnergy", "int", "移除前剩余能量每点的额外效果分"),
            ("PushDistance", "int", "单次能力直接推进的格数上限"),
            ("RayRange", "int", "射线搜索或延伸距离"),
            ("ChargeThreshold", "int", "蓄量释放阈值"),
            ("SecondaryEffectScore", "int", "次生效果分"),
            ("Stage", "string", "解锁阶段"),
            ("SourceHint", "string", "来源提示"),
            ("UseHint", "string", "一句话功能提示"),
        ],
    },
    {
        "key": "Board/LevelTable",
        "comment": "维修关卡的基础定义",
        "fields": [
            ("Key", "string", "关卡稳定键"),
            ("Name", "string", "关卡显示名称"),
            ("Radius", "int", "盘面半径档位 n，合法格数 3n(n-1)+1"),
            ("InitialPartCount", "int", "初盘零件总数"),
            ("InitialArmMoves", "int", "初始机械臂次数"),
            ("InitialPublicRepairEnergy", "int", "初始公共维修能量"),
            ("Objective", "string", "关卡目标文案"),
            ("Duration", "int", "预计耗时格数"),
            ("EventKey", "string", "绑定事件稳定键，空表示教学或独立关卡"),
        ],
    },
    {
        "key": "Board/LevelRoundTable",
        "comment": "关卡轮次关系",
        "fields": [
            ("LevelKey", "string", "关卡稳定键"),
            ("RoundIndex", "int", "轮次序号，从 0 开始"),
            ("TapCount", "int", "本轮拍数"),
            ("TargetScore", "int", "本轮目标分"),
        ],
    },
    {
        "key": "Board/LevelGoalTable",
        "comment": "关卡轮次特殊目标关系",
        "fields": [
            ("LevelKey", "string", "关卡稳定键"),
            ("RoundIndex", "int", "轮次序号，从 0 开始"),
            ("GoalKey", "string", "目标稳定键"),
            ("GoalKind", "int", "GoalKind 数值"),
            ("EntityId", "int", "被追踪实体 ID，0 表示任意实体"),
            ("AnchorLabel", "int", "目标锚点标签"),
            ("Required", "int", "所需进度量"),
            ("Text", "string", "目标显示文案"),
        ],
    },
    {
        "key": "Board/LevelKeyPieceTable",
        "comment": "关卡关键件固定落位关系",
        "fields": [
            ("LevelKey", "string", "关卡稳定键"),
            ("PartKind", "int", "PartType 数值"),
            ("Q", "int", "六边形坐标 Q"),
            ("R", "int", "六边形坐标 R"),
            ("SortOrder", "int", "落位顺序"),
        ],
    },
    {
        "key": "Board/LevelCarryPartTable",
        "comment": "关卡携带池关系",
        "fields": [
            ("LevelKey", "string", "关卡稳定键"),
            ("PartKind", "int", "PartType 数值"),
            ("SortOrder", "int", "携带池顺序"),
            ("IsBorrowed", "bool", "是否为现场借用种类"),
        ],
    },
    {
        "key": "Board/LevelFixedElementTable",
        "comment": "关卡固定元素关系",
        "fields": [
            ("LevelKey", "string", "关卡稳定键"),
            ("ElementKind", "int", "FixedElementKind 数值"),
            ("Q", "int", "六边形坐标 Q"),
            ("R", "int", "六边形坐标 R"),
            ("ObstacleType", "int", "ObstacleType 数值"),
            ("Label", "int", "端点标签"),
            ("GoalKey", "string", "绑定目标稳定键，空表示无"),
            ("GoalKind", "int", "GoalKind 数值"),
            ("GoalEntityId", "int", "目标实体 ID"),
            ("GoalAnchorLabel", "int", "目标锚点标签"),
            ("GoalRequired", "int", "目标所需量"),
            ("RepairPartKinds", "string", "维修零件 PartType 数值，逗号分隔"),
            ("RepairRequired", "int", "维修所需进度"),
            ("RepairEnergyCost", "int", "单次维修能量消耗"),
            ("InstanceData", "string", "实例关系 k=v；固定元素行ID"),
        ],
    },
    {
        "key": "Board/RedShoeTable",
        "comment": "红舞鞋关卡专属规则静态定义",
        "fields": [
            ("LevelKey", "string", "绑定关卡稳定键"),
            ("StartQ", "int", "红鞋起点 Q"),
            ("StartR", "int", "红鞋起点 R"),
            ("StartDirection", "int", "红鞋初始方向数值"),
            ("BoxQ", "int", "封存匣 Q"),
            ("BoxR", "int", "封存匣 R"),
            ("BoxRequired", "int", "封存匣维修所需进度"),
            ("EntryQ", "int", "封存入口 Q"),
            ("EntryR", "int", "封存入口 R"),
            ("RequiredDiversion", "int", "分离所需导流格数"),
            ("RestraintLimit", "int", "束缚失败上限"),
            ("StepsPerTap", "int", "每拍规则步进次数"),
        ],
    },
    {
        "key": "Board/RedShoeGuideTable",
        "comment": "红舞鞋导流格关系",
        "fields": [
            ("LevelKey", "string", "绑定关卡稳定键"),
            ("Q", "int", "导流格 Q"),
            ("R", "int", "导流格 R"),
            ("SortOrder", "int", "导流格顺序"),
        ],
    },
    {
        "key": "Board/RedShoeTurnTable",
        "comment": "红舞鞋转向格关系",
        "fields": [
            ("LevelKey", "string", "绑定关卡稳定键"),
            ("Q", "int", "转向格 Q"),
            ("R", "int", "转向格 R"),
            ("Direction", "int", "转向后的方向数值"),
            ("SortOrder", "int", "转向格顺序"),
        ],
    },
    {
        "key": "Board/FormTable",
        "comment": "形态的完整静态定义",
        "fields": [
            ("Key", "string", "形态稳定键（P-001-F01 或 M-001）"),
            ("Name", "string", "形态显示名称"),
            ("FormKind", "int", "形态种类（F=0、M=1）"),
            ("HostPartKind", "int", "宿主零件 PartType 数值"),
            ("Description", "string", "形态功能描述"),
            ("UnlockFee", "int", "加工所需维修费"),
            ("BlueprintId", "string", "所需图样 ID"),
            ("SourceHint", "string", "来源提示"),
            ("AutoSetCurrent", "bool", "解锁后是否自动设为当前形态"),
        ],
    },
    {
        "key": "Board/FormMaterialTable",
        "comment": "形态材料配方关系",
        "fields": [
            ("FormKey", "string", "所属形态稳定键"),
            ("MaterialKey", "string", "材料稳定键"),
            ("Count", "int", "所需数量"),
            ("SourceCase", "string", "类型化材料来源案件，空表示不限定"),
            ("SortOrder", "int", "配方显示顺序"),
        ],
    },
    {
        "key": "Board/BuffTable",
        "comment": "局内 Buff 的完整静态定义",
        "fields": [
            ("Key", "string", "稳定键（BF-001~BF-020）"),
            ("Name", "string", "Buff 显示名称"),
            ("Stage", "string", "最早进入内容池的玩家阶段"),
            ("Description", "string", "玩家可见效果描述"),
            ("Stackable", "bool", "是否允许叠加"),
            ("MaxStacks", "int", "最大叠加层数"),
            ("Effect", "int", "BuffEffectKind 数值"),
            ("Value", "int", "每层效果数值"),
            ("TargetPartKind", "int", "作用零件 PartType 数值，0 表示通用"),
        ],
    },
    {
        "key": "Board/AnomalyTable",
        "comment": "棋盘异常的完整静态参数",
        "fields": [
            ("Key", "string", "稳定键（AN-001~AN-008）"),
            ("AnomalyKind", "int", "AnomalyType 数值"),
            ("Deflection", "int", "局部定势偏转步数"),
            ("SettleDirection", "int", "沉降方向相位序号"),
            ("Threshold", "int", "扩张或过载阈值"),
            ("CoreCount", "int", "过载场芯数量"),
            ("TapPeriod", "int", "拍击后反转周期"),
            ("ExtraData", "string", EXTRA_DATA_NOTE),
        ],
        "sample_rows": [
            (1, "AN-001", 1, 1, 0, 0, 0, 0, "Priority=10;Controllable=1"),
            (2, "AN-002", 2, 0, 0, 3, 0, 0, "MaxExpansion=2"),
            (3, "AN-003", 3, 0, 0, 0, 0, 0, "EchoQuota=3;Resettable=1;NoEchoOfEcho=1"),
            (4, "AN-004", 4, 0, 0, 0, 0, 0, "SuppressStoryNodes=0;Controllable=1"),
            (5, "AN-005", 5, 0, 2, 0, 0, 0, "StepLength=1"),
            (6, "AN-006", 6, 0, 0, 3, 3, 0, "RechargeAmount=1"),
            (7, "AN-007", 7, 0, 0, 0, 0, 0, "MaxLayers=2;PerRoundTargets=1"),
            (8, "AN-008", 8, 0, 0, 0, 0, 2, "FlipDegrees=180"),
        ],
    },
    {
        "key": "Board/ChallengeTable",
        "comment": "怪谈专属挑战配置",
        "fields": [
            ("Key", "string", "挑战稳定键（C-001~C-015）"),
            ("CaseKey", "string", "绑定案件稳定键"),
            ("Name", "string", "挑战显示名称"),
            ("LevelKey", "string", "绑定关卡稳定键，空表示逐关参数后置"),
            ("ChallengeKind", "int", "ChallengeKind 数值"),
            ("RuleId", "string", "规则库编号（CR-xxx）"),
            ("Stage", "string", "首次投放阶段（S1~S5）"),
            ("GoalText", "string", "挑战目标文案"),
            ("ExtraData", "string", EXTRA_DATA_NOTE),
        ],
    },
    {
        "key": "Meta/StageReleaseTable",
        "comment": "阶段投放与解锁条件",
        "fields": [
            ("Key", "string", "投放条目稳定键"),
            ("ReleaseKind", "int", "ReleaseKind 数值"),
            ("Stage", "int", "StageLevel 数值"),
            ("FirstAppearance", "string", "首次出现提示"),
            ("UnlockCondition", "string", "永久解锁条件"),
        ],
    },
    {
        "key": "Meta/PlaceTable",
        "comment": "世界地图地点目录",
        "fields": [
            ("Key", "string", "地点稳定键"),
            ("Name", "string", "地点名称"),
            ("Description", "string", "地点描述"),
            ("UnlockSource", "int", "PlaceUnlockSource 数值"),
            ("X", "float", "地图 X 坐标"),
            ("Y", "float", "地图 Y 坐标"),
            ("IsHome", "bool", "是否为家园地点"),
        ],
    },
    {
        "key": "Meta/TrialTable",
        "comment": "试机配置与特殊目标",
        "fields": [
            ("Key", "string", "试机稳定键"),
            ("Name", "string", "试机名称"),
            ("BorrowedFormKey", "string", "借用形态稳定键"),
            ("HostPartKind", "int", "宿主 PartType 数值"),
            ("BlueprintKey", "string", "完成后获得的图样键"),
            ("TapCount", "int", "每轮拍数"),
            ("TargetScore", "int", "每轮目标分"),
            ("SpecialGoal", "string", "特殊目标描述"),
        ],
    },
    {
        "key": "Meta/TaskTable",
        "comment": "任务模板与投放条件",
        "fields": [
            ("Key", "string", "任务稳定键"),
            ("Name", "string", "任务名称"),
            ("RewardFee", "int", "完成奖励维修费"),
            ("IsMain", "bool", "是否主线任务"),
            ("Client", "string", "委托人"),
            ("Description", "string", "任务描述"),
            ("TotalSteps", "int", "总步骤数"),
            ("RequiredCaseDone", "string", "前置案件键，空表示无"),
            ("RequiredSuccessCount", "int", "前置成功普通维修次数"),
            ("PlaceKey", "string", "目标地点键"),
        ],
    },
    {
        "key": "Meta/TaskBlueprintTable",
        "comment": "任务奖励图样关系",
        "fields": [
            ("TaskKey", "string", "任务稳定键"),
            ("BlueprintKey", "string", "图样键"),
            ("SortOrder", "int", "奖励图样顺序"),
        ],
    },
    {
        "key": "Meta/EventTable",
        "comment": "普通供给事件模板",
        "fields": [
            ("Key", "string", "事件稳定键"),
            ("Name", "string", "事件显示名称"),
            ("EventKind", "int", "EventKind 数值"),
            ("MinStage", "int", "最早投放阶段"),
            ("DayWeight", "int", "白天投放权重"),
            ("NightWeight", "int", "夜晚投放权重"),
            ("TimeCost", "int", "完成后消耗的时间格"),
            ("RewardFee", "int", "完成奖励维修费"),
            ("PlaceKey", "string", "投放地点稳定键"),
            ("Repeatable", "bool", "是否可重复"),
            ("FailureWrapUp", "string", "失败收尾文案"),
            ("RewardMaterialKey", "string", "奖励材料稳定键，空表示无"),
        ],
    },
    {
        "key": "Meta/EventOpenPeriodTable",
        "comment": "事件开放时段关系",
        "fields": [
            ("EventKey", "string", "事件稳定键"),
            ("Period", "int", "TimeOfDay 数值"),
            ("SortOrder", "int", "开放时段显示顺序"),
        ],
    },
    {
        "key": "Meta/RewardMaterialTable",
        "comment": "事件与案件奖励材料关系",
        "fields": [
            ("OwnerKey", "string", "奖励所属事件或案件稳定键"),
            ("MaterialKey", "string", "材料稳定键"),
            ("Count", "int", "奖励数量"),
            ("SourceCase", "string", "类型化材料来源案件，空表示不限定"),
            ("SortOrder", "int", "奖励显示顺序"),
        ],
    },
    {
        "key": "Meta/MaterialTable",
        "comment": "材料的完整静态定义",
        "fields": [
            ("Key", "string", "稳定键（MT-001~MT-006）"),
            ("Name", "string", "材料显示名称"),
            ("Rarity", "int", "材料稀有度（Common=0、Refined=1、Rare=2）"),
            ("IsTypedByCase", "bool", "是否按来源怪谈类型化"),
            ("FlavorText", "string", "材料风味描述"),
        ],
    },
    {
        "key": "Meta/CaseTable",
        "comment": "怪谈案件模板静态定义",
        "fields": [
            ("Key", "string", "案件稳定键"),
            ("Name", "string", "案件名称"),
            ("Batch", "string", "故事批次"),
            ("TotalStages", "int", "案件总阶段数"),
            ("Source", "string", "案件来源类型"),
            ("FirstPlaceKey", "string", "首次出现地点稳定键"),
        ],
    },
    {
        "key": "Meta/InvestigationTable",
        "comment": "调查入口静态定义",
        "fields": [
            ("Key", "string", "调查稳定键"),
            ("Name", "string", "调查名称"),
            ("ObservationCount", "int", "所需观察项数量"),
            ("CaseKey", "string", "绑定案件稳定键"),
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
    {
        "key": "Meta/CopyTable",
        "comment": "统一文案定义",
        "fields": [
            ("Key", "string", "文案稳定键"),
            ("Category", "int", "文案分类枚举"),
            ("Text", "string", "文案正文"),
            ("Calibrated", "bool", "是否已完成文案校准"),
        ],
        "sample_rows": [
            (1, "DLG-OPEN", 0, "你回到槐江市，代管长明修理铺。", True),
            (2, "DLG-MASTER", 0, "师父周衡：店里就交给你了，先跟我学怎么看盘面。", True),
            (3, "DLG-SHEN", 0, "沈遥：那台节拍器……它停不下来，帮帮我。", True),
            (4, "HINT-ROTATE", 1, "旋转内盘，让撞锤沿定势方向撞向标记或固定墙。", True),
            (5, "HINT-ENERGY", 1, "把连锁接到棘轮，让它产出公共维修能量。", True),
            (6, "HINT-BLAST", 1, "用爆破线圈炸开脆裂隔板，让目标标记通过校正格。", True),
            (7, "FAIL-NOT-MET", 2, "未达标", True),
            (8, "FAIL-RETREAT", 2, "已撤退，本次尝试不记失败。", True),
            (9, "UI-TITLE-MAP", 3, "槐江市地图", True),
            (10, "UI-TITLE-CODEX", 3, "图鉴", True),
            (11, "UI-TITLE-WORKBENCH", 3, "加工台", True),
            (12, "UI-UNKNOWN", 3, "？？？", True),
        ],
    },
    {
        "key": "Meta/PortraitTable",
        "comment": "人物立绘目录",
        "fields": [
            ("Key", "string", "人物稳定键"),
            ("Name", "string", "人物名称"),
            ("Description", "string", "人物说明"),
        ],
        "sample_rows": [
            (1, "CH-MASTER", "师父周衡", "长明修理铺原店主，画作修复期间失联"),
            (2, "CH-SHEN", "沈遥", "舞蹈教室求助者，红舞鞋事件当事人"),
            (3, "CH-PARTNER", "搭档", "维修协作搭档，事件间对话对象"),
        ],
    },
    {
        "key": "Meta/TutorialHintTable",
        "comment": "教学引导提示",
        "fields": [
            ("Key", "string", "教学提示稳定键"),
            ("StageIndex", "int", "教学段序号，从 0 开始"),
            ("Title", "string", "教学标题"),
            ("TeachGoal", "string", "教学目标"),
            ("GuideText", "string", "引导文案"),
            ("HighlightTarget", "string", "高亮目标"),
            ("UnlockPartKind", "int", "通过后解锁的零件 PartType 数值"),
        ],
        "sample_rows": [
            (1, "TUT-001", 0, "第一段 旋转与碰撞", "选方向、拍击、看碰撞", "旋转内盘，让撞锤沿定势方向撞向标记或固定墙；压下冲击柄拍击。", "旋转按钮", 1),
            (2, "TUT-002", 1, "第二段 能量与维修", "连锁怎样产出维修能量", "把连锁接到棘轮，让它触发产出公共维修能量；再让一次有效维修落在待维修对象上。", "计量棘轮", 2),
            (3, "TUT-003", 2, "第三段 爆破与拆障", "用爆破打通通路", "用爆破线圈炸开脆裂隔板，让目标标记通过校正格。", "爆破线圈", 3),
        ],
    },
    {
        "key": "Meta/CalibrationTable",
        "comment": "关卡与经济校准参数",
        "fields": [
            ("Key", "string", "校准项稳定键"),
            ("Category", "int", "校准分类枚举"),
            ("Name", "string", "校准项名称"),
            ("Value", "int", "当前校准值"),
            ("Min", "int", "可接受下界"),
            ("Max", "int", "可接受上界"),
            ("Note", "string", "校准说明"),
        ],
        "sample_rows": [
            (1, "TUT-STAGES", 0, "教学段数", 3, 3, 3, "固定三段"),
            (2, "TUT-TIME", 0, "教学时间格", 0, 0, 0, "0 时间格"),
            (3, "TUT-SCORE", 0, "教学通过分", 30, 20, 40, "三段 28/30/32 的宽松区间"),
            (4, "RS-ROUNDS", 1, "红舞鞋轮数", 3, 3, 3, "三轮"),
            (5, "RS-TARGET", 1, "红舞鞋末轮目标分", 170, 140, 200, "30/100/170 末轮口径"),
            (6, "RS-TIME", 1, "红舞鞋时间格", 4, 3, 5, "耗时 4"),
            (7, "RS-DIVERSION", 1, "导流分离次数", 6, 5, 7, "D-051 导流 6 次分离"),
            (8, "RD-ROUNDS", 2, "卷帘门轮数", 1, 1, 1, "一轮"),
            (9, "RD-TAPS", 2, "卷帘门拍数", 3, 2, 5, "拍数口径"),
            (10, "RD-SCORE", 2, "卷帘门目标分", 30, 20, 40, "目标分口径"),
            (11, "RD-TIME", 2, "卷帘门时间格", 2, 1, 3, "时间格口径"),
            (12, "RD-FEE", 2, "卷帘门维修费", 40, 20, 60, "奖励费口径"),
            (13, "ECO-FEE-CELL", 3, "维修费单价", 20, 15, 25, "20 费/格"),
            (14, "ECO-MOD-CLAIM", 3, "改装支线领奖", 40, 30, 50, "改装领奖费"),
            (15, "ECO-FORM-MIN", 3, "形态解锁费下界", 80, 60, 100, "F 类起点 80"),
            (16, "ECO-FORM-MAX", 3, "形态解锁费上界", 480, 400, 560, "M 类上限 480"),
            (17, "ECO-REVISIT", 3, "回访奖励费", 120, 100, 140, "回访费口径"),
            (18, "REPAIR-SCORE", 3, "维修效果分", 20, 20, 20, "默认维修对象每次有效维修效果分"),
        ],
    },
    {
        "key": "Board/ObstacleTable",
        "comment": "障碍设施参数",
        "fields": [
            ("Key", "string", "障碍稳定键"),
            ("ObstacleKind", "int", "ObstacleType 数值"),
            ("MaxDurability", "int", "最大耐久，无耐久为 0"),
            ("PassDirection", "int", "方向参数，无方向为 -1"),
            ("ExtraData", "string", EXTRA_DATA_NOTE),
        ],
        "sample_rows": [
            (1, "O-001", 1, 0, -1, ""),
            (2, "O-002", 2, 2, -1, "RootBlastOncePerRoot=1"),
            (3, "O-003", 3, 0, -1, "InitialOpen=0;ToggleOncePerRoot=1;DeferCloseWhenOccupied=1"),
            (4, "O-004", 4, 0, 1, ""),
            (5, "O-005", 5, 0, -1, "AllowTags=normal;InitialClampedEntityId=0;ReleaseWindowTaps=1"),
            (6, "O-006", 6, 0, 0, "ToggleDirection=3"),
            (7, "O-007", 7, 1, -1, "GrowthPerTap=1;SegmentMaxDurability=1"),
            (8, "O-008", 8, 3, -1, "AttachedPartitions=2;ProtectedObject=0;Repairable=1"),
        ],
    },
    {
        "key": "Meta/AudioCueTable",
        "comment": "音效位目录",
        "fields": [
            ("Key", "string", "音效位稳定键"),
            ("CueKind", "int", "AudioCueKind 数值"),
            ("Name", "string", "音效位名称"),
            ("Description", "string", "音效位用途说明"),
        ],
        "sample_rows": [
            (1, "CUE-UI", 0, "点击", "按钮/选项点击反馈"),
            (2, "CUE-SNAP", 1, "零件咬合", "零件碰撞/触发咬合"),
            (3, "CUE-ENV", 2, "环境音", "店铺/街巷环境底噪"),
            (4, "CUE-REPAIR", 3, "维修", "有效维修落点"),
            (5, "CUE-TAP", 4, "拍击", "冲击柄拍击定势"),
            (6, "CUE-CULTURE", 5, "怪谈文化", "怪谈事件专属氛围"),
        ],
    },
    {
        "key": "Meta/BatchTable",
        "comment": "怪谈批次开放与完成条件目录",
        "fields": [
            ("Key", "string", "批次稳定键"),
            ("BatchNumber", "int", "批次序号"),
            ("Name", "string", "批次名称"),
            ("RequiredCount", "int", "开放下一批所需完成数量"),
        ],
        "sample_rows": [
            (1, "B1", 1, "第一批", 1),
            (2, "B2", 2, "第二批", 3),
            (3, "B3", 3, "第三批", 5),
            (4, "B4", 4, "第四批", 5),
            (5, "B5", 5, "第五批", 1),
        ],
    },
    {
        "key": "Meta/BgmTable",
        "comment": "昼夜背景音乐目录",
        "fields": [
            ("Key", "string", "BGM 稳定键"),
            ("Track", "int", "BgmTrack 数值"),
            ("Name", "string", "BGM 名称"),
            ("Description", "string", "BGM 用途说明"),
        ],
        "sample_rows": [
            (1, "BGM-DAY", 0, "长明白昼", "白昼店铺与街巷背景音乐"),
            (2, "BGM-NIGHT", 1, "长明深夜", "夜晚追查与怪谈背景音乐"),
        ],
    },
    {
        "key": "Meta/AssetBindingTable",
        "comment": "正式资源槽位目录",
        "fields": [
            ("Key", "string", "资源槽位稳定键"),
            ("Category", "int", "AssetCategory 数值"),
            ("Name", "string", "资源槽位名称"),
            ("Path", "string", "资源路径，未挂接时为空"),
        ],
        "sample_rows": [
            (1, "AS-UI-THEME", 0, "主题背景", ""),
            (2, "AS-UI-HEX", 1, "盘面六边形底", ""),
            (3, "AS-BGM-DAY", 2, "长明白昼", ""),
            (4, "AS-BGM-NIGHT", 2, "长明深夜", ""),
            (5, "AS-PORT-MASTER", 3, "师父周衡", ""),
            (6, "AS-PORT-SHEN", 3, "沈遥", ""),
            (7, "AS-SFX-TAP", 4, "拍击", ""),
            (8, "AS-SFX-REPAIR", 4, "维修", ""),
            (9, "AS-VFX-PULSE", 5, "连击脉冲", ""),
        ],
    },
    {
        "key": "Meta/NarrativeTable",
        "comment": "首案序章与对话节点目录",
        "fields": [
            ("Key", "string", "叙事条目稳定键"),
            ("ScriptKey", "string", "脚本图稳定键，opening/request/confirmed/thanks"),
            ("SortOrder", "int", "脚本内顺序"),
            ("NodeKey", "string", "对话节点键，序章可为空"),
            ("Speaker", "string", "说话人"),
            ("Text", "string", "台词或旁白正文"),
            ("ChoiceLabel", "string", "当前节点选项文案，空表示无选项"),
            ("NextNodeKey", "string", "选项目标节点，空表示结束"),
            ("Duration", "float", "序章停留秒数，对话为 0"),
        ],
        "sample_rows": [
            (1, "N-OPEN-001", "opening", 0, "", "旁白", "夜雨落在旧城区的铁皮屋顶上。", "", "", 2.5),
            (2, "N-OPEN-002", "opening", 1, "", "玩家", "我回来了。长明修理铺的灯还亮着。", "", "", 3.0),
            (3, "N-OPEN-003", "opening", 2, "", "周衡", "先熟悉工作台，再一起接下邻里的维修委托。", "", "", 3.0),
            (4, "N-REQ-001", "request", 0, "request", "沈遥", "节拍器和音乐停下了，我的脚却还在补完下一组动作。", "去舞蹈教室查看", "reply", 0.0),
            (5, "N-REQ-002", "request", 1, "reply", "玩家", "先确认红鞋和动作之间的联系，别强行抓住她。", "", "", 0.0),
            (6, "N-CON-001", "confirmed", 0, "confirmed", "玩家", "音乐停止后，红鞋仍在带动身体。先导流、分离，再修好封存匣。", "记下作业目标", "response", 0.0),
            (7, "N-CON-002", "confirmed", 1, "response", "沈遥", "让我停下来。鞋还在动，也请把它单独封存。", "", "", 0.0),
            (8, "N-REV-001", "thanks", 0, "thanks", "沈遥", "今天终于能按自己的节奏走路了。谢谢你，这份报酬请收下。", "领取回访报酬", "reply", 0.0),
            (9, "N-REV-002", "thanks", 1, "reply", "玩家", "红鞋已经封存。如果还有异常，就来长明修理铺找我。", "", "", 0.0),
        ],
    },
    {
        "key": "Meta/CaseEventTable",
        "comment": "首案事件入口与目标说明",
        "fields": [
            ("Key", "string", "案件事件稳定键"),
            ("CaseKey", "string", "案件稳定键"),
            ("InvestigationTitle", "string", "调查入口标题"),
            ("InvestigationDescription", "string", "调查入口说明"),
            ("RepairTitle", "string", "维修入口标题"),
            ("RepairDescription", "string", "维修入口说明"),
            ("RepairTimeCost", "int", "维修入口显示耗时格数"),
            ("InvestigationPageTitle", "string", "调查页面标题"),
            ("Hotspot1Name", "string", "调查热点一名称"),
            ("Hotspot1X", "float", "调查热点一归一化横坐标"),
            ("Hotspot1Y", "float", "调查热点一归一化纵坐标"),
            ("Hotspot2Name", "string", "调查热点二名称"),
            ("Hotspot2X", "float", "调查热点二归一化横坐标"),
            ("Hotspot2Y", "float", "调查热点二归一化纵坐标"),
            ("Hotspot3Name", "string", "调查热点三名称"),
            ("Hotspot3X", "float", "调查热点三归一化横坐标"),
            ("Hotspot3Y", "float", "调查热点三归一化纵坐标"),
            ("RevisitFee", "int", "回访奖励维修费"),
            ("RevisitBlueprintKey", "string", "回访图样稳定键"),
            ("CompletionPartKind", "int", "结案后解锁的零件 PartType 数值"),
        ],
        "sample_rows": [
            (1, "L-01-01", "L-01", "没有结束的排练", "确认音乐停止、鞋印增加和反复出现的四拍步法。开放：夜晚、深夜。", "停不下来的排练", "导流、分离、维修封存匣，再把红鞋封存。开放：夜晚、深夜。", 4, "舞蹈教室 · 没有结束的排练", "音乐已经停止", 0.22, 0.65, "鞋印仍在增加", 0.75, 0.4, "反复重来的四拍步法", 0.25, 0.2, 120, "L-01", 5),
        ],
    },
    {
        "key": "Meta/CaseRewardOptionTable",
        "comment": "案件局内奖励选项关系",
        "fields": [
            ("Key", "string", "奖励选项关系稳定键"),
            ("OwnerKey", "string", "所属案件事件稳定键"),
            ("RewardKind", "int", "RewardKind 数值"),
            ("RewardId", "string", "Buff、零件或机械臂奖励键"),
            ("SortOrder", "int", "奖励池顺序"),
            ("Label", "string", "奖励显示文案"),
        ],
        "sample_rows": [
            (1, "RO-L01-001", "L-01-01", 0, "BF-001", 0, "增压回路"),
            (2, "RO-L01-002", "L-01-01", 0, "BF-002", 1, "缓冲线圈"),
            (3, "RO-L01-003", "L-01-01", 0, "BF-003", 2, "余震"),
            (4, "RO-L01-004", "L-01-01", 0, "BF-004", 3, "导流增幅"),
            (5, "RO-L01-005", "L-01-01", 0, "BF-011", 4, "能量回灌"),
            (6, "RO-L01-006", "L-01-01", 0, "BF-014", 5, "校准偏置"),
            (7, "RO-L01-007", "L-01-01", 0, "BF-015", 6, "拆障加速"),
            (8, "RO-L01-008", "L-01-01", 0, "BF-016", 7, "长程撞锤"),
            (9, "RO-L01-009", "L-01-01", 0, "BF-020", 8, "维修折返"),
            (10, "RO-L01-010", "L-01-01", 2, "arm", 9, "机械臂 +1"),
            (11, "RO-L01-011", "L-01-01", 1, "InertiaHammer", 10, "本关补给 · 惯性撞锤"),
            (12, "RO-L01-012", "L-01-01", 1, "MeteringRatchet", 11, "本关补给 · 计量棘轮"),
            (13, "RO-L01-013", "L-01-01", 1, "BlastCoil", 12, "本关补给 · 爆破线圈"),
            (14, "RO-L01-014", "L-01-01", 1, "RivetPliers", 13, "本关补给 · 铆钉钳"),
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
        # 已进入迁移的业务表以已提交的生成文本作为往返种子，避免旧的骨架 schema 在
        # `--seed` 时把正式配置行覆盖回示例数据。Unity 生成器仍以 Excel 为输入；这里
        # 只负责在缺少源文件或执行显式 seed 时保留可重复的工程工具链。
        committed_txt = TXT_ROOT / f"{key}.txt"
        rows = parse_txt(committed_txt) if key in {"Board/PartTable", "Board/FormTable", "Board/FormMaterialTable", "Meta/MaterialTable"} and committed_txt.exists() else schema_rows(schema)
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
