using System.Collections.Generic;
using System.Linq;
using FamiliaMyth.Familia;
using FamiliaMyth.Pharna;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FamiliaMyth.Quest
{
    //罪犯追捕主状态机（过程 part）：
    //  清剿阶段（威胁清除→快照参与者+解除站点超时表）→ 通缉犯死亡→普通结算；
    //  存活→引导带回（玩家将其带上远行队离开窝点图）→ 离图起 1~3 天计时 →
    //  抵达玩家据点后召唤公会押解穿梭机（2 天登机窗口，required 上船即飞）→
    //  SentSatisfied（活着通缉犯登机起飞）→ 双倍结算（囚犯 despawn discard，不改派系）；
    //  SentUnsatisfied（窗口满仍存活未登机）→ 失败。
    //  死亡（任意时点、任意原因）→ 普通成功结算（降落后死亡 → quest.End 令穿梭机 cleanup 直接离开）。
    //  招募/奴役（脱离押送）与活着失踪（不在窝点图/玩家远行队/玩家据点）→ 失败。
    public class QuestPart_CriminalHunt : QuestPartActivable
    {
        public enum Stage { WaitingThreatClear, Escorting, WaitingExtradition, Boarding, Done }

        public Site site;                    //罪犯窝点
        public Pawn wanted;                  //通缉犯（唯一目标）
        public Pawn acceptor;                //接取者（奖励规则用）
        public FamiliaCategoryDef category;  //分类（升级点）
        public float baseExploits;           //基础伟业值（押解线 ×2）
        public Thing shipThing;              //押解穿梭机（登机判定）

        //威胁清除时点快照的参与者名单，结算按此发放
        public List<Pawn> participants = new();

        //生成期 hardcode 的同串信号
        public string inSignalCleared;       //解除站点清剿超时表
        public string inSignalExtradite;     //召唤押解穿梭机 util
        public string inSignalSatisfied;     //穿梭机载着通缉犯起飞
        public string inSignalUnsatisfied;   //窗口期满，通缉犯未登机起飞

        public const int CheckIntervalTicks = 250;
        public const int ExtraditionDelayMinTicks = 60000;    //1 天
        public const int ExtraditionDelayMaxTicks = 180000;   //3 天

        //任务追踪：始终指向通缉犯（进图后可点击跳转，任务进行期间随时识别目标）
        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get
            {
                foreach (var target in base.QuestLookTargets)
                    yield return target;
                if (wanted != null && !wanted.Destroyed)
                    yield return wanted;
            }
        }

        private Stage stage = Stage.WaitingThreatClear;
        private int leaveAtTick = -1;        //离图后穿梭机抵达时刻（1~3 天）
        private bool shipCalled;             //已发 huntExtradite

        //Quest 仅对 Enabled（收到 site.AllEnemiesDefeated = 威胁清除）后调用
        public override void QuestPartTick()
        {
            if (stage == Stage.Done) return;
            if (Find.TickManager.TicksGame % CheckIntervalTicks != 0) return;

            //威胁清除：快照参与者、停站点超时表、按通缉犯存活分流
            if (stage == Stage.WaitingThreatClear)
            {
                SnapshotParticipants();
                if (wanted == null || wanted.Dead || wanted.Destroyed)
                {
                    SettleNormal("FamiliaMyth.QuestPart.CriminalKilledInOutpost".Translate(),
                        "FamiliaMyth.QuestPart.CriminalKilledInOutpostText".Translate());
                    return;
                }

                stage = Stage.Escorting;
                Find.SignalManager.SendSignal(new Signal(inSignalCleared));
                SendLetter("FamiliaMyth.QuestPart.CriminalSubdued".Translate(),
                    "FamiliaMyth.QuestPart.CriminalSubduedText".Translate());
                return;
            }

            //后续阶段共用监控：死亡→普通结算；招募/奴役或活着失踪→失败
            if (!HandleWantedLoss())
                return;

            //玩家已带离窝点 → 启动 1~3 天穿梭机抵达计时
            //（扛在殖民者身上不算离图：以 wanted 或搬运者所在地图为准）
            if (stage == Stage.Escorting && leaveAtTick < 0
                && (site?.Map == null || WantedLocationMap() != site.Map))
            {
                //离图当拍收押：玩家将其打包带走即视为归案（原版通常已自动视为囚犯，此处兜底）
                if (!wanted.IsPrisonerOfColony)
                    wanted.guest?.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);

                leaveAtTick = Find.TickManager.TicksGame
                    + Rand.RangeInclusive(ExtraditionDelayMinTicks, ExtraditionDelayMaxTicks);
                stage = Stage.WaitingExtradition;
                return;
            }

            //计时到期 且 通缉犯已押至玩家据点（家图 + 玩家囚犯）→ 召唤公会押解穿梭机
            if (stage == Stage.WaitingExtradition && !shipCalled
                && Find.TickManager.TicksGame >= leaveAtTick
                && wanted.Spawned && wanted.Map.IsPlayerHome && wanted.IsPrisonerOfColony)
            {
                shipCalled = true;
                stage = Stage.Boarding;
                Find.SignalManager.SendSignal(new Signal(inSignalExtradite));
            }
        }

        //穿梭机起飞信号（载犯满载 SentSatisfied / 2 天窗口未载 SentUnsatisfied）
        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (signal.tag == inSignalSatisfied) { SettleDouble(); return; }
            if (signal.tag == inSignalUnsatisfied)
            {
                SettleFail("FamiliaMyth.QuestPart.ExtraditionFailed".Translate(),
                    "FamiliaMyth.QuestPart.ExtraditionFailedText".Translate());
                return;
            }
            base.Notify_QuestSignalReceived(signal);
        }

        //通缉犯被杀广播（威胁清除后才立即结算；清剿完成前的死亡由分流统一处理，防「只杀通缉犯即成功」）
        public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
        {
            if (stage == Stage.Done || pawn != wanted || stage == Stage.WaitingThreatClear) return;
            SettleNormal("FamiliaMyth.QuestPart.CriminalDied".Translate(),
                "FamiliaMyth.QuestPart.CriminalDiedText".Translate());
        }

        //监控返回值：false = 已结算/应停止本阶段推进
        private bool HandleWantedLoss()
        {
            if (wanted == null || wanted.Destroyed || wanted.Dead)
            {
                SettleNormal("FamiliaMyth.QuestPart.CriminalDied".Translate(),
                    "FamiliaMyth.QuestPart.CriminalDiedText".Translate());
                return false;
            }

            //招募/奴役：脱离囚犯押送，视为失败
            if (wanted.Faction == Faction.OfPlayer || wanted.IsSlave)
            {
                SettleFail("FamiliaMyth.QuestPart.CriminalClaimed".Translate(),
                    "FamiliaMyth.QuestPart.CriminalClaimedText".Translate());
                return false;
            }

            //活着失踪：不在窝点图 / 玩家远行队 / 玩家据点（含被丢弃、留原地离图）
            if (!InLegalHold())
            {
                SettleFail("FamiliaMyth.QuestPart.CriminalMissing".Translate(),
                    "FamiliaMyth.QuestPart.CriminalMissingText".Translate());
                return false;
            }
            return true;
        }

        //押送合法性判定：
        //  - 显式位置（窝点图 / 家图+玩家囚犯 / 玩家远行队 / 被玩家成员扛起）→ 合法；
        //  - 位于任意运输载体容器中（空投舱 / 玩家或任务穿梭机 / gravship 等：装载与飞行期间
        //    pawn 会离开地图且不世界化）→ 押送「待定」，不判死；落定（spawn/世界化）后按位置再判；
        //  - 无主世界 pawn（被丢弃、地图卸载未带走）→ 失踪。
        private bool InLegalHold()
        {
            if (wanted == null || wanted.Destroyed || wanted.Dead) return false;

            //被扛着：搬运者须在场且为玩家派系
            Pawn carrier = wanted.CarriedBy;
            if (carrier != null)
                return carrier.Spawned && !carrier.Dead && carrier.Faction == Faction.OfPlayer;

            if (!wanted.Spawned)
            {
                if (wanted.IsWorldPawn())
                {
                    var caravan = CaravanUtility.GetCaravan(wanted);
                    if (caravan != null && caravan.Faction == Faction.OfPlayer) return true;
                    return false;
                }
                //非世界 pawn 且未 spawn：位于某运输容器中（空投舱/穿梭机/载具），押送待定
                return true;
            }

            //仍在窝点地图
            if (site?.Map != null && wanted.Map == site.Map) return true;

            //已押至玩家据点：须保持玩家囚犯身份（释放/非囚犯 = 失去押送）
            if (wanted.Map.IsPlayerHome && wanted.IsPrisonerOfColony) return true;

            return false;
        }

        //wanted 当前实质所在地图（含被扛起的情形）；世界/容器中返回 null
        private Map WantedLocationMap()
        {
            if (wanted == null || wanted.Destroyed) return null;
            if (wanted.Spawned) return wanted.Map;
            if (wanted.CarriedBy is Pawn carrier && carrier.Spawned) return carrier.Map;
            return null;
        }

        private void SnapshotParticipants()
        {
            if (participants.Count > 0) return;
            QuestUtilities.SnapshotSiteParticipants(site, participants);
        }

        //成功押解（×2）：通缉犯在机舱内随船起飞 → despawn discard（不改派系），双倍结算
        private void SettleDouble()
        {
            if (stage == Stage.Done) return;
            stage = Stage.Done;

            RemoveWantedFromWorld();
            SendLetter("FamiliaMyth.QuestPart.ExtraditionComplete".Translate(),
                "FamiliaMyth.QuestPart.ExtraditionCompleteText".Translate());
            QuestUtilities.DistributeExploits(participants, acceptor, baseExploits * 2f,
                "FamiliaMyth.QuestPart.QuestCompleteDoubleReward".Translate(),
                "FamiliaMyth.QuestPart.RewardReasonExtradition".Translate());
            GameComponent_FamiliaRankTracker.Instance?.GainPoints(category, 20);
            quest.End(QuestEndOutcome.Success);
        }

        //普通结算：通缉犯死亡线（窝点击毙 / 押解途中与窗口期死亡）
        private void SettleNormal(string label, string text)
        {
            if (stage == Stage.Done) return;
            stage = Stage.Done;

            SendLetter(label, text);
            QuestUtilities.DistributeExploits(participants, acceptor, baseExploits,
                "FamiliaMyth.QuestPart.QuestComplete".Translate(),
                "FamiliaMyth.QuestPart.RewardReasonOutstanding".Translate());
            GameComponent_FamiliaRankTracker.Instance?.GainPoints(category, 10);
            quest.End(QuestEndOutcome.Success);
        }

        private void SettleFail(string label, string text)
        {
            if (stage == Stage.Done) return;
            stage = Stage.Done;

            SendLetter(label, text);
            quest.End(QuestEndOutcome.Fail);
        }

        //将通缉犯从世界移除（仅押解成功路径）：先出穿梭机容器再销毁，避免滞留为世界 pawn
        private void RemoveWantedFromWorld()
        {
            if (wanted == null || wanted.Destroyed) return;

            if (shipThing != null && !shipThing.Destroyed
                && shipThing.TryGetComp<CompShuttle>() is CompShuttle comp)
            {
                var container = comp.Transporter.innerContainer;
                if (container.Contains(wanted))
                    container.Take(wanted);
            }
            if (wanted.IsWorldPawn())
                Find.WorldPawns.RemovePawn(wanted);
            else
                wanted.Destroy(DestroyMode.Vanish);
        }

        private void SendLetter(string label, string text) =>
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent));

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref site, "site");
            Scribe_References.Look(ref wanted, "wanted");
            Scribe_References.Look(ref acceptor, "acceptor");
            Scribe_Defs.Look(ref category, "category");
            Scribe_Values.Look(ref baseExploits, "baseExploits");
            Scribe_References.Look(ref shipThing, "shipThing");
            Scribe_Collections.Look(ref participants, "participants", LookMode.Reference);
            Scribe_Values.Look(ref inSignalCleared, "inSignalCleared");
            Scribe_Values.Look(ref inSignalExtradite, "inSignalExtradite");
            Scribe_Values.Look(ref inSignalSatisfied, "inSignalSatisfied");
            Scribe_Values.Look(ref inSignalUnsatisfied, "inSignalUnsatisfied");
            Scribe_Values.Look(ref stage, "stage");
            Scribe_Values.Look(ref leaveAtTick, "leaveAtTick", -1);
            Scribe_Values.Look(ref shipCalled, "shipCalled");
        }
    }
}
