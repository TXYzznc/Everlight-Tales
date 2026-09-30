using System.Collections.Generic;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>图鉴三页动态 Item 渲染器。</summary>
    public sealed class CodexPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _partItemTemplate;
        [SerializeField] private GameObject _formItemTemplate;
        [SerializeField] private GameObject _caseItemTemplate;
        [SerializeField] private RectTransform _partRoot;
        [SerializeField] private RectTransform _formRoot;
        [SerializeField] private RectTransform _caseRoot;
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;

        private int _subTab;
        private TextMeshProUGUI _progressLabel;
        private readonly Button[] _subButtons = new Button[3];
        private Transform _staticRoot;
        private UIFormBase _form;
        private bool _bound;

        public void BindStaticLayout()
        {
            _staticRoot = transform.name == "Panel_Codex" ? transform : transform.Find("Panel_Codex");
            if (_staticRoot == null) return;
            Transform top = _staticRoot.Find("Panel_CodexTop");
            Transform list = _staticRoot.Find("Panel_CodexList");
            _progressLabel = top != null ? top.Find("Txt_Progress")?.GetComponent<TextMeshProUGUI>() : null;
            for (int i = 0; i < _subButtons.Length; i++) _subButtons[i] = top != null ? top.Find("Btn_Sub_" + i)?.GetComponent<Button>() : null;
            if (list != null)
            {
                _partRoot = list.Find("PartScroll") as RectTransform;
                _formRoot = list.Find("FormScroll") as RectTransform;
                _caseRoot = list.Find("CaseScroll") as RectTransform;
            }
            _form = GetComponentInParent<UIFormBase>();
            _bound = _form != null && _partItemTemplate != null && _formItemTemplate != null && _caseItemTemplate != null
                && _partRoot != null && _formRoot != null && _caseRoot != null;
        }

        public void Build()
        {
            if (!_bound) BindStaticLayout();
            if (!_bound)
            {
                Debug.LogWarning("[CodexPanel] 当前宿主未提供 CodexPage 的完整 Item 容器，跳过该宿主刷新。", this);
                return;
            }
            for (int i = 0; i < _subButtons.Length; i++)
            {
                int index = i;
                _subButtons[i].onClick.RemoveAllListeners();
                _subButtons[i].onClick.AddListener(() => SelectSubTab(index));
            }
            SelectSubTab(0);
        }

        public void Refresh()
        {
            if (!_bound || WorldSession.Current == null) return;
            _progressLabel.text = "已拥有 " + CodexLayout.OwnedCount(WorldSession.Current.World) + "/" + CodexLayout.TotalCount;
            RebuildList();
        }

        private void SelectSubTab(int index)
        {
            _subTab = index;
            for (int i = 0; i < _subButtons.Length; i++)
            {
                UIFactory.SetSelected(_subButtons[i], i == index);
            }
            if (_partRoot != null) _partRoot.gameObject.SetActive(index == 0);
            if (_formRoot != null) _formRoot.gameObject.SetActive(index == 1);
            if (_caseRoot != null) _caseRoot.gameObject.SetActive(index == 2);
            RebuildList();
        }

        private void RebuildList()
        {
            if (WorldSession.Current == null) return;
            WorldState world = WorldSession.Current.World;
            if (_subTab == 0) RebuildEntries(CodexLayout.Parts(world), _partItemTemplate, _partRoot, false);
            else if (_subTab == 1) RebuildEntries(CodexLayout.Forms(world), _formItemTemplate, _formRoot, true);
            else RebuildCases(world);
        }

        private void RebuildEntries(IReadOnlyList<CodexEntry> entries, GameObject template, RectTransform root, bool form)
        {
            _form.UnspawnAllChildItem<CodexItemObject>(template);
            foreach (CodexEntry entry in entries)
            {
                CodexItemObject view = _form.SpawnChildItem<CodexItemObject>(template, root);
                Color color = entry.State == CodexState.Owned ? new Color(0.92f, 0.92f, 0.92f, 1f)
                    : entry.State == CodexState.Known ? new Color(0.55f, 0.62f, 0.70f, 1f)
                    : new Color(0.38f, 0.38f, 0.40f, 1f);
                string state = entry.State == CodexState.Owned ? (form ? "已解锁" : "已拥有") : entry.State == CodexState.Known ? "已知未拥有" : "？？？";
                string source = entry.State == CodexState.Unknown ? "" : entry.SourceHint;
                view.Bind(entry.Id, CodexLayout.DisplayName(entry), state, source, Icon(form ? "ICO-061形态" : "ICO-060零件"), Frame(entry.State), color, entry.State != CodexState.Unknown);
            }
        }

        private void RebuildCases(WorldState world)
        {
            _form.UnspawnAllChildItem<CodexItemObject>(_caseItemTemplate);
            int shown = 0;
            foreach (CaseState c in world.Cases)
            {
                if (c.Kind == CaseStateKind.NotTriggered) continue;
                shown++;
                CodexItemObject view = _form.SpawnChildItem<CodexItemObject>(_caseItemTemplate, _caseRoot);
                view.Bind(c.Config.Batch, c.Config.Name, CaseKindText(c.Kind), c.Config.Source + (string.IsNullOrEmpty(c.Config.FirstPlace) ? "" : " · " + c.Config.FirstPlace), Icon("ICO-008保管"), null, Color.white, true);
            }
            if (shown == 0) SetEmpty(_caseRoot, "尚无已触发的怪谈档案");
            else SetEmpty(_caseRoot, null);
        }

        private Sprite Icon(string key) => _spriteCatalog != null ? _spriteCatalog.Get(key) : null;
        private Sprite Frame(CodexState state) => _spriteCatalog != null ? _spriteCatalog.Get(state == CodexState.Owned ? "SHR-046-selected" : "SHR-046-normal") : null;

        private static void SetEmpty(RectTransform root, string message)
        {
            if (root == null) return;
            Transform empty = root.Find("EmptyState");
            if (empty == null && message != null)
            {
                GameObject go = new GameObject("EmptyState", typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(root, false);
                RectTransform rect = (RectTransform)go.transform; rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(0.5f, 1f); rect.anchoredPosition = new Vector2(0f, -18f); rect.sizeDelta = new Vector2(-40f, 64f);
                TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>(); text.font = UIFactory.BuiltinFont; text.fontSize = 24; text.alignment = TextAlignmentOptions.Center; text.color = new Color(0.5f, 0.5f, 0.52f, 1f);
                empty = go.transform;
            }
            if (empty != null) { empty.gameObject.SetActive(message != null); var text = empty.GetComponent<TextMeshProUGUI>(); if (text != null && message != null) text.text = "（" + message + "）"; }
        }

        private static string CaseKindText(CaseStateKind kind)
        {
            switch (kind)
            {
                case CaseStateKind.Investigating: return "调查中";
                case CaseStateKind.AwaitingRepair: return "待维修";
                case CaseStateKind.Repairing: return "维修中";
                case CaseStateKind.Resolved: return "已解决";
                case CaseStateKind.AwaitingRevisit: return "待回访";
                case CaseStateKind.Revisited: return "已回访";
                default: return "未触发";
            }
        }
    }
}
