namespace Everlight.Tales.Data
{
    /// <summary>基础怪谈批次目录（P3-016，D-015/01-批次与触发）：五个有序批次与每批完成数。</summary>
    public static class BatchCatalog
    {
        public const int TotalBatches = 5;

        public static string BatchName(int batch)
        {
            return "B" + batch;
        }

        /// <summary>每批基础怪谈数量：B1=1、B2=3、B3=5、B4=5、B5=1。</summary>
        public static int RequiredCount(int batch)
        {
            switch (batch)
            {
                case 1: return 1;  // 红舞鞋
                case 2: return 3;  // 画皮/扫帚/裂口女
                case 3: return 5;  // 猴爪/聂小倩/穿墙术/风月宝鉴/花子
                case 4: return 5;  // 画壁/枕中梦境/道林画像/吹笛人/独立影子
                case 5: return 1;  // 如月车站
                default: return 0;
            }
        }

        public static bool TryParseBatch(string batchName, out int batch)
        {
            batch = 0;
            if (string.IsNullOrEmpty(batchName) || batchName.Length < 2 || batchName[0] != 'B')
            {
                return false;
            }

            return int.TryParse(batchName.Substring(1), out batch);
        }
    }
}
