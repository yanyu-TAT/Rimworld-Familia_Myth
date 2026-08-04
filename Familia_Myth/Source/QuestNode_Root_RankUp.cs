using FamiliaMyth.Familia;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace Familia_Myth.Source
{
    public class QuestNode_Root_RankUp : QuestNode
    {
        //强制任务难度系数
        const float RankUpMultiplier = 1.5f;

        public SlateRef<FamiliaCategoryDef> category;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var cat = category.GetValue(slate);
            if (cat == null) return;

            float difficulty = StorytellerUtility.DefaultSiteThreatPointsNow() * RankUpMultiplier;

            //生成站点
            var outpostDef = DefDatabase<SitePartDef>.GetNamed("Outpost");
            if (outpostDef == null) return;
            var sitePartsParams = new List<SitePartDefWithParams>
            {
                new(outpostDef, new SitePartParams { threatPoints = difficulty })
            };

            //随机敌对派系
            var faction = Find.FactionManager.RandomEnemyFaction(
                allowHidden: false, allowNonHumanlike: true, allowDefeated: false);

            slate.Set("difficulty", difficulty);
            slate.Set("sitePartsParams", sitePartsParams);
            slate.Set("siteFaction", faction);

        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
