using FamiliaMyth.Familia;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //强制任务「据点清剿」根节点：读取分类、统一计算点数（内部自动 ×1.5）并生成站点参数
    public class QuestNode_Root_RankUp_SiteClearance : QuestNode
    {
        public SlateRef<FamiliaCategoryDef> category;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var cat = category.GetValue(slate);
            if (cat == null) return;

            //统一任务点数计算（强制任务内部自动 ×1.5）
            QuestUtilities.CalculateTaskParams(isRankUp: true, out float threatPoints);

            //生成站点
            var outpostDef = DefDatabase<SitePartDef>.GetNamed("Outpost");
            if (outpostDef == null) return;
            var sitePartsParams = new List<SitePartDefWithParams>
            {
                new(outpostDef, new SitePartParams { threatPoints = threatPoints })
            };

            //随机敌对派系
            var faction = Find.FactionManager.RandomEnemyFaction(
                allowHidden: false, allowNonHumanlike: true, allowDefeated: false);

            slate.Set("difficulty", threatPoints);
            slate.Set("sitePartsParams", sitePartsParams);
            slate.Set("siteFaction", faction);
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
