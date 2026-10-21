using System.Text;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 当前装备武器熟练度的展示状态，标签按档位显示，tooltip 显示具体概率。
    /// </summary>
    public class Hediff_WeaponAttunement : Hediff
    {
        private static HediffDef cachedDef;
        public static HediffDef HediffDef => cachedDef ??= DefDatabase<HediffDef>.GetNamed("Hediff_WeaponAttunement");

        public ThingDef weaponDef;

        private DevelopableAbility_WeaponAttunement ability;

        private DevelopableAbility_WeaponAttunement Ability
        {
            get
            {
                if (ability == null && pawn != null)
                    ability = pawn.GetWeaponAttunement();
                return ability;
            }
        }

        private string Rank => Ability?.GetRankText(weaponDef) ?? "I";

        public override string Label
        {
            get
            {
                if (weaponDef == null) return base.Label;
                return "FamiliaMyth.WeaponAttunement.HediffLabel".Translate(
                    weaponDef.LabelCap.ToString(), Rank);
            }
        }

        public override string TipStringExtra
        {
            get
            {
                DevelopableAbility_WeaponAttunement attunement = Ability;
                if (attunement == null || weaponDef == null) return "";

                float value = attunement.GetWeaponProficiency(weaponDef);
                float max = attunement.MaxWeaponProficiency;
                StringBuilder sb = new();
                sb.AppendLine("FamiliaMyth.WeaponAttunement.Summary".Translate(
                    ((int) value).ToString(), ((int) max).ToString()));
                sb.AppendLine("FamiliaMyth.WeaponAttunement.ResetChance".Translate(
                    attunement.GetCooldownResetChance(weaponDef).ToStringPercent()));
                sb.AppendLine("FamiliaMyth.WeaponAttunement.CritChance".Translate(
                    attunement.GetCritChance(weaponDef).ToStringPercent()));
                return sb.ToString().TrimEnd();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref weaponDef, "weaponDef");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                ability = null;
        }
    }
}
