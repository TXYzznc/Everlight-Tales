using System.Collections.Generic;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Everlight.Tales.UI
{
    /// <summary>图鉴三页动态 Item 渲染器。</summary>
    public sealed class CodexPanel : MonoBehaviour
    {
        [FormerlySerializedAs("_partItemTemplate")]
        [SerializeField] private GameObject _rowItemTemplate;
        private readonly ListRowCollection _partRows = new ListRowCollection();
        private readonly ListRowCollection _formRows = new ListRowCollection();
        private readonly ListRowCollection _caseRows = new ListRowCollection();
        [SerializeField] private RectTransform _partRoot;
        [SerializeField] private RectTransform _formRoot;
        [SerializeField] private RectTransform _caseRoot;
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;

        [SerializeField] private RectTransform _partViewport;
        [SerializeField] private RectTransform _formViewport;
        [SerializeField] private RectTransform _caseViewport;

        private int _subTab;
        [SerializeField] private TextMeshProUGUI _progressLabel;
        [SerializeField] private Button[] _subButtons = new Button[3];
        [SerializeField] private Transform _staticRoot;
        [SerializeField] private Transform _topRoot;
        [SerializeField] private Transform _listRoot;
        private UIFormBase _form;
        private bool _bound;

        public void BindStaticLayout()
        {
            if (_staticRoot == null || _topRoot == null || _listRoot == null || _progressLabel == null || _subButtons == null || _subButtons.Length != 3
                || _partViewport == null || _formViewport == null || _caseViewport == null || _partRoot == null || _formRoot == null || _caseRoot == null)
            {
                Debug.LogError("[CodexPanel][Contract] 页面 View 引用未完整绑定，拒绝运行时层级推断。", this);
                return;
            }
            _form = GetComponentInParent<UIFormBase>(true);
            _bound = _form != null && _progressLabel != null && _subButtons != null && _subButtons.Length == 3 && _subButtons[0] != null && _subButtons[1] != null && _subButtons[2] != null && _rowItemTemplate != null
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
            if (!_bound) return;
            // HomePage 先在未激活实例上 Build，再启用页面；Button.OnEnable 会恢复
            // Normal 视觉。挂载完成后的刷新必须重新同步业务选中状态。
            SyncSubTabSelection();
            if (WorldSession.Current == null) return;
            _progressLabel.text = "已拥有 " + CodexLayout.OwnedCount(WorldSession.Current.World) + "/" + CodexLayout.TotalCount;
            RebuildList();
        }

        private void SelectSubTab(int index)
        {
            if (index < 0 || index >= _subButtons.Length) return;
            _subTab = index;
            SyncSubTabSelection();
            if (_partViewport != null) _partViewport.gameObject.SetActive(index == 0);
            if (_formViewport != null) _formViewport.gameObject.SetActive(index == 1);
            if (_caseViewport != null) _caseViewport.gameObject.SetActive(index == 2);
            RebuildList();
        }

        private void SyncSubTabSelection()
        {
            for (int i = 0; i < _subButtons.Length; i++)
            {
                UIButtonStateUtility.SetSelected(_subButtons[i], i == _subTab);
            }
        }

        private void RebuildList()
        {
            if (WorldSession.Current == null) return;
            WorldState world = WorldSession.Current.World;
            if (_subTab == 0) RebuildEntries(CodexLayout.Parts(world), _partRows, _partRoot, false);
            else if (_subTab == 1) RebuildEntries(CodexLayout.Forms(world), _formRows, _formRoot, true);
            else RebuildCases(world);
        }

        private void RebuildEntries(IReadOnlyList<CodexEntry> entries, ListRowCollection rows, RectTransform root, bool form)
        {
            rows.Clear();
            foreach (CodexEntry entry in entries)
            {
                ListRowItemObject view = rows.Spawn(_form, _rowItemTemplate, root);
                Color color = entry.State == CodexState.Owned ? new Color(0.92f, 0.92f, 0.92f, 1f)
                    : entry.State == CodexState.Known ? new Color(0.55f, 0.62f, 0.70f, 1f)
                    : new Color32(110,118,132,255);
                string state = entry.State == CodexState.Owned ? (form ? "已解锁" : "已拥有") : entry.State == CodexState.Known ? "已知未拥有" : "？？？";
                string source = entry.State == CodexState.Unknown ? "" : entry.SourceHint;
                view.Bind(new ListRowData(CodexLayout.DisplayName(entry)) { Id = entry.Id, Detail = source, RightText = state,
                    Icon = _spriteCatalog.Object(entry.Id, form, entry.State != CodexState.Unknown), StateFrame = Frame(entry.State), TextColor = color,
                    Interactable = entry.State != CodexState.Unknown,
                    OnClick = () => GlobalUI.ShowDialog(CodexLayout.DisplayName(entry), "编号：" + entry.Id + "\n状态：" + state + "\n来源：" + source) });
            }
        }

        private void RebuildCases(WorldState world)
        {
            _caseRows.Clear();
            int shown = 0;
            foreach (CaseState c in world.Cases)
            {
                if (c.Kind == CaseStateKind.NotTriggered) continue;
                shown++;
                ListRowItemObject view = _caseRows.Spawn(_form, _rowItemTemplate, _caseRoot);
                view.Bind(new ListRowData(c.Config.Name) { Id = c.Config.Batch, RightText = CaseKindText(c.Kind),
                    Detail = c.Config.Source + (string.IsNullOrEmpty(c.Config.FirstPlace) ? "" : " · " + c.Config.FirstPlace), Icon = Icon("ICO-045"),
                    OnClick = () => GlobalUI.ShowDialog(c.Config.Name, "状态：" + CaseKindText(c.Kind) + "\n来源：" + c.Config.Source + "\n地点：" + c.Config.FirstPlace) });
            }
            if (shown == 0) SetEmpty(_caseRoot, "尚无已触发的怪谈档案");
            else SetEmpty(_caseRoot, null);
        }

        private Sprite Icon(string key) => _spriteCatalog != null ? _spriteCatalog.Get(key) : null;
        private Sprite Frame(CodexState state) => _spriteCatalog != null ? _spriteCatalog.Get(state == CodexState.Owned ? "SHR-046-selected" : "SHR-046-normal") : null;

        private static void SetEmpty(RectTransform root, string message)
        {
            if (root == null) return;
            Transform empty = root.Find("EmptyState") ?? root.parent?.Find("EmptyState");
            if (empty == null) { Debug.LogError("CodexPanel 缺少 EmptyState 静态节点。", root); return; }
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
