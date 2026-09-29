using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI.EditorTools
{
    /// <summary>
    /// 将首批没有 JSON 契约的 UIForm 绑定到正式 UI Sprite。
    /// 规则按节点语义匹配，结果写回 Prefab；运行时不做资源查找。
    /// </summary>
    public static class EverlightFormalUiResourceBinder
    {
        private const string UiRoot = "Assets/Game/Sprites/UI/";

        [MenuItem("Game Framework/EverlightTales/UI/接入首批正式资源", priority = 2010)]
        public static void BindFirstBatch()
        {
            BindPrefab("Assets/Game/Prefabs/UI/MainPageShell.prefab");
            BindPrefab("Assets/Game/Prefabs/UI/DialogView.prefab");
            // BoardPage 的主要控件目前由 BoardPageForm/BoardPage 运行时装配，
            // 本轮不向领域逻辑注入资源查找；下一批迁移到契约后再绑定。
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EverlightFormalUiResourceBinder] 首批正式资源绑定完成。");
        }

        private static void BindPrefab(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (prefabPath.EndsWith("MainPageShell.prefab", StringComparison.Ordinal))
                {
                    EnsureMapLayout(root);
                    EnsureHomeLayout(root);
                    EnsureJournalLayout(root);
                    EnsureOpeningLayout(root);
                    EnsureTabIcons(root);
                }

                int count = 0;
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    Image image = child.GetComponent<Image>();
                    if (image == null)
                    {
                        continue;
                    }

                    string asset = ResolveAsset(child.name, prefabPath);
                    if (string.IsNullOrEmpty(asset))
                    {
                        continue;
                    }

                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + asset);
                    if (sprite == null)
                    {
                        Debug.LogError($"[EverlightFormalUiResourceBinder] 找不到 Sprite：{UiRoot + asset}（{prefabPath}/{child.name}）");
                        continue;
                    }

                    image.sprite = sprite;
                    image.type = Image.Type.Sliced;
                    image.color = Color.white;
                    count++;
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"[EverlightFormalUiResourceBinder] {prefabPath} 绑定 {count} 个 Sprite。");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void EnsureMapLayout(GameObject root)
        {
            Transform content = FindDescendant(root.transform, "Content");
            if (content == null) return;
            Transform existingPanel = FindDescendant(content, "Panel_Map");
            if (existingPanel != null)
            {
                EnsureMapTimeBar(existingPanel);
                EnsureMapNodeTemplates(existingPanel.Find("Panel_MapView"));
                Transform existingList = existingPanel.Find("Panel_Place/List_Event");
                if (existingList != null) EnsureEventCardTemplate(existingList);
                return;
            }

            GameObject panel = CreateRect("Panel_Map", content);
            Stretch(panel.GetComponent<RectTransform>());
            panel.AddComponent<CanvasGroup>().blocksRaycasts = true;

            GameObject timeBar = CreateRect("Panel_TimeBar", panel.transform);
            SetRect(timeBar.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 120f), Vector2.zero);
            var timeImage = timeBar.AddComponent<Image>();
            timeImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png");
            timeImage.type = Image.Type.Sliced;
            timeImage.color = new Color(0.08f, 0.10f, 0.14f, 0.90f);
            timeImage.raycastTarget = false;
            timeBar.AddComponent<Everlight.Tales.UI.TimePeriodBar>();
            AddText(timeBar.transform, "Txt_DayPeriod", "第1天 · 清晨", 26f, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-20f, 40f), new Vector2(20f, 10f), new Color(0.94f, 0.95f, 0.96f, 1f));
            Transform dayText = timeBar.transform.Find("Txt_DayPeriod");
            dayText.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Left;
            AddText(timeBar.transform, "Txt_Remaining", "剩4/4", 26f, new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 40f), new Vector2(-20f, 10f), new Color(0.94f, 0.95f, 0.96f, 1f));
            Transform remainingText = timeBar.transform.Find("Txt_Remaining");
            remainingText.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Right;
            for (int i = 0; i < 4; i++)
            {
                GameObject cell = CreateRect("Cell_" + i, timeBar.transform);
                SetRect(cell.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0.5f, 0f), new Vector2(56f, 14f), new Vector2(80f + i * 70f, 6f));
                var cellImage = cell.AddComponent<Image>();
                cellImage.color = TimePeriodBar.DayIdle;
                cellImage.raycastTarget = false;
            }

            GameObject track = CreateRect("Txt_Track", panel.transform);
            SetRect(track.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(920f, 40f), new Vector2(0f, -150f));
            var trackText = track.AddComponent<TMPro.TextMeshProUGUI>();
            trackText.font = TMPro.TMP_Settings.defaultFontAsset;
            trackText.fontSize = 20f;
            trackText.alignment = TMPro.TextAlignmentOptions.Center;
            trackText.raycastTarget = false;
            trackText.color = new Color(0.88f, 0.66f, 0.35f, 1f);

            GameObject map = CreateRect("Panel_MapView", panel.transform);
            SetRect(map.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(-40f, -360f), Vector2.zero);
            var mapImage = map.AddComponent<Image>();
            mapImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png");
            mapImage.type = Image.Type.Sliced;
            mapImage.color = new Color(0.10f, 0.12f, 0.16f, 0.35f);
            mapImage.raycastTarget = false;
            EnsureMapNodeTemplates(map.transform);

            GameObject place = CreateRect("Panel_Place", panel.transform);
            SetRect(place.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(-40f, 360f), new Vector2(0f, 0f));
            var placeImage = place.AddComponent<Image>();
            placeImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png");
            placeImage.type = Image.Type.Sliced;
            placeImage.color = Color.white;
            placeImage.raycastTarget = false;

            AddText(place.transform, "Txt_PlaceLabel", "点击地图节点查看地点", 28f, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(920f, 40f), new Vector2(0f, -28f));
            AddText(place.transform, "Txt_PlaceDesc", "", 20f, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(920f, 56f), new Vector2(0f, -72f));

            GameObject list = CreateRect("List_Event", place.transform);
            SetRect(list.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-40f, 30f), new Vector2(0f, 120f));
            EnsureEventCardTemplate(list.transform);

            GameObject wait = CreateRect("Btn_Wait", place.transform);
            SetRect(wait.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(360f, 52f), new Vector2(0f, 24f));
            var waitImage = wait.AddComponent<Image>();
            waitImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-021-normal主按钮.png");
            waitImage.type = Image.Type.Sliced;
            waitImage.color = Color.white;
            var waitButton = wait.AddComponent<Button>();
            waitButton.targetGraphic = waitImage;
            AddText(wait.transform, "Txt_WaitLabel", "等待到下一时段", 24f, Vector2.zero, Vector2.one, new Vector2(-16f, -8f), Vector2.zero, new Color(0.09f, 0.10f, 0.125f, 1f));
        }

        private static void EnsureMapTimeBar(Transform panel)
        {
            if (panel.Find("Panel_TimeBar") != null) return;
            GameObject timeBar = CreateRect("Panel_TimeBar", panel);
            SetRect(timeBar.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 120f), Vector2.zero);
            var image = timeBar.AddComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png");
            image.type = Image.Type.Sliced; image.color = new Color(0.08f, 0.10f, 0.14f, 0.90f); image.raycastTarget = false;
            timeBar.AddComponent<Everlight.Tales.UI.TimePeriodBar>();
            AddText(timeBar.transform, "Txt_DayPeriod", "第1天 · 清晨", 26f, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-20f, 40f), new Vector2(20f, 10f));
            timeBar.transform.Find("Txt_DayPeriod").GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Left;
            AddText(timeBar.transform, "Txt_Remaining", "剩4/4", 26f, new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 40f), new Vector2(-20f, 10f));
            timeBar.transform.Find("Txt_Remaining").GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Right;
            for (int i = 0; i < 4; i++)
            {
                GameObject cell = CreateRect("Cell_" + i, timeBar.transform);
                SetRect(cell.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0f), new Vector2(56f, 14f), new Vector2(80f + i * 70f, 6f));
                var cellImage = cell.AddComponent<Image>(); cellImage.color = TimePeriodBar.DayIdle; cellImage.raycastTarget = false;
            }
        }

        private static void EnsureMapNodeTemplates(Transform mapRoot)
        {
            if (mapRoot == null) return;
            string[] ids = { "home", "street", "dance", "tailor", "community" };
            string[] sprites =
            {
                "图标/地点/ICO-071-unlocked长明修理铺.png",
                "图标/地点/ICO-072-unlocked街区小店.png",
                "图标/地点/ICO-073-unlocked舞蹈教室.png",
                "图标/地点/ICO-074-unlocked裁缝铺.png",
                "图标/地点/ICO-075-unlocked社区活动中心.png"
            };
            for (int i = 0; i < ids.Length; i++)
            {
                if (mapRoot.Find("NodeTemplate_" + ids[i]) != null) continue;
                GameObject node = CreateRect("NodeTemplate_" + ids[i], mapRoot);
                node.SetActive(false);
                SetRect(node.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(44f, 44f), Vector2.zero);
                Image image = node.AddComponent<Image>();
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + sprites[i]);
                image.color = Color.white;
                image.raycastTarget = true;
                Button button = node.AddComponent<Button>();
                button.targetGraphic = image;
            }
        }

        private static void EnsureEventCardTemplate(Transform list)
        {
            if (list.Find("EventCardTemplate") != null) return;
            GameObject card = CreateRect("EventCardTemplate", list);
            card.SetActive(false);
            SetRect(card.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 56f), Vector2.zero);
            var image = card.AddComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-003-normal卡片.png");
            image.type = Image.Type.Sliced; image.color = Color.white;
            var button = card.AddComponent<Button>(); button.targetGraphic = image;
            AddText(card.transform, "Txt_EventName", "事件名称", 22f, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 30f), new Vector2(0f, 10f));
            Transform name = card.transform.Find("Txt_EventName"); name.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Left;
            AddText(card.transform, "Txt_EventMeta", "类型 · 耗时", 16f, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 24f), new Vector2(0f, -16f), new Color(0.68f, 0.71f, 0.75f, 1f));
            Transform meta = card.transform.Find("Txt_EventMeta"); meta.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Left;
        }

        private static void EnsureHomeLayout(GameObject root)
        {
            Transform content = FindDescendant(root.transform, "Content");
            if (content == null) return;
            Transform home = FindDescendant(content, "Panel_Home");
            if (home == null)
            {
                GameObject panel = CreateRect("Panel_Home", content);
                Stretch(panel.GetComponent<RectTransform>());
                panel.AddComponent<CanvasGroup>().blocksRaycasts = true;
                home = panel.transform;
            }
            if (home.Find("Panel_HomeTop") == null)
            {
                GameObject top = CreateRect("Panel_HomeTop", home);
                SetRect(top.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 96f), Vector2.zero);
                var topImage = top.AddComponent<Image>(); topImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png"); topImage.type = Image.Type.Sliced; topImage.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
                string[] labels = { "来客", "加工", "收藏", "保管", "服务" };
                for (int i = 0; i < labels.Length; i++)
                {
                    GameObject button = CreateRect("Btn_Zone_" + i, top.transform);
                    SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(170f, 56f), new Vector2(-340f + i * 170f, -20f));
                    var image = button.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-028-normal页签5.png"); image.type = Image.Type.Sliced; image.color = Color.white;
                    var buttonComp = button.AddComponent<Button>(); buttonComp.targetGraphic = image;
                    AddText(button.transform, "Txt_Label", labels[i], 24f, Vector2.zero, Vector2.one, new Vector2(-12f, -8f), Vector2.zero, new Color(0.09f, 0.10f, 0.125f, 1f));
                }
            }
            if (home.Find("Panel_HomeArea") == null)
            {
                GameObject area = CreateRect("Panel_HomeArea", home);
                SetRect(area.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, -96f), new Vector2(0f, -48f));
            }
            Transform homeArea = home.Find("Panel_HomeArea");
            string[] contentNames = { "Panel_Workbench", "Panel_Codex", "Panel_Archive", "Panel_Guest", "Panel_Service" };
            foreach (string name in contentNames)
            {
                Transform existing = homeArea.Find(name);
                if (existing != null)
                {
                if (name == "Panel_Workbench" && existing.Find("Panel_WorkbenchTop") == null) EnsureWorkbenchLayout(existing);
                if (name == "Panel_Codex" && existing.Find("Panel_CodexTop") == null) EnsureCodexLayout(existing);
                if (name == "Panel_Archive" && existing.Find("Panel_ArchiveTop") == null) EnsureArchiveLayout(existing);
                if (name == "Panel_Guest" && existing.Find("Panel_GuestContent") == null) EnsureContentRoot(existing, "Panel_GuestContent");
                if (name == "Panel_Service" && existing.Find("Panel_ServiceContent") == null) EnsureContentRoot(existing, "Panel_ServiceContent");
                continue;
                }
                GameObject child = CreateRect(name, homeArea);
                Stretch(child.GetComponent<RectTransform>());
                child.AddComponent<CanvasGroup>().blocksRaycasts = true;
                child.SetActive(false);
                if (name == "Panel_Workbench") EnsureWorkbenchLayout(child.transform);
                if (name == "Panel_Codex") EnsureCodexLayout(child.transform);
                if (name == "Panel_Archive") EnsureArchiveLayout(child.transform);
                if (name == "Panel_Guest") EnsureContentRoot(child.transform, "Panel_GuestContent");
                if (name == "Panel_Service") EnsureContentRoot(child.transform, "Panel_ServiceContent");
            }
        }

        private static void EnsureContentRoot(Transform parent, string name)
        {
            GameObject content = CreateRect(name, parent);
            Stretch(content.GetComponent<RectTransform>());
            var image = content.AddComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png");
            image.type = Image.Type.Sliced;
            image.color = new Color(0.08f, 0.10f, 0.14f, 0.40f);
            image.raycastTarget = false;
        }

        private static void EnsureOpeningLayout(GameObject root)
        {
            if (FindDescendant(root.transform, "Panel_OpeningOverlay") != null) return;
            GameObject panel = CreateRect("Panel_OpeningOverlay", root.transform);
            Stretch(panel.GetComponent<RectTransform>());
            panel.SetActive(false);
            var image = panel.AddComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-002对话框.png");
            image.type = Image.Type.Sliced; image.color = new Color(0.02f, 0.03f, 0.05f, 0.97f);
            GameObject subtitle = CreateRect("Txt_OpeningSubtitle", panel.transform);
            SetRect(subtitle.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(940f, 140f), new Vector2(0f, -160f));
            var subtitleText = subtitle.AddComponent<TMPro.TextMeshProUGUI>(); subtitleText.font = TMPro.TMP_Settings.defaultFontAsset; subtitleText.fontSize = 30f; subtitleText.alignment = TMPro.TextAlignmentOptions.Center; subtitleText.color = Color.white; subtitleText.raycastTarget = false;
            GameObject button = CreateRect("Btn_OpeningAction", panel.transform);
            SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(260f, 56f), new Vector2(0f, -360f));
            var buttonImage = button.AddComponent<Image>(); buttonImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-021-normal主按钮.png"); buttonImage.type = Image.Type.Sliced;
            var buttonComponent = button.AddComponent<Button>(); buttonComponent.targetGraphic = buttonImage;
            AddText(button.transform, "Txt_ActionLabel", "跳过", 24f, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        }

        private static void EnsureTabIcons(GameObject root)
        {
            string[] assets = { "图标/ICO-001地图.png", "图标/ICO-002任务.png", "图标/ICO-003家园.png", "图标/ICO-004工作台.png", "图标/ICO-012设置.png" };
            for (int i = 0; i < assets.Length; i++)
            {
                Transform tab = FindDescendant(root.transform, "Tab_" + i);
                if (tab == null || tab.Find("Img_Icon") != null) continue;
                GameObject icon = CreateRect("Img_Icon", tab);
                SetRect(icon.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(38f, 38f), new Vector2(0f, -10f));
                var image = icon.AddComponent<Image>();
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + assets[i]);
                image.preserveAspect = true; image.raycastTarget = false;
            }
        }

        private static void EnsureCodexLayout(Transform root)
        {
            GameObject top = CreateRect("Panel_CodexTop", root);
            SetRect(top.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 96f), Vector2.zero);
            var image = top.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png"); image.type = Image.Type.Sliced; image.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
            AddText(top.transform, "Txt_Progress", "已拥有 0/0", 26f, new Vector2(0.5f, 0.5f), Vector2.one, new Vector2(-40f, 36f), new Vector2(-24f, -24f));
            top.transform.Find("Txt_Progress").GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Right;
            string[] labels = { "零件 P", "形态 M", "资料台" };
            for (int i = 0; i < labels.Length; i++)
            {
                GameObject button = CreateRect("Btn_Sub_" + i, top.transform);
                SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(140f, 56f), new Vector2(-430f + i * 150f, -24f));
                var btnImage = button.AddComponent<Image>(); btnImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-028-normal页签5.png"); btnImage.type = Image.Type.Sliced; btnImage.color = Color.white;
                var btn = button.AddComponent<Button>(); btn.targetGraphic = btnImage;
                AddText(button.transform, "Txt_Label", labels[i], 24f, Vector2.zero, Vector2.one, new Vector2(-12f, -8f), Vector2.zero, new Color(0.09f, 0.10f, 0.125f, 1f));
            }
            GameObject list = CreateRect("Panel_CodexList", root);
            SetRect(list.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, -96f), new Vector2(0f, -48f));
        }

        private static void EnsureArchiveLayout(Transform root)
        {
            GameObject top = CreateRect("Panel_ArchiveTop", root);
            SetRect(top.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 96f), Vector2.zero);
            var image = top.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png"); image.type = Image.Type.Sliced; image.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
            AddText(top.transform, "Txt_Title", "陈列 / 档案", 28f, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300f, 36f), new Vector2(24f, -24f), new Color(1f, 0.85f, 0.35f, 1f));
            GameObject list = CreateRect("Panel_ArchiveList", root);
            SetRect(list.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, -96f), new Vector2(0f, -48f));
        }

        private static void EnsureWorkbenchLayout(Transform root)
        {
            GameObject top = CreateRect("Panel_WorkbenchTop", root);
            SetRect(top.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 96f), Vector2.zero);
            var topImage = top.AddComponent<Image>(); topImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png"); topImage.type = Image.Type.Sliced; topImage.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
            AddText(top.transform, "Txt_Fee", "维修费 0", 26f, new Vector2(0.5f, 0.5f), Vector2.one, new Vector2(-40f, 36f), new Vector2(-24f, -24f));
            top.transform.Find("Txt_Fee").GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Right;
            string[] labels = { "加工", "材料", "账目" };
            for (int i = 0; i < labels.Length; i++)
            {
                GameObject button = CreateRect("Btn_Sub_" + i, top.transform);
                SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(140f, 56f), new Vector2(-430f + i * 150f, -24f));
                var image = button.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-028-normal页签5.png"); image.type = Image.Type.Sliced; image.color = Color.white;
                var buttonComp = button.AddComponent<Button>(); buttonComp.targetGraphic = image;
                AddText(button.transform, "Txt_Label", labels[i], 24f, Vector2.zero, Vector2.one, new Vector2(-12f, -8f), Vector2.zero, new Color(0.09f, 0.10f, 0.125f, 1f));
            }
            GameObject body = CreateRect("Panel_WorkbenchBody", root);
            SetRect(body.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, -96f), new Vector2(0f, -48f));
            string[] bodies = { "Panel_Workbench", "Panel_Materials", "Panel_Ledger" };
            foreach (string bodyName in bodies)
            {
                GameObject child = CreateRect(bodyName, body.transform);
                Stretch(child.GetComponent<RectTransform>());
                child.AddComponent<CanvasGroup>().blocksRaycasts = true;
                child.SetActive(false);
            }
        }

        private static void EnsureJournalLayout(GameObject root)
        {
            Transform content = FindDescendant(root.transform, "Content");
            if (content == null) return;
            Transform journal = FindDescendant(content, "Panel_Journal");
            if (journal == null)
            {
                GameObject panel = CreateRect("Panel_Journal", content);
                Stretch(panel.GetComponent<RectTransform>());
                panel.AddComponent<CanvasGroup>().blocksRaycasts = true;
                journal = panel.transform;
            }
            if (journal.Find("Panel_JournalTop") == null)
            {
                GameObject top = CreateRect("Panel_JournalTop", journal);
                SetRect(top.GetComponent<RectTransform>(), new Vector2(0f, 1f), Vector2.one, new Vector2(0.5f, 1f), new Vector2(0f, 96f), Vector2.zero);
                var topImage = top.AddComponent<Image>(); topImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "九宫格/SHR-001面板.png"); topImage.type = Image.Type.Sliced; topImage.color = new Color(0.08f, 0.10f, 0.14f, 0.94f);
                AddText(top.transform, "Txt_Balance", "维修费 0", 26f, new Vector2(0.5f, 0.5f), Vector2.one, new Vector2(-40f, 36f), new Vector2(-24f, -24f));
                top.transform.Find("Txt_Balance").GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Right;
                string[] labels = { "事件", "任务", "怪谈" };
                for (int i = 0; i < labels.Length; i++)
                {
                    GameObject button = CreateRect("Btn_Sub_" + i, top.transform);
                    SetRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(140f, 56f), new Vector2(-430f + i * 150f, -24f));
                    var image = button.AddComponent<Image>(); image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(UiRoot + "控件/SHR-028-normal页签5.png"); image.type = Image.Type.Sliced; image.color = Color.white;
                    var buttonComp = button.AddComponent<Button>(); buttonComp.targetGraphic = image;
                    AddText(button.transform, "Txt_Label", labels[i], 24f, Vector2.zero, Vector2.one, new Vector2(-12f, -8f), Vector2.zero, new Color(0.09f, 0.10f, 0.125f, 1f));
                    AddText(top.transform, "Txt_Badge_" + i, "", 20f, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(36f, 26f), new Vector2(-376f + i * 150f, -6f), new Color(0.88f, 0.66f, 0.35f, 1f));
                }
            }
            if (journal.Find("Panel_JournalList") == null)
            {
                GameObject list = CreateRect("Panel_JournalList", journal);
                SetRect(list.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, -96f), new Vector2(0f, -48f));
            }
        }

        private static GameObject CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform rect)
        {
            SetRect(rect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot, Vector2 size, Vector2 position)
        {
            rect.anchorMin = min; rect.anchorMax = max; rect.pivot = pivot; rect.sizeDelta = size; rect.anchoredPosition = position;
        }

        private static void AddText(Transform parent, string name, string value, float size, Vector2 min, Vector2 max, Vector2 dimensions, Vector2 position, Color? color = null)
        {
            GameObject go = CreateRect(name, parent);
            RectTransform rect = go.GetComponent<RectTransform>();
            SetRect(rect, min, max, new Vector2(0.5f, 0.5f), dimensions, position);
            var text = go.AddComponent<TMPro.TextMeshProUGUI>();
            text.font = TMPro.TMP_Settings.defaultFontAsset; text.fontSize = size; text.text = value;
            text.alignment = TMPro.TextAlignmentOptions.Center; text.raycastTarget = false;
            text.color = color ?? new Color(0.94f, 0.95f, 0.96f, 1f);
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                Transform hit = FindDescendant(child, name);
                if (hit != null) return hit;
            }
            return null;
        }

        private static string ResolveAsset(string nodeName, string prefabPath)
        {
            if (prefabPath.EndsWith("MainPageShell.prefab", StringComparison.Ordinal))
            {
                if (nodeName.StartsWith("Tab_", StringComparison.Ordinal))
                {
                    return "控件/SHR-028-normal页签5.png";
                }

                if (nodeName == "TopBar")
                {
                    return "九宫格/SHR-006-night顶部条.png";
                }
            }

            if (prefabPath.EndsWith("DialogView.prefab", StringComparison.Ordinal))
            {
                switch (nodeName)
                {
                    case "Panel":
                        return "九宫格/SHR-002对话框.png";
                    case "Button_0":
                        return "控件/SHR-021-normal主按钮.png";
                    case "Button_1":
                        return "控件/SHR-022-normal次按钮.png";
                }
            }

            return null;
        }
    }
}
