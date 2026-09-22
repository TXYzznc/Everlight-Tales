#!/usr/bin/env python3
"""生成 ART0 视觉样张图（色板 / 控件 / 盘面）。

输出到 Docs/GameDesign/11-美术规范/样张/：
- ui_color_palette.png   色板样张
- ui_controls.png        控件样张
- board_sample.png       六相定势盘视觉样张

纯 Pillow，与 UI 视觉规范.md / 盘面视觉规范.md 对应。
"""

from __future__ import annotations

import math
import os
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "Docs" / "GameDesign" / "11-美术规范" / "样张"

# ---- 字体（Windows 内置，找不到则回退默认） ----
def _font(size: int):
    for p in (r"C:\Windows\Fonts\msyh.ttc", r"C:\Windows\Fonts\msyh.ttf",
              r"C:\Windows\Fonts\simhei.ttf", r"C:\Windows\Fonts\simsun.ttc"):
        if os.path.exists(p):
            try:
                return ImageFont.truetype(p, size)
            except Exception:
                continue
    return ImageFont.load_default()


def _mono(size: int):
    for p in (r"C:\Windows\Fonts\consola.ttf", r"C:\Windows\Fonts\consolab.ttf"):
        if os.path.exists(p):
            try:
                return ImageFont.truetype(p, size)
            except Exception:
                continue
    return _font(size)


def hex2rgb(h: str):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


# =====================================================================
# 色板样张
# =====================================================================
PALETTE = [
    ("品牌 / 功能色", [
        ("primary-500", "#E0A858"), ("primary-600", "#C88A3A"), ("primary-700", "#9A6424"),
        ("secondary-500", "#5A9EB8"), ("secondary-600", "#3E7A92"),
        ("accent-500", "#A86AC8"), ("accent-600", "#8650A6"),
    ]),
    ("中性色", [
        ("bg-900", "#171A20"), ("bg-800", "#1E2229"),
        ("surface-700", "#262B34"), ("surface-600", "#2F3540"),
        ("border-500", "#3E4652"),
        ("text-100", "#E8EAF0"), ("text-300", "#A8B0BC"), ("text-500", "#6E7684"),
    ]),
    ("语义色", [
        ("success", "#5AB878"), ("danger", "#D05A5A"), ("warning", "#D8A858"), ("info", "#5A9ED8"),
    ]),
]


def color_palette() -> Image.Image:
    cols = 4
    sw = 150          # 色块宽
    sh = 90           # 色块高
    label_h = 40
    section_h = 44
    rows_per_sec = [math.ceil(len(items) / cols) for _, items in PALETTE]
    height = sum(section_h + r * sh + r * label_h for r in rows_per_sec) + 40
    width = cols * sw + 40

    img = Image.new("RGBA", (width, height), (23, 26, 32, 255))
    d = ImageDraw.Draw(img)
    f_title = _font(22)
    f_label = _font(14)
    f_mono = _mono(13)

    y = 20
    for (title, items), rcount in zip(PALETTE, rows_per_sec):
        d.text((20, y), title, fill=(200, 205, 214, 255), font=f_title)
        y += section_h
        for i, (name, hexv) in enumerate(items):
            col = i % cols
            row = i // cols
            x = 20 + col * sw
            yy = y + row * (sh + label_h)
            rgb = hex2rgb(hexv)
            d.rectangle([x, yy, x + sw - 20, yy + sh - 20], fill=rgb + (255,), outline=(62, 70, 82, 255))
            d.text((x, yy + sh - 18), name, fill=(168, 176, 188, 255), font=f_label)
            d.text((x, yy + sh - 36), hexv, fill=(110, 118, 132, 255), font=f_mono)
        y += rcount * (sh + label_h) + 10
    return img


# =====================================================================
# 控件样张
# =====================================================================
def _button(d, x, y, w, h, fill, outline, label, text_fill):
    d.rounded_rectangle([x, y, x + w, y + h], radius=8, fill=fill, outline=outline, width=2)
    f = _font(18)
    tw = d.textlength(label, font=f)
    d.text((x + (w - tw) / 2, y + h / 2 - 13), label, fill=text_fill, font=f)


def ui_controls() -> Image.Image:
    W, H = 860, 720
    img = Image.new("RGBA", (W, H), (30, 34, 42, 255))
    d = ImageDraw.Draw(img)
    f_cap = _font(15)
    f_title = _font(22)

    primary = hex2rgb("#E0A858") + (255,)
    primary_down = hex2rgb("#C88A3A") + (255,)
    primary_dark = hex2rgb("#9A6424") + (255,)
    surface = hex2rgb("#2F3540") + (255,)
    surface6 = hex2rgb("#262B34") + (255,)
    border = hex2rgb("#3E4652") + (255,)
    text1 = (232, 234, 240, 255)
    text5 = (110, 118, 132, 255)
    bg9 = hex2rgb("#171A20") + (255,)

    x, y = 30, 20
    d.text((x, y), "控件样张（草案）", fill=text1, font=f_title)
    y += 50

    def caption(txt, yy):
        d.text((x, yy), txt, fill=(168, 176, 188, 255), font=f_cap)

    # 主按钮
    caption("主按钮", y)
    y += 26
    _button(d, x, y, 240, 64, primary, primary_dark, "维修", (26, 18, 8, 255))
    _button(d, x + 270, y, 240, 64, primary_down, primary_dark, "维修（按下）", (26, 18, 8, 255))
    _button(d, x + 540, y, 240, 64, surface6, border, "维修（禁用）", text5)
    y += 90

    # 次按钮
    caption("次按钮", y)
    y += 26
    _button(d, x, y, 240, 64, surface, border, "取消", text1)
    _button(d, x + 270, y, 240, 64, surface, (90, 158, 184, 255), "旋转", text1)
    _button(d, x + 540, y, 240, 64, surface6, border, "取消（禁用）", text5)
    y += 90

    # 页签
    caption("页签", y)
    y += 26
    _button(d, x, y, 200, 56, primary, primary_dark, "地图", (26, 18, 8, 255))
    _button(d, x + 220, y, 200, 56, surface, border, "事件", text1)
    _button(d, x + 440, y, 200, 56, surface, border, "任务", text1)
    y += 82

    # 输入框
    caption("输入框", y)
    y += 26
    d.rounded_rectangle([x, y, x + 320, y + 56], radius=8, fill=surface6, outline=border, width=2)
    d.text((x + 16, y + 16), "输入维修编号…", fill=text5, font=_font(16))
    d.rounded_rectangle([x + 350, y, x + 670, y + 56], radius=8, fill=surface6, outline=(224, 168, 88, 255), width=2)
    d.text((x + 366, y + 16), "EV-N01", fill=text1, font=_font(16))
    y += 82

    # 进度条
    caption("进度条", y)
    y += 26
    d.rounded_rectangle([x, y, x + 320, y + 16], radius=8, fill=bg9, outline=border, width=1)
    d.rounded_rectangle([x, y, x + 192, y + 16], radius=8, fill=primary)
    d.text((x + 340, y - 2), "60%", fill=text1, font=_font(14))
    y += 42

    # 滑块
    caption("滑块", y)
    y += 26
    d.rounded_rectangle([x, y + 5, x + 320, y + 11], radius=3, fill=bg9, outline=border, width=1)
    d.rounded_rectangle([x, y + 5, x + 160, y + 11], radius=3, fill=primary)
    d.ellipse([x + 148, y, x + 172, y + 16], fill=primary, outline=primary_dark, width=2)
    y += 60

    d.text((x, y), "（控件实际渲染由 uGUI + 九宫格切片承接，此处为视觉方向示意）",
           fill=(110, 118, 132, 255), font=_font(14))
    return img


# =====================================================================
# 盘面样张
# =====================================================================
def axial_to_pixel(q, r, size):
    x = size * math.sqrt(3) * (q + r / 2)
    y = size * 1.5 * r
    return x, y


def hex_vertices(cx, cy, rad):
    return [(cx + rad * math.cos(math.radians(90 + i * 60)),
             cy + rad * math.sin(math.radians(90 + i * 60))) for i in range(6)]


def board_sample() -> Image.Image:
    W = H = 760
    img = Image.new("RGBA", (W, H), (23, 26, 32, 255))
    d = ImageDraw.Draw(img)
    f_title = _font(22)
    f_cap = _font(14)
    f_mono = _mono(13)

    board_radius = 5
    cell = 26.0
    cx, cy = W / 2, H / 2 + 18

    normal_fill = hex2rgb("#232833") + (255,)
    normal_edge = hex2rgb("#3A4250") + (255,)
    wall_fill = hex2rgb("#3A2026") + (255,)
    wall_edge = hex2rgb("#5A2A32") + (255,)
    frame_edge = hex2rgb("#5A6472") + (255,)

    # 蜂窝格（轴向半径簇：max(|q|,|r|,|q+r|) <= board_radius）
    for q in range(-board_radius, board_radius + 1):
        for r in range(-board_radius, board_radius + 1):
            if max(abs(q), abs(r), abs(q + r)) > board_radius:
                continue
            px, py = axial_to_pixel(q, r, cell)
            x, y = cx + px, cy + py
            verts = hex_vertices(x, y, cell - 1.5)
            is_wall = max(abs(q), abs(r), abs(q + r)) == board_radius
            fill = wall_fill if is_wall else normal_fill
            edge = wall_edge if is_wall else normal_edge
            d.polygon(verts, fill=fill, outline=edge)

    # 盘框（点顶大六边形，半径 = 2·(board_radius+1)·cell）
    frame_r = 2 * (board_radius + 1) * cell
    d.polygon(hex_vertices(cx, cy, frame_r), outline=frame_edge, width=3)

    # 示例棋子（中心区 3 枚）
    pieces = [
        (0, 0, hex2rgb("#F2A968") + (255,), hex2rgb("#7A4418") + (255,)),
        (1, 0, hex2rgb("#82C6DA") + (255,), hex2rgb("#1F5464") + (255,)),
        (-1, 0, hex2rgb("#E28888") + (255,), hex2rgb("#6E2626") + (255,)),
    ]
    for q, r, fill, edge in pieces:
        px, py = axial_to_pixel(q, r, cell)
        x, y = cx + px, cy + py
        d.polygon(hex_vertices(x, y, cell - 4), fill=fill, outline=edge, width=2)

    # 6 相位方向箭头（D0~D5，标注在盘框外）
    for i in range(6):
        ang = math.radians(90 + i * 60)
        ax = cx + (frame_r + 46) * math.cos(ang)
        ay = cy + (frame_r + 46) * math.sin(ang)
        # 箭头：从盘框边指向外
        head = (ax, ay)
        tail = (cx + (frame_r + 8) * math.cos(ang), cy + (frame_r + 8) * math.sin(ang))
        d.line([tail, head], fill=(224, 168, 88, 255), width=3)
        # 箭头尖
        perp = ang + math.pi / 2
        for s in (-1, 1):
            px_ = ax - 14 * math.cos(ang) + s * 7 * math.cos(perp)
            py_ = ay - 14 * math.sin(ang) + s * 7 * math.sin(perp)
            d.line([(ax, ay), (px_, py_)], fill=(224, 168, 88, 255), width=3)
        label = f"D{i}"
        tw = d.textlength(label, font=f_mono)
        lx = ax + 24 * math.cos(ang) - tw / 2
        ly = ay + 24 * math.sin(ang) - 9
        d.text((lx, ly), label, fill=(224, 168, 88, 255), font=f_mono)

    d.text((30, 22), "六相定势盘视觉样张（草案）", fill=(232, 234, 240, 255), font=f_title)
    d.text((30, 58), f"boardRadius={board_radius}  正常格深灰蓝 / 墙格暗红 / 6 相位 D0~D5",
           fill=(168, 176, 188, 255), font=f_cap)
    return img


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    jobs = [
        ("ui_color_palette.png", color_palette),
        ("ui_controls.png", ui_controls),
        ("board_sample.png", board_sample),
    ]
    for name, fn in jobs:
        img = fn()
        img.convert("RGBA").save(OUT / name)
        print(f"[OK] {OUT / name}  {img.size}")
    print("done")


if __name__ == "__main__":
    main()
