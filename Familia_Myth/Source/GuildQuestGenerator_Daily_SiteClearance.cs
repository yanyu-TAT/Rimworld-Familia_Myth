using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    public class GuildQuestGenerator_Daily_SiteClearance : GuildQuestGenerator
    {
        public Faction siteFaction;

        public override string QuestScriptDefName => "Quest_Daily_SiteClearance";

        public override void SetupParams(Map map)
        {
            this.map = map;

            //随机敌对派系
            siteFaction = Find.FactionManager.RandomEnemyFaction(
                allowHidden: false,
                allowNonHumanlike: true,
                allowDefeated: false);
        }

        public override RimWorld.Quest SummonQuest(Pawn envoy, Pawn acceptor)
        {
            Slate slate = new();
            slate.Set("map", map);
            slate.Set("siteFaction", siteFaction);
            slate.Set("envoy", envoy);
            slate.Set("acceptor", acceptor);
            slate.Set("initialAcceptor", acceptor);
            slate.Set("category", Category);
            return QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDef, slate);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref siteFaction, "siteFaction");
        }
    }
}