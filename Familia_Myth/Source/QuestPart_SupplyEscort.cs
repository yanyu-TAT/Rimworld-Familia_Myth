using FamiliaMyth.Familia;
using FamiliaMyth.Pharna;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FamiliaMyth.Quest
{
    //「物资押运」主状态机：
    //  接取（12 天计时启动）→ 远行队停在 A 领货（comp 按钮 → 信号）：按 A 的交易清单现生成「押运清单」并发放
    //  → 押运途中由 QuestPart_SupplyEscortAmbush 伏击 → 远行队停在 B 交付（comp 按钮 → 信号）
    //  → **分批交付**：每拍只收背包里现有且属于待交清单的部分，未交齐则发信提示剩余缺口
    //  → 待交清单清空 → 快照参与者 → rewardsReady（晋升评级）；
    //  12 天未交付 → 发 timeout 信号（XML 侧信件 + RankUpFailed + End Fail）。
    //  达标口径 = 清单相等（非按重量）：不追踪物资实例来源，玩家可用自备的同种同量物资补齐。
    public class QuestPart_SupplyEscort : QuestPartActivable
    {
        //任务时限：接取起 12 天
        public const int TimeLimitTicks = 720000;

        //状态机检查间隔
        public const int CheckIntervalTicks = 250;

        public Settlement settlementA;        //领取点
        public Settlement settlementB;        //交付点
        public FamiliaCategoryDef category;   //分类（晋升评级）

        //押运清单（领取当拍生成并持久化；交付须与之逐项一致）
        public List<ThingDefCountClass> manifest = new();

        //待交清单（分批交付的剩余量；清空即完成）
        public List<ThingDefCountClass> pending = new();

        //生成期固化信号
        public string inSignalPickup;         //A 点领取（comp 发出，带 CARAVAN 参数）
        public string inSignalDeliver;        //B 点交付（comp 发出，带 CARAVAN 参数）
        public string inSignalPickedUp;       //领取完成广播（伏击 part 借此启动冷却）
        public string inSignalTimeout;        //12 天超时（XML 处理失败线）
        public string outSignalRewardsReady;  //结算广播（XML 的 RankUpComplete 监听）

        //交付当拍快照的参与者名单

        private enum Stage { WaitingPickup, Escorting, Done }

        private Stage stage = Stage.WaitingPickup;
        private bool pickedUp;
        private int deadlineTick = -1;
        private Caravan cargoCaravan;         //领取物资的远行队

        public Caravan CargoCaravan => cargoCaravan;

        //任务追踪：始终指向 A、B 两个定居点（任务栏可点击跳转）
        public override IEnumerable<GlobalTargetInfo> QuestLookTargets
        {
            get
            {
                foreach (var target in base.QuestLookTargets)
                    yield return target;
                if (settlementA != null && !settlementA.Destroyed)
                    yield return settlementA;
                if (settlementB != null && !settlementB.Destroyed)
                    yield return settlementB;
            }
        }

        protected override void Enable(SignalArgs receivedArgs)
        {
            base.Enable(receivedArgs);

            if (deadlineTick < 0)
            {
                deadlineTick = quest != null && quest.acceptanceTick > 0
                    ? quest.acceptanceTick + TimeLimitTicks
                    : Find.TickManager.TicksGame + TimeLimitTicks;
            }

            SetupComps();
        }

        public override void QuestPartTick()
        {
            if (stage == Stage.Done) return;
            if (Find.TickManager.TicksGame % CheckIntervalTicks != 0) return;

            EnsureComps();

            if (deadlineTick > 0 && Find.TickManager.TicksGame >= deadlineTick)
            {
                stage = Stage.Done;
                SendLetter("FamiliaMyth.QuestPart.SupplyEscortTimeout".Translate(),
                    "FamiliaMyth.QuestPart.SupplyEscortTimeoutText".Translate(
                        quest?.name, settlementB?.Label));
                Find.SignalManager.SendSignal(new Signal(inSignalTimeout));
            }
        }

        protected override void ProcessQuestSignal(Signal signal)
        {
            if (signal.tag == inSignalPickup)
            {
                HandlePickup(signal);
                return;
            }
            if (signal.tag == inSignalDeliver)
            {
                HandleDeliver(signal);
                return;
            }
        }

        //==================== 领取（A 点） ====================

        private void HandlePickup(Signal signal)
        {
            if (stage != Stage.WaitingPickup || pickedUp) return;
            if (!signal.args.TryGetArg<Caravan>("CARAVAN", out var caravan) || caravan == null) return;

            //本次清单总重 300~600kg 随机
            float cargoMass = SupplyEscortUtility.RandomCargoMass();
            var newManifest = SupplyEscortUtility.BuildManifest(settlementA, cargoMass);
            if (newManifest.NullOrEmpty())
            {
                //A 的交易清单异常（无 stockGenerator 等）：兜底等重白银，保证任务可完成
                Log.Error($"[FamiliaMyth] 物资押运：无法从 {(settlementA?.Label ?? "A 点")} 的交易清单生成押运物资，改用白银兜底。");
                newManifest = SupplyEscortUtility.MakeSilverFallback(cargoMass);
            }

            //按清单实际重量判断超重：超重则先弹确认窗（取消则本次不发放，可再点领取）
            float actualMass = SupplyEscortUtility.ManifestMass(newManifest);
            float overweight = caravan.MassUsage + actualMass - caravan.MassCapacity;
            if (overweight > 0f)
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "FamiliaMyth.QuestPart.SupplyEscortOverweightConfirm".Translate(
                        actualMass.ToString("F0"), overweight.ToString("F0")),
                    () => CompletePickup(caravan, newManifest, actualMass)));
                return;
            }

            CompletePickup(caravan, newManifest, actualMass);
        }

        //确认接收：写入清单、发放物资、进入押运阶段
        private void CompletePickup(Caravan caravan, List<ThingDefCountClass> newManifest, float actualMass)
        {
            if (stage != Stage.WaitingPickup || pickedUp || caravan == null) return;

            manifest = newManifest;
            pending = SupplyEscortUtility.CloneManifest(manifest);
            SupplyEscortUtility.GiveManifest(caravan, manifest);

            pickedUp = true;
            cargoCaravan = caravan;
            stage = Stage.Escorting;
            PushManifestToComps();

            SendLetter("FamiliaMyth.QuestPart.SupplyEscortPickedUp".Translate(),
                "FamiliaMyth.QuestPart.SupplyEscortPickedUpText".Translate(
                    settlementA?.Label, actualMass.ToString("F0"),
                    SupplyEscortUtility.ManifestSummary(manifest), settlementB?.Label));

            Find.SignalManager.SendSignal(new Signal(inSignalPickedUp, caravan.Named("CARAVAN")));
        }

        //==================== 交付（B 点） ====================

        private void HandleDeliver(Signal signal)
        {
            if (stage != Stage.Escorting) return;
            if (!signal.args.TryGetArg<Caravan>("CARAVAN", out var caravan) || caravan == null) return;

            //分批交付：只收背包里现有、且属于待交清单的部分，其余物品一律不动
            var delivered = SupplyEscortUtility.TakePartial(caravan, pending);
            if (!delivered.NullOrEmpty())
            {
                SupplyEscortUtility.SubtractPending(pending, delivered);
                PushManifestToComps();
            }

            if (pending.NullOrEmpty())
            {
                stage = Stage.Done;
                ClearComps();

                SendLetter("FamiliaMyth.QuestPart.SupplyEscortDelivered".Translate(),
                    "FamiliaMyth.QuestPart.SupplyEscortDeliveredText".Translate(settlementB?.Label));


                //升级点数（眷族共享，不随参与者名单分摊）
                if (category != null)
                    GameComponent_FamiliaRankTracker.Instance?.GainPoints(category, 10);

                Find.SignalManager.SendSignal(new Signal(outSignalRewardsReady));
                return;
            }

            //未交齐：本次交付与剩余缺口都照实提示（一件都没收到时也发信）
            string deliveredText = delivered.NullOrEmpty()
                ? "FamiliaMyth.QuestPart.SupplyEscortNothingCarried".Translate()
                : SupplyEscortUtility.ManifestSummary(delivered);

            SendLetter("FamiliaMyth.QuestPart.SupplyEscortIncomplete".Translate(),
                "FamiliaMyth.QuestPart.SupplyEscortIncompleteText".Translate(
                    deliveredText, SupplyEscortUtility.ManifestSummary(pending), settlementB?.Label));
        }

        //==================== 定居点 comp 装配 ====================

        private void EnsureComps()
        {
            if (!pickedUp) SetupComps();
        }

        private void SetupComps()
        {
            string label = quest?.name;
            settlementA?.GetComponent<WorldObjectComp_SupplyEscort>()
                ?.Setup(WorldObjectComp_SupplyEscort.Role.Pickup, inSignalPickup, inSignalDeliver, label);
            settlementB?.GetComponent<WorldObjectComp_SupplyEscort>()
                ?.Setup(WorldObjectComp_SupplyEscort.Role.Delivery, inSignalPickup, inSignalDeliver, label);

            PushManifestToComps();
        }

        //把「清单是否就绪 + 剩余缺口」推给两端的交互 comp（交付按钮据此可用/提示）
        private void PushManifestToComps()
        {
            bool ready = !manifest.NullOrEmpty();
            string summary = SupplyEscortUtility.ManifestSummary(pending);

            settlementA?.GetComponent<WorldObjectComp_SupplyEscort>()?.SetManifest(ready, summary);
            settlementB?.GetComponent<WorldObjectComp_SupplyEscort>()?.SetManifest(ready, summary);
        }

        private void ClearComps()
        {
            settlementA?.GetComponent<WorldObjectComp_SupplyEscort>()?.Clear();
            settlementB?.GetComponent<WorldObjectComp_SupplyEscort>()?.Clear();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            ClearComps();
        }

        private static void SendLetter(string label, string text) =>
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(label, text, LetterDefOf.NeutralEvent));

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref settlementA, "settlementA");
            Scribe_References.Look(ref settlementB, "settlementB");
            Scribe_References.Look(ref cargoCaravan, "cargoCaravan");
            Scribe_Defs.Look(ref category, "category");
            Scribe_Values.Look(ref inSignalPickup, "inSignalPickup");
            Scribe_Values.Look(ref inSignalDeliver, "inSignalDeliver");
            Scribe_Values.Look(ref inSignalPickedUp, "inSignalPickedUp");
            Scribe_Values.Look(ref inSignalTimeout, "inSignalTimeout");
            Scribe_Values.Look(ref outSignalRewardsReady, "outSignalRewardsReady");
            Scribe_Collections.Look(ref manifest, "manifest", LookMode.Deep);
            Scribe_Collections.Look(ref pending, "pending", LookMode.Deep);
            Scribe_Values.Look(ref stage, "stage", Stage.WaitingPickup);
            Scribe_Values.Look(ref pickedUp, "pickedUp");
            Scribe_Values.Look(ref deadlineTick, "deadlineTick", -1);
        }
    }
}
