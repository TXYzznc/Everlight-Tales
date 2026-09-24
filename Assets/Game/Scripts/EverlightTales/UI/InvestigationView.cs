using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Everlight.Tales.UI
{
    /// <summary>
    /// 现场调查界面（P2 叙事层，D-015 表现）：场景占位 + 可点击热点。
    /// 点热点高亮并计数，全部确认后回调（对应 InvestigationService.ConfirmAnomaly）。
    /// 程序化构建，自带 ScreenSpaceOverlay Canvas（sortingOrder 600）。
    /// 热点具体内容（工具包/施工记录/画中敲击等）与场景立绘由调查事件配置 + 美术填充。
    /// </summary>
    public sealed class InvestigationView : MonoBehaviour
    {
        /// <summary>一个可点击调查热点。</summary>
        public sealed class Hotspot
        {
            public string Name;

            /// <summary>归一化位置（0~1，左下原点）。</summary>
            public Vector2 Position;

            public Action OnTap;
        }

        private readonly List<(Hotspot Hotspot, Image Ring, TextMeshProUGUI Label)> m_Items =
            new List<(Hotspot, Image, TextMeshProUGUI)>();

        private TextMeshProUGUI m_SceneLabel;

        private Action m_OnComplete;

        private bool m_Finished;

        private int m_ConfirmedCount;

        /// <summary>创建全屏调查面板；全部热点确认后回调并自毁。</summary>
        public static InvestigationView Show(string sceneName, IReadOnlyList<Hotspot> hotspots, Action onComplete)
        {
            var go = new GameObject("investigation_view", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(InvestigationView));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 600;
            var view = go.GetComponent<InvestigationView>();
            view.Play(sceneName, hotspots, onComplete);
            return view;
        }

        public void Play(string sceneName, IReadOnlyList<Hotspot> hotspots, Action onComplete)
        {
            m_OnComplete = onComplete;
            BuildUI(sceneName, hotspots);
        }

        private void BuildUI(string sceneName, IReadOnlyList<Hotspot> hotspots)
        {
            var rt = (RectTransform)transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color(0.03f, 0.04f, 0.06f, 0.97f);
            bg.raycastTarget = true;

            m_SceneLabel = MakeText(transform, "scene", new Vector2(0f, 340f), new Vector2(920f, 44f), 28, TextAlignmentOptions.Center);
            m_SceneLabel.color = new Color(0.88f, 0.66f, 0.35f, 1f);
            m_SceneLabel.text = sceneName ?? "现场调查";

            // 场景占位（中央大块）。
            var sceneGo = new GameObject("scene_placeholder", typeof(RectTransform), typeof(Image));
            sceneGo.transform.SetParent(transform, false);
            var sceneRt = (RectTransform)sceneGo.transform;
            sceneRt.anchorMin = new Vector2(0.5f, 0.5f);
            sceneRt.anchorMax = new Vector2(0.5f, 0.5f);
            sceneRt.pivot = new Vector2(0.5f, 0.5f);
            sceneRt.anchoredPosition = new Vector2(0f, 120f);
            sceneRt.sizeDelta = new Vector2(920f, 720f);
            sceneGo.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 1f);

            if (hotspots != null)
            {
                foreach (Hotspot hotspot in hotspots)
                {
                    AddHotspot(sceneRt, hotspot);
                }
            }

            var closeBtn = MakeButton(transform, "btn_finish", new Vector2(0f, -420f), new Vector2(360f, 56f), "完成调查");
            closeBtn.onClick.AddListener(Finish);
        }

        private void AddHotspot(RectTransform sceneRt, Hotspot hotspot)
        {
            var go = new GameObject("hotspot", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(sceneRt, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(140f, 140f);
            rt.anchoredPosition = new Vector2(
                (hotspot.Position.x - 0.5f) * sceneRt.sizeDelta.x,
                (hotspot.Position.y - 0.5f) * sceneRt.sizeDelta.y);

            var ring = go.GetComponent<Image>();
            ring.color = new Color(0.66f, 0.42f, 0.78f, 0.35f);

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.fontSize = 18;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.text = hotspot.Name;

            m_Items.Add((hotspot, ring, label));

            var captured = hotspot;
            go.GetComponent<Button>().onClick.AddListener(() => OnHotspotTap(captured, ring, label));
        }

        private void OnHotspotTap(Hotspot hotspot, Image ring, TextMeshProUGUI label)
        {
            if (m_ConfirmedCount >= m_Items.Count)
            {
                return;
            }

            // 高亮为已确认（冷紫 → 暖铜，标签加勾）。
            ring.color = new Color(0.88f, 0.66f, 0.35f, 0.55f);
            label.text = hotspot.Name + " ✓";
            m_ConfirmedCount++;
            hotspot.OnTap?.Invoke();

            if (m_ConfirmedCount >= m_Items.Count)
            {
                Finish();
            }
        }

        private void Finish()
        {
            if (m_Finished)
            {
                return;
            }

            m_Finished = true;
            Action callback = m_OnComplete;
            m_OnComplete = null;
            callback?.Invoke();
            Destroy(gameObject);
        }

        private static TextMeshProUGUI MakeText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, TextAlignmentOptions anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = anchor;
            text.raycastTarget = false;
            return text;
        }

        private static Button MakeButton(Transform parent, string name, Vector2 pos, Vector2 size, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = new Color(0.30f, 0.42f, 0.55f, 1f);

            var labelGo = new GameObject("label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(go.transform, false);
            var labelRt = (RectTransform)labelGo.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var text = labelGo.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.text = label;

            return go.GetComponent<Button>();
        }
    }
}
