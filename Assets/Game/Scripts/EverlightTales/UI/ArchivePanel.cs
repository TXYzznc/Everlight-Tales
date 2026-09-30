using System.Collections.Generic;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using TMPro;
using UnityEngine;

namespace Everlight.Tales.UI
{
    /// <summary>保管区五类动态 Item 渲染器。页面只持有模板和容器，数据行由 GF 对象池创建。</summary>
    public sealed class ArchivePanel : MonoBehaviour
    {
        [SerializeField] private GameObject _displayItemTemplate;
        [SerializeField] private GameObject _ownedItemTemplate;
        [SerializeField] private GameObject _materialItemTemplate;
        [SerializeField] private GameObject _blueprintItemTemplate;
        [SerializeField] private GameObject _summaryItemTemplate;
        [SerializeField] private RectTransform _displayRoot;
        [SerializeField] private RectTransform _ownedRoot;
        [SerializeField] private RectTransform _materialRoot;
        [SerializeField] private RectTransform _blueprintRoot;
        [SerializeField] private RectTransform _summaryRoot;
        [SerializeField] private UIFormalSpriteCatalog _spriteCatalog;

        private Transform _staticRoot;
        private UIFormBase _form;
        private bool _bound;

        public void BindStaticLayout()
        {
            _staticRoot = transform.name == "Panel_Archive" ? transform : transform.Find("Panel_Archive");
            if (_staticRoot == null) return;
            Transform list = _staticRoot.Find("Panel_ArchiveList");
            if (list != null)
            {
                _displayRoot = list.Find("Content_Display") as RectTransform;
                _ownedRoot = list.Find("Content_Owned") as RectTransform;
                _materialRoot = list.Find("Content_Material") as RectTransform;
                _blueprintRoot = list.Find("Content_Blueprint") as RectTransform;
                _summaryRoot = list.Find("Content_Summary") as RectTransform;
            }
            _form = GetComponentInParent<UIFormBase>();
            _bound = _form != null && _displayItemTemplate != null && _ownedItemTemplate != null && _materialItemTemplate != null
                && _blueprintItemTemplate != null && _summaryItemTemplate != null && _displayRoot != null && _ownedRoot != null
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
            Refresh();
        }

        public void Refresh()
        {
            if (!_bound || WorldSession.Current == null) return;
            WorldState world = WorldSession.Current.World;
            RebuildDisplay(world);
            RebuildOwned(world);
            RebuildMaterials(world);
            RebuildBlueprints(world);
            RebuildSummary(world);
        }

        private void RebuildDisplay(WorldState world)
        {
            ClearItems<ArchiveItemObject>(_displayItemTemplate, _displayRoot);
            if (world.DisplayItems.Count == 0)
            {
                SetEmpty(_displayRoot, "尚无陈列物");
                return;
            }
            SetEmpty(_displayRoot, null);
            foreach (DisplayItem item in world.DisplayItems)
            {
                ArchiveItemObject view = _form.SpawnChildItem<ArchiveItemObject>(_displayItemTemplate, _displayRoot);
                view.Bind(item.Name, "第 " + item.ObtainedDay + " 天 · " + KindLabel(item.Kind), "", Icon("ICO-008保管"), Color.white, true);
            }
        }

        private void RebuildOwned(WorldState world)
        {
            ClearItems<ArchiveItemObject>(_ownedItemTemplate, _ownedRoot);
            SetEmpty(_ownedRoot, null);
            ArchiveItemObject fee = _form.SpawnChildItem<ArchiveItemObject>(_ownedItemTemplate, _ownedRoot);
            fee.Bind("维修费", "拥有物与已解锁形态", world.RepairFee.ToString(), Icon("ICO-051维修费"), Color.white);
            foreach (PartType part in world.OwnedParts)
            {
                ArchiveItemObject view = _form.SpawnChildItem<ArchiveItemObject>(_ownedItemTemplate, _ownedRoot);
                PartCodexConfig config = PartCodexCatalog.Get(part);
                view.Bind(config != null ? config.Name : part.ToString(), "永久零件", "已拥有", Icon("ICO-060零件"), Color.white);
            }
        }

        private void RebuildMaterials(WorldState world)
        {
            ClearItems<ArchiveItemObject>(_materialItemTemplate, _materialRoot);
            SetEmpty(_materialRoot, null);
            foreach (MaterialConfig material in MaterialCatalog.All())
            {
                int count = TotalMaterial(world.Materials, material);
                Color color = count > 0 ? Color.white : new Color(0.5f, 0.5f, 0.52f, 1f);
                ArchiveItemObject view = _form.SpawnChildItem<ArchiveItemObject>(_materialItemTemplate, _materialRoot);
                view.Bind(material.Name, material.IsTypedByCase ? "类型化材料" : "材料库存", "×" + count, Icon(MaterialIconKey(material.Id)), color);
            }
        }

        private void RebuildBlueprints(WorldState world)
        {
            ClearItems<ArchiveItemObject>(_blueprintItemTemplate, _blueprintRoot);
            if (world.Blueprints.Count == 0)
            {
                SetEmpty(_blueprintRoot, "暂无图样");
                return;
            }
            SetEmpty(_blueprintRoot, null);
            foreach (string blueprint in world.Blueprints)
            {
                ArchiveItemObject view = _form.SpawnChildItem<ArchiveItemObject>(_blueprintItemTemplate, _blueprintRoot);
                view.Bind(blueprint, "已取得图样", "已解锁", Icon("ICO-058图样"), Color.white);
            }
        }

        private void RebuildSummary(WorldState world)
        {
            ClearItems<ArchiveItemObject>(_summaryItemTemplate, _summaryRoot);
            SetEmpty(_summaryRoot, null);
            ArchiveItemObject holdings = _form.SpawnChildItem<ArchiveItemObject>(_summaryItemTemplate, _summaryRoot);
            holdings.Bind("永久零件与形态", "收集汇总", world.OwnedParts.Count + " 零件 / " + world.UnlockedForms.Count + " 形态", Icon("ICO-008保管"), Color.white);
            ArchiveItemObject display = _form.SpawnChildItem<ArchiveItemObject>(_summaryItemTemplate, _summaryRoot);
            display.Bind("纪念物与档案", "陈列物总数", world.DisplayItems.Count.ToString(), Icon("ICO-008保管"), Color.white);
        }

        private void ClearItems<T>(GameObject template, RectTransform root) where T : UIItemObject, new()
        {
            if (template != null) _form.UnspawnAllChildItem<T>(template);
            if (root != null) SetEmpty(root, null);
        }

        private static void SetEmpty(RectTransform root, string message)
        {
            if (root == null) return;
            Transform empty = root.Find("EmptyState");
            if (empty == null && message != null)
            {
                GameObject go = new GameObject("EmptyState", typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(root, false);
                empty = go.transform;
                RectTransform rect = (RectTransform)empty;
                rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -18f); rect.sizeDelta = new Vector2(-40f, 64f);
                TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
                text.font = UIFactory.BuiltinFont; text.fontSize = 24; text.alignment = TextAlignmentOptions.Center; text.color = new Color(0.5f, 0.5f, 0.52f, 1f);
            }
            if (empty != null)
            {
                empty.gameObject.SetActive(message != null);
                TextMeshProUGUI text = empty.GetComponent<TextMeshProUGUI>();
                if (text != null && message != null) text.text = "（" + message + "）";
            }
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
