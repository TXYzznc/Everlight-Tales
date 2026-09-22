#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
程序化占位美术资源生成器（任务 ART-000）

用 Pillow 程序化生成带「渐变 / 描边 / 高光 / 纹理」的占位 PNG，
导入 Assets/Art/Placeholder/ 使用；正式美术到位后逐项替换。

用法：
    python tools/generate_placeholder_art.py            # 生成样张（默认）
    python tools/generate_placeholder_art.py --preview   # 仅生成拼图预览不写资源

输出：
    Assets/Game/Sprites/Placeholder/Board/parts/*.png
    Assets/Game/Sprites/Placeholder/Board/others/*.png
    Assets/Game/Sprites/Placeholder/UI/*.png
"""
import math
import os
import sys

from PIL import Image, ImageChops, ImageDraw, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "Assets", "Game", "Sprites", "Placeholder")

TILE = 256          # 盘面棋子图尺寸
ICON_SCALE = 0.36   # 图标相对棋子尺寸

# ---------------------------------------------------------------- 基础工具

def hex2rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


def lerp(c1, c2, t):
    return tuple(int(round(a + (b - a) * t)) for a, b in zip(c1, c2))


def vgrad(c_top, c_bottom, w, h):
    """垂直渐变 RGB 图。"""
    img = Image.new("RGB", (w, h))
    px = img.load()
    denom = max(1, h - 1)
    for y in range(h):
        c = lerp(c_top, c_bottom, y / denom)
        for x in range(w):
            px[x, y] = c
    return img


def hex_vertices(cx, cy, r):
    """点顶六边形顶点（屏幕坐标，y 向下），自顶部顺时针。"""
    return [(cx + r * math.cos(math.radians(90 + 60 * i)),
             cy - r * math.sin(math.radians(90 + 60 * i))) for i in range(6)]


def shape_gradient(size, mask, c_top, c_bottom):
    """把垂直渐变裁剪到 shape mask 内，其余透明。"""
    grad = vgrad(c_top, c_bottom, *size).convert("RGBA")
    out = Image.new("RGBA", size, (0, 0, 0, 0))
    out.paste(grad, (0, 0), mask)
    return out


def add_gloss(img, mask, alpha=64):
    """顶部柔光高光（提升立体感）。"""
    w, h = img.size
    gloss = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(gloss)
    d.ellipse([w * 0.08, h * 0.02, w * 0.92, h * 0.52], fill=(255, 255, 255, alpha))
    gloss = gloss.filter(ImageFilter.GaussianBlur(w * 0.14))
    combined = ImageChops.multiply(gloss.split()[3], mask)
    gloss.putalpha(combined)
    img.alpha_composite(gloss)


def add_hatch(img, mask, color=(0, 0, 0), alpha=12, step=22):
    """斜纹纹理（低透明度，营造材质感）。"""
    w, h = img.size
    hatch = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(hatch)
    x0 = -h
    while x0 < w:
        d.line([x0, h, x0 + h, 0], fill=color + (alpha,), width=2)
        x0 += step
    hatch.putalpha(ImageChops.multiply(hatch.split()[3], mask))
    img.alpha_composite(hatch)


def outline_poly(draw, verts, color, width):
    draw.polygon(verts, outline=color, width=width)


# ---------------------------------------------------------------- 图标

def _stroke(draw, pts, color, width):
    # 先画深色阴影描边，再画亮色主体，增强对比
    draw.line(pts, fill=(0, 0, 0, 120), width=width + 3)
    draw.line(pts, fill=color, width=width)


def icon_hammer(d, cx, cy, s):
    c = (250, 250, 252)
    # 锤头（横条）
    d.rounded_rectangle([cx - s * 0.9, cy - s * 0.55, cx + s * 0.9, cy - s * 0.1],
                        radius=s * 0.18, fill=c, outline=(0, 0, 0, 120), width=2)
    # 锤柄（斜向下）
    _stroke(d, [cx + s * 0.45, cy - s * 0.3, cx - s * 0.45, cy + s * 0.95], c, max(3, int(s * 0.16)))


def _gear(d, cx, cy, s, c):
    teeth = 8
    r_out, r_in = s, s * 0.62
    for i in range(teeth):
        a = math.radians(i * 360 / teeth)
        x1, y1 = cx + math.cos(a) * r_in, cy + math.sin(a) * r_in
        x2, y2 = cx + math.cos(a) * r_out * 1.12, cy + math.sin(a) * r_out * 1.12
        d.line([x1, y1, x2, y2], fill=c, width=max(3, int(s * 0.16)))
    d.ellipse([cx - r_out, cy - r_out, cx + r_out, cy + r_out], outline=(0, 0, 0, 120), width=2)
    d.ellipse([cx - r_in, cy - r_in, cx + r_in, cy + r_in], fill=c)
    d.ellipse([cx - s * 0.22, cy - s * 0.22, cx + s * 0.22, cy + s * 0.22], fill=(20, 20, 26))


def icon_gear(d, cx, cy, s):
    _gear(d, cx, cy, s, (250, 250, 252))


def icon_ratchet(d, cx, cy, s):
    _gear(d, cx, cy, s, (230, 248, 250))
    # 棘爪（右下小斜杠）
    _stroke(d, [cx + s * 0.5, cy + s * 0.5, cx + s * 0.95, cy + s * 0.95], (230, 248, 250), max(2, int(s * 0.1)))


def icon_coil(d, cx, cy, s):
    c = (250, 250, 252)
    for rr in (s * 0.75, s * 0.45, s * 0.16):
        d.ellipse([cx - rr, cy - rr, cx + rr, cy + rr], outline=c, width=max(3, int(s * 0.14)))


def icon_pliers(d, cx, cy, s):
    c = (250, 250, 252)
    _stroke(d, [cx - s * 0.6, cy - s * 0.85, cx + s * 0.6, cy + s * 0.85], c, max(3, int(s * 0.13)))
    _stroke(d, [cx + s * 0.6, cy - s * 0.85, cx - s * 0.6, cy + s * 0.85], c, max(3, int(s * 0.13)))
    d.ellipse([cx - s * 0.16, cy - s * 0.16, cx + s * 0.16, cy + s * 0.16], fill=(20, 20, 26))
    # 钳口（顶部两短竖）
    d.rectangle([cx - s * 0.34, cy - s * 0.95, cx - s * 0.14, cy - s * 0.55], fill=c, outline=(0, 0, 0, 120), width=1)
    d.rectangle([cx + s * 0.14, cy - s * 0.95, cx + s * 0.34, cy - s * 0.55], fill=c, outline=(0, 0, 0, 120), width=1)


def icon_spring(d, cx, cy, s):
    c = (250, 250, 252)
    pts = []
    for i in range(8):
        x = cx - s * 0.7 + i * s * 0.2
        y = cy + (s * 0.55 if i % 2 == 0 else -s * 0.55)
        pts.append((x, y))
    _stroke(d, pts, c, max(3, int(s * 0.11)))


def icon_wall(d, cx, cy, s):
    c = (235, 237, 240)
    bw, bh = s * 0.5, s * 0.32
    x0, y0 = cx - s * 0.75, cy - s * 0.75
    for row in range(3):
        for col in range(3):
            ox = (bw * 0.5) if row % 2 == 1 else 0
            x = x0 + col * bw * 1.05 + ox
            y = y0 + row * bh * 1.05
            d.rectangle([x, y, x + bw, y + bh], outline=(0, 0, 0, 130), width=2)
            d.line([x, y + bh * 0.5, x + bw, y + bh * 0.5], fill=(0, 0, 0, 130), width=1)


def icon_spiral(d, cx, cy, s):
    c = (250, 250, 252)
    segs = [(0, 0, 90), (0, 90, 180), (180, 270, 180), (180, 360, 90), (0, 90, 180)]
    r0 = s * 0.15
    for k, (start, end, rr) in enumerate(segs):
        rad = r0 * (k + 1) * 1.5
        d.arc([cx - rad, cy - rad, cx + rad, cy + rad], start, end, fill=c, width=max(3, int(s * 0.11)))


def icon_flag(d, cx, cy, s):
    c = (250, 248, 240)
    _stroke(d, [cx, cy + s * 0.8, cx, cy - s * 0.9], (90, 70, 30), max(3, int(s * 0.1)))
    d.polygon([(cx, cy - s * 0.9), (cx + s * 0.9, cy - s * 0.55), (cx, cy - s * 0.2)], fill=c, outline=(0, 0, 0, 120), width=2)


def icon_wrench(d, cx, cy, s):
    c = (250, 250, 252)
    _stroke(d, [cx - s * 0.3, cy - s * 0.9, cx + s * 0.35, cy + s * 0.85], c, max(3, int(s * 0.14)))
    # 开口端（C 形头）
    d.arc([cx - s * 0.7, cy - s * 1.1, cx + s * 0.1, cy - s * 0.3], 200, 360, fill=c, width=max(3, int(s * 0.16)))
    d.arc([cx - s * 0.7, cy - s * 1.1, cx + s * 0.1, cy - s * 0.3], 200, 360, fill=(0, 0, 0, 120), width=1)


def icon_mold(d, cx, cy, s):
    c = (250, 250, 252)
    d.rectangle([cx - s * 0.85, cy - s * 0.6, cx - s * 0.15, cy + s * 0.6], outline=c, width=3)
    d.rectangle([cx + s * 0.15, cy - s * 0.6, cx + s * 0.85, cy + s * 0.6], outline=c, width=3)


def icon_stomach(d, cx, cy, s):
    c = (250, 250, 252)
    d.ellipse([cx - s * 0.7, cy - s * 0.5, cx + s * 0.7, cy + s * 0.7], outline=c, width=3)
    d.arc([cx - s * 0.3, cy - s * 0.55, cx + s * 0.3, cy + s * 0.1], 180, 360, fill=c, width=3)


def icon_furnace(d, cx, cy, s):
    c = (250, 250, 252)
    d.rectangle([cx - s * 0.6, cy - s * 0.2, cx + s * 0.6, cy + s * 0.7], outline=c, width=3)
    d.polygon([(cx - s * 0.4, cy - s * 0.2), (cx, cy - s * 0.8), (cx + s * 0.4, cy - s * 0.2)], outline=c, width=2)


def icon_fork(d, cx, cy, s):
    c = (250, 250, 252)
    _stroke(d, [cx, cy + s * 0.8, cx, cy - s * 0.2], c, 3)
    _stroke(d, [cx, cy - s * 0.2, cx - s * 0.6, cy - s * 0.8], c, 3)
    _stroke(d, [cx, cy - s * 0.2, cx + s * 0.6, cy - s * 0.8], c, 3)


def icon_rotor(d, cx, cy, s):
    c = (250, 250, 252)
    for i in range(3):
        a = math.radians(i * 120 - 90)
        x = cx + math.cos(a) * s * 0.7
        y = cy + math.sin(a) * s * 0.7
        _stroke(d, [cx, cy, x, y], c, 4)
    d.ellipse([cx - s * 0.15, cy - s * 0.15, cx + s * 0.15, cy + s * 0.15], fill=c)


def icon_flywheel(d, cx, cy, s):
    c = (250, 250, 252)
    d.ellipse([cx - s * 0.7, cy - s * 0.7, cx + s * 0.7, cy + s * 0.7], outline=c, width=3)
    d.ellipse([cx - s * 0.25, cy - s * 0.25, cx + s * 0.25, cy + s * 0.25], fill=c)
    for i in range(6):
        a = math.radians(i * 60)
        x1, y1 = cx + math.cos(a) * s * 0.25, cy + math.sin(a) * s * 0.25
        x2, y2 = cx + math.cos(a) * s * 0.7, cy + math.sin(a) * s * 0.7
        d.line([x1, y1, x2, y2], fill=c, width=2)


def icon_bridge(d, cx, cy, s):
    c = (250, 250, 252)
    d.arc([cx - s * 0.8, cy - s * 0.7, cx + s * 0.8, cy + s * 0.7], 0, 180, fill=c, width=3)
    d.line([cx - s * 0.7, cy + s * 0.5, cx + s * 0.7, cy + s * 0.5], fill=c, width=2)


def icon_prism(d, cx, cy, s):
    c = (250, 250, 252)
    d.polygon([(cx, cy - s * 0.75), (cx - s * 0.7, cy + s * 0.6), (cx + s * 0.7, cy + s * 0.6)], outline=c, width=3)


def icon_tuningfork(d, cx, cy, s):
    c = (250, 250, 252)
    _stroke(d, [cx, cy + s * 0.75, cx, cy - s * 0.4], c, 4)
    d.arc([cx - s * 0.5, cy - s * 0.9, cx + s * 0.5, cy + s * 0.1], 0, 180, fill=c, width=3)


def icon_impeller(d, cx, cy, s):
    c = (250, 250, 252)
    for i in range(4):
        a = math.radians(i * 90 - 90)
        x = cx + math.cos(a) * s * 0.7
        y = cy + math.sin(a) * s * 0.7
        _stroke(d, [cx, cy, x, y], c, 3)
    d.ellipse([cx - s * 0.2, cy - s * 0.2, cx + s * 0.2, cy + s * 0.2], fill=c)


def icon_bladder(d, cx, cy, s):
    c = (250, 250, 252)
    d.ellipse([cx - s * 0.6, cy - s * 0.6, cx + s * 0.6, cy + s * 0.6], outline=c, width=3)
    _stroke(d, [cx, cy - s * 0.6, cx, cy - s * 0.9], c, 3)


def icon_probe(d, cx, cy, s):
    c = (250, 250, 252)
    _stroke(d, [cx, cy + s * 0.8, cx, cy - s * 0.8], c, 4)
    for i in range(3):
        y = cy - s * 0.4 + i * s * 0.3
        d.line([cx, y, cx + s * 0.4, y], fill=c, width=2)
    d.polygon([(cx, cy - s * 0.8), (cx - s * 0.15, cy - s * 0.5), (cx + s * 0.15, cy - s * 0.5)], fill=c)


def icon_magnet(d, cx, cy, s):
    c = (250, 250, 252)
    d.arc([cx - s * 0.6, cy - s * 0.7, cx + s * 0.6, cy + s * 0.3], 180, 360, fill=c, width=4)
    d.rectangle([cx - s * 0.6, cy - s * 0.2, cx - s * 0.3, cy + s * 0.6], fill=c)
    d.rectangle([cx + s * 0.3, cy - s * 0.2, cx + s * 0.6, cy + s * 0.6], fill=c)


def icon_battery(d, cx, cy, s):
    c = (250, 250, 252)
    d.rectangle([cx - s * 0.6, cy - s * 0.6, cx + s * 0.6, cy + s * 0.6], outline=c, width=3)
    d.rectangle([cx - s * 0.3, cy - s * 0.6, cx + s * 0.3, cy + s * 0.6], outline=c, width=2)
    d.rectangle([cx - s * 0.2, cy - s * 0.85, cx + s * 0.2, cy - s * 0.6], fill=c)


def icon_partition(d, cx, cy, s):
    c = (250, 250, 252)
    d.rectangle([cx - s * 0.7, cy - s * 0.6, cx + s * 0.7, cy + s * 0.6], outline=c, width=3)
    d.line([cx - s * 0.3, cy - s * 0.5, cx - s * 0.1, cy, cx - s * 0.3, cy + s * 0.5], fill=c, width=2)


def icon_gate(d, cx, cy, s):
    c = (250, 250, 252)
    d.rectangle([cx - s * 0.6, cy - s * 0.7, cx + s * 0.6, cy + s * 0.7], outline=c, width=3)
    d.line([cx, cy - s * 0.7, cx, cy + s * 0.7], fill=c, width=2)


def icon_rail(d, cx, cy, s):
    c = (250, 250, 252)
    _stroke(d, [cx - s * 0.8, cy, cx + s * 0.5, cy], c, 3)
    d.polygon([(cx + s * 0.5, cy - s * 0.35), (cx + s * 0.85, cy), (cx + s * 0.5, cy + s * 0.35)], fill=c)


def icon_seat(d, cx, cy, s):
    c = (250, 250, 252)
    d.arc([cx - s * 0.6, cy - s * 0.7, cx + s * 0.6, cy + s * 0.7], 0, 180, fill=c, width=4)
    d.line([cx - s * 0.6, cy, cx + s * 0.6, cy], fill=c, width=2)


def icon_shutter(d, cx, cy, s):
    c = (250, 250, 252)
    for i in range(4):
        y = cy - s * 0.6 + i * s * 0.4
        d.line([cx - s * 0.7, y, cx + s * 0.7, y], fill=c, width=2)


def icon_seal(d, cx, cy, s):
    c = (250, 250, 252)
    _stroke(d, [cx - s * 0.6, cy - s * 0.6, cx + s * 0.6, cy + s * 0.6], c, 4)
    _stroke(d, [cx - s * 0.6, cy + s * 0.6, cx + s * 0.6, cy - s * 0.6], c, 4)


def icon_pillar(d, cx, cy, s):
    c = (250, 250, 252)
    d.rectangle([cx - s * 0.3, cy - s * 0.6, cx + s * 0.3, cy + s * 0.4], outline=c, width=3)
    d.line([cx - s * 0.5, cy + s * 0.6, cx + s * 0.5, cy + s * 0.6], fill=c, width=3)


def icon_deflection(d, cx, cy, s):
    c = (250, 250, 252)
    _stroke(d, [cx - s * 0.6, cy + s * 0.5, cx + s * 0.2, cy - s * 0.2], c, 3)
    d.polygon([(cx + s * 0.2, cy - s * 0.6), (cx + s * 0.5, cy - s * 0.2), (cx + s * 0.1, cy - s * 0.05)], fill=c)


def icon_rift(d, cx, cy, s):
    c = (250, 250, 252)
    _stroke(d, [cx - s * 0.5, cy - s * 0.6, cx, cy, cx - s * 0.4, cy + s * 0.6], c, 3)


def icon_reserved(d, cx, cy, s):
    c = (250, 250, 252)
    d.arc([cx - s * 0.4, cy - s * 0.7, cx + s * 0.4, cy + s * 0.1], 0, 180, fill=c, width=3)
    _stroke(d, [cx, cy + s * 0.15, cx, cy + s * 0.4], c, 3)
    d.ellipse([cx - s * 0.07, cy + s * 0.55, cx + s * 0.07, cy + s * 0.7], fill=c)


def icon_silence(d, cx, cy, s):
    c = (250, 250, 252)
    d.ellipse([cx - s * 0.6, cy - s * 0.6, cx + s * 0.6, cy + s * 0.6], outline=c, width=3)
    _stroke(d, [cx - s * 0.45, cy - s * 0.45, cx + s * 0.45, cy + s * 0.45], c, 3)


def icon_settlerail(d, cx, cy, s):
    c = (250, 250, 252)
    _stroke(d, [cx, cy - s * 0.7, cx, cy + s * 0.3], c, 4)
    d.polygon([(cx, cy + s * 0.7), (cx - s * 0.4, cy + s * 0.15), (cx + s * 0.4, cy + s * 0.15)], fill=c)


def icon_overload(d, cx, cy, s):
    c = (250, 250, 252)
    for rr in (s * 0.7, s * 0.45, s * 0.2):
        d.ellipse([cx - rr, cy - rr, cx + rr, cy + rr], outline=c, width=3)


def icon_posttap(d, cx, cy, s):
    c = (250, 250, 252)
    _stroke(d, [cx - s * 0.7, cy - s * 0.3, cx + s * 0.7, cy - s * 0.3], c, 3)
    _stroke(d, [cx - s * 0.7, cy + s * 0.3, cx + s * 0.7, cy + s * 0.3], c, 3)
    d.polygon([(cx - s * 0.7, cy - s * 0.3), (cx - s * 0.3, cy - s * 0.55), (cx - s * 0.3, cy - s * 0.05)], fill=c)
    d.polygon([(cx + s * 0.7, cy + s * 0.3), (cx + s * 0.3, cy + s * 0.55), (cx + s * 0.3, cy + s * 0.05)], fill=c)


ICONS = {
    "hammer": icon_hammer,
    "gear": icon_gear,
    "ratchet": icon_ratchet,
    "coil": icon_coil,
    "pliers": icon_pliers,
    "spring": icon_spring,
    "wall": icon_wall,
    "spiral": icon_spiral,
    "flag": icon_flag,
    "wrench": icon_wrench,
    "mold": icon_mold,
    "stomach": icon_stomach,
    "furnace": icon_furnace,
    "fork": icon_fork,
    "rotor": icon_rotor,
    "flywheel": icon_flywheel,
    "bridge": icon_bridge,
    "prism": icon_prism,
    "tuningfork": icon_tuningfork,
    "impeller": icon_impeller,
    "bladder": icon_bladder,
    "probe": icon_probe,
    "magnet": icon_magnet,
    "battery": icon_battery,
    "partition": icon_partition,
    "gate": icon_gate,
    "rail": icon_rail,
    "seat": icon_seat,
    "shutter": icon_shutter,
    "seal": icon_seal,
    "pillar": icon_pillar,
    "deflection": icon_deflection,
    "rift": icon_rift,
    "reserved": icon_reserved,
    "silence": icon_silence,
    "settlerail": icon_settlerail,
    "overload": icon_overload,
    "posttap": icon_posttap,
}


# ---------------------------------------------------------------- 组装

def hex_piece(size, c_top, c_bottom, outline, icon, icon_dark=False):
    """盘面棋子：六边形底 + 渐变 + 描边 + 高光 + 纹理 + 图标。"""
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    margin = 14
    r = (size - 2 * margin) / 2
    cx = cy = size / 2
    verts = hex_vertices(cx, cy, r)

    mask = Image.new("L", (size, size), 0)
    ImageDraw.Draw(mask).polygon(verts, fill=255)

    img = shape_gradient((size, size), mask, c_top, c_bottom)
    d = ImageDraw.Draw(img)
    outline_poly(d, verts, outline, 3)
    add_gloss(img, mask, 56)
    add_hatch(img, mask)

    if icon:
        icol = (30, 30, 36) if icon_dark else (252, 253, 255)
        ICONS[icon](d, cx, cy, size * ICON_SCALE)
    return img


def ui_rounded(size, radius, c_top, c_bottom, outline, border=2):
    """圆角矩形 UI 底（按钮/面板/页签共用）。"""
    w, h = size
    img = Image.new("RGBA", size, (0, 0, 0, 0))
    mask = Image.new("L", size, 0)
    ImageDraw.Draw(mask).rounded_rectangle([0, 0, w - 1, h - 1], radius=radius, fill=255)
    img = shape_gradient(size, mask, c_top, c_bottom)
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([border // 2, border // 2, w - 1 - border // 2, h - 1 - border // 2],
                        radius=radius, outline=outline, width=border)
    add_gloss(img, mask, 40)
    add_hatch(img, mask, alpha=8)
    return img


# ---------------------------------------------------------------- 内容清单

def parts():
    return [
        ("p001_hammer", "hammer", "#F2A968", "#C87A38", "#7A4418"),
        ("p002_ratchet", "ratchet", "#82C6DA", "#3E8FA6", "#1F5464"),
        ("p003_coil", "coil", "#E28888", "#B04646", "#6E2626"),
        ("p004_gear", "gear", "#86C48A", "#4C9852", "#285C2C"),
        ("p005_spring", "spring", "#74C8B2", "#3C9078", "#1E5848"),
        ("p006_mold", "mold", "#C8B08A", "#9A7A50", "#5C4626"),
        ("p007_stomach", "stomach", "#E8B8C8", "#B07090", "#6E3454"),
        ("p008_furnace", "furnace", "#E0A060", "#B06030", "#6E3020"),
        ("p009_fork", "fork", "#A8C8E8", "#6080B0", "#30486E"),
        ("p010_rotor", "rotor", "#90D0C8", "#489088", "#205854"),
        ("p011_flywheel", "flywheel", "#C0A0E0", "#8050B0", "#482860"),
        ("p012_bridge", "bridge", "#D0C088", "#988048", "#585026"),
        ("p013_prism", "prism", "#88D8E8", "#4890B0", "#205464"),
        ("p014_pliers", "pliers", "#B088D8", "#7C54A6", "#483066"),
        ("p015_tuningfork", "tuningfork", "#E8D088", "#B09848", "#6E5826"),
        ("p016_impeller", "impeller", "#88C8B8", "#489080", "#205850"),
        ("p017_bladder", "bladder", "#E8A0C0", "#B05880", "#6E2848"),
        ("p018_probe", "probe", "#B8C8E8", "#7080B0", "#384868"),
        ("p019_magnet", "magnet", "#D89088", "#A05048", "#682820"),
        ("p020_battery", "battery", "#A8E0A0", "#58A858", "#286028"),
    ]


def forms():
    # 15 个怪谈形态：统一紫色系 + 宿主零件图标（同宿主形态暂共享图标，正式美术替换）。
    purple = [
        ("m001_escort", "magnet", "#C8A0E8", "#8850B0", "#502868"),
        ("m002_sleeve", "probe", "#C8A0E8", "#8850B0", "#502868"),
        ("m003_mirror", "prism", "#C8A0E8", "#8850B0", "#502868"),
        ("m004_dream", "stomach", "#C8A0E8", "#8850B0", "#502868"),
        ("m005_pump", "impeller", "#C8A0E8", "#8850B0", "#502868"),
        ("m006_frame", "bladder", "#C8A0E8", "#8850B0", "#502868"),
        ("m007_whistle", "tuningfork", "#C8A0E8", "#8850B0", "#502868"),
        ("m008_shadow", "hammer", "#C8A0E8", "#8850B0", "#502868"),
        ("m009_lure", "tuningfork", "#C8A0E8", "#8850B0", "#502868"),
        ("m010_echo", "tuningfork", "#C8A0E8", "#8850B0", "#502868"),
        ("m011_beacon", "probe", "#C8A0E8", "#8850B0", "#502868"),
        ("m012_redirect", "gear", "#C8A0E8", "#8850B0", "#502868"),
        ("m013_identity", "prism", "#C8A0E8", "#8850B0", "#502868"),
        ("m014_clamp", "pliers", "#C8A0E8", "#8850B0", "#502868"),
        ("m015_reuse", "tuningfork", "#C8A0E8", "#8850B0", "#502868"),
    ]
    return purple


def obstacles():
    return [
        ("o001_wall", "wall", "#7A7E88", "#464A54", "#282B32"),
        ("o002_partition", "partition", "#9A8E7A", "#605848", "#38302A"),
        ("o003_gate", "gate", "#8894A4", "#525E6C", "#303A46"),
        ("o004_rail", "rail", "#94A8A0", "#5C7068", "#34423C"),
        ("o005_seat", "seat", "#A8A4B0", "#6C6878", "#3C3A44"),
        ("o006_shutter", "shutter", "#8E9AA6", "#56626E", "#303842"),
        ("o007_seal", "seal", "#C09860", "#886040", "#503824"),
        ("o008_pillar", "pillar", "#9E94A8", "#665C74", "#3A3444"),
    ]


def anomalies():
    return [
        ("a001_deflection", "deflection", "#C088D0", "#884E9E", "#522E5E"),
        ("a002_rift", "rift", "#B890C8", "#805090", "#4A2E5E"),
        ("a003_reserved", "reserved", "#C0A0C8", "#886090", "#503856"),
        ("a004_silence", "silence", "#C8A0B8", "#906078", "#563846"),
        ("a005_settlerail", "settlerail", "#A890C8", "#705098", "#402E5E"),
        ("a006_overload", "overload", "#C898D0", "#9058A0", "#563260"),
        ("a007_reserved", "reserved", "#C0A0C8", "#886090", "#503856"),
        ("a008_posttap", "posttap", "#C088C0", "#905090", "#563058"),
    ]


def others():
    return [
        ("marker_task", "flag", "#E4C464", "#B8942E", "#6E581A", True),
        ("marker_repair", "wrench", "#9A86CC", "#6652A0", "#3C305E"),
    ]


def ui_items():
    return [
        ("button_primary", (320, 96), 22, "#64A4EA", "#2E68B4", "#163E70"),
        ("button_secondary", (320, 96), 22, "#9AA6B6", "#5E6A7A", "#38424E"),
        ("panel", (512, 512), 28, "#404A5C", "#262E3C", "#141A24"),
        ("dialog", (512, 360), 28, "#4C566A", "#2E3646", "#1A202A"),
        ("tab_active", (256, 96), 18, "#58A0E0", "#2A6AB0", "#163E6C"),
        ("tab_normal", (256, 96), 18, "#8894A4", "#525E6C", "#303A46"),
    ]


# ---------------------------------------------------------------- 生成

def collect():
    """收集全部占位资源（相对目录 + 名称, 图片），不写盘。"""
    result = []

    def add(directory, rows):
        for name, icon, top, bottom, outline, *rest in rows:
            dark = bool(rest and rest[0])
            img = hex_piece(TILE, hex2rgb(top), hex2rgb(bottom), hex2rgb(outline), icon, icon_dark=dark)
            result.append((os.path.join(directory, name + ".png"), img))

    add("parts", parts())
    add("forms", forms())
    add("obstacles", obstacles())
    add("anomalies", anomalies())
    add("others", others())
    for name, size, radius, top, bottom, outline in ui_items():
        img = ui_rounded(size, radius, hex2rgb(top), hex2rgb(bottom), hex2rgb(outline))
        result.append((os.path.join("UI", name + ".png"), img))
    return result


def build_all():
    os.makedirs(os.path.join(OUT, "Board", "parts"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "Board", "forms"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "Board", "obstacles"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "Board", "anomalies"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "Board", "others"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "UI"), exist_ok=True)

    # 清理旧命名占位（obstacle_wall/anomaly 已拆分到 obstacles/anomalies 目录）。
    for legacy in ("obstacle_wall", "anomaly"):
        lp = os.path.join(OUT, "Board", "others", legacy + ".png")
        if os.path.exists(lp):
            os.remove(lp)

    generated = []
    for rel, img in collect():
        if rel.startswith("UI" + os.sep):
            p = os.path.join(OUT, rel)
        else:
            p = os.path.join(OUT, "Board", rel)
        img.save(p)
        generated.append((p, img))
    return generated


def make_contact_sheet(generated, out_path):
    cols = 6
    cell = 300
    label_h = 30
    rows = math.ceil(len(generated) / cols)
    sheet = Image.new("RGBA", (cols * cell, rows * cell + label_h), (24, 27, 34, 255))
    d = ImageDraw.Draw(sheet)
    for i, (p, img) in enumerate(generated):
        r, c = divmod(i, cols)
        x, y = c * cell, r * cell + label_h
        im = img.convert("RGBA")
        im.thumbnail((cell - 40, cell - 40))
        sheet.alpha_composite(im, (x + (cell - im.width) // 2, y + (cell - im.height) // 2))
        d.text((x + 8, y + cell - 22), os.path.basename(p), fill=(220, 224, 232, 255))
    sheet.convert("RGB").save(out_path)
    return out_path


def main():
    preview_only = "--preview" in sys.argv
    if preview_only:
        # 仅预览：生成到内存，不出资源
        generated = []
        for rel, img in collect():
            generated.append((rel, img))
        out = os.path.join(ROOT, "tools", "_art_preview.png")
        make_contact_sheet(generated, out)
        print("preview ->", out)
        return

    generated = build_all()
    out = os.path.join(ROOT, "tools", "_art_preview.png")
    make_contact_sheet(generated, out)
    print(f"generated {len(generated)} assets -> {OUT}")
    print("preview ->", out)


if __name__ == "__main__":
    main()
