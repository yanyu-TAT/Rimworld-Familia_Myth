using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.TryAddEntropy))]
    public static class Patch_Pawn_PsychicTracker
    {
        private static DevelopableAbilityDef magicDef;
        private static DevelopableAbilityDef MagicDef => magicDef ??= DefDatabase<DevelopableAbilityDef>.GetNamed("DevelopableAbility_Magic");
        public static void Prefix(ref float __state, Pawn_PsychicEntropyTracker __instance)
        {
            __state = __instance.EntropyValue;
        }
        public static void Postfix(Pawn_PsychicEntropyTracker __instance, float __state)
        {
            float amount = __instance.EntropyValue - __state;
            if (amount <= 0) return;
            var pawn = __instance.Pawn;
            var pharna = Hediff_Pharna.GetPharna(pawn);

            //每点心灵熵0.1点
            pharna.GainStatus(PharnaStatusType.Mana, amount * 0.1f);

            //魔导成长
            var magic = pharna.developableAbilities.Where(p => p.def == MagicDef).First();
            //每点心灵熵0.05点
            magic?.GainProficiency(amount * 0.05f);
        }
    }
}