#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Everlight.Tales.UI;

namespace Everlight.Tales.Editor
{
    /// <summary>把正式按钮状态 Sprite 绑定到全部 UIForm 和 UIItem Prefab。</summary>
    public static class EverlightButtonSpriteBinder
    {
        private const string UiRoot = "Assets/Game/Sprites/UI/";
        private const string ResourceLibraryPath = "Assets/Resources/UI/FormalButtonLibrary.asset";

        [MenuItem("Game Framework/EverlightTales/UI/接入正式按钮 Sprite Swap", priority = 2012)]
        [MenuItem("EverlightTales/美术/接入正式按钮 Sprite Swap", priority = 110)]
        public static void BindAll()
        {
            CreateRuntimeLibrary();
            string[] pages =
            {
                "MainPageShell", "DialogView", "BoardPage", "PreparationPage", "SettlementPage", "Settings",
                "SaveSlotPage", "MapPage", "HomePage", "JournalPage", "WorkbenchPage", "CodexPage",
                "ArchivePage", "GuestPage", "ServicePage", "DialoguePage", "ProloguePage", "InvestigationPage",
                "RecoveryPage", "FeedbackPage"
            };
            int bound = 0;
            for (int i = 0; i < pages.Length; i++) bound += BindPrefab("Assets/Game/Prefabs/UI/" + pages[i] + ".prefab");
            string[] items = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/Prefabs/UI/Item" });
            for (int i = 0; i < items.Length; i++) bound += BindPrefab(AssetDatabase.GUIDToAssetPath(items[i]));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightButtonSpriteBinder] 已绑定正式按钮 Sprite Swap，按钮数=" + bound);
        }

        private static int BindPrefab(string path)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            int count = 0;
            try
            {
                foreach (Button button in root.GetComponentsInChildren<Button>(true))
                {
                    Image image = button.GetComponent<Image>();
                    if (image == null) continue;
                    UISpriteButtonRole role = InferRole(button.gameObject, image);
                    SpriteSet set = LoadSet(role);
                    if (set.Normal == null) continue;
                    image.sprite = set.Normal;
                    image.type = Image.Type.Sliced;
                    image.color = Color.white;
                    UISpriteButton state = button.GetComponent<UISpriteButton>() ?? button.gameObject.AddComponent<UISpriteButton>();
                    state.Configure(image, button, set.Normal, set.Highlighted, set.Pressed, set.Disabled, set.Selected);
                    ColorBlock colors = button.colors;
                    colors.normalColor = Color.white;
                    colors.highlightedColor = Color.white;
                    colors.pressedColor = Color.white;
                    colors.selectedColor = Color.white;
                    colors.disabledColor = Color.white;
                    button.colors = colors;
                    count++;
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            return count;
        }

        private static UISpriteButtonRole InferRole(GameObject go, Image image)
        {
            string name = go.name.ToLowerInvariant();
            string sprite = image.sprite == null ? string.Empty : image.sprite.name;
            if (sprite.Contains("SHR-021")) return UISpriteButtonRole.Main;
            if (sprite.Contains("SHR-022")) return UISpriteButtonRole.Secondary;
            if (sprite.Contains("SHR-023")) return UISpriteButtonRole.Small;
            if (sprite.Contains("SHR-024")) return UISpriteButtonRole.Danger;
            if (sprite.Contains("SHR-025")) return UISpriteButtonRole.Icon;
            if (sprite.Contains("SHR-029")) return UISpriteButtonRole.Filter;
            if (sprite.Contains("SHR-026") || sprite.Contains("SHR-027") || sprite.Contains("SHR-028")) return UISpriteButtonRole.Tab;
            if (name.Contains("filter") || name.Contains("筛选")) return UISpriteButtonRole.Filter;
            if (name.StartsWith("tab_") || name.Contains("sub_") || name.Contains("zone_")) return UISpriteButtonRole.Tab;
            if (sprite.Contains("SHR-005")) return UISpriteButtonRole.ListRow;
            if (sprite.Contains("SHR-003")) return UISpriteButtonRole.Card;
            if (sprite.Contains("SHR-046")) return UISpriteButtonRole.Hex;
            if (name.Contains("delete") || name.Contains("danger") || name.Contains("retreat") || name.Contains("放弃")) return UISpriteButtonRole.Danger;
            if (name.Contains("action") || name.Contains("wait") || name.Contains("confirm") || name.Contains("start") || name.Contains("claim") || name.Contains("finish")) return UISpriteButtonRole.Main;
            if (name.Contains("icon")) return UISpriteButtonRole.Icon;
            if (name.Contains("small") || name.Contains("close")) return UISpriteButtonRole.Small;
            return UISpriteButtonRole.Secondary;
        }

        private readonly struct SpriteSet
        {
            public readonly Sprite Normal, Highlighted, Pressed, Disabled, Selected;
            public SpriteSet(Sprite normal, Sprite highlighted, Sprite pressed, Sprite disabled, Sprite selected)
            { Normal = normal; Highlighted = highlighted; Pressed = pressed; Disabled = disabled; Selected = selected; }
        }

        private static SpriteSet LoadSet(UISpriteButtonRole role)
        {
            string prefix; string folder = "控件/";
            switch (role)
            {
                case UISpriteButtonRole.Main: prefix = "SHR-021-"; break;
                case UISpriteButtonRole.Secondary: prefix = "SHR-022-"; break;
                case UISpriteButtonRole.Small: prefix = "SHR-023-"; break;
                case UISpriteButtonRole.Danger: prefix = "SHR-024-"; break;
                case UISpriteButtonRole.Icon: prefix = "SHR-025-"; break;
                case UISpriteButtonRole.Tab: prefix = "SHR-028-"; break;
                case UISpriteButtonRole.Filter: prefix = "SHR-029-"; break;
                case UISpriteButtonRole.Card: folder = "九宫格/"; prefix = "SHR-003-"; break;
                case UISpriteButtonRole.ListRow: folder = "九宫格/"; prefix = "SHR-005-"; break;
                case UISpriteButtonRole.Hex: prefix = "SHR-046-"; break;
                default: prefix = "SHR-022-"; break;
            }
            Sprite normal = Load(folder + prefix + (role == UISpriteButtonRole.Tab ? "normal页签5.png" : role == UISpriteButtonRole.Filter ? "normal筛选片.png" : role == UISpriteButtonRole.Card ? "normal卡片.png" : role == UISpriteButtonRole.ListRow ? "normal列表行.png" : role == UISpriteButtonRole.Hex ? "normal六边形框.png" : "normal" + Label(role) + ".png"));
            Sprite highlighted = Load(folder + prefix + (role == UISpriteButtonRole.Tab ? "hover页签5.png" : role == UISpriteButtonRole.Filter ? "hover筛选片.png" : role == UISpriteButtonRole.Card ? "hover卡片.png" : role == UISpriteButtonRole.ListRow ? "hover列表行.png" : role == UISpriteButtonRole.Hex ? "hover六边形框.png" : "hover" + Label(role) + ".png"));
            Sprite pressed = Load(folder + prefix + (role == UISpriteButtonRole.Tab ? "active页签5.png" : role == UISpriteButtonRole.Filter ? "active筛选片.png" : role == UISpriteButtonRole.Card ? "pressed卡片.png" : role == UISpriteButtonRole.ListRow ? "selected列表行.png" : role == UISpriteButtonRole.Hex ? "selected六边形框.png" : "pressed" + Label(role) + ".png"));
            Sprite disabled = Load(folder + prefix + (role == UISpriteButtonRole.Tab ? "normal页签5.png" : role == UISpriteButtonRole.Filter ? "normal筛选片.png" : role == UISpriteButtonRole.Card ? "disabled卡片.png" : role == UISpriteButtonRole.ListRow ? "normal列表行.png" : role == UISpriteButtonRole.Hex ? "disabled六边形框.png" : "disabled" + Label(role) + ".png"));
            Sprite selected = Load(folder + prefix + (role == UISpriteButtonRole.Tab ? "active页签5.png" : role == UISpriteButtonRole.Filter ? "active筛选片.png" : role == UISpriteButtonRole.Card ? "selected卡片.png" : role == UISpriteButtonRole.ListRow ? "selected列表行.png" : role == UISpriteButtonRole.Hex ? "selected六边形框.png" : "normal" + Label(role) + ".png"));
            return new SpriteSet(normal, highlighted, pressed, disabled, selected);
        }

        private static string Label(UISpriteButtonRole role)
        {
            switch (role)
            {
                case UISpriteButtonRole.Main: return "主按钮";
                case UISpriteButtonRole.Secondary: return "次按钮";
                case UISpriteButtonRole.Small: return "小按钮";
                case UISpriteButtonRole.Danger: return "危险按钮";
                case UISpriteButtonRole.Icon: return "图标按钮";
                default: return "次按钮";
            }
        }

        private static Sprite Load(string relative)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + relative);
        }

        private static void CreateRuntimeLibrary()
        {
            // 运行时动态生成的 UIFactory 按钮使用同一组正式 Sprite；Prefab 本身仍直接保存 Sprite 引用。
            string dir = "Assets/Resources/UI";
            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/Resources", "UI");
            UIFormalButtonLibrary asset = AssetDatabase.LoadAssetAtPath<UIFormalButtonLibrary>(ResourceLibraryPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<UIFormalButtonLibrary>();
                AssetDatabase.CreateAsset(asset, ResourceLibraryPath);
            }
            SerializedObject so = new SerializedObject(asset);
            SetLibrary(so, "Main", LoadSet(UISpriteButtonRole.Main));
            SetLibrary(so, "Secondary", LoadSet(UISpriteButtonRole.Secondary));
            SetLibrary(so, "Small", LoadSet(UISpriteButtonRole.Small));
            SetLibrary(so, "Danger", LoadSet(UISpriteButtonRole.Danger));
            SetLibrary(so, "Tab", LoadSet(UISpriteButtonRole.Tab));
            SetLibrary(so, "Filter", LoadSet(UISpriteButtonRole.Filter));
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        private static void SetLibrary(SerializedObject so, string name, SpriteSet set)
        {
            Set(so, "_" + name.ToLowerInvariant() + "Normal", set.Normal);
            Set(so, "_" + name.ToLowerInvariant() + "Highlighted", set.Highlighted);
            Set(so, "_" + name.ToLowerInvariant() + "Pressed", set.Pressed);
            Set(so, "_" + name.ToLowerInvariant() + "Disabled", set.Disabled);
            Set(so, "_" + name.ToLowerInvariant() + "Selected", set.Selected);
        }

        private static void Set(SerializedObject so, string field, Sprite value)
        {
            SerializedProperty property = so.FindProperty(field);
            if (property != null) property.objectReferenceValue = value;
        }
    }
}
#endif
