using FamiliaMyth.Abilities;
using FamiliaMyth.UI;
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

        /// <summary>
        /// 奖励身份，取自对应 Def 的 defName，用于判断恩惠是否已拥有该奖励
        /// </summary>
        string DefName { get; }

        void Apply(Hediff_Pharna pharna);

        void RewardInitialize();

        string Label { get; }
        string Description { get; }

        Color RarityColor => Rarity switch
        {
            RewardRarity.Common => UI_Palette.RarityCommon,
            RewardRarity.Uncommon => UI_Palette.RarityUncommon,
            RewardRarity.Rare => UI_Palette.RarityRare,
            RewardRarity.Unique => UI_Palette.RarityUnique,
            _ => Color.white
        };
    }

    /// <summary>
    /// 升华奖励注册项，只保存类型、稀有度与权重，实例在抽取时创建
    /// </summary>
    public readonly struct AscendRewardEntry
    {
        public Type Type { get; }
        public RewardRarity Rarity { get; }
        public float Weight { get; }

        public AscendRewardEntry(Type type, RewardRarity rarity, float weight)
        {
            Type = type;
            Rarity = rarity;
            Weight = weight;
        }
    }

    [StaticConstructorOnStartup]
    public static class AscendHelper
    {
        //各稀有度的抽取权重
        private const float CommonWeight = 20f;
        private const float UncommonWeight = 10f;
        private const float RareWeight = 1f;

        //保底：连续未获得罕见奖励时，罕见权重每层增加的倍率与封顶
        private const float PityWeightStep = 1f;
        private const float PityWeightMaxMultiplier = 10f;

        //每次升华的刷新次数
        private const int RerollsPerAscend = 1;

        private static readonly List<AscendRewardEntry> entries = new();

        /// <summary>
        /// 已注册的升华奖励，含不入随机池的项（权重不大于零）
        /// </summary>
        public static IReadOnlyList<AscendRewardEntry> Entries => entries;

        //通过Def为奖励注册
        static AscendHelper()
        {
            try
            {
                foreach (var def in DefDatabase<SpellDef>.AllDefs)
                {
                    if (def.spellClass != null)
                        RegisterReward(def.spellClass, def.rarity);
                }

                foreach (var def in DefDatabase<SkillDef>.AllDefs)
                {
                    if (def.skillClass != null)
                        RegisterReward(def.skillClass, def.rarity);
                }

                foreach (var def in DefDatabase<DevelopableAbilityDef>.AllDefs)
                {
                    if (def.abilityClass != null)
                        RegisterReward(def.abilityClass, def.rarity);
                }
            }
            catch (Exception e)
            {
                Log.Error($"[FamiliaMyth] Failed to build ascend reward registry: {e}");
            }
        }

        /// <summary>
        /// 注册升华奖励，权重不大于零的稀有度只登记不入随机池
        /// </summary>
        private static void RegisterReward(Type type, RewardRarity rarity)
        {
            entries.Add(new AscendRewardEntry(type, rarity, WeightOf(rarity)));
        }

        private static float WeightOf(RewardRarity rarity) => rarity switch
        {
            RewardRarity.Common => CommonWeight,
            RewardRarity.Uncommon => UncommonWeight,
            RewardRarity.Rare => RareWeight,
            _ => 0f    //独有奖励不入随机池，由特殊途径发放
        };

        /// <summary>
        /// 本次升华可用的刷新次数，后续版本按仪式质量计算
        /// </summary>
        public static int GetRerollCount(Hediff_Pharna pharna) => RerollsPerAscend;

        /// <summary>
        /// 计入保底后的抽取权重
        /// </summary>
        private static float WeightFor(AscendRewardEntry entry, Hediff_Pharna pharna)
        {
            if (entry.Weight <= 0f) return 0f;
            if (entry.Rarity != RewardRarity.Rare || pharna.ascendsWithoutRare <= 0) return entry.Weight;

            float multiplier = Mathf.Min(1f + PityWeightStep * pharna.ascendsWithoutRare, PityWeightMaxMultiplier);
            return entry.Weight * multiplier;
        }

        /// <summary>
        /// 抽取种子：同一恩惠、同一等级、同一轮刷新得到同一批结果，存读档重做仪式不会改变结果
        /// </summary>
        private static int GetRewardSeed(Hediff_Pharna pharna, int rerollIndex)
        {
            int pawnSeed = Gen.HashCombineInt(pharna.pawn?.thingIDNumber ?? 0, pharna.level);
            return Gen.HashCombineInt(pawnSeed, rerollIndex);
        }

        /// <summary>
        /// 创建并初始化一个奖励实例，类缺少对应 Def 或无法实例化时返回 null
        /// </summary>
        public static IAscendReward CreateReward(Type type)
        {
            if (type == null) return null;

            try
            {
                if (Activator.CreateInstance(type) is not IAscendReward reward)
                {
                    Log.Error($"[FamiliaMyth] {type} is not an ascend reward");
                    return null;
                }

                reward.RewardInitialize();

                if (reward.DefName.NullOrEmpty())
                {
                    Log.Error($"[FamiliaMyth] Ascend reward {type} has no matching def");
                    return null;
                }

                return reward;
            }
            catch (Exception e)
            {
                Log.Error($"[FamiliaMyth] Failed to create ascend reward {type}: {e}");
                return null;
            }
        }

        /// <summary>
        /// 为对应的恩惠获取不重复的随机奖励，可用性与权重在抽取时判定
        /// </summary>
        /// <param name="n">奖励数量</param>
        /// <param name="pharna">目标恩惠</param>
        /// <param name="excludeDefNames">不参与本次抽取的奖励</param>
        /// <param name="rerollIndex">本次升华内已消耗的刷新次数，用于区隔刷新前后的结果</param>
        /// <returns>实例集合</returns>
        public static List<IAscendReward> GetRandomRewardsFor(int n, Hediff_Pharna pharna,
            HashSet<string> excludeDefNames = null, int rerollIndex = 0)
        {
            List<IAscendReward> result = new();
            if (pharna == null) return result;

            List<(IAscendReward Reward, float Weight)> pool = new();
            foreach (var entry in entries)
            {
                if (entry.Weight <= 0f) continue;

                var reward = CreateReward(entry.Type);
                if (reward == null) continue;
                if (!reward.Available) continue;
                if (excludeDefNames != null && excludeDefNames.Contains(reward.DefName)) continue;
                if (pharna.HasReward(reward)) continue;

                pool.Add((reward, WeightFor(entry, pharna)));
            }

            //抽样包在确定性种子里，使结果只取决于恩惠、等级与刷新轮次
            Rand.PushState(GetRewardSeed(pharna, rerollIndex));
            try
            {
                for (int i = 0; i < n; i++)
                    if (pool.TryRandomElementByWeight(p => p.Weight, out var picked))
                    {
                        result.Add(picked.Reward);
                        pool.Remove(picked);
                    }
            }
            finally
            {
                Rand.PopState();
            }

            if (result.Count == 0)
                Log.Error($"[FamiliaMyth] Can't get any ascend reward for {pharna.pawn?.Name?.ToStringShort ?? "unknown"}");

            return result;
        }
    }

    /// <summary>
    /// 一次升华的奖励选取会话：卡片、锁定状态与剩余刷新次数
    /// </summary>
    public class AscendRewardSession
    {
        private readonly Hediff_Pharna pharna;
        private readonly List<IAscendReward> rewards;
        private readonly HashSet<int> lockedIndexes = new();

        //已消耗的刷新次数，参与抽取种子
        private int rerollIndex;

        public IReadOnlyList<IAscendReward> Rewards => rewards;
        public int RerollsLeft { get; private set; }
        public bool AllLocked => lockedIndexes.Count >= rewards.Count;
        public bool CanRefresh => RerollsLeft > 0 && !AllLocked;

        public AscendRewardSession(Hediff_Pharna pharna, int rewardCount, int rerollCount)
        {
            this.pharna = pharna;
            RerollsLeft = rerollCount;
            rewards = AscendHelper.GetRandomRewardsFor(rewardCount, pharna);
        }

        public bool IsLocked(int index) => lockedIndexes.Contains(index);

        public void ToggleLock(int index)
        {
            if (index < 0 || index >= rewards.Count) return;
            if (!lockedIndexes.Remove(index))
                lockedIndexes.Add(index);
        }

        /// <summary>
        /// 重抽未锁定的卡位，排除锁定卡与上一批卡片；抽取不足时保留原有卡片
        /// </summary>
        public void Refresh()
        {
            if (!CanRefresh) return;

            HashSet<string> exclude = new();
            foreach (var reward in rewards)
                exclude.Add(reward.DefName);

            int count = rewards.Count - lockedIndexes.Count;
            rerollIndex++;
            var rolled = AscendHelper.GetRandomRewardsFor(count, pharna, exclude, rerollIndex);
            if (rolled.Count != count)
            {
                Log.Error("[FamiliaMyth] Refreshing ascend rewards failed, current rewards are kept");
                return;
            }

            RerollsLeft--;
            int cursor = 0;
            for (int i = 0; i < rewards.Count; i++)
                if (!lockedIndexes.Contains(i))
                    rewards[i] = rolled[cursor++];
        }

        /// <summary>
        /// 选定奖励并结算保底
        /// </summary>
        public void Pick(IAscendReward reward)
        {
            if (reward == null) return;
            reward.Apply(pharna);
            pharna.NotifyAscendRewardPicked(reward.Rarity);
        }
    }
}
