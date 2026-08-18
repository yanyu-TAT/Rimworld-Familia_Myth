using FamiliaMyth.Pharna;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Hediff_EfficientCirculation : Hediff
    {
        public DevelopableAbility_EfficientCirculation ability;

        public DevelopableAbility_EfficientCirculation Ability
        {
            get
            {
                if (ability == null)
                {
                    if (pawn.GetPharna().developableAbilities.FirstOrDefault(d => d is DevelopableAbility_EfficientCirculation)
                        is DevelopableAbility_EfficientCirculation abi)
                        ability = abi;
                }
                return ability;
            }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (pawn.needs?.rest?.CurLevel == null || Ability == null) return null;
                var stage = new HediffStage()
                {
                    hungerRateFactor = Ability.NutritionFactor * Mathf.Lerp(0, 1, pawn.needs.rest.CurLevel)
                };
                return stage;
            }
        }
    }
}
