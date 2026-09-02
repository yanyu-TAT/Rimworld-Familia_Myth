using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class DevelopableAbility_Healing : DevelopableAbility, ISpellPowerModifier
    {
        private static StatDef medicalOperationSpeedDef;
        private static StatDef MedicalOperationSpeedDef =>
            medicalOperationSpeedDef ??= DefDatabase<StatDef>.GetNamed("MedicalOperationSpeed");

        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            if (pharna == null || stat == null)
                return false;
            if (stat == StatDefOf.MedicalTendQuality)
            {
                value *= 1 + proficiency * 0.002f;
                return true;
            }
            if (stat == StatDefOf.MedicalTendSpeed || stat == MedicalOperationSpeedDef)
            {
                value *= 1 + proficiency * 0.005f;
                return true;
            }
            if (stat == StatDefOf.MedicalSurgerySuccessChance)
            {
                value *= 1 + proficiency * 0.003f;
                return true;
            }
            return false;
        }

        public bool ShouldApplySpellPower(SpellType type)
            => type == SpellType.Heal;

        public void ApplySpellPowerModifier(ref float power)
        {
            power += proficiency * 0.0025f;
        }

        public override string GetExplanation(StatDef stat)
        {
            if (pharna == null || stat == null)
                return "";
            string label = "治愈";
            string str = "Lv." + pharna.level.ToString() + " " + label + Rank + ": ";
            if (stat == StatDefOf.MedicalTendQuality)
            {
                str += "x" + (1 + proficiency * 0.002f).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.MedicalTendSpeed || stat == MedicalOperationSpeedDef)
            {
                str += "x" + (1 + proficiency * 0.005f).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.MedicalSurgerySuccessChance)
            {
                str += "x" + (1 + proficiency * 0.003f).ToStringPercent();
                return str;
            }

            return "";
        }
    }
}
