#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity开发工具箱面板：Game 视图 UI 中键快速选中。
/// 面板只提供开关与状态显示，实际逻辑在 <see cref="GameViewUiQuickSelect"/>，开关持久化在 EditorPrefs。
/// </summary>
[ToolHubItem("UI工具/Game视图 UI 中键选中", "鼠标中键在 Game 视图中点击 UI 元素，直接在 Hierarchy 中选中该对象（开关可持久化）", 2)]
public sealed class GameViewUiQuickSelectPanel : IToolHubPanel
{
    public void OnEnable()
    {
        // 兜底同步一次：开关可能是上次会话留下的，或由菜单切换过
        GameViewUiQuickSelect.SyncHookState();
    }

    public void OnDisable() { }

    public void OnDestroy() { }

    public string GetHelpText() =>
        "1) 打开开关，鼠标放到 Game 视图上，按鼠标中键点击任意 UI 元素，Hierarchy 立即选中该对象。\n" +
        "2) 开关状态写在 EditorPrefs，按用户/机器保存，重启编辑器后保持。\n" +
        "3) Play 模式命中结果与运行时 UI 事件一致；编辑模式按 Canvas 排序 + 图形几何命中。\n" +
        "4) 仅对 UGUI（Image / Text / TextMeshPro 等 Graphic 元素）生效，不支持 UI Toolkit 与 IMGUI。";

    public void OnGUI()
    {
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Game 视图 UI 中键快速选中", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            bool enabled = GameViewUiQuickSelect.Enabled;
            bool newValue = EditorGUILayout.ToggleLeft(
                new GUIContent(
                    "启用：鼠标中键在 Game 视图中点击 UI → 在层级中选中该对象",
                    "开关写入 EditorPrefs，跨编辑器会话保持"),
                enabled);

            if (newValue != enabled)
                GameViewUiQuickSelect.Enabled = newValue;

            EditorGUILayout.Space(6);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("当前状态", GameViewUiQuickSelect.Enabled ? "已开启" : "已关闭");
                EditorGUILayout.LabelField("最近一次", GameViewUiQuickSelect.LastPickInfo);
                EditorGUILayout.LabelField("持久化键", GameViewUiQuickSelect.EnabledPrefKey);
            }

            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(GameViewUiQuickSelect.LastPicked == null))
                {
                    if (GUILayout.Button("重新选中最近一次命中的对象", GUILayout.Height(22)))
                    {
                        Selection.activeGameObject = GameViewUiQuickSelect.LastPicked;
                        EditorGUIUtility.PingObject(GameViewUiQuickSelect.LastPicked);
                    }
                }

                if (GUILayout.Button("打开/关闭（同菜单项）", GUILayout.Height(22), GUILayout.Width(170)))
                    GameViewUiQuickSelect.Enabled = !GameViewUiQuickSelect.Enabled;
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(
                "使用说明\n" +
                "· 只在鼠标位于 Game 视图内、按鼠标中键时触发；Scene 视图与其它窗口不受影响。\n" +
                "· Play 模式下若场景存在启用的 EventSystem，命中结果与运行时 UI 事件完全一致。\n" +
                "· 编辑模式下没有 EventSystem（Graphic 未注册），工具按 Canvas 排序 + 图形几何自行命中。\n" +
                "· 仅支持 UGUI；UI Toolkit（UIElements）与 IMGUI 不在范围内。",
                MessageType.Info);
        }
    }
}
#endif
