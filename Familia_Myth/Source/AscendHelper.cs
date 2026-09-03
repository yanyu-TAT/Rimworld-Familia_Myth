using FamiliaMyth.Abilities;
using UnityEngine;
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
        bool Available => true;
        RewardType RewardType { get; }
        RewardRarity Rarity { get; }

        void Apply(Hediff_Pharna pharna);

        void RewardInitialize();

        string Label { get; }
        string Description { get; }

        Color RarityColor => Rarity switch
        {
            RewardRarity.Common => new Color(0.3f, 0.8f, 0.9f),
            RewardRarity.Uncommon => new Color(1f, 0.3f, 1f),
            RewardRarity.Rare => new Color(1f, 0.75f, 0.2f),
            RewardRarity.Unique => new Color(1f, 0.1f, 0.1f),
            _ => Color.white
        };
    }

    public static class AscendHelper
    {
        public static List<(Func<IAscendReward> Factory, int Weight)> rewards = new();

        //通过Def为奖励注册
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
            if (!t.Available) return;
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
        }

        /// <summary>
        /// 为对应的恩惠获取不重复的随机奖励
        /// </summary>
        /// <param name="n">奖励数量</param>
        /// <returns>实例集合</returns>
        public static List<IAscendReward> GetRandomRewardsFor(int n, Hediff_Pharna pharna)
        {
            List<(Func<IAscendReward> Factory, int Weight)> result = new();
            var pool = rewards.Where(r =>
            {
                var reward = r.Factory();
                reward.RewardInitialize();
                return !pharna.HasReward(reward);
            }).ToList();

            for (int i = 0; i < n; i++)
                if (pool.TryRandomElementByWeight(r => r.Weight, out var r))
                {
                    result.Add(r);
                    pool.Remove(r);
                }

            if (result.Count == 0)
                Log.Warning($"Can't Get Any Reward For {pharna.pawn.Name.ToStringShort}");

            return result.Select( f =>
            {
                var reward = f.Factory();
                reward.RewardInitialize();
                return reward;
            }).ToList();
        }
    }
}