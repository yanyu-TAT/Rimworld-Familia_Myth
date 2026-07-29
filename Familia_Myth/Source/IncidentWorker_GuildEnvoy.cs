using FamiliaMyth.Pharna;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace FamiliaMyth.Incidents
{
    public class IncidentWorker_GuildEnvoy : IncidentWorker
    {
        public const int TimeoutTicks = 120000; //2 days

        public static Dictionary<int, MissionParams> Envoys => Current.Game?
            .GetComponent<GameComponent_GuildEnvoyTracker>()?.envoys ?? new();
        public static List<Pawn> NotSpoken => Current.Game?
            .GetComponent<GameComponent_GuildEnvoyTracker>()?.notSpoken ?? new();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (parms.target is not Map map) return false;
            return !map.mapPawns.AllPawnsSpawned
                .Any(p => p.kindDef?.defName == "Guild_Envoy");
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (parms.target is not Map map) return false;

            //根据殖民地财富值和殖民者恩惠等级计算点数
            var point = StorytellerUtility.DefaultSiteThreatPointsNow();
            var m = 1f;
            foreach (var pawn in map.mapPawns.AllPawnsSpawned.Where(p => p.Faction == Faction.OfPlayer && p.IsColonist))
                if (Hediff_Pharna.GetPharna(pawn) is Hediff_Pharna pharna)
                    m += (pharna.level - 1) * (pharna.level - 1) / 10;
            point *= m;

            //随机敌对派系
            var faction = Find.FactionManager.RandomEnemyFaction(
                allowHidden: false,
                allowNonHumanlike: true,
                allowDefeated: false);
            if (faction == null) return false;

            //构建站点
            var outpostDef = DefDatabase<SitePartDef>.GetNamed("Outpost");
            if (outpostDef == null) return false;
            var partParams = new SitePartParams
            {
                threatPoints = point   // 直接使用全部威胁点数
            };
            List<SitePartDefWithParams> sitePartsParams = new()
            {
                new(outpostDef, partParams)
            };

            //保存参数
            var missionParams = new MissionParams
            {
                map = map,
                points = point,
                siteFaction = faction,
                sitePartsParams = sitePartsParams,
            };

            var envoy = GenerateEnvoy(map, missionParams);
            if (envoy == null) return false;
            
            SendArrivalLetter(envoy);
            Envoys.Add(envoy.thingIDNumber, missionParams);
            NotSpoken.Add(envoy);

            return true;
        }

        private Pawn GenerateEnvoy(Map map, MissionParams missionParams)
        {
            //检查是否有多余的储存
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

    public class MissionParams : IExposable
    {
        public Map map;
        public float points;
        public Faction siteFaction;
        public List<SitePartDefWithParams> sitePartsParams;

        public void ExposeData()
        {
            Scribe_References.Look(ref map, "map");
            Scribe_Values.Look(ref points, "points");
            Scribe_References.Look(ref siteFaction, "siteFaction");

            //无法保存，直接生成
            if (Scribe.mode == LoadSaveMode.PostLoadInit && sitePartsParams == null)
            {
                var outpostDef = DefDatabase<SitePartDef>.GetNamed("Outpost");
                var partParams = new SitePartParams { threatPoints = points };
                sitePartsParams = new() { new(outpostDef, partParams) };
            }
        }
    }
}