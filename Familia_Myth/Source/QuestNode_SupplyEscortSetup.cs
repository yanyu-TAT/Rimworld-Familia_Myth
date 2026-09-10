using FamiliaMyth.Familia;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //「物资押运」装配部件：读 slate 的 A/B 定居点，装配主状态机与伏击部件
    public class QuestNode_SupplyEscortSetup : QuestNode
    {
        protected override void RunInt()
        {
            var slate = QuestGen.slate;

            var settlementA = slate.Get<Settlement>("settlementA");
            var settlementB = slate.Get<Settlement>("settlementB");
            if (settlementA == null || settlementB == null) return;

            var category = slate.Get<FamiliaCategoryDef>("category");
            var acceptor = slate.Get<Pawn>("acceptor");
            float baseExploits = slate.Get<float>("totalExploits");
            float ambushPoints = slate.Get<float>("ambushPoints");

            string signalPickedUp = QuestGenUtility.HardcodedSignalWithQuestID("escortPickedUp");

            //主状态机：任务启动即启用（接取起算 12 天）
            QuestGen.quest.AddPart(new QuestPart_SupplyEscort
            {
                settlementA = settlementA,
                settlementB = settlementB,
                acceptor = acceptor,
                category = category,
                baseExploits = baseExploits,
                inSignalEnable = QuestGen.quest.InitiateSignal,
                inSignalPickup = QuestGenUtility.HardcodedSignalWithQuestID("escortPickup"),
                inSignalDeliver = QuestGenUtility.HardcodedSignalWithQuestID("escortDeliver"),
                inSignalPickedUp = signalPickedUp,
                inSignalTimeout = QuestGenUtility.HardcodedSignalWithQuestID("escortTimeout"),
                outSignalRewardsReady = QuestGenUtility.HardcodedSignalWithQuestID("rewardsReady")
            });

            //伏击轮询：同样任务启动即启用，领取物资当拍起算冷却
            QuestGen.quest.AddPart(new QuestPart_SupplyEscortAmbush
            {
                settlementA = settlementA,
                settlementB = settlementB,
                ambushPoints = ambushPoints,
                inSignalEnable = QuestGen.quest.InitiateSignal,
                inSignalPickedUp = signalPickedUp
            });
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
