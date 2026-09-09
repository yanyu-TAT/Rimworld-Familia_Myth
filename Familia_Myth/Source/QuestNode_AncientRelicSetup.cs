using FamiliaMyth.Familia;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Quest
{
    //装配部件：须在穿梭机 util（QuestNode_Signal relicShuttleCall）之后执行，
    //读取 slate 中的 pickupShipThing；装配主状态机与守家袭击部件（QuestPart_RandomRaid）。
    public class QuestNode_AncientRelicSetup : QuestNode
    {
        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var site = slate.Get<Site>("site");
            var acceptor = slate.Get<Pawn>("acceptor");
            var category = slate.Get<FamiliaCategoryDef>("category");
            float baseExploits = slate.Get<float>("totalExploits");
            float difficulty = slate.Get<float>("difficulty");
            var raidFaction = slate.Get<Faction>("siteFaction");
            var shipThing = slate.Get<Thing>("pickupShipThing");
            if (site == null)
                return;

            //主状态机：进图落位机关箱 → 开箱发现 → 双计时 → 交付/失败
            var state = new QuestPart_AncientRelic
            {
                site = site,
                acceptor = acceptor,
                category = category,
                baseExploits = baseExploits,
                shipThing = shipThing,
                inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated"),
                inSignalFound = QuestGenUtility.HardcodedSignalWithQuestID(QuestNode_Root_Daily_AncientRelic.CrateOpenedSignal),
                inSignalRaid = QuestGenUtility.HardcodedSignalWithQuestID(QuestNode_Root_Daily_AncientRelic.RaidArriveSignal),
                inSignalShuttle = QuestGenUtility.HardcodedSignalWithQuestID(QuestNode_Root_Daily_AncientRelic.ShuttleCallSignal),
                inSignalSatisfied = QuestGenUtility.HardcodedSignalWithQuestID("pickupShipThing.SentSatisfied"),
                inSignalUnsatisfied = QuestGenUtility.HardcodedSignalWithQuestID("pickupShipThing.SentUnsatisfied")
            };
            QuestGen.quest.AddPart(state);

            //0.5× 守家袭击（发现后 1~3 天由状态机发 relicRaidArrive 触发）
            Map homeMap = slate.Get<Map>("map");
            if (raidFaction != null && homeMap?.Parent != null)
            {
                float raidPoints = difficulty * QuestPart_AncientRelic.RaidPointsFactor;
                QuestGen.quest.AddPart(new QuestPart_RandomRaid
                {
                    inSignal = QuestGenUtility.HardcodedSignalWithQuestID(QuestNode_Root_Daily_AncientRelic.RaidArriveSignal),
                    mapParent = homeMap.Parent,
                    pointsRange = new FloatRange(raidPoints, raidPoints),
                    faction = raidFaction,
                    fallbackToPlayerHomeMap = true
                });
            }
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
