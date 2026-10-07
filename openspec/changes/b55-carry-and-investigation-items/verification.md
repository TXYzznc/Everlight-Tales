# 验收记录

日期：2026-10-07。Unity：2022.3.62f3c1 / WindowsEditor。

## 范围

保留 CarryAvailableItem、InvestigationHotspotItem 独立预制体。逐个修改两个条目及 PreparationPage、InvestigationPage，同步显示脚本、页面入口、编辑器生成/绑定工具和布局契约。

## 静态与编译验证

- 停止 Play Mode 后使用普通 Unity 编译流程；未重启编辑器，未使用 FSR 验证结构变更。
- 四个预制体序列化引用有效；六个携带槽位具有投放目标；按钮 SpriteSwap 图片完整；没有 Missing Script。
- 单项条目和调查页面的绑定菜单运行后，引用审计通过。
- `python tools/audit_framework_purity.py` 通过。
- 脚本 diff 检查通过；Unity 自动序列化的预制体存在空值行尾空格，未为消除空格手改 YAML。

## 运行日志验证

通过 GF 实际打开、关闭并再次打开准备页和调查页，各两轮。使用临时验收 World，不写正式存档；未截图。

- 携带模型：投放到指定空槽、替换计数、不重复携带通过。
- 携带页面：12 个候选条目的宽度、持续选中状态、点击补位、槽位移除、拖放替换、满槽处理、纵向滚动通过。
- 形态：只切换已解锁的本零件形态，不修改 World.CurrentForms；确认后盘面收到准备形态，基础形态仍按原契约使用 null。
- 池复用：隐藏节点、回调与选中状态重置，重建无残留；空数据下无候选行、槽位和确认按钮禁用。
- 调查：正式参数入口、不同归一化位置、重复点击只计一次、确认后锁定、池重建清理通过。
- 调查完成：未完成时不能结束，完成回调只调用一次，通过 GF 关闭；默认自动结束与空数据状态通过。

最终日志：8 条通过记录，Error 0，Warning 0。结束后追加资产引用审计通过。

原始记录位于本机 `Library/SpecialItemValidation/`：`final-runtime.json`、`final-errors.json`、`final-warnings.json`、`final-assets.json`、`final-editor-state.json`。

## 清理与边界

- 已关闭验收数据开关。最终编辑器状态 isPlaying=false、isCompiling=false。
- 初次验收时调查专用圆环图片尚未交付，使用程序绘制圆环；后续用户要求改用规范图片，见下方追加验收。
- 调查页已提供 InvestigationPageData 入口；实际剧情调用方仍需传入真实热点配置，本次未虚构剧情或热点数据。
- OpenSpec CLI 当前不可用，变更文档已人工维护，未声称通过 CLI 校验。

## 规范图片追加验收（2026-10-07）

- 创建 `Assets/Game/Sprites/UI/控件/` 下 SCR-16-04 idle/pulse、SCR-16-05 confirmed、SCR-16-06 pressed 四张 256×256 透明占位 PNG，并由 Unity 生成 meta。
- 根 Image 和 Button SpriteSwap 引用对应图片，预制体移除程序圆环节点；脚本使用白色乘色，避免正式彩色资源被二次染色。
- 普通 Unity 编译完成，Console Error 0。资产审计验证四张图片的尺寸、Sprite/Single、透明、无 mipmap、Clamp 和状态路径，全部通过。
- 重复执行接入菜单后，四张 PNG 及各自 meta 的 SHA256 均保持不变，验证工具不会覆盖后续替换资源。
- 本次为资源接入检查，未重复前次业务流程运行验收，未截图；编辑器保持退出 Play Mode。
- 日志保存于 `Library/SpecialItemValidation/sprite-assets.json`。后续覆盖同名 PNG 并保留 meta，即可保持 GUID 和预制体引用。
