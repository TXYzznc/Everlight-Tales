#!/usr/bin/env python3
"""Create deterministic Unity .meta files for newly added Assets content.

Unity generates .meta files on import, but command-line and reviewer workflows
in this repository expect the meta of new assets to exist in the same commit as
the asset itself. This tool only *creates missing* meta files; it never rewrites
an existing one, so Unity-authored GUIDs stay authoritative.

Usage:
    python tools/unity_meta.py                       # scan Assets/ for missing meta
    python tools/unity_meta.py --path Assets/Game    # narrow the scan root
    python tools/unity_meta.py --check               # report only, exit 1 if missing
"""

from __future__ import annotations

import argparse
import hashlib
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
ASSETS_ROOT = REPO_ROOT / "Assets"
GUID_NAMESPACE = b"everlight-tales-unity-meta-v1"

# Unity treats these as folders regardless of the filesystem entry type.
KNOWN_FOLDER_SUFFIXES = {".meta"}

FOLDER_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

SCRIPT_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

ASSEMBLY_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
AssemblyDefinitionImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

DEFAULT_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

SCENE_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

TEXT_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
TextScriptImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

# suffix -> template. Keys are matched case-sensitively like Unity does.
FILE_TEMPLATES = {
    ".cs": SCRIPT_TEMPLATE,
    ".asmdef": ASSEMBLY_TEMPLATE,
    ".asmref": ASSEMBLY_TEMPLATE,
    ".unity": SCENE_TEMPLATE,
    ".md": TEXT_TEMPLATE,
    ".txt": TEXT_TEMPLATE,
    ".json": TEXT_TEMPLATE,
    ".asset": DEFAULT_TEMPLATE,
}


def deterministic_guid(relative_path: str) -> str:
    """Stable 32-hex GUID derived from the asset's repo-relative path."""
    digest = hashlib.sha256(GUID_NAMESPACE + relative_path.replace("\\", "/").encode("utf-8")).hexdigest()
    return digest[:32]


def iter_assets(scan_root: Path):
    for entry in sorted(scan_root.rglob("*")):
        name = entry.name
        if name.endswith(".meta"):
            continue
        if any(part.startswith(".") or part == "__pycache__" for part in entry.relative_to(scan_root).parts):
            continue
        yield entry


def template_for(entry: Path) -> str:
    if entry.is_dir():
        return FOLDER_TEMPLATE
    return FILE_TEMPLATES.get(entry.suffix, DEFAULT_TEMPLATE)


def repo_relative(entry: Path) -> str:
    return entry.relative_to(REPO_ROOT).as_posix()


def process(entry: Path, check_only: bool) -> bool:
    """Return True when the entry needed (and received) a meta file."""
    meta_path = entry.with_name(entry.name + ".meta")
    if meta_path.exists():
        return False

    relative = repo_relative(entry)
    if check_only:
        print(f"[MISSING] {relative}")
        return True

    meta_path.write_text(
        template_for(entry).format(guid=deterministic_guid(relative)),
        encoding="utf-8",
        newline="\n",
    )
    print(f"[CREATED] {relative}.meta")
    return True


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--path", default=str(ASSETS_ROOT), help="scan root inside the repository")
    parser.add_argument("--check", action="store_true", help="report missing meta files without writing them")
    args = parser.parse_args()

    scan_root = Path(args.path)
    if not scan_root.is_absolute():
        scan_root = REPO_ROOT / scan_root
    if not scan_root.is_dir():
        print(f"scan root does not exist: {scan_root}", file=sys.stderr)
        return 2

    created = 0
    for entry in iter_assets(scan_root):
        if process(entry, args.check):
            created += 1

    verb = "missing" if args.check else "created"
    print(f"[OK] meta files {verb}: {created}")
    return 1 if (args.check and created) else 0


if __name__ == "__main__":
    raise SystemExit(main())
