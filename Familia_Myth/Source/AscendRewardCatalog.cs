using System;
using System.Collections.Generic;
using FamiliaMyth.Abilities;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Pharna
{
    /// <summary>
    /// 一条可获得的升华奖励：只保存 Def 引用与展示信息，不持有奖励实例
    /// </summary>
    public readonly struct AscendRewardCatalogEntry
    {
        public int Index { get; }
        public Def Def { get; }
        public RewardType Type { get; }
        public RewardRarity Rarity { get; }
        public Type RewardClass { get; }
        public string DefName { get; }
        public string Label { get; }
        public string Description { get; }
        public Texture2D Icon { get; }

        public AscendRewardCatalogEntry(int index, Def def, RewardType type, RewardRarity rarity,
            Type rewardClass, Texture2D icon)
        {
            Index = index;
            Def = def;
            Type = type;
            Rarity = rarity;
            RewardClass = rewardClass;
            DefName = def?.defName;
            Label = def?.label ?? def?.defName;
            Description = def?.description ?? string.Empty;
            Icon = icon;
        }
    }

    /// <summary>
    /// 升华奖励全集：与 AscendHelper 共用同一套「有效奖励」判据
    /// 构建顺序即抽取注册顺序（魔法 → 技能 → 发展能力），保证确定性种子下的结果不变
    /// </summary>
    public static class AscendRewardCatalog
    {
        private static readonly List<AscendRewardCatalogEntry> entries = new();

        private static bool built;

        public static IReadOnlyList<AscendRewardCatalogEntry> All
        {
            get
            {
                EnsureBuilt();
                return entries;
            }
        }

        public static int TotalCount
        {
            get
            {
                EnsureBuilt();
                return entries.Count;
            }
        }

        /// <summary>
        /// 按 Def 库构建全集；类的引用字段为空的 Def 实际抽不到，一律跳过
        /// </summary>
        public static void EnsureBuilt()
        {
            if (built) return;
            built = true;

            try
            {
                foreach (SpellDef def in DefDatabase<SpellDef>.AllDefs)
                {
                    if (def?.spellClass == null) continue;
                    Add(def, def.spellClass, RewardType.Spell, def.rarity, IconOf(def.uiIcon));
                }

                foreach (SkillDef def in DefDatabase<SkillDef>.AllDefs)
                {
                    if (def?.skillClass == null) continue;
                    Add(def, def.skillClass, RewardType.Skill, def.rarity, IconOfPath(def.iconPath));
                }

                foreach (DevelopableAbilityDef def in DefDatabase<DevelopableAbilityDef>.AllDefs)
                {
                    if (def?.abilityClass == null) continue;
                    Add(def, def.abilityClass, RewardType.DevelopableAbility, def.rarity, null);
                }
            }
            catch (Exception e)
            {
                Log.Error($"[FamiliaMyth] Failed to build ascend reward catalog: {e}");
            }
        }

        public static int CountOf(RewardType type)
        {
            EnsureBuilt();
            int n = 0;
            for (int i = 0; i < entries.Count; i++)
                if (entries[i].Type == type) n++;
            return n;
        }

        public static AscendRewardCatalogEntry GetByDefName(string defName)
        {
            EnsureBuilt();
            if (defName.NullOrEmpty()) return default;
            for (int i = 0; i < entries.Count; i++)
                if (entries[i].DefName == defName) return entries[i];
            return default;
        }

        private static void Add(Def def, Type rewardClass, RewardType type, RewardRarity rarity, Texture2D icon)
            => entries.Add(new AscendRewardCatalogEntry(entries.Count + 1, def, type, rarity, rewardClass, icon));

        //失败贴图按无图标处理
        private static Texture2D IconOf(Texture2D icon)
            => icon == null || icon == BaseContent.BadTex ? null : icon;

        private static Texture2D IconOfPath(string path)
            => path.NullOrEmpty() ? null : ContentFinder<Texture2D>.Get(path, false);
    }
}
