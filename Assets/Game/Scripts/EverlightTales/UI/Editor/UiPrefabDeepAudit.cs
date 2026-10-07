#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>
    /// 扫描 UIForm/UIDialog/UIItem 预制体的层级、布局约束、射线层级和常见布局冲突。
    /// 只读扫描，不修改资源；报告写入 Library，供每次 Prefab 变更后的回归检查使用。
    /// </summary>
    public static class UiPrefabDeepAudit
    {
        private const string UiRoot = "Assets/Game/Prefabs/UI";
        private const string ReportPath = "Library/UiPrefabDeepAudit.md";

        [MenuItem("Game Framework/EverlightTales/UI/深度审计全部 UI Prefab", priority = 2090)]
        public static void Run()
        {
            Directory.CreateDirectory("Library");
            var report = new StringBuilder();
            report.AppendLine("# Everlight Tales UI Prefab 深度审计");
            report.AppendLine();
            report.AppendLine("- 基准分辨率：1080×1920");
            report.AppendLine("- 规则：只读扫描；Overlap 为静态近似，需结合 1080×1920 PlayMode 截图复核。");
            report.AppendLine();

            int prefabCount = 0;
            int errorCount = 0;
            int warningCount = 0;
            foreach (string file in Directory.GetFiles(UiRoot, "*.prefab", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    prefabCount++;
                    var issues = new List<Issue>();
                    AuditRoot(root, issues);
                    AuditHierarchy(root.transform, issues);
                    AuditInteractiveOrder(root.transform, issues);
                    AuditLayoutContracts(root.transform, issues);

                    report.AppendLine("## " + path);
                    report.AppendLine();
                    report.AppendLine("- 对象数：" + root.GetComponentsInChildren<Transform>(true).Length);
                    report.AppendLine("- Canvas：" + root.GetComponentsInChildren<Canvas>(true).Length);
                    report.AppendLine("- GraphicRaycaster：" + root.GetComponentsInChildren<GraphicRaycaster>(true).Length);
                    report.AppendLine("- TMP 文本：" + root.GetComponentsInChildren<TMP_Text>(true).Length);
                    report.AppendLine("- LayoutGroup：" + root.GetComponentsInChildren<LayoutGroup>(true).Length);
                    report.AppendLine("- UIItem：" + CountComponents(root, "UIItemBase"));
                    report.AppendLine("- 问题：" + (issues.Count == 0 ? "无" : string.Empty));
                    foreach (Issue issue in issues)
                    {
                        report.AppendLine("  - [" + issue.Level + "] " + issue.Message);
                        if (issue.Level == "ERROR") errorCount++; else warningCount++;
                    }
                    report.AppendLine();
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            report.Insert(0, "- 统计：" + prefabCount + " 个 Prefab，ERROR=" + errorCount + "，WARNING=" + warningCount + "\n\n");
            File.WriteAllText(ReportPath, report.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log("[UiPrefabDeepAudit] 扫描完成：" + prefabCount + " 个 Prefab，ERROR=" + errorCount + "，WARNING=" + warningCount + "。报告：" + ReportPath);
        }

        [MenuItem("Game Framework/EverlightTales/UI/修复装饰引用图层级", priority = 2091)]
        public static void FixReferenceRaycastTargets()
        {
            int changedPrefabs = 0;
            int changedGraphics = 0;
            foreach (string file in Directory.GetFiles(UiRoot, "*.prefab", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    int changed = 0;
                    foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    {
                        if (!child.name.StartsWith("__Reference_EffectImage_", StringComparison.Ordinal)) continue;
                        Graphic graphic = child.GetComponent<Graphic>();
                        if (graphic == null || !graphic.raycastTarget) continue;
                        if (child.GetComponent<Selectable>() != null || child.GetComponent<EventTrigger>() != null || child.GetComponent<ScrollRect>() != null) continue;
                        graphic.raycastTarget = false;
                        changed++;
                    }
                    if (changed == 0) continue;
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    changedPrefabs++;
                    changedGraphics += changed;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UiPrefabDeepAudit] 已关闭 " + changedGraphics + " 个装饰引用图 Raycast Target，涉及 " + changedPrefabs + " 个 Prefab。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/规范化 UIForm 根节点", priority = 2092)]
        public static void NormalizeFormRoots()
        {
            int changed = 0;
            foreach (string file in Directory.GetFiles(UiRoot, "*.prefab", SearchOption.TopDirectoryOnly))
            {
                string path = file.Replace('\\', '/');
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    if (root.GetComponent("UIFormBase") == null) continue;
                    RectTransform rect = root.transform as RectTransform;
                    if (rect == null) continue;
                    bool needsChange = rect.anchorMin != Vector2.zero || rect.anchorMax != Vector2.one ||
                        rect.pivot != new Vector2(0.5f, 0.5f) || rect.offsetMin != Vector2.zero ||
                        rect.offsetMax != Vector2.zero || rect.anchoredPosition != Vector2.zero ||
                        rect.localScale != Vector3.one || rect.localPosition != Vector3.zero;
                    if (!needsChange) continue;
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;
                    rect.anchoredPosition = Vector2.zero;
                    rect.localPosition = Vector3.zero;
                    rect.localScale = Vector3.one;
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    changed++;
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UiPrefabDeepAudit] 已规范化 " + changed + " 个 UIForm 根节点。");
        }

        private static void AuditRoot(GameObject root, List<Issue> issues)
        {
            // UIFormBase/UIItemBase 位于框架程序集，编辑器审计程序集不能直接引用其类型，
            // 使用稳定类型名解析，避免把审计工具耦合到具体程序集定义。
            bool hasForm = root.GetComponent("UIFormBase") != null;
            bool hasItem = root.GetComponent("UIItemBase") != null;
            if (!hasForm && !hasItem)
            {
                issues.Add(new Issue("ERROR", "根节点既不是 UIFormBase 也不是 UIItemBase"));
            }

            if (hasForm)
            {
                RectTransform rect = root.transform as RectTransform;
                if (rect == null || rect.anchorMin != Vector2.zero || rect.anchorMax != Vector2.one ||
                    rect.offsetMin != Vector2.zero || rect.offsetMax != Vector2.zero)
                {
                    issues.Add(new Issue("WARNING", "UIForm 根节点不是 StretchAll/offset=0，可能在不同分辨率出现边缘偏移"));
                }
            }

            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            if (canvases.Length == 0 && !hasItem) issues.Add(new Issue("ERROR", "缺少 Canvas"));
            if (root.GetComponent<GraphicRaycaster>() == null && hasForm)
                issues.Add(new Issue("ERROR", "UIForm 根节点缺少 GraphicRaycaster"));

            foreach (Canvas canvas in canvases)
            {
                if (canvas == root.GetComponent<Canvas>()) continue;
                if (!canvas.overrideSorting)
                    issues.Add(new Issue("WARNING", Path(canvas.transform) + " 存在嵌套 Canvas 但未启用 overrideSorting"));
            }
        }

        private static void AuditHierarchy(Transform root, List<Issue> issues)
        {
            foreach (Transform parent in root.GetComponentsInChildren<Transform>(true))
            {
                var names = new HashSet<string>(StringComparer.Ordinal);
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    if (!names.Add(child.name))
                        issues.Add(new Issue("WARNING", Path(child) + " 与同级节点重名，运行时 Find/绑定可能不稳定"));
                }

                ScrollRect scroll = parent.GetComponent<ScrollRect>();
                if (scroll == null) continue;
                if (scroll.viewport == null) issues.Add(new Issue("ERROR", Path(parent) + " ScrollRect 缺少 viewport"));
                if (scroll.content == null) issues.Add(new Issue("ERROR", Path(parent) + " ScrollRect 缺少 content"));
                if (scroll.horizontal && scroll.vertical)
                    issues.Add(new Issue("WARNING", Path(parent) + " 同时开启横向和纵向滚动，需确认是否为刻意设计"));
            }
        }

        private static int CountComponents(GameObject root, string typeName)
        {
            int count = 0;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.GetComponent(typeName) != null) count++;
            }
            return count;
        }

        private static void AuditLayoutContracts(Transform root, List<Issue> issues)
        {
            foreach (Transform current in root.GetComponentsInChildren<Transform>(true))
            {
                LayoutGroup group = current.GetComponent<LayoutGroup>();
                ContentSizeFitter fitter = current.GetComponent<ContentSizeFitter>();
                bool isScrollContent = false;
                if (fitter != null && group != null)
                {
                    foreach (ScrollRect scroll in root.GetComponentsInChildren<ScrollRect>(true))
                    {
                        if (scroll.content == current) { isScrollContent = true; break; }
                    }
                }
                if (group != null && fitter != null && !isScrollContent)
                    issues.Add(new Issue("WARNING", Path(current) + " 同时持有 LayoutGroup 与 ContentSizeFitter，可能触发布局重建循环"));

                TMP_Text text = current.GetComponent<TMP_Text>();
                if (text != null && text.raycastTarget)
                    issues.Add(new Issue("WARNING", Path(current) + " 文本开启 Raycast Target，可能遮挡下层按钮；纯展示文本应关闭"));

                EventTrigger trigger = current.GetComponent<EventTrigger>();
                if (trigger != null && current.GetComponent<Graphic>() == null)
                    issues.Add(new Issue("WARNING", Path(current) + " EventTrigger 所在节点没有 Graphic，输入命中依赖父节点，建议明确绑定"));
            }
        }

        private static void AuditInteractiveOrder(Transform root, List<Issue> issues)
        {
            foreach (Transform parent in root.GetComponentsInChildren<Transform>(true))
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform first = parent.GetChild(i);
                    Graphic firstGraphic = first.GetComponent<Graphic>();
                    if (firstGraphic == null || !firstGraphic.raycastTarget) continue;
                    Button firstButton = first.GetComponent<Button>();
                    if (firstButton != null) continue;
                    if (!TryGetFixedRect(first as RectTransform, out Rect firstRect)) continue;

                    for (int j = i + 1; j < parent.childCount; j++)
                    {
                        Transform later = parent.GetChild(j);
                        Button laterButton = later.GetComponent<Button>();
                        if (laterButton == null) continue;
                        if (!TryGetFixedRect(later as RectTransform, out Rect laterRect)) continue;
                        Rect overlap = Intersect(firstRect, laterRect);
                        float area = overlap.width * overlap.height;
                        float buttonArea = laterRect.width * laterRect.height;
                        if (buttonArea > 0f && area / buttonArea > 0.8f)
                            issues.Add(new Issue("WARNING", Path(first) + " 的 RaycastTarget 覆盖后续按钮 " + Path(later) + "，可能阻断点击"));
                    }
                }
            }
        }

        private static bool TryGetFixedRect(RectTransform rect, out Rect result)
        {
            result = default(Rect);
            if (rect == null || rect.anchorMin != rect.anchorMax) return false;
            Vector2 size = rect.sizeDelta;
            if (size.x <= 0f || size.y <= 0f) return false;
            Vector2 min = rect.anchoredPosition - Vector2.Scale(size, rect.pivot);
            result = new Rect(min, size);
            return true;
        }

        private static Rect Intersect(Rect a, Rect b)
        {
            float xMin = Mathf.Max(a.xMin, b.xMin);
            float yMin = Mathf.Max(a.yMin, b.yMin);
            float xMax = Mathf.Min(a.xMax, b.xMax);
            float yMax = Mathf.Min(a.yMax, b.yMax);
            return xMax <= xMin || yMax <= yMin ? new Rect() : Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private static string Path(Transform target)
        {
            if (target == null) return "<null>";
            var names = new Stack<string>();
            for (Transform current = target; current != null; current = current.parent) names.Push(current.name);
            return string.Join("/", names.ToArray());
        }

        private readonly struct Issue
        {
            public readonly string Level;
            public readonly string Message;

            public Issue(string level, string message)
            {
                Level = level;
                Message = message;
            }
        }
    }
}
#endif
