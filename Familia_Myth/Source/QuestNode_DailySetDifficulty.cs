using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //日常任务难度与奖励：统一任务点数计算（准则5，非强制）
    public class QuestNode_DailySetDifficulty : QuestNode
    {
        public SlateRef<string> storeDifficultyAs;
        public SlateRef<string> storeExploitsAs;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;

            //统一计算：威胁点数 + 伟业值（点数已含原版财富基准 × 全殖民地恩惠成长）
            QuestUtilities.CalculateTaskParams(isRankUp: false, out float threatPoints, out float totalExploits);

            //构建站点部件参数（威胁点直接供原版生成敌人）
            var outpostDef = DefDatabase<SitePartDef>.GetNamed("Outpost");
            if (outpostDef != null)
            {
                var sitePartsParams = new List<SitePartDefWithParams>
                {
                    new(outpostDef, new SitePartParams { threatPoints = threatPoints })
                };
                slate.Set("sitePartsParams", sitePartsParams);
            }

            //存储结果到Slate
            string difficultyVarName = storeDifficultyAs.GetValue(slate) ?? "difficulty";
            string exploitsVarName = storeExploitsAs.GetValue(slate) ?? "totalExploits";
            slate.Set(difficultyVarName, threatPoints);
            slate.Set(exploitsVarName, totalExploits);
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
