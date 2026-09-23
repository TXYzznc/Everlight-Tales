# -*- coding: utf-8 -*-
"""对比两张截图的暖白噪点：检测 v5 的孤立暖白噪点，看 v8 对应位置是否变回背景。"""
from PIL import Image
import sys

def is_card_noise(r, g, b):
    return (r > g > b) and (3 <= r - g <= 18) and (5 <= g - b <= 30) and 80 <= r <= 205

def is_bg(r, g, b):
    # 背景：中性灰（预览场景背景）
    return abs(r-g) <= 4 and abs(g-b) <= 4 and 60 <= r <= 90

def collect_noise(path):
    img = Image.open(path).convert("RGB")
    w, h = img.size
    px = img.load()
    # 找孤立噪点：暖白 + 周围 5px 大部分是背景
    pts = []
    for y in range(0, h, 1):
        for x in range(0, w, 1):
            r, g, b = px[x, y]
            if not is_card_noise(r, g, b):
                continue
            # 7x7 邻域统计
            bg_n = warm_n = 0
            for dy in range(-3, 4):
                for dx in range(-3, 4):
                    nx, ny = x+dx, y+dy
                    if 0 <= nx < w and 0 <= ny < h:
                        nr, ng, nb = px[nx, ny]
                        if is_bg(nr, ng, nb): bg_n += 1
                        elif is_card_noise(nr, ng, nb): warm_n += 1
            if bg_n >= 30 and warm_n <= 8:  # 周围大部分背景，暖白极少 → 孤立噪点
                pts.append((x, y))
    return pts

if len(sys.argv) < 3:
    print("usage: diff_noise.py <before> <after>")
    sys.exit(1)

before = collect_noise(sys.argv[1])
print(f"before({sys.argv[1]}): 孤立暖白噪点 {len(before)} 个")

after_img = Image.open(sys.argv[2]).convert("RGB")
apx = after_img.load()
removed = still = 0
samples = []
for (x, y) in before:
    r, g, b = apx[x, y]
    if is_card_noise(r, g, b):
        still += 1
        if len(samples) < 6:
            samples.append((x, y, (r, g, b)))
    else:
        removed += 1

print(f"after({sys.argv[2]}): 这些噪点中，仍存在 {still} 个，已消除 {removed} 个")
if samples:
    print(f"  仍存在的样本: {samples}")
