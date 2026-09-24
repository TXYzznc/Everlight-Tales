#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""从 `<Form>.contract.json` 生成 Form 脚本。

每个 Form 产出两个文件，构成同一个 partial 类：

  `<Form>.Fields.cs`  契约绑定字段与只读属性（完全由契约决定，永远生成）
  `<Form>.cs`         类逻辑（业务逻辑手写；首次运行生成骨架，之后跳过）

这样切分的原因：节点引用是纯粹的机械内容且数量大，必须由契约生成；业务逻辑手写。
手写脚本只跳过 `.cs`，`.Fields.cs` 照常生成，因此它们能随契约自动获得新增的节点引用。

Everlight 版与 stellar-frontier 版的差异：
  * namespace = Everlight.Tales.UI；基类 = UIFormBase + IProjectUIForm（不是 AutoEraShellFormBase）
  * 契约直接手写（Everlight 页面是单页竖屏，没有 stellar 的共享外壳/多页导航），
    绑定全部在契约 bindings / formArrays 里显式声明，不做命名规则自动推导。
  * FormKey = Form 名去掉结尾 `Form`（如 PreparationPageForm → PreparationPage）。

用法：
    python tools/ui_contract_to_form_script.py            # 生成全部契约的脚本
    python tools/ui_contract_to_form_script.py --force    # 覆盖已存在的逻辑脚本骨架
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

CONTRACT_DIR = Path("Docs/Development/UI-PrefabLayouts")
SCRIPT_DIR = Path("Assets/Game/Scripts/EverlightTales/UI")

# 契约 kind → C# 字段类型
TYPE_BY_KIND = {
    "GameObject": "GameObject",
    "TextMeshProUGUI": "TMP_Text",
    "Image": "Image",
    "Button": "Button",
    "Slider": "Slider",
    "Toggle": "Toggle",
    "RectTransform": "RectTransform",
    "Transform": "Transform",
}

FIELDS_TEMPLATE = """using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{{
    /// <summary>
    /// {form} 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/{form}.contract.json 生成，请勿手改）。
    ///
    /// 与 {form}.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// {form}.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class {form}
    {{
{fields}    }}
}}
"""

LOGIC_TEMPLATE = """using UnityEngine;

namespace Everlight.Tales.UI
{{
    /// <summary>
    /// {form} 的业务逻辑（契约绑定字段见同名的 {form}.Fields.cs，同一 partial 类）。
    /// 结构由 Docs/Development/UI-PrefabLayouts/{form}.contract.json 生成；本文件手写维护。
    /// </summary>
    public sealed partial class {form} : UIFormBase, IProjectUIForm
    {{
        public string FormKey => "{form_key}";

        protected override void OnInit(object userData)
        {{
            base.OnInit(userData);
        }}

        protected override void OnOpen(object userData)
        {{
            base.OnOpen(userData);
        }}
    }}
}}
"""


def property_name(field: str) -> str:
    name = field.lstrip("_")
    return name[:1].upper() + name[1:] if name else field


def form_key_of(form: str) -> str:
    return form[:-4] if form.endswith("Form") else form


def build_fields(contract: dict) -> tuple[str, list[str], list[str]]:
    """返回 (字段声明文本, 字段名列表, 警告列表)。"""
    lines: list[str] = []
    names: list[str] = []
    warnings: list[str] = []

    for binding in contract.get("bindings", []):
        field = binding["path"]
        kind = binding.get("kind", "GameObject")
        cs_type = TYPE_BY_KIND.get(kind)
        if cs_type is None:
            warnings.append(f"未知 kind {kind}（字段 {field}），已按 GameObject 处理")
            cs_type = "GameObject"
        lines.append(f"        [SerializeField] private {cs_type} {field};")
        lines.append(f"        public {cs_type} {property_name(field)} => {field};")
        names.append(field)

    for field, items in (contract.get("formArrays") or {}).items():
        kinds = {item.get("kind", "GameObject") for item in items}
        if len(kinds) != 1:
            warnings.append(f"{field} 的元素 kind 不一致 {sorted(kinds)}，已按 GameObject 处理")
        kind = kinds.pop() if len(kinds) == 1 else "GameObject"
        cs_type = TYPE_BY_KIND.get(kind, "GameObject")
        lines.append(f"        [SerializeField] private {cs_type}[] {field};")
        lines.append(f"        public {cs_type}[] {property_name(field)} => {field};")
        names.append(field)

    return "\n".join(lines) + "\n", names, warnings


def build_fields_file(contract: dict, fields_text: str) -> str:
    return FIELDS_TEMPLATE.format(form=contract["form"], fields=fields_text)


def build_logic_file(contract: dict) -> str:
    return LOGIC_TEMPLATE.format(
        form=contract["form"],
        form_key=form_key_of(contract["form"]),
    )


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--force", action="store_true", help="覆盖已存在的逻辑脚本骨架")
    ap.add_argument("--out", default=str(SCRIPT_DIR))
    args = ap.parse_args()

    out_dir = Path(args.out)
    out_dir.mkdir(parents=True, exist_ok=True)

    contracts = sorted(CONTRACT_DIR.glob("*.contract.json"))
    if not contracts:
        print(f"未找到契约：{CONTRACT_DIR}", file=sys.stderr)
        return 2

    fields_written = logic_written = logic_skipped = 0
    for path in contracts:
        contract = json.loads(path.read_text(encoding="utf-8"))
        form = contract["form"]
        fields_text, names, warnings = build_fields(contract)

        (out_dir / f"{form}.Fields.cs").write_text(
            build_fields_file(contract, fields_text), encoding="utf-8")
        fields_written += 1

        logic_path = out_dir / f"{form}.cs"
        if logic_path.exists() and not args.force:
            logic_skipped += 1
        else:
            logic_path.write_text(build_logic_file(contract), encoding="utf-8")
            logic_written += 1

        for warning in warnings:
            print(f"  [警告] {form}: {warning}")

    print(f"\n字段文件 {fields_written} 个；逻辑脚本生成 {logic_written} 个、跳过 {logic_skipped} 个（已存在）")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
