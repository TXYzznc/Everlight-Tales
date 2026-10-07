using System.Collections.Generic;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Everlight.Tales.UI
{
    /// <summary>保管区五类动态 Item 渲染器。页面只持有模板和容器，数据行由 GF 对象池创建。</summary>
    public sealed class ArchivePanel : MonoBehaviour
    {
        [FormerlySerializedAs("_displayItemTemplate")]
        [SerializeField] private GameObject _rowItemTemplate;
        private readonly ListRowCollection _displayRows = new ListRowCollection();
        private readonly ListRowCollection _ownedRows = new ListRowCollection();
        private readonly ListRowCollection _materialRows = new ListRowCollection();
        private readonly ListRowCollection _blueprintRows = new ListRowCollection();
        private readonly ListRowCollection _summaryRows = new ListRowCollection();
        [SerializeField] private RectTransform _displayRoot;
        [SerializeField] private RectTransform _ownedRoot;
        [SerializeField] private RectTransform _materialRoot;
        [SerializeField] private RectTransform _blueprintRoot;
        [SerializeField] private RectTransform _summaryRoot;
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;

        [SerializeField] private Transform _staticRoot;
        [SerializeField] private Transform _topRoot;
        [SerializeField] private Transform _listRoot;
        private UIFormBase _form;
        private bool _bound;
        [SerializeField] private Button[] _subButtons = new Button[5];
        [SerializeField] private RectTransform[] _subViews = new RectTransform[5];
        [SerializeField] private TMP_Text _sectionHeader;
        [SerializeField] private TMP_Text[] _emptyLabels = new TMP_Text[5];
        private static readonly string[] SectionTitles = { "陈列物", "拥有物", "材料", "图样", "汇总" };
        private int _subTab;

        public void BindStaticLayout()
        {
            if (_staticRoot == null || _topRoot == null || _listRoot == null || _subButtons == null || _subButtons.Length != 5 || _subViews == null || _subViews.Length != 5
                || _displayRoot == null || _ownedRoot == null || _materialRoot == null || _blueprintRoot == null || _summaryRoot == null
                || _sectionHeader == null || _emptyLabels == null || _emptyLabels.Length != 5)
            {
                Debug.LogError("[ArchivePanel][Contract] 页面 View 引用未完整绑定，拒绝运行时层级推断。", this);
                return;
            }
            for (int i = 0; i < _subViews.Length; i++)
            {
                if (_subButtons[i] == null || _subViews[i] == null || _emptyLabels[i] == null)
                {
                    Debug.LogError($"[ArchivePanel][Contract] 分类 {i} 缺少按钮、内容区或空状态文本引用。", this);
                    return;
                }
                for (int j = 0; j < i; j++)
                    if (_subViews[i] == _subViews[j])
                    {
                        Debug.LogError($"[ArchivePanel][Contract] 分类 {i} 与 {j} 引用了同一内容区。", this);
                        return;
                    }
            }
            _form = GetComponentInParent<UIFormBase>(true);
            _bound = _form != null && _rowItemTemplate != null && _displayRoot != null && _ownedRoot != null
                && _materialRoot != null && _blueprintRoot != null && _summaryRoot != null;
        }

        public void Build()
        {
            if (!_bound) BindStaticLayout();
            if (!_bound)
            {
                Debug.LogWarning("[ArchivePanel] 当前宿主未提供 ArchivePage 的完整 Item 容器，跳过该宿主刷新。", this);
                return;
            }
            for (int i = 0; i < _subButtons.Length; i++)
            {
                int index = i;
                if (_subButtons[i] == null) continue;
                _subButtons[i].onClick.RemoveAllListeners();
                _subButtons[i].onClick.AddListener(() => SelectSubTab(index));
            }
            SelectSubTab(0);
            Refresh();
        }

        private void SelectSubTab(int index)
        {
            _subTab = Mathf.Clamp(index, 0, _subViews.Length - 1);
            SyncSubTabSelection();
        }

        private void SyncSubTabSelection()
        {
            if (_sectionHeader != null) _sectionHeader.text = SectionTitles[_subTab];
            for (int i = 0; i < _subViews.Length; i++)
            {
                if (_subViews[i] != null) _subViews[i].gameObject.SetActive(i == _subTab);
                if (_subButtons[i] != null) UIButtonStateUtility.SetSelected(_subButtons[i], i == _subTab);
            }
        }

        public void Refresh()
        {
            if (!_bound) return;
            SyncSubTabSelection();
            if (WorldSession.Current == null) return;
            WorldState world = WorldSession.Current.World;
            RebuildDisplay(world);
            RebuildOwned(world);
            RebuildMaterials(world);
            RebuildBlueprints(world);
            RebuildSummary(world);
        }

        private void RebuildDisplay(WorldState world)
        {
            ClearRows(_displayRows, _displayRoot);
            if (world.DisplayItems.Count == 0)
            {
                SetEmpty(_displayRoot, "尚无陈列物");
                return;
            }
            SetEmpty(_displayRoot, null);
            foreach (DisplayItem item in world.DisplayItems)
            {
                ListRowItemObject view = _displayRows.Spawn(_form, _rowItemTemplate, _displayRoot);
                string detail = "第 " + item.ObtainedDay + " 天 · " + KindLabel(item.Kind);
                BindDetails(view, item.Name, detail, "", Icon("ICO-008保管"), Color.white);
            }
        }

        private void RebuildOwned(WorldState world)
        {
            ClearRows(_ownedRows, _ownedRoot);
            SetEmpty(_ownedRoot, null);
            ListRowItemObject fee = _ownedRows.Spawn(_form, _rowItemTemplate, _ownedRoot);
            BindDetails(fee, "维修费", "拥有物与已解锁形态", world.RepairFee.ToString(), Icon("ICO-051维修费"), Color.white);
            foreach (PartType part in world.OwnedParts)
            {
                ListRowItemObject view = _ownedRows.Spawn(_form, _rowItemTemplate, _ownedRoot);
                PartCodexConfig config = PartCodexCatalog.Get(part);
                BindDetails(view, config != null ? config.Name : part.ToString(), "永久零件", "已拥有", Icon("ICO-060零件"), Color.white);
            }
        }

        private void RebuildMaterials(WorldState world)
        {
            ClearRows(_materialRows, _materialRoot);
            SetEmpty(_materialRoot, null);
            foreach (MaterialConfig material in MaterialCatalog.All())
            {
                int count = TotalMaterial(world.Materials, material);
                Color color = count > 0 ? Color.white : new Color(0.5f, 0.5f, 0.52f, 1f);
                ListRowItemObject view = _materialRows.Spawn(_form, _rowItemTemplate, _materialRoot);
                BindDetails(view, material.Name, material.IsTypedByCase ? "类型化材料" : "材料库存", "×" + count, Icon(MaterialIconKey(material.Id)), color);
            }
        }

        private void RebuildBlueprints(WorldState world)
        {
            ClearRows(_blueprintRows, _blueprintRoot);
            if (world.Blueprints.Count == 0)
            {
                SetEmpty(_blueprintRoot, "暂无图样");
                return;
            }
            SetEmpty(_blueprintRoot, null);
            foreach (string blueprint in world.Blueprints)
            {
                ListRowItemObject view = _blueprintRows.Spawn(_form, _rowItemTemplate, _blueprintRoot);
                BindDetails(view, blueprint, "已取得图样", "已解锁", Icon("ICO-058图样"), Color.white);
            }
        }

        private void RebuildSummary(WorldState world)
        {
            ClearRows(_summaryRows, _summaryRoot);
            SetEmpty(_summaryRoot, null);
            ListRowItemObject holdings = _summaryRows.Spawn(_form, _rowItemTemplate, _summaryRoot);
            BindDetails(holdings, "永久零件与形态", "收集汇总", world.OwnedParts.Count + " 零件 / " + world.UnlockedForms.Count + " 形态", Icon("ICO-008保管"), Color.white);
            ListRowItemObject display = _summaryRows.Spawn(_form, _rowItemTemplate, _summaryRoot);
            BindDetails(display, "纪念物与档案", "陈列物总数", world.DisplayItems.Count.ToString(), Icon("ICO-008保管"), Color.white);
        }

        private static void BindDetails(ListRowItemObject row, string title, string detail, string value, Sprite icon, Color color)
        {
            string message = string.IsNullOrEmpty(value) ? detail : detail + "\n" + value;
            row.Bind(new ListRowData(title) { Detail = detail, RightText = value, Icon = icon, TextColor = color,
                OnClick = () => GlobalUI.ShowDialog(title, message) });
        }

        private void ClearRows(ListRowCollection rows, RectTransform root)
        {
            rows.Clear();
            if (root != null) SetEmpty(root, null);
        }

        private void SetEmpty(RectTransform root, string message)
        {
            if (root == null) return;
            int index = root == _displayRoot ? 0 : root == _ownedRoot ? 1 : root == _materialRoot ? 2 : root == _blueprintRoot ? 3 : root == _summaryRoot ? 4 : -1;
            if (index < 0 || _emptyLabels[index] == null) return;
            TMP_Text text = _emptyLabels[index];
            text.gameObject.SetActive(message != null);
            if (message != null) text.text = "（" + message + "）";
        }

        private Sprite Icon(string key) => _spriteCatalog != null ? _spriteCatalog.Get(key) : null;

        private static string MaterialIconKey(string id)
        {
            switch (id)
            {
                case "MT-001": return "ICO-052铜芯线";
                case "MT-002": return "ICO-053精密齿轮";
                case "MT-003": return "ICO-054玻璃镜片";
                case "MT-004": return "ICO-055校准簧片";
                case "MT-005": return "ICO-056定势残晶";
                default: return "ICO-057异常纹样";
            }
        }

        private static int TotalMaterial(MaterialBackpack backpack, MaterialConfig material)
        {
            int total = 0;
            foreach (MaterialStack stack in backpack.Stacks)
                if (stack.MaterialId == material.Id) total += stack.Count;
            return total;
        }

        private static string KindLabel(DisplayKind kind)
        {
            switch (kind)
            {
                case DisplayKind.LifeGift: return "回礼";
                case DisplayKind.Exhibition: return "展览";
                case DisplayKind.CaseMemento: return "纪念";
                default: return "维修";
            }
        }
    }
}
