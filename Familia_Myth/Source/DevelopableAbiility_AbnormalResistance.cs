using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 发展能力 异常抗性
    /// </summary>
    public class DevelopableAbiility_AbnormalResistance : DevelopableAbility
    {
        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            if (pharna == null || stat == null) return false;
            if (stat == StatDefOf.ImmunityGainSpeed)
            {
                value += proficiency * 0.005f;
                return true;
            }
            if (stat == StatDefOf.ToxicEnvironmentResistance)
            {
                value += proficiency * 0.0025f;
                return true;
            }
            if (stat == StatDefOf.ToxicResistance)
            {
                value += proficiency * 0.0015f;
                return true;
            }

            return false;
        }

        public float DiseaseResistChance => 0.2f + Proficiency * 0.00075f;

        public override string GetExplanation(StatDef stat)
        {

            if (pharna == null || stat == null)
                return "";
            string label = "FamiliaMyth.DevelopableAbilityAbnormalResistanceLabel".Translate();
            string str = "Lv." + pharna.level.ToString() + " " + label + Rank + proficiency.ToString() + ": ";
            if (stat == StatDefOf.ImmunityGainSpeed)
            {
                str += "+" + (proficiency * 0.005f).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.ToxicEnvironmentResistance)
            {
                str += "+" + (proficiency * 0.0025f).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.ToxicResistance)
            {
                str += "+" + (proficiency * 0.0015f).ToStringPercent();
                return str;
            }

            return "";
        }
    }
}