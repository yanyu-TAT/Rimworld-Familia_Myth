using System.Collections.Generic;
using FamiliaMyth.Familia;
using FamiliaMyth.Quest;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace Familia_Myth.Source
{
    public class QuestNode_Root_RankUp : QuestNode
    {
        public SlateRef<FamiliaCategoryDef> category;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var cat = category.GetValue(slate);
            if (cat == null) return;

            //统一任务点数计算（强制任务内部自动 ×1.5）
            QuestUtilities.CalculateTaskParams(isRankUp: true, out float threatPoints, out float totalExploits);

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
            slate.Set("totalExploits", totalExploits);
            slate.Set("sitePartsParams", sitePartsParams);
            slate.Set("siteFaction", faction);
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
