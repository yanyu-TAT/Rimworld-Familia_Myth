using FamiliaMyth.Feat;
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
        /// <param name="healer">受益归属者；缺省或与被治疗者相同则不计伟业贡献</param>
        public static void RestoreParts(this Pawn pawn, int? cnt = null, Pawn healer = null)
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

            float restored = 0f;
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
                restored += part.def.GetMaxHealth(pawn);
                //Log.Message($"[FamiliaMyth] {Pawn.Name} restored part {part.Label}");
                var injury = (Hediff_Injury)HediffMaker.MakeHediff(HediffDefOf.Cut, pawn, part);
                float maxHp = part.def.GetMaxHealth(pawn);
                injury.Severity = maxHp * 0.99f;
                pawn.health.AddHediff(injury, part);
            }

            ReportSupport(pawn, healer, restored * FeatUtility.RestoreWeight);
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
        /// <param name="healer">受益归属者；缺省或与被治疗者相同则不计伟业贡献</param>
        /// <returns>未使用的治疗量</returns>
        public static float HealInjuries(this Pawn pawn, float budget, Func<Hediff_Injury, bool> validator, Pawn healer = null)
        {
            if (pawn?.health?.hediffSet?.hediffs == null) return budget;

            //按照伤势严重度排序
            var injuries = pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(validator)
                .OrderByDescending(i => i.Severity);

            float healed = 0f;
            foreach (var item in injuries)
            {
                if (budget <= 0f) break;
                float heal = Math.Min(item.Severity, budget);
                item.Heal(heal);
                budget -= heal;
                healed += heal;
            }

            ReportSupport(pawn, healer, healed * FeatUtility.HealWeight);
            return budget;
        }

        /// <summary>
        /// 治疗伤势
        /// </summary>
        /// <param name="healer">受益归属者；缺省或与被治疗者相同则不计伟业贡献</param>
        /// <returns>未使用的治疗量</returns>
        public static float HealInjuries(this Pawn pawn, float budget, Pawn healer = null)
        {
            return pawn.HealInjuries(budget, h => true, healer);
        }

        /// <summary>
        /// 包扎伤口
        /// </summary>
        /// <param name="cnt">包扎伤口数，默认全部包扎</param>
        /// <param name="healer">受益归属者；缺省或与被治疗者相同则不计伟业贡献</param>
        public static void TendInjuries(this Pawn pawn, int? cnt = null, float quality = 1f, Pawn healer = null)
        {
            //按伤势降序
            var injuries = pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .OrderByDescending(i => i.Severity);

            float tended = 0f;
            foreach (var injury in injuries)
            {
                if (cnt.HasValue)
                {
                    if (cnt <= 0) break;
                    cnt--;
                }
                var tendComp = injury.TryGetComp<HediffComp_TendDuration>();
                if (tendComp == null) continue;
                tendComp.CompTended(quality, quality);
                tended += injury.Severity;
            }

            ReportSupport(pawn, healer, tended * FeatUtility.TendWeight);
        }

        private static void ReportSupport(Pawn target, Pawn healer, float points)
        {
            if (healer == null || healer == target || points <= 0f) return;
            GameComponent_FeatTracker.Instance?.RecordSupport(healer, points);
        }
    }
}
