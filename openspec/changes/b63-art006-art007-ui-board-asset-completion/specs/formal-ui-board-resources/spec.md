# formal-ui-board-resources

## Purpose

保证正式控件和盘面资源按语义与状态完整交付，源画布、透明边缘和九宫格均适合现有界面使用，并在替换图片时保持引用稳定和小尺寸可辨识的实际质量。

## ADDED Requirements

### Requirement: Control states share a consistent canvas

正式控件 SHALL 按批准清单覆盖37个现有状态，同组画布/留白/Border一致；每态独立资源，拉伸不破坏装饰和交互辨识。

#### Scenario: Resize repaired control

- **WHEN** 修补后的SHR-021或SHR-030在常用和拉伸尺寸显示
- **THEN** 边框正常、状态切换不跳位、装饰不被拉进中心

### Requirement: Source icons are normalized at source

五类来源图标及通用回退 SHALL 使用统一128×128源画布，无无关碎片，主体边距和中心一致，透明正确。

#### Scenario: Importer only limits resolution

- **WHEN** 源图仍大于128且带碎片，但导入最大尺寸设128
- **THEN** 不能通过源图验收，必须修正实际文件

### Requirement: Board assets cover active semantics

盘面资源 MUST 按当前实际枚举与清单覆盖20P、19种形态、8O、8A、标记和首发维修目标，不能按旧15M口径遗漏已实现形态。

#### Scenario: New anomaly becomes playable

- **WHEN** A-003或A-007行为已完成且其正式图就绪
- **THEN** 绑定对应语义，验证要求正确引用，不继续禁止新键，也不开放后置案件

### Requirement: Replacement preserves references

原路径资源替换 SHALL 保留GUID并核对引用；新语义资源须有显式映射，正式必需图缺失不能靠开发占位通过验收。

#### Scenario: Image replaced

- **WHEN** 一个已有UI状态源图被修补
- **THEN** 预制体仍解析到同一语义资源，GUID无意外变化

### Requirement: Small icons remain distinguishable

关键盘面图标 SHALL 在32/24像素与灰度预览中通过轮廓或纹理区分，不仅依靠颜色。

#### Scenario: Two icons differ only by color

- **WHEN** 缩小并转灰度后两种关键对象不可区分
- **THEN** 该组需修补形状或纹理后才能交付
