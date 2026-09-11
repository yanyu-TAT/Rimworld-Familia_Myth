using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(MentalBreaker), nameof(MentalBreaker.TryDoRandomMoodCausedMentalBreak))]
    public static class Patch_Virtue_Trigger
    {
        public static bool Prefix(MentalBreaker __instance, ref bool __result)
        {
            try
            {
                var pawnField = AccessTools.FieldRefAccess<MentalBreaker, Pawn>("pawn");
                var pawn = pawnField(__instance);

                if (pawn == null) return true;

                var pharna = pawn.GetPharna();
                if (pharna == null) return true;

                //处于美德状态时拦截其他将触发的精神状态
                if (pharna.currentVirtue.HasValue)
                {
                    __result = false;
                    return false;
                }
                if (pharna.TryTriggerVirtue())
                {
                    __result = true;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_Virtue_Trigger)} error: {ex}");
                return true; //出错则放行原版崩溃逻辑
            }
        }
    }
}
