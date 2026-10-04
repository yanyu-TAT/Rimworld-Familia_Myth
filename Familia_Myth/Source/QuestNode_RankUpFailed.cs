using FamiliaMyth.Familia;
using RimWorld.QuestGen;

namespace FamiliaMyth.Quest
{
    public class QuestNode_RankUpFailed : QuestNode
    {
        //可选：显式指定监听信号；缺省使用外层节点传入的信号（如超时分支的内部信号）
        public SlateRef<string> inSignal;

        public SlateRef<FamiliaCategoryDef> categoryDef;

        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var cat = categoryDef.GetValue(slate);
            if (cat == null) return;

            var part = new QuestPart_RankUpFailed
            {
                inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal"),
                category = cat
            };
            QuestGen.quest.AddPart(part);
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
