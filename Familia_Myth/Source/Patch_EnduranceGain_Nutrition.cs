using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.Ingested))]
    public static class Patch_EnduranceGain_Nutrition
    {
        //每点摄入营养值的能力值增长
        private const float EndurancePerNutrition = 2f;

        public static void Postfix(Pawn ingester, float __result)
        {
            try
            {
                if (ingester == null) return;
                var pharna = ingester.GetPharna();
                if (pharna == null) return;

                //每点摄入的营养值
                pharna.GainStatus(PharnaStatusType.Endurance, __result * EndurancePerNutrition);

                if (pharna.developableAbilities.FirstOrDefault(d => d is DevelopableAbility_EfficientCirculation)
                    is not DevelopableAbility_EfficientCirculation ability) return;
                //每点摄入的营养值+0.05
                ability.GainProficiency(__result * DevelopableAbility_EfficientCirculation.ProfGainPerNutrition);
                //根据系数恢复休息值
                if (ingester.needs?.rest?.CurLevel != null)
                    ingester.needs.rest.CurLevel += __result * ability.RestFactor;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_EnduranceGain_Nutrition)} error: {ex}");
            }
        }
    }
}