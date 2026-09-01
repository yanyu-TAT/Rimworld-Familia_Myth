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

            Pawn.RestoreParts();
            Pawn.HealInjuries(InjuryHealPerTick, Validator);

            //设置失血封顶80%
            var bloodLoss = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (bloodLoss != null)
                bloodLoss.Severity = Math.Min(0.8f, bloodLoss.Severity);
        }

        private bool Validator(Hediff_Injury injury)
        {
            float hp = injury.Part.def.GetMaxHealth(Pawn);
            return injury.Severity >= hp * 0.8f;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            var after = HediffMaker.MakeHediff(HediffDef.Named("EmberAftereffect"), Pawn);
            Pawn.health.AddHediff(after);
        }
    }
}