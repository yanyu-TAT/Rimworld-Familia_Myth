using FamiliaMyth.Familia;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //「据点清剿」奖励节点：生成 QuestPart_SiteClearanceReward
    //日常传 acceptor（50%+均分）；强制不传 acceptor（全员均分）；letterLabel 控制结算信标题
    public class QuestNode_SiteClearanceReward : QuestNode
    {
        //任务站点（快照参与者名单的来源，威胁清除时点记录）
        public SlateRef<MapParent> site;
        //接取者（强制任务可省略 → 全员均分）
        public SlateRef<Pawn> acceptor;
        //发放的伟业值总量（由统一点数计算写入 slate）
        public SlateRef<float> totalExploits;
        //结算信标题（空 → "委托完成"）
        public SlateRef<string> letterLabel;
        //升级点数归属分类与数量（省略 → 不发点数）
        public SlateRef<FamiliaCategoryDef> category;
        public SlateRef<int> upgradePoints;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var s = site.GetValue(slate);
            var total = totalExploits.GetValue(slate);
            var cat = category.GetValue(slate);
            var points = upgradePoints.GetValue(slate);
            if (s == null) return;
            if (total <= 0f && (cat == null || points <= 0)) return;

            var part = new QuestPart_SiteClearanceReward
            {
                inSignal = QuestGenUtility.HardcodedSignalWithQuestID("rewardsReady") ?? slate.Get<string>("inSignal"),
                snapshotSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.AllEnemiesDefeated"),
                site = s,
                acceptor = acceptor.GetValue(slate),
                totalExploits = total,
                letterLabel = letterLabel.GetValue(slate),
                category = cat,
                upgradePoints = points
            };
            QuestGen.quest.AddPart(part);
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
