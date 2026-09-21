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
        ("p014_pliers", "pliers", "#B088D8", "#7C54A6", "#483066"),
    ]


def others():
    return [
        ("obstacle_wall", "wall", "#7A7E88", "#464A54", "#282B32"),
        ("anomaly", "spiral", "#C088D0", "#884E9E", "#522E5E"),
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

def build_all():
    os.makedirs(os.path.join(OUT, "Board", "parts"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "Board", "others"), exist_ok=True)
    os.makedirs(os.path.join(OUT, "UI"), exist_ok=True)

    generated = []

    for name, icon, top, bottom, outline in parts():
        img = hex_piece(TILE, hex2rgb(top), hex2rgb(bottom), hex2rgb(outline), icon)
        p = os.path.join(OUT, "Board", "parts", name + ".png")
        img.save(p)
        generated.append((p, img))

    for name, icon, top, bottom, outline, *rest in others():
        dark = bool(rest and rest[0])
        img = hex_piece(TILE, hex2rgb(top), hex2rgb(bottom), hex2rgb(outline), icon, icon_dark=dark)
        p = os.path.join(OUT, "Board", "others", name + ".png")
        img.save(p)
        generated.append((p, img))

    for name, size, radius, top, bottom, outline in ui_items():
        img = ui_rounded(size, radius, hex2rgb(top), hex2rgb(bottom), hex2rgb(outline))
        p = os.path.join(OUT, "UI", name + ".png")
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
        for name, icon, top, bottom, outline in parts():
            generated.append((name, hex_piece(TILE, hex2rgb(top), hex2rgb(bottom), hex2rgb(outline), icon)))
        for name, icon, top, bottom, outline, *rest in others():
            dark = bool(rest and rest[0])
            generated.append((name, hex_piece(TILE, hex2rgb(top), hex2rgb(bottom), hex2rgb(outline), icon, dark)))
        for name, size, radius, top, bottom, outline in ui_items():
            generated.append((name, ui_rounded(size, radius, hex2rgb(top), hex2rgb(bottom), hex2rgb(outline))))
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
