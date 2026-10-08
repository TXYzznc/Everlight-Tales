#if UNITY_EDITOR
using System;
using System.Linq;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>逐个维护两个独立 Item 的完整契约，不改通用 ListRowItem。</summary>
    public static class SpecialItemPrefabMigration
    {
        private const string Root = "Assets/Game/Prefabs/UI/";
        private static TMP_FontAsset Font => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/Font/Common/SIMHEI SDF.asset");
        [MenuItem("Game Framework/EverlightTales/UI/特殊条目/修复携带预制体")]
        public static void Carry()
        {
            RequireStopped();
            Edit(Root + "Item/CarryAvailableItem.prefab", root =>
            {
                ClearChildren(root);
                RectTransform rect = (RectTransform)root.transform;
                rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one; rect.pivot = new Vector2(0.5f, 1); rect.sizeDelta = new Vector2(0, 120);
                Image background = root.GetComponent<Image>() ?? root.AddComponent<Image>(); background.sprite = Sprite("九宫格/SHR-005-normal列表行"); background.type = Image.Type.Sliced; background.color = Color.white;
                Button button = root.GetComponent<Button>() ?? root.AddComponent<Button>(); ConfigureRow(button, background);
                EnsureComponent(root, "PersistentButtonVisualState");
                LayoutElement height = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>(); height.minHeight = height.preferredHeight = 120;
                VerticalLayoutGroup layout = root.GetComponent<VerticalLayoutGroup>() ?? root.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(18, 14, 8, 8); layout.spacing = 3; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
                GameObject top = Rect("Header", root.transform); Size(top, -1, 28); HorizontalLayoutGroup header = top.AddComponent<HorizontalLayoutGroup>();
                header.childControlWidth = header.childControlHeight = true; header.childForceExpandWidth = header.childForceExpandHeight = false; header.spacing = 8; header.childAlignment = TextAnchor.MiddleLeft;
                Image icon = ImageChild("Icon", top.transform); Size(icon.gameObject, 28, 28);
                TMP_Text label = Text("Txt_Label", top.transform, 22); Size(label.gameObject, -1, 28).flexibleWidth = 1;
                TMP_Text selected = Text("Txt_Selected", top.transform, 19); Size(selected.gameObject, 46, 28); selected.color = new Color(0.88f, 0.66f, 0.35f);
                Image anomaly = ImageChild("Img_Anomaly", top.transform); anomaly.sprite = Sprite("图标/ICO-029异化"); Size(anomaly.gameObject, 24, 24);
                TMP_Text detail = Text("Txt_Detail", root.transform, 18); Size(detail.gameObject, -1, 24);
                TMP_Text hint = Text("Txt_Hint", root.transform, 16); Size(hint.gameObject, -1, 21); hint.color = new Color(0.88f, 0.66f, 0.35f);
                GameObject form = Rect("Btn_Form", root.transform); Size(form, -1, 28);
                Image formBg = form.AddComponent<Image>(); formBg.sprite = Sprite("控件/SHR-022-normal次按钮"); formBg.type = Image.Type.Sliced;
                Button formButton = form.AddComponent<Button>(); ConfigureButton(formButton, formBg, "控件/SHR-022", "次按钮");
                TMP_Text formLabel = Text("Txt_Form", form.transform, 17); Stretch((RectTransform)formLabel.transform); formLabel.alignment = TextAlignmentOptions.Center;
                Component logic = EnsureComponent(root, "Everlight.Tales.UI.CarryAvailableItem");
                Bind(logic, "_label", label, "_button", button, "_detail", detail, "_formLabel", formLabel, "_hint", hint, "_selectedLabel", selected, "_icon", icon, "_anomalyIcon", anomaly, "_formButton", formButton, "_height", height);
                selected.gameObject.SetActive(false); anomaly.gameObject.SetActive(false); hint.gameObject.SetActive(false);
            });
            Edit(Root + "PreparationPage.prefab", root =>
            {
                Transform panel = Find(root, "Panel_Carry");
                RectTransform slots = (RectTransform)Find(root, "Grp_CarrySlots"); Place(slots, new Vector2(0.59f, 1), Vector2.one, new Vector2(-20, 568), new Vector2(0, -94));
                GridLayoutGroup grid = slots.GetComponent<GridLayoutGroup>(); grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 1; grid.cellSize = new Vector2(350, 88); grid.spacing = new Vector2(0, 8); grid.childAlignment = TextAnchor.UpperCenter;
                RectTransform list = (RectTransform)Find(root, "List_CarryAvailable"); Place(list, new Vector2(0, 1), new Vector2(0.57f, 1), new Vector2(-36, 536), new Vector2(0, -126));
                TMP_Text heading = Find(root, "Txt_CarryAvailableHeading").GetComponent<TMP_Text>(); heading.text = "全部可用零件 · 点击加入 / 拖动替换"; heading.fontSize = 18;
                Place((RectTransform)heading.transform, new Vector2(0, 1), new Vector2(0.57f, 1), new Vector2(-36, 28), new Vector2(0, -94));
                VerticalLayoutGroup content = Find(root, "Content_CarryAvailable").GetComponent<VerticalLayoutGroup>(); content.childControlWidth = content.childControlHeight = true; content.childForceExpandWidth = true; content.childForceExpandHeight = false;
                for (int i = 0; i < 6; i++)
                {
                    Transform slot = Find(root, "Btn_CarrySlot" + i); EnsureComponent(slot.gameObject, "Everlight.Tales.UI.CarrySlotDropTarget");
                    TMP_Text label = slot.GetComponentInChildren<TMP_Text>(true); label.fontSize = 18; label.enableWordWrapping = false; label.overflowMode = TextOverflowModes.Ellipsis;
                    Button button = slot.GetComponent<Button>(); ConfigureButton(button, button.image, "控件/SHR-022", "次按钮");
                }
                Transform statusNode = panel.Find("Txt_CarryStatus"); TMP_Text status = statusNode != null ? statusNode.GetComponent<TMP_Text>() : Text("Txt_CarryStatus", panel, 20);
                Place((RectTransform)status.transform, Vector2.zero, Vector2.right, new Vector2(-40, 32), new Vector2(0, 4)); status.alignment = TextAlignmentOptions.Left; status.text = "已选 0/6";
                Component form = Find(root, "PreparationPage").GetComponent("PreparationPageForm");
                Bind(form, "_carryStatus", status, "_spriteCatalog", AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Game/Data/UI/UIFormalSpriteCatalog.asset"));
            });
            Debug.Log("[SpecialItems][Prefab] PASS CarryAvailableItem; SHR-005; explicit fields; Preparation two columns; slot drop targets.");
        }

        [MenuItem("Game Framework/EverlightTales/UI/特殊条目/修复调查预制体")]
        public static void Investigation()
        {
            RequireStopped();
            InvestigationHotspotSpriteAssets.EnsureMissing();
            Edit(Root + "Item/InvestigationHotspotItem.prefab", root =>
            {
                ClearChildren(root);
                Image target = root.GetComponent<Image>() ?? root.AddComponent<Image>(); target.type = Image.Type.Simple; target.preserveAspect = true; target.raycastTarget = true;
                Button button = root.GetComponent<Button>() ?? root.AddComponent<Button>(); button.targetGraphic = target; button.transition = Selectable.Transition.SpriteSwap;
                ConfigureHotspotSprites(target, button);
                Sprite confirmedSprite = Sprite("图标/ICO-026已完成");
                RectTransform rect = (RectTransform)root.transform; rect.sizeDelta = new Vector2(140, 140);
                GameObject labelRoot = Rect("LabelBackground", root.transform); Place((RectTransform)labelRoot.transform, new Vector2(-0.15f, 0), new Vector2(1.15f, 0), new Vector2(0, 32), new Vector2(0, -34));
                Image labelBg = labelRoot.AddComponent<Image>(); labelBg.sprite = Sprite("九宫格/SHR-001面板"); labelBg.type = Image.Type.Sliced; labelBg.raycastTarget = false;
                TMP_Text label = Text("Txt_Title", labelRoot.transform, 18); Stretch((RectTransform)label.transform); label.alignment = TextAlignmentOptions.Center;
                Image check = ImageChild("Img_Confirmed", root.transform); check.sprite = confirmedSprite;
                RectTransform checkRect = (RectTransform)check.transform; checkRect.anchorMin = checkRect.anchorMax = new Vector2(1, 1); checkRect.sizeDelta = new Vector2(28, 28); checkRect.anchoredPosition = new Vector2(-14, -14);
                Component logic = EnsureComponent(root, "Everlight.Tales.UI.InvestigationHotspotItem"); Bind(logic, "_ring", target, "_label", label, "_button", button, "_confirmedIcon", check); check.gameObject.SetActive(false);
            });
            Edit(Root + "InvestigationPage.prefab", root =>
            {
                Component view = root.GetComponentInChildren(TypeFor("Everlight.Tales.UI.InvestigationView"), true);
                Transform parent = view.transform;
                Transform old = parent.Find("Txt_Progress"); TMP_Text progress = old != null ? old.GetComponent<TMP_Text>() : Text("Txt_Progress", parent, 22);
                Place((RectTransform)progress.transform, new Vector2(0.1f, 0.16f), new Vector2(0.9f, 0.2f), Vector2.zero, Vector2.zero); progress.alignment = TextAlignmentOptions.Center; progress.text = "已确认 0/0";
                Bind(view, "_progress", progress, "m_Form", root.GetComponent("InvestigationPageForm"));
                Button finish = Find(root, "Btn_Finish").GetComponent<Button>(); ConfigureButton(finish, finish.image, "控件/SHR-021", "主按钮");
            });
            Debug.Log("[SpecialItems][Prefab] PASS InvestigationHotspotItem; 140x140 ring; SpriteSwap; confirmed icon; progress; GF form.");
        }

        [MenuItem("Game Framework/EverlightTales/UI/特殊条目/接入调查占位图片")]
        public static void InvestigationSprites()
        {
            RequireStopped();
            InvestigationHotspotSpriteAssets.EnsureMissing();
            Edit(Root + "Item/InvestigationHotspotItem.prefab", root =>
            {
                Image image = root.GetComponent<Image>();
                ConfigureHotspotSprites(image, root.GetComponent<Button>());
                Bind(root.GetComponent(TypeFor("Everlight.Tales.UI.InvestigationHotspotItem")), "_ring", image);
                Transform oldRing = root.transform.Find("Ring");
                if (oldRing != null) UnityEngine.Object.DestroyImmediate(oldRing.gameObject);
            });
            Debug.Log("[SpecialItems][Sprites] PASS SCR-16-04/05/06; four 256x256 Sprite assets; existing images preserved; prefab SpriteSwap bound.");
        }

        private static void ConfigureHotspotSprites(Image image, Button button)
        {
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(InvestigationHotspotSpriteAssets.Idle);
            image.color = Color.white; image.type = Image.Type.Simple; image.preserveAspect = true;
            button.targetGraphic = image; button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState
            {
                highlightedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(InvestigationHotspotSpriteAssets.Pulse),
                pressedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(InvestigationHotspotSpriteAssets.Pressed),
                selectedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(InvestigationHotspotSpriteAssets.Idle),
                disabledSprite = AssetDatabase.LoadAssetAtPath<Sprite>(InvestigationHotspotSpriteAssets.Confirmed)
            };
            NoNavigation(button);
        }

        [MenuItem("Game Framework/EverlightTales/UI/特殊条目/验证运行流程")]
        public static void Validate() => TypeFor("Everlight.Tales.UI.SpecialItemRuntimeValidation").GetMethod("Run").Invoke(null, null);
        [MenuItem("Game Framework/EverlightTales/UI/特殊条目/检查引用")]
        public static void Audit()
        {
            InvestigationHotspotSpriteAssets.Validate();
            GameObject hotspot = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Item/InvestigationHotspotItem.prefab");
            Button hotspotButton = hotspot.GetComponent<Button>();
            if (AssetDatabase.GetAssetPath(hotspotButton.image.sprite) != InvestigationHotspotSpriteAssets.Idle ||
                AssetDatabase.GetAssetPath(hotspotButton.spriteState.highlightedSprite) != InvestigationHotspotSpriteAssets.Pulse ||
                AssetDatabase.GetAssetPath(hotspotButton.spriteState.pressedSprite) != InvestigationHotspotSpriteAssets.Pressed ||
                AssetDatabase.GetAssetPath(hotspotButton.spriteState.disabledSprite) != InvestigationHotspotSpriteAssets.Confirmed)
                throw new InvalidOperationException("调查热点必须绑定 SCR-16-04/05/06 图片。");
            Check(Root + "Item/CarryAvailableItem.prefab", "Everlight.Tales.UI.CarryAvailableItem", "_label", "_button", "_detail", "_formLabel", "_hint", "_selectedLabel", "_icon", "_anomalyIcon", "_formButton", "_height");
            Check(Root + "Item/InvestigationHotspotItem.prefab", "Everlight.Tales.UI.InvestigationHotspotItem", "_ring", "_label", "_button", "_confirmedIcon");
            Check(Root + "PreparationPage.prefab", "Everlight.Tales.UI.PreparationPageForm", "_carryStatus", "_spriteCatalog", "_availableContent", "_availableItemTemplate");
            Check(Root + "InvestigationPage.prefab", "Everlight.Tales.UI.InvestigationView", "_progress", "m_Form", "m_HotspotTemplate", "m_SceneLabel", "m_StaticSceneRoot", "m_StaticFinishButton");
            GameObject carry = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "PreparationPage.prefab");
            if (carry.GetComponentsInChildren(TypeFor("Everlight.Tales.UI.CarrySlotDropTarget"), true).Length != 6) throw new InvalidOperationException("六个携带槽必须全部有投放接入。");
            Debug.Log("[SpecialItems][Audit] PASS 4 prefabs; serialized fields; 6 drop targets; SpriteSwap sprites; SCR-16 imports and state paths; no missing scripts.");
        }
        private static void Check(string path, string type, params string[] fields)
        {
            GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (Transform node in root.GetComponentsInChildren<Transform>(true)) if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) != 0) throw new InvalidOperationException("缺少脚本 " + path + "/" + node.name);
            Component logic = root.GetComponentInChildren(TypeFor(type), true);
            SerializedObject so = new SerializedObject(logic);
            foreach (string field in fields) if (so.FindProperty(field)?.objectReferenceValue == null) throw new InvalidOperationException(path + " 未绑定 " + field);
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
                if (button.transition != Selectable.Transition.SpriteSwap || button.image == null || button.image.sprite == null || button.spriteState.highlightedSprite == null || button.spriteState.pressedSprite == null || button.spriteState.selectedSprite == null || button.spriteState.disabledSprite == null)
                    throw new InvalidOperationException(path + "/" + button.name + " 图片过渡引用不完整。");
        }
        private static void RequireStopped() { if (EditorApplication.isPlaying) throw new InvalidOperationException("先退出 Play Mode。"); }
        private static void Edit(string path, Action<GameObject> action)
        {
            bool existing = File.Exists(path);
            GameObject root = existing ? PrefabUtility.LoadPrefabContents(path) : Rect(Path.GetFileNameWithoutExtension(path), null);
            try { action(root); FormalResourceMigration.BuildCatalog(); FormalResourceMigration.Upgrade(root, path);
                PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { if (existing) PrefabUtility.UnloadPrefabContents(root); else UnityEngine.Object.DestroyImmediate(root); }
            AssetDatabase.SaveAssets();
        }
        private static Type TypeFor(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
        private static Component EnsureComponent(GameObject root, string type) => root.GetComponent(TypeFor(type)) ?? root.AddComponent(TypeFor(type));
        private static Transform Find(GameObject root, string name) => root.GetComponentsInChildren<Transform>(true).First(t => t.name == name);
        private static void ClearChildren(GameObject root) { while (root.transform.childCount > 0) UnityEngine.Object.DestroyImmediate(root.transform.GetChild(0).gameObject); }
        private static Sprite Sprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Sprites/UI/" + path + ".png") ?? throw new InvalidOperationException("素材缺失 " + path);
        private static GameObject Rect(string name, Transform parent) { GameObject go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); go.layer = 5; return go; }
        private static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        private static void Place(RectTransform rect, Vector2 min, Vector2 max, Vector2 size, Vector2 pos) { rect.anchorMin = min; rect.anchorMax = max; rect.pivot = new Vector2(0.5f, min.y == 1 ? 1 : 0); rect.sizeDelta = size; rect.anchoredPosition = pos; }
        private static Image ImageChild(string name, Transform parent) { Image image = Rect(name, parent).AddComponent<Image>(); image.raycastTarget = false; image.preserveAspect = true; return image; }
        private static TMP_Text Text(string name, Transform parent, float size) { TMP_Text text = Rect(name, parent).AddComponent<TextMeshProUGUI>(); text.font = Font; text.fontSize = size; text.enableWordWrapping = false; text.overflowMode = TextOverflowModes.Ellipsis; text.alignment = TextAlignmentOptions.Left; text.raycastTarget = false; return text; }
        private static LayoutElement Size(GameObject go, float width, float height) { LayoutElement element = go.AddComponent<LayoutElement>(); element.minWidth = 0; element.preferredWidth = width; element.preferredHeight = height; return element; }
        private static void NoNavigation(Button button) { Navigation nav = button.navigation; nav.mode = Navigation.Mode.None; button.navigation = nav; }
        private static void ConfigureRow(Button button, Image image) { image.color = Color.white; button.targetGraphic = image; button.transition = Selectable.Transition.SpriteSwap; button.spriteState = new SpriteState { highlightedSprite = Sprite("九宫格/SHR-005-hover列表行"), pressedSprite = Sprite("九宫格/SHR-005-selected列表行"), selectedSprite = Sprite("九宫格/SHR-005-selected列表行"), disabledSprite = Sprite("九宫格/SHR-005-normal列表行") }; NoNavigation(button); }
        private static void ConfigureButton(Button button, Image image, string prefix, string suffix) { image.sprite = Sprite(prefix + "-normal" + suffix); image.type = Image.Type.Sliced; button.targetGraphic = image; button.transition = Selectable.Transition.SpriteSwap; button.spriteState = new SpriteState { highlightedSprite = Sprite(prefix + "-hover" + suffix), pressedSprite = Sprite(prefix + "-pressed" + suffix), selectedSprite = Sprite(prefix + "-hover" + suffix), disabledSprite = Sprite(prefix + "-disabled" + suffix) }; NoNavigation(button); }
        private static void Bind(Component logic, params object[] refs) { SerializedObject so = new SerializedObject(logic); for (int i = 0; i < refs.Length; i += 2) { SerializedProperty p = so.FindProperty((string)refs[i]); if (p == null) throw new InvalidOperationException("字段缺失 " + refs[i]); p.objectReferenceValue = (UnityEngine.Object)refs[i + 1]; } so.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
#endif
