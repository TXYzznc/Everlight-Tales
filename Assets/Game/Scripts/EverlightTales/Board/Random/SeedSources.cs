namespace Everlight.Tales.Board
{
    /// <summary>种子来源：提供本次演算使用的种子，可插拔以在「固定 / 随机混合」之间切换。</summary>
    public interface ISeedSource
    {
        int GetSeed();
    }

    /// <summary>固定种子来源（默认）：确定性，保证同种子同序列。</summary>
    public sealed class FixedSeedSource : ISeedSource
    {
        private readonly int m_Seed;

        public FixedSeedSource(int seed)
        {
            m_Seed = seed;
        }

        public int GetSeed()
        {
            return m_Seed;
        }
    }

    /// <summary>固定种子 + 随机混合来源：在固定基础上叠加随机成分（无引擎，仅用 System.Random）。</summary>
    public sealed class MixedSeedSource : ISeedSource
    {
        private readonly int m_BaseSeed;

        public MixedSeedSource(int baseSeed)
        {
            m_BaseSeed = baseSeed;
        }

        public int GetSeed()
        {
            // 固定基础 + 随机扰动；替换为 FixedSeedSource 即消除随机性，调用方无需改动。
            return m_BaseSeed ^ new System.Random().Next();
        }
    }
}
