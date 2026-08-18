using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(RecordsUtility), nameof(RecordsUtility.Notify_PawnDowned))]
    public static class Patch_RecordsUtility_PawnDowned
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn downed, Pawn instigator)
        {
            //非友方单位
            if (downed == null || instigator == null || downed.Faction == instigator.Faction) return;

            var pharna = instigator.GetPharna();

            //每次击倒5点
            pharna?.GainStatus(PharnaStatusType.Strength, 5f);
        }
    }
}
