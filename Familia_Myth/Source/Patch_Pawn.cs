using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    public static class Patch_Pawn
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
        public static void PostApplyDamage(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            //每点承受伤害0.5点
            Hediff_Pharna.GetPharna(__instance)?.GainStatus(PharnaStatusType.Endurance, totalDamageDealt / 2f);
        }
    }
}