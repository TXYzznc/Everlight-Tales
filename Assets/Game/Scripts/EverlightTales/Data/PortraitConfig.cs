using System.Collections.Generic;

namespace Everlight.Tales.Data
{
    /// <summary>立绘场景（P5-008）。</summary>
    public enum PortraitScene : byte
    {
        Idle = 0,     // 闲置（地图/常驻）
        Event = 1,    // 登场（事件/剧情登场）
        Dialogue = 2, // 事件对话
    }

    /// <summary>立绘情感动作（P5-008）。</summary>
    public enum PortraitEmotion : byte
    {
        Neutral = 0,
        Happy = 1,
        Worried = 2,
        Surprised = 3,
    }

    /// <summary>人物立绘配置（P5-008）：人物编号 + 名称 + 素材说明。</summary>
    public sealed class PortraitConfig
    {
        public string CharacterId;
        public string Name;
        public string Description;

        public PortraitConfig(string characterId, string name, string description)
        {
            CharacterId = characterId;
            Name = name;
            Description = description;
        }
    }

    /// <summary>人物立绘目录（P5-008）：首批关键人物，素材由 ART0 挂接。</summary>
    public static class PortraitCatalog
    {
        private static readonly IReadOnlyList<PortraitConfig> _characters = new[]
        {
            new PortraitConfig("CH-MASTER", "师父周衡", "长明修理铺原店主，画作修复期间失联"),
            new PortraitConfig("CH-SHEN", "沈遥", "舞蹈教室求助者，红舞鞋事件当事人"),
            new PortraitConfig("CH-PARTNER", "搭档", "维修协作搭档，事件间对话对象"),
        };

        public static IReadOnlyList<PortraitConfig> All() => _characters;

        public static PortraitConfig Get(string characterId)
        {
            foreach (PortraitConfig p in _characters)
            {
                if (p.CharacterId == characterId)
                {
                    return p;
                }
            }

            return null;
        }
    }
}
