using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(RecordsUtility), nameof(RecordsUtility.Notify_BillDone))]
    public static class Patch_DexterityGain_Craft
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn billDoer, List<Thing> products)
        {
            try
            {
                var pharna = billDoer.GetPharna();
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
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_DexterityGain_Craft)} error: {ex}");
            }
        }
    }
}
