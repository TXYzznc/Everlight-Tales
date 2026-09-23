# -*- coding: utf-8 -*-
"""检测『背景中的孤立暖白噪点』：暖白卡纸色 + 周围 20px 内无内容（纯背景）。"""
from PIL import Image
import sys

def is_content(r, g, b):
    mx, mn = max(r, g, b), min(r, g, b)
    return mx > 185 or (mx - mn) > 80

def is_card_noise(r, g, b):
    return (r > g > b) and (3 <= r - g <= 18) and (5 <= g - b <= 30) and 80 <= r <= 205

def analyze(path):
    img = Image.open(path).convert("RGB")
    w, h = img.size
    px = img.load()
    isolated = 0
    samples = []
    for y in range(0, h):
        for x in range(0, w):
            r, g, b = px[x, y]
            if not is_card_noise(r, g, b):
                continue
            near_content = False
            for dy in range(-20, 21, 4):
                for dx in range(-20, 21, 4):
                    nx, ny = x+dx, y+dy
                    if 0 <= nx < w and 0 <= ny < h:
                        if is_content(*px[nx, ny]):
                            near_content = True
                            break
                if near_content:
                    break
            if not near_content:
                isolated += 1
                if len(samples) < 8:
                    samples.append((x, y, (r, g, b)))
    print(f"{path} ({w}x{h})")
    print(f"  背景孤立暖白噪点: {isolated}")
    if samples:
        print(f"  样本(x,y,rgb): {samples}")

for p in sys.argv[1:]:
    analyze(p)
    print()
