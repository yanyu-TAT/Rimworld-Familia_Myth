using FamiliaMyth.Abilities;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace FamiliaMyth.Pharna
{
    public enum RewardType
    {
        DevelopableAbility, //发展能力
        Skill,              //技能
        Spell               //魔法
    }

    public enum RewardRarity
    {
        Common,     //通常
        Uncommon,   //稀有
        Rare,       //罕见
        Unique      //独有
    }

    public interface IAscendReward
    {

        RewardType RewardType { get; }
        RewardRarity Rarity { get; }

        void Apply(Hediff_Pharna pharna);

        void RewardInitialize();

        string Label { get; }
        string Description { get; }

    }

    public static class AscendHelper
    {
        public static List<(Func<IAscendReward> Factory, int Weight)> rewards = new();

        private static int totalWeight = 0;

        private static readonly Random random = new();

        //通过Def为魔法注册
        static AscendHelper()
        {
            foreach (var def in DefDatabase<SpellDef>.AllDefs)
            {
                if (def.spellClass != null)
                {
                    RegisterReward(def.spellClass);
                }
            }

            foreach (var def in DefDatabase<SkillDef>.AllDefs)
            {
                if (def.skillClass != null)
                {
                    RegisterReward(def.skillClass);
                }
            }

            foreach (var def in DefDatabase<DevelopableAbilityDef>.AllDefs)
            {
                if (def.abilityClass != null)
                {
                    RegisterReward(def.abilityClass);
                }
            }
        }

        /// <summary>
        /// 注册升华奖励
        /// </summary>
        public static void RegisterReward(Type type)
        {
            var t = Activator.CreateInstance(type) as IAscendReward;
            t.RewardInitialize();
            var rarity = t.Rarity;

            var weight = rarity switch
            {
                RewardRarity.Common => 20,
                RewardRarity.Uncommon => 10,
                RewardRarity.Rare => 1, 
                RewardRarity.Unique => 0, //须由特殊途径获取（后续实现）
                _ => 0
            };
            rewards.Add((() => (IAscendReward)Activator.CreateInstance(type), weight));
            totalWeight += weight;
        }

        /// <summary>
        /// 获取随机奖励
        /// </summary>
        /// <param name="n">奖励数量</param>
        /// <returns>实例集合</returns>
        public static List<IAscendReward> GetRandomRewards(int n)
        {
            var k = rewards.ToList();
            var t = rewards.ToList();
            var tt = totalWeight;
            t.Clear();

            while (n > t.Count && k.Count > 0 && tt > 0)
            {
                var w = 0;
                var r = random.Next(tt);
                foreach (var reward in k)
                {
                    w += reward.Weight;
                    if (w > r)
                    {
                        t.Add(reward);
                        k.Remove(reward);
                        tt -= reward.Weight;
                        break;
                    }
                }
            }

            return t.Select( f =>
            {
                var reward = f.Factory();
                reward.RewardInitialize();
                return reward;
            }).ToList();
        }
    }
}