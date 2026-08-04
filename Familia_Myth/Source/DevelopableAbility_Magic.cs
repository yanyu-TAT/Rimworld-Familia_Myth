using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    /// <summary>
    /// 发展能力 魔导
    /// </summary>
    public class DevelopableAbility_Magic : DevelopableAbility, ISpellPowerModifier
    {

        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            if (pharna == null || stat == null) return false;
            if (stat.defName == "PsychicEntropyRecoveryRate"){
                value += proficiency * 0.001f;
                return true;
            }
            // if (stat.defName == "PsyfocusMax"){
            //     value += proficiency * 0.001f;
            //     return true;
            // }
            return false;
        }

        public void ApplySpellPowerModifier(ref float power)
        {
            power += proficiency * 0.001f;
        }

        public override string GetExplanation(StatDef stat)
        {
            if (pharna == null || stat == null) 
                return "";
            string label = "FamiliaMyth.DevelopableAbilityMagicLabel".Translate();
            string str = "Lv." + pharna.level.ToString()+ " " + label + Rank + proficiency.ToString() + ": ";
            if (stat.defName == "PsychicEntropyRecoveryRate"){
                str += "+" + (proficiency * 0.001f).ToString("F3") + "/s";
                return str;
            }
            // if (stat.defName == "PsyfocusMax"){
            //     str += "+" + (proficiency * 0.001f).ToString("F1");
            //     return str;
            // }

            return "";
        }
    }
}