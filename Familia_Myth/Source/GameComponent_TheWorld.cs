using Verse;

namespace FamiliaMyth.Abilities
{
    public class GameComponent_TheWorld : GameComponent
    {
        public static GameComponent_TheWorld Instance => Current.Game?.GetComponent<GameComponent_TheWorld>();

        public Dictionary<Pawn, IntVec3> fields = new();

        public GameComponent_TheWorld(Game game) { }
        public GameComponent_TheWorld() { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref fields, "fields",
                keyLookMode: LookMode.Reference, valueLookMode: LookMode.Value);
        }
    }
}
