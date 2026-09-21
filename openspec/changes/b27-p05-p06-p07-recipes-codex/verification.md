# 验证：b27 F 类配方 + M 类图样来源与配方 + 零件图鉴

## 验证命令

1. `python tools/unity_meta.py`（补齐新增 .meta）
2. `python tools/verify_project_assemblies.py`（五层 asmdef 引用方向）
3. `python tools/audit_framework_purity.py`（框架纯度）
4. UnitySkills：`debug_force_recompile` → `/compile/status` 0 错 0 警 → `editor_play` → `console_get_logs`

## 探针用例（B27AcceptanceRunner，Play Mode，37/37 通过）

### 目录计数（P4-005/P4-006）
- 形态目录 19 条（4 F + 15 M）；F 4 条、M 15 条；零件图鉴 20 条
- 异常纹样（MT-006）只用于 5 个代表性形态（M-003/M-006/M-011/M-012/M-013）

### 配方数据（P4-005/P4-006）
- F：P-001-F01 80/T-G01/无材料、P-001-F02 120/T-G02/精密齿轮×2+铜芯线×1
- M：M-001 宿主 P-019/320/L-05/聂小倩 且材料正确；M-006 异常纹样·道林格雷的画像×1；M-011 480/L-15/异常纹样·如月车站×1；M-012 宿主 P-004/L-01/红舞鞋；M-003 异常纹样·风月宝鉴×1；M-013 异常纹样·画皮×1

### 图鉴三态（P4-007）
- P 页 20 条、M 页 15 条、总条数 35
- P 三态：初始完全未知 → 已拥有（OwnedParts）→ 已现源（KnownParts）
- M 三态：无图样完全未知 → 有图样已知未拥有 → 加工后已拥有；图样保留
- 收集进度：已拥有 P=1 + M=1 → 2/35；完全未知显示「？？？」、已拥有显示真实名称
- 类型化纹样隔离：解锁 M-003（风月宝鉴）不消耗画皮纹样、风月宝鉴纹样已消耗

### 存档往返
- OwnedParts/KnownParts/Blueprints 往返恢复；恢复后收集进度 2/35

## 结果

- 探针 37/37 通过，探针文件及 .meta 验收后删除。
- 最终重编译 0 错 0 警。
- 静态检查（meta/程序集/纯度）全部通过。
