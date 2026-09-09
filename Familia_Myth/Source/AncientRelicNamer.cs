using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace FamiliaMyth.Quest
{
    //圣物匣命名工具：随机抽一个「其他文化」(meme) 的词条，
    //按原版圣物命名规则包 NamerIdeoRelic 现造一个异教圣物名（如 Longlier / Skycaller）。
    //出处：IdeoFoundation.cs L715（GrammarRequest.IncludesBare.Add(meme.generalRules)）、
    //      RulePacks_Ideo_Relic.xml（root r_ideoRelicName）。
    public static class AncientRelicNamer
    {
        public const string RelicNameRulePack = "NamerIdeoRelic";
        public const string RelicNameRootRule = "r_ideoRelicName";

        public static string GenerateRelicName()
        {
            //随机选一个 meme 词条源（优先非玩家主流文化；词条缺失时回退任选）
            MemeDef meme = null;
            try
            {
                if (Faction.OfPlayer?.ideos != null)
                {
                    var excluded = Faction.OfPlayer.ideos.AllIdeos
                        .SelectMany(i => i.memes)
                        .ToHashSet();
                    var pool = DefDatabase<MemeDef>.AllDefsListForReading
                        .Where(m => m.generalRules != null && !excluded.Contains(m)).ToList();
                    if (pool.Count > 0)
                        meme = pool.RandomElement();
                }
            }
            catch { }
            meme ??= DefDatabase<MemeDef>.AllDefsListForReading
                .Where(m => m.generalRules != null).RandomElement();

            var request = new GrammarRequest();
            var rulePack = DefDatabase<RulePackDef>.GetNamedSilentFail(RelicNameRulePack);
            if (rulePack != null)
                request.Includes.Add(rulePack);
            if (meme?.generalRules != null)
                request.IncludesBare.Add(meme.generalRules);

            string result = null;
            try
            {
                result = GrammarResolver.Resolve(RelicNameRootRule, request);
            }
            catch { }
            if (result.NullOrEmpty())
                return null;
            return GenText.CapitalizeAsTitle(result);
        }
    }
}
