using System;
using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    //耐久经验：每点获得的免疫力 +1
    [HarmonyPatch(typeof(ImmunityRecord), nameof(ImmunityRecord.ImmunityTickInterval))]
    public static class Patch_EnduranceGain_Immunity
    {
        public static void Prefix(ImmunityRecord __instance, out float __state)
        {
            try
            {
                __state = __instance.immunity;
            }
            catch (Exception ex)
            {
                //置极大值，令 Postfix 判定为无效初始值而直接跳过结算
                __state = float.MaxValue;
                Log.Error($"[FamiliaMyth] {nameof(Patch_EnduranceGain_Immunity)} prefix error: {ex}");
            }
        }

        public static void Postfix(ImmunityRecord __instance, float __state, Pawn pawn)
        {
            try
            {
                if (float.IsNaN(__state) || __state > 1e9f) return; //Prefix 未取到有效初始值
                float amount = __instance.immunity - __state;
                if (amount <= 0f) return;
                pawn?.GetPharna()?.GainStatus(PharnaStatusType.Endurance, amount);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_EnduranceGain_Immunity)} postfix error: {ex}");
            }
        }
    }
}
