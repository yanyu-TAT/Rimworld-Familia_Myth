using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class HediffComp_EmberInEffect : HediffComp
    {
        private const float InjuryHealPerTick = 2f;
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            //恢复器官
            //按照器官覆盖面积排序
            var missingParts = Pawn.health.hediffSet.GetMissingPartsCommonAncestors()
                .OrderByDescending(i => i.Part.coverageAbsWithChildren);

            foreach (var mp in missingParts)
            {
                var part = mp.Part;
                
                //避免与植入体冲突
                if (Pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part))
                    continue;

                Pawn.health.RestorePart(part);

                var injury = (Hediff_Injury)HediffMaker.MakeHediff(HediffDefOf.Cut, Pawn, part);
                float maxHp = part.def.GetMaxHealth(Pawn);
                injury.Severity = Math.Max(1f, maxHp * 0.99f);
                Pawn.health.AddHediff(injury);
            }

            //治疗伤势
            float budget = InjuryHealPerTick;
            //按照伤势严重度排序（仅治愈重伤）
            var injuries = Pawn.health.hediffSet.hediffs
                .OfType<Hediff_Injury>()
                .Where(i => i.Severity > 0.8f)
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

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            var after = HediffMaker.MakeHediff(HediffDef.Named("EmberAftereffect"), Pawn);
            Pawn.health.AddHediff(after);
        }
    }
}