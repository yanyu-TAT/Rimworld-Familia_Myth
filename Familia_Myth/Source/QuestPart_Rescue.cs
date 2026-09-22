using FamiliaMyth.Familia;
using FamiliaMyth.Pharna;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FamiliaMyth.Quest
{
    //紧急救援主状态机（过程 part，就地接送方案）：
    //  搜救阶段（威胁清除→快照参与者+解除站点 2 天表）→ 3~6 小时 → 召唤穿梭机就地降落
    //  → 降落后 3 天登机窗口（SentSatisfied 满载 / Unsatisfied 超时）→ 结算。
    //  威胁清除当拍：幸存者难民式临时加入玩家派系（SetFaction 玩家 + HomeFaction"原派系"标记），
    //  上头像栏、可指挥、角色卡显示"原派系"；同时释放守点 lord（守点周期到加入为止，之后不再发放）；
    //  结算时一律归还原派系——登机者随穿梭机返回；滞留者自行离图（LordJob_ExitMapBest），
    //  全程无人口白送（死亡/离图时 QuestPart_ExtraFaction 的原版行为也会自动归还原派系）。
    public class QuestPart_Rescue : QuestPartActivable
    {
        public enum Stage { WaitingThreatClear, WaitingShuttleArrive, WaitingBoarding, Done }

        public Site site;
        public Faction originalFaction;          //遇难者所属原派系（临时加入玩家后作 HomeFaction"原派系"标记；结算归还于此）
        public Pawn acceptor;                    //接取者（奖励规则用）
        public FamiliaCategoryDef category;      //分类（升级点 +10）
        public List<Pawn> victims = new();       //遇难者（威胁清除后临时加入玩家派系）
        public int totalCount;                   //生成总数（奖励分母）
        public float baseExploits;               //基础伟业值（× 登船存活/总数）
        public Thing shipThing;                  //穿梭机载具（登船判定）

        //搜救完成（威胁清除）时点快照的参与者名单，结算按此发放
        public List<Pawn> participants = new();

        //生成期 hardcode 的同串信号
        public string inSignalRescueDone;        //解除站点 2 天搜救超时表
        public string inSignalEvacuate;          //召唤穿梭机 util（就地降落）
        public string inSignalSatisfied;         //穿梭机满载起飞
        public string inSignalUnsatisfied;       //穿梭机超时未满起飞

        private Stage stage = Stage.WaitingThreatClear;
        private int evacuateTick = -1;           //威胁清除后 3~6 小时召唤穿梭机
        private const int CheckIntervalTicks = 250;
        private const int ShuttleDelayMinTicks = 7500;   //3 小时
        private const int ShuttleDelayMaxTicks = 15000;  //6 小时

        //Quest 仅对 Enabled（收到 site.AllEnemiesDefeated = 威胁清除）后调用
        public override void QuestPartTick()
        {
            if (stage == Stage.Done) return;
            if (Find.TickManager.TicksGame % CheckIntervalTicks != 0) return;

            //威胁清除：快照参与者、解除站点超时、幸存者临时加入玩家派系（释放守点 lord）、
            //安排穿梭机 3~6 小时后就地降落
            if (stage == Stage.WaitingThreatClear)
            {
                if (victims.All(p => p == null || p.Dead)) return;   //全灭由 Notify_PawnKilled 结算，不再召穿梭机

                SnapshotParticipants();
                stage = Stage.WaitingShuttleArrive;
                evacuateTick = Find.TickManager.TicksGame
                    + Rand.RangeInclusive(ShuttleDelayMinTicks, ShuttleDelayMaxTicks);

                JoinVictimsToPlayer();
                Find.SignalManager.SendSignal(new Signal(inSignalRescueDone));
                SendLetter("FamiliaMyth.QuestPart.RescueSearchComplete".Translate(),
                    "FamiliaMyth.QuestPart.RescueSearchCompleteText".Translate());
                return;
            }

            //3~6 小时倒计时到 → 召唤穿梭机（就地在站点降落）；守点 lord 已在加入玩家派系时释放
            if (stage == Stage.WaitingShuttleArrive && Find.TickManager.TicksGame >= evacuateTick)
            {
                stage = Stage.WaitingBoarding;
                Find.SignalManager.SendSignal(new Signal(inSignalEvacuate)); //util：降落+3 天登机
            }
        }

        //穿梭机起飞信号（满载 SentSatisfied / 3 天未满 SentUnsatisfied）
        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (signal.tag == inSignalSatisfied) { Settle(true); return; }
            if (signal.tag == inSignalUnsatisfied) { Settle(false); return; }
            base.Notify_QuestSignalReceived(signal);   //Activable：处理 inSignalEnable(site.AllEnemiesDefeated) 等
        }

        //任意 pawn 被杀都会被广播到这里（QuestManager），仅关注遇难者
        public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
        {
            if (!victims.Contains(pawn) || stage == Stage.Done) return;
            if (victims.All(p => p == null || p.Dead))
            {
                //全灭：直接失败（尸体派系由 QuestPart_ExtraFaction 的原版行为自动归还原派系）
                stage = Stage.Done;
                SendLetter("FamiliaMyth.QuestPart.RescueFailed".Translate(),
                    "FamiliaMyth.QuestPart.RescueFailedAllDeadText".Translate());
                quest.End(QuestEndOutcome.Fail);
            }
        }

        private void SnapshotParticipants()
        {
            if (participants.Count > 0) return;
            QuestUtilities.SnapshotSiteParticipants(site, participants);
        }

        //威胁清除当拍：幸存者难民式加入玩家派系（真实加入 → 上头像栏/可指挥），
        //同时释放守点 lord（守点周期到加入为止，之后不再发放）
        private void JoinVictimsToPlayer()
        {
            foreach (var p in victims)
            {
                if (p == null || p.Destroyed || p.Dead) continue;
                if (p.GetLord() != null)
                    p.Map?.lordManager?.RemoveLord(p.GetLord());
                if (p.Faction != Faction.OfPlayer)
                    p.SetFaction(Faction.OfPlayer);
            }
        }

        //结算归还：所有幸存者恢复原派系（登机者随穿梭机返回；滞留者此后自行离图/由系统回收）
        private void RestoreOriginalFactions()
        {
            if (originalFaction == null) return;
            foreach (var p in victims)
            {
                if (p == null || p.Destroyed || p.Dead) continue;
                if (p.Faction != originalFaction)
                    p.SetFaction(originalFaction);
            }
        }

        private void Settle(bool satisfied)
        {
            if (stage == Stage.Done) return;

            if (satisfied)
            {
                //满载起飞：舱内伤员此时仍在内容器中，先归还原派系，随后由穿梭机带回其派系；任务成功
                stage = Stage.Done;
                if (participants.Count == 0) SnapshotParticipants();

                RestoreOriginalFactions();
                int boarded = CountBoarded();
                float ratio = totalCount > 0 ? (float)boarded / totalCount : 0f;

                SendLetter("FamiliaMyth.QuestPart.RescueComplete".Translate(),
                    "FamiliaMyth.QuestPart.RescueCompleteText".Translate(boarded, totalCount));
                QuestUtilities.DistributeExploits(participants, acceptor,
                    baseExploits * ratio,
                    "FamiliaMyth.QuestPart.QuestComplete".Translate(),
                    "FamiliaMyth.QuestPart.RewardReasonRescue".Translate());
                GameComponent_FamiliaRankTracker.Instance?.GainPoints(category, 10);
                quest.End(QuestEndOutcome.Success);
                return;
            }

            //失败（超时未满起飞）：已登机者仍随穿梭机返回；其余恢复原派系后自行离图
            SendLetter("FamiliaMyth.QuestPart.RescueFailed".Translate(),
                "FamiliaMyth.QuestPart.RescueFailedShuttleLeftText".Translate());

            RestoreOriginalFactions();
            MakeLeaveMap();

            stage = Stage.Done;
            quest.End(QuestEndOutcome.Fail);
        }

        private int CountBoarded()
        {
            int boarded = 0;
            var shipComp = shipThing?.TryGetComp<CompShuttle>();
            if (shipComp == null) return boarded;
            foreach (var p in victims.Where(p => p != null && !p.Dead))
                if (shipComp.Transporter.innerContainer.Contains(p))
                    boarded++;
            return boarded;
        }

        //为滞留者安排走出地图（离开后由系统按世界 pawn 处理）
        private void MakeLeaveMap()
        {
            var pawns = victims.Where(p => p?.Map != null && p.Spawned && !p.DeadOrDowned).ToList();
            if (pawns.NullOrEmpty()) return;

            var lord = LordMaker.MakeNewLord(pawns.First().Faction, new LordJob_ExitMapBest(), pawns.First().Map);
            lord.AddPawns(pawns);
        }

        private void SendLetter(string label, string text) =>
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent));

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref site, "site");
            Scribe_References.Look(ref originalFaction, "originalFaction");
            Scribe_References.Look(ref acceptor, "acceptor");
            Scribe_Defs.Look(ref category, "category");
            Scribe_Collections.Look(ref victims, "victims", LookMode.Reference);
            Scribe_Values.Look(ref totalCount, "totalCount");
            Scribe_Values.Look(ref baseExploits, "baseExploits");
            Scribe_References.Look(ref shipThing, "shipThing");
            Scribe_Collections.Look(ref participants, "participants", LookMode.Reference);
            Scribe_Values.Look(ref inSignalRescueDone, "inSignalRescueDone");
            Scribe_Values.Look(ref inSignalEvacuate, "inSignalEvacuate");
            Scribe_Values.Look(ref inSignalSatisfied, "inSignalSatisfied");
            Scribe_Values.Look(ref inSignalUnsatisfied, "inSignalUnsatisfied");
            Scribe_Values.Look(ref stage, "stage");
            Scribe_Values.Look(ref evacuateTick, "evacuateTick", -1);
        }
    }
}
