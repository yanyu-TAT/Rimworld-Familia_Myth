using FamiliaMyth.Pharna;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class DevelopableAbility_EfficientCirculation : DevelopableAbility
    {
        private static HediffDef hediffDef;
        private static HediffDef HediffDeff =>
            hediffDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_EfficientCirculation");

        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            return false;
        }

        public override string GetExplanation(StatDef stat)
        {
            return "";
        }

        /// <summary>
        /// 摄入营养值时的休息值恢复比例
        /// </summary>
        public float RestFactor => 1 - Mathf.Pow(0.998f, proficiency);

        /// <summary>
        /// 营养值消耗降低百分比
        /// </summary>
        public float NutritionFactor => Mathf.Pow(0.999f, proficiency);

        public override void Apply(Hediff_Pharna p)
        {
            base.Apply(p);
            var pawn = p.pawn;
            pawn.health.AddHediff(HediffDeff);
        }
    }
}
