using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Patch
{
    public class Patch_PawnCapacitiesHandler
    {
        private static PawnCapacityDef def;
        private static PawnCapacityDef Def => def ??= DefDatabase<PawnCapacityDef>.GetNamed("Metabolism");

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PawnCapacitiesHandler), nameof(PawnCapacitiesHandler.GetLevel))]
        public static void GetLevel(PawnCapacitiesHandler __instance, PawnCapacityDef capacity, ref float __result)
        {
            var pawnField = AccessTools.Field(typeof(PawnCapacitiesHandler), "pawn");
            var pawn = pawnField?.GetValue(__instance) as Pawn;
            if (pawn == null || pawn.Dead) return;

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
                __result = Mathf.Max(__result, 1f); // 锁定最低100%
            }
        }
    }
}