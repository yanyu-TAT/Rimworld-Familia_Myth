using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    public class GuildQuestGenerator_Daily_Rescue : GuildQuestGenerator
    {
        public override string QuestScriptDefName => "Quest_Rescue";

        public override void SetupParams(Map map) { this.map = map; }

        public override RimWorld.Quest SummonQuest(Pawn envoy, Pawn acceptor)
        {
            var slate = new Slate();
            slate.Set("map", map);
            slate.Set("envoy", envoy);
            slate.Set("acceptor", acceptor);
            slate.Set("category", Category);
            return QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDef, slate);
        }
    }
}
