"""只读资产审计与HTML审阅排版；不改原图片、meta、Prefab或用户工作簿。"""
from pathlib import Path
from collections import Counter, defaultdict
from PIL import Image
import hashlib, html, json, re, shutil
from urllib.parse import quote

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[3]
SPRITES = ROOT / "Assets/Game/Sprites"


def text(path):
    return path.read_text(encoding="utf-8-sig")


def decode_yaml(source):
    def decode(match):
        try:
            return json.loads(match.group())
        except ValueError:
            return match.group()
    return re.sub(r'"(?:[^"\\]|\\.)*"', decode, source)


catalog = text(ROOT / "Assets/Game/Data/UI/UIFormalSpriteCatalog.asset")
aliases = defaultdict(list)
for key, guid in re.findall(r"- Key: (.*?)\n\s+Sprite: \{fileID: \d+, guid: ([a-f0-9]{32})", catalog):
    aliases[guid].append(json.loads(key) if key.startswith('"') else key)

sources = []
for folder, pattern in [(ROOT / "Assets/Game/Prefabs/UI", "*.prefab"),
                        (ROOT / "Assets/Game/Scripts/EverlightTales/UI", "*.cs")]:
    for path in sorted(folder.rglob(pattern)):
        if "/Editor/" in path.as_posix() or "/Diagnostics/" in path.as_posix() or "Validation" in path.name:
            continue
        sources.append((path.relative_to(ROOT).as_posix(), [decode_yaml(line) for line in text(path).splitlines()] if path.suffix == ".prefab" else text(path).splitlines()))
for path in sorted((ROOT / "Assets/Game/DataTable/Meta").glob("*.txt")):
    if path.name in {"NarrativeTable.txt", "AssetBindingTable.txt", "PortraitTable.txt"}:
        sources.append((path.relative_to(ROOT).as_posix(), text(path).splitlines()))

sources.append(("Assets/Game/Scripts/EverlightTales/Data/FirstCaseContent.cs", text(ROOT/"Assets/Game/Scripts/EverlightTales/Data/FirstCaseContent.cs").splitlines()))

inventory = []
for path in sorted(SPRITES.rglob("*.png")):
    meta = text(path.with_name(path.name + ".meta"))
    guid = re.search(r"^guid: ([a-f0-9]{32})", meta, re.M).group(1)
    keys = aliases.get(guid, [])
    references = set()
    for filename, lines in sources:
        for line, content in enumerate(lines, 1):
            direct = guid in content
            semantic = any(('"' + key + '"') in content or (": " + key) in content
                           or (filename.endswith(".txt") and key in content.split("\t")) for key in keys)
            dynamic = any(key.startswith(prefix) and ('"' + prefix + '"') in content
                          for prefix in ["part:", "form:", "obstacle:", "anomaly:"] for key in keys)
            if direct or semantic or dynamic:
                references.add(filename + ":" + str(line) + ("（动态前缀）" if dynamic else ""))
    width, height = Image.open(path).size
    border = re.search(r"spriteBorder: \{x: ([\d.]+), y: ([\d.]+), z: ([\d.]+), w: ([\d.]+)\}", meta)
    limit = re.search(r"buildTarget: DefaultTexturePlatform\s+maxTextureSize: (\d+)", meta)
    limit = int(limit.group(1)) if limit else int(re.search(r"maxTextureSize: (\d+)", meta).group(1))
    scale = min(1, limit / max(width, height))
    relative = path.relative_to(ROOT).as_posix()
    group = path.relative_to(SPRITES).parts[0]
    treatment = "复用（现有引用）" if references else "复用备库（未检出直接引用）"
    if group == "Placeholder":
        treatment = "占位保留；不得冒充正式完成"
    if path.name == "SCR-07-07-normal定势座.png":
        treatment = "退役视觉；只保留文件，不恢复GravitySeat"
    if path.name in {"回声区域.png", "重绘帷幕.png"}:
        treatment = "图已存在；b63补anomaly语义绑定"
    inventory.append(dict(path=relative, guid=guid, keys=keys, group=group,
                          state=path.stem.split("-")[2] if path.stem.count("-") >= 2 else "见语义名",
                          size=[width, height], import_limit=limit,
                          import_estimate=[round(width * scale), round(height * scale)],
                          border=list(map(float, border.groups())) if border else [0, 0, 0, 0],
                          sha256=hashlib.sha256(path.read_bytes()).hexdigest(), references=sorted(references),
                          treatment=treatment, batch="b63" if group in {"UI", "盘面", "Placeholder"} else "b64",
                          sample={"UI":"A-UI", "盘面":"B-BOARD", "人物":"C-CHAR", "场景":"D-SCENE", "怪谈":"C-L01"}.get(group,"待分类")))

counts = Counter(x["group"] for x in inventory)
formal = [x for x in inventory if x["group"] != "Placeholder"]
controls = {p.name.split("-")[0] + "-" + p.name.split("-")[1]
            for p in (SPRITES / "UI/控件").glob("SHR-*.png")}
report = dict(groups=counts, formal=len(formal), placeholders=counts["Placeholder"],
              catalog_aliases=sum(map(len, aliases.values())), catalog_images=len(aliases),
              control_families=len(controls), assets=inventory)
(HERE / "inventory.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")

lines = ["# b62 生产清单 v1（待样张选定）", "",
         "2026-10-14。以当前文件、GUID、正式Prefab和运行代码抽取，不改用户任务工作簿。",
         "统计：" + "、".join(f"{k} {v}" for k,v in counts.items()) + f"；正式{len(formal)}图，占位{counts['Placeholder']}图。",
         f"正式目录{len(aliases)}张图、{sum(map(len,aliases.values()))}个语义别名；控件文件按SHR编号有{len(controls)}组（与旧37项口径逐项对照，不能把状态张数当控件数）。",
         "", "引用是静态抽取证据，动态前缀不等于每个变体均已在首发露出；完整位置见inventory.json。导入尺寸为源尺寸按Default上限推算，尚非Unity贴图实测，b63/b64须复核NPOT/平台覆盖与实际Border。",
         "每行样张列对应review.html的A/B/C/D组。全部状态为待选定/待最终验收，已有正式绑定可复用；不自动批准新稿。",
         "", "## 数量与明确缺口", "",
         "- 20P + 19形态（15M、4F）+ 8O + 8A + 2标记 = 57对象图；盘面70图还包含结构/事件等13图，不重复安排已完成F4行为。",
         "- 回声区域与重绘帷幕文件存在，但anomaly:EchoZone/RedrawCurtain尚未进正式目录；b63定向绑定并更新旧验证断言。",
         "- 当前正式人物只有玩家、沈遥各一张中性全身。五人真实状态表见scope-and-handoff.md；不把一张中性图记成三态。",
         "- L01只有一张红鞋对象图，hint/manifest/after为待映射状态；其它14案不新增正式美术。",
         "- 五地点十张昼夜图完整；P0九组按复用/缺层区分，不能用十张地点图抵作九组母版全部完成。",
         "- SHR021/030等状态和长文本/九宫格在A组排版夹具检查，真正Prefab状态接入在b63/b65复验。",
         "", "## 逐资源清单", "",
         "| 语义键/状态 | 路径与GUID | 源→预计导入/上限；Border | 正式引用点（最多展示2处） | 分类/归属/样张 |",
         "|---|---|---|---|---|"]
for item in inventory:
    label = ", ".join(item["keys"][:3]) or "未进正式目录：" + Path(item["path"]).stem
    size = "×".join(map(str,item["size"])) + "→" + "×".join(map(str,item["import_estimate"]))
    refs = "<br>".join(item["references"][:2]) or "静态未检出；复用点待具体接入"
    lines.append(f"| {label}<br>{item['state']} | `{item['path']}`<br>`{item['guid']}` | {size} / {item['import_limit']}；{item['border']} | {refs} | {item['treatment']} / {item['batch']} / {item['sample']} |")
lines += ["", "## 新代表稿（未导入，不生成正式GUID）", "",
          "| 稿件 | 文件 | 生产处理 |", "|---|---|---|"]
for path in sorted((HERE/"raw").glob("*.png")):
    lines.append(f"| {path.stem} | `raw/{path.name}` | 仅样张；待具体版本选定后交b64，保持既有GUID不变 |")
lines += ["", "## 缺项/状态与计划语义（不是已实现绑定）", "",
          "下列状态ID来自人物/背景设计，GUID为待制作或待映射；不存在的文件不能标为完成。当前精确引用与后置边界见scope-and-handoff.md。",
          "", "| 设计语义/状态 | 当前图或GUID | 引用与处理 | 归属/样张 |", "|---|---|---|---|"]
for identity, states in [("玩家",["normal","active","night"]),("沈遥",["tense_dance","responsive","recovered"]),
                         ("周衡",["normal","lost","rescued"]),("林岑",["normal","research","warning"]),
                         ("顾师傅",["normal","craft","encourage"])]:
    for state in states:
        current = "无独立状态图/未分配GUID"
        treatment = "当前未引用，后置"
        if identity=="玩家" and state=="normal":
            current="玩家.png / 见上方GUID"; treatment="正式对话左挂点，复用"
        if identity=="沈遥" and state in {"tense_dance","recovered"}:
            current="当前复用沈遥.png中性图，状态图待补"; treatment="现有request/confirmed或thanks引用，按剧情差分"
        if identity=="周衡" and state=="normal":
            current="CHAR-MASTER-v1样张，未导入"; treatment="AS-PORT-MASTER待补；序章只有文字，不为用图改布局"
        lines.append(f"| {identity}.{state} | {current} | {treatment} | b64 / C-CHAR |")
lines += ["| 居民模板.normal | CHAR-RESIDENT-v1，未导入 | 代表稿待选；无实际引用前不批量变体 | b64 / C-CHAR |",
          "| L01.hint / manifest / after | 当前均只有红鞋.png，见GUID | 状态容器/公开进度映射，不能一张图算三态完成 | b64/b65 / C-L01 |"]
for key in ["BG-T01","BG-H01","BG-M01","BG-P01","BG-B01","BG-S01","BG-D01","BG-D02","BG-O01"]:
    lines.append(f"| {key} | 专用GUID/分层待核对 | scope-and-handoff中逐组复用/新增，不用地点10图抵作全部完成 | b64 / D-SCENE |")
(HERE / "production-manifest.md").write_text("\n".join(lines)+"\n",encoding="utf-8")


def uri(path):
    return quote("../../../../" + path.relative_to(ROOT).as_posix(), safe="/:.-_")


def picture(path, label, cls=""):
    return f'<figure class="{cls}"><a href="{uri(path)}" target="_blank" title="查看原图"><img src="{uri(path)}" alt="{html.escape(label)}"></a><figcaption>{html.escape(label)}</figcaption></figure>'


def find(pattern):
    return next(SPRITES.rglob(pattern))


style = """
*{box-sizing:border-box}body{margin:0;background:#131b25;color:#e9e4da;font:15px 'Microsoft YaHei',sans-serif;line-height:1.6}
header{padding:28px 40px;background:#202c38;border-bottom:1px solid #6f6250}h1{font-size:27px;margin:0}h2{color:#f2c687;margin:0 0 18px}h3{font-size:17px}p{color:#bdc8ce}
nav{position:sticky;top:0;background:#17212ded;padding:12px 40px;z-index:2;display:flex;flex-wrap:wrap;gap:24px;backdrop-filter:blur(8px)}a{color:#f1c588}main{padding:28px 40px;max-width:1520px;margin:auto}section{margin:0 0 56px;padding:24px;background:#1e2936;border:1px solid #455363;border-radius:12px}
figure{margin:0;display:flex;flex-direction:column;align-items:center;gap:8px}figure img{max-width:100%;object-fit:contain}figcaption{text-align:center;font-size:12px;color:#c8cbd0;max-width:220px}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(125px,1fr));gap:18px}.tiles img{width:112px;height:112px;background:url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'%3E%3Cpolygon points='25,7 75,7 100,50 75,93 25,93 0,50' fill='%23303b48'/%3E%3C/svg%3E") center/contain no-repeat}
.states{display:flex;flex-wrap:wrap;gap:14px}.states figure{width:240px}.states img{width:240px;height:74px}.icons{display:flex;gap:28px;flex-wrap:wrap}.icons img{width:60px;height:60px}.portraits{display:flex;justify-content:center;gap:12px;flex-wrap:wrap;background:#121820;padding:16px}.portraits figure{width:200px}.portraits img{height:326px;width:200px}.light{background:#e6e0d5}.portraits.light figcaption{color:#3b4855}.portraits .square{overflow:hidden}.portraits .square img{width:326px;height:326px;max-width:none;flex-shrink:0}.scenes{display:grid;grid-template-columns:repeat(auto-fit,minmax(230px,1fr));gap:20px}.scenes img{width:270px;height:480px}.proofs{display:flex;flex-wrap:wrap;gap:20px}.proofs img{width:270px;height:480px}.nine{display:flex;gap:16px;flex-wrap:wrap;align-items:center}.nine div{max-width:100%;border:24px solid transparent;color:#fff;padding:0 8px;text-align:center;background:#27323c;font-size:16px;min-height:64px;display:grid;place-items:center}.nine small{color:#aebdc8}.note{padding:12px 16px;border-left:3px solid #d4a166;background:#263341}.badge{display:inline-block;padding:2px 9px;border-radius:5px;background:#574533;color:#ffdba7}button{padding:8px 18px;background:#e6c08c;color:#17212d;border:0;border-radius:5px;cursor:pointer}table{border-collapse:collapse;width:100%;font-size:13px}td,th{padding:9px;border:1px solid #4e5d6c;text-align:left}.mixboard{max-width:600px;padding:20px;background:#121b25;border:1px solid #7e7662;border-radius:12px}.mixrow{display:flex;justify-content:center;gap:2px;margin-top:-8px}.mixrow figure{width:106px;min-width:0}.mixrow img{width:96px;height:96px;background:url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 100 100'%3E%3Cpolygon points='25,7 75,7 100,50 75,93 25,93 0,50' fill='%23354252'/%3E%3C/svg%3E") center/contain no-repeat}.mixrow figcaption{font-size:11px}
"""
parts = [f'<!doctype html><html lang="zh-CN"><meta charset="utf-8"><title>b62 样张审阅 v1</title><link rel="icon" href="data:,"><style>{style}</style>',
         '<header><span class="badge">待具体版本选定 · 保留现有方向</span><h1>Everlight Tales / b62 样张审阅</h1><p>先看小尺寸辨识，再看原稿。以下排版夹具用于美术审阅；正式Unity画面单独列出。原始图片、GUID和现有Prefab没有替换。</p></header>',
         '<nav><a href="#ui">A 控件与图标</a><a href="#board">B 盘面</a><a href="#characters">C 人物</a><a href="#scenes">D 背景</a><a href="#formal">正式画面</a></nav><main>',
         '<section id="ui"><h2>A / 控件、状态与留白</h2><p>SHR021/030沿用当前图，分别查看normal/hover/pressed/disabled；selected用页签active作代表。</p><div class="states">']
for code in ["SHR-021", "SHR-030"]:
    for state in ["normal","hover","pressed","disabled"]:
        parts.append(picture(find(f"{code}-{state}*.png"),f"{code} {state}"))
parts += [picture(find("SHR-026-active*.png"),"SHR026 active / selected代表"),'</div><h3>九宫格与中文文字覆盖</h3><div class="nine">']
button=find("SHR-021-normal*.png")
item=next(x for x in inventory if x["path"]==button.relative_to(ROOT).as_posix())
border=" ".join(str(int(x))+" " for x in item["border"]).strip()
for width,label in [(240,"确认携带"),(360,"继续未完成的维修"),(640,"保留进度并从当前轮起点继续维修")]:
    parts.append(f'<div style="width:{width}px;border-image:url(\'{uri(button)}\') {border} fill / 24px stretch">{label}</div>')
row=find("SHR-005-normal*.png")
parts += [f'</div><div class="long-row" style="border:24px solid transparent;max-width:680px;min-height:136px;display:flex;align-items:center;padding:8px 12px;line-height:1.7;color:#e9e4da;border-image:url(\'{uri(row)}\') 24 fill / 24px stretch">长文本行：本次修复将消耗4格时间。请核对本轮目标、携带的关键件与恢复点；说明必须完整显示，不由底图烘焙文字。</div>',
          '<h3>来源图标：维修 / 处置 / 生活 / 调查 / 怪谈 / 通用回退</h3><div class="icons">']
for code in ["ICO-041","ICO-042","ICO-043","ICO-044","ICO-045","ICO-046"]:
    parts.append(picture(find(code+"*.png"),code))
parts += ['</div></section><section id="board"><h2>B / 同屏盘面对象</h2><p>20P、19形态、8O、8A和2标记；A003/A007此处是已有文件样张，正式语义绑定仍交b63。没有恢复退役GravitySeat视觉。</p>']
parts.append('<h3>类别辨识组合夹具（不是正式关卡）</h3><div class="mixboard">')
mix = [["撞锤.png","贯通撞锤.png","固定墙体.png"],
       ["回声区域.png","铆合钳.png","标记维修对象.png","重绘帷幕.png"],
       ["联动闸门.png","标记任务标记.png","借力改道夹.png"]]
for row in mix:
    parts.append('<div class="mixrow">')
    for name in row:
        parts.append(picture(find(name),Path(name).stem))
    parts.append('</div>')
parts.append('</div>')
for folder,label in [("零件","普通零件 20"),("形态","形态 19（含4F）"),("障碍","障碍 8"),("异常","异常 8"),("标记","标记与维修目标")]:
    parts.append(f'<h3>{label}</h3><div class="grid tiles">')
    for path in sorted((SPRITES/"盘面"/folder).rglob("*.png")):
        parts.append(picture(path,path.stem))
    parts.append('</div>')
parts += ['</section><section id="characters"><h2>C / 人物身份与透明边缘</h2><p>统一人物高度作风格对照，原图透明空边只在本页CSS裁去，未改文件；正式画像框适配仍交b64/b65。原玩家与原沈遥作风格基线；沈遥v2为边缘实验稿，周衡normal及居民v1为新增代表稿。五人三态并未全部制作，具体状态在scope-and-handoff.md中保留。</p><button onclick="document.querySelectorAll(\'.portraits\').forEach(x=>x.classList.toggle(\'light\'))">切换深 / 浅背景</button><div class="portraits">']
for path,label in [(SPRITES/"人物/玩家/玩家.png","现有玩家 / normal"),(SPRITES/"人物/沈遥/沈遥.png","现有沈遥 / 中性站姿"),
                   (HERE/"raw/CHAR-SHEN-v2.png","沈遥v2 / 边缘实验"),(HERE/"raw/CHAR-MASTER-v1.png","周衡v1 / normal"),
                   (HERE/"raw/CHAR-RESIDENT-v1.png","居民v1 / 通用代表")]:
    parts.append(picture(path,label,"square" if path.name in {"玩家.png","沈遥.png","CHAR-SHEN-v2.png"} else ""))
parts += ['</div><p class="note">RGBA检查：预览中红黄边缘主要为alpha=1的低透明像素；饱和红黄边界像素没有alpha≥32者。以此处实际深浅底合成判读，不把预览显示的低alpha颜色误判为实心红边。新稿保留原生尺寸，未拉伸到840×1360。</p></section>',
          '<section id="scenes"><h2>D / 工作台与五地点昼夜母版</h2><p>270×480窗口按原比例contain，不裁切或烘焙UI。点击图片查看原稿；1080×1920目标窗口见<a href="workbench.html" target="_blank">工作台工作尺寸对照</a>（源941×1672，终稿尺寸仍须b64处理）。P0九组的复用/缺层在清单另列。</p><div class="scenes">']
for name,label in [("BG-P01-day-v1.png","工作台 day v1（新样张）"),("BG-P01-night-v1.png","工作台 night v1（同镜头）")]:
    parts.append(picture(HERE/"raw"/name,label))
for place in ["长明修理铺","舞蹈教室","街区小店","裁缝铺","社区活动中心"]:
    for path in sorted((SPRITES/"场景/局内"/place).glob("*.png")):
        parts.append(picture(path,path.stem.replace("SCR-07-08-","")))
parts += ['</div></section><section id="formal"><h2>正式Unity运行画面</h2><p>来自本轮102项正式GF.UI回归的实际截图，按270×480检查；它们与上方审阅排版夹具分开记录。</p><div class="proofs">']
for name,label in [("01-opening.png","序章"),("02-shen-dialogue.png","对话画像框"),("04-board.png","红鞋实际盘面"),
                   ("07-roller-success.png","卷帘门实际结算"),("08-terminal-choice-recovery.png","终局选择恢复")]:
    path=ROOT/"Library/FirstCaseValidation"/name
    if path.exists():
        (HERE/"evidence").mkdir(exist_ok=True)
        saved=HERE/"evidence"/name
        shutil.copy2(path,saved)
        parts.append(picture(saved,label))
parts += ['</div><p class="note">已知终局候选行的文字裁切和反馈生命周期问题交b65；本页不把这些问题标为已修复。</p></section></main></html>']
(HERE/"review.html").write_text("\n".join(parts),encoding="utf-8")
print(json.dumps({k:v for k,v in report.items() if k!="assets"},ensure_ascii=False))
