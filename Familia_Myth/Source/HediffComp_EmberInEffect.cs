using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class HediffComp_EmberInEffect : HediffComp
    {
        private const float InjuryHealPerTick = 20f;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            //恢复器官
            //按照器官覆盖面积排序
            var missingParts = Pawn.health.hediffSet.GetMissingPartsCommonAncestors()
                .OrderByDescending(i => i.Part.coverageAbsWithChildren);

            // 过滤掉父部件也在恢复列表中的子部件
            var partsToRestore = missingParts
                .Where(mp => !missingParts.Any(other => other != mp && IsAncestorOf(other.Part, mp.Part)))
                .ToList();

            foreach (var mp in partsToRestore)
            {
                var part = mp.Part;
                
                //避免与植入体冲突
                if (Pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                    continue;

                Pawn.health.RestorePart(part, checkStateChange : false);
                Log.Message($"[FamiliaMyth] {Pawn.Name} restored part {part.Label}");
                var injury = (Hediff_Injury)HediffMaker.MakeHediff(HediffDefOf.Cut, Pawn, part);
                float maxHp = part.def.GetMaxHealth(Pawn);
                injury.Severity = maxHp * 0.9f;
                Pawn.health.AddHediff(injury, part);
            }

            Pawn.health.CheckForStateChange(null, null);

            //治疗伤势
            float budget = InjuryHealPerTick;
            //按照伤势严重度排序（仅治愈重伤）
            var injuries = Pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(i => i.Severity > i.Part.def.GetMaxHealth(Pawn) * 0.5f)
                .OrderByDescending(i => i.Severity);

            foreach (var item in injuries)
            {
                if (budget <= 0f) break;
                float heal = Math.Min(item.Severity, budget);
                item.Heal(heal);
                budget -= heal;
            }

            //设置失血封顶80%
            var bloodLoss = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (bloodLoss != null)
                bloodLoss.Severity = Math.Min(0.8f, bloodLoss.Severity);
        }

        // 辅助方法：检查 ancestor 是否是 part 的祖先
        static bool IsAncestorOf(BodyPartRecord ancestor, BodyPartRecord part)
        {
            var parent = part.parent;
            while (parent != null)
            {
                if (parent == ancestor) return true;
                parent = parent.parent;
            }
            return false;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            var after = HediffMaker.MakeHediff(HediffDef.Named("EmberAftereffect"), Pawn);
            Pawn.health.AddHediff(after);
        }
    }
}