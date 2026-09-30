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
        private enum ButtonVisualRole { Main, Secondary, Small, Danger, Icon, Tab, Filter, Card, ListRow, Hex }
        private const string UiRoot = "Assets/Game/Sprites/UI/";
        private const string LibraryPath = "Assets/Game/Config/UI/FormalButtonLibrary.asset";

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
                    ButtonVisualRole role = InferRole(button.gameObject, image);
                    SpriteSet set = LoadSet(role);
                    if (set.Normal == null) continue;
                    image.sprite = set.Normal;
                    image.type = Image.Type.Sliced;
                    image.color = Color.white;
                    button.transition = Selectable.Transition.SpriteSwap;
                    button.spriteState = new SpriteState
                    {
                        highlightedSprite = set.Highlighted,
                        pressedSprite = set.Pressed,
                        selectedSprite = set.Selected,
                        disabledSprite = set.Disabled
                    };
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

        private static ButtonVisualRole InferRole(GameObject go, Image image)
        {
            string name = go.name.ToLowerInvariant();
            string sprite = image.sprite == null ? string.Empty : image.sprite.name;
            if (sprite.Contains("SHR-021")) return ButtonVisualRole.Main;
            if (sprite.Contains("SHR-022")) return ButtonVisualRole.Secondary;
            if (sprite.Contains("SHR-023")) return ButtonVisualRole.Small;
            if (sprite.Contains("SHR-024")) return ButtonVisualRole.Danger;
            if (sprite.Contains("SHR-025")) return ButtonVisualRole.Icon;
            if (sprite.Contains("SHR-029")) return ButtonVisualRole.Filter;
            if (sprite.Contains("SHR-026") || sprite.Contains("SHR-027") || sprite.Contains("SHR-028")) return ButtonVisualRole.Tab;
            if (name.Contains("filter") || name.Contains("筛选")) return ButtonVisualRole.Filter;
            if (name.StartsWith("tab_") || name.Contains("sub_") || name.Contains("zone_")) return ButtonVisualRole.Tab;
            if (sprite.Contains("SHR-005")) return ButtonVisualRole.ListRow;
            if (sprite.Contains("SHR-003")) return ButtonVisualRole.Card;
            if (sprite.Contains("SHR-046")) return ButtonVisualRole.Hex;
            if (name.Contains("delete") || name.Contains("danger") || name.Contains("retreat") || name.Contains("放弃")) return ButtonVisualRole.Danger;
            if (name.Contains("action") || name.Contains("wait") || name.Contains("confirm") || name.Contains("start") || name.Contains("claim") || name.Contains("finish")) return ButtonVisualRole.Main;
            if (name.Contains("icon")) return ButtonVisualRole.Icon;
            if (name.Contains("small") || name.Contains("close")) return ButtonVisualRole.Small;
            return ButtonVisualRole.Secondary;
        }

        private readonly struct SpriteSet
        {
            public readonly Sprite Normal, Highlighted, Pressed, Disabled, Selected;
            public SpriteSet(Sprite normal, Sprite highlighted, Sprite pressed, Sprite disabled, Sprite selected)
            { Normal = normal; Highlighted = highlighted; Pressed = pressed; Disabled = disabled; Selected = selected; }
        }

        private static SpriteSet LoadSet(ButtonVisualRole role)
        {
            string prefix; string folder = "控件/";
            switch (role)
            {
                case ButtonVisualRole.Main: prefix = "SHR-021-"; break;
                case ButtonVisualRole.Secondary: prefix = "SHR-022-"; break;
                case ButtonVisualRole.Small: prefix = "SHR-023-"; break;
                case ButtonVisualRole.Danger: prefix = "SHR-024-"; break;
                case ButtonVisualRole.Icon: prefix = "SHR-025-"; break;
                case ButtonVisualRole.Tab: prefix = "SHR-028-"; break;
                case ButtonVisualRole.Filter: prefix = "SHR-029-"; break;
                case ButtonVisualRole.Card: folder = "九宫格/"; prefix = "SHR-003-"; break;
                case ButtonVisualRole.ListRow: folder = "九宫格/"; prefix = "SHR-005-"; break;
                case ButtonVisualRole.Hex: prefix = "SHR-046-"; break;
                default: prefix = "SHR-022-"; break;
            }
            Sprite normal = Load(folder + prefix + (role == ButtonVisualRole.Tab ? "normal页签5.png" : role == ButtonVisualRole.Filter ? "normal筛选片.png" : role == ButtonVisualRole.Card ? "normal卡片.png" : role == ButtonVisualRole.ListRow ? "normal列表行.png" : role == ButtonVisualRole.Hex ? "normal六边形框.png" : "normal" + Label(role) + ".png"));
            Sprite highlighted = Load(folder + prefix + (role == ButtonVisualRole.Tab ? "hover页签5.png" : role == ButtonVisualRole.Filter ? "hover筛选片.png" : role == ButtonVisualRole.Card ? "hover卡片.png" : role == ButtonVisualRole.ListRow ? "hover列表行.png" : role == ButtonVisualRole.Hex ? "hover六边形框.png" : "hover" + Label(role) + ".png"));
            Sprite pressed = Load(folder + prefix + (role == ButtonVisualRole.Tab ? "active页签5.png" : role == ButtonVisualRole.Filter ? "active筛选片.png" : role == ButtonVisualRole.Card ? "pressed卡片.png" : role == ButtonVisualRole.ListRow ? "selected列表行.png" : role == ButtonVisualRole.Hex ? "selected六边形框.png" : "pressed" + Label(role) + ".png"));
            Sprite disabled = Load(folder + prefix + (role == ButtonVisualRole.Tab ? "normal页签5.png" : role == ButtonVisualRole.Filter ? "normal筛选片.png" : role == ButtonVisualRole.Card ? "disabled卡片.png" : role == ButtonVisualRole.ListRow ? "normal列表行.png" : role == ButtonVisualRole.Hex ? "disabled六边形框.png" : "disabled" + Label(role) + ".png"));
            Sprite selected = Load(folder + prefix + (role == ButtonVisualRole.Tab ? "active页签5.png" : role == ButtonVisualRole.Filter ? "active筛选片.png" : role == ButtonVisualRole.Card ? "selected卡片.png" : role == ButtonVisualRole.ListRow ? "selected列表行.png" : role == ButtonVisualRole.Hex ? "selected六边形框.png" : "normal" + Label(role) + ".png"));
            return new SpriteSet(normal, highlighted, pressed, disabled, selected);
        }

        private static string Label(ButtonVisualRole role)
        {
            switch (role)
            {
                case ButtonVisualRole.Main: return "主按钮";
                case ButtonVisualRole.Secondary: return "次按钮";
                case ButtonVisualRole.Small: return "小按钮";
                case ButtonVisualRole.Danger: return "危险按钮";
                case ButtonVisualRole.Icon: return "图标按钮";
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
            string dir = "Assets/Game/Config/UI";
            if (!AssetDatabase.IsValidFolder("Assets/Game/Config")) AssetDatabase.CreateFolder("Assets/Game", "Config");
            if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/Game/Config", "UI");
            UIFormalButtonLibrary asset = AssetDatabase.LoadAssetAtPath<UIFormalButtonLibrary>(LibraryPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<UIFormalButtonLibrary>();
                AssetDatabase.CreateAsset(asset, LibraryPath);
            }
            SerializedObject so = new SerializedObject(asset);
            SetLibrary(so, "Main", LoadSet(ButtonVisualRole.Main));
            SetLibrary(so, "Secondary", LoadSet(ButtonVisualRole.Secondary));
            SetLibrary(so, "Small", LoadSet(ButtonVisualRole.Small));
            SetLibrary(so, "Danger", LoadSet(ButtonVisualRole.Danger));
            SetLibrary(so, "Tab", LoadSet(ButtonVisualRole.Tab));
            SetLibrary(so, "Filter", LoadSet(ButtonVisualRole.Filter));
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
