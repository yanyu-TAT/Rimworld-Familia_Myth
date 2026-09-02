using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class DevelopableAbility_Mineralogy : DevelopableAbility
    {
        private const float MiningSpeedPerProf = 0.004f;
        private const float DrillSpeedPerProf = 0.004f;
        private const float MiningYieldPerProf = 0.003f;
        public const float ProfGainPerXp = 0.005f;

        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            if (pharna == null || stat == null)
                return false;
            if (stat == StatDefOf.MiningSpeed)
            {
                value *= 1 + proficiency * MiningSpeedPerProf;
                return true;
            }
            if (stat == StatDefOf.DeepDrillingSpeed)
            {
                value *= 1 + proficiency * DrillSpeedPerProf;
                return true;
            }
            if (stat == StatDefOf.MiningYield)
            {
                value *= 1 + proficiency * MiningYieldPerProf;
                return true;
            }
            return false;
        }

        public override string GetExplanation(StatDef stat)
        {
            if (pharna == null || stat == null)
                return "";
            string label = "矿物学";
            string str = "Lv." + pharna.level.ToString() + " " + label + Rank + ": ";
            if (stat == StatDefOf.MiningSpeed)
            {
                str += "x" + (1 + proficiency * MiningSpeedPerProf).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.DeepDrillingSpeed)
            {
                str += "x" + (1 + proficiency * DrillSpeedPerProf).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.MiningYield)
            {
                str += "x" + (1 + proficiency * MiningYieldPerProf).ToStringPercent();
                return str;
            }
            return "";
        }
    }
}