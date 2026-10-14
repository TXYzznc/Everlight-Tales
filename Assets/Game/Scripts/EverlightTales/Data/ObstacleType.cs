using System;
using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>障碍设施种类（O-001～O-008）。数值为静态配置，行为由 Board 层解释。</summary>
    public enum ObstacleType : byte
    {
        /// <summary>非障碍。</summary>
        None = 0,

        /// <summary>O-001 固定墙体：固定阻挡，不可被普通爆破／铆合／机械臂移除。</summary>
        FixedWall = 1,

        /// <summary>O-002 脆裂隔板：耐久2，物理碰撞或爆破冲击扣1，归零移除。</summary>
        BrittlePartition = 2,

        /// <summary>O-003 联动闸门：开关端点切换开／闭，关闭时阻挡。</summary>
        LinkedGate = 3,

        /// <summary>O-004 转向轨道（地形层）：能力步进进入时改沿固定输出方向。</summary>
        TurningRail = 4,

        /// <summary>O-005 夹持座（地形层）：首个允许实体进入即锁定位置。</summary>
        ClampingSeat = 5,

        /// <summary>O-006 单向百叶（地形层）：只允许入口方向进入，反向阻挡。</summary>
        OneWayShutter = 6,

        /// <summary>O-007 增生封条：根节点+封条实体，拍末最多增1段。</summary>
        ProliferatingSeal = 7,

        /// <summary>O-008 承压支柱：耐久3，归零移除并拆最多2个附属隔板。</summary>
        PressurePillar = 8,
    }

    /// <summary>障碍设施静态配置。方向以 0～5 的相位序号存，Board 层转 HexDirection。</summary>
    public sealed class ObstacleConfig
    {
        public ObstacleConfig(
            ObstacleType type,
            int maxDurability = 0,
            int passDirection = -1,
            ExtraParamSet extra = null)
        {
            Type = type;
            MaxDurability = maxDurability;
            PassDirection = passDirection;
            Extra = extra ?? ExtraParamSet.Empty;
            _allowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string tag in AllowTags.Split(',')) _allowedTags.Add(tag.Trim());
        }
        private readonly HashSet<string> _allowedTags;

        public ObstacleType Type { get; }

        /// <summary>最大耐久（O-002=2、O-007 段=1、O-008=3，无耐久为 0）。</summary>
        public int MaxDurability { get; }

        /// <summary>方向参数（-1 无）：O-004 输出 D1=1、O-006 入口 D0=0。</summary>
        public int PassDirection { get; }

        /// <summary>逐项参数（配置表 ExtraData 列）。</summary>
        public ExtraParamSet Extra { get; }

        /// <summary>O-002：同一根爆破只扣一次耐久。</summary>
        public bool RootBlastOncePerRoot
        {
            get { return Extra.GetBool("RootBlastOncePerRoot", true); }
        }

        /// <summary>O-003：闸门初始是否开启。</summary>
        public bool InitialOpen
        {
            get { return Extra.GetBool("InitialOpen", false); }
        }

        /// <summary>O-003：同一根信号对开关只切换一次。</summary>
        public bool ToggleOncePerRoot
        {
            get { return Extra.GetBool("ToggleOncePerRoot", true); }
        }

        /// <summary>O-003：关闭时若闸门格有实体，标记待关闭、腾空后落闸。</summary>
        public bool DeferCloseWhenOccupied
        {
            get { return Extra.GetBool("DeferCloseWhenOccupied", true); }
        }

        /// <summary>O-005：允许夹持的对象标签，逗号分隔；基础只夹普通件。</summary>
        public string AllowTags
        {
            get { return Extra.GetText("AllowTags", "normal"); }
        }

        /// <summary>O-005：初始已夹持的实体 ID，0 表示初始空闲。</summary>
        public int InitialClampedEntityId
        {
            get { return Extra.GetInt("InitialClampedEntityId", 0); }
        }

        /// <summary>O-005：释放后保持开放的拍数。</summary>
        public int ReleaseWindowTaps
        {
            get { return Extra.GetInt("ReleaseWindowTaps", 1); }
        }

        /// <summary>O-006：信号切换后的入口方向相位序号。</summary>
        public int ToggleDirection
        {
            get { return Extra.GetInt("ToggleDirection", 3); }
        }

        /// <summary>O-007：拍末最多新增的封条段数。</summary>
        public int GrowthPerTap
        {
            get { return Extra.GetInt("GrowthPerTap", 1); }
        }

        /// <summary>O-007：单个封条段的耐久。</summary>
        public int SegmentMaxDurability
        {
            get { return Extra.GetInt("SegmentMaxDurability", 1); }
        }

        /// <summary>O-008：支柱归零时一并移除的预登记附属隔板数。</summary>
        public int AttachedPartitions
        {
            get { return Extra.GetInt("AttachedPartitions", 0); }
        }

        /// <summary>O-008：支柱是否登记为保护对象（归零另记结构事故）。</summary>
        public bool ProtectedObject
        {
            get { return Extra.GetBool("ProtectedObject", false); }
        }

        /// <summary>O-008：是否允许修复能力每次恢复 1 耐久。</summary>
        public bool Repairable
        {
            get { return Extra.GetBool("Repairable", true); }
        }

        /// <summary>O-005：该标签是否允许被本夹持座锁定。</summary>
        public bool AllowsTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return false;
            }

            return _allowedTags.Contains(tag);
        }
    }

    /// <summary>障碍设施目录（P2-002）。初值待试玩校准。</summary>
    public static class ObstacleCatalog
    {
        /// <summary>各障碍类型接受的逐项参数键；未列出的键在装载时判为拼写错误。</summary>
        private static readonly Dictionary<ObstacleType, Dictionary<string, ExtraParamKind>> ExtraSchema =
            new Dictionary<ObstacleType, Dictionary<string, ExtraParamKind>>
            {
                {
                    ObstacleType.BrittlePartition, new Dictionary<string, ExtraParamKind>
                    {
                        { "RootBlastOncePerRoot", ExtraParamKind.Bool },
                    }
                },
                {
                    ObstacleType.LinkedGate, new Dictionary<string, ExtraParamKind>
                    {
                        { "InitialOpen", ExtraParamKind.Bool },
                        { "ToggleOncePerRoot", ExtraParamKind.Bool },
                        { "DeferCloseWhenOccupied", ExtraParamKind.Bool },
                    }
                },
                {
                    ObstacleType.ClampingSeat, new Dictionary<string, ExtraParamKind>
                    {
                        { "AllowTags", ExtraParamKind.Text },
                        { "InitialClampedEntityId", ExtraParamKind.Int },
                        { "ReleaseWindowTaps", ExtraParamKind.Int },
                    }
                },
                {
                    ObstacleType.OneWayShutter, new Dictionary<string, ExtraParamKind>
                    {
                        { "ToggleDirection", ExtraParamKind.Int },
                    }
                },
                {
                    ObstacleType.ProliferatingSeal, new Dictionary<string, ExtraParamKind>
                    {
                        { "GrowthPerTap", ExtraParamKind.Int },
                        { "SegmentMaxDurability", ExtraParamKind.Int },
                    }
                },
                {
                    ObstacleType.PressurePillar, new Dictionary<string, ExtraParamKind>
                    {
                        { "AttachedPartitions", ExtraParamKind.Int },
                        { "ProtectedObject", ExtraParamKind.Bool },
                        { "Repairable", ExtraParamKind.Bool },
                    }
                },
            };

        private static IReadOnlyDictionary<ObstacleType, ObstacleConfig> _configs = new Dictionary<ObstacleType, ObstacleConfig>();

        public static void Install(IEnumerable<ObstacleConfig> configs)
        {
            var map = new Dictionary<ObstacleType, ObstacleConfig>();
            foreach (ObstacleConfig config in configs)
            {
                if (config == null || config.Type == ObstacleType.None || !Enum.IsDefined(typeof(ObstacleType), config.Type) || config.MaxDurability < 0 || config.PassDirection < -1 || config.PassDirection > 5 || !map.TryAdd(config.Type, config))
                    throw new ArgumentException("ObstacleCatalog 存在无效障碍配置或重复类型。", nameof(configs));

                ExtraSchema.TryGetValue(config.Type, out Dictionary<string, ExtraParamKind> schema);
                config.Extra.Validate("障碍 " + config.Type, schema);
                if (config.Type == ObstacleType.BrittlePartition && !config.RootBlastOncePerRoot
                    || config.Type == ObstacleType.LinkedGate && !config.ToggleOncePerRoot)
                    throw new ArgumentException("当前规则要求同根爆破/开关只处理一次。", nameof(configs));
                if ((config.Type == ObstacleType.TurningRail || config.Type == ObstacleType.OneWayShutter) && config.PassDirection < 0
                    || config.Type == ObstacleType.OneWayShutter && (config.ToggleDirection < 0 || config.ToggleDirection > 5)
                    || config.Type == ObstacleType.ClampingSeat && (config.ReleaseWindowTaps < 0 || config.InitialClampedEntityId != 0)
                    || config.Type == ObstacleType.ProliferatingSeal && (config.GrowthPerTap < 0 || config.SegmentMaxDurability <= 0)
                    || config.Type == ObstacleType.PressurePillar && config.AttachedPartitions < 0)
                    throw new ArgumentException("障碍参数无效；初始夹持使用固定元素InstanceData行ID，不使用类型表运行时ID。", nameof(configs));
                if (config.Type == ObstacleType.ClampingSeat)
                    foreach (string tag in config.AllowTags.Split(','))
                        if (tag.Trim() != "normal" && tag.Trim() != "task")
                            throw new ArgumentException("夹持标签仅接受normal或明确登记的task。", nameof(configs));
            }
            _configs = map;
        }

        public static ObstacleConfig Get(ObstacleType type)
        {
            return _configs.TryGetValue(type, out ObstacleConfig config) ? config : null;
        }
    }
}
