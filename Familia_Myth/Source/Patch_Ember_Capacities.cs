using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(PawnCapacitiesHandler), nameof(PawnCapacitiesHandler.GetLevel))]
    public static class Patch_Ember_Capacities
    {
        private static PawnCapacityDef def;
        private static PawnCapacityDef Def => def ??= DefDatabase<PawnCapacityDef>.GetNamed("Metabolism");
        public static void Postfix(PawnCapacitiesHandler __instance, PawnCapacityDef capacity, ref float __result)
        {
            try
            {
                var pawnField = AccessTools.Field(typeof(PawnCapacitiesHandler), "pawn");
                if (pawnField?.GetValue(__instance) is not Pawn pawn || pawn.Dead) return;

                //要求余烬触发中
                if (pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef.Named("EmberInEffect")) == null)
                    return;

                if (capacity == PawnCapacityDefOf.Consciousness ||
                   capacity == PawnCapacityDefOf.Moving ||
                   capacity == PawnCapacityDefOf.Manipulation ||
                   capacity == PawnCapacityDefOf.Breathing ||
                   capacity == PawnCapacityDefOf.BloodPumping ||
                   capacity == PawnCapacityDefOf.BloodFiltration ||
                   capacity == Def)
                {
                    __result = Mathf.Max(__result, 1.2f); // 锁定最低120%
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_Ember_Capacities)} error: {ex}");
            }
        }
    }
}