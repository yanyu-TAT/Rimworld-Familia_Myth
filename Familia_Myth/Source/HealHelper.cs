using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public static class HealHelper
    {

        /// <summary>
        /// 恢复器官
        /// </summary>
        /// <param name="cnt">恢复个数，默认全部恢复</param>
        public static void RestoreParts(this Pawn pawn, int? cnt = null)
        {
            if (pawn?.health == null)
                return;
            //按照器官覆盖面积排序
            var missingParts = pawn.health.hediffSet.GetMissingPartsCommonAncestors()
                .OrderByDescending(i => i.Part.coverageAbsWithChildren);

            // 过滤掉父部件也在恢复列表中的子部件
            var partsToRestore = missingParts
                .Where(mp => !missingParts.Any(other => other != mp && IsAncestorOf(other.Part, mp.Part)))
                .ToList();

            foreach (var mp in partsToRestore)
            {
                if (cnt.HasValue)
                {
                    if (cnt <= 0) break;
                    cnt--;
                }
                var part = mp.Part;

                //避免与植入体冲突
                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                    continue;

                pawn.health.RestorePart(part, checkStateChange: false);
                //Log.Message($"[FamiliaMyth] {Pawn.Name} restored part {part.Label}");
                var injury = (Hediff_Injury)HediffMaker.MakeHediff(HediffDefOf.Cut, pawn, part);
                float maxHp = part.def.GetMaxHealth(pawn);
                injury.Severity = maxHp * 0.99f;
                pawn.health.AddHediff(injury, part);
            }

            pawn.health.CheckForStateChange(null, null);
        }

        /// <summary>
        /// 辅助方法：检查 ancestor 是否是 part 的祖先
        /// </summary>
        private static bool IsAncestorOf(BodyPartRecord ancestor, BodyPartRecord part)
        {
            var parent = part.parent;
            while (parent != null)
            {
                if (parent == ancestor) return true;
                parent = parent.parent;
            }
            return false;
        }

        /// <summary>
        /// 治疗伤势
        /// </summary>
        /// <returns>未使用的治疗量</returns>
        public static float HealInjuries(this Pawn pawn, float budget, Func<Hediff_Injury, bool> validator)
        {
            if (pawn?.health?.hediffSet?.hediffs == null) return budget;

            //按照伤势严重度排序
            var injuries = pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(validator)
                .OrderByDescending(i => i.Severity);

            foreach (var item in injuries)
            {
                if (budget <= 0f) break;
                float heal = Math.Min(item.Severity, budget);
                item.Heal(heal);
                budget -= heal;
            }

            return budget;
        }

        /// <summary>
        /// 治疗伤势
        /// </summary>
        /// <returns>未使用的治疗量</returns>
        public static float HealInjuries(this Pawn pawn, float budget)
        {
            return pawn.HealInjuries(budget, h => true);
        }

        /// <summary>
        /// 包扎伤口
        /// </summary>
        /// <param name="cnt">包扎伤口数，默认全部包扎</param>
        public static void TendInjuries(this Pawn pawn, int? cnt = null, float quality = 1f)
        {
            //按伤势降序
            var injuries = pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .OrderByDescending(i => i.Severity);

            foreach (var injury in injuries)
            {
                if (cnt.HasValue)
                {
                    if (cnt <= 0) break;
                    cnt--;
                }
                var tendComp = injury.TryGetComp<HediffComp_TendDuration>();
                tendComp?.CompTended(quality, quality);
            }
        }
    }
}
