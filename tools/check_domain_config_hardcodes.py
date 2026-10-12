"""Check that migrated domain content is not reintroduced as production literals."""

from pathlib import Path
import re
import sys


ROOT = Path(__file__).resolve().parents[1]
RULES = {
    "Assets/Game/Scripts/EverlightTales/UI/WorldSession.FirstCaseRewards.cs": [
        (r'"BF-\d+"', "首案 Buff 奖励池应来自 CaseRewardOptionTable"),
        (r'PartType\.(InertiaHammer|MeteringRatchet|BlastCoil|RivetPliers)', "首案零件奖励池应来自 CaseRewardOptionTable"),
    ],
    "Assets/Game/Scripts/EverlightTales/UI/WorldSession.FirstCase.cs": [
        (r'RevisitFee\s*=\s*\d+', "首案回访费应来自 CaseEventTable"),
        (r'RevisitBlueprints\s*=\s*new\[\]\s*\{\s*"', "首案回访图样应来自 CaseEventTable"),
    ],
    "Assets/Game/Scripts/EverlightTales/UI/WorldSession.cs": [
        (r'RevisitFee\s*=\s*\d+', "回访费应来自 CaseEventTable"),
        (r'RevisitBlueprints\s*=\s*new\[\]\s*\{\s*"', "回访图样应来自 CaseEventTable"),
    ],
    "Assets/Game/Scripts/EverlightTales/Meta/TutorialService.cs": [
        (r'case\s+\d+\s*:\s*return\s+PartType\.', "教学解锁零件应来自 TutorialHintTable"),
    ],
}


def main() -> int:
    failures = []
    for relative, checks in RULES.items():
        path = ROOT / relative
        if not path.exists():
            failures.append(f"missing: {relative}")
            continue
        text = path.read_text(encoding="utf-8")
        for pattern, message in checks:
            match = re.search(pattern, text)
            if match:
                line = text.count("\n", 0, match.start()) + 1
                failures.append(f"{relative}:{line}: {message}")
    if failures:
        for failure in failures:
            print(f"[FAIL] {failure}")
        return 1
    print(f"[OK] migrated domain hardcode scan passed: {len(RULES)} production entry points")
    return 0


if __name__ == "__main__":
    sys.exit(main())
