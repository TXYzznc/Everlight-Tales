namespace Everlight.Tales.Data
{
    /// <summary>怪谈形态种类（P2-006）。形态不占携带种类，附着宿主零件字段与表现。</summary>
    public enum MFormType : byte
    {
        None = 0,

        /// <summary>M-012 借力改道夹（红舞鞋形态），宿主 P-004 换向齿轮。</summary>
        RedShoe = 1,
    }

    /// <summary>怪谈形态静态配置（P2-006）。</summary>
    public sealed class MFormConfig
    {
        public MFormType Type { get; }

        /// <summary>形态编号（M-012）。</summary>
        public string Id { get; }

        public string Name { get; }

        /// <summary>宿主零件种类。</summary>
        public PartType HostPart { get; }

        /// <summary>每次作业可用次数（跨轮保留，电池不补次数）。</summary>
        public int UsesPerJob { get; }

        public MFormConfig(MFormType type, string id, string name, PartType hostPart, int usesPerJob)
        {
            Type = type;
            Id = id;
            Name = name;
            HostPart = hostPart;
            UsesPerJob = usesPerJob;
        }
    }

    /// <summary>怪谈形态目录（P2-006，首批 M-012）。</summary>
    public static class MFormCatalog
    {
        public static MFormConfig Get(MFormType type)
        {
            switch (type)
            {
                case MFormType.RedShoe:
                    return new MFormConfig(type, "M-012", "借力改道夹（红舞鞋形态）", PartType.ReversalGear, usesPerJob: 1);
                default:
                    return null;
            }
        }
    }
}
