using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Hediff_Injury), nameof(Hediff_Injury.Heal))]
    public static class Patch_EnduranceGain_Heal
    {
        public static void Postfix(Hediff_Injury __instance, float amount)
        {
            try
            {
                //每10点回复的伤势1点
                __instance.pawn.GetPharna()?.GainStatus(PharnaStatusType.Endurance, amount * 0.1f);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_EnduranceGain_Heal)} error: {ex}");
            }
        }
    }
}