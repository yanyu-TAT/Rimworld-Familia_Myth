using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //日常任务「据点清剿」根节点：统一任务点数计算（非强制）并生成站点参数
    public class QuestNode_Root_Daily_SiteClearance : QuestNode
    {
        protected override void RunInt()
        {
            var slate = QuestGen.slate;

            //统一计算：威胁点数 + 伟业值（点数已含原版财富基准 × 全殖民地恩惠成长）
            QuestUtilities.CalculateTaskParams(isRankUp: false, out float threatPoints, out float totalExploits);

            //生成站点参数（威胁点直接供原版生成敌人）
            var outpostDef = DefDatabase<SitePartDef>.GetNamed("Outpost");
            if (outpostDef == null) return;
            var sitePartsParams = new List<SitePartDefWithParams>
            {
                new(outpostDef, new SitePartParams { threatPoints = threatPoints })
            };

            //随机敌对派系
            var enemy = Find.FactionManager.RandomEnemyFaction(
                allowHidden: false, allowNonHumanlike: true, allowDefeated: false);

            slate.Set("difficulty", threatPoints);
            slate.Set("totalExploits", totalExploits);
            slate.Set("sitePartsParams", sitePartsParams);
            slate.Set("siteFaction", enemy);
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
