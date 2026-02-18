using Verse;

namespace FamiliaMyth.Pharna
{
    public enum RewardType
    {
        DevelopableAbility, //发展能力
        Skill,              //技能
        Magic               //魔法
    }

    public enum RewardRarity
    {
        Common,     //通常
        Rare,       //稀有
        Special,    //特殊
        Unique      //独有
    }

    public interface IAscendReward
    {
        RewardType RewardType { get; }
        RewardRarity Rarity { get; }

        void Apply(Pawn pawn, Hediff_Pharna pharna);
    }

    public static class AscendHelper
    {
        public static List<IAscendReward> rewards = new();

        static AscendHelper()
        {
            //注册升华奖励
        }

        public static List<IAscendReward> GetRandomRewards()
        {
            List<IAscendReward> temp = new();
            //获取随机奖励
            return temp;
        }
    }
}