using FamiliaMyth.Familia;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //「据点清剿」奖励节点：生成 QuestPart_SiteClearanceReward（结算时按分类发放升级点数）
    public class QuestNode_SiteClearanceReward : QuestNode
    {
        //升级点数归属分类与数量（省略 → 不发点数）
        public SlateRef<FamiliaCategoryDef> category;
        public SlateRef<int> upgradePoints;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var cat = category.GetValue(slate);
            var points = upgradePoints.GetValue(slate);
            if (cat == null || points <= 0) return;

            var part = new QuestPart_SiteClearanceReward
            {
                inSignal = QuestGenUtility.HardcodedSignalWithQuestID("rewardsReady") ?? slate.Get<string>("inSignal"),
                category = cat,
                upgradePoints = points
            };
            QuestGen.quest.AddPart(part);
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
