using System;
using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Everlight.Tales.UI
{
    /// <summary>预制体中的地图节点及其运行时地点状态。</summary>
    public sealed class CityMapNodeView
    {
        public PlaceState Place;
        public MapNodeItem Item;
        public RectTransform Rect;
        public Vector2 BasePosition;

        public CityMapNodeView(PlaceState place, MapNodeItem item, RectTransform rect, Vector2 basePosition)
        {
            Place = place;
            Item = item;
            Rect = rect;
            BasePosition = basePosition;
        }
    }

    /// <summary>
    /// 城市地图视图。节点由 MapPage/MapContent 预先放置，运行时仅按地点 ID 绑定世界状态。
    /// </summary>
    public sealed class CityMapView : MonoBehaviour
    {
        public Vector2 ViewportOffset;

        private readonly List<CityMapNodeView> _nodes = new List<CityMapNodeView>();
        private readonly List<MapNodeItem> _staticItems = new List<MapNodeItem>();
        private readonly Dictionary<MapNodeItem, Vector2> _basePositions = new Dictionary<MapNodeItem, Vector2>();
        [SerializeField] private RectTransform _nodeRoot;

        public IReadOnlyList<CityMapNodeView> Nodes => _nodes;
        public event Action<string> NodeClicked;
        public string SelectedPlaceId { get; private set; }

        public void BindNodeTemplates(Transform root)
        {
            if (_nodeRoot == null)
            {
                _nodeRoot = root != null && root.Find("MapContent") != null
                    ? root.Find("MapContent") as RectTransform
                    : root as RectTransform;
            }
            _staticItems.Clear();
            _basePositions.Clear();
            if (_nodeRoot == null)
            {
                Debug.LogError("CityMapView 缺少 MapContent，无法绑定预制体地图节点。", this);
                return;
            }

            _staticItems.AddRange(_nodeRoot.GetComponentsInChildren<MapNodeItem>(true));
            for (int i = 0; i < _staticItems.Count; i++)
            {
                _basePositions[_staticItems[i]] = (_staticItems[i].transform as RectTransform).anchoredPosition - ViewportOffset;
                if (string.IsNullOrEmpty(_staticItems[i].PlaceId))
                {
                    Debug.LogError("MapContent 下的 MapNodeItem 未配置地点 ID。", _staticItems[i]);
                }
            }
        }

        /// <summary>刷新预制体节点，不创建、不销毁节点。</summary>
        public void Build(MapState map)
        {
            _nodes.Clear();
            if (map == null)
            {
                SetAllInactive();
                return;
            }

            for (int i = 0; i < _staticItems.Count; i++)
            {
                MapNodeItem item = _staticItems[i];
                PlaceState place = map.Get(item.PlaceId);
                if (place == null || place.Status == PlaceNodeStatus.Undiscovered)
                {
                    item.gameObject.SetActive(false);
                    continue;
                }

                item.gameObject.SetActive(true);
                string id = item.PlaceId;
                item.Bind(place.Config.Name,
                    place.Status == PlaceNodeStatus.KnownLocked ? new Color(0.6f, 0.6f, 0.6f, 0.5f) : Color.white,
                    () => Select(id));
                item.SetSelected(id == SelectedPlaceId);
                RectTransform rect = item.transform as RectTransform;
                var node = new CityMapNodeView(place, item, rect, _basePositions[item]);
                _nodes.Add(node);
                Reposition(node);
            }
        }

        public void Pan(float dx, float dy)
        {
            ViewportOffset += new Vector2(dx, dy);
            for (int i = 0; i < _nodes.Count; i++) Reposition(_nodes[i]);
        }

        public void Select(string placeId)
        {
            SelectedPlaceId = placeId;
            for (int i = 0; i < _staticItems.Count; i++)
            {
                _staticItems[i].SetSelected(_staticItems[i].PlaceId == placeId);
            }
            NodeClicked?.Invoke(placeId);
        }

        /// <summary>清除地图节点的业务选中和 EventSystem 焦点，保持节点与详情面板一致。</summary>
        public void ClearSelection()
        {
            SelectedPlaceId = null;
            for (int i = 0; i < _staticItems.Count; i++)
            {
                _staticItems[i].SetSelected(false);
            }

            GameObject current = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            if (current != null && current.GetComponentInParent<MapNodeItem>() != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public TimeAdvanceResult WaitToNextPeriod(TimeState time) => time.Advance(time.RemainingCells);

        private void Reposition(CityMapNodeView node)
        {
            node.Rect.anchoredPosition = node.BasePosition + ViewportOffset;
        }

        private void SetAllInactive()
        {
            for (int i = 0; i < _staticItems.Count; i++) _staticItems[i].gameObject.SetActive(false);
        }
    }
}
