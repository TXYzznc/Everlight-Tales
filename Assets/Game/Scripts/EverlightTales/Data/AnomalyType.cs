using System.Collections.Generic;

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

        /// <summary>A-003 回声区域：区域内声音源按剩余额度把一次根声音派生为第二次。</summary>
        EchoZone = 3,

        /// <summary>A-004 静默区：区内能力源主动能力被抑制。</summary>
        SilenceZone = 4,

        /// <summary>A-005 沉降轨道：拍首与拍末同格停留者沿固定方向推进。</summary>
        SettlingRail = 5,

        /// <summary>A-006 超载蓄势场：能力执行计热，满阈值补能，芯耗尽转静默。</summary>
        OverloadField = 6,

        /// <summary>A-007 重绘帷幕：轮后给区域内登记伪装载体增加一层伪装。</summary>
        RedrawCurtain = 7,

        /// <summary>A-008 拍击后定势反转：每2拍定势翻转180度并整理。</summary>
        PostTapReversal = 8,
    }

    /// <summary>棋盘异常静态配置。区域格集合由 Board 层注册（含 HexCoord）；数值列 0 表示该项不适用。</summary>
    public sealed class AnomalyConfig
    {
        public AnomalyConfig(
            AnomalyType type,
            int deflection = 0,
            int settleDirection = 0,
            int threshold = 0,
            int coreCount = 0,
            int tapPeriod = 0,
            ExtraParamSet extra = null)
        {
            Type = type;
            Deflection = deflection;
            SettleDirection = settleDirection;
            Threshold = threshold;
            CoreCount = coreCount;
            TapPeriod = tapPeriod;
            Extra = extra ?? ExtraParamSet.Empty;
        }

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

        /// <summary>逐项参数（配置表 ExtraData 列）。</summary>
        public ExtraParamSet Extra { get; }

        /// <summary>A-001 多个局部定势场重叠时的实例优先值。</summary>
        public int Priority
        {
            get { return Extra.GetInt("Priority", 10); }
        }

        /// <summary>A-001／A-004 是否可由校准控制端切换。</summary>
        public bool Controllable
        {
            get { return Extra.GetBool("Controllable", true); }
        }

        /// <summary>A-002 最多扩张格数。</summary>
        public int MaxExpansion
        {
            get { return Extra.GetInt("MaxExpansion", 2); }
        }

        /// <summary>A-003 回声音额。</summary>
        public int EchoQuota
        {
            get { return Extra.GetInt("EchoQuota", 3); }
        }

        /// <summary>A-003 是否可由复位端补满额度。</summary>
        public bool Resettable
        {
            get { return Extra.GetBool("Resettable", true); }
        }

        /// <summary>A-003 派生声音是否禁止再被回声区复制。</summary>
        public bool NoEchoOfEcho
        {
            get { return Extra.GetBool("NoEchoOfEcho", true); }
        }

        /// <summary>A-004 是否抑制任务固定节点（基础不抑制）。</summary>
        public bool SuppressStoryNodes
        {
            get { return Extra.GetBool("SuppressStoryNodes", false); }
        }

        /// <summary>A-005 单次沉降步长。</summary>
        public int StepLength
        {
            get { return Extra.GetInt("StepLength", 1); }
        }

        /// <summary>A-006 过载时为区内可充电件补充的充能量。</summary>
        public int RechargeAmount
        {
            get { return Extra.GetInt("RechargeAmount", 1); }
        }

        /// <summary>A-007 伪装层数上限。</summary>
        public int MaxLayers
        {
            get { return Extra.GetInt("MaxLayers", 2); }
        }

        /// <summary>A-007 每轮最多增加伪装的目标数。</summary>
        public int PerRoundTargets
        {
            get { return Extra.GetInt("PerRoundTargets", 1); }
        }

        /// <summary>A-008 反转角度（180 度）。</summary>
        public int FlipDegrees
        {
            get { return Extra.GetInt("FlipDegrees", 180); }
        }
    }

    /// <summary>棋盘异常目录（P2-003）。</summary>
    public static class AnomalyCatalog
    {
        /// <summary>各异常类型接受的逐项参数键；未列出的键在装载时判为拼写错误。</summary>
        private static readonly Dictionary<AnomalyType, Dictionary<string, ExtraParamKind>> ExtraSchema =
            new Dictionary<AnomalyType, Dictionary<string, ExtraParamKind>>
            {
                {
                    AnomalyType.LocalSettleDeflection, new Dictionary<string, ExtraParamKind>
                    {
                        { "Priority", ExtraParamKind.Int },
                        { "Controllable", ExtraParamKind.Bool },
                    }
                },
                {
                    AnomalyType.ExpandingRift, new Dictionary<string, ExtraParamKind>
                    {
                        { "MaxExpansion", ExtraParamKind.Int },
                    }
                },
                {
                    AnomalyType.EchoZone, new Dictionary<string, ExtraParamKind>
                    {
                        { "Priority", ExtraParamKind.Int },
                        { "EchoQuota", ExtraParamKind.Int },
                        { "Resettable", ExtraParamKind.Bool },
                        { "NoEchoOfEcho", ExtraParamKind.Bool },
                    }
                },
                {
                    AnomalyType.SilenceZone, new Dictionary<string, ExtraParamKind>
                    {
                        { "SuppressStoryNodes", ExtraParamKind.Bool },
                        { "Controllable", ExtraParamKind.Bool },
                    }
                },
                {
                    AnomalyType.SettlingRail, new Dictionary<string, ExtraParamKind>
                    {
                        { "StepLength", ExtraParamKind.Int },
                    }
                },
                {
                    AnomalyType.OverloadField, new Dictionary<string, ExtraParamKind>
                    {
                        { "RechargeAmount", ExtraParamKind.Int },
                    }
                },
                {
                    AnomalyType.RedrawCurtain, new Dictionary<string, ExtraParamKind>
                    {
                        { "MaxLayers", ExtraParamKind.Int },
                        { "PerRoundTargets", ExtraParamKind.Int },
                    }
                },
                {
                    AnomalyType.PostTapReversal, new Dictionary<string, ExtraParamKind>
                    {
                        { "FlipDegrees", ExtraParamKind.Int },
                    }
                },
            };

        private static Dictionary<AnomalyType, AnomalyConfig> _installed;

        public static void Install(IEnumerable<AnomalyConfig> configs)
        {
            var index = new Dictionary<AnomalyType, AnomalyConfig>();
            foreach (AnomalyConfig config in configs)
            {
                if (config == null || config.Type == AnomalyType.None || config.Deflection < 0 || config.Threshold < 0 || config.CoreCount < 0 || config.TapPeriod < 0 || !index.TryAdd(config.Type, config))
                {
                    throw new System.InvalidOperationException("异常配置存在空条目、无效数值或重复类型。");
                }

                ExtraSchema.TryGetValue(config.Type, out Dictionary<string, ExtraParamKind> schema);
                config.Extra.Validate("异常 " + config.Type, schema);
                bool invalid = schema == null;
                switch (config.Type)
                {
                    case AnomalyType.LocalSettleDeflection: invalid |= config.Deflection > 5; break;
                    case AnomalyType.ExpandingRift: invalid |= config.Threshold <= 0 || config.MaxExpansion < 0; break;
                    case AnomalyType.EchoZone: invalid |= config.EchoQuota < 0 || !config.NoEchoOfEcho; break;
                    case AnomalyType.SettlingRail: invalid |= config.SettleDirection < 0 || config.SettleDirection > 5 || config.StepLength <= 0; break;
                    case AnomalyType.OverloadField: invalid |= config.Threshold <= 0 || config.CoreCount <= 0 || config.RechargeAmount <= 0; break;
                    case AnomalyType.RedrawCurtain: invalid |= config.MaxLayers <= 0 || config.PerRoundTargets <= 0; break;
                    case AnomalyType.PostTapReversal: invalid |= config.TapPeriod <= 0 || config.FlipDegrees <= 0 || config.FlipDegrees >= 360 || config.FlipDegrees % 60 != 0; break;
                }
                if (invalid) throw new System.InvalidOperationException("异常参数不满足规则：" + config.Type);
            }

            _installed = index;
        }

        public static AnomalyConfig Get(AnomalyType type)
        {
            if (_installed != null)
            {
                _installed.TryGetValue(type, out AnomalyConfig config);
                return config;
            }

            return null;
        }
    }
}
