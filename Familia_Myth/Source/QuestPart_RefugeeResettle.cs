using FamiliaMyth.Familia;
using FamiliaMyth.Pharna;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Quest
{
    //「难民安置」主状态机（过程 part，全程在玩家据点地图）：
    //  接取 → 难民边缘到访并临时加入玩家派系（PawnsArrive joinPlayer + MiniFaction 标记，由装配节点负责）
    //  → 停留 15 天：到场当拍施加随机受伤；按点数×1.2 发 2 次袭击（首发 3~5 天，之后间隔 3~5 天）
    //  → 到点叫公会穿梭机（util，停留 2 天）：登机者随船离场；未登机者视为失踪，转中立后自行离图
    //  → 结算：升级点数 = 基础 15 × 存活率（登机数 + 已转正数）/ 生成总数
    //  失败线（囚禁/奴役、二次有害手术、全灭、无人生还）：图上的存活难民（含已转正者）转中立后自行离场。
    public class QuestPart_RefugeeResettle : QuestPartActivable
    {
        public enum Stage { Active, Boarding, Done }

        //—— 生成期传入 ——
        public List<Pawn> victims = new();      //全部难民（奖励分母、死亡监控）
        public List<Pawn> refugees = new();     //仍需登机者（被劝加入者移出）
        public List<Pawn> adopted = new();      //已转正（视为存活）
        public FamiliaCategoryDef category;
        public Faction refugeeFaction;          //临时难民派系（离场时转为此派系＝中立）
        public Faction raiderFaction;           //生成期选定的袭击派系
        public MapParent mapParent;
        public Thing shipThing;                 //公会穿梭机（util 生成）
        public float raidPoints;
        public int totalCount;

        //—— 信号（生成期固化） ——
        public List<string> inSignalsJoinAccepted = new();
        public string inSignalEvacuate;
        public string inSignalSatisfied;
        public string inSignalUnsatisfied;
        public string inSignalSurgeryViolation;
        public string outSignalVictimDied;
        public string outSignalRewardsReady;
        public string outSignalFailed;

        //停留天数（与 XML 描述、util 的 leaveDelayTicks 对应）
        public const int StayDays = 15;
        public const int BoardingWindowDays = 2;

        private const int CheckIntervalTicks = 250;

        //基础升级点数（按存活比例结算）
        private const int BaseUpgradePoints = 15;

        //袭击排程
        private const int MaxRaids = 2;
        private const int RaidFirstDelayMinDays = 3;
        private const int RaidFirstDelayMaxDays = 5;
        private const int RaidIntervalMinDays = 3;
        private const int RaidIntervalMaxDays = 5;
        private const int RaidRetryDelayTicks = 60000;

        //随机受伤
        private const float InjuryChance = 0.7f;
        private const int InjuryCountMin = 1;
        private const int InjuryCountMax = 3;
        private const float InjurySeverityMin = 4f;
        private const float InjurySeverityMax = 12f;
        private const float DownedChance = 0.25f;

        //有害手术容忍次数（超过即失败）
        private const int SurgeryViolationLimit = 1;

        private Stage stage = Stage.Active;
        private bool injuriesApplied;
        private int raidsFired;
        private int nextRaidTick = -1;
        private int stayEndTick = -1;
        private int surgeryViolations;
        private bool shipCalled;

        //任务启动即启用（接取信号）：停留期与袭击表从任务接取起算
        protected override void Enable(SignalArgs receivedArgs)
        {
            base.Enable(receivedArgs);
            int now = Find.TickManager.TicksGame;
            if (nextRaidTick < 0)
                nextRaidTick = now
                    + Rand.RangeInclusive(RaidFirstDelayMinDays, RaidFirstDelayMaxDays) * GenDate.TicksPerDay;
            if (stayEndTick < 0)
                stayEndTick = now + StayDays * GenDate.TicksPerDay;
        }

        public override void QuestPartTick()
        {
            if (stage == Stage.Done) return;
            if (Find.TickManager.TicksGame % CheckIntervalTicks != 0) return;

            //到场当拍施加随机受伤（等待难民真正 spawn 后再动手）
            if (!injuriesApplied)
            {
                ApplyInitialInjuries();
                return;
            }

            if (!CheckCaptivity()) return;
            TickRaids();
            TickEvacuation();
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (stage != Stage.Done)
            {
                if (inSignalsJoinAccepted.Contains(signal.tag))
                {
                    if (signal.args.TryGetArg("SUBJECT", out Pawn recruited))
                        OnRefugeeAdopted(recruited);
                }
                else if (signal.tag == inSignalSurgeryViolation)
                {
                    if (signal.args.TryGetArg("SUBJECT", out Pawn victim) && victims.Contains(victim))
                        OnSurgeryViolation(victim);
                }
                else if (signal.tag == inSignalSatisfied || signal.tag == inSignalUnsatisfied)
                {
                    Settle();
                    return;
                }
            }
            base.Notify_QuestSignalReceived(signal);
        }

        //任意 pawn 被杀都会广播到这里（QuestManager），仅关注难民
        public override void Notify_PawnKilled(Pawn pawn, DamageInfo? dinfo)
        {
            if (stage == Stage.Done || pawn == null || !victims.Contains(pawn)) return;

            //目击同伴死亡（生成期的 AddMemoryThought 绑定本信号）
            if (!outSignalVictimDied.NullOrEmpty())
                Find.SignalManager.SendSignal(new Signal(outSignalVictimDied, pawn.Named("SUBJECT")));

            refugees.Remove(pawn);
            if (victims.All(p => p == null || p.Destroyed || p.Dead))
                SettleFail("FamiliaMyth.QuestPart.RefugeeResettleFailed".Translate(),
                    "FamiliaMyth.QuestPart.RefugeeResettleFailedAllDead".Translate());
        }

        //到场当拍：随机部位/随机强度的伤害，部分难民倒地（需玩家救治）
        private void ApplyInitialInjuries()
        {
            if (!victims.Any(p => p != null && p.Spawned)) return;   //尚未到场，下轮再试

            injuriesApplied = true;
            foreach (var pawn in victims)
            {
                if (pawn == null || pawn.Destroyed || pawn.Dead || !pawn.Spawned) continue;

                if (Rand.Chance(InjuryChance))
                {
                    int times = Rand.RangeInclusive(InjuryCountMin, InjuryCountMax);
                    for (int i = 0; i < times; i++)
                    {
                        if (pawn.Dead) break;
                        //不指定部位 → 由引擎随机选部位
                        pawn.TakeDamage(new DamageInfo(HealthUtility.RandomViolenceDamageType(),
                            Rand.Range(InjurySeverityMin, InjurySeverityMax)));
                    }
                }

                if (!pawn.Dead && !pawn.Downed && Rand.Chance(DownedChance))
                    HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false);
            }
        }

        //囚禁/奴役 → 直接失败（玩家可以逮捕自己的 lodger，此路径真实可达）
        private bool CheckCaptivity()
        {
            foreach (var pawn in refugees)
            {
                if (pawn == null || pawn.Destroyed || pawn.Dead) continue;
                if (pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony)
                {
                    SettleFail("FamiliaMyth.QuestPart.CommissionRevoked".Translate(),
                        "FamiliaMyth.QuestPart.CommissionRevokedCaptivity".Translate());
                    return false;
                }
            }
            return true;
        }

        //停留期袭击：点数×1.2，共 2 次，间隔 3~5 天
        private void TickRaids()
        {
            if (raidsFired >= MaxRaids || nextRaidTick < 0) return;
            if (Find.TickManager.TicksGame < nextRaidTick) return;

            if (mapParent?.Map is not Map map) { ScheduleNextRaid(); return; }

            //尊重难度设置：关闭"大威胁"时本次袭击作废（不补发，任务照常）
            if (!Find.Storyteller.difficulty.allowBigThreats)
            {
                raidsFired++;
                ScheduleNextRaid();
                return;
            }

            var faction = ResolveRaiderFaction();
            if (faction == null)
            {
                nextRaidTick = Find.TickManager.TicksGame + RaidRetryDelayTicks;
                return;
            }

            var def = IncidentDefOf.RaidEnemy;
            if (def?.Worker == null)
            {
                raidsFired++;
                ScheduleNextRaid();
                return;
            }

            //forced：绕开 CanFireNow 的故事讲述者检查（发送时机由本部件掌握）；
            //实际强度会再乘 raidArrivalMode / raidStrategy 的点数曲线（原版 IncidentWorker_Raid.AdjustedRaidPoints）
            var parms = new IncidentParms
            {
                target = map,
                points = raidPoints,
                faction = faction,
                forced = true,
                quest = quest,
                customLetterLabel = "FamiliaMyth.QuestPart.RefugeeRaidLetterLabel".Translate(),
                customLetterText = "FamiliaMyth.QuestPart.RefugeeRaidLetterText".Translate()
            };

            if (!def.Worker.TryExecute(parms))
            {
                nextRaidTick = Find.TickManager.TicksGame + RaidRetryDelayTicks;
                return;
            }

            raidsFired++;
            ScheduleNextRaid();
        }

        private void ScheduleNextRaid()
        {
            nextRaidTick = Find.TickManager.TicksGame
                + Rand.RangeInclusive(RaidIntervalMinDays, RaidIntervalMaxDays) * GenDate.TicksPerDay;
        }

        //优先用生成期选定者（仍敌对且未停用），失效则现选一个敌对派系
        private Faction ResolveRaiderFaction()
        {
            if (raiderFaction != null && !raiderFaction.deactivated && raiderFaction.HostileTo(Faction.OfPlayer))
                return raiderFaction;

            raiderFaction = Find.FactionManager.RandomEnemyFaction(
                allowHidden: false, allowNonHumanlike: true, allowDefeated: false);
            return raiderFaction;
        }

        //停留期满 → 叫公会穿梭机（util 降落，登机窗口 2 天）
        private void TickEvacuation()
        {
            if (shipCalled || stayEndTick < 0) return;
            if (Find.TickManager.TicksGame < stayEndTick) return;

            shipCalled = true;
            stage = Stage.Boarding;
            Find.SignalManager.SendSignal(new Signal(inSignalEvacuate));
            //不在此处发信：穿梭机降落时由脚本的 pickupShipThing.Spawned 节点统一发「已抵达」信
            //（登机窗口 2 天由 util 的 leaveDelayTicks 控制）
        }

        //被劝加入：视为存活、退出登机名单；lodger 标记由 ExtraFaction 部件响应同一信号自动移除
        private void OnRefugeeAdopted(Pawn pawn)
        {
            if (pawn == null || !refugees.Contains(pawn)) return;

            refugees.Remove(pawn);
            if (!adopted.Contains(pawn)) adopted.Add(pawn);

            //CompShuttle.requiredPawns 是公开列表：移除后起飞达标判定不再等待该难民
            shipThing?.TryGetComp<CompShuttle>()?.requiredPawns.Remove(pawn);

            SendLetter("FamiliaMyth.QuestPart.RefugeeJoinedLabel".Translate(),
                "FamiliaMyth.QuestPart.RefugeeJoined".Translate(pawn.Name.ToStringShort));

            //全员加入：所有未死者都已转正 → 直接结算完成，不再召唤穿梭机
            if (refugees.Count == 0)
                Settle();
        }

        //有害手术：首次警告，再次撤销委托
        private void OnSurgeryViolation(Pawn victim)
        {
            surgeryViolations++;
            if (surgeryViolations > SurgeryViolationLimit)
            {
                SettleFail("FamiliaMyth.QuestPart.CommissionRevoked".Translate(),
                    "FamiliaMyth.QuestPart.CommissionRevokedSurgery".Translate());
                return;
            }

            SendLetter("FamiliaMyth.QuestPart.GuildWarning".Translate(),
                "FamiliaMyth.QuestPart.RefugeeSurgeryWarning".Translate(victim.Name.ToStringShort));
        }

        //穿梭机起飞信号（满载 SentSatisfied / 窗口满 SentUnsatisfied）
        private void Settle()
        {
            if (stage == Stage.Done) return;

            int boarded = CountBoarded();
            int adoptedCount = adopted.Count(p => p != null && !p.Destroyed && !p.Dead);
            int survived = boarded + adoptedCount;

            if (survived <= 0)
            {
                SettleFail("FamiliaMyth.QuestPart.RefugeeResettleFailed".Translate(),
                    "FamiliaMyth.QuestPart.RefugeeResettleFailedNoSurvivor".Translate());
                return;
            }

            stage = Stage.Done;
            float ratio = totalCount > 0 ? (float)survived / totalCount : 0f;

            //未登机且未转正者视为失踪：转中立后自行离图；
            //已转正者留下（已是正式殖民者）；登机者仍在机舱内，当拍归还原派系，随船以难民派系身份离场
            Dismiss(OnMapAlive().Where(p => !IsBoarded(p) && !adopted.Contains(p)).ToList());
            RestoreBoardedFaction();

            string boardedText = boarded > 0
                ? "FamiliaMyth.QuestPart.RefugeeBoardedCount".Translate(boarded)
                : "FamiliaMyth.QuestPart.RefugeeNoneBoarded".Translate();
            string adoptedText = adoptedCount > 0
                ? "FamiliaMyth.QuestPart.RefugeeAdoptedCount".Translate(adoptedCount)
                : "FamiliaMyth.QuestPart.RefugeeAdoptedNone".Translate();
            SendLetter("FamiliaMyth.QuestPart.RefugeeResettleComplete".Translate(),
                "FamiliaMyth.QuestPart.RefugeeResettleCompleteText".Translate(
                    survived, totalCount, ratio.ToStringPercent(), boardedText, adoptedText));

            GameComponent_FamiliaRankTracker.Instance?.GainPoints(category, Mathf.RoundToInt(BaseUpgradePoints * ratio));

            if (!outSignalRewardsReady.NullOrEmpty())
                Find.SignalManager.SendSignal(new Signal(outSignalRewardsReady));
        }

        private void SettleFail(string label, string text)
        {
            if (stage == Stage.Done) return;
            stage = Stage.Done;

            SendLetter(label, text);

            //失败：图上的存活难民（含已被劝加入者）一并转中立后自行离场
            //（被玩家囚禁/奴役的受害者除外，见 Dismiss）
            Dismiss(OnMapAlive());
            RestoreBoardedFaction();

            if (!outSignalFailed.NullOrEmpty())
                Find.SignalManager.SendSignal(new Signal(outSignalFailed));
        }

        //转中立（临时难民派系）后自行离开地图：失败线与失踪线共用。
        //例外：被玩家囚禁/奴役的「受害者」不离场——他们已被玩家捕获，任务撤销后仍留在玩家手中
        private void Dismiss(List<Pawn> list)
        {
            if (list.NullOrEmpty()) return;

            var leaving = list
                .Where(p => p != null && !p.Destroyed && !p.Dead && !p.IsPrisonerOfColony && !p.IsSlaveOfColony)
                .ToList();
            if (leaving.Count == 0) return;

            foreach (var pawn in leaving)
            {
                if (pawn.Faction != refugeeFaction)
                    pawn.SetFaction(refugeeFaction);        //转为中立难民派系成员
            }

            //原版工具：清 guest/手中物/lord，并对未倒地者安排 LordJob_ExitMapBest 走出地图
            LeaveQuestPartUtility.MakePawnsLeave(leaving, sendLetter: false, quest, wakeUp: true);
        }

        //登机者归还原派系：SentSatisfied/SentUnsatisfied 当拍他们仍在穿梭机内容器里，
        //此时归位即可以难民派系身份随船离场（项目救援「紧急救援」同款处理）。
        //若漏掉这一步，登机者会带着玩家派系飞走，且延迟回报子任务会找不到 rewardGiver
        //（原版 QuestPart_AddQuestRefugeeDelayedReward.GetSlate 只挑「非玩家派系」的难民）
        private void RestoreBoardedFaction()
        {
            foreach (var pawn in victims)
            {
                if (pawn == null || pawn.Destroyed || pawn.Dead) continue;
                if (!IsBoarded(pawn)) continue;
                if (pawn.Faction != refugeeFaction)
                    pawn.SetFaction(refugeeFaction);
            }
        }

        private List<Pawn> OnMapAlive()
        {
            var map = mapParent?.Map;
            return victims
                .Where(p => p != null && !p.Destroyed && !p.Dead && p.Spawned && (map == null || p.Map == map))
                .ToList();
        }

        private bool IsBoarded(Pawn pawn)
        {
            var comp = shipThing?.TryGetComp<CompShuttle>();
            return comp != null && comp.Transporter.innerContainer.Contains(pawn);
        }

        private int CountBoarded() =>
            victims.Count(p => p != null && !p.Dead && IsBoarded(p));

        private void SendLetter(string label, string text) =>
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent));

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref victims, "victims", LookMode.Reference);
            Scribe_Collections.Look(ref refugees, "refugees", LookMode.Reference);
            Scribe_Collections.Look(ref adopted, "adopted", LookMode.Reference);
            Scribe_Defs.Look(ref category, "category");
            Scribe_References.Look(ref refugeeFaction, "refugeeFaction");
            Scribe_References.Look(ref raiderFaction, "raiderFaction");
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_References.Look(ref shipThing, "shipThing");
            Scribe_Values.Look(ref raidPoints, "raidPoints");
            Scribe_Values.Look(ref totalCount, "totalCount");
            Scribe_Collections.Look(ref inSignalsJoinAccepted, "inSignalsJoinAccepted", LookMode.Value);
            Scribe_Values.Look(ref inSignalEvacuate, "inSignalEvacuate");
            Scribe_Values.Look(ref inSignalSatisfied, "inSignalSatisfied");
            Scribe_Values.Look(ref inSignalUnsatisfied, "inSignalUnsatisfied");
            Scribe_Values.Look(ref inSignalSurgeryViolation, "inSignalSurgeryViolation");
            Scribe_Values.Look(ref outSignalVictimDied, "outSignalVictimDied");
            Scribe_Values.Look(ref outSignalRewardsReady, "outSignalRewardsReady");
            Scribe_Values.Look(ref outSignalFailed, "outSignalFailed");
            Scribe_Values.Look(ref stage, "stage");
            Scribe_Values.Look(ref injuriesApplied, "injuriesApplied");
            Scribe_Values.Look(ref raidsFired, "raidsFired");
            Scribe_Values.Look(ref nextRaidTick, "nextRaidTick", -1);
            Scribe_Values.Look(ref stayEndTick, "stayEndTick", -1);
            Scribe_Values.Look(ref surgeryViolations, "surgeryViolations");
            Scribe_Values.Look(ref shipCalled, "shipCalled");
        }
    }
}
