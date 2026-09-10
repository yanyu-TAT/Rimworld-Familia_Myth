using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class DevelopableAbility_Botany : DevelopableAbility
    {
        private const float PlantWorkPerProf = 0.004f;
        private const float HarvestYieldPerProf = 0.003f;
        private const float ForagePerProf = 0.004f;
        public const float ProfGainPerXp = 0.005f;

        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            if (pharna == null || stat == null)
                return false;
            if (stat == StatDefOf.PlantWorkSpeed)
            {
                value *= 1 + proficiency * PlantWorkPerProf;
                return true;
            }
            if (stat == StatDefOf.PlantHarvestYield)
            {
                value *= 1 + proficiency * HarvestYieldPerProf;
                return true;
            }
            if (stat == StatDefOf.ForagedNutritionPerDay)
            {
                value *= 1 + proficiency * ForagePerProf;
                return true;
            }
            return false;
        }

        public override string GetExplanation(StatDef stat)
        {
            if (pharna == null || stat == null)
                return "";
            string label = "FamiliaMyth.DevelopableAbility.BotanyLabel".Translate();
            string str = "Lv." + pharna.level.ToString() + " " + label + Rank + ": ";
            if (stat == StatDefOf.PlantWorkSpeed)
            {
                str += "x" + (1 + proficiency * PlantWorkPerProf).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.PlantHarvestYield)
            {
                str += "x" + (1 + proficiency * HarvestYieldPerProf).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.ForagedNutritionPerDay)
            {
                str += "x" + (1 + proficiency * ForagePerProf).ToStringPercent();
                return str;
            }
            return "";
        }
    }
}