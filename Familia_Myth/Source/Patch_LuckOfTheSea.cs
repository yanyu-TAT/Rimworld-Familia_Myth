using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch()]
    public static class Patch_LuckOfTheSea
    {
        [HarmonyPatch(typeof(FishingUtility), nameof(FishingUtility.GetCatchesFor))]
        public static class Patch_LuckOfTheSea_GetCatchesFor
        {
            [HarmonyPostfix]
            public static void Postfix(Pawn pawn, bool animalFishing,
                ref bool rare, List<Thing> __result)
            {
                try
                {
                    if (animalFishing || pawn == null || __result == null || !pawn.Spawned)
                        return;

                    var pharna = pawn.GetPharna();
                    if (pharna == null) return;

                    var ability = (DevelopableAbility_LuckOfTheSea)pharna.developableAbilities
                        .FirstOrDefault(a => a is DevelopableAbility_LuckOfTheSea);
                    if (ability == null) return;

                    if (__result.Count > 0)
                        ability.GainProficiency(DevelopableAbility_LuckOfTheSea.ProfGainPerCatch);

                    if (rare) return;
                    if (!Rand.Chance(ability.ExtraRareCatchChance)) return;

                    //直接从自定义池生成稀有渔获并替换
                    var custom = DefDatabase<ThingSetMakerDef>.GetNamedSilentFail("LuckOfTheSeaExtraRareCatches");
                    if (custom?.root == null) return;
                    var extras = custom.root.Generate();
                    if (extras.Count == 0) return;

                    __result = extras;
                    rare = true;
                }
                catch (Exception ex)
                {
                    Log.Error($"[FamiliaMyth] {nameof(Patch_LuckOfTheSea_GetCatchesFor)} error: {ex}");
                }
            }
        }
    }
}
