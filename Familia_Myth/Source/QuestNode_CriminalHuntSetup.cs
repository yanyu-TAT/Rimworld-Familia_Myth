using FamiliaMyth.Familia;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //装配部件：需在 Util_TransportShip_Pickup 之后执行（读取 slate 中的 pickupShipThing）
    public class QuestNode_CriminalHuntSetup : QuestNode
    {
        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var site = slate.Get<Site>("site");
            var wanted = slate.Get<Pawn>("wanted");
            var category = slate.Get<FamiliaCategoryDef>("category");
            var shipThing = slate.Get<Thing>("pickupShipThing");
            if (site == null || wanted == null) return;

            //通缉犯进图放置（营地中心，与守卫同场）
            QuestGen.quest.AddPart(new QuestPart_CriminalHuntSpawn
            {
                inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated"),
                site = site,
                wanted = wanted
            });

            //主状态机：威胁清除分流 → 离图计时 → 押解窗口 → 结算
            QuestGen.quest.AddPart(new QuestPart_CriminalHunt
            {
                site = site,
                wanted = wanted,
                category = category,
                shipThing = shipThing,
                inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID("site.AllEnemiesDefeated"),
                inSignalCleared = QuestGenUtility.HardcodedSignalWithQuestID("huntCleared"),
                inSignalExtradite = QuestGenUtility.HardcodedSignalWithQuestID("huntExtradite"),
                inSignalSatisfied = QuestGenUtility.HardcodedSignalWithQuestID("pickupShipThing.SentSatisfied"),
                inSignalUnsatisfied = QuestGenUtility.HardcodedSignalWithQuestID("pickupShipThing.SentUnsatisfied")
            });
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
