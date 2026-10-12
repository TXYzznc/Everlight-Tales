using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>首版地点目录（P3-012）：世界存档恢复时据此重建地图节点。</summary>
    public static class PlaceCatalog
    {
        private static IReadOnlyList<PlaceConfig> _places = System.Array.Empty<PlaceConfig>();

        public static IReadOnlyList<PlaceConfig> FirstBatch() => _places;

        public static void Install(IEnumerable<PlaceConfig> places)
        {
            var list = new List<PlaceConfig>();
            var ids = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (PlaceConfig place in places)
            {
                if (place == null || string.IsNullOrEmpty(place.Id) || !ids.Add(place.Id))
                {
                    throw new System.InvalidOperationException("地点配置存在空条目或重复 ID。");
                }
                list.Add(place);
            }
            _places = list.AsReadOnly();
        }

        public static PlaceConfig Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            foreach (PlaceConfig place in FirstBatch())
            {
                if (place.Id == id)
                {
                    return place;
                }
            }

            return null;
        }
    }
}
