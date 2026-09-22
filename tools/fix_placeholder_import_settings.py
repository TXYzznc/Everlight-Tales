#!/usr/bin/env python3
"""把 Assets/Game/Sprites/Placeholder 下占位 PNG 的 .meta 统一改为正确的
Sprite 导入设置，并按屏幕预计大小设置 maxTextureSize。

档位（据屏幕预计大小）：
- 盘面棋子（Board/parts|forms|obstacles|anomalies|others）: 128  （HexBoardView m_CellSize=28px，棋子直径约 56px，128 含 2x 高分屏余量）
- UI 大面板（UI/panel、UI/dialog）                     : 512  （可能半屏~全屏拉伸）
- UI 按钮/页签（UI/button_*、UI/tab_*）                : 256  （屏幕约 100~200px）

关键字段：textureType=8(Sprite)、spriteMode=1(Single)、alphaIsTransparency=1、
enableMipMap=0、wrapMode=1(Clamp)。保留原有 guid 不动。

用法：python tools/fix_placeholder_import_settings.py [--check]
"""

from __future__ import annotations

import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
BASE = REPO_ROOT / "Assets" / "Game" / "Sprites" / "Placeholder"

# 完整 TextureImporter（Unity 2022.3，serializedVersion 13），guid/maxsize 占位。
TEMPLATE = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: {maxsize}
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 1
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: {maxsize}
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  - serializedVersion: 3
    buildTarget: Standalone
    maxTextureSize: {maxsize}
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

GUID_RE = re.compile(r"guid: ([0-9a-f]{32})")


def maxsize_for(rel: str) -> int:
    """rel 形如 'Board/parts/p001_hammer.png.meta' 或 'UI/panel.png.meta'。"""
    if rel.startswith("UI/"):
        name = Path(rel).name
        if name.startswith(("panel", "dialog")):
            return 512
        return 256  # button_* / tab_*
    return 128  # 盘面棋子


def read_guid(meta: Path):
    m = GUID_RE.search(meta.read_text(encoding="utf-8"))
    return m.group(1) if m else None


def main() -> int:
    check_only = "--check" in sys.argv
    metas = sorted(BASE.rglob("*.png.meta"))
    changed = 0
    skipped = 0
    for meta in metas:
        rel = meta.relative_to(BASE).as_posix()
        guid = read_guid(meta)
        if not guid:
            print(f"[SKIP  no-guid] {rel}")
            skipped += 1
            continue
        ms = maxsize_for(rel)
        if check_only:
            print(f"[WOULD] {ms:4}  {rel}")
            changed += 1
            continue
        meta.write_text(TEMPLATE.format(guid=guid, maxsize=ms), encoding="utf-8", newline="\n")
        print(f"[OK]   {ms:4}  {rel}")
        changed += 1
    print(f"[DONE] {'would-change' if check_only else 'changed'} {changed}, skipped {skipped}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
