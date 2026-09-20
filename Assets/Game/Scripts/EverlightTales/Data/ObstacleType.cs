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
        public ObstacleType Type { get; }

        /// <summary>最大耐久（O-002=2、O-008=3，无耐久为 0）。</summary>
        public int MaxDurability { get; }

        /// <summary>方向参数（-1 无）：O-004 输出 D1=1、O-006 入口 D0=0。</summary>
        public int PassDirection { get; }

        public ObstacleConfig(ObstacleType type, int maxDurability = 0, int passDirection = -1)
        {
            Type = type;
            MaxDurability = maxDurability;
            PassDirection = passDirection;
        }
    }

    /// <summary>障碍设施目录（P2-002）。初值待试玩校准。</summary>
    public static class ObstacleCatalog
    {
        public static ObstacleConfig Get(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.FixedWall: return new ObstacleConfig(type);
                case ObstacleType.BrittlePartition: return new ObstacleConfig(type, maxDurability: 2);
                case ObstacleType.LinkedGate: return new ObstacleConfig(type);
                case ObstacleType.TurningRail: return new ObstacleConfig(type, passDirection: 1);
                case ObstacleType.ClampingSeat: return new ObstacleConfig(type);
                case ObstacleType.OneWayShutter: return new ObstacleConfig(type, passDirection: 0);
                case ObstacleType.ProliferatingSeal: return new ObstacleConfig(type, maxDurability: 1);
                case ObstacleType.PressurePillar: return new ObstacleConfig(type, maxDurability: 3);
                default: return null;
            }
        }
    }
}
