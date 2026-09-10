using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 发展能力 异常抗性
    /// </summary>
    public class DevelopableAbility_AbnormalResistance : DevelopableAbility
    {
        private const float ImmunityGainPerProf = 0.005f;
        private const float ToxicEnvironmentPerProf = 0.0025f;
        private const float ToxicResistPerProf = 0.0015f;
        private const float DiseaseResistBase = 0.2f;
        private const float DiseaseResistPerProf = 0.00075f;
        public const float ProfGainPerCure = 5f;
        public const float ProfGainPerDissipated = 0.5f;

        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            if (pharna == null || stat == null) return false;
            if (stat == StatDefOf.ImmunityGainSpeed)
            {
                value += proficiency * ImmunityGainPerProf;
                return true;
            }
            if (stat == StatDefOf.ToxicEnvironmentResistance)
            {
                value += proficiency * ToxicEnvironmentPerProf;
                return true;
            }
            if (stat == StatDefOf.ToxicResistance)
            {
                value += proficiency * ToxicResistPerProf;
                return true;
            }

            return false;
        }

        public float DiseaseResistChance => DiseaseResistBase + Proficiency * DiseaseResistPerProf;

        public override string GetExplanation(StatDef stat)
        {
            if (pharna == null || stat == null)
                return "";
            string label = "FamiliaMyth.DevelopableAbility.AbnormalResistanceLabel".Translate();
            string str = "Lv." + pharna.level.ToString() + " " + label + Rank + ": ";
            if (stat == StatDefOf.ImmunityGainSpeed)
            {
                str += "+" + (proficiency * ImmunityGainPerProf).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.ToxicEnvironmentResistance)
            {
                str += "+" + (proficiency * ToxicEnvironmentPerProf).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.ToxicResistance)
            {
                str += "+" + (proficiency * ToxicResistPerProf).ToStringPercent();
                return str;
            }

            return "";
        }
    }
}