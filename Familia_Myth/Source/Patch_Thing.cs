using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Thing), nameof(Thing.Ingested))]
    public static class Patch_Thing
    {
        public static void Postfix(Pawn ingester, float __result)
        {
            if (ingester == null) return;
            var pharna = Hediff_Pharna.GetPharna(ingester);
            if (pharna == null) return;

            //每点摄入的营养值1点
            pharna.GainStatus(PharnaStatusType.Endurance, __result);

            if (pharna.developableAbilities.FirstOrDefault(d => d is DevelopableAbility_EfficientCirculation)
                is not DevelopableAbility_EfficientCirculation ability) return;
            //每点摄入的营养值+0.05
            ability.GainProficiency(__result * 0.05f);
            //根据系数恢复休息值
            if (ingester.needs?.rest?.CurLevel != null)
                ingester.needs.rest.CurLevel += __result * ability.RestFactor;
        }
    }
}