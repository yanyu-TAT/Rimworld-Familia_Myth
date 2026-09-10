using System.Collections.Generic;
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
    //  → 待交清单清空 → 快照参与者 → 伟业值 + rewardsReady（晋升评级）；
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
        public Pawn acceptor;                 //接取者（强制任务恒为 null）
        public FamiliaCategoryDef category;   //分类（晋升评级）
        public float baseExploits;            //基础伟业值

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
        public List<Pawn> participants = new();

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
                SendLetter("押运超时",
                    $"委托「{quest?.name}」限时 12 天已到，押运物资未能如期送达 {settlementB?.Label}。\n\n委托失败。");
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
                    $"押运物资总重约 {actualMass:F0}kg，接收后队伍将超载约 {overweight:F0}kg，"
                    + "可能导致队伍无法移动。\n\n确定硬塞吗？",
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

            SendLetter("押运物资已接收",
                $"已从 {settlementA?.Label} 接收押运物资（总重约 {actualMass:F0}kg）：\n\n"
                + $"{SupplyEscortUtility.ManifestSummary(manifest)}\n\n"
                + $"请在 12 天内将上述物资原样送达 {settlementB?.Label}——"
                + "交付时按清单逐项核对（种类与数量均需一致），可分批交付，清单外的物品不会被收取。\n\n"
                + "注意：押运途中可能遭遇伏击。");

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
                SnapshotParticipants(caravan);
                stage = Stage.Done;
                ClearComps();

                SendLetter("押运完成",
                    $"押运物资已如数送达 {settlementB?.Label}。\n\n公会对眷族的表现予以认可。");

                QuestUtilities.DistributeExploits(participants, acceptor, baseExploits,
                    "强制任务完成", "由于完成了物资押运：\n\n");

                Find.SignalManager.SendSignal(new Signal(outSignalRewardsReady));
                return;
            }

            //未交齐：本次交付与剩余缺口都照实提示（一件都没收到时也发信）
            string deliveredText = delivered.NullOrEmpty()
                ? "（远行队未携带待交清单上的物资）"
                : SupplyEscortUtility.ManifestSummary(delivered);

            SendLetter("押运物资未交齐",
                $"本次交付：{deliveredText}\n\n"
                + $"尚缺：{SupplyEscortUtility.ManifestSummary(pending)}\n\n"
                + $"请继续把剩余物资运至 {settlementB?.Label} 分批交付。");
        }

        private void SnapshotParticipants(Caravan caravan)
        {
            participants.Clear();
            if (caravan == null) return;

            foreach (var pawn in caravan.PawnsListForReading)
            {
                if (pawn == null || pawn.Dead) continue;
                if (pawn.Faction != Faction.OfPlayer) continue;
                if (pawn.GetPharna() == null) continue;
                participants.Add(pawn);
            }
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
            Scribe_References.Look(ref acceptor, "acceptor");
            Scribe_References.Look(ref cargoCaravan, "cargoCaravan");
            Scribe_Defs.Look(ref category, "category");
            Scribe_Values.Look(ref baseExploits, "baseExploits");
            Scribe_Values.Look(ref inSignalPickup, "inSignalPickup");
            Scribe_Values.Look(ref inSignalDeliver, "inSignalDeliver");
            Scribe_Values.Look(ref inSignalPickedUp, "inSignalPickedUp");
            Scribe_Values.Look(ref inSignalTimeout, "inSignalTimeout");
            Scribe_Values.Look(ref outSignalRewardsReady, "outSignalRewardsReady");
            Scribe_Collections.Look(ref manifest, "manifest", LookMode.Deep);
            Scribe_Collections.Look(ref pending, "pending", LookMode.Deep);
            Scribe_Collections.Look(ref participants, "participants", LookMode.Reference);
            Scribe_Values.Look(ref stage, "stage", Stage.WaitingPickup);
            Scribe_Values.Look(ref pickedUp, "pickedUp");
            Scribe_Values.Look(ref deadlineTick, "deadlineTick", -1);
        }
    }
}
