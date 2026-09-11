using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(RecordsUtility), nameof(RecordsUtility.Notify_PawnKilled))]
    public static class Patch_StrengthGain_Kill
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn killed, Pawn killer)
        {
            try
            {
                //非友方单位
                if (killed == null || killer == null || killed.Faction == killer.Faction) return;

                var pharna = killer.GetPharna();

                //每次击杀8点
                pharna?.GainStatus(PharnaStatusType.Strength, 8f);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_StrengthGain_Kill)} error: {ex}");
            }
        }
    }
}
