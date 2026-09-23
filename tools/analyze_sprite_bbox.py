# -*- coding: utf-8 -*-
"""分析 6 个 sprite 的内容(alpha>0)包围盒，看内容是否接近纹理边缘（导致卡纸外扩被截断）。"""
from PIL import Image
import os

DIR = r'Assets/Game/Sprites/美术风格测试/'
names = ['撞锤', '棱镜', '红舞鞋', '维修卷帘门', '玩家', '沈遥']

for n in names:
    img = Image.open(DIR + n + '.png').convert('RGBA')
    w, h = img.size
    px = img.load()
    minx, miny, maxx, maxy = w, h, -1, -1
    count = 0
    for y in range(h):
        for x in range(w):
            if px[x, y][3] > 0:
                count += 1
                if x < minx: minx = x
                if x > maxx: maxx = x
                if y < miny: miny = y
                if y > maxy: maxy = y
    bw = maxx - minx + 1
    bh = maxy - miny + 1
    # 内容包围盒占纹理的比例 + 边缘留白
    left, top = minx, miny
    right_pad = w - 1 - maxx
    bottom_pad = h - 1 - maxy
    print(f'{n}: {w}x{h} 内容bbox=({minx},{miny})-({maxx},{maxy}) 尺寸{bw}x{bh} '
          f'占{bw*100//w}%x{bh*100//h}% 边缘留白 L{left}/T{top}/R{right_pad}/B{bottom_pad} 内容像素{count}')
