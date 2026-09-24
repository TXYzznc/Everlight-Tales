using System.Collections.Generic;
using Everlight.Tales.Board;
using Everlight.Tales.Data;

namespace Everlight.Tales.Events
{
    /// <summary>普通事件实例（P3-008）：当前地图上实际出现的一次可处理普通内容。</summary>
    public sealed class SupplyInstance
    {
        public string InstanceId;
        public SupplyCandidate Template;
        public bool Night;
        public bool Processed;
        public string PlaceId;

        public SupplyInstance(string instanceId, SupplyCandidate template, bool night)
        {
            InstanceId = instanceId;
            Template = template;
            Night = night;
            PlaceId = template.PlaceId;
        }
    }

    /// <summary>当前昼/夜的一批普通供给（P3-008）。</summary>
    public sealed class SupplyBatch
    {
        public bool Night;
        public List<SupplyInstance> Instances = new List<SupplyInstance>();

        public SupplyBatch(bool night)
        {
            Night = night;
        }
    }

    /// <summary>
    /// 普通供给服务（P3-008，D-068/D-071）：每昼/夜各生成至多 4 个普通实例。
    /// 过滤玩家阶段、对应权重与开放时段后按权重无放回抽取；未处理实例随昼夜交替退出（重新生成新批），
    /// 关键事件（主线/支线/怪谈/调查/回访）不进入普通刷新池、天然保留。
    /// Events 层引用 Board 的 RandomService 保证同种子复现。
    /// </summary>
    public static class SupplyService
    {
        public const int MaxPerBatch = 4;

        /// <summary>生成一批普通供给（night=true 为夜晚批，false 为白天批）。</summary>
        public static List<SupplyInstance> Refresh(IReadOnlyList<SupplyCandidate> candidates, bool night, int stage, RandomService rng)
        {
            var result = new List<SupplyInstance>();
            var pool = new List<SupplyCandidate>();
            foreach (SupplyCandidate candidate in candidates)
            {
                int weight = night ? candidate.NightWeight : candidate.DayWeight;
                if (candidate.MinStage <= stage && weight > 0 && IsOpenIn(candidate.OpenPeriods, night))
                {
                    pool.Add(candidate);
                }
            }

            int id = 1;
            while (result.Count < MaxPerBatch && pool.Count > 0)
            {
                int total = 0;
                foreach (SupplyCandidate candidate in pool)
                {
                    total += night ? candidate.NightWeight : candidate.DayWeight;
                }

                int roll = rng.NextInt(0, total);
                int acc = 0;
                int pickedIndex = -1;
                for (int i = 0; i < pool.Count; i++)
                {
                    acc += night ? pool[i].NightWeight : pool[i].DayWeight;
                    if (roll < acc)
                    {
                        pickedIndex = i;
                        break;
                    }
                }

                SupplyCandidate picked = pool[pickedIndex];
                pool.RemoveAt(pickedIndex);
                result.Add(new SupplyInstance("EV-" + id++, picked, night));
            }

            return result;
        }

        /// <summary>该候选是否在指定昼/夜至少有一个可开始时段。</summary>
        public static bool IsOpenIn(TimeOfDay[] periods, bool night)
        {
            foreach (TimeOfDay period in periods)
            {
                if (TimePeriod.IsDaylight(period) != night)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
