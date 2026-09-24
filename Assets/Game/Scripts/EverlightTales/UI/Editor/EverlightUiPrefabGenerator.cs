using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.EditorTools
{
    /// <summary>
    /// Builds a UIForm prefab from a page contract JSON (schemaVersion 3).
    ///
    /// 契约（Docs/Development/UI-PrefabLayouts/<Form>.contract.json）是节点树、RectTransform、
    /// 组件、颜色、布局参数与 Form 序列化字段的单一事实来源。绑定按「契约节点路径」解析，
    /// 从不按名字搜索层级——这让跨页错绑在写法上就不可能发生。
    ///
    /// 本工具只 BUILD 不 validate：命名/结构/射线/绑定合规由 GF-UI-Standards 的门检查另行判定，
    /// 生成成功不等于合规通过。
    ///
    /// 与 stellar-frontier 版的差异：Everlight 的 UIForm 根自带 Canvas + GraphicRaycaster
    /// （UIFormBase.OnInit 也会兜底添加），且根 scale=(0,0,0) 是模板常态、框架实例化时重置为
    /// (1,1,1)（DefaultUIFormHelper.CreateUIForm）。基准分辨率竖屏 1080×1920，字体走
    /// TMP_Settings.defaultFontAsset（思源黑体 SIMHEI SDF）。
    /// </summary>
    public static class EverlightUiPrefabGenerator
    {
        /// <summary>Directory holding one `&lt;Form&gt;.contract.json` per page.</summary>
        public const string ContractDirectory = "Docs/Development/UI-PrefabLayouts";

        /// <summary>Default contract for the single-file menu entries.</summary>
        public const string DefaultContractPath =
            "Docs/Development/UI-PrefabLayouts/PreparationPageForm.contract.json";

        private static readonly Regex SegmentPattern =
            new Regex(@"^(?<field>[A-Za-z0-9_]+)(?:\[(?<index>\d+)\])?$", RegexOptions.Compiled);

        /// <summary>
        /// Explicit UGUI/TMP types. Short-name lookup is ambiguous — several unrelated
        /// types are also called `Image` — so these must never be resolved by name scan.
        /// </summary>
        private static readonly Dictionary<string, Type> WellKnownTypes =
            new Dictionary<string, Type>(StringComparer.Ordinal)
            {
                { "Image", typeof(Image) },
                { "RawImage", typeof(RawImage) },
                { "Button", typeof(Button) },
                { "Toggle", typeof(Toggle) },
                { "Slider", typeof(Slider) },
                { "ScrollRect", typeof(ScrollRect) },
                { "Mask", typeof(Mask) },
                { "RectMask2D", typeof(RectMask2D) },
                { "HorizontalLayoutGroup", typeof(HorizontalLayoutGroup) },
                { "VerticalLayoutGroup", typeof(VerticalLayoutGroup) },
                { "GridLayoutGroup", typeof(GridLayoutGroup) },
                { "ContentSizeFitter", typeof(ContentSizeFitter) },
                { "LayoutElement", typeof(LayoutElement) },
                { "CanvasGroup", typeof(CanvasGroup) },
                { "CanvasRenderer", typeof(CanvasRenderer) },
                { "Canvas", typeof(Canvas) },
                { "GraphicRaycaster", typeof(GraphicRaycaster) },
                { "TextMeshProUGUI", typeof(TextMeshProUGUI) },
                { "TextMeshPro", typeof(TextMeshPro) },
            };

        private static Dictionary<string, Type> _typeIndex;

        // ---------------------------------------------------------------- menus

        [MenuItem("Game Framework/EverlightTales/UI/从契约重建预制体", priority = 2000)]
        public static void RebuildFromDefaultContract()
        {
            Rebuild(DefaultContractPath);
        }

        [MenuItem("Game Framework/EverlightTales/UI/按契约刷新绑定（不改结构）", priority = 2001)]
        public static void RefreshBindingsFromDefaultContract()
        {
            RefreshBindingsOnly(DefaultContractPath);
        }

        /// <summary>Rebuild every page that ships a contract, one file per page.</summary>
        [MenuItem("Game Framework/EverlightTales/UI/从契约重建所有页面", priority = 2002)]
        public static void RebuildAllContracts()
        {
            string[] contracts = Directory.GetFiles(ContractDirectory, "*.contract.json");
            if (contracts.Length == 0)
            {
                Debug.LogWarning($"[EverlightUiPrefabGenerator] 未找到任何契约：{ContractDirectory}");
                return;
            }
            foreach (string contract in contracts)
            {
                Rebuild(contract.Replace('\\', '/'));
            }
            Debug.Log($"[EverlightUiPrefabGenerator] 已按 {contracts.Length} 份契约重建页面。");
        }

        /// <summary>Refresh the serialized fields of every page that ships a contract, without
        /// touching structure or appearance.</summary>
        [MenuItem("Game Framework/EverlightTales/UI/按契约刷新所有页面绑定（不改结构）", priority = 2003)]
        public static void RefreshAllBindings()
        {
            string[] contracts = Directory.GetFiles(ContractDirectory, "*.contract.json");
            if (contracts.Length == 0)
            {
                Debug.LogWarning($"[EverlightUiPrefabGenerator] 未找到任何契约：{ContractDirectory}");
                return;
            }

            int unresolvedTotal = 0;
            foreach (string contract in contracts)
            {
                unresolvedTotal += RefreshBindingsOnly(contract.Replace('\\', '/'));
            }

            Debug.Log($"[EverlightUiPrefabGenerator] 已按 {contracts.Length} 份契约刷新绑定" +
                      $"（未解析 {unresolvedTotal}）。");
        }

        // ------------------------------------------------------------- rebuild

        /// <summary>Rebuild the whole prefab from a contract. Structure AND bindings.</summary>
        public static void Rebuild(string contractPath)
        {
            JObject doc = LoadContract(contractPath);
            string prefabPath = RequireString(doc, "prefabPath");
            var rootNode = (JObject)doc["root"];
            string rootName = RequireString(rootNode, "name");
            string contractRoot = rootName;

            var index = new Dictionary<string, GameObject>(StringComparer.Ordinal);
            GameObject root = null;
            try
            {
                root = BuildNode(rootNode, null);
                IndexPaths(root, null, index);

                // Everlight UIForm 模板常态：根 scale=(0,0,0)，框架实例化时重置为 (1,1,1)。
                root.transform.localScale = Vector3.zero;

                var form = EnsureFormComponent(root, doc);
                ApplyBindings(doc, form, index, rootName);
                ApplyScriptFields(doc, index, contractRoot, rootName);
                SavePrefab(root, prefabPath);
                Debug.Log($"[EverlightUiPrefabGenerator] 已按契约重建预制体：{prefabPath}" +
                          $"（节点 {index.Count}，绑定 {((JArray)doc["bindings"])?.Count ?? 0}，" +
                          $"标量 {((JArray)doc["scalars"])?.Count ?? 0}）");
            }
            finally
            {
                if (root != null)
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        /// <summary>
        /// Refresh only the Form's serialized fields on an existing prefab, leaving
        /// structure and appearance untouched.
        /// </summary>
        public static int RefreshBindingsOnly(string contractPath)
        {
            JObject doc = LoadContract(contractPath);
            string prefabPath = RequireString(doc, "prefabPath");
            string rootName = RequireString((JObject)doc["root"], "name");
            string contractRoot = (string)((JObject)doc["root"])["name"];

            GameObject contents = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var index = new Dictionary<string, GameObject>(StringComparer.Ordinal);
                IndexPaths(contents, null, index);
                var form = EnsureFormComponent(contents, doc);
                int unresolved = ApplyBindings(doc, form, index, rootName);
                ApplyScriptFields(doc, index, contractRoot, rootName);
                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"[EverlightUiPrefabGenerator] 已刷新绑定：{prefabPath}（未解析 {unresolved}）");
                if (unresolved > 0)
                {
                    Debug.LogWarning($"[EverlightUiPrefabGenerator] 有 {unresolved} 条绑定未解析；" +
                                     "契约里的节点路径与实际层级不一致，请先修正契约。");
                }

                return unresolved;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        // ---------------------------------------------------------- node build

        private static GameObject BuildNode(JObject node, Transform parent)
        {
            string name = RequireString(node, "name");
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            var rect = go.GetComponent<RectTransform>();
            ApplyRect(rect, node);

            if (node["active"] != null && !node["active"].Value<bool>())
            {
                go.SetActive(false);
            }

            if (node["components"] is JArray comps)
            {
                foreach (JObject spec in comps)
                {
                    AddComponent(go, spec);
                }

                // targetGraphic 是「同节点 Image」这一结构约定的兑现点。所有组件落地后统一兜底，
                // 结构约定才与组件顺序无关。
                foreach (Button button in go.GetComponents<Button>())
                {
                    if (button.targetGraphic == null)
                    {
                        button.targetGraphic = go.GetComponent<Graphic>();
                    }
                }
            }

            if (node["children"] is JArray children)
            {
                foreach (JObject child in children)
                {
                    BuildNode(child, go.transform);
                }
            }

            return go;
        }

        private static void ApplyRect(RectTransform rect, JObject node)
        {
            if (node["anchorMin"] != null) rect.anchorMin = ToVector2((JArray)node["anchorMin"]);
            if (node["anchorMax"] != null) rect.anchorMax = ToVector2((JArray)node["anchorMax"]);
            if (node["pivot"] != null) rect.pivot = ToVector2((JArray)node["pivot"]);
            if (node["sizeDelta"] != null) rect.sizeDelta = ToVector2((JArray)node["sizeDelta"]);
            if (node["anchoredPosition"] != null) rect.anchoredPosition = ToVector2((JArray)node["anchoredPosition"]);
        }

        private static void AddComponent(GameObject go, JObject spec)
        {
            string typeName = RequireString(spec, "type");
            Type type = ResolveType(typeName);
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                Debug.LogWarning($"[EverlightUiPrefabGenerator] 未知或非组件类型：{typeName}（节点 {go.name}）");
                return;
            }

            // `??` compares by REFERENCE, not through UnityEngine.Object's overloaded `==`.
            Component component = go.GetComponent(type);
            if (component == null)
            {
                component = go.AddComponent(type);
            }
            if (component == null)
            {
                Debug.LogWarning($"[EverlightUiPrefabGenerator] 无法添加组件 {typeName}（节点 {go.name}）");
                return;
            }
            ApplyComponent(component, spec, go);
        }

        private static void ApplyComponent(Component component, JObject spec, GameObject go)
        {
            switch (component)
            {
                case Image image:
                {
                    string spritePath = Str(spec, "spritePath");
                    if (!string.IsNullOrEmpty(spritePath))
                    {
                        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                        if (image.sprite == null)
                        {
                            Debug.LogWarning($"[EverlightUiPrefabGenerator] sprite 未找到：{spritePath}（节点 {go.name}）");
                        }
                    }
                    if (Val(spec, "imageType") != null) image.type = (Image.Type)spec["imageType"].Value<int>();
                    if (Val(spec, "preserveAspect") != null) image.preserveAspect = spec["preserveAspect"].Value<bool>();
                    if (Val(spec, "fillMethod") != null) image.fillMethod = (Image.FillMethod)spec["fillMethod"].Value<int>();
                    if (Val(spec, "fillAmount") != null) image.fillAmount = spec["fillAmount"].Value<float>();
                    if (Val(spec, "color") is JArray imageColor) image.color = ToColor(imageColor);
                    if (Val(spec, "raycastTarget") != null) image.raycastTarget = spec["raycastTarget"].Value<bool>();
                    break;
                }

                case TMP_Text tmp:
                {
                    string fontPath = Str(spec, "fontPath");
                    if (!string.IsNullOrEmpty(fontPath))
                    {
                        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
                        if (fontAsset != null)
                        {
                            tmp.font = fontAsset;
                        }
                        else
                        {
                            Debug.LogWarning($"[EverlightUiPrefabGenerator] 字体未找到：{fontPath}（节点 {go.name}）");
                        }
                    }
                    else
                    {
                        // Everlight 字体约定：无显式字体时用 TMP 默认字体（思源黑体 SIMHEI SDF）。
                        tmp.font = TMP_Settings.defaultFontAsset;
                    }
                    if (Val(spec, "text") != null) tmp.text = spec["text"].Value<string>();
                    if (Val(spec, "fontSize") != null) tmp.fontSize = spec["fontSize"].Value<float>();
                    if (Val(spec, "color") is JArray textColor) tmp.color = ToColor(textColor);
                    if (Val(spec, "hAlign") != null) tmp.horizontalAlignment = (HorizontalAlignmentOptions)spec["hAlign"].Value<int>();
                    if (Val(spec, "vAlign") != null) tmp.verticalAlignment = (VerticalAlignmentOptions)spec["vAlign"].Value<int>();
                    if (Val(spec, "wordWrap") != null) tmp.enableWordWrapping = spec["wordWrap"].Value<bool>();
                    if (Val(spec, "raycastTarget") != null) tmp.raycastTarget = spec["raycastTarget"].Value<bool>();
                    break;
                }

                case Button button:
                    // The doc requires the Button and its Image on the SAME node.
                    button.targetGraphic = go.GetComponent<Graphic>();
                    if (Val(spec, "transition") != null) button.transition = (Selectable.Transition)spec["transition"].Value<int>();
                    if (Val(spec, "interactable") != null) button.interactable = spec["interactable"].Value<bool>();
                    break;

                case HorizontalOrVerticalLayoutGroup layout:
                    if (Val(spec, "spacing") != null) layout.spacing = spec["spacing"].Value<float>();
                    if (Val(spec, "padding") is JArray layoutPadding) layout.padding = ToRectOffset(layoutPadding);
                    if (Val(spec, "childAlignment") != null) layout.childAlignment = (TextAnchor)spec["childAlignment"].Value<int>();
                    if (Val(spec, "ChildForceExpandWidth") != null) layout.childForceExpandWidth = spec["ChildForceExpandWidth"].Value<bool>();
                    if (Val(spec, "ChildForceExpandHeight") != null) layout.childForceExpandHeight = spec["ChildForceExpandHeight"].Value<bool>();
                    if (Val(spec, "ChildControlWidth") != null) layout.childControlWidth = spec["ChildControlWidth"].Value<bool>();
                    if (Val(spec, "ChildControlHeight") != null) layout.childControlHeight = spec["ChildControlHeight"].Value<bool>();
                    break;

                case GridLayoutGroup grid:
                    if (Val(spec, "cellSize") is JArray cell) grid.cellSize = ToVector2(cell);
                    if (Val(spec, "spacing") is JArray gridSpacing) grid.spacing = ToVector2(gridSpacing);
                    if (Val(spec, "padding") is JArray gridPadding) grid.padding = ToRectOffset(gridPadding);
                    if (Val(spec, "constraint") != null) grid.constraint = (GridLayoutGroup.Constraint)spec["constraint"].Value<int>();
                    if (Val(spec, "constraintCount") != null) grid.constraintCount = spec["constraintCount"].Value<int>();
                    break;

                case ContentSizeFitter fitter:
                    if (Val(spec, "horizontalFit") != null) fitter.horizontalFit = (ContentSizeFitter.FitMode)spec["horizontalFit"].Value<int>();
                    if (Val(spec, "verticalFit") != null) fitter.verticalFit = (ContentSizeFitter.FitMode)spec["verticalFit"].Value<int>();
                    break;

                // 布局组会用自己的 ILayoutElement 查询覆盖子节点的 anchor/sizeDelta，
                // 所以「组内的孩子」唯一有效的尺寸声明就在这里。
                case LayoutElement element:
                    if (Val(spec, "ignoreLayout") != null) element.ignoreLayout = spec["ignoreLayout"].Value<bool>();
                    if (Val(spec, "minWidth") != null) element.minWidth = spec["minWidth"].Value<float>();
                    if (Val(spec, "minHeight") != null) element.minHeight = spec["minHeight"].Value<float>();
                    if (Val(spec, "preferredWidth") != null) element.preferredWidth = spec["preferredWidth"].Value<float>();
                    if (Val(spec, "preferredHeight") != null) element.preferredHeight = spec["preferredHeight"].Value<float>();
                    if (Val(spec, "flexibleWidth") != null) element.flexibleWidth = spec["flexibleWidth"].Value<float>();
                    if (Val(spec, "flexibleHeight") != null) element.flexibleHeight = spec["flexibleHeight"].Value<float>();
                    if (Val(spec, "layoutPriority") != null) element.layoutPriority = spec["layoutPriority"].Value<int>();
                    break;

                case ScrollRect scroll:
                    if (Val(spec, "horizontal") != null) scroll.horizontal = spec["horizontal"].Value<bool>();
                    if (Val(spec, "vertical") != null) scroll.vertical = spec["vertical"].Value<bool>();
                    if (Val(spec, "movementType") != null) scroll.movementType = (ScrollRect.MovementType)spec["movementType"].Value<int>();
                    break;

                case CanvasGroup group:
                    if (Val(spec, "alpha") != null) group.alpha = spec["alpha"].Value<float>();
                    if (Val(spec, "interactable") != null) group.interactable = spec["interactable"].Value<bool>();
                    if (Val(spec, "blocksRaycasts") != null) group.blocksRaycasts = spec["blocksRaycasts"].Value<bool>();
                    break;

                case Canvas canvas:
                    if (Val(spec, "renderMode") != null) canvas.renderMode = (RenderMode)spec["renderMode"].Value<int>();
                    break;
            }
        }

        /// <summary>A JSON `null` is still a JToken; every optional field goes through this.</summary>
        private static JToken Val(JObject spec, string key)
        {
            JToken token = spec[key];
            return token == null || token.Type == JTokenType.Null ? null : token;
        }

        private static string Str(JObject spec, string key)
        {
            return Val(spec, key)?.Value<string>();
        }

        /// <summary>Load an external asset so it actually fits the target field.</summary>
        private static UnityEngine.Object LoadAssetFor(SerializedProperty property, string assetPath)
        {
            Type wanted = null;
            string declared = property.type;                 // e.g. "PPtr<$Sprite>"
            int open = declared.IndexOf('<');
            int close = declared.IndexOf('>');
            if (open >= 0 && close > open)
            {
                string name = declared.Substring(open + 1, close - open - 1).TrimStart('$');
                wanted = ResolveType(name);
            }

            if (wanted != null)
            {
                UnityEngine.Object typed = AssetDatabase.LoadAssetAtPath(assetPath, wanted);
                if (typed != null) return typed;
            }

            foreach (UnityEngine.Object candidate in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (candidate != null && (wanted == null || wanted.IsInstanceOfType(candidate)))
                {
                    return candidate;
                }
            }
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
        }

        /// <summary>Register every node by its full path from the root, plus its root-relative path.</summary>
        private static void IndexPaths(GameObject go, string prefix, Dictionary<string, GameObject> index)
        {
            string full = string.IsNullOrEmpty(prefix) ? go.name : prefix + "/" + go.name;
            index[full] = go;
            foreach (Transform child in go.transform)
            {
                IndexPaths(child.gameObject, full, index);
            }
        }

        /// <summary>Wire each ScrollRect to its Viewport_/Content_ descendants, if present.</summary>
        private static void WireScrollRects(GameObject root)
        {
            foreach (var scroll in root.GetComponentsInChildren<ScrollRect>(true))
            {
                Transform viewport = FindDescendant(scroll.transform, "Viewport_");
                Transform content = FindDescendant(scroll.transform, "Content_");
                if (viewport != null) scroll.viewport = viewport as RectTransform;
                if (content != null) scroll.content = content as RectTransform;
            }
        }

        private static Transform FindDescendant(Transform parent, string prefix)
        {
            foreach (Transform child in parent)
            {
                if (child.name.StartsWith(prefix, StringComparison.Ordinal)) return child;
                Transform hit = FindDescendant(child, prefix);
                if (hit != null) return hit;
            }
            return null;
        }

        // ------------------------------------------------------------- bindings

        private static Component EnsureFormComponent(GameObject root, JObject doc)
        {
            string formName = RequireString(doc, "form");
            Type formType = ResolveType(formName);
            if (formType == null)
            {
                throw new InvalidOperationException($"契约声明的 Form 类型不存在：{formName}");
            }
            return root.GetComponent(formType) ?? root.AddComponent(formType);
        }

        /// <summary>Apply every binding and scalar. Returns the number of unresolved references.</summary>
        private static int ApplyBindings(JObject doc, Component form, Dictionary<string, GameObject> index, string rootName)
        {
            var serialized = new SerializedObject(form);
            string contractRoot = (string)((JObject)doc["root"])["name"];
            int unresolved = 0;

            if (doc["bindings"] is JArray bindings)
            {
                foreach (JObject binding in bindings)
                {
                    string path = RequireString(binding, "path");
                    string nodePath = (string)binding["node"];
                    string kind = (string)binding["kind"];

                    SerializedProperty property = ResolveProperty(serialized, path);
                    if (property == null)
                    {
                        Debug.LogWarning($"[EverlightUiPrefabGenerator] 绑定字段不存在：{path}");
                        unresolved++;
                        continue;
                    }

                    UnityEngine.Object target;
                    string assetPath = (string)binding["asset"];
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        target = LoadAssetFor(property, assetPath);
                        if (target == null)
                        {
                            Debug.LogWarning($"[EverlightUiPrefabGenerator] 外部资源未找到：{path} -> {assetPath}");
                            unresolved++;
                        }
                    }
                    else
                    {
                        target = ResolveTarget(index, contractRoot, rootName, nodePath, kind);
                        if (target == null)
                        {
                            Debug.LogWarning($"[EverlightUiPrefabGenerator] 绑定目标未解析：{path} -> {nodePath} ({kind})");
                            unresolved++;
                        }
                    }
                    property.objectReferenceValue = target;
                }
            }

            if (doc["scalars"] is JArray scalars)
            {
                foreach (JObject scalar in scalars)
                {
                    SerializedProperty property = ResolveProperty(serialized, RequireString(scalar, "path"));
                    if (property == null) continue;
                    ApplyScalar(property, scalar["value"]);
                }
            }

            if (doc["formArrays"] is JObject formArrays)
            {
                foreach (var pair in formArrays)
                {
                    SerializedProperty property = ResolveProperty(serialized, pair.Key);
                    if (property == null || !property.isArray) continue;
                    var items = (JArray)pair.Value;
                    property.arraySize = items.Count;
                    for (int i = 0; i < items.Count; i++)
                    {
                        SerializedProperty element = property.GetArrayElementAtIndex(i);
                        if (items[i] is JObject spec)
                        {
                            element.objectReferenceValue = ResolveTarget(
                                index, contractRoot, rootName, (string)spec["node"], (string)spec["kind"]);
                        }
                        else
                        {
                            ApplyScalar(element, items[i]);
                        }
                    }
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return unresolved;
        }

        private static void ApplyScalar(SerializedProperty property, JToken value)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    property.objectReferenceValue = null;
                }
                return;
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = value.Value<int>();
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = value.Value<int>();
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = value.Value<float>();
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = value.Value<bool>();
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = value.Value<string>();
                    break;
                case SerializedPropertyType.Color:
                    if (value is JArray arr) property.colorValue = ToColor(arr);
                    break;
            }
        }

        /// <summary>Resolve `_arr[0]._field` style paths, growing arrays as needed.</summary>
        private static SerializedProperty ResolveProperty(SerializedObject serialized, string path)
        {
            SerializedProperty property = null;
            foreach (string segment in path.Split('.'))
            {
                Match match = SegmentPattern.Match(segment);
                if (!match.Success) return null;

                string field = match.Groups["field"].Value;
                property = property == null
                    ? serialized.FindProperty(field)
                    : property.FindPropertyRelative(field);
                if (property == null) return null;

                if (match.Groups["index"].Success)
                {
                    int index = int.Parse(match.Groups["index"].Value);
                    if (!property.isArray) return null;
                    if (property.arraySize <= index) property.arraySize = index + 1;
                    property = property.GetArrayElementAtIndex(index);
                }
            }
            return property;
        }

        private static UnityEngine.Object ResolveTarget(
            Dictionary<string, GameObject> index, string contractRoot, string rootName, string nodePath, string kind)
        {
            GameObject go = ResolveNode(index, contractRoot, rootName, nodePath);
            if (go == null) return null;

            switch (kind)
            {
                case null:
                case "GameObject":
                    return go;
                case "TextMeshProUGUI":
                    return go.GetComponent<TMP_Text>();
                case "Image":
                    return go.GetComponent<Image>();
                case "Button":
                    return go.GetComponent<Button>();
                case "RectTransform":
                    return go.GetComponent<RectTransform>();
                case "Transform":
                    return go.transform;
                default:
                    Type type = ResolveType(kind);
                    return type == null ? null : go.GetComponent(type);
            }
        }

        /// <summary>Contract paths are root-relative; accept both forms.</summary>
        private static GameObject ResolveNode(
            Dictionary<string, GameObject> index, string contractRoot, string rootName, string nodePath)
        {
            if (string.IsNullOrEmpty(nodePath)) return null;
            if (index.TryGetValue(nodePath, out GameObject go)) return go;
            if (index.TryGetValue(rootName + "/" + nodePath, out go)) return go;
            if (index.TryGetValue(contractRoot + "/" + nodePath, out go)) return go;
            return null;
        }

        /// <summary>
        /// Restore the serialized fields of project-script components (future-proof; not part
        /// of the Form's bindings).
        /// </summary>
        private static void ApplyScriptFields(
            JObject doc, Dictionary<string, GameObject> index, string contractRoot, string rootName)
        {
            if (!(doc["scriptFields"] is JArray entries)) return;

            foreach (JObject entry in entries)
            {
                string nodePath = (string)entry["node"];
                string typeName = (string)entry["type"];
                GameObject go = ResolveNode(index, contractRoot, rootName, nodePath);
                Type type = ResolveType(typeName);
                if (go == null || type == null || !typeof(Component).IsAssignableFrom(type)) continue;

                var component = go.GetComponent(type);
                if (component == null)
                {
                    component = go.AddComponent(type);
                }

                var serialized = new SerializedObject(component);
                if (entry["refs"] is JObject refs)
                {
                    foreach (var pair in refs)
                    {
                        SerializedProperty property = serialized.FindProperty(pair.Key);
                        if (property == null) continue;
                        var spec = (JObject)pair.Value;
                        string assetPath = (string)spec["asset"];
                        property.objectReferenceValue = !string.IsNullOrEmpty(assetPath)
                            ? LoadAssetFor(property, assetPath)
                            : ResolveTarget(index, contractRoot, rootName, (string)spec["node"], (string)spec["kind"]);
                    }
                }
                if (entry["values"] is JObject values)
                {
                    foreach (var pair in values)
                    {
                        SerializedProperty property = serialized.FindProperty(pair.Key);
                        if (property != null) ApplyScalar(property, pair.Value);
                    }
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();

                if (entry["arrays"] is JObject arrays)
                {
                    var arraySerialized = new SerializedObject(component);
                    foreach (var pair in arrays)
                    {
                        SerializedProperty property = arraySerialized.FindProperty(pair.Key);
                        if (property == null || !property.isArray) continue;
                        var items = (JArray)pair.Value;
                        property.arraySize = items.Count;
                        for (int i = 0; i < items.Count; i++)
                        {
                            SerializedProperty element = property.GetArrayElementAtIndex(i);
                            JToken item = items[i];
                            if (item is JObject spec)
                            {
                                element.objectReferenceValue = ResolveTarget(
                                    index, contractRoot, rootName, (string)spec["node"], (string)spec["kind"]);
                            }
                            else
                            {
                                ApplyScalar(element, item);
                            }
                        }
                    }
                    arraySerialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        // ------------------------------------------------------------- utilities

        private static JObject LoadContract(string assetPath)
        {
            if (!File.Exists(assetPath))
            {
                throw new FileNotFoundException($"契约文件不存在：{assetPath}");
            }
            string json = File.ReadAllText(assetPath, Encoding.UTF8);
            var doc = JObject.Parse(json);
            int version = doc["schemaVersion"] == null ? 0 : doc["schemaVersion"].Value<int>();
            if (version != 3)
            {
                throw new InvalidOperationException(
                    $"契约 schemaVersion 必须是 3，实际为 {version}：{assetPath}");
            }
            return doc;
        }

        private static void SavePrefab(GameObject root, string prefabPath)
        {
            WireScrollRects(root);

            string directory = Path.GetDirectoryName(prefabPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out bool success);
            if (!success)
            {
                throw new InvalidOperationException($"预制体保存失败：{prefabPath}");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string RequireString(JObject obj, string key)
        {
            string value = (string)obj[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"契约缺少必填字段：{key}");
            }
            return value;
        }

        internal static Type ResolveType(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            if (WellKnownTypes.TryGetValue(name, out Type known)) return known;

            if (_typeIndex == null)
            {
                _typeIndex = new Dictionary<string, Type>(StringComparer.Ordinal);
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try { types = assembly.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }
                    catch (Exception) { continue; }
                    foreach (Type type in types)
                    {
                        // Prefer Component-derived types: AddComponent needs one.
                        bool isComponent = typeof(Component).IsAssignableFrom(type);
                        if (!_typeIndex.TryGetValue(type.Name, out Type existing) ||
                            (isComponent && !typeof(Component).IsAssignableFrom(existing)))
                        {
                            _typeIndex[type.Name] = type;
                        }
                    }
                }
            }
            return _typeIndex.TryGetValue(name, out Type found) ? found : null;
        }

        private static Vector2 ToVector2(JArray array)
        {
            return new Vector2(array[0].Value<float>(), array[1].Value<float>());
        }

        private static Color ToColor(JArray array)
        {
            return new Color(
                array[0].Value<float>(), array[1].Value<float>(),
                array[2].Value<float>(), array.Count > 3 ? array[3].Value<float>() : 1f);
        }

        private static RectOffset ToRectOffset(JArray array)
        {
            return new RectOffset(
                (int)array[0].Value<float>(), (int)array[1].Value<float>(),
                (int)array[2].Value<float>(), (int)array[3].Value<float>());
        }
    }
}
