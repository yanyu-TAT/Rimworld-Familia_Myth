using FamiliaMyth.Incidents;
using FamiliaMyth.Quest;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace FamiliaMyth.Familia
{
    public class GameComponent_FamiliaRankTracker : GameComponent
    {
        private const int EnvoyTimeoutTicks = 120000;
        private const int CheckIntervalTicks = 1000;

        public Dictionary<FamiliaCategoryDef, FamiliaRankState> rankStates = new();

        public static PawnKindDef envoyDef;
        public static PawnKindDef EnvoyDef =>
            envoyDef ??= DefDatabase<PawnKindDef>.GetNamed("Guild_Envoy");

        public static FactionDef guildDef;
        public static FactionDef GuildDef =>
            guildDef ??= DefDatabase<FactionDef>.GetNamed("Guild");

        public static IncidentDef incidentDef;
        public static IncidentDef IncidentDef =>
            incidentDef ??= DefDatabase<IncidentDef>.GetNamed("GiveQuest_Daily");

        private static int CurrentTick => Find.TickManager.TicksGame;

        private int nextIncidentTick = GenDate.TicksPerDay * 10;

        public static GameComponent_FamiliaRankTracker Instance => Current.Game?.GetComponent<GameComponent_FamiliaRankTracker>();

        public GameComponent_FamiliaRankTracker() { }

        public GameComponent_FamiliaRankTracker(Game game) { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            EnsureGuildFaction();
            foreach (var cat in DefDatabase<FamiliaCategoryDef>.AllDefs)
                GetOrAddState(cat)?.Init(cat);
        }

        private static void EnsureGuildFaction()
        {
            if (Find.FactionManager.FirstFactionOfDef(GuildDef) != null) return;

            var faction = FactionGenerator.NewGeneratedFaction(
                new FactionGeneratorParms(GuildDef, default, hidden: true));
            Find.FactionManager.Add(faction);

            //保证与玩家中立，避免使者因派系关系变为敌对
            if (Faction.OfPlayer != null
                && faction.RelationKindWith(Faction.OfPlayer) == FactionRelationKind.Hostile)
                faction.SetRelationDirect(Faction.OfPlayer, FactionRelationKind.Neutral);

            Log.Message("[FamiliaMyth] Guild faction instance ensured for quest envoys.");
        }

        private bool CanFireEnvoy()
        {
            var map = Find.AnyPlayerHomeMap ?? Find.CurrentMap;
            if (map == null) return false;
            return IncidentDef.Worker.CanFireNow(
                StorytellerUtility.DefaultParmsNow(IncidentDef.category, map));
        }

        public override void GameComponentTick()
        {
            if (CurrentTick % CheckIntervalTicks != 0) return;
            if (GameComponent_GuildEnvoyTracker.HasEnvoy) return;

            //Log.Message("Trying To Issue Quest.");

            //尝试发布升级任务
            foreach (var kv in rankStates)
            {
                var state = kv.Value;
                if (state.InRetryCooldown) continue;
                if (TrySpawnRankUpEnvoy(kv.Key))
                {
                    state.rankUpQuestRetryTick = -1;
                    break;
                }
            }

            //尝试发布日常任务
            if (CurrentTick >= nextIncidentTick && CanFireEnvoy())
            {
                var map = Find.AnyPlayerHomeMap ?? Find.CurrentMap;
                if (map == null) return;

                var parms = StorytellerUtility.DefaultParmsNow(IncidentDef.category, map);
                Find.Storyteller?.incidentQueue?.Add(IncidentDef, GenDate.TicksPerDay, parms, GenDate.TicksPerDay * 3);

                nextIncidentTick = CurrentTick + Rand.RangeInclusive(1, 5) * GenDate.TicksPerDay;
            }
        }

        public FamiliaRankState GetOrAddState(FamiliaCategoryDef def)
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

        public FamiliaRankDef GetCurrentRank(FamiliaCategoryDef category) => GetOrAddState(category)?.curRankDef;

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

        public int GetUpgradePointsRequired(FamiliaCategoryDef category) => GetNextRank(category)?.upgradePointsRequired ?? 0;

        public bool CanRankUp(FamiliaCategoryDef category)
        {
            var next = GetUpgradePointsRequired(category);
            var cur = GetOrAddState(category)?.curRankUpPoints;
            if (cur == null) return false;
            return cur >= next;
        }

        public void GainPoints(FamiliaCategoryDef category, int amount)
        {
            if (category == null || amount <= 0) return;

            var state = GetOrAddState(category);
            state.curRankUpPoints += amount;

            var letter = LetterMaker.MakeLetter(
                "眷族成长",
                $"由于在「{category.LabelCap}」方面的卓越贡献\n" +
                $"您的眷族获得了 {amount} 点升级点数。\n\n" +
                $"当前：{state.curRankUpPoints} / {GetUpgradePointsRequired(category)}",
                LetterDefOf.PositiveEvent);
            Find.LetterStack.ReceiveLetter(letter);
        }

        private bool TrySpawnRankUpEnvoy(FamiliaCategoryDef category)
        {
            var state = GetOrAddState(category);
            if (!CanRankUp(category)) return false;       //未达到升级条件
            if (state.rankUpQuestID != -1) return false;  //已有任务

            var map = Find.AnyPlayerHomeMap ?? Find.CurrentMap;
            if (map == null) return false;

            //从该类别的可用强制任务中随机挑选
            var gens = category.GetAvailableRankUpQuests();
            if (gens.NullOrEmpty())
            {
                Log.Warning($"No Available Rank Up Quest For Category {category.label}");
                return false;
            }

            return StartRankUpQuest(gens.RandomElement(), map);
        }

        //让使者携带指定强制任务出场（轮询自动发布与 Debug 指定任务共用入口）
        //入口校验放在任何副作用之前，避免"使者已出场才发现参数不对"的残留
        public bool StartRankUpQuest(GuildQuestGenerator gen, Map map)
        {
            if (gen?.def == null || map == null)
            {
                Log.Error($"[FamiliaMyth] StartRankUpQuest 参数无效：gen={gen != null}, def={gen?.def != null}, map={map != null}");
                return false;
            }

            var state = GetOrAddState(gen.def.category);
            if (state == null)
            {
                Log.Error($"[FamiliaMyth] 强制任务 {gen.def.defName} 未指定有效分类（category），无法发布。");
                return false;
            }

            Faction guild = Find.FactionManager.FirstFactionOfDef(GuildDef);
            if (guild == null) return false;   //公会派系缺失（FinalizeInit 已补建，此处仅防御）

            //生成参数（统一在此调用，调用方无需重复）
            gen.SetupParams(map);

            var envoy = PawnGenerator.GeneratePawn(new PawnGenerationRequest(EnvoyDef, guild));

            //搜索进入点
            if (!CellFinder.TryFindRandomEdgeCellWith(
                c => map.reachability.CanReachColony(c),
                map, CellFinder.EdgeRoadChance_Neutral, out var entryCell))
            {
                Log.Warning("[FamiliaMyth] No Way To Colony For Guild Envoy");
                return false;
            }

            //搜索停留点
            if (!RCellFinder.TryFindRandomSpotJustOutsideColony(entryCell, map, out var chillSpot))
                chillSpot = DropCellFinder.TradeDropSpot(map);

            //生成使者
            GenSpawn.Spawn(envoy, entryCell, map);
            var job = new LordJob_VisitColony(guild, chillSpot, EnvoyTimeoutTicks);
            var lord = LordMaker.MakeNewLord(guild, job, map);
            lord.AddPawn(envoy);

            GameComponent_GuildEnvoyTracker.Register(envoy, gen);
            var quest = gen.SummonQuest(envoy, null);
            if (quest == null) return false;   //任务生成失败（Quest 脚本异常时静默，避免留下无任务使者）
            state.OnRankUpQuestStart(quest.id);

            TaggedString label = "公会使者到访";
            TaggedString text = $"来自冒险者公会的使者 {envoy.Name.ToStringShort} 已到达你的据点。\n\n" +
                "请派遣殖民者前往与使者对话，了解委托的详情。";
            ChoiceLetter letter = LetterMaker.MakeLetter(
                label, text, LetterDefOf.NeutralEvent, envoy);
            Find.LetterStack.ReceiveLetter(letter);

            return true;
        }
             
        public void OnRankUpRequestDone(FamiliaCategoryDef category)
        {
            var state = GetOrAddState(category);
            var next = GetNextRank(category);
            if (next == null || state ==  null) return;

            state.curRankDef = next;
            state.curRankUpPoints -= next.upgradePointsRequired;
            state.rankUpQuestID = -1;

            var letter = LetterMaker.MakeLetter(
                "眷族评级晋升",
                $"公会已将您的眷族在「{category.LabelCap}」方面的评级提升为「{next.LabelCap}」！",
                LetterDefOf.PositiveEvent);
            Find.LetterStack.ReceiveLetter(letter);
        }

        public void OnRankUpQuestFailed(FamiliaCategoryDef category)
        {
            var state = GetOrAddState(category);
            if (state == null) return;

            state.rankUpQuestID = -1;
            state.rankUpQuestRetryTick = CurrentTick + Rand.RangeInclusive(1, 5) * GenDate.TicksPerDay;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref rankStates, "rankStates", 
                keyLookMode: LookMode.Def,
                valueLookMode: LookMode.Deep);
            Scribe_Values.Look(ref nextIncidentTick, "nextIncidentTick", GenDate.TicksPerDay * 10);
        }
    }
}
