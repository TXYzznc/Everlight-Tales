using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>调查页的表现组件，页面负责数据注入与 GF 生命周期。</summary>
    public sealed class InvestigationView : MonoBehaviour
    {
        [SerializeField] private GameObject m_HotspotTemplate;
        [SerializeField] private TextMeshProUGUI m_SceneLabel;
        [SerializeField] private RectTransform m_StaticSceneRoot;
        [SerializeField] private Button m_StaticFinishButton;
        [SerializeField] private UIFormBase m_Form;
        [SerializeField] private TMP_Text _progress;
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;
        [SerializeField] private Image _sceneObject;
        private readonly List<InvestigationHotspotItemObject> _items = new List<InvestigationHotspotItemObject>();
        private Action _onComplete;
        private bool _finished;
        private bool _playing;
        private bool _autoFinish;
        private int _confirmedCount;
        public int ConfirmedCount => _confirmedCount;
        public int HotspotCount => _items.Count;

        public sealed class Hotspot
        {
            public string Name;
            /// <summary>归一化位置，左下为 (0,0)。</summary>
            public Vector2 Position;
            public Action OnTap;
        }

        public void SetHotspotTemplate(GameObject template) => m_HotspotTemplate = template;
        public void BindStaticLayout()
        {
            if (m_SceneLabel == null || m_StaticSceneRoot == null || m_StaticFinishButton == null || m_Form == null || _progress == null || m_HotspotTemplate == null)
                throw new InvalidOperationException("[InvestigationView][Contract] 调查页序列化引用不完整。");
        }

        /// <summary>兼容旧调用，但仍由正式 UIForm 加载与关闭。</summary>
        public static InvestigationView Show(string sceneName, IReadOnlyList<Hotspot> hotspots, Action onComplete)
        {
            InvestigationPageForm.Open(new InvestigationPageData(sceneName, hotspots, onComplete));
            return null; // GF 异步加载，不能同步返回尚未创建的组件。
        }

        public void Play(string sceneName, IReadOnlyList<Hotspot> hotspots, Action onComplete, bool autoFinish = true, string caseId = null)
        {
            Stop();
            BindStaticLayout();
            _playing = true; _finished = false; _autoFinish = autoFinish; _onComplete = onComplete;
            m_SceneLabel.text = sceneName ?? "现场调查";
            _sceneObject.sprite = caseId == "L-01" || caseId == "L01" ? _spriteCatalog.Get("红舞鞋") : null;
            _sceneObject.gameObject.SetActive(_sceneObject.sprite != null);
            if (hotspots != null)
                foreach (Hotspot hotspot in hotspots) if (hotspot != null) AddHotspot(hotspot);
            m_StaticFinishButton.onClick.RemoveAllListeners();
            m_StaticFinishButton.onClick.AddListener(Finish);
            RefreshProgress();
        }

        public void ShowEmpty()
        {
            Stop(); BindStaticLayout();
            m_SceneLabel.text = "现场调查";
            _sceneObject.gameObject.SetActive(false);
            _progress.text = "没有待调查的现场";
            m_StaticFinishButton.interactable = false;
            UIButtonStateUtility.SetSelected(m_StaticFinishButton, false);
        }

        private void AddHotspot(Hotspot hotspot)
        {
            InvestigationHotspotItemObject item = m_Form.SpawnChildItem<InvestigationHotspotItemObject>(m_HotspotTemplate, m_StaticSceneRoot);
            RectTransform rect = (RectTransform)item.gameObject.transform;
            // 用归一化锚点适配容器实际尺寸；拉伸容器的 sizeDelta 可能为零。
            rect.anchorMin = rect.anchorMax = new Vector2(Mathf.Clamp01(hotspot.Position.x), Mathf.Clamp01(hotspot.Position.y));
            rect.pivot = Vector2.one * 0.5f; rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(140, 140);
            _items.Add(item);
            item.Bind(hotspot.Name, () => OnHotspotTap(item, hotspot));
        }

        private void OnHotspotTap(InvestigationHotspotItemObject item, Hotspot hotspot)
        {
            if (!_playing || _finished || !item.View.IsConfirmed) return;
            _confirmedCount++;
            RefreshProgress();
            // 先锁定热点与计数，再执行外部回调；支持回调关闭或重开本页。
            int generation = _generation;
            hotspot.OnTap?.Invoke();
            if (generation == _generation && _playing && !_finished && _autoFinish && _confirmedCount == _items.Count) Finish();
        }

        private void RefreshProgress()
        {
            _progress.text = "已确认 " + _confirmedCount + "/" + _items.Count;
            bool complete = _items.Count > 0 && _confirmedCount == _items.Count;
            m_StaticFinishButton.interactable = complete;
            UIButtonStateUtility.SetSelected(m_StaticFinishButton, complete);
        }

        private void Finish()
        {
            if (!_playing || _finished || _items.Count == 0 || _confirmedCount != _items.Count) return;
            _finished = true;
            Action callback = _onComplete; _onComplete = null;
            // 通过 GF 关闭，不销毁可复用页面的子节点。
            m_Form.OnClickClose();
            callback?.Invoke();
        }

        private int _generation;
        public void Stop()
        {
            _generation++; _playing = false; _finished = false; _confirmedCount = 0; _onComplete = null;
            if (_items.Count > 0 && m_Form != null && m_HotspotTemplate != null)
                m_Form.UnspawnAllChildItem<InvestigationHotspotItemObject>(m_HotspotTemplate);
            _items.Clear();
            if (m_StaticFinishButton != null) m_StaticFinishButton.onClick.RemoveAllListeners();
        }
    }
}
