using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FamiliaMyth.Quest
{
    //进图后放置通缉犯：营地中心附近（与守卫同场、敌意参战）
    //站立者发守点 lord（营地小范围踱步，遭遇威胁自卫互搏，禁逃跑离图）
    public class QuestPart_CriminalHuntSpawn : QuestPartCanDisable
    {
        public string inSignal;
        public Site site;
        public Pawn wanted;

        protected override void ProcessQuestSignal(Signal signal)
        {
            if (signal.tag != inSignal || site?.Map == null) return;
            if (wanted == null || wanted.Destroyed || wanted.Dead || wanted.Spawned) return;

            Map map = site.Map;
            if (!CellFinder.TryFindRandomCellNear(map.Center, map, 10,
                    c => c.Standable(map) && !c.Fogged(map)
                         && c.DistanceTo(map.Center) > 4, out var cell))
                return;

            GenSpawn.Spawn(wanted, cell, map);

            //提示玩家本次目标：信件可点击跳转到通缉犯位置
            Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(
                "发现通缉犯",
                $"{wanted.Name.ToStringShort} 正是本次追捕的通缉犯，混在窝点守卫之中。\n\n" +
                "将其击毙即可完成委托；将其击倒并带回据点、送上公会的押解穿梭机，可获得双倍酬劳。",
                LetterDefOf.NeutralEvent, new GlobalTargetInfo(wanted)));

            //守点：窝点内迎战玩家；addFleeToil=false 防止逃跑离图
            if (!wanted.Downed && wanted.GetLord() == null && wanted.Faction != null)
            {
                var lord = LordMaker.MakeNewLord(wanted.Faction,
                    new LordJob_DefendPoint(cell, 4f, 15f, isCaravanSendable: false, addFleeToil: false), map);
                lord.AddPawn(wanted);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_References.Look(ref site, "site");
            Scribe_References.Look(ref wanted, "wanted");
        }
    }
}
