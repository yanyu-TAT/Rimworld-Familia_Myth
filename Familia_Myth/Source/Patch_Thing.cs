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
            //每点摄入的营养值1点
            Hediff_Pharna.GetPharna(ingester)?.GainStatus(PharnaStatusType.Endurance, __result);
        }
    }
}