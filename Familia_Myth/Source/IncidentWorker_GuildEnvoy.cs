using FamiliaMyth.Familia;
using FamiliaMyth.Quest;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace FamiliaMyth.Incidents
{
    public class IncidentWorker_GuildEnvoy : IncidentWorker
    {
        public const int TimeoutTicks = 120000; //2 days

        private List<FamiliaCategoryDef> AvailableCats =>
            DefDatabase<FamiliaCategoryDef>.AllDefs
                .Where(c => c.dailyQuests != null && c.dailyQuests.Count > 0)
                .Where(c => GameComponent_FamiliaRankTracker.Instance?.GetOrAddState(c)?.InRetryCooldown != true)
                .ToList();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (parms.target is not Map map) return false;
            if (GameComponent_GuildEnvoyTracker.HasEnvoy) return false;
            return AvailableCats.Any();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (parms.target is not Map map) return false;

            //从可挑选的种类中随机挑选
            if (AvailableCats.NullOrEmpty())
            {
                Log.Warning("No Available Familia Category");
                return false;
            }
            var category = AvailableCats.RandomElement();
            var gens = category.GetAvailableDailyQuests();
            if (gens.NullOrEmpty())
            {
                Log.Warning($"No Available Daily Quest For Category {category.label}");
                return false;
            }
            var gen = gens.RandomElement();

            return SpawnEnvoyWithQuest(gen, map);
        }

        //让使者携带指定任务出场（日常事件随机选任务与 Debug 指定任务共用入口）
        public static bool SpawnEnvoyWithQuest(GuildQuestGenerator gen, Map map)
        {
            //入口校验（在任何副作用之前）：gen 必须已绑定 GuildQuestDef
            if (gen?.def == null || map == null)
            {
                Log.Error($"[FamiliaMyth] SpawnEnvoyWithQuest 参数无效：gen={gen != null}, def={gen?.def != null}, map={map != null}");
                return false;
            }

            //生成参数
            gen.SetupParams(map);

            var envoy = GenerateEnvoy(map);
            if (envoy == null) return false;

            SendArrivalLetter(envoy);
            GameComponent_GuildEnvoyTracker.Register(envoy, gen);

            return true;
        }

        private static Pawn GenerateEnvoy(Map map)
        {
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

        private static void SendArrivalLetter(Pawn envoy)
        {
            TaggedString label = "FamiliaMyth.QuestMisc.EnvoyArrivalLetterLabel".Translate();
            TaggedString text = "FamiliaMyth.QuestMisc.EnvoyArrivalLetterText".Translate(envoy.Name.ToStringShort);
            ChoiceLetter letter = LetterMaker.MakeLetter(
                label, text, LetterDefOf.NeutralEvent, envoy);
            Find.LetterStack.ReceiveLetter(letter);
        }
    }
}
