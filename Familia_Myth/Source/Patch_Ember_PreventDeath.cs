using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.ShouldBeDead))]
    public static class Patch_Ember_PreventDeath
    {
        public static void Postfix(Pawn ___pawn, ref bool __result)
        {
            try
            {
                if (___pawn == null || !__result) return;
                var hediff = ___pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("EmberInEffect"));
                //余烬生效时拦截死亡
                if (hediff != null)
                    __result = false;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_Ember_PreventDeath)} error: {ex}");
            }
        }
    }
}