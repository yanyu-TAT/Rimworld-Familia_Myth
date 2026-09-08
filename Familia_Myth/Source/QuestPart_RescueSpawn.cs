using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FamiliaMyth.Quest
{
    //进图后放置遇难者：站点边缘（远离中心兽群 = 人兽分离）；部分倒地（不出血，由玩家治疗）
    //站立者无行为会自行离图 → 安排"守点"（营地小范围踱步，遭遇威胁自卫互搏）；
    //守点周期到"威胁清除加入玩家派系"为止，由主状态机当拍释放，之后不再发放
    public class QuestPart_RescueSpawn : QuestPartCanDisable
    {
        public string inSignal;
        public Site site;
        public List<Pawn> victims = new();
        public float downedChance = 0.4f;

        protected override void ProcessQuestSignal(Signal signal)
        {
            if (signal.tag != inSignal || site?.Map == null) return;
            Map map = site.Map;
            foreach (var pawn in victims)
            {
                if (pawn == null || pawn.Dead || pawn.Spawned) continue;
                if (!CellFinder.TryFindRandomCellNear(map.Center, map, 32,
                        c => c.Standable(map) && !c.Fogged(map)
                             && c.DistanceTo(map.Center) > 20, out var cell))
                    continue;
                if (Rand.Chance(downedChance))
                    HealthUtility.DamageUntilDowned(pawn, allowBleedingWounds: false);
                GenSpawn.Spawn(pawn, cell, map);

                //站立者守点：营地小范围踱步；威胁进入范围会自卫（互搏）；addFleeToil=false 防止逃跑离图
                if (!pawn.Downed && pawn.GetLord() == null && pawn.Faction != null)
                {
                    var lord = LordMaker.MakeNewLord(pawn.Faction,
                        new LordJob_DefendPoint(cell, 3f, 12f, isCaravanSendable: false, addFleeToil: false), map);
                    lord.AddPawn(pawn);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_References.Look(ref site, "site");
            Scribe_Collections.Look(ref victims, "victims", LookMode.Reference);
            Scribe_Values.Look(ref downedChance, "downedChance", 0.4f);
        }
    }
}
