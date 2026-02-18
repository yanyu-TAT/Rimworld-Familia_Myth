using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Hediff_Injury), nameof(Hediff_Injury.Heal))]
    public static class Patch_Hediff_Injury
    {
        public static void Postfix(Hediff_Injury __instance, float amount)
        {
            //每10点回复的伤势1点
            Hediff_Pharna.GetPharna(__instance.pawn)?.GainStatus(PharnaStatusType.Endurance, amount * 0.1f);
        }
    }
}