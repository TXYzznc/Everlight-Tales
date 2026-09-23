# -*- coding: utf-8 -*-
"""分析美术风格测试 PNG 的边缘/透明像素，定位彩色泄漏根源。"""
import os
from PIL import Image

DIR = r"Assets/Game/Sprites/美术风格测试"
names = ["撞锤", "棱镜", "红舞鞋", "维修卷帘门", "玩家", "沈遥"]

for n in names:
    p = os.path.join(DIR, n + ".png")
    if not os.path.exists(p):
        print(f"[缺] {n}")
        continue
    img = Image.open(p).convert("RGBA")
    w, h = img.size
    px = img.load()

    total = w * h
    transparent = 0        # alpha == 0
    opaque = 0             # alpha == 255
    semitrans = 0          # 0 < alpha < 255
    transparent_colored = 0  # alpha==0 但 RGB 非 0（保留颜色）
    semitrans_rgb_nonzero = 0  # 半透明且 RGB 明显非 0
    semitrans_samples = []   # 半透明像素的 (r,g,b,a) 样本

    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                transparent += 1
                if (r, g, b) != (0, 0, 0):
                    transparent_colored += 1
            elif a == 255:
                opaque += 1
            else:
                semitrans += 1
                if (r, g, b) != (0, 0, 0):
                    semitrans_rgb_nonzero += 1
                    if len(semitrans_samples) < 6:
                        semitrans_samples.append((r, g, b, a))

    print(f"== {n}  {w}x{h} ==")
    print(f"  透明(alpha=0): {transparent}  其中RGB非0(保留颜色): {transparent_colored}")
    print(f"  不透明(alpha=255): {opaque}")
    print(f"  半透明(0<a<255): {semitrans}  其中RGB非0: {semitrans_rgb_nonzero}")
    if semitrans_samples:
        print(f"  半透明样本(r,g,b,a): {semitrans_samples}")
    print()
