using System;

namespace Everlight.Tales.Board
{
    /// <summary>
    /// 统一随机服务（零引擎）：xorshift32 确定性随机源。
    /// 业务代码禁止直接调用 UnityEngine.Random 或 System.Random，全部经本服务获取随机数。
    /// </summary>
    public sealed class RandomService
    {
        private uint m_State;
        private int m_Seed;
        private int m_ConsumedCount;

        public RandomService(int seed)
        {
            Reset(seed);
        }

        public RandomService(ISeedSource source)
            : this(source.GetSeed())
        {
        }

        /// <summary>当前种子。</summary>
        public int Seed => m_Seed;

        /// <summary>已消费的随机次数，只增不减，重置时归零。</summary>
        public int ConsumedCount => m_ConsumedCount;

        /// <summary>重置种子并清零消费计数。</summary>
        public void Reset(int seed)
        {
            m_Seed = seed;
            // xorshift32 状态不能为 0，否则恒为 0。
            m_State = seed == 0 ? 0x6D2B79F5u : (uint)seed;
            m_ConsumedCount = 0;
        }

        /// <summary>返回 [minInclusive, maxExclusive) 区间内的整数。</summary>
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive 必须大于 minInclusive");
            }

            m_ConsumedCount++;
            long range = (long)maxExclusive - minInclusive;
            return minInclusive + (int)(NextUInt() % (uint)range);
        }

        /// <summary>恢复随机序列：重置种子并推进指定次数（存档恢复，P2-010）。</summary>
        public void Restore(int seed, int consumed)
        {
            Reset(seed);
            for (int i = 0; i < consumed; i++)
            {
                NextUInt();
                m_ConsumedCount++;
            }
        }

        /// <summary>返回 [0, 1) 区间的浮点数。</summary>
        public float NextFloat()
        {
            m_ConsumedCount++;
            return (NextUInt() >> 8) * (1.0f / 16777216.0f);
        }

        private uint NextUInt()
        {
            uint x = m_State;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            m_State = x;
            return x;
        }
    }
}
