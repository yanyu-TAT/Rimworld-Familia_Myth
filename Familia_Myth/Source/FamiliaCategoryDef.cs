using FamiliaMyth.Quest;
using Verse;

namespace FamiliaMyth.Familia
{
    public class FamiliaCategoryDef : Def
    {
        public int sortOrder = -1; //排序

        public List<FamiliaRankDef> rankDefs;

        public List<GuildQuestDef> dailyQuests;
        public List<GuildQuestDef> rankUpQuests;

        public List<GuildQuestGenerator> GetAvailableDailyQuests()
        {
            if (dailyQuests.NullOrEmpty()) return new List<GuildQuestGenerator>();

            return dailyQuests.Select(GuildQuestGenerator.Create)
                .Where(g => g != null && g.Available())
                .ToList();
        }

        public List<GuildQuestGenerator> GetAvailableRankUpQuests()
        {
            if (rankUpQuests.NullOrEmpty()) return new List<GuildQuestGenerator>();

            return rankUpQuests.Select(GuildQuestGenerator.Create)
                .Where(g => g != null && g.Available())
                .ToList();
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var s in base.ConfigErrors())
                yield return s;

            if (!DefDatabase<FamiliaRankDef>.AllDefsListForReading.Any(d => d.categoryDef == this))
                yield return $"Category {defName} Defined No Ranks.";

            if (dailyQuests == null || !dailyQuests.Any())
                yield return $"Category {defName} Defined No Daily Quests";

            if (rankUpQuests == null || !rankUpQuests.Any())
                yield return $"Category {defName} Defined No Rank Up Quests";
        }
    }
}
