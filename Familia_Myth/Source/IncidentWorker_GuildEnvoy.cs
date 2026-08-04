using FamiliaMyth.Familia;
using FamiliaMyth.Pharna;
using FamiliaMyth.Quest;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FamiliaMyth.Incidents
{
    public class IncidentWorker_GuildEnvoy : IncidentWorker
    {
        public const int TimeoutTicks = 120000; //2 days

        private static Dictionary<int, GuildQuestGenerator> Envoys => GameComponent_GuildEnvoyTracker.Envoys ?? new();
        private static List<Pawn> NotSpoken => GameComponent_GuildEnvoyTracker.NotSpoken ?? new();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (parms.target is not Map map) return false;
            return !map.mapPawns.AllPawnsSpawned
                .Any(p => p.kindDef?.defName == "Guild_Envoy");
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (parms.target is not Map map) return false;

            //从可挑选的种类中随机挑选
            var cats = DefDatabase<FamiliaCategoryDef>.AllDefs
                .Where(c => c.dailyQuests != null && c.dailyQuests.Count > 0)
                .ToList();
            if (cats.Count == 0) return false;
            var category = cats.RandomElement();
            var questDef = category.dailyQuests.RandomElement();

            //生成参数
            var gen = (GuildQuestGenerator)Activator.CreateInstance(questDef.generatorClass);
            gen.def = questDef;
            gen.SetupParams(map);

            var envoy = GenerateEnvoy(map);
            if (envoy == null) return false;
            
            SendArrivalLetter(envoy);
            Envoys[envoy.thingIDNumber] = gen;
            NotSpoken.Add(envoy);

            return true;
        }

        private Pawn GenerateEnvoy(Map map)
        {
            //检查是否有多余使者
            var pawns = map.mapPawns.AllPawnsSpawned;
            var toRemove = new List<int>();
            foreach (var ky in Envoys)
                if (!pawns.Any(p => p.thingIDNumber == ky.Key)) 
                    toRemove.Add(ky.Key);
            foreach (var key in toRemove)
                Envoys.Remove(key);

            var toRemoveP = new List<Pawn>();
            foreach (var pawn in NotSpoken)
                if (!pawns.Any(p => p.thingIDNumber == pawn.thingIDNumber))
                    toRemoveP.Add(pawn);
            foreach (var pawn in toRemoveP)
                NotSpoken.Remove(pawn);

            var kindDef = PawnKindDef.Named("Guild_Envoy");
            var faction = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("Guild"));
            var envoy = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kindDef, faction));

            if (CellFinder.TryFindRandomEdgeCellWith(
                c => map.reachability.CanReachColony(c),
                map,
                CellFinder.EdgeRoadChance_Neutral,
                out IntVec3 entryCell))
            {
                GenSpawn.Spawn(envoy, entryCell, map, Rot4.South);

                //设定使者停留点
                if (!RCellFinder.TryFindRandomSpotJustOutsideColony(entryCell, map, out IntVec3 chillSpot))
                    chillSpot = DropCellFinder.TradeDropSpot(map);

                //设定使者工作
                var lordJob = new LordJob_VisitColony(faction, chillSpot, TimeoutTicks);
                var lord = LordMaker.MakeNewLord(faction, lordJob, map);
                lord.AddPawn(envoy);
            }

            return envoy;
        }
        private void SendArrivalLetter(Pawn envoy)
        {
            TaggedString label = "公会使者到访";
            TaggedString text = $"来自冒险者公会的使者 {envoy.Name.ToStringShort} 已到达你的据点。\n\n" +
                "请派遣殖民者前往与使者对话，了解委托的详情。";
            ChoiceLetter letter = LetterMaker.MakeLetter(
                label, text, LetterDefOf.NeutralEvent, envoy);
            Find.LetterStack.ReceiveLetter(letter);
        }
    }
}