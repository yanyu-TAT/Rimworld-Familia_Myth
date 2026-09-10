using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class DevelopableAbility_LuckOfTheSea : DevelopableAbility, IAscendReward //显式实现接口以覆盖默认实现
    {
        //钓鱼速度倍率
        private const float SpeedPerProf = 0.008f;
        //钓鱼产出数量倍率
        private const float YieldPerProf = 0.008f;
        //每次完成钓鱼增长的熟练度
        public const float ProfGainPerCatch = 1f;

        public float ExtraRareCatchChance => 0.1f + proficiency * 0.0005f;

        //仅Odyssey启用时可注册
        public bool Available => ModsConfig.OdysseyActive;

        public override bool ApplyModifiers(StatDef stat, ref float value)
        {
            if (pharna == null || stat == null)
                return false;
            if (stat == StatDefOf.FishingSpeed)
            {
                value *= 1f + SpeedPerProf * proficiency;
                return true;
            }
            if (stat == StatDefOf.FishingYield)
            {
                value *= 1f + YieldPerProf * proficiency;
                return true;
            }

            return false;
        }

        public override string GetExplanation(StatDef stat)
        {
            if (pharna == null || stat == null) return "";
            string label = "FamiliaMyth.DevelopableAbility.LuckOfTheSeaLabel".Translate();
            string str = "Lv." + pharna.level + " " + label + Rank + ": ";
            if (stat == StatDefOf.FishingSpeed)
            {
                str += "x" + (1f + proficiency * SpeedPerProf).ToStringPercent();
                return str;
            }
            if (stat == StatDefOf.FishingYield)
            {
                str += "x" + (1f + proficiency * YieldPerProf).ToStringPercent();
                return str;
            }
            return "";
        }
    }
}
