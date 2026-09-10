using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace FamiliaMyth.Quest
{
    //「物资押运」伏击部件：
    //  领取物资当拍起算冷却，此后每 CheckIntervalTicks 扫一次「以 A、B 为圆心、AB 距离为半径」的两个圆，
    //  对范围内任意玩家远行队尝试发动原版伏击事件（IncidentDef "Ambush"），每次袭击间隔 2~4 天，上限 2 次。
    //  技术约束（原版）：伏击需在目标 tile 生成临时地图，而该 tile 被未开 allowCaravanIncidentsWhichGenerateMap
    //  的世界对象（定居点等）占用时无法生成 → 该轮跳过，下一轮再试（"走在路上才会被劫"）。
    public class QuestPart_SupplyEscortAmbush : QuestPartActivable
    {
        //轮询间隔
        public const int CheckIntervalTicks = 1000;

        //袭击次数上限
        public const int MaxAmbushes = 2;

        //两次袭击之间的间隔（天）
        public const int DelayMinDays = 2;
        public const int DelayMaxDays = 4;

        public Settlement settlementA;   //圆 A 圆心
        public Settlement settlementB;   //圆 B 圆心
        public float ambushPoints;       //伏击点数（生成期已算好）

        public string inSignalPickedUp;  //玩家在 A 领取物资 → 启动冷却

        private int nextAmbushTick = -1;
        private int ambushCount;
        private bool armed;

        //半径 = A 到 B 的距离
        private float Radius =>
            settlementA == null || settlementB == null
                ? 0f
                : SupplyEscortUtility.ApproxDistance(settlementA.Tile, settlementB.Tile);

        public override void QuestPartTick()
        {
            if (ambushCount >= MaxAmbushes) return;
            if (!armed || nextAmbushTick < 0) return;
            if (Find.TickManager.TicksGame % CheckIntervalTicks != 0) return;
            if (Find.TickManager.TicksGame < nextAmbushTick) return;

            var caravan = FindTargetCaravan();
            if (caravan == null) return;

            //该 tile 无法生成伏击图（被世界对象占用等）→ 保留冷却，下一轮再试
            if (!TryFireAmbush(caravan)) return;

            ambushCount++;
            nextAmbushTick = Find.TickManager.TicksGame
                + Rand.RangeInclusive(DelayMinDays, DelayMaxDays) * GenDate.TicksPerDay;
        }

        protected override void ProcessQuestSignal(Signal signal)
        {
            if (signal.tag != inSignalPickedUp || armed) return;

            armed = true;
            nextAmbushTick = Find.TickManager.TicksGame
                + Rand.RangeInclusive(DelayMinDays, DelayMaxDays) * GenDate.TicksPerDay;
        }

        //范围内任意玩家远行队（正在地图里的队伍已不在世界层，自然不会入选）
        private Caravan FindTargetCaravan()
        {
            float radius = Radius;
            if (radius <= 0f) return null;

            var candidates = new List<Caravan>();
            foreach (var caravan in Find.WorldObjects.Caravans)
            {
                if (caravan == null || caravan.Destroyed) continue;
                if (caravan.Faction != Faction.OfPlayer) continue;
                if (caravan.PawnsListForReading.NullOrEmpty()) continue;
                if (!InRange(caravan.Tile, radius)) continue;

                candidates.Add(caravan);
            }
            return candidates.RandomElementWithFallback();
        }

        private bool InRange(PlanetTile tile, float radius)
        {
            if (!tile.Valid) return false;
            return SupplyEscortUtility.ApproxDistance(tile, settlementA.Tile) <= radius
                || SupplyEscortUtility.ApproxDistance(tile, settlementB.Tile) <= radius;
        }

        private bool TryFireAmbush(Caravan caravan)
        {
            var def = DefDatabase<IncidentDef>.GetNamedSilentFail("Ambush");
            if (def == null || def.Worker == null) return false;

            //forced 会短路 CanFireNow 的全部检查，因此这里必须自己确认该 tile 能生成伏击图
            if (!CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(caravan.Tile)) return false;

            var parms = new IncidentParms
            {
                forced = true,
                quest = quest,
                target = caravan,
                points = ambushPoints,
                customLetterLabel = "押运队遭遇伏击",
                customLetterText = "押运队在路上遭到伏击！\n\n"
                    + "敌方武装拦住了去路——打赢这场战斗才能继续赶路。\n\n{BASETEXT}"
            };

            return def.Worker.TryExecute(parms);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref settlementA, "ambushSettlementA");
            Scribe_References.Look(ref settlementB, "ambushSettlementB");
            Scribe_Values.Look(ref ambushPoints, "ambushPoints");
            Scribe_Values.Look(ref inSignalPickedUp, "ambushInSignalPickedUp");
            Scribe_Values.Look(ref nextAmbushTick, "nextAmbushTick", -1);
            Scribe_Values.Look(ref ambushCount, "ambushCount");
            Scribe_Values.Look(ref armed, "armed");
        }
    }
}
