using FamiliaMyth.Incidents;
using FamiliaMyth.Quest;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace FamiliaMyth.Familia
{
    public class GameComponent_FamiliaRankTracker : GameComponent
    {
        public const int EnvoyTimeoutTicks = 120000;

        public Dictionary<FamiliaCategoryDef, FamiliaRankState> rankStates = new();

        public static PawnKindDef envoyDef;
        public static PawnKindDef EnvoyDef =>
            envoyDef ??= DefDatabase<PawnKindDef>.GetNamed("Guild_Envoy");

        public static FactionDef guildDef;
        public static FactionDef GuildDef =>
            guildDef ??= DefDatabase<FactionDef>.GetNamed("Guild");

        public static IncidentDef incidentDef;
        public static IncidentDef IncidentDeff =>
            incidentDef ??= DefDatabase<IncidentDef>.GetNamed("GiveQuest_Daily");

        private int incidentInterval = -1;
        private int IncidentInterval
        {
            get
            {
                if (incidentInterval <= 0)
                {
                    incidentInterval = (Rand.Int % 5 + 1) * GenDate.TicksPerDay;
                }
                return incidentInterval;
            }
        }
        private int lastIncidentTick = GenDate.TicksPerDay * 10;

        public static GameComponent_FamiliaRankTracker Instance => Current.Game?.GetComponent<GameComponent_FamiliaRankTracker>();

        public GameComponent_FamiliaRankTracker() { }

        public GameComponent_FamiliaRankTracker(Game game) { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            foreach (var cat in DefDatabase<FamiliaCategoryDef>.AllDefs)
                GetState(cat).Init(cat);
        }
        private bool CanFireEnvoy()
        {
            var map = Find.AnyPlayerHomeMap ?? Find.CurrentMap;
            if (map == null) return false;
            return incidentDef.Worker.CanFireNow(
                StorytellerUtility.DefaultParmsNow(incidentDef.category, map));
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame - lastIncidentTick >= IncidentInterval && CanFireEnvoy())
            {
                var map = Find.AnyPlayerHomeMap ?? Find.CurrentMap;
                if (map == null) return;

                var parms = StorytellerUtility.DefaultParmsNow(incidentDef.category, map);
                Find.Storyteller?.incidentQueue?.Add(incidentDef, GenDate.TicksPerDay, parms, GenDate.TicksPerDay * 3);
            }
        }

        public FamiliaRankState GetState(FamiliaCategoryDef def)
        {
            if (def == null) return null;
            if (!rankStates.TryGetValue(def, out FamiliaRankState state))
            {
                state = new FamiliaRankState();
                state.Init(def);
                rankStates[def] = state;
            }
            return state;
        }

        public FamiliaRankDef GetCurrentRank(FamiliaCategoryDef category) => GetState(category)?.curRankDef;

        public FamiliaRankDef GetNextRank(FamiliaCategoryDef category)
        {
            if (category == null) return null;
            var curRank = GetCurrentRank(category);
            var ranks = category.rankDefs.ToList();
            if (curRank == null || ranks == null) return ranks?.FirstOrDefault();
            int idx = ranks.IndexOf(curRank);
            if (idx >= 0 && idx + 1 < ranks.Count) return ranks[idx + 1];
            return null;
        }

        public int GetUpgradePoints(FamiliaCategoryDef category) => GetState(category)?.curRankDef?.upgradePointsRequired ?? 0;

        public bool CanRankUp(FamiliaCategoryDef category)
        {
            var next = GetNextRank(category)?.upgradePointsRequired;
            var cur = GetState(category)?.curRankUpPoints;
            if (next == null || cur == null) return false;
            return cur >= next;
        }

        public void GainPoints(FamiliaCategoryDef category, int amount)
        {
            if (category == null || amount <= 0) return;

            var state = GetState(category);
            state.curRankUpPoints += amount;

            var letter = LetterMaker.MakeLetter(
                "眷族成长",
                $"由于在「{category.LabelCap}」方面的卓越贡献\n" +
                $"您的眷族获得了 {amount} 点升级点数。\n\n" +
                $"当前：{state.curRankUpPoints} / {GetUpgradePoints(category)}",
                LetterDefOf.PositiveEvent);
            Find.LetterStack.ReceiveLetter(letter);

            TrySpawnRankUpEnvoy(category);
        }

        private void TrySpawnRankUpEnvoy(FamiliaCategoryDef category)
        {
            var state = GetState(category);
            if (!CanRankUp(category)) return;       //未达到升级条件
            if (state.RankUpEnvoyPending) return;   //已有使者
            if (state.rankUpQuestID != -1) return;  //已有任务

            var map = Find.AnyPlayerHomeMap ?? Find.CurrentMap;
            if (map == null) return;

            Faction guild = Find.FactionManager.FirstFactionOfDef(GuildDef);
            if (guild == null) return;

            var envoy = PawnGenerator.GeneratePawn(new PawnGenerationRequest(EnvoyDef, guild));
            //搜索进入点
            if (!CellFinder.TryFindRandomEdgeCellWith(
                c => map.reachability.CanReachColony(c),
                map, CellFinder.EdgeRoadChance_Neutral, out var entryCell))
            {
                Log.Warning("[FamiliaMyth] No Way To Colony For Guild Envoy");
                return;
            }

            //搜索停留点
            if (!RCellFinder.TryFindRandomSpotJustOutsideColony(entryCell, map, out var chillSpot))
                chillSpot = DropCellFinder.TradeDropSpot(map);

            GenSpawn.Spawn(envoy, entryCell, map);
            var job = new LordJob_VisitColony(guild, chillSpot, EnvoyTimeoutTicks);
            var lord = LordMaker.MakeNewLord(guild, job, map);
            lord.AddPawn(envoy);

            state.rankUpEnvoyID = envoy.thingIDNumber;

            //注册任务
            if (category.rankUpQuests == null || category.rankUpQuests.Count == 0) return;
            var rankUpQuest = category.rankUpQuests.RandomElement();
            var gen = (GuildQuestGenerator)Activator.CreateInstance(rankUpQuest.generatorClass);
            gen.def = rankUpQuest;
            gen.SetupParams(map);
            GameComponent_GuildEnvoyTracker.Envoys[envoy.thingIDNumber] = gen;
            GameComponent_GuildEnvoyTracker.NotSpoken.Add(envoy);

            TaggedString label = "公会使者到访";
            TaggedString text = $"来自冒险者公会的使者 {envoy.Name.ToStringShort} 已到达你的据点。\n\n" +
                "请派遣殖民者前往与使者对话，了解委托的详情。";
            ChoiceLetter letter = LetterMaker.MakeLetter(
                label, text, LetterDefOf.NeutralEvent, envoy);
            Find.LetterStack.ReceiveLetter(letter);
        }

        public void OnRankUpRequestDone(FamiliaCategoryDef category)
        {
            var state = GetState(category);
            var next = GetNextRank(category);
            if (next == null || state ==  null) return;

            state.curRankDef = next;
            state.curRankUpPoints -= next.upgradePointsRequired;

            state.rankUpEnvoyID = -1;
            state.rankUpQuestID = -1;

            var letter = LetterMaker.MakeLetter(
                "眷族评级晋升",
                $"公会已将您的眷族在「{category.LabelCap}」方面的评级提升为「{next.LabelCap}」！",
                LetterDefOf.PositiveEvent);
            Find.LetterStack.ReceiveLetter(letter);
        }

        public FamiliaCategoryDef GetCategory(int EnvoyID)
        {
            foreach (var kv in rankStates)
                if (kv.Value.rankUpEnvoyID == EnvoyID)
                    return kv.Key;
            return null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref rankStates, "rankStates", 
                keyLookMode: LookMode.Def,
                valueLookMode: LookMode.Deep);
            Scribe_Values.Look(ref incidentInterval, "incidentInterval", -1);
            Scribe_Values.Look(ref lastIncidentTick, "lastIncidentTick", GenDate.TicksPerDay * 10);
        }
    }
}
