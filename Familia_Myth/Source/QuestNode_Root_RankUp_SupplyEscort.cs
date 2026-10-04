using FamiliaMyth.Familia;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //强制任务「物资押运」根节点：统一点数 → 选取 A/B（已有定居点）→ 写入 slate
    //A/B 选取失败属异常路径（Available 已预筛）：打红字并且不发布任务
    public class QuestNode_Root_RankUp_SupplyEscort : QuestNode
    {
        //伏击点数相对任务点数的系数（供调参）
        public const float AmbushPointsFactor = 0.8f;

        public SlateRef<FamiliaCategoryDef> category;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;

            QuestUtilities.CalculateTaskParams(isRankUp: true, out float points);

            var homeMap = ResolveHomeMap(slate);
            if (homeMap == null) return;

            if (!SupplyEscortUtility.TryFindSettlementPair(homeMap, out var settlementA, out var settlementB))
                return;   //红字已在 TestRunInt 打出

            slate.Set("settlementA", settlementA);
            slate.Set("settlementB", settlementB);
            slate.Set("difficulty", points);
            slate.Set("ambushPoints", points * AmbushPointsFactor);
            slate.Set("category", category.GetValue(slate));
        }

        protected override bool TestRunInt(Slate slate)
        {
            var homeMap = ResolveHomeMap(slate);
            if (homeMap == null)
            {
                Log.Error("[FamiliaMyth] 物资押运：无法确定玩家据点，任务未发布。");
                return false;
            }

            if (!SupplyEscortUtility.TryFindSettlementPair(homeMap, out _, out _))
            {
                Log.Error("[FamiliaMyth] 物资押运：世界中找不到满足「中立/友好 + 与据点三边均 ≤ "
                    + SupplyEscortUtility.MaxDistanceToHome + " 格」的定居点组合，任务未发布。");
                return false;
            }
            return true;
        }

        private static Map ResolveHomeMap(Slate slate) =>
            slate.Get<Map>("map") ?? Find.AnyPlayerHomeMap ?? Find.CurrentMap;
    }
}
