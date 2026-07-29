using Verse;

namespace FamiliaMyth.Incidents
{
    public class GameComponent_GuildEnvoyTracker : GameComponent
    {
        public Dictionary<int, MissionParams> envoys = new();
        public List<Pawn> notSpoken = new();

        public GameComponent_GuildEnvoyTracker()
        {
        }

        public GameComponent_GuildEnvoyTracker(Game game)
        {
        }

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
