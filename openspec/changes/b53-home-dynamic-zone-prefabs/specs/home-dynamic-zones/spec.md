## 新增需求：HomePage 动态四区

### 需求：HomePage 内容区为空容器
`Panel_HomeArea` 不保存静态业务子界面，仅运行时挂载 GuestPage、CodexPage、ArchivePage 和 ServicePage。

### 需求：四区导航
HomePage 只保留来客、收藏、保管、服务四个按钮。删除 `Btn_Zone_1`，保留其余按钮名称。按钮与资产数组依次对应四个区域，索引为 0 至 3。

#### 场景：依次点击四个区域
- **当** 用户依次点击来客、收藏、保管和服务
- **则** 在 `Panel_HomeArea` 下生成对应页面；每区缓存一个实例，切换时同步显隐与选中状态。

### 需求：工作台独立承载
HomePage 不引用或实例化 WorkbenchPage，也不保留加工跳转入口。底部工作台页签继续由 MainPageShell 加载独立 WorkbenchPage UIForm。
