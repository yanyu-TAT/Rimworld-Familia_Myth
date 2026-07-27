using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    public static class Patch_RecordsUtility
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecordsUtility), nameof(RecordsUtility.Notify_PawnDowned))]
        public static void Notify_PawnDowned(Pawn downed, Pawn instigator)
        {
            //非友方单位
            if (downed == null || instigator == null || downed.Faction == instigator.Faction) return;

            var pharna = Hediff_Pharna.GetPharna(instigator);

            //每次击倒5点
            pharna?.GainStatus(PharnaStatusType.Strength, 5f);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecordsUtility), nameof(RecordsUtility.Notify_PawnKilled))]
        public static void Notify_PawnKilled(Pawn killed, Pawn killer)
        {
            //非友方单位
            if (killed == null || killer == null || killed.Faction == killer.Faction) return;
            
            var pharna = Hediff_Pharna.GetPharna(killer);

            //每次击杀8点
            pharna?.GainStatus(PharnaStatusType.Strength, 8f);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RecordsUtility), nameof(RecordsUtility.Notify_BillDone))]
        public static void Notify_BillDone(Pawn billDoer, List<Thing> products)
        {
            var pharna = Hediff_Pharna.GetPharna(billDoer);
            var amount = 0f;
            if (pharna == null) return;
            foreach (var item in products)
            {
                if (item.TryGetQuality(out QualityCategory quality))
                    amount += quality switch {
                        QualityCategory.Awful => 0.5f,
                        QualityCategory.Poor => 1f,
                        QualityCategory.Normal => 2f,
                        QualityCategory.Good => 5f,
                        QualityCategory.Excellent => 10f,
                        QualityCategory.Masterwork => 20f,
                        QualityCategory.Legendary => 50f,
                        _ => 0f
                    };
            }

            //按照完成的品质获得
            pharna.GainStatus(PharnaStatusType.Dexterity, amount);
        }
    }
}