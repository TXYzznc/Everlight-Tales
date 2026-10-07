#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// 字体资源替换工具 - 一键替换对象（含子对象）或文件夹内 Prefab 中所有 Text / TMP_Text 的字体
/// </summary>
[ToolHubItem("UI工具/字体资源替换", "批量替换 Text 和 TextMeshPro 的字体资源", 10)]
public class FontReplacerPanel : IToolHubPanel
{
    private UnityEngine.Object m_Target;
    private Font m_NewFont;
    private TMP_FontAsset m_NewTmpFont;

    private bool m_ReplaceText = true;
    private bool m_ReplaceTmp = true;
    private bool m_IncludeInactive = true;

    private Vector2 m_ScrollPos;
    private List<ResultEntry> m_Results = new List<ResultEntry>();
    private bool m_HasResult;

    private struct ResultEntry
    {
        public string Path;
        public string ComponentType;
        public string OldFont;
        public string NewFont;
    }

    public void OnEnable() { }
    public void OnDisable() { }
    public void OnDestroy() { }

    public string GetHelpText()
    {
        return "将目标对象（含所有子对象）或文件夹内 Prefab 中的 Text 和 TextMeshPro 字体统一替换为指定字体。\n" +
               "文件夹会递归处理其中的 Prefab 资源。操作支持 Undo。";
    }

    public void OnGUI()
    {
        EditorGUILayout.LabelField("字体资源替换工具", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // --- 目标对象 ---
        m_Target = EditorGUILayout.ObjectField(
            new GUIContent("目标对象 / 文件夹"),
            m_Target,
            typeof(UnityEngine.Object),
            true
        );

        if (m_Target != null)
        {
            string targetPath = AssetDatabase.GetAssetPath(m_Target);
            if (IsFolderTarget())
                EditorGUILayout.HelpBox($"将递归处理文件夹中的 Prefab 资源：{targetPath}", MessageType.Info);
            else if (!(m_Target is GameObject))
                EditorGUILayout.HelpBox("目标必须是 GameObject 或 Project 窗口中的文件夹。", MessageType.Warning);
        }

        EditorGUILayout.Space(8);

        // --- 替换范围 ---
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("替换范围", EditorStyles.boldLabel);
            m_ReplaceText = EditorGUILayout.Toggle("Unity Text (UGUI)", m_ReplaceText);
            m_ReplaceTmp = EditorGUILayout.Toggle("TextMeshPro", m_ReplaceTmp);
            m_IncludeInactive = EditorGUILayout.Toggle("包含未激活对象", m_IncludeInactive);
        }

        EditorGUILayout.Space(4);

        // --- 目标字体 ---
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("目标字体", EditorStyles.boldLabel);

            if (m_ReplaceText)
                m_NewFont = (Font)EditorGUILayout.ObjectField("Text 字体", m_NewFont, typeof(Font), false);

            if (m_ReplaceTmp)
                m_NewTmpFont = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP 字体资产", m_NewTmpFont, typeof(TMP_FontAsset), false);
        }

        EditorGUILayout.Space(8);

        // --- 操作按钮 ---
        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.enabled = CanPreview();
            if (GUILayout.Button("预览", GUILayout.Height(28)))
                DoPreview();

            GUI.enabled = CanReplace();
            if (GUILayout.Button("执行替换", GUILayout.Height(28)))
                DoReplace();

            GUI.enabled = true;
        }

        // --- 结果列表 ---
        if (m_HasResult)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField($"匹配结果: {m_Results.Count} 个组件", EditorStyles.boldLabel);

            if (m_Results.Count > 0)
            {
                m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.MaxHeight(300));

                foreach (var r in m_Results)
                {
                    using (new EditorGUILayout.HorizontalScope("box"))
                    {
                        EditorGUILayout.LabelField(r.Path, GUILayout.MinWidth(200));
                        EditorGUILayout.LabelField(r.ComponentType, GUILayout.Width(80));
                        EditorGUILayout.LabelField($"{r.OldFont} → {r.NewFont}", GUILayout.Width(200));
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }

    private bool CanPreview()
    {
        return IsValidTarget() && (m_ReplaceText || m_ReplaceTmp);
    }

    private bool CanReplace()
    {
        if (!IsValidTarget()) return false;
        if (m_ReplaceText && m_NewFont == null && m_ReplaceTmp && m_NewTmpFont == null) return false;
        if (m_ReplaceText && m_NewFont == null && !m_ReplaceTmp) return false;
        if (m_ReplaceTmp && m_NewTmpFont == null && !m_ReplaceText) return false;
        return m_ReplaceText || m_ReplaceTmp;
    }

    private void DoPreview()
    {
        m_Results.Clear();
        m_HasResult = true;
        CollectEntries(false);
    }

    private void DoReplace()
    {
        m_Results.Clear();
        m_HasResult = true;

        Undo.SetCurrentGroupName("字体资源替换");
        int undoGroup = Undo.GetCurrentGroup();

        CollectEntries(true);

        Undo.CollapseUndoOperations(undoGroup);

        if (m_Results.Count > 0)
            EditorUtility.DisplayDialog("字体替换完成", $"已替换 {m_Results.Count} 个组件的字体。", "确定");
        else
            EditorUtility.DisplayDialog("字体替换", "未找到需要替换的组件。", "确定");
    }

    private void CollectEntries(bool apply)
    {
        if (IsFolderTarget())
        {
            string folderPath = AssetDatabase.GetAssetPath(m_Target);
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefabRoot = null;
                try
                {
                    prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                    if (prefabRoot != null)
                        CollectEntriesForRoot(prefabRoot, apply, prefabPath);
                }
                finally
                {
                    if (prefabRoot != null)
                    {
                        if (apply)
                            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
                    }
                }
            }

            if (apply && prefabGuids.Length > 0)
                AssetDatabase.SaveAssets();
            return;
        }

        CollectEntriesForRoot((GameObject)m_Target, apply, null);
        if (apply && PrefabUtility.IsPartOfPrefabAsset(m_Target))
            AssetDatabase.SaveAssets();
    }

    private void CollectEntriesForRoot(GameObject root, bool apply, string resultPrefix)
    {
        if (m_ReplaceText && (apply ? m_NewFont != null : true))
        {
            var texts = root.GetComponentsInChildren<Text>(m_IncludeInactive);
            foreach (var t in texts)
            {
                string oldName = t.font != null ? t.font.name : "(None)";
                string newName = m_NewFont != null ? m_NewFont.name : "(未指定)";

                if (apply && m_NewFont != null && t.font != m_NewFont)
                {
                    Undo.RecordObject(t, "Replace Font");
                    t.font = m_NewFont;
                    EditorUtility.SetDirty(t);
                }

                m_Results.Add(new ResultEntry
                {
                    Path = GetResultPath(t.transform, root, resultPrefix),
                    ComponentType = "Text",
                    OldFont = oldName,
                    NewFont = newName
                });
            }
        }

        if (m_ReplaceTmp && (apply ? m_NewTmpFont != null : true))
        {
            var tmps = root.GetComponentsInChildren<TMP_Text>(m_IncludeInactive);
            foreach (var t in tmps)
            {
                string oldName = t.font != null ? t.font.name : "(None)";
                string newName = m_NewTmpFont != null ? m_NewTmpFont.name : "(未指定)";

                if (apply && m_NewTmpFont != null && t.font != m_NewTmpFont)
                {
                    Undo.RecordObject(t, "Replace TMP Font");
                    t.font = m_NewTmpFont;
                    EditorUtility.SetDirty(t);
                }

                m_Results.Add(new ResultEntry
                {
                    Path = GetResultPath(t.transform, root, resultPrefix),
                    ComponentType = "TMP",
                    OldFont = oldName,
                    NewFont = newName
                });
            }
        }
    }

    private string GetResultPath(Transform t, GameObject root, string resultPrefix)
    {
        string path = GetPath(t, root.transform);
        return string.IsNullOrEmpty(resultPrefix) ? path : $"{resultPrefix}/{path}";
    }

    private static string GetPath(Transform t, Transform root)
    {
        if (t == root)
            return t.name;

        var parts = new List<string>();
        var current = t;
        while (current != null && current != root)
        {
            parts.Add(current.name);
            current = current.parent;
        }
        parts.Reverse();
        return string.Join("/", parts);
    }

    private bool IsFolderTarget()
    {
        if (m_Target == null)
            return false;

        string path = AssetDatabase.GetAssetPath(m_Target);
        return !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path);
    }

    private bool IsValidTarget()
    {
        return m_Target is GameObject || IsFolderTarget();
    }
}
#endif
