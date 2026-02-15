using RimWorld;
using Verse;

namespace FamiliaMyth.Ritual
{
    public class RitualRoleColonist_NoPharna : RitualRoleColonist
    {
        private static HediffDef cachedPharnaDef;
        private static HediffDef PharnaDef => cachedPharnaDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_Pharna");

        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            if (!base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason))
                return false;

            if (p == null || p.health == null || p.health.hediffSet == null) 
                return false;

            if (p.health.hediffSet.GetFirstHediffOfDef(PharnaDef) != null){
                if (!skipReason)
                    reason = "FamiliaMyth.AlreadyHavePharnaRefusion".Translate();
                return false;
            }

            return true;
        }
    }
}