## 1. 准备页结构

- [x] 1.1 更新 PreparationPage 契约和预制体，加入任务概览节点与局部详情卡片节点。
- [x] 1.2 重新生成 PreparationPageForm.Fields.cs 并绑定新节点。
- [x] 1.3 移除 PreviewBoardView 的准备页主流程调用，保留任务概览摘要。

## 2. 数据与详情

- [x] 2.1 提取共享零件详情格式化器，复用 BoardPage 的数值详情逻辑。
- [x] 2.2 PreparationPage 绑定三段任务概览文本。
- [x] 2.3 CarryAvailableItem 接入长按、松开、移出和拖拽取消详情卡片。

## 3. 拖拽与验证

- [x] 3.1 将拖拽代理改为 80% 透明零件图片。
- [x] 3.2 停止 Play Mode 后完成结构修改，刷新资源并检查引用。
- [x] 3.3 编译、控制台错误、准备页打开和拖拽/长按路径验证。
