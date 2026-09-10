using FamiliaMyth.Quest;
using Verse;
using Verse.AI.Group;

namespace FamiliaMyth.Incidents
{
    public class GameComponent_GuildEnvoyTracker : GameComponent
    {
        public static GameComponent_GuildEnvoyTracker Instance => Current.Game?.GetComponent<GameComponent_GuildEnvoyTracker>();

        //当前尚未对话过的使者及对应的任务生成器
        private Pawn curEnvoy;
        private GuildQuestGenerator curQuestGen;

        public static Pawn Envoy => Instance?.curEnvoy;
        public static GuildQuestGenerator QuestGenerator => Instance?.curQuestGen;

        public GameComponent_GuildEnvoyTracker() { }

        public GameComponent_GuildEnvoyTracker(Game game) { }

        public static bool HasEnvoy =>  QuestGenerator != null && Envoy != null && Envoy.Spawned && !Envoy.Dead;

        public static void Register(Pawn envoy, GuildQuestGenerator generator)
        {
            if (Instance == null || envoy == null || generator == null) return;

            if (Envoy != null && Envoy.Spawned && !Envoy.Dead)
                TryLeave(Envoy);

            Instance.curEnvoy = envoy;
            Instance.curQuestGen = generator;
        }


        public static void RemoveEnvoy()
        {
            if (Instance == null) return;

            if (Envoy != null && Envoy.Spawned && !Envoy.Dead)
                TryLeave(Envoy);

            Instance.curEnvoy = null;
            Instance.curQuestGen = null;
        }

        private static void TryLeave(Pawn envoy)
        {
            var lord = envoy.GetLord();
            envoy.Map?.lordManager?.RemoveLord(lord);
            var job = new LordJob_ExitMapBest();
            var newLord = LordMaker.MakeNewLord(envoy.Faction, job, envoy.Map);
            newLord.AddPawn(envoy);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref curEnvoy, "curEnvoy");
            Scribe_Deep.Look(ref curQuestGen, "curQuestGen");
        }
    }
}
