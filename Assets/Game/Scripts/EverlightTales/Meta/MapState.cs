using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>地点节点运行时状态（P3-003）。</summary>
    public sealed class PlaceState
    {
        public PlaceConfig Config;
        public PlaceNodeStatus Status;
        public int EventCount;

        public PlaceState(PlaceConfig config, PlaceNodeStatus status)
        {
            Config = config;
            Status = status;
        }

        public bool IsUnlocked => Status == PlaceNodeStatus.Unlocked;

        /// <summary>有可处理事项：已解锁且当前存在可处理事件。</summary>
        public bool HasActionable => IsUnlocked && EventCount > 0;
    }

    /// <summary>城市地图状态（P3-003）：地点节点的解锁集合与当前事件占用。</summary>
    public sealed class MapState
    {
        private readonly Dictionary<string, PlaceState> _byId = new Dictionary<string, PlaceState>();
        private readonly List<PlaceState> _places = new List<PlaceState>();

        public IReadOnlyList<PlaceState> Places => _places;

        public void Register(PlaceConfig config, PlaceNodeStatus status)
        {
            var state = new PlaceState(config, status);
            _byId[config.Id] = state;
            _places.Add(state);
        }

        public PlaceState Get(string id)
        {
            return _byId.TryGetValue(id, out PlaceState state) ? state : null;
        }

        /// <summary>永久解锁：点亮为实色，长期保留。</summary>
        public bool Unlock(string id)
        {
            PlaceState state = Get(id);
            if (state == null)
            {
                return false;
            }

            state.Status = PlaceNodeStatus.Unlocked;
            return true;
        }

        /// <summary>标记已知未开放：虚线轮廓 + 问号（已解锁不再降级）。</summary>
        public bool MarkKnown(string id)
        {
            PlaceState state = Get(id);
            if (state == null || state.Status == PlaceNodeStatus.Unlocked)
            {
                return false;
            }

            state.Status = PlaceNodeStatus.KnownLocked;
            return true;
        }

        /// <summary>刷新某地点的可处理事件计数（由供给规则更新）。</summary>
        public void SetEventCount(string id, int count)
        {
            PlaceState state = Get(id);
            if (state != null)
            {
                state.EventCount = count;
            }
        }
    }
}
