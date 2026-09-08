using System.Collections.Generic;
using FamiliaMyth.Familia;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //装配部件：需在 Util_TransportShip_Pickup 之后执行（读取 slate 中的 pickupShipThing）
    public class QuestNode_RescueSetup : QuestNode
    {
        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var site = slate.Get<Site>("site");
            var victims = slate.Get<List<Pawn>>("victims");
            var acceptor = slate.Get<Pawn>("acceptor");
            var category = slate.Get<FamiliaCategoryDef>("category");
            int count = slate.Get<int>("rescueCount");
            float baseExploits = slate.Get<float>("totalExploits");
            var shipThing = slate.Get<Thing>("pickupShipThing");
            var original = slate.Get<Faction>("rescueFaction");
            if (site == null || victims == null) return;

            string threatClear = QuestGenUtility.HardcodedSignalWithQuestID("site.AllEnemiesDefeated");

            //遇难者进图放置（含守点防离图）
            QuestGen.quest.AddPart(new QuestPart_RescueSpawn
            {
                inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated"),
                site = site,
                victims = victims
            });

            //主状态机（威胁清除时难民式临时加入玩家派系 → 可进头像栏/被指挥；结算时归还原派系）
            QuestGen.quest.AddPart(new QuestPart_Rescue
            {
                site = site,
                acceptor = acceptor,
                category = category,
                victims = victims,
                originalFaction = original,
                totalCount = count,
                baseExploits = baseExploits,
                shipThing = shipThing,
                inSignalEnable = threatClear,
                inSignalRescueDone = QuestGenUtility.HardcodedSignalWithQuestID("rescueDone"),
                inSignalEvacuate = QuestGenUtility.HardcodedSignalWithQuestID("rescueEvacuate"),
                inSignalSatisfied = QuestGenUtility.HardcodedSignalWithQuestID("pickupShipThing.SentSatisfied"),
                inSignalUnsatisfied = QuestGenUtility.HardcodedSignalWithQuestID("pickupShipThing.SentUnsatisfied")
            });

            //"原派系"标记：受害者临时加入玩家派系期间带 HomeFaction 声明 → 角色卡显示原派系、
            //远行队等 lodger 限制自动生效（部件随任务 Ongoing 全程存在，quest.End 时撤销）
            if (original != null)
                QuestGen.quest.AddPart(new QuestPart_ExtraFaction
                {
                    extraFaction = new ExtraFaction(original, ExtraFactionType.HomeFaction),
                    affectedPawns = victims,
                    areHelpers = false
                });
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
