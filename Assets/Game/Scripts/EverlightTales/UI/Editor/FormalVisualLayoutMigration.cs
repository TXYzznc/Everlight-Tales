using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.Editor
{
    /// <summary>基于实际截图校正图文布局；由正式资源迁移和专用生成器共同调用。</summary>
    public static class FormalVisualLayoutMigration
    {
        public static Vector4 Border(string name, int width, int height)
        {
            float x = 24, y = 24;
            if (name.StartsWith("SHR-001") || name.StartsWith("SHR-002")) x = y = 80;
            else if (name.StartsWith("SHR-003")) x = y = 56;
            else if (name.StartsWith("SHR-004")) x = y = 64;
            else if (name.StartsWith("SHR-005")) { x = 64; y = 40; }
            else if (name.StartsWith("SHR-006")) { x = 40; y = 32; }
            else if (name.StartsWith("SHR-007")) { x = 48; y = 48; }
            else if (name.StartsWith("SHR-025")) x = y = 104;
            else if (name.StartsWith("SHR-041")) return new Vector4(52,28,36,28);
            x = Mathf.Min(x, width / 2f - 1); y = Mathf.Min(y, height / 2f - 1);
            return new Vector4(x, y, x, y);
        }

        public static void Apply(GameObject root, UIFormalSpriteCatalog catalog)
        {
            Image pageBackground = root.GetComponentsInChildren<Image>(true).FirstOrDefault(i => i.name == "Panel_Background");
            if (pageBackground != null)
            {
                Transform found = pageBackground.transform.parent.Find("OpaquePageBacking");
                var backing = found != null ? (RectTransform)found : (RectTransform)new GameObject("OpaquePageBacking",typeof(RectTransform)).transform;
                backing.SetParent(pageBackground.transform.parent,false); backing.SetSiblingIndex(pageBackground.transform.GetSiblingIndex());
                backing.anchorMin = Vector2.zero; backing.anchorMax = Vector2.one; backing.offsetMin = backing.offsetMax = Vector2.zero;
                Image image = backing.GetComponent<Image>() ?? backing.gameObject.AddComponent<Image>(); image.color = new Color32(23,26,32,255); image.raycastTarget = false;
            }
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                if (image.name == "Dimed" || image.name == "Panel_Dim" || image.name == "Panel_Pause" || image.name.Contains("Backdrop"))
                {
                    image.sprite = null; image.color = new Color(0,0,0,.6f);
                    var button = image.GetComponent<Button>(); if (button != null) { button.transition = Selectable.Transition.None; button.spriteState = default; }
                    continue;
                }
                if (image.sprite == null || !image.sprite.name.StartsWith("SHR-")) continue;
                image.color = Color.white;
                string name = image.sprite.name;
                if (name.StartsWith("SHR-00") || name.StartsWith("SHR-041") || name.StartsWith("SHR-043")) image.type = Image.Type.Sliced;
            }
            foreach (TMP_Text label in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (label.name == "Txt_Title" && (root.GetComponent<DialoguePageForm>() != null || root.GetComponent<FeedbackPageForm>() != null || root.GetComponent<RecoveryPageForm>() != null))
                {
                    label.rectTransform.anchorMin = new Vector2(0,1); label.rectTransform.anchorMax = Vector2.one;
                    label.rectTransform.pivot = new Vector2(.5f,1); label.rectTransform.anchoredPosition = new Vector2(0,-48); label.rectTransform.sizeDelta = new Vector2(-112,52); label.alignment = TextAlignmentOptions.MidlineLeft;
                }
                Image icon = label.transform.Find("Img_Counter")?.GetComponent<Image>();
                if (icon == null) continue;
                icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = label.rectTransform.pivot;
            }
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                var navigation = button.GetComponent<FormalNavigationIcon>();
                if (button.name.StartsWith("Btn_Filter_"))
                {
                    // 筛选片选中底图为深色，保留浅字；内边距需避开六角边框。
                    if (navigation != null) { Object.DestroyImmediate(navigation); navigation = null; }
                    var filterIcon = button.transform.Find("Img_Icon") as RectTransform;
                    if (filterIcon != null) { filterIcon.sizeDelta = new Vector2(24,24); filterIcon.anchoredPosition = new Vector2(32,0); }
                    foreach (TMP_Text label in button.GetComponentsInChildren<TMP_Text>(true))
                        if (label.name != "Txt_Num") { label.margin = new Vector4(48,0,12,0); label.color = new Color32(232,234,240,255); }
                }
                if (navigation == null && (button.name.StartsWith("Tab_") || button.name.StartsWith("Btn_Sub_") || button.name.StartsWith("Btn_Zone_")))
                {
                    navigation = button.gameObject.AddComponent<FormalNavigationIcon>(); Bind(navigation,"_button",button);
                }
                if (navigation != null) Bind(navigation,"_label",button.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault(t => t.name != "Txt_Num"));
                if (button.name.StartsWith("Tab_"))
                { var indicator = button.transform.Find("Indicator")?.GetComponent<Image>(); if (indicator != null) indicator.enabled = false; }
                if (button.name != "Btn_Pause" && button.name != "Btn_Settings") continue;
                if (((RectTransform)button.transform).rect.width > 100) continue;
                if (button.name == "Btn_Pause") ((RectTransform)button.transform).sizeDelta = new Vector2(96,96);
                var icon = button.transform.Find("Img_Icon") as RectTransform;
                if (icon != null) { icon.anchorMin = icon.anchorMax = new Vector2(.5f,.5f); icon.anchoredPosition = Vector2.zero; }
                foreach (TMP_Text label in button.GetComponentsInChildren<TMP_Text>(true)) label.gameObject.SetActive(false);
            }
            foreach (WorkbenchItem item in root.GetComponentsInChildren<WorkbenchItem>(true)) UpgradeHost(item, catalog);
            foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.name == "HostScroll") { rect.sizeDelta = new Vector2(-48,128); }
                if (rect.name == "HostContent")
                {
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x,128);
                    var layout = rect.GetComponent<HorizontalLayoutGroup>();
                    if (layout != null) { layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandHeight = layout.childForceExpandWidth = false; layout.childAlignment = TextAnchor.MiddleLeft; layout.spacing = 12; }
                }
                if (rect.name == "FormScroll" && root.GetComponentInChildren<WorkbenchPanel>(true) != null)
                { rect.anchoredPosition = new Vector2(0,-144); rect.sizeDelta = new Vector2(-48,300); }
            }
            foreach (SettingsForm settings in root.GetComponentsInChildren<SettingsForm>(true))
            {
                UpgradeSlider(settings.MusicSlider, settings.MusicFill, settings.MusicHandle, settings.MusicValue, catalog);
                UpgradeSlider(settings.SfxSlider, settings.SfxFill, settings.SfxHandle, settings.SfxValue, catalog);
            }
            foreach (PreparationPageForm preparation in root.GetComponentsInChildren<PreparationPageForm>(true))
            {
                RectTransform status = preparation.CarryStatus.rectTransform;
                status.anchorMin = Vector2.zero; status.anchorMax = Vector2.right; status.pivot = new Vector2(.5f,0);
                status.anchoredPosition = new Vector2(0,12); status.sizeDelta = new Vector2(-64,32);
                var availableScroll = preparation.AvailableContent.GetComponentInParent<ScrollRect>();
                var availableRect = (RectTransform)availableScroll.transform;
                availableRect.anchorMin = new Vector2(availableRect.anchorMin.x,0);
                availableRect.anchorMax = new Vector2(availableRect.anchorMax.x,1);
                availableRect.offsetMin = new Vector2(availableRect.offsetMin.x,56);
                availableRect.offsetMax = new Vector2(availableRect.offsetMax.x,-128);
                for (int i = 0; i < preparation.CarrySlots.Length; i++)
                {
                    Button slot = preparation.CarrySlots[i];
                    RectTransform art = slot.transform.Find("Img_Object") as RectTransform;
                    art.anchorMin = art.anchorMax = new Vector2(0,.5f); art.anchoredPosition = new Vector2(36,0);
                    var key = slot.transform.Find("Img_Key") as RectTransform; key.anchoredPosition = new Vector2(-18,-18);
                    TMP_Text label = preparation.CarrySlotLabels[i];
                    label.rectTransform.anchorMin = Vector2.zero; label.rectTransform.anchorMax = Vector2.one;
                    label.rectTransform.offsetMin = new Vector2(70,8); label.rectTransform.offsetMax = new Vector2(-28,-8);
                    label.margin = Vector4.zero; label.alignment = TextAlignmentOptions.MidlineLeft; label.fontSize = 17;
                }
            }
            foreach (OpeningOverlay opening in root.GetComponentsInChildren<OpeningOverlay>(true))
            {
                var so = new SerializedObject(opening); var subtitle = so.FindProperty("m_Subtitle").objectReferenceValue as TMP_Text;
                subtitle.rectTransform.anchorMin = new Vector2(.08f,.24f); subtitle.rectTransform.anchorMax = new Vector2(.92f,.4f);
                subtitle.rectTransform.offsetMin = subtitle.rectTransform.offsetMax = Vector2.zero; subtitle.alignment = TextAlignmentOptions.TopLeft;
                subtitle.enableWordWrapping = true; subtitle.fontSize = 28;
                Image portrait = opening.transform.Find("Img_Portrait")?.GetComponent<Image>();
                if (portrait != null) { portrait.rectTransform.anchorMin = portrait.rectTransform.anchorMax = new Vector2(.5f,.46f); portrait.rectTransform.pivot = new Vector2(.5f,0); portrait.rectTransform.anchoredPosition = Vector2.zero; }
            }
            foreach (WorkbenchPanel workbench in root.GetComponentsInChildren<WorkbenchPanel>(true)) UpgradeMaterialDetail(workbench);
            foreach (DialoguePageForm dialogue in root.GetComponentsInChildren<DialoguePageForm>(true))
            {
                RectTransform panel = dialogue.GetComponentsInChildren<RectTransform>(true).First(r => r.name == "Panel_Dialog");
                panel.anchorMin = new Vector2(.06f,.08f); panel.anchorMax = new Vector2(.94f,.43f);
                panel.offsetMin = panel.offsetMax = Vector2.zero;
                var portrait = (RectTransform)dialogue.transform.Find("Img_Portrait");
                portrait.SetSiblingIndex(panel.GetSiblingIndex());
                portrait.anchorMin = portrait.anchorMax = new Vector2(.5f,.46f); portrait.pivot = new Vector2(.5f,0); portrait.anchoredPosition = Vector2.zero;
                TMP_Text content = panel.GetComponentsInChildren<TMP_Text>(true).First(t => t.name == "Txt_Content");
                content.rectTransform.anchorMin = new Vector2(0,0); content.rectTransform.anchorMax = Vector2.one;
                content.rectTransform.offsetMin = new Vector2(56,144); content.rectTransform.offsetMax = new Vector2(-56,-120);
                content.alignment = TextAlignmentOptions.TopLeft; content.enableWordWrapping = true;
            }
            foreach (TMP_Text heading in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (heading.name != "Txt_InfoHeading" && heading.name != "Txt_CarryHeading" && heading.name != "Txt_PreviewHeading") continue;
                heading.rectTransform.anchoredPosition = new Vector2(heading.rectTransform.anchoredPosition.x,-14);
                heading.margin = new Vector4(16,0,0,0);
            }
        }

        private static void UpgradeMaterialDetail(WorkbenchPanel workbench)
        {
            var so = new SerializedObject(workbench);
            TMP_Text title = so.FindProperty("_materialTitle").objectReferenceValue as TMP_Text;
            Image icon = so.FindProperty("_materialIcon").objectReferenceValue as Image;
            RectTransform detail = workbench.GetComponentsInChildren<RectTransform>(true).First(r => r.name == "Panel_MaterialDetail");
            detail.anchorMin = Vector2.zero; detail.anchorMax = Vector2.right; detail.pivot = new Vector2(.5f,0); detail.anchoredPosition = new Vector2(0,16); detail.sizeDelta = new Vector2(-48,536);
            var layout = detail.GetComponent<VerticalLayoutGroup>() ?? detail.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24,24,20,20); layout.spacing = 12; layout.childAlignment = TextAnchor.UpperLeft; layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            Transform found = detail.Find("MaterialHeading"); var header = found != null ? found.gameObject : new GameObject("MaterialHeading",typeof(RectTransform)); header.transform.SetParent(detail,false); header.transform.SetAsFirstSibling();
            SetSize(header,-1,64);
            var row = header.GetComponent<HorizontalLayoutGroup>() ?? header.AddComponent<HorizontalLayoutGroup>(); row.spacing = 16; row.childAlignment = TextAnchor.MiddleLeft; row.childControlWidth = row.childControlHeight = true; row.childForceExpandWidth = row.childForceExpandHeight = false;
            icon.transform.SetParent(header.transform,false); icon.transform.SetAsFirstSibling(); SetSize(icon.gameObject,64,64);
            title.transform.SetParent(header.transform,false); var titleSize = SetSize(title.gameObject,-1,64); titleSize.flexibleWidth = 1; title.fontSize = 24; title.alignment = TextAlignmentOptions.MidlineLeft; title.margin = Vector4.zero; title.enableWordWrapping = true;
            string[] fields = { "_materialUsage", "_materialSource", "_materialFlavor" }; float[] heights = { 192,56,80 };
            for (int i = 0; i < fields.Length; i++)
            {
                var text = so.FindProperty(fields[i]).objectReferenceValue as TMP_Text;
                text.transform.SetParent(detail,false); text.transform.SetSiblingIndex(i+1); SetSize(text.gameObject,-1,heights[i]); text.fontSize = 18; text.alignment = TextAlignmentOptions.TopLeft; text.enableWordWrapping = true; text.margin = Vector4.zero;
            }
            RectTransform scroll = workbench.GetComponentsInChildren<RectTransform>(true).First(r => r.name == "MaterialsScroll");
            scroll.anchorMin = Vector2.zero; scroll.anchorMax = Vector2.one; scroll.offsetMin = new Vector2(24,568); scroll.offsetMax = new Vector2(-24,-24);
            var ledger = so.FindProperty("m_LedgerRoot").objectReferenceValue as RectTransform;
            var ledgerLayout = ledger.GetComponent<VerticalLayoutGroup>() ?? ledger.gameObject.AddComponent<VerticalLayoutGroup>();
            ledgerLayout.childAlignment = TextAnchor.UpperLeft; ledgerLayout.childControlWidth = ledgerLayout.childControlHeight = true; ledgerLayout.childForceExpandWidth = true; ledgerLayout.childForceExpandHeight = false; ledgerLayout.padding = new RectOffset(24,24,24,24); ledgerLayout.spacing = 12;
        }

        private static LayoutElement SetSize(GameObject go,float width,float height)
        {
            var element = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>(); element.minWidth = element.preferredWidth = width; element.minHeight = element.preferredHeight = height; return element;
        }

        private static void UpgradeHost(WorkbenchItem item, UIFormalSpriteCatalog catalog)
        {
            var rect = (RectTransform)item.transform; rect.sizeDelta = new Vector2(112,116);
            var layout = item.GetComponent<LayoutElement>(); layout.minWidth = layout.preferredWidth = 112; layout.minHeight = layout.preferredHeight = 116;
            Image hit = item.GetComponent<Image>(); hit.sprite = null; hit.color = Color.clear;
            Transform found = item.transform.Find("Img_HostFrame");
            var frameRect = found != null ? (RectTransform)found : (RectTransform)new GameObject("Img_HostFrame",typeof(RectTransform)).transform;
            frameRect.SetParent(item.transform,false); frameRect.SetAsFirstSibling();
            frameRect.anchorMin = frameRect.anchorMax = new Vector2(.5f,1); frameRect.pivot = new Vector2(.5f,1); frameRect.anchoredPosition = new Vector2(0,-4); frameRect.sizeDelta = new Vector2(80,80);
            var frame = frameRect.GetComponent<Image>() ?? frameRect.gameObject.AddComponent<Image>();
            frame.sprite = catalog.Get("SHR-046-normal"); frame.type = Image.Type.Simple; frame.preserveAspect = true; frame.raycastTarget = false; frame.color = Color.white;
            var icon = (RectTransform)item.transform.Find("Icon"); icon.anchorMin = icon.anchorMax = new Vector2(.5f,1); icon.pivot = new Vector2(.5f,.5f); icon.anchoredPosition = new Vector2(0,-44); icon.sizeDelta = new Vector2(48,48);
            var title = item.transform.Find("Txt_Title").GetComponent<TMP_Text>(); title.rectTransform.anchorMin = new Vector2(0,0); title.rectTransform.anchorMax = new Vector2(1,0); title.rectTransform.pivot = new Vector2(.5f,0); title.rectTransform.anchoredPosition = new Vector2(0,4); title.rectTransform.sizeDelta = new Vector2(-8,28); title.fontSize = 18; title.alignment = TextAlignmentOptions.Center;
            var button = item.GetComponent<Button>(); button.transition = Selectable.Transition.None; button.spriteState = default; button.targetGraphic = frame;
            Bind(item,"_background",frame);
        }

        private static void UpgradeSlider(Slider slider, Image fill, Image handle, TMP_Text value, UIFormalSpriteCatalog catalog)
        {
            var sliderRect = (RectTransform)slider.transform; sliderRect.sizeDelta = new Vector2(sliderRect.sizeDelta.x,32);
            Image rail = slider.GetComponent<Image>(); rail.sprite = catalog.Get("SHR-031"); rail.type = Image.Type.Sliced; rail.color = Color.white;
            fill.sprite = catalog.Get("SHR-032-normal"); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.color = Color.white;
            slider.fillRect = fill.rectTransform;
            var found = slider.transform.Find("HandleSlide");
            var slide = found != null ? (RectTransform)found : (RectTransform)new GameObject("HandleSlide",typeof(RectTransform)).transform;
            slide.SetParent(slider.transform,false); slide.anchorMin = new Vector2(0,.5f); slide.anchorMax = new Vector2(1,.5f); slide.anchoredPosition = Vector2.zero; slide.sizeDelta = new Vector2(-32,72);
            handle.rectTransform.SetParent(slide,false); handle.rectTransform.anchoredPosition = Vector2.zero; handle.rectTransform.sizeDelta = new Vector2(48,0);
            handle.sprite = catalog.Get("SHR-033-normal"); handle.type = Image.Type.Simple; handle.preserveAspect = true; handle.color = Color.white;
            slider.handleRect = handle.rectTransform; slider.targetGraphic = handle; slider.transition = Selectable.Transition.SpriteSwap;
            slider.spriteState = new SpriteState { highlightedSprite = catalog.Get("SHR-033-hover"), pressedSprite = catalog.Get("SHR-033-pressed"), disabledSprite = catalog.Get("SHR-033-disabled"), selectedSprite = catalog.Get("SHR-033-normal") };
            value.rectTransform.anchoredPosition = new Vector2(-20,-8); value.alignment = TextAlignmentOptions.Right;
        }

        private static void Bind(Object target, params object[] pairs)
        {
            var so = new SerializedObject(target);
            for (int i = 0; i < pairs.Length; i += 2) so.FindProperty((string)pairs[i]).objectReferenceValue = (Object)pairs[i+1];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
