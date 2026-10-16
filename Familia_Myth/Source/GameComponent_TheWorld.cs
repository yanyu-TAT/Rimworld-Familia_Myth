using Verse;

namespace FamiliaMyth.Abilities
{
    public class GameComponent_TheWorld : GameComponent
    {
        public static GameComponent_TheWorld Instance => Current.Game?.GetComponent<GameComponent_TheWorld>();

        public Dictionary<Pawn, IntVec3> fields = new();
        public List<Pawn> pawns = new();
        public List<IntVec3> cells = new();

        public GameComponent_TheWorld(Game game) { }
        public GameComponent_TheWorld() { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref fields, "fields",
                keyLookMode: LookMode.Reference, valueLookMode: LookMode.Value,
                ref pawns, ref cells);  //必须传入可跨方法缓存的列表引用，否则会导致解析时报错
        }
    }
}
