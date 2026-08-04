using FamiliaMyth.Familia;
using RimWorld.QuestGen;

namespace FamiliaMyth.Quest
{
    public class QuestNode_GainUpgradePoints : QuestNode
    {
        public SlateRef<FamiliaCategoryDef> category;
        public SlateRef<int> amount;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var cat = category.GetValue(slate);
            var amt= amount.GetValue(slate);
            if (cat == null || amt <= 0) return;

            var part = new QuestPart_GainUpgradePoints()
            {
                inSignal = QuestGenUtility.HardcodedSignalWithQuestID("rewardsReady") ?? slate.Get<string>("inSignal"),
                category = cat,
                amount = amt
            };

            QuestGen.quest.AddPart(part);
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
