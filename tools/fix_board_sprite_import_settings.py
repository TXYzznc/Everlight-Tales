# -*- coding: utf-8 -*-
"""把 Assets/Game/Sprites/盘面/ 下正式美术 PNG 的 .meta maxTextureSize 统一为 256。

正式盘面图（零件/形态/障碍/异常/标记）源图为 230×230，256 为不缩放的最小 2 的幂，
保留全细节（对比 Unity 默认 2048 明显浪费）。仅替换 maxTextureSize 数值，保留
guid / spriteID / 其余字段不动（与 fix_placeholder_import_settings.py 的整段重写不同）。

用法：python tools/fix_board_sprite_import_settings.py [--check]
"""
from __future__ import annotations

import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
BASE = REPO_ROOT / "Assets" / "Game" / "Sprites" / "盘面"

MAXSIZE = 256

SIZE_RE = re.compile(r"maxTextureSize: \d+")


def main() -> int:
    check = "--check" in sys.argv
    metas = sorted(BASE.rglob("*.png.meta"))
    changed = 0
    for meta in metas:
        text = meta.read_text(encoding="utf-8")
        sizes = [int(m) for m in re.findall(r"maxTextureSize: (\d+)", text)]
        if check:
            if any(s != MAXSIZE for s in sizes):
                print(f"[WOULD] {sizes} -> {MAXSIZE}  {meta.relative_to(REPO_ROOT)}")
                changed += 1
            continue
        if any(s != MAXSIZE for s in sizes):
            new = SIZE_RE.sub(f"maxTextureSize: {MAXSIZE}", text)
            meta.write_text(new, encoding="utf-8", newline="\n")
            print(f"[OK] {sizes} -> {MAXSIZE}  {meta.relative_to(REPO_ROOT)}")
            changed += 1
    print(f"[DONE] {'would-change' if check else 'changed'} {changed}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
