using FamiliaMyth.Quest;
using Verse;

namespace FamiliaMyth.Incidents
{
    public class GameComponent_GuildEnvoyTracker : GameComponent
    {
        public static GameComponent_GuildEnvoyTracker Instance => Current.Game?.GetComponent<GameComponent_GuildEnvoyTracker>();


        private Dictionary<int, GuildQuestGenerator> envoys = new();
        private List<Pawn> notSpoken = new();

        public static Dictionary<int, GuildQuestGenerator> Envoys => Instance?.envoys ?? new();
        public static List<Pawn> NotSpoken => Instance?.notSpoken ?? new();

        public GameComponent_GuildEnvoyTracker() { }

        public GameComponent_GuildEnvoyTracker(Game game) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref envoys, "envoys",
                keyLookMode: LookMode.Value,
                valueLookMode: LookMode.Deep);

            Scribe_Collections.Look(ref notSpoken, "notSpoken",
                LookMode.Reference);
        }
    }
}
