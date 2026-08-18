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

            //魔导
            var magic = pharna.developableAbilities.Where(p => p.def == MagicDef).First();
            //每点心灵熵0.05点
            magic?.GainProficiency(amount * 0.05f);
        }


        [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.OffsetPsyfocusDirectly))]
        public static class Patch_OffsetPsyfocus
        {
            public static void Postfix(Pawn_PsychicEntropyTracker __instance, float offset)
            {
                if (offset >= 0f) return;                       //只统计消耗
                if (DevelopableAbility_Magic.IsAutoStoring) return; //排除自动存储转移
                var pharna = Hediff_Pharna.GetPharna(__instance.Pawn);
                //魔导
                var magic = pharna?.developableAbilities.FirstOrDefault(a => a is DevelopableAbility_Magic) as DevelopableAbility_Magic;
                magic?.GainProficiency(Math.Abs(offset) * 5f); //每1%精神力消耗0.05点
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.NeedToShowGizmo))]
        public static void Postfix_NeedToShowGizmo(ref bool __result, Pawn_PsychicEntropyTracker __instance)
        {
            if (__result) return;
            //拥有魔法时强制显示灵能ui
            if ((Hediff_Pharna.GetPharna(__instance.Pawn)?.spells?.Count > 0))
            {
                __result = true;
                //初始化精神力
                if (__instance.CurrentPsyfocus < 0f)
                    __instance.SetInitialPsyfocusLevel();
            }
        }
    }
}