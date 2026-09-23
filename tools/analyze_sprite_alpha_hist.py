# -*- coding: utf-8 -*-
"""分析 sprite PNG 的 alpha 直方图，确认透明区噪点的 alpha 分布范围。"""
from PIL import Image
from collections import Counter
import sys, os

def analyze(path):
    img = Image.open(path)
    img = img.convert("RGBA")
    w, h = img.size
    px = img.load()
    hist = Counter()
    for y in range(h):
        for x in range(w):
            a = px[x, y][3]
            hist[a] += 1
    total = w * h
    print(f"=== {os.path.basename(path)} ({w}x{h}, total={total}) ===")
    print(f"  alpha=0 (透明):   {hist[0]}")
    print(f"  alpha=255 (不透明): {hist[255]}")
    semitrans = total - hist[0] - hist[255]
    print(f"  alpha=1..254 (半透明): {semitrans}")
    # 低 alpha 直方图（1..60），这是噪点嫌疑区
    low = sum(hist[a] for a in range(1, 61))
    mid = sum(hist[a] for a in range(61, 200))
    high = sum(hist[a] for a in range(200, 255))
    print(f"  alpha 1..60  (噪点嫌疑): {low}")
    print(f"  alpha 61..199 (抗锯齿):  {mid}")
    print(f"  alpha 200..254 (近实心): {high}")
    # 打印 1..60 的分布（每 5 一档）
    bins = {}
    for a in range(1, 61):
        b = (a - 1) // 5 * 5 + 1
        bins[b] = bins.get(b, 0) + hist[a]
    print("  低 alpha 分布 (区间: 像素数):")
    for b in sorted(bins):
        print(f"    {b}..{b+4}: {bins[b]}")

for p in sys.argv[1:]:
    analyze(p)
    print()
