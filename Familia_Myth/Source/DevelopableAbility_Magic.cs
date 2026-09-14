using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class DevelopableAbility_Magic : DevelopableAbility, IManaControlModifier
    {
        //心灵熵消散速率
        private const float EntropyRecoveryPerProf = 0.0005f;
        //魔力操控（经魔法强度体现）
        private const float ManaControlPerProf = 0.0025f;
        //每点产生的心灵熵
        public const float ProfGainPerEntropy = 0.05f;
        //每点消耗的魔力
        public const float ProfGainPerMana = 0.0005f;

        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            if (pharna == null || stat == null) return false;
            if (stat.defName == "PsychicEntropyRecoveryRate")
            {
                value += proficiency * EntropyRecoveryPerProf;
                return true;
            }
            return false;
        }

        public void ApplyManaControlModifier(ref float power)
        {
            power += proficiency * ManaControlPerProf;
        }

        public override string GetExplanation(StatDef stat)
        {
            if (pharna == null || stat == null)
                return "";
            string label = "FamiliaMyth.DevelopableAbility.MagicLabel".Translate();
            string str = "Lv." + pharna.level.ToString() + " " + label + Rank + ": ";
            if (stat.defName == "PsychicEntropyRecoveryRate")
            {
                str += "+" + (proficiency * EntropyRecoveryPerProf).ToString("F3") + "/s";
                return str;
            }

            return "";
        }
    }
}
