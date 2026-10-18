using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Hediff_2FightIs2Be : HediffWithComps
    {
        private const float MentalBreakThresholdRate = 800f;
        private const float HungerRestFactorRate = 1500f;

        private readonly HediffStage stage = new();
        private readonly List<StatModifier> modifiers = new() { new() { stat = StatDefOf.MentalBreakThreshold} };
        public override HediffStage CurStage
        {
            get
            {
                float age = ageTicks == 0 ? 1 : ageTicks;
                stage.hungerRateFactor = Mathf.Clamp01(HungerRestFactorRate / age);
                stage.restFallFactor = Mathf.Clamp01(HungerRestFactorRate / age);
                stage.statFactors = modifiers;
                modifiers[0].value = Mathf.Clamp01(MentalBreakThresholdRate / age);

                return stage;
            }
        }

        //脱离战斗自动移除
        public override bool ShouldRemove => base.ShouldRemove || pawn?.records?.BattleActive == null;
    }

    public class HediffComp_2FightIs2Be : HediffComp, IStatusModifier
    {
        private const float StatusGainPerTick = 1f / 50f;

        public int Age => parent?.ageTicks ?? 0;

        public void ApplyStatusModifier(PharnaStatusType type, ref float val)
        {
            val += Age * StatusGainPerTick;
        }

        public string GetExplanation(PharnaStatusType type)
            => "FamiliaMyth.Ability.2FightIs2Be".Translate((Age * StatusGainPerTick).ToString("F1"));
    }
}
