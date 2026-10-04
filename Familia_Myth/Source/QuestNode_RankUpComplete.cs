using FamiliaMyth.Familia;
using RimWorld.QuestGen;

namespace FamiliaMyth.Quest
{
    public class QuestNode_RankUpComplete : QuestNode
    {
        public SlateRef<FamiliaCategoryDef> categoryDef;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var cat = categoryDef.GetValue(slate);
            if (cat == null) return;

            var part = new QuestPart_RankUpComplete
            {
                inSignal = QuestGenUtility.HardcodedSignalWithQuestID("rewardsReady") ?? slate.Get<string>("inSignal"),
                category = cat
            };
            QuestGen.quest.AddPart(part);
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
