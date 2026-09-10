using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Patch
{
    public static class Patch_ImmunityHandler
    {
        [HarmonyPatch]
        public static class Patch_DiseaseContractChanceFactor
        {
            public static MethodBase TargetMethod() =>
                AccessTools.Method(typeof(ImmunityHandler), nameof(ImmunityHandler.DiseaseContractChanceFactor),
                    new[] { typeof(HediffDef), typeof(HediffDef).MakeByRefType(), typeof(BodyPartRecord) });

            public static void Prefix(ImmunityHandler __instance, HediffDef diseaseDef)
            {
                var pawn = __instance?.pawn;
                if (pawn == null || pawn.health == null || diseaseDef == null) return;

                //避免重复判定
                if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Hediff_AbnormalResistanceImmunity")) != null)
                    return;

                var pharna = pawn.GetPharna();
                if (pharna?.developableAbilities?.FirstOrDefault(a => a is DevelopableAbility_AbnormalResistance) 
                    is not DevelopableAbility_AbnormalResistance abr) return;

                //20% + 0.0075% * x
                float resistChance = abr.DiseaseResistChance;
                Log.Message($"Trying Give Immunity To {pawn.Name.ToStringShort} With Chance : {resistChance}");
                if (Rand.Chance(resistChance))
                {
                    var hiddenDef = DefDatabase<HediffDef>.GetNamed("Hediff_AbnormalResistanceImmunity");
                    pawn.health.AddHediff(HediffMaker.MakeHediff(hiddenDef, pawn));
                }
            }
        }

        [HarmonyPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff))]
        public static class Patch_RemoveHediff
        {
            public static void Postfix(Pawn_HealthTracker __instance, Hediff hediff, Pawn ___pawn)
            {
                if (hediff?.def == null || ___pawn == null) return;
                if (!hediff.def.PossibleToDevelopImmunityNaturally()) return;

                var pharna = ___pawn.GetPharna();
                var abr = pharna?.developableAbilities?.FirstOrDefault(a => a is DevelopableAbility_AbnormalResistance) as DevelopableAbility_AbnormalResistance;
                abr?.GainProficiency(DevelopableAbility_AbnormalResistance.ProfGainPerCure); //每次痊愈
            }
        }

        [HarmonyPatch(typeof(HediffComp_SeverityModifierBase), nameof(HediffComp_SeverityModifierBase.CompPostTickInterval))]
        public static class Patch_ToxinDissipation
        {
            public static void Postfix(HediffComp_SeverityModifierBase __instance)
            {
                //仅针对毒素消散
                if (__instance is not HediffComp_ImmunizableToxic) return;

                //每日速率
                float perDay = __instance.SeverityChangePerDay();
                if (perDay >= 0f) return;

                var pawn = __instance?.Pawn;
                if (pawn == null) return;

                var pharna = pawn.GetPharna();
                if (pharna?.developableAbilities?.FirstOrDefault(a => a is DevelopableAbility_AbnormalResistance)
                    is not DevelopableAbility_AbnormalResistance abr) return;

                float dissipated = Mathf.Abs(perDay) / 300f;
                abr.GainProficiency(dissipated * DevelopableAbility_AbnormalResistance.ProfGainPerDissipated); // 每点消散
            }
        }
    }
}
