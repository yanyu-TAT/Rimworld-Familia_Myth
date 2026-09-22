using System;
using System.Linq;
using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    //高效循环熟练度：每点消耗的休息值
    [HarmonyPatch(typeof(Need_Rest), nameof(Need_Rest.NeedInterval))]
    public static class Patch_EfficientCirculation_Rest
    {
        //每点消耗的休息值带来的熟练度
        private const float ProfGainPerRest = 3f;

        private static readonly AccessTools.FieldRef<Need, Pawn> PawnRef =
            AccessTools.FieldRefAccess<Need, Pawn>("pawn");

        public static void Prefix(Need_Rest __instance, out float __state)
        {
            try
            {
                __state = __instance.CurLevel;
            }
            catch (Exception ex)
            {
                //置极小值，令 Postfix 判定为无效初始值而直接跳过结算
                __state = float.MinValue;
                Log.Error($"[FamiliaMyth] {nameof(Patch_EfficientCirculation_Rest)} prefix error: {ex}");
            }
        }

        public static void Postfix(Need_Rest __instance, float __state)
        {
            try
            {
                if (float.IsNaN(__state) || __state < -1e9f) return; //Prefix 未取到有效初始值
                //该方法内既有消耗也有睡觉恢复，取正值即为净消耗量
                float spent = __state - __instance.CurLevel;
                if (spent <= 0f) return;

                var pharna = PawnRef(__instance).GetPharna();
                var ability = pharna?.developableAbilities?.FirstOrDefault(d => d is DevelopableAbility_EfficientCirculation);
                ability?.GainProficiency(spent * ProfGainPerRest);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_EfficientCirculation_Rest)} postfix error: {ex}");
            }
        }
    }
}
