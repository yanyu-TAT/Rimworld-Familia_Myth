using FamiliaMyth.Familia;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Quest
{
    //「遗迹探索」主状态机（过程 part）：
    //  站点地图生成 → 在复杂体室内房间落位任务机关箱(AncientHermeticCrate)并装入目标物品
    //    （随机异教文化现造名，见 Thing_AncientRelic / AncientRelicNamer）→
    //  玩家开箱（openedSignal = 发现）→ 唤醒房内沉睡守卫 + 启动双计时：
    //    1~3 天 → 0.5× 袭击打玩家据点（QuestPart_RandomRaid 直发）
    //    3~5 天 → 公会穿梭机降落据点（XML util；2 天窗口，目标物品上船即飞）
    //  SentSatisfied → 交付结算；SentUnsatisfied → 失败；
    //  目标物品被毁 / 落于他人掌控（失踪）→ 失败；箱被砸开（未点开箱）→ 视同发现。
    public class QuestPart_AncientRelic : QuestPartActivable
    {
        public const string RelicDefName = QuestNode_Root_Daily_AncientRelic.RelicDefName;
        public const float RaidPointsFactor = 0.5f;
        public const int RaidDelayMinTicks = 60000;      //1 天
        public const int RaidDelayMaxTicks = 180000;     //3 天
        public const int ShuttleDelayMinTicks = 180000;  //3 天
        public const int ShuttleDelayMaxTicks = 300000;  //5 天
        public const int CheckIntervalTicks = 250;

        private enum Stage { AwaitingMap, Discovering, Guarding, WaitingShip, Done }

        public Site site;                    //复杂体站点
        public FamiliaCategoryDef category;  //分类（升级点）
        public Thing shipThing;              //公会穿梭机（交付判定/清算）

        //生成期 hardcode 的同串信号
        public string inSignalFound;         //机关箱开箱（=发现）
        public string inSignalRaid;          //0.5× 守家袭击
        public string inSignalShuttle;       //召唤穿梭机 util
        public string inSignalSatisfied;     //目标物品装船起飞
        public string inSignalUnsatisfied;   //窗口满未交付起飞

        //机关箱与其中的目标物品（探索期内供追踪/发现）
        public Building_Crate crate;
        public Thing targetItem;

        //威胁清除时点快照的参与者名单，结算按此发放

        private Stage stage = Stage.AwaitingMap;
        private int discoveredTick = -1;     //开箱发现时刻
        private int raidAtTick = -1;         //袭击时刻（发现后 1~3 天）
        private int shuttleAtTick = -1;      //穿梭机时刻（发现后 3~5 天）
        private bool raidSent;
        private bool shuttleCalled;

        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get
            {
                foreach (var t in base.QuestLookTargets)
                    yield return t;
                if (crate != null && !crate.Destroyed)
                    yield return crate;
                else if (targetItem != null && !targetItem.Destroyed)
                    yield return targetItem;
            }
        }

        //仅在站点地图生成后 Enabled（此后玩家才可能进图开箱）
        public override void QuestPartTick()
        {
            if (stage == Stage.Done)
                return;
            if (Find.TickManager.TicksGame % CheckIntervalTicks != 0)
                return;

            //地图生成后：落位机关箱并装入目标物品
            if (stage == Stage.AwaitingMap)
            {
                if (!TrySpawnCrateAndRelic()){
                    Log.Warning("[FamiliaMyth]Failed To Summon Relic For Current Quest.");
                    return; //落位失败（极小概率）→ 维持等待；12 天超时表兜底失败
                }
                stage = Stage.Discovering;
                return;
            }

            //探索期兜底：机关箱被砸开/摧毁而目标物品未随箱毁 → 视同发现（目标物品掉落在旁）
            if (stage == Stage.Discovering && crate != null && crate.Destroyed)
            {
                if (targetItem == null || targetItem.Destroyed)
                {
                    SettleFail("FamiliaMyth.QuestPart.AncientRelicDestroyed".Translate(),
                        "FamiliaMyth.QuestPart.AncientRelicDestroyedText".Translate());
                    return;
                }
                StartGuarding();
                return;
            }

            if (stage == Stage.Done)
                return;

            //守家/等待交付期：目标物品损毁或落于他人掌控 → 失败
            if (stage is Stage.Guarding or Stage.WaitingShip)
            {
                if (!HandleRelicLoss())
                    return;

                //袭击到点（1~3 天；无论目标物品是否已回据点，固定打玩家据点）
                if (stage == Stage.Guarding && !raidSent && Find.TickManager.TicksGame >= raidAtTick)
                {
                    raidSent = true;
                    Find.SignalManager.SendSignal(new Signal(inSignalRaid));
                }
                //穿梭机到点（3~5 天）→ 召唤公会押运 util
                if (stage == Stage.Guarding && !shuttleCalled && Find.TickManager.TicksGame >= shuttleAtTick)
                {
                    shuttleCalled = true;
                    stage = Stage.WaitingShip;
                    Find.SignalManager.SendSignal(new Signal(inSignalShuttle));
                }

                //等待交付期：穿梭机落地后自动把目标物品加入待装清单（方案甲，等价原版 CompShuttle.autoload）
                if (stage == Stage.WaitingShip)
                    TryQueueRequiredItemLoading();
            }
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            //站点地图生成当拍即落位机关箱（不等 250 tick 刻度，进图立刻可见）
            if (stage == Stage.AwaitingMap && !signal.tag.NullOrEmpty()
                && signal.tag == inSignalEnable && TrySpawnCrateAndRelic())
            {
                stage = Stage.Discovering;
                return;
            }
            if (signal.tag == inSignalFound && stage == Stage.Discovering)
            {
                StartGuarding();
                return;
            }
            if (signal.tag == inSignalSatisfied) { SettleSuccess(); return; }
            if (signal.tag == inSignalUnsatisfied)
            {
                SettleFail("FamiliaMyth.QuestPart.RelicDeliveryFailed".Translate(),
                    "FamiliaMyth.QuestPart.RelicDeliveryFailedText".Translate());
                return;
            }
            base.Notify_QuestSignalReceived(signal);
        }

        //开箱（或箱子被砸开）→ 发现：快照参与者、启动双计时、唤醒房内守卫、发提示信
        private void StartGuarding()
        {
            if (stage == Stage.Done)
                return;
            stage = Stage.Guarding;
            discoveredTick = Find.TickManager.TicksGame;
            raidAtTick = discoveredTick + Rand.RangeInclusive(RaidDelayMinTicks, RaidDelayMaxTicks);
            shuttleAtTick = discoveredTick + Rand.RangeInclusive(ShuttleDelayMinTicks, ShuttleDelayMaxTicks);

            WakeUpRoomGuards();

            string itemLabel = targetItem != null && !targetItem.Destroyed
                ? targetItem.LabelCap
                : "FamiliaMyth.QuestPart.AncientRelicUnknownItem".Translate();

            GlobalTargetInfo focusTarget;
            if (targetItem != null && !targetItem.Destroyed)
                focusTarget = targetItem;
            else if (crate != null && !crate.Destroyed)
                focusTarget = crate;
            else
                focusTarget = site;

            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(
                "FamiliaMyth.QuestPart.AncientRelicFound".Translate(),
                "FamiliaMyth.QuestPart.AncientRelicFoundText".Translate(itemLabel),
                LetterDefOf.NeutralEvent, focusTarget));
        }

        //探索/守家期间目标物品状态监控：返回 false = 已结算、应停止本阶段推进
        private bool HandleRelicLoss()
        {
            if (targetItem == null || targetItem.Destroyed)
            {
                SettleFail("FamiliaMyth.QuestPart.AncientRelicMissing".Translate(),
                    "FamiliaMyth.QuestPart.AncientRelicMissingText".Translate());
                return false;
            }
            if (!InPlayerControl())
            {
                SettleFail("FamiliaMyth.QuestPart.AncientRelicMissing".Translate(),
                    "FamiliaMyth.QuestPart.AncientRelicLostText".Translate());
                return false;
            }
            return true;
        }

        //自动装载（交付）：穿梭机落地后每节拍执行——
        //  ① 确保穿梭机 CompTransporter 处于「可装载态」（InitiateLoading，否则 JobDriver_HaulToContainer
        //     会被 FailOn(WasLoadingCanceled) 秒判失败）；
        //  ② 幂等把目标物加入 toLoad → WorkGiver_LoadTransporters 自动派 HaulToTransporter 搬运上船（有演出）。
        //与「抱囚犯上船」(FloatMenuOptionProvider_CarryToShuttle 先 InitiateLoading 再 HaulToTransporter) 同机制。
        //入舱后由 util 的 leaveImmediatelyWhenSatisfied 检测满员 → SentSatisfied → 起飞。
        private void TryQueueRequiredItemLoading()
        {
            if (shipThing == null || shipThing.Destroyed || !shipThing.Spawned)
                return;
            if (targetItem == null || targetItem.Destroyed)
                return;
            if (shipThing.TryGetComp<CompShuttle>() is not CompShuttle shuttle || shuttle.Transporter == null)
                return;

            CompTransporter comp = shuttle.Transporter;
            ThingDef relicDef = targetItem.def;

            //舱内已装满足 → 无需再处理（交给 leaveImmediatelyWhenSatisfied 起飞）
            if (comp.innerContainer.TotalStackCountOfDef(relicDef) >= 1)
                return;
            //目标物已在穿梭机货舱内
            if (comp.innerContainer.Contains(targetItem))
                return;

            //① 进入可装载态（幂等：LoadingInProgress 后不再重复 InitiateLoading）
            if (!comp.LoadingInProgressOrReadyToLaunch)
                TransporterUtility.InitiateLoading(Gen.YieldSingle(comp));

            //② 目标物已在 toLoad → 已排队，交给 hauler；不清理不重复
            if (LeftToLoadContainsDef(comp, relicDef))
                return;

            //目标物必须已回到船所在图且可被装载（sendable；放地面/存储/库存均可；在异图则等带回）
            if (targetItem.MapHeld != shipThing.Map)
                return;
            var matches = new List<Thing>();
            foreach (Thing t in TransporterUtility.AllSendableItems(
                         new List<CompTransporter> { comp }, shipThing.Map))
                if (t.def == relicDef && !matches.Contains(t))
                    matches.Add(t);
            if (matches.Count == 0)
                return; //目标物尚不可送（路上/背包）→ 等下次节拍

            var transferable = new TransferableOneWay();
            transferable.things.AddRange(matches);
            comp.AddToTheToLoadList(transferable, 1);
        }

        private static bool LeftToLoadContainsDef(CompTransporter comp, ThingDef def)
        {
            if (comp.leftToLoad == null)
                return false;
            foreach (var entry in comp.leftToLoad)
            {
                if (entry == null)
                    continue;
                for (int i = 0; i < entry.things.Count; i++)
                    if (entry.things[i] != null && entry.things[i].def == def)
                        return true;
            }
            return false;
        }

        //目标物品仍在玩家掌控中的判定（宽松口径：遗落地图由卸载销毁兜底，防误杀）
        private bool InPlayerControl()
        {
            if (targetItem == null || targetItem.Destroyed)
                return false;

            //已装进穿梭机（交付阶段）
            if (shipThing != null && !shipThing.Destroyed
                && shipThing.TryGetComp<CompShuttle>() is CompShuttle shuttleComp
                && shuttleComp.Transporter != null
                && shuttleComp.Transporter.innerContainer.Contains(targetItem))
                return true;

            //上溯持有链根：被某 pawn 携带/装入某容器
            IThingHolder cur = targetItem.ParentHolder;
            int depth = 0;
            while (cur != null && cur.ParentHolder != null && depth++ < 64)
                cur = cur.ParentHolder;

            switch (cur)
            {
                case Pawn pawn:
                    return pawn.Spawned && !pawn.Dead && pawn.Faction == Faction.OfPlayer;
                case Caravan caravan:
                    return caravan.Faction == Faction.OfPlayer;
                case null:
                    return targetItem.Spawned; //地面物：遗落地图由卸载销毁兜底失败
                default:
                    return true; //其他容器/载体（玩家运输舱、穿梭机等）：宽松放行
            }
        }

        //int cnt = 0;
        //机关箱落位（复杂体室内房间内）+ 装入目标物品 + 设置开箱信号
        private bool TrySpawnCrateAndRelic()
        {
            //Log.Message($"try : {cnt++}");
            if (site?.Map is not Map map)
                return false;
            //Log.Message("Map Summoned");

            if (crate != null && !crate.Destroyed)
                return true;

            //Log.Message("Tring Find Position To Spawn Crate And Relic.");
            var crateDef = ThingDefOf.AncientHermeticCrate;
            if (crateDef == null)
                return false;
            var relicDef = DefDatabase<ThingDef>.GetNamedSilentFail(RelicDefName);
            if (relicDef == null)
                return false;

            if (!TryFindIndoorCellFor(crateDef, map, out IntVec3 cell, out Rot4 rot))
                return false;

            //Log.Message("Position Found. Try Spawn.");
            Thing thing = ThingMaker.MakeThing(crateDef);
            GenSpawn.Spawn(thing, cell, map, rot);
            if (thing is not Building_Crate newCrate)
                return false;

            //目标物品：现造异教目标物品名 → 装入箱内
            Thing relicThing = ThingMaker.MakeThing(relicDef);
            if (relicThing is Thing_AncientRelic ancientRelic)
                ancientRelic.relicName = AncientRelicNamer.GenerateRelicName();
            if (!newCrate.TryAcceptThing(relicThing, allowSpecialEffects: false))
                GenSpawn.Spawn(relicThing, cell, map); //兜底：箱满时置于箱旁

            crate = newCrate;
            targetItem = relicThing;
            crate.openedSignal = inSignalFound;
            return true;
        }

        //在复杂体室内（roofed、无实体建筑占用）房间/通道中寻找可放 1×N 建筑的空位
        private static bool TryFindIndoorCellFor(ThingDef def, Map map, out IntVec3 cell, out Rot4 rot)
        {
            rot = Rot4.South;
            cell = IntVec3.Invalid;
            foreach (var room in map.regionGrid.AllRooms
                         .Where(r => !r.PsychologicallyOutdoors)
                         .OrderByDescending(r => r.CellCount)
                         .ThenBy(r => Rand.Value))
            {
                var cells = room.Cells.ToList();
                int tries = Mathf.Min(cells.Count, 48);
                for (int i = 0; i < tries; i++)
                {
                    IntVec3 candidate = cells[Rand.Range(0, cells.Count)];
                    if (CanPlaceAt(def, candidate, map, rot))
                    {
                        cell = candidate;
                        return true;
                    }
                }
            }
            return false;
        }

        //落位判定：只校验空间/屋顶/实体占用。
        private static bool CanPlaceAt(ThingDef def, IntVec3 root, Map map, Rot4 rot)
        {
            if (!root.InBounds(map) || !root.Standable(map))
                return false;
            if (!map.roofGrid.Roofed(root))
                return false;
            foreach (IntVec3 c in GenAdj.CellsOccupiedBy(root, rot, def.size))
            {
                if (!c.InBounds(map) || !c.Standable(map))
                    return false;
                if (c.GetEdifice(map) != null)
                    return false;
                if (!map.roofGrid.Roofed(c))
                    return false;
            }
            return true;
        }

        //开箱警报：唤醒机关箱所在房间内沉睡的守卫（LordJob_SleepThenAssaultColony）
        private void WakeUpRoomGuards()
        {
            if (crate == null || crate.Destroyed)
                return;
            Map map = crate.Map;
            if (map == null)
                return;
            Room room = crate.Position.GetRoom(map);
            if (room == null)
                return;
            foreach (var lord in map.lordManager.lords.ToList())
            {
                if (lord.LordJob is not LordJob_SleepThenAssaultColony)
                    continue;
                bool inRoom = lord.ownedPawns.Any(p => p != null && p.Spawned && !p.Dead
                    && p.Position.GetRoom(map) == room && !p.Awake());
                if (inRoom)
                    lord.Notify_DormancyWakeup();
            }
        }

        //交付成功：目标物品装上穿梭机起飞
        private void SettleSuccess()
        {
            if (stage == Stage.Done)
                return;
            stage = Stage.Done;
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(
                "FamiliaMyth.QuestPart.QuestComplete".Translate(),
                "FamiliaMyth.QuestPart.AncientRelicShuttleLeft".Translate(),
                LetterDefOf.NeutralEvent));
            GameComponent_FamiliaRankTracker.Instance?.GainPoints(category, 10);
            quest.End(QuestEndOutcome.Success);
        }

        private void SettleFail(string label, string text)
        {
            if (stage == Stage.Done)
                return;
            stage = Stage.Done;
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(label, text, LetterDefOf.NegativeEvent));
            quest.End(QuestEndOutcome.Fail);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref site, "site");
            Scribe_Defs.Look(ref category, "category");
            Scribe_References.Look(ref shipThing, "shipThing");
            Scribe_Values.Look(ref inSignalFound, "inSignalFound");
            Scribe_Values.Look(ref inSignalRaid, "inSignalRaid");
            Scribe_Values.Look(ref inSignalShuttle, "inSignalShuttle");
            Scribe_Values.Look(ref inSignalSatisfied, "inSignalSatisfied");
            Scribe_Values.Look(ref inSignalUnsatisfied, "inSignalUnsatisfied");
            Scribe_References.Look(ref crate, "crate");
            Scribe_References.Look(ref targetItem, "relic");
            Scribe_Values.Look(ref stage, "stage");
            Scribe_Values.Look(ref discoveredTick, "discoveredTick", -1);
            Scribe_Values.Look(ref raidAtTick, "raidAtTick", -1);
            Scribe_Values.Look(ref shuttleAtTick, "shuttleAtTick", -1);
            Scribe_Values.Look(ref raidSent, "raidSent");
            Scribe_Values.Look(ref shuttleCalled, "shuttleCalled");
        }
    }
}
