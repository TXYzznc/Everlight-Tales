using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 批次资格与怪谈触发（P3-016，D-015/01-批次与触发）：
    /// 五批有序，同批全部完成才开放下一批；资格取得不自动创建实例（触发不自动生成新案，同案合并只记一份）。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public static class BatchService
    {
        public static bool HasEligibility(WorldState world, int batch)
        {
            return batch <= world.BatchNumber;
        }

        /// <summary>某批已计完成的基础怪谈数（BatchCounted 且批次匹配）。</summary>
        public static int CompletedIn(WorldState world, int batch)
        {
            string batchName = BatchCatalog.BatchName(batch);
            int count = 0;
            foreach (CaseState c in world.Cases)
            {
                if (c.BatchCounted && c.Config.Batch == batchName)
                {
                    count++;
                }
            }

            return count;
        }

        public static int CompletedInCurrent(WorldState world)
        {
            return CompletedIn(world, world.BatchNumber);
        }

        public static bool CanOpenNext(WorldState world)
        {
            if (world.BatchNumber >= BatchCatalog.TotalBatches)
            {
                return false;
            }

            return CompletedInCurrent(world) >= BatchCatalog.RequiredCount(world.BatchNumber);
        }

        /// <summary>本批全部完成时开放下一批；未满足或已到最后一批返回 false。</summary>
        public static bool TryOpenNext(WorldState world)
        {
            if (!CanOpenNext(world))
            {
                return false;
            }

            world.BatchNumber++;
            return true;
        }

        /// <summary>触发不自动生成新案：同案已存在（合并）或未获该批资格则不触发。</summary>
        public static bool CanTrigger(WorldState world, CaseConfig config)
        {
            if (config == null)
            {
                return false;
            }

            foreach (CaseState c in world.Cases)
            {
                if (c.Config.Id == config.Id)
                {
                    return false;
                }
            }

            if (!BatchCatalog.TryParseBatch(config.Batch, out int batch))
            {
                return false;
            }

            return HasEligibility(world, batch);
        }
    }
}
