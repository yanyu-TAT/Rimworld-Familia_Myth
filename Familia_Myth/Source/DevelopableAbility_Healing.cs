using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class DevelopableAbility_Healing : DevelopableAbility, IManaControlModifier
    {
        private static StatDef medicalOperationSpeedDef;
        private static StatDef MedicalOperationSpeedDef =>
            medicalOperationSpeedDef ??= DefDatabase<StatDef>.GetNamed("MedicalOperationSpeed");

        private const float TendQualityPerProf = 0.002f;
        private const float TendSpeedPerProf = 0.005f;
        private const float SurgeryChancePerProf = 0.003f;
        private const float ManaControlPerProf = 0.0025f;
        public const float ProfGainPerXp = 0.005f;

        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            if (pharna == null || stat == null)
                return false;
            if (stat == StatDefOf.MedicalTendQuality)
            {
                value *= 1 + proficiency * TendQualityPerProf;
                return true;
            }
            if (stat == StatDefOf.MedicalTendSpeed || stat == MedicalOperationSpeedDef)
            {
                value *= 1 + proficiency * TendSpeedPerProf;
                return true;
            }
            if (stat == StatDefOf.MedicalSurgerySuccessChance)
            {
                value *= 1 + proficiency * SurgeryChancePerProf;
                return true;
            }
            return false;
        }

        public bool ShouldApplyManaControl(SpellType type)
            => type == SpellType.Heal;

        public void ApplyManaControlModifier(ref float power)
        {
            power += proficiency * ManaControlPerProf;
        }

        public override string GetExplanation(StatDef stat)
        {
            if (pharna == null || stat == null)
                return "";
            string label = "FamiliaMyth.DevelopableAbility.HealingLabel".Translate();
            string str = "Lv." + pharna.level.ToString() + " " + label + Rank + ": ";
            if (stat == StatDefOf.MedicalTendQuality)
            {
                str += "x" + (1 + proficiency * TendQualityPerProf).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.MedicalTendSpeed || stat == MedicalOperationSpeedDef)
            {
                str += "x" + (1 + proficiency * TendSpeedPerProf).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.MedicalSurgerySuccessChance)
            {
                str += "x" + (1 + proficiency * SurgeryChancePerProf).ToStringPercent();
                return str;
            }

            return "";
        }
    }
}
