#!/usr/bin/env python3
"""Verify the business assembly layout declared by the project assemblies.

Checks performed:

1. Every ``Assets/Game/Scripts/EverlightTales/<Layer>/*.asmdef`` parses and declares the
   expected assembly name, root namespace and directory.
2. Every entry in ``references`` resolves either to another asmdef in this repository or to
   a known Unity/framework assembly that exists as an asmdef somewhere under ``Assets/``.
3. The business reference graph is acyclic.
4. Layer reference directions follow the confirmed layout: ``Data`` references no other
   business assembly, ``Board``/``Meta`` reference only ``Data`` (+
   ``Events`` references ``Data`` and ``Board``), ``UI`` may reference any of them.
5. Engine-independent layers (``Data``, ``Board``, ``Events``) set
   ``noEngineReferences: true``; the engine-facing layers do not.

Usage:
    python tools/verify_project_assemblies.py
    python tools/verify_project_assemblies.py --root Assets/Game/Scripts/EverlightTales
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import defaultdict
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
ASSETS_ROOT = REPO_ROOT / "Assets"
DEFAULT_ROOT = ASSETS_ROOT / "Game" / "Scripts" / "EverlightTales"
BUSINESS_PREFIX = "Everlight.Tales."

# Assembly definitions may live in the project, in a local package, or in an
# immutable package pulled into the package cache. All three are legitimate
# reference targets.
ASMDEF_SEARCH_ROOTS = (
    ASSETS_ROOT,
    REPO_ROOT / "Packages",
    REPO_ROOT / "Library" / "PackageCache",
)

# layer directory -> (assembly name suffix, expected references to other business assemblies)
EXPECTED_LAYERS = {
    "Data": (),
    "Board": ("Data",),
    "Meta": ("Data",),
    "Events": ("Data", "Board"),
    "UI": ("Data", "Board", "Events", "Meta"),
}

# Layers that must not pull in the Unity engine.
ENGINE_FREE_LAYERS = {"Data", "Board", "Events"}


def load_asmdefs(path: Path):
    for file in sorted(path.rglob("*.asmdef")):
        try:
            data = json.loads(file.read_text(encoding="utf-8"))
        except json.JSONDecodeError as exc:
            yield file, None, f"invalid JSON: {exc}"
            continue
        yield file, data, None


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--root", default=str(DEFAULT_ROOT), help="business assembly root")
    args = parser.parse_args()

    root = Path(args.root)
    if not root.is_absolute():
        root = REPO_ROOT / root
    if not root.is_dir():
        print(f"[FAIL] business assembly root not found: {root}", file=sys.stderr)
        return 2

    errors: list[str] = []
    business: dict[str, dict] = {}
    layer_of: dict[str, str] = {}

    for file, data, error in load_asmdefs(root):
        relative = file.relative_to(REPO_ROOT).as_posix()
        if error:
            errors.append(f"{relative}: {error}")
            continue

        name = data.get("name")
        if not name:
            errors.append(f"{relative}: missing 'name'")
            continue
        if not name.startswith(BUSINESS_PREFIX):
            errors.append(f"{relative}: business assembly name must start with '{BUSINESS_PREFIX}' (got '{name}')")
            continue

        layer = file.parent.name
        expected_name = BUSINESS_PREFIX + layer
        if name != expected_name:
            errors.append(f"{relative}: assembly name '{name}' does not match directory layer '{layer}' (expected '{expected_name}')")
            continue
        if data.get("rootNamespace") != name:
            errors.append(f"{relative}: rootNamespace must equal '{name}'")

        if layer not in EXPECTED_LAYERS:
            errors.append(f"{relative}: unknown layer '{layer}'")
            continue

        business[name] = data
        layer_of[name] = layer

    missing_layers = sorted(set(EXPECTED_LAYERS) - {layer_of[n] for n in business})
    for layer in missing_layers:
        errors.append(f"missing assembly for layer '{layer}'")

    # Reference presence against the confirmed layout.
    for name, data in business.items():
        layer = layer_of[name]
        references = data.get("references") or []
        actual_business = {ref for ref in references if ref.startswith(BUSINESS_PREFIX)}
        expected_business = {BUSINESS_PREFIX + dep for dep in EXPECTED_LAYERS[layer]}
        if actual_business != expected_business:
            errors.append(
                f"{layer}: business references {sorted(actual_business)} do not match the confirmed layout "
                f"{sorted(expected_business)}"
            )

    # Every reference must resolve to an asmdef that exists in the project, a local
    # package, or the package cache.
    known = set()
    for search_root in ASMDEF_SEARCH_ROOTS:
        if not search_root.is_dir():
            continue
        for file in search_root.rglob("*.asmdef"):
            if "__pycache__" in file.parts:
                continue
            try:
                known.add(json.loads(file.read_text(encoding="utf-8")).get("name"))
            except (json.JSONDecodeError, OSError):
                continue
    for name, data in business.items():
        for ref in data.get("references") or []:
            if ref not in known:
                errors.append(
                    f"{layer_of[name]}: reference '{ref}' does not resolve to any asmdef under "
                    "Assets/, Packages/ or Library/PackageCache/"
                )

    # Cycle detection over the business graph only.
    graph = defaultdict(set)
    for name, data in business.items():
        graph[name] = {ref for ref in (data.get("references") or []) if ref.startswith(BUSINESS_PREFIX)}
    state: dict[str, int] = {}

    def visit(node: str, trail: list[str]) -> None:
        if state.get(node) == 1:
            errors.append("business assembly reference cycle: " + " -> ".join(trail + [node]))
            return
        if state.get(node) == 2:
            return
        state[node] = 1
        for dep in sorted(graph.get(node, ())):
            visit(dep, trail + [node])
        state[node] = 2

    for node in sorted(graph):
        visit(node, [])

    # Engine independence.
    for name, data in business.items():
        layer = layer_of[name]
        flag = bool(data.get("noEngineReferences"))
        if layer in ENGINE_FREE_LAYERS and not flag:
            errors.append(f"{layer}: 'noEngineReferences' must be true for engine-independent layers")
        if layer not in ENGINE_FREE_LAYERS and flag:
            errors.append(f"{layer}: 'noEngineReferences' must be false for engine-facing layers")

    if errors:
        for error in errors:
            print(f"[FAIL] {error}")
        print(f"[FAIL] project assembly verification: {len(errors)} problem(s)")
        return 1

    print(f"[OK] project assembly verification passed: {len(business)} business assemblies, acyclic, layout confirmed")
    for name in sorted(business):
        layer = layer_of[name]
        refs = ", ".join(sorted(business[name].get("references") or [])) or "(none)"
        flag = "engine-free" if business[name].get("noEngineReferences") else "engine-facing"
        print(f"     {layer:<7} {name:<26} {flag:<13} refs: {refs}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
