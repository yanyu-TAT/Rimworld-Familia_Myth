using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDead")]
    public static class Patch_Pawn_HealthTracker
    {
        public static void Postfix(Pawn ___pawn, ref bool __result)
        {
            if (___pawn == null || !__result) return;
            //Log.Message($"Kill {___pawn.Name}");
            var hediff = ___pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("EmberInEffect"));
            //余烬生效时拦截死亡
            if (hediff != null)
                __result = false;
        }
    }
}