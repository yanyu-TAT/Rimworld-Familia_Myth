using System.Collections.Generic;
using FamiliaMyth.Familia;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //「难民安置」装配节点：需排在 Util_TransportShip_Pickup 之后执行（读取 slate 的 pickupShipThing）。
    //装配顺序要点：ExtraFaction 必须先于到访部件加入——Pawn.SetFaction 里"非 lodger 才 ChangeKind"，
    //标记已在任务部件中，难民加入玩家派系时才不会被改成殖民者 kind。
    public class QuestNode_RefugeeResettleSetup : QuestNode
    {
        protected override void RunInt()
        {
            var slate = QuestGen.slate;
            var quest = QuestGen.quest;

            var victims = slate.Get<List<Pawn>>("victims");
            var refugeeFaction = slate.Get<Faction>("refugeeFaction");
            var raider = slate.Get<Faction>("raiderFaction");
            var map = slate.Get<Map>("map");
            var acceptor = slate.Get<Pawn>("acceptor");
            var category = slate.Get<FamiliaCategoryDef>("category");
            float baseExploits = slate.Get<float>("totalExploits");
            float raidPoints = slate.Get<float>("raidPoints");
            var shipThing = slate.Get<Thing>("pickupShipThing");
            if (victims.NullOrEmpty() || refugeeFaction == null || map == null) return;

            string initiate = quest.InitiateSignal;
            string victimDied = QuestGenUtility.HardcodedSignalWithQuestID("refugeeDied");
            string evacuate = QuestGenUtility.HardcodedSignalWithQuestID("refugeeEvacuate");
            string rewardsReady = QuestGenUtility.HardcodedSignalWithQuestID("rewardsReady");
            string failed = QuestGenUtility.HardcodedSignalWithQuestID("refugeeFailed");

            //「原派系」标记（对齐原版难民：MiniFaction）。
            //inSignalEnable 必须显式设置：QuestPartActivable 只在 Enabled 时处理移除信号，
            //原版所有 ExtraFaction 调用点均未设置 → 移除信号实际永不生效，这里显式启用。
            var extraFaction = new QuestPart_ExtraFaction
            {
                extraFaction = new ExtraFaction(refugeeFaction, ExtraFactionType.MiniFaction),
                affectedPawns = victims,
                areHelpers = false,
                inSignalEnable = initiate
            };
            quest.AddPart(extraFaction);

            //到访：地图边缘走入 + 当拍加入玩家派系（临时加入，带 MiniFaction 标记 = lodger）。
            //inSignal 必须显式传：根层 slate 没有 "inSignal" 变量，缺省取值会得到 null → 部件永不触发
            string arrivalLabel = "FamiliaMyth.QuestMisc.RefugeeResettleArrivalLetterLabel".Translate();
            string arrivalText = "FamiliaMyth.QuestMisc.RefugeeResettleArrivalLetterText".Translate();
            quest.PawnsArrive(victims, inSignal: initiate, mapParent: map.Parent, joinPlayer: true,
                customLetterLabel: arrivalLabel,
                customLetterText: arrivalText);

            //装备锁定：难民是安置对象，不允许被剥取装备
            quest.SetAllApparelLocked(victims, inSignal: initiate);

            //目击同伴死亡的心情（OtherTravelerDied 未声明 doNotApplyToQuestLodgers，lodger 同样受影响）
            quest.AddMemoryThought(victims, ThoughtDefOf.OtherTravelerDied, victimDied);

            //有害手术惩罚（★3）：信号由 RecipeWorker.ReportViolation 从被手术者身上广播，
            //同串同时完成 questTag 登记（生成结束时把 slate 的 victims 名单打成 Quest{id}.victims）
            string surgerySignal = QuestGenUtility.HardcodedSignalWithQuestID("victims.SurgeryViolation");
            if (ThoughtDefOf.OtherTravelerSurgicallyViolated != null)
                quest.AddMemoryThought(victims, ThoughtDefOf.OtherTravelerSurgicallyViolated, "victims.SurgeryViolation");

            //劝加入（对齐原版难民）：心情≥50% 时按 MTB 发抉择信，接受即转正并退出登机名单
            var joinSignals = new List<string>();
            foreach (var pawn in victims)
            {
                string offerLabel = "FamiliaMyth.QuestMisc.RefugeeJoinOfferLetterLabel".Translate();
                string offerText = "FamiliaMyth.QuestMisc.RefugeeJoinOfferLetterText".Translate();
                string offerDesc = "FamiliaMyth.QuestMisc.RefugeeJoinOfferDesc".Translate(pawn.Name.ToStringShort);
                var offer = quest.PawnJoinOffer(pawn,
                    offerLabel,
                    offerText,
                    offerDesc,
                    inSignal: initiate,
                    charity: true);
                if (!offer.outSignalPawnAccepted.NullOrEmpty())
                    joinSignals.Add(offer.outSignalPawnAccepted);
            }
            if (joinSignals.Count > 0)
                extraFaction.inSignalsRemovePawn = joinSignals;

            //主状态机（停留期袭击 / 15 天计时 / 结算 / 失败线）
            quest.AddPart(new QuestPart_RefugeeResettle
            {
                victims = victims,
                refugees = new List<Pawn>(victims),
                acceptor = acceptor,
                category = category,
                refugeeFaction = refugeeFaction,
                raiderFaction = raider,
                mapParent = map.Parent,
                shipThing = shipThing,
                baseExploits = baseExploits,
                raidPoints = raidPoints,
                totalCount = victims.Count,
                inSignalEnable = initiate,
                inSignalsJoinAccepted = joinSignals,
                inSignalEvacuate = evacuate,
                inSignalSatisfied = QuestGenUtility.HardcodedSignalWithQuestID("pickupShipThing.SentSatisfied"),
                inSignalUnsatisfied = QuestGenUtility.HardcodedSignalWithQuestID("pickupShipThing.SentUnsatisfied"),
                inSignalSurgeryViolation = surgerySignal,
                outSignalVictimDied = victimDied,
                outSignalRewardsReady = rewardsReady,
                outSignalFailed = failed
            });

            //延迟回报子任务（★2）：概率在生成期掷定，绑定结算信号；
            //用 Safe 版做候选预检（原版在找不到 rewardGiver 时会生成出文本无法解析的子任务）
            if (Rand.Chance(QuestNode_Root_RankUp_RefugeeResettle.RewardSubQuestChance))
            {
                float value = victims.Count * QuestPart_RefugeeResettle.StayDays
                    * QuestNode_Root_RankUp_RefugeeResettle.RewardValuePerRefugeeDay
                    * Find.Storyteller.difficulty.EffectiveQuestRewardValueFactor;
                quest.AddPart(new QuestPart_AddQuestRefugeeDelayedRewardSafe
                {
                    inSignal = rewardsReady,
                    acceptee = acceptor,
                    parent = quest,
                    lodgers = new List<Pawn>(victims),
                    faction = refugeeFaction,
                    marketValueRange = new FloatRange(0.7f, 1.3f) * value
                });
            }
        }

        protected override bool TestRunInt(Slate slate) => true;
    }
}
