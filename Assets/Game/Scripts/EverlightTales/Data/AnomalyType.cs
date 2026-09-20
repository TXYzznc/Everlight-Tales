namespace Everlight.Tales.Data
{
    /// <summary>棋盘异常种类（A-001～A-008）。区域注册与触发框架落 Board 层，配置落此处。</summary>
    public enum AnomalyType : byte
    {
        None = 0,

        /// <summary>A-001 局部定势偏转：区域内定势取全盘定势偏转 +1。</summary>
        LocalSettleDeflection = 1,

        /// <summary>A-002 扩张裂隙：入口区→出口格原子传送，每3次扩张1格。</summary>
        ExpandingRift = 2,

        /// <summary>A-003 保留（文档待补）。</summary>
        ReservedA003 = 3,

        /// <summary>A-004 静默区：区内能力源主动能力被抑制。</summary>
        SilenceZone = 4,

        /// <summary>A-005 沉降轨道：区内同格停留者在轮后沿固定方向推进1格。</summary>
        SettlingRail = 5,

        /// <summary>A-006 超载蓄势场：能力执行计热，满阈值补能，芯耗尽转静默。</summary>
        OverloadField = 6,

        /// <summary>A-007 保留（文档待补）。</summary>
        ReservedA007 = 7,

        /// <summary>A-008 拍击后定势反转：每2拍定势翻转180度并整理。</summary>
        PostTapReversal = 8,
    }

    /// <summary>棋盘异常静态配置。区域格集合由 Board 层注册（含 HexCoord）。</summary>
    public sealed class AnomalyConfig
    {
        public AnomalyType Type { get; }

        /// <summary>A-001 偏转步数（+1）。</summary>
        public int Deflection { get; }

        /// <summary>A-005 沉降方向（相位序号，D2=2）。</summary>
        public int SettleDirection { get; }

        /// <summary>A-002 扩张阈值 / A-006 场热阈值（3）。</summary>
        public int Threshold { get; }

        /// <summary>A-006 过载芯数量（3）。</summary>
        public int CoreCount { get; }

        /// <summary>A-008 反转周期拍数（2）。</summary>
        public int TapPeriod { get; }

        public AnomalyConfig(AnomalyType type, int deflection = 1, int settleDirection = 2, int threshold = 3, int coreCount = 3, int tapPeriod = 2)
        {
            Type = type;
            Deflection = deflection;
            SettleDirection = settleDirection;
            Threshold = threshold;
            CoreCount = coreCount;
            TapPeriod = tapPeriod;
        }
    }

    /// <summary>棋盘异常目录（P2-003）。A-003/A-007 文档待补，先登记为保留类型。</summary>
    public static class AnomalyCatalog
    {
        public static AnomalyConfig Get(AnomalyType type)
        {
            switch (type)
            {
                case AnomalyType.LocalSettleDeflection: return new AnomalyConfig(type, deflection: 1);
                case AnomalyType.ExpandingRift: return new AnomalyConfig(type, threshold: 3);
                case AnomalyType.ReservedA003: return new AnomalyConfig(type);
                case AnomalyType.SilenceZone: return new AnomalyConfig(type);
                case AnomalyType.SettlingRail: return new AnomalyConfig(type, settleDirection: 2);
                case AnomalyType.OverloadField: return new AnomalyConfig(type, threshold: 3, coreCount: 3);
                case AnomalyType.ReservedA007: return new AnomalyConfig(type);
                case AnomalyType.PostTapReversal: return new AnomalyConfig(type, tapPeriod: 2);
                default: return null;
            }
        }
    }
}
