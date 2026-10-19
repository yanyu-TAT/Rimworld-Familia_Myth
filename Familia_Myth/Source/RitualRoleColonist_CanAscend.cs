using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Ritual
{
    public class RitualRoleColonist_CanAscend : RitualRoleColonist
    {
        private static HediffDef cachedPharnaDef;
        private static HediffDef PharnaDef => cachedPharnaDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_Pharna");

        public override bool AppliesToPawn(Pawn p, out string reason, TargetInfo selectedTarget, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            if (!base.AppliesToPawn(p, out reason, selectedTarget, ritual, assignments, precept, skipReason))
                return false;

            if (p == null || p.health == null || p.health.hediffSet == null) 
                return false;

            if (!RitualCandidateUtility.IsFamiliaIdeoMember(p, out reason, skipReason))
                return false;

            if (p.health.hediffSet.GetFirstHediffOfDef(PharnaDef) is not Hediff_Pharna pharna){
                if (!skipReason)
                    reason = "FamiliaMyth.HaveNoPharnaRefusion".Translate();
                return false;
            }

            var acceptance = pharna.CanAscend();
            if (!acceptance)
            {
                if (!skipReason)
                    reason = acceptance.Reason;
                return false;
            }

            return true;
        }
    }
}