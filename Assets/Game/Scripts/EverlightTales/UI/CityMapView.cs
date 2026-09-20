using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;
using Everlight.Tales.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Everlight.Tales.UI
{
    /// <summary>地图节点视图（P3-004 表现层）。</summary>
    public sealed class CityMapNodeView
    {
        public PlaceState Place;
        public RectTransform Rect;

        public CityMapNodeView(PlaceState place, RectTransform rect)
        {
            Place = place;
            Rect = rect;
        }
    }

    /// <summary>
    /// 城市地图视图（P3-004）：程序化布局地点节点（未发现不出现、已知未开放虚线弱化、已解锁实色），
    /// 单指拖动平移（Pan）、点击节点（Select）、等待到下一时段（WaitToNextPeriod）。
    /// 拖动只改视图，事件状态与刷新节奏由供给规则决定（D-080 固定视角，不实现双指缩放）。
    /// </summary>
    public sealed class CityMapView : MonoBehaviour
    {
        public float Scale = 80f;
        public Vector2 ViewportOffset;

        private readonly List<CityMapNodeView> _nodes = new List<CityMapNodeView>();

        public IReadOnlyList<CityMapNodeView> Nodes => _nodes;

        public string SelectedPlaceId { get; private set; }

        /// <summary>按地图状态重建节点（未发现地点不出现）。</summary>
        public void Build(MapState map)
        {
            Clear();
            foreach (PlaceState place in map.Places)
            {
                if (place.Status == PlaceNodeStatus.Undiscovered)
                {
                    continue;
                }

                CreateNode(place);
            }
        }

        /// <summary>单指拖动平移：累加视口偏移并重排全部节点。</summary>
        public void Pan(float dx, float dy)
        {
            ViewportOffset += new Vector2(dx, dy);
            foreach (CityMapNodeView node in _nodes)
            {
                Reposition(node, node.Place.Config);
            }
        }

        /// <summary>点击节点：记录选中地点（可进入地点详情/事件）。</summary>
        public void Select(string placeId)
        {
            SelectedPlaceId = placeId;
        }

        /// <summary>等待到下一时段：把剩余格一次推进到下一时段（不处理事件、不发奖励）。</summary>
        public TimeAdvanceResult WaitToNextPeriod(TimeState time)
        {
            return time.Advance(time.RemainingCells);
        }

        private void CreateNode(PlaceState place)
        {
            var go = new GameObject("node-" + place.Config.Id, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(transform, false);

            var image = go.GetComponent<Image>();
            image.color = place.Status == PlaceNodeStatus.KnownLocked ? new Color(0.6f, 0.6f, 0.6f, 0.5f) : Color.white;

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(44f, 44f);

            string id = place.Config.Id;
            go.GetComponent<Button>().onClick.AddListener(() => Select(id));

            var node = new CityMapNodeView(place, rect);
            _nodes.Add(node);
            Reposition(node, place.Config);
        }

        private void Reposition(CityMapNodeView node, PlaceConfig config)
        {
            node.Rect.anchoredPosition = new Vector2(config.X * Scale + ViewportOffset.x, config.Y * Scale + ViewportOffset.y);
        }

        private void Clear()
        {
            foreach (CityMapNodeView node in _nodes)
            {
                if (node.Rect != null)
                {
                    Destroy(node.Rect.gameObject);
                }
            }

            _nodes.Clear();
        }
    }
}
