using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(MentalBreaker), nameof(MentalBreaker.TryDoRandomMoodCausedMentalBreak))]
    public static class Patch_MentalBreaker
    {
        public static bool Prefix(MentalBreaker __instance, ref bool __result)
        {
            var pawnField = AccessTools.FieldRefAccess<MentalBreaker, Pawn>("pawn");
            var pawn = pawnField(__instance);

            if (pawn == null) return true;

            var pharna = Hediff_Pharna.GetPharna(pawn);
            if (pharna == null) return true;

            //处于美德状态时拦截其他将触发的精神状态
            if (pharna.currentVirtue.HasValue)
            {
                __result = false;
                return false;
            }
            if (!pharna.TryTriggerVirtue()) return true;

            var values = Enum.GetValues(typeof(VirtueType));
            var type = (VirtueType)values.GetValue(Rand.Range(0, values.Length));
            var def = DefDatabase<MentalStateDef>.GetNamed($"MentalState_Virtue_{type}");

            if (def == null) return true;
            pharna.currentVirtue = type;
            if (pawn.mindState.mentalStateHandler.TryStartMentalState(def))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
