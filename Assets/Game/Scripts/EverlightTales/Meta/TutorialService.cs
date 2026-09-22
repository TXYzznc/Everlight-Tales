using Everlight.Tales.Data;

namespace Everlight.Tales.Meta
{
    /// <summary>
    /// 教学服务（P5-001/P5-003，D-014）：S0 三段教学进度与通过解锁。
    /// 通过每段永久解锁对应零件（P-001→P-002→P-003）并推进进度；教学 0 时间格（不结算时间）、
    /// 原地重试（重复通过不重复解锁、不扣资源）、宽松通过（无失败收尾）。
    /// 纯逻辑、零引擎（Meta 仅引用 Data）。
    /// </summary>
    public static class TutorialService
    {
        public const int StageCount = 3;

        /// <summary>第 stageIndex 段通过后解锁的零件（P-001/P-002/P-003）。</summary>
        public static PartType StagePart(int stageIndex)
        {
            switch (stageIndex)
            {
                case 0:
                    return PartType.InertiaHammer;
                case 1:
                    return PartType.MeteringRatchet;
                case 2:
                    return PartType.BlastCoil;
                default:
                    return PartType.None;
            }
        }

        /// <summary>某段是否已通过（进度 &gt; stageIndex）。</summary>
        public static bool IsStagePassed(WorldState world, int stageIndex)
        {
            return world != null && stageIndex >= 0 && stageIndex < StageCount && world.TutorialStage > stageIndex;
        }

        /// <summary>三段是否全部通过。</summary>
        public static bool IsComplete(WorldState world)
        {
            return world != null && world.TutorialStage >= StageCount;
        }

        /// <summary>
        /// 通过某段教学：永久解锁对应零件 + 推进进度。
        /// 原地重试语义：已通过的段返回 false、不重复解锁、不推进；非法段返回 false。
        /// </summary>
        public static bool CompleteStage(WorldState world, int stageIndex)
        {
            if (world == null || stageIndex < 0 || stageIndex >= StageCount)
            {
                return false;
            }

            if (IsStagePassed(world, stageIndex))
            {
                return false;
            }

            PartType part = StagePart(stageIndex);
            if (part != PartType.None && !world.OwnedParts.Contains(part))
            {
                world.OwnedParts.Add(part);
                world.KnownParts.Remove(part); // 解锁后不再是「已知未拥有」
            }

            world.TutorialStage = stageIndex + 1;
            return true;
        }

        /// <summary>一次性补完三段（演示/回退用）：按序解锁 P-001~P-003。</summary>
        public static void CompleteAll(WorldState world)
        {
            for (int i = 0; i < StageCount; i++)
            {
                CompleteStage(world, i);
            }
        }
    }
}
