#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Game 视图中键快速选中 UI 元素（编辑器辅助工具，仅 Editor 生效，不进入运行时）。
///
/// 用法：鼠标中键在 Game 视图中点击某个 UI 元素 → 立即在 Hierarchy 中选中该对象。
/// 开关：<see cref="Enabled"/>，状态持久化在 EditorPrefs（按用户/机器保存，重启编辑器后保持）。
///       工具箱面板 GameViewUiQuickSelectPanel 与菜单 Tools/Unity开发工具箱/Game视图 UI 中键选中 都可切换。
///
/// 命中方式：
/// - Play 模式且场景内存在启用的 EventSystem 时，走 EventSystem.RaycastAll，命中结果与运行时 UI 事件一致；
/// - 其余情况（编辑模式没有 EventSystem，Graphic 未注册到 GraphicRegistry）退化为按 Canvas 排序 +
///   图形几何命中，规则与 UGUI 的 GraphicRaycaster 对齐（raycastTarget / CanvasGroup.blocksRaycasts / raycastPadding）。
/// 限制：只对 UGUI（Image、Text、TextMeshPro 等 Graphic 派生组件）生效；不支持 UI Toolkit 与 IMGUI。
/// </summary>
[InitializeOnLoad]
public static class GameViewUiQuickSelect
{
    /// <summary>开关持久化键（EditorPrefs）</summary>
    public const string EnabledPrefKey = "ToolHub.GameViewUiQuickSelect.Enabled";

    private const string MenuPath = "Tools/Unity开发工具箱/Game视图 UI 中键选中";
    private const string GameViewTypeName = "UnityEditor.GameView,UnityEditor";
    private const int MiddleMouseButton = 2;

    private static readonly List<RaycastResult> _raycastResultBuffer = new List<RaycastResult>();

    private static bool _updateHooked;
    private static Type _gameViewType;

    static GameViewUiQuickSelect()
    {
        // 每次域重载后都要重新挂 update 钩子；延迟一帧，避开编辑器初始化阶段
        EditorApplication.delayCall += SyncHookState;
    }

    /// <summary>最近一次通过本工具选中的对象（工具箱面板用于显示状态）</summary>
    public static GameObject LastPicked { get; private set; }

    /// <summary>最近一次操作结果说明（工具箱面板用于显示状态）</summary>
    public static string LastPickInfo { get; private set; } = "尚未使用";

    /// <summary>开关：是否启用中键快速选中（读写 EditorPrefs，跨编辑器会话持久化）</summary>
    public static bool Enabled
    {
        get { return EditorPrefs.GetBool(EnabledPrefKey, false); }
        set
        {
            if (Enabled == value)
                return;

            EditorPrefs.SetBool(EnabledPrefKey, value);
            SyncHookState();
            LastPickInfo = value ? "已开启，等待在 Game 视图中键点击 UI" : "已关闭";
        }
    }

    /// <summary>按 EditorPrefs 中的开关状态同步 EditorApplication.update 钩子</summary>
    public static void SyncHookState()
    {
        bool shouldHook = Enabled;
        if (shouldHook == _updateHooked)
            return;

        if (shouldHook)
            EditorApplication.update += OnEditorUpdate;
        else
            EditorApplication.update -= OnEditorUpdate;

        _updateHooked = shouldHook;
    }

    /// <summary>在指定屏幕坐标（Game 视图坐标，左下为原点）拾取最上层的 UGUI 元素</summary>
    public static GameObject PickUiElement(Vector2 screenPosition)
    {
        var eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.isActiveAndEnabled)
        {
            var picked = PickByEventSystem(eventSystem, screenPosition);
            if (picked != null)
                return picked;

            // Play 模式下 EventSystem 的结论就是权威结论；
            // 编辑模式下 Graphic 不参与 EventSystem 的 raycast（未注册到 GraphicRegistry），继续退化为几何命中
            if (Application.isPlaying)
                return null;
        }

        return PickByGraphicGeometry(screenPosition);
    }

    [MenuItem(MenuPath, false, 20)]
    private static void ToggleFromMenu()
    {
        Enabled = !Enabled;
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleFromMenuValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    private static void OnEditorUpdate()
    {
        if (!TryGetMiddleMouseScreenPosition(out var screenPosition))
            return;

        if (!IsMouseOverGameView())
            return;

        var picked = PickUiElement(screenPosition);
        if (picked == null)
        {
            LastPickInfo = "最近一次中键点击未命中 UI 元素";
            return;
        }

        SelectInHierarchy(picked);
    }

    /// <summary>
    /// 读取鼠标中键按下当帧的屏幕坐标（Game 视图坐标）。
    /// 项目当前使用旧输入系统（ProjectSettings.activeInputHandler = 0，宏 ENABLE_LEGACY_INPUT_MANAGER 已定义）。
    /// 若将来整体切到「仅新输入系统」，这里返回 false（本程序集刻意不依赖 Unity.InputSystem，避免编译期引用耦合）。
    /// </summary>
    private static bool TryGetMiddleMouseScreenPosition(out Vector2 screenPosition)
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(MiddleMouseButton))
        {
            screenPosition = Input.mousePosition;
            return true;
        }
#endif
        screenPosition = default;
        return false;
    }

    /// <summary>鼠标是否正停在 Game 视图上（Scene 视图、其它窗口以及编辑器 UI 都不会触发）</summary>
    private static bool IsMouseOverGameView()
    {
        var mouseOver = EditorWindow.mouseOverWindow;
        if (mouseOver == null)
            return false;

        if (_gameViewType == null)
            _gameViewType = Type.GetType(GameViewTypeName);

        return _gameViewType != null && _gameViewType.IsInstanceOfType(mouseOver);
    }

    private static void SelectInHierarchy(GameObject target)
    {
        Selection.activeGameObject = target;
        EditorGUIUtility.PingObject(target); // 在 Hierarchy 中闪一下，便于定位
        SceneView.RepaintAll();

        LastPicked = target;
        LastPickInfo = $"已选中：{target.name}";
    }

    private static GameObject PickByEventSystem(EventSystem eventSystem, Vector2 screenPosition)
    {
        _raycastResultBuffer.Clear();

        var eventData = new PointerEventData(eventSystem) { position = screenPosition };
        eventSystem.RaycastAll(eventData, _raycastResultBuffer);

        for (int i = 0; i < _raycastResultBuffer.Count; i++)
        {
            var go = _raycastResultBuffer[i].gameObject;
            if (go != null)
                return go;
        }

        return null;
    }

    /// <summary>
    /// 编辑模式下的兜底命中：Graphic 在编辑模式不执行 OnEnable，GraphicRegistry 为空，
    /// 因此不能直接用 GraphicRaycaster，改为按 Canvas 排序 + 图形几何自行判定。
    /// </summary>
    public static GameObject PickByGraphicGeometry(Vector2 screenPosition)
    {
        return PickByGraphicGeometry(screenPosition, UnityEngine.Object.FindObjectsOfType<Canvas>(true));
    }

    /// <summary>
    /// 在指定的 Canvas 集合上做几何命中（按排序升序覆盖，最后者最上层）。
    /// 与全局版本的区别只是搜索范围，便于回归测试隔离验证，也便于限定只在自己关心的 Canvas 里拾取。
    /// </summary>
    public static GameObject PickByGraphicGeometry(Vector2 screenPosition, Canvas[] canvases)
    {
        if (canvases == null || canvases.Length == 0)
            return null;

        var sorted = (Canvas[])canvases.Clone();
        Array.Sort(sorted, CompareCanvasSorting); // 升序：后处理的覆盖先处理的

        GameObject best = null;

        for (int i = 0; i < sorted.Length; i++)
        {
            var canvas = sorted[i];
            if (canvas == null || !canvas.isActiveAndEnabled)
                continue;

            var canvasCamera = ResolveCanvasCamera(canvas);
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvasCamera == null)
                continue; // 屏幕点无法映射到该 Canvas

            var graphics = canvas.GetComponentsInChildren<Graphic>(true);
            for (int j = 0; j < graphics.Length; j++)
            {
                var graphic = graphics[j];
                if (!IsPickable(graphic))
                    continue;

                if (!RectTransformUtility.RectangleContainsScreenPoint(
                        graphic.rectTransform,
                        screenPosition,
                        canvasCamera,
                        graphic.raycastPadding))
                    continue;

                // GetComponentsInChildren 是先序（父在前、兄弟按层级顺序），越靠后越上层
                best = graphic.gameObject;
            }
        }

        return best;
    }

    private static int CompareCanvasSorting(Canvas a, Canvas b)
    {
        if (a == b)
            return 0;
        if (a == null)
            return -1;
        if (b == null)
            return 1;

        int layerA = SortingLayer.GetLayerValueFromID(a.sortingLayerID);
        int layerB = SortingLayer.GetLayerValueFromID(b.sortingLayerID);
        if (layerA != layerB)
            return layerA.CompareTo(layerB);

        if (a.sortingOrder != b.sortingOrder)
            return a.sortingOrder.CompareTo(b.sortingOrder);

        // 同层同序时给一个稳定顺序，避免每次点击结果抖动
        return a.GetInstanceID().CompareTo(b.GetInstanceID());
    }

    private static Camera ResolveCanvasCamera(Canvas canvas)
    {
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
    }

    private static bool IsPickable(Graphic graphic)
    {
        if (graphic == null || !graphic.raycastTarget || !graphic.isActiveAndEnabled)
            return false;

        // 与 GraphicRaycaster 一致：被上层 CanvasGroup 关掉 raycast 的图形不参与命中
        var parent = graphic.transform.parent;
        while (parent != null)
        {
            var canvasGroup = parent.GetComponent<CanvasGroup>();
            if (canvasGroup != null && !canvasGroup.blocksRaycasts)
                return false;

            parent = parent.parent;
        }

        return true;
    }
}
#endif
