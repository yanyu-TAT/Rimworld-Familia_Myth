using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
    public static class Patch_Pawn
    {
        public static void Postfix(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            //每点承受伤害0.5点
            __instance.GetPharna()?.GainStatus(PharnaStatusType.Endurance, totalDamageDealt / 2f);
        }
    }
}