#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>审计 UI 预制体中可交互控件的射线目标配置，不自动修改资源。</summary>
    public static class UiInteractiveRaycastAudit
    {
        private const string UiRoot = "Assets/Game/Prefabs/UI";

        [MenuItem("Game Framework/EverlightTales/UI/Fix Interactive Raycasts", priority = 2097)]
        public static void Fix()
        {
            int changedPrefabs = 0;
            int changedTargets = 0;
            foreach (string file in Directory.GetFiles(UiRoot, "*.prefab", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    int changedInPrefab = 0;
                    foreach (Selectable selectable in root.GetComponentsInChildren<Selectable>(true))
                    {
                        Graphic target = selectable.targetGraphic;
                        if (target == null)
                        {
                            Debug.LogWarning($"[UiInteractiveRaycastAudit] {path}: {Hierarchy(selectable.transform, root.transform)} 没有 targetGraphic，需人工选择按钮图形。");
                            continue;
                        }
                        if (target.raycastTarget) continue;
                        target.raycastTarget = true;
                        changedInPrefab++;
                    }

                    foreach (ScrollRect scroll in root.GetComponentsInChildren<ScrollRect>(true))
                    {
                        if (scroll.viewport == null) continue;
                        Graphic graphic = scroll.viewport.GetComponent<Graphic>();
                        if (graphic == null)
                        {
                            Image image = scroll.viewport.gameObject.AddComponent<Image>();
                            image.color = Color.clear;
                            image.raycastTarget = true;
                            graphic = image;
                            changedInPrefab++;
                            continue;
                        }
                        if (graphic.raycastTarget) continue;
                        graphic.raycastTarget = true;
                        changedInPrefab++;
                    }

                    if (changedInPrefab == 0) continue;
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    changedPrefabs++;
                    changedTargets += changedInPrefab;
                    Debug.Log($"[UiInteractiveRaycastAudit] 修复 {path}: {changedInPrefab} 处。");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[UiInteractiveRaycastAudit] 修复完成：{changedPrefabs} 个预制体，{changedTargets} 处射线目标。");
        }

        [MenuItem("Game Framework/EverlightTales/UI/Audit Interactive Raycasts", priority = 2096)]
        public static void Run()
        {
            var report = new StringBuilder();
            int prefabCount = 0;
            int issueCount = 0;
            foreach (string file in Directory.GetFiles(UiRoot, "*.prefab", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    prefabCount++;
                    foreach (Selectable selectable in root.GetComponentsInChildren<Selectable>(true))
                    {
                        Graphic target = selectable.targetGraphic;
                        if (target == null)
                        {
                            report.AppendLine($"{path}: {Hierarchy(selectable.transform, root.transform)} [{selectable.GetType().Name}] targetGraphic 缺失");
                            issueCount++;
                        }
                        else if (!target.raycastTarget)
                        {
                            report.AppendLine($"{path}: {Hierarchy(selectable.transform, root.transform)} [{selectable.GetType().Name}] targetGraphic={target.gameObject.name} Raycast Target 未勾选");
                            issueCount++;
                        }
                    }

                    foreach (ScrollRect scroll in root.GetComponentsInChildren<ScrollRect>(true))
                    {
                        if (scroll.viewport == null)
                        {
                            report.AppendLine($"{path}: {Hierarchy(scroll.transform, root.transform)} [ScrollRect] viewport 缺失");
                            issueCount++;
                            continue;
                        }
                        Graphic graphic = scroll.viewport.GetComponent<Graphic>();
                        if (graphic == null || !graphic.raycastTarget)
                        {
                            report.AppendLine($"{path}: {Hierarchy(scroll.transform, root.transform)} [ScrollRect] viewport={scroll.viewport.name} Raycast Target 未启用");
                            issueCount++;
                        }
                    }

                    foreach (EventTrigger trigger in root.GetComponentsInChildren<EventTrigger>(true))
                    {
                        Graphic graphic = trigger.GetComponent<Graphic>();
                        if (graphic == null || !graphic.raycastTarget)
                        {
                            report.AppendLine($"{path}: {Hierarchy(trigger.transform, root.transform)} [EventTrigger] Graphic Raycast Target 未启用");
                            issueCount++;
                        }
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            foreach (string line in report.ToString().Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries))
            {
                Debug.Log("[UiInteractiveRaycastAuditIssue] " + line);
            }
            Debug.Log($"[UiInteractiveRaycastAudit] 扫描 {prefabCount} 个 UI 预制体，发现 {issueCount} 个交互射线问题。");
        }

        private static string Hierarchy(Transform target, Transform root)
        {
            var names = new Stack<string>();
            for (Transform current = target; current != null && current != root; current = current.parent)
            {
                names.Push(current.name);
            }
            return string.Join("/", names);
        }
    }
}
#endif
