using System.Text;
using Everlight.Tales.Data;

namespace Everlight.Tales.UI
{
    /// <summary>将零件配置和当前形态格式化为玩家可读的详情文本。</summary>
    public static class PartDetailFormatter
    {
        public static string DisplayName(PartType part, string formId)
        {
            PartCodexConfig codex = PartCodexCatalog.Get(part);
            string name = codex != null ? codex.Name : part.ToString();
            FormConfig form = string.IsNullOrEmpty(formId) ? null : FormCatalog.Get(formId);
            return form == null || string.IsNullOrEmpty(form.Name) ? name + "·基础形态" : name + "·" + form.Name;
        }

        public static string Build(PartType part, string formId)
        {
            PartCodexConfig codex = PartCodexCatalog.Get(part);
            string name = codex != null ? codex.Name + "（" + codex.Id + "）" : part.ToString();
            PartConfig config = PartCatalog.Get(part);
            if (config == null)
            {
                return name + "\n（无能力配置）";
            }

            var sb = new StringBuilder();
            FormConfig form = string.IsNullOrEmpty(formId) ? null : FormCatalog.Get(formId);
            if (form != null)
            {
                sb.Append("形态：").Append(form.Name).Append('\n');
                if (!string.IsNullOrEmpty(form.Description))
                {
                    sb.Append("效果：").Append(form.Description).Append('\n');
                }
            }
            else
            {
                sb.Append("功能：").Append(PartCodexCatalog.UseHint(part)).Append('\n');
            }

            sb.Append("触发分：").Append(config.TriggerScore).Append("（触发不耗能）\n");
            sb.Append("能量：容量 ").Append(config.EnergyCapacity).Append(" / 单次消耗 ").Append(config.EffectCost).Append('\n');
            if (config.EffectScorePerCell > 0)
            {
                sb.Append("效果分：每推移 1 格 +").Append(config.EffectScorePerCell).Append('\n');
            }
            if (config.EffectScorePerTarget > 0)
            {
                sb.Append("效果分：每命中 +").Append(config.EffectScorePerTarget).Append('\n');
            }
            if (config.PublicEnergyPerEffect > 0)
            {
                sb.Append("产能：公共能量 +").Append(config.PublicEnergyPerEffect).Append('\n');
            }
            if (config.BonusNth > 0 && config.BonusScoreFromNth > 0)
            {
                sb.Append("第 ").Append(config.BonusNth).Append(" 次起每次 +").Append(config.BonusScoreFromNth).Append('\n');
            }
            if (config.EffectScorePerEnergy > 0)
            {
                sb.Append("剩余能量：每点 +").Append(config.EffectScorePerEnergy).Append(" 分\n");
            }
            if (config.PushDistance > 1)
            {
                sb.Append("推距：").Append(config.PushDistance).Append(" 格\n");
            }
            if (config.RayRange > 0)
            {
                sb.Append("作用距离：").Append(config.RayRange).Append(" 格\n");
            }
            if (config.ChargeThreshold > 0)
            {
                sb.Append("蓄量阈值：").Append(config.ChargeThreshold).Append('\n');
            }
            if (config.SecondaryEffectScore > 0)
            {
                sb.Append("次生效果分：+").Append(config.SecondaryEffectScore).Append('\n');
            }
            if (config.RemovesSelf)
            {
                sb.Append("起爆后移除本体\n");
            }

            return sb.ToString().TrimEnd('\n');
        }
    }
}
