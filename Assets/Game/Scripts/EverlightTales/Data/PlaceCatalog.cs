using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>首版地点目录（P3-012）：世界存档恢复时据此重建地图节点。</summary>
    public static class PlaceCatalog
    {
        public static IReadOnlyList<PlaceConfig> FirstBatch()
        {
            return new[]
            {
                new PlaceConfig("home", "长明修理铺", "家园，加工配装资料索引", PlaceUnlockSource.Start, 0f, 0f, true),
                new PlaceConfig("street", "街区小店", "卷帘门／夜班送还", PlaceUnlockSource.Story, 1f, 0f),
                new PlaceConfig("dance", "舞蹈教室", "红舞鞋调查与维修", PlaceUnlockSource.Story, 0f, 1f),
                new PlaceConfig("tailor", "裁缝铺", "画皮线索", PlaceUnlockSource.Investigate, 2f, 1f),
                new PlaceConfig("community", "社区活动中心", "积水处置／邻里找物", PlaceUnlockSource.Story, 2f, 0f),
            };
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
