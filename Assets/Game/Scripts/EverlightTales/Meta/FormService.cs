using System.Collections.Generic;
using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>解锁检查结果（P4-003）：是否可加工、还差什么。</summary>
    public sealed class UnlockCheck
    {
        public bool CanUnlock;
        public bool AlreadyUnlocked;
        public int FeeShort;                                        // 还差维修费（>=0）
        public bool MissingBlueprint;
        public IReadOnlyList<FormMaterialCost> MissingMaterials;    // 还差的材料（数量=缺口）

        public static readonly UnlockCheck Already = new UnlockCheck { AlreadyUnlocked = true };
    }

    /// <summary>解锁结果（P4-003）：一次性支付维修费与材料，图样保留。</summary>
    public sealed class UnlockResult
    {
        public bool Success;
        public bool AlreadyUnlocked;
        public bool Insufficient;
        public int FeePaid;
        public IReadOnlyList<FormMaterialCost> MaterialsConsumed;
        public bool AutoSetCurrent;
    }

    /// <summary>
    /// 形态服务（P4-004，D-079）：形态永久解锁与当前使用。
    /// 解锁 = 一次性支付维修费 + 按配方消耗材料（图样保留不消耗，新形态自动设为当前使用）；
    /// 切换 = 已解锁形态间免费、立即生效、不耗时。
    /// 「当前使用」是准备页默认带出形态；准备页临时切换只影响本关、不改当前使用（ResolveForm）。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public static class FormService
    {
        /// <summary>基础形态 ID（空串）：每个宿主零件固有的默认工作方式，始终已解锁。</summary>
        public const string BaseFormId = "";

        // ---- 图样（永久条件，保留不消耗）----

        public static bool HasBlueprint(WorldState world, string blueprintId)
        {
            return world != null && !string.IsNullOrEmpty(blueprintId) && world.Blueprints.Contains(blueprintId);
        }

        /// <summary>授予图样（幂等，永久保留）。</summary>
        public static void GrantBlueprint(WorldState world, string blueprintId)
        {
            if (world == null || string.IsNullOrEmpty(blueprintId) || world.Blueprints.Contains(blueprintId))
            {
                return;
            }

            world.Blueprints.Add(blueprintId);
        }

        // ---- 解锁状态 ----

        public static bool IsUnlocked(WorldState world, string formId)
        {
            return world != null && !string.IsNullOrEmpty(formId) && world.UnlockedForms.Contains(formId);
        }

        /// <summary>当前使用形态 ID（空 = 基础形态）。</summary>
        public static string GetCurrent(WorldState world, PartType hostPart)
        {
            if (world == null)
            {
                return BaseFormId;
            }

            return world.CurrentForms.TryGetValue(hostPart, out string id) ? id : BaseFormId;
        }

        /// <summary>切换当前使用（免费、立即生效）：仅限基础形态或该宿主已解锁形态。</summary>
        public static bool SwitchCurrent(WorldState world, PartType hostPart, string formId)
        {
            if (world == null)
            {
                return false;
            }

            formId = formId ?? BaseFormId;
            if (formId == BaseFormId)
            {
                world.CurrentForms[hostPart] = BaseFormId;
                return true;
            }

            if (!IsUnlocked(world, formId))
            {
                return false;
            }

            FormConfig form = FormCatalog.Get(formId);
            if (form == null || form.HostPart != hostPart)
            {
                return false;
            }

            world.CurrentForms[hostPart] = formId;
            return true;
        }

        /// <summary>
        /// 结算时实际形态：准备页临时切换（tempOverride）优先且只影响本关，
        /// 否则退回加工台当前使用。绝不改动 CurrentForms（当前使用/临时切换不冲突，P4-004）。
        /// </summary>
        public static string ResolveForm(WorldState world, PartType hostPart, string tempOverride)
        {
            if (tempOverride != null)
            {
                if (tempOverride.Length == 0)
                {
                    return BaseFormId;
                }

                FormConfig f = FormCatalog.Get(tempOverride);
                if (f != null && f.HostPart == hostPart && IsUnlocked(world, tempOverride))
                {
                    return tempOverride;
                }
            }

            return GetCurrent(world, hostPart);
        }

        // ---- 解锁（加工）----

        public static UnlockCheck CanUnlock(WorldState world, FormConfig form)
        {
            if (world == null || form == null)
            {
                return UnlockCheck.Already;
            }

            if (IsUnlocked(world, form.Id))
            {
                return UnlockCheck.Already;
            }

            int feeShort = form.UnlockFee - EconomyService.Balance(world);
            bool missingBlueprint = form.RequiresBlueprint && !HasBlueprint(world, form.BlueprintId);
            var missing = new List<FormMaterialCost>();
            foreach (FormMaterialCost m in form.Materials)
            {
                int held = world.Materials.GetCount(m.MaterialId, m.SourceCase);
                if (held < m.Count)
                {
                    missing.Add(new FormMaterialCost(m.MaterialId, m.Count - held, m.SourceCase));
                }
            }

            bool can = feeShort <= 0 && !missingBlueprint && missing.Count == 0;
            return new UnlockCheck
            {
                CanUnlock = can,
                AlreadyUnlocked = false,
                FeeShort = feeShort > 0 ? feeShort : 0,
                MissingBlueprint = missingBlueprint,
                MissingMaterials = missing,
            };
        }

        public static UnlockResult Unlock(WorldState world, FormConfig form, int tick)
        {
            if (world == null || form == null)
            {
                return new UnlockResult { Insufficient = true };
            }

            if (IsUnlocked(world, form.Id))
            {
                return new UnlockResult { AlreadyUnlocked = true };
            }

            UnlockCheck check = CanUnlock(world, form);
            if (!check.CanUnlock)
            {
                return new UnlockResult { Insufficient = true };
            }

            // 一次性支付维修费（加工支出通道）。
            if (!EconomyService.Spend(world, form.UnlockFee, PayChannel.Craft, form.Name, tick))
            {
                return new UnlockResult { Insufficient = true };
            }

            // 按配方消耗材料（CanUnlock 已保证充足）；图样保留不消耗。
            foreach (FormMaterialCost m in form.Materials)
            {
                world.Materials.Remove(m.MaterialId, m.Count, m.SourceCase);
            }

            world.UnlockedForms.Add(form.Id);

            if (form.AutoSetCurrent)
            {
                world.CurrentForms[form.HostPart] = form.Id;
            }

            return new UnlockResult
            {
                Success = true,
                FeePaid = form.UnlockFee,
                MaterialsConsumed = form.Materials,
                AutoSetCurrent = form.AutoSetCurrent,
            };
        }
    }
}
