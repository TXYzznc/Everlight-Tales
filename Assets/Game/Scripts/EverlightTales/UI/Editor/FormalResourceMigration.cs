using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Everlight.Tales.Data;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>设计规则的幂等增量接入。只修改声明的节点和引用，保留页面主布局与资源 GUID。</summary>
    public static class FormalResourceMigration
    {
        public const string CatalogPath = "Assets/Game/Data/UI/UIFormalSpriteCatalog.asset";
        private static UIFormalSpriteCatalog Catalog;
        private static TMP_FontAsset Font => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/Font/Common/SIMHEI SDF.asset");
        private static Dictionary<string, Sprite> Sprites;

        [MenuItem("Game Framework/EverlightTales/UI/正式资源规则/接入全部")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请停止 Play Mode 后执行资源迁移。");
            BuildCatalog();
            var library = AssetDatabase.LoadAssetAtPath<UIFormalButtonLibrary>("Assets/Game/Config/UI/FormalButtonLibrary.asset");
            if (library != null) Bind(library,"_catalog",Catalog);
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Game/Prefabs/UI" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try { Upgrade(root, path); PrefabUtility.SaveAsPrefabAsset(root, path); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            Everlight.Tales.Editor.EverlightButtonSpriteBinder.BindAll();
            AssetDatabase.SaveAssets();
            Debug.Log("[FormalResources] 正式资源目录、状态组件与页面引用接入完成。");
        }
        public static UIFormalSpriteCatalog BuildCatalog()
        {
            Sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Game/Sprites" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Placeholder") || !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
                string name = Path.GetFileNameWithoutExtension(path);
                if (name.StartsWith("SHR-027-badge") || name.StartsWith("SHR-047")) continue;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                bool changed = importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single || !importer.alphaIsTransparency || importer.mipmapEnabled;
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
                // 固定 RectTransform 与九宫格边界消除状态图尺寸差异导致的控件形变。
                if (Regex.IsMatch(name, @"^SHR-0(0[1-7]|2[1-9]|30|41|43)"))
                {
                    importer.GetSourceTextureWidthAndHeight(out int w, out int h);
                    var border = FormalVisualLayoutMigration.Border(name, w, h);
                    if (importer.spriteBorder != border) { importer.spriteBorder = border; changed = true; }
                }
                if(name.StartsWith("ICO-057-") || name.StartsWith("ICO-063") || name.StartsWith("ICO-064"))
                { if(importer.maxTextureSize != 128) { importer.maxTextureSize = 128; changed = true; } }
                if (changed) importer.SaveAndReimport();
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null) throw new InvalidOperationException("Sprite 导入失败：" + path);
                Sprites[name] = sprite;
                Match key = Regex.Match(name, @"^(?:ICO|SHR|SCR)-\d+(?:-[a-z0-9_]+)*");
                if (key.Success) Sprites[key.Value] = sprite;
            }
            string[] parts = { "撞锤", "棘轮", "线圈", "齿轮", "弹簧", "铸模", "胃袋", "熔炉", "拨叉", "转子", "飞轮", "导电桥", "棱镜", "铆合钳", "音叉", "叶轮", "缓冲囊", "探针", "磁吸", "电池" };
            PartType[] types = { PartType.InertiaHammer, PartType.MeteringRatchet, PartType.BlastCoil, PartType.ReversalGear, PartType.SpringLauncher, PartType.SplitMold, PartType.StorageStomach, PartType.MaterialFurnace, PartType.SwapFork, PartType.VortexRotor, PartType.EnergyFlywheel, PartType.ConductiveBridge, PartType.LightingPrism, PartType.RivetPliers, PartType.TuningFork, PartType.DrainImpeller, PartType.BufferBladder, PartType.CalibrationProbe, PartType.MagneticTractor, PartType.RelayBattery };
            for (int i = 0; i < parts.Length; i++) { Alias("part:" + types[i], parts[i]); Alias("object:P-" + (i + 1).ToString("000"), parts[i]); }
            foreach (FormConfig form in FormCatalog.All()) Alias("object:" + form.Id, form.Name.Split('（')[0]);
            for (int i = 1; i <= 5; i++) Alias("material:MT-" + i.ToString("000"), "ICO-0" + (51 + i));
            string[] sourceNames = { "红舞鞋", "画皮", "风月宝鉴", "道林格雷的画像", "如月车站" };
            string[] sourceKeys = { "redshoe", "paintedskin", "mirror", "portrait", "station" };
            for (int i = 0; i < sourceNames.Length; i++) Alias("source:" + sourceNames[i], "ICO-057-" + sourceKeys[i]);
            Alias("source:L-01", "ICO-057-redshoe"); Alias("source:L-02", "ICO-057-paintedskin"); Alias("source:L-07", "ICO-057-mirror");
            Alias("ICO-057异常纹样", "ICO-057-generic");
            foreach (EventKind kind in Enum.GetValues(typeof(EventKind))) Alias("event:" + kind, "ICO-0" + (kind == EventKind.None ? 46 : 40 + (int)kind));
            string[] obstacles = { "固定墙体", "脆裂隔板", "联动闸门", "转向轨道", "夹持座", "单向百叶", "增生封条", "承压支柱" };
            for (int i = 0; i < obstacles.Length; i++) Alias("obstacle:" + (ObstacleType)(i + 1), obstacles[i]);
            string[] anomalies = { "局部定势偏转", "扩张裂隙", null, "静默区", "沉降轨道", "超载蓄势场", null, "拍击后定势反转" };
            for (int i = 0; i < anomalies.Length; i++) if (anomalies[i] != null) Alias("anomaly:" + (AnomalyType)(i + 1), anomalies[i]);
            string[] places = { "home", "street", "dance", "tailor", "community" };
            for (int i = 0; i < places.Length; i++) foreach (string state in new[] { "known_locked", "unlocked", "selected" }) Alias("place:" + places[i] + ":" + state, "ICO-0" + (71 + i) + "-" + state);
            Catalog = AssetDatabase.LoadAssetAtPath<UIFormalSpriteCatalog>(CatalogPath);
            if (Catalog == null) { Catalog = ScriptableObject.CreateInstance<UIFormalSpriteCatalog>(); AssetDatabase.CreateAsset(Catalog, CatalogPath); }
            SerializedObject so = new SerializedObject(Catalog); SerializedProperty entries = so.FindProperty("_entries"); entries.arraySize = Sprites.Count;
            int index = 0;
            foreach (var pair in Sprites.OrderBy(p => p.Key, StringComparer.Ordinal))
            {
                var entry = entries.GetArrayElementAtIndex(index++); entry.FindPropertyRelative("Key").stringValue = pair.Key; entry.FindPropertyRelative("Sprite").objectReferenceValue = pair.Value;
            }
            so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(Catalog); return Catalog;
        }
        private static void Alias(string key, string source)
        {
            if (!Sprites.TryGetValue(source, out Sprite sprite)) throw new InvalidOperationException("资源语义映射缺图：" + key + " -> " + source);
            Sprites[key] = sprite;
        }
        private static Sprite S(string key) => Sprites.TryGetValue(key, out Sprite sprite) ? sprite : null;
        public static void Upgrade(GameObject root, string path)
        {
            if (!path.Contains("/Item/"))
            {
                var resources = root.GetComponent<FormalUIResources>() ?? root.AddComponent<FormalUIResources>();
                Bind(resources, "_spriteCatalog", Catalog, "_font", Font, "_buttons", AssetDatabase.LoadAssetAtPath<UIFormalButtonLibrary>("Assets/Game/Config/UI/FormalButtonLibrary.asset"));
            }
            var feedback = root.GetComponent<FormalUIResources>();
            if(feedback != null) { var settings = new SerializedObject(feedback); settings.FindProperty("_toastBottomInset").floatValue = path.Contains("BoardPage") ? 360 : 200; settings.ApplyModifiedPropertiesWithoutUndo(); }
            foreach (MonoBehaviour component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null) throw new InvalidOperationException("Prefab 存在丢失脚本：" + path);
                SerializedObject so = new SerializedObject(component);
                var catalog = so.FindProperty("_spriteCatalog"); if (catalog != null) { catalog.objectReferenceValue = Catalog; so.ApplyModifiedPropertiesWithoutUndo(); }
            }
            UpgradePageVisuals(root, path);
            foreach (ListRowItem row in root.GetComponentsInChildren<ListRowItem>(true)) UpgradeRow(row);
            foreach (MapNodeItem node in root.GetComponentsInChildren<MapNodeItem>(true)) UpgradeMap(node);
            foreach (CarryAvailableItem carry in root.GetComponentsInChildren<CarryAvailableItem>(true)) UpgradeCarry(carry);
            foreach (InvestigationHotspotItem hotspot in root.GetComponentsInChildren<InvestigationHotspotItem>(true)) UpgradeHotspot(hotspot);
            foreach (PreparationPageForm preparation in root.GetComponentsInChildren<PreparationPageForm>(true))
            {
                foreach (Button slot in preparation.CarrySlots)
                {
                    var visual = slot.GetComponent<CarrySlotVisual>() ?? slot.gameObject.AddComponent<CarrySlotVisual>();
                    Image art = Picture(slot.transform,"Img_Object",new Vector2(48,48)); art.rectTransform.anchoredPosition = new Vector2(0,8); art.gameObject.SetActive(false);
                    Image key = Picture(slot.transform,"Img_Key",new Vector2(24,24)); key.rectTransform.anchorMin = key.rectTransform.anchorMax = Vector2.one; key.rectTransform.anchoredPosition = new Vector2(-6,-6); key.sprite = S("ICO-030"); key.gameObject.SetActive(false);
                    slot.transition = Selectable.Transition.None; slot.spriteState = default; slot.image.sprite = S("SHR-004-empty"); slot.image.type = Image.Type.Sliced; slot.image.color = Color.white;
                    Bind(visual,"_spriteCatalog",Catalog,"_background",slot.image,"_object",art,"_key",key);
                }
            }
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                if (image.sprite != null && image.sprite.name.StartsWith("ICO-")) { image.type = Image.Type.Simple; image.preserveAspect = true; image.raycastTarget = false; image.color = Color.white; }
                if (image.sprite != null && AssetDatabase.GetAssetPath(image.sprite).StartsWith("Assets/Test/", StringComparison.Ordinal)) { image.sprite = null; image.enabled = false; }
            }
            foreach (ScrollRect scroll in root.GetComponentsInChildren<ScrollRect>(true))
            {
                if(scroll.vertical && scroll.verticalScrollbar == null && scroll.viewport != null)
                {
                    RectTransform track = Child(scroll.transform,"VerticalScrollbar",new Vector2(12,-8)); track.anchorMin = new Vector2(1,0); track.anchorMax = Vector2.one; track.pivot = new Vector2(1,.5f); track.anchoredPosition = new Vector2(-2,0);
                    var bg = track.gameObject.AddComponent<Image>(); bg.sprite = S("SHR-034"); bg.type = Image.Type.Sliced;
                    RectTransform slide = Child(track,"SlidingArea",Vector2.zero); slide.anchorMin = Vector2.zero; slide.anchorMax = Vector2.one; slide.offsetMin = slide.offsetMax = Vector2.zero;
                    Image handle = Picture(slide,"Handle",Vector2.zero); handle.raycastTarget = true; handle.sprite = S("SHR-035-normal"); handle.type = Image.Type.Sliced; handle.rectTransform.anchorMin = Vector2.zero; handle.rectTransform.anchorMax = Vector2.one;
                    var bar = track.gameObject.AddComponent<Scrollbar>(); bar.direction = Scrollbar.Direction.BottomToTop; bar.handleRect = handle.rectTransform; bar.targetGraphic = handle; bar.transition = Selectable.Transition.SpriteSwap;
                    bar.spriteState = new SpriteState { highlightedSprite = S("SHR-035-hover"), pressedSprite = S("SHR-035-pressed"), selectedSprite = S("SHR-035-normal"), disabledSprite = S("SHR-035-normal") };
                    scroll.verticalScrollbar = bar;
                }
                scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                scroll.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            }
            FormalVisualLayoutMigration.Apply(root, Catalog);
            FirstCaseArtMigration.Upgrade(root);
        }
        private static void UpgradePageVisuals(GameObject root, string path)
        {
            foreach (MapPanel map in root.GetComponentsInChildren<MapPanel>(true))
            {
                var so = new SerializedObject(map); var list = so.FindProperty("m_EventListRoot").objectReferenceValue as Transform;
                Image empty = Picture(list.parent,"Img_ContextEmpty",new Vector2(96,96)); empty.sprite = S("SHR-048-empty-slot"); empty.gameObject.SetActive(false); Bind(map,"_emptyArt",empty);
            }
            foreach (RecoveryPageForm recovery in root.GetComponentsInChildren<RecoveryPageForm>(true))
            {
                var so = new SerializedObject(recovery); var label = so.FindProperty("m_StatusText").objectReferenceValue as TMP_Text;
                Image status = Picture(label.transform,"Img_Status",new Vector2(24,24)); status.rectTransform.anchorMin = status.rectTransform.anchorMax = new Vector2(0,.5f); status.rectTransform.anchoredPosition = new Vector2(12,0); status.sprite = S("ICO-021"); status.gameObject.SetActive(false); label.margin = new Vector4(32,0,0,0); Bind(recovery,"_statusIcon",status);
            }
            foreach (OpeningOverlay opening in root.GetComponentsInChildren<OpeningOverlay>(true))
                Bind(opening, "_portrait", Portrait(opening.transform));
            foreach (DialoguePageForm dialogue in root.GetComponentsInChildren<DialoguePageForm>(true))
                Bind(dialogue, "_portrait", Portrait(dialogue.transform));
            foreach (InvestigationView investigation in root.GetComponentsInChildren<InvestigationView>(true))
            {
                var so = new SerializedObject(investigation); Transform scene = so.FindProperty("m_StaticSceneRoot").objectReferenceValue as Transform;
                Image art = Picture(scene,"Img_InvestigationObject",new Vector2(320,320)); art.transform.SetAsFirstSibling(); art.gameObject.SetActive(false);
                Bind(investigation,"_sceneObject",art);
            }
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                string key = null; string name = button.name;
                if (name.StartsWith("Tab_") && int.TryParse(name.Substring(4),out int tab) && tab < 4) key = "ICO-00" + (tab + 1);
                else if (name.StartsWith("Btn_Sub_") && int.TryParse(name.Substring(8),out int sub))
                {
                    if (path.Contains("JournalPage") && sub < 3) key = new[] { "ICO-005","ICO-002","ICO-006" }[sub];
                    else if (path.Contains("CodexPage") && sub < 3) key = new[] { "ICO-060","ICO-061","ICO-006" }[sub];
                }
                else if (name.StartsWith("Btn_Filter_") && int.TryParse(name.Substring(11),out int filter) && filter < 6) key = "ICO-0" + (filter == 0 ? 46 : 40 + filter);
                else if (name.StartsWith("Btn_Zone_")) key = name == "Btn_Zone_0" ? "ICO-010" : name == "Btn_Zone_2" ? "ICO-007" : name == "Btn_Zone_3" ? "ICO-008" : "ICO-011";
                else if (name == "Btn_Settings" || name == "Btn_PauseSettings") key = "ICO-012";
                else if (name == "Btn_Pause") key = "ICO-013";
                else if (name == "Btn_Back" || name == "BackButton") key = "ICO-014";
                else if (name == "Btn_Close") key = "ICO-015";
                else if (name == "Btn_Wait") key = "ICO-016";
                if (key == null) continue;
                Image icon = Picture(button.transform,"Img_Icon",new Vector2(32,32)); icon.sprite = S(key);
                if (name.StartsWith("Btn_Filter_") && int.TryParse(name.Substring(11), out int filterIndex))
                { var rect = (RectTransform)button.transform; rect.sizeDelta = new Vector2(128,48); rect.anchoredPosition = new Vector2(-384 + filterIndex * 140, -12); }
                bool tabIcon = name.StartsWith("Tab_"); icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(tabIcon ? .5f : 0,.5f);
                icon.rectTransform.anchoredPosition = tabIcon ? new Vector2(0,16) : new Vector2(24,0);
                bool mono = int.TryParse(key.Substring(4),out int code) && code <= 16;
                if (mono) { var visual = button.GetComponent<FormalNavigationIcon>() ?? button.gameObject.AddComponent<FormalNavigationIcon>(); Bind(visual,"_button",button,"_icon",icon); }
                foreach (TMP_Text label in button.GetComponentsInChildren<TMP_Text>(true))
                    if (label.name != "Txt_Num") label.margin = tabIcon ? new Vector4(0,28,0,0) : new Vector4(44,0,8,0);
            }
            foreach (TMP_Text label in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (label.font == null) label.font = Font;
                // 旧隐藏节点残留的临时材质 fileID 不可保存到 Prefab。
                if (label.fontSharedMaterial == null || !AssetDatabase.Contains(label.fontSharedMaterial)) label.fontSharedMaterial = label.font.material;
                if (label.name == "EmptyState" || label.name.EndsWith("Empty",StringComparison.Ordinal))
                {
                    label.rectTransform.anchorMin = label.rectTransform.anchorMax = label.rectTransform.pivot = new Vector2(.5f,.5f); label.rectTransform.anchoredPosition = Vector2.zero; label.rectTransform.sizeDelta = new Vector2(320,192); label.alignment = TextAlignmentOptions.Bottom; label.margin = new Vector4(0,0,0,8);
                    Image art = Picture(label.transform,"Img_Empty",new Vector2(128,128)); art.sprite = S("SHR-048-empty-list"); art.rectTransform.anchorMin = art.rectTransform.anchorMax = new Vector2(.5f,1); art.rectTransform.pivot = new Vector2(.5f,1); art.rectTransform.anchoredPosition = Vector2.zero;
                    if (label.GetComponent<LayoutElement>() != null) Fixed(label.gameObject,320,192);
                }
                string counter = label.name == "Txt_HudArm" ? "ICO-064" : label.name == "Txt_Remaining" || label.name == "Txt_InfoDuration" ? "ICO-062" : label.name == "Txt_Fee" || label.name == "Txt_Balance" ? "ICO-051" : null;
                if (counter != null) { Image icon = Picture(label.transform,"Img_Counter",new Vector2(32,32)); icon.sprite = S(counter); icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0,.5f); icon.rectTransform.anchoredPosition = new Vector2(16,0); label.margin = new Vector4(40,0,0,0); }
                if (path.Contains("CodexPage") && label.text == "资料台") label.text = "怪谈";
            }
            foreach (Slider slider in root.GetComponentsInChildren<Slider>(true))
            {
                Image rail = slider.GetComponent<Image>(); if (rail != null) { rail.sprite = S("SHR-031"); rail.type = Image.Type.Sliced; rail.color = Color.white; }
                if (slider.fillRect != null) { Image fill = slider.fillRect.GetComponent<Image>(); if(fill != null) { fill.sprite = S("SHR-032-normal"); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.color = Color.white; } }
                if (slider.handleRect != null) { Image handle = slider.handleRect.GetComponent<Image>(); if(handle != null) { handle.sprite = S("SHR-033-normal"); handle.color = Color.white; slider.targetGraphic = handle; slider.transition = Selectable.Transition.SpriteSwap; slider.spriteState = new SpriteState { highlightedSprite = S("SHR-033-hover"), pressedSprite = S("SHR-033-pressed"), disabledSprite = S("SHR-033-disabled"), selectedSprite = S("SHR-033-normal") }; } }
            }
            if (path.EndsWith("MainPageShell.prefab",StringComparison.Ordinal))
            {
                Transform openingRoot = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Panel_OpeningOverlay");
                if (openingRoot != null)
                {
                    OpeningOverlay opening = openingRoot.GetComponent<OpeningOverlay>() ?? openingRoot.gameObject.AddComponent<OpeningOverlay>();
                    var children = openingRoot.GetComponentsInChildren<Transform>(true);
                    Bind(opening,"m_Subtitle", children.First(t => t.name == "Txt_OpeningSubtitle").GetComponent<TextMeshProUGUI>(),
                        "m_ActionButton", children.First(t => t.name == "Btn_OpeningAction").GetComponent<Button>(),
                        "m_ActionLabel", children.First(t => t.name == "Txt_ActionLabel").GetComponent<TextMeshProUGUI>(),"_portrait",Portrait(openingRoot));
                    Bind(root.GetComponent<MainPageShell>(),"m_Opening",opening);
                }
                Transform top = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "TopBar");
                if(top != null) { var chrome = top.GetComponent<FormalTopBar>() ?? top.gameObject.AddComponent<FormalTopBar>(); Bind(chrome,"_spriteCatalog",Catalog,"_image",top.GetComponent<Image>()); }
                Transform tabs = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "TabBar");
                if(tabs != null && tabs.GetComponent<Image>() != null) tabs.GetComponent<Image>().sprite = S("SHR-007");
            }
        }
        private static FormalPortrait Portrait(Transform parent)
        {
            Image image = Picture(parent,"Img_Portrait",new Vector2(420,640)); image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0,.5f); image.rectTransform.pivot = new Vector2(0,.5f); image.rectTransform.anchoredPosition = new Vector2(48,120); image.enabled = false; image.transform.SetAsFirstSibling();
            var portrait = image.GetComponent<FormalPortrait>() ?? image.gameObject.AddComponent<FormalPortrait>(); Bind(portrait,"_spriteCatalog",Catalog,"_image",image); return portrait;
        }
        public static void UpgradeRow(ListRowItem row)
        {
            Transform root = row.transform;
            Fixed(root.Find("IconRoot").gameObject, 48, 48); Fixed(root.Find("Action").gameObject, 140, 48);
            root.Find("TextColumn").GetComponent<LayoutElement>().minWidth = 96;
            RectTransform group = Child(root, "StatusGroup", new Vector2(144,32)); group.SetSiblingIndex(root.Find("Txt_Right").GetSiblingIndex()); Fixed(group.gameObject,144,32);
            var layout = group.GetComponent<HorizontalLayoutGroup>() ?? group.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8; layout.childAlignment = TextAnchor.MiddleLeft; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = layout.childForceExpandHeight = false;
            Image icon = Picture(group, "Img_Status", new Vector2(24,24)); Fixed(icon.gameObject,24,24);
            TMP_Text text = Text(group, "Txt_Status", new Vector2(112,32),18); Fixed(text.gameObject,112,32);
            Bind(row,"_statusGroup",group,"_statusIcon",icon,"_statusText",text); group.gameObject.SetActive(false);
        }
        private static void UpgradeMap(MapNodeItem node)
        {
            var root = (RectTransform)node.transform; root.sizeDelta = new Vector2(200,200); root.pivot = new Vector2(.5f,1);
            Image hit = node.GetComponent<Image>(); if (hit != null) { hit.sprite = null; hit.color = Color.clear; hit.raycastTarget = true; }
            node.GetComponent<Button>().transition = Selectable.Transition.None;
            node.GetComponent<Button>().spriteState = default;
            Image building = Picture(root,"Img_Building",new Vector2(160,160)); building.rectTransform.anchoredPosition = new Vector2(0,12);
            Image selection = Picture(root,"Img_Selection",new Vector2(176,176)); selection.rectTransform.anchoredPosition = new Vector2(0,12); selection.sprite = S("SHR-046-selected"); selection.gameObject.SetActive(false);
            RectTransform badge = Child(root,"Badge",new Vector2(40,28)); badge.anchorMin = badge.anchorMax = Vector2.one; badge.anchoredPosition = new Vector2(-12,-12);
            var bg = badge.GetComponent<Image>() ?? badge.gameObject.AddComponent<Image>(); bg.sprite = S("SHR-043-alert"); bg.type = Image.Type.Sliced; bg.raycastTarget = false;
            TMP_Text count = Text(badge,"Count",new Vector2(40,28),18); count.alignment = TextAlignmentOptions.Center; badge.gameObject.SetActive(false);
            Bind(node,"_building",building,"_selection",selection,"_badge",count,"_spriteCatalog",Catalog);
            building.sprite = S("place:" + node.PlaceId + ":unlocked");
            var so = new SerializedObject(node); var label = so.FindProperty("_label").objectReferenceValue as TMP_Text;
            if (label != null) { var rect = label.rectTransform; rect.anchorMin = rect.anchorMax = new Vector2(.5f,0); rect.pivot = new Vector2(.5f,0); rect.anchoredPosition = new Vector2(0,4); rect.sizeDelta = new Vector2(184,28); label.transform.SetAsLastSibling(); }
        }
        private static void UpgradeCarry(CarryAvailableItem item)
        {
            Transform root = item.transform; Fixed(root.gameObject,-1,144);
            Fixed(root.Find("Header").gameObject,-1,48); Fixed(root.Find("Header/Icon").gameObject,48,48);
            Fixed(root.Find("Btn_Form").gameObject,-1,48); Fixed(root.Find("Txt_Hint").gameObject,-1,24);
            root.GetComponent<VerticalLayoutGroup>().spacing = 4;
            Image key = Picture(root.Find("Header"),"Img_Key",new Vector2(24,24)); Fixed(key.gameObject,24,24); key.sprite = S("ICO-030"); key.gameObject.SetActive(false); Bind(item,"_keyIcon",key);
        }
        private static void UpgradeHotspot(InvestigationHotspotItem item)
        {
            Button button = item.GetComponent<Button>(); Image image = button.image;
            image.sprite = S("SCR-16-04-idle"); image.type = Image.Type.Simple; image.preserveAspect = true; image.color = Color.white;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState { highlightedSprite = S("SCR-16-04-pulse"), pressedSprite = S("SCR-16-06-pressed"), selectedSprite = S("SCR-16-04-idle"), disabledSprite = S("SCR-16-05-confirmed") };
            SerializedObject so = new SerializedObject(item); Image check = so.FindProperty("_confirmedIcon").objectReferenceValue as Image;
            if (check != null) { check.sprite = S("ICO-026"); check.type = Image.Type.Simple; check.preserveAspect = true; check.rectTransform.sizeDelta = new Vector2(28,28); check.color = Color.white; check.raycastTarget = false; }
        }
        private static RectTransform Child(Transform parent, string name, Vector2 size)
        {
            Transform existing = parent.Find(name); var rect = existing != null ? (RectTransform)existing : (RectTransform)new GameObject(name,typeof(RectTransform)).transform;
            if (existing == null) rect.SetParent(parent,false); rect.sizeDelta = size; return rect;
        }
        private static Image Picture(Transform parent,string name,Vector2 size)
        {
            var rect = Child(parent,name,size); rect.pivot = new Vector2(.5f,.5f); var image = rect.GetComponent<Image>() ?? rect.gameObject.AddComponent<Image>(); image.preserveAspect = true; image.raycastTarget = false; return image;
        }
        private static TMP_Text Text(Transform parent,string name,Vector2 size,float fontSize)
        {
            var rect = Child(parent,name,size); var text = rect.GetComponent<TextMeshProUGUI>() ?? rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font = Font; text.fontSize = fontSize; text.color = Color.white; text.raycastTarget = false; text.enableWordWrapping = false; text.overflowMode = TextOverflowModes.Ellipsis; return text;
        }
        private static void Fixed(GameObject go,float width,float height)
        {
            var layout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>(); layout.minWidth = layout.preferredWidth = width; layout.minHeight = layout.preferredHeight = height;
        }
        private static void Bind(UnityEngine.Object target,params object[] fields)
        {
            var so = new SerializedObject(target);
            for (int i=0;i<fields.Length;i+=2) { var field = so.FindProperty((string)fields[i]); if(field == null) throw new InvalidOperationException("缺少序列化字段 " + fields[i]); field.objectReferenceValue = fields[i+1] as UnityEngine.Object; }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
