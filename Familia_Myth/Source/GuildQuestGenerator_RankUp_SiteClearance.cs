using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    public class GuildQuestGenerator_RankUP_SiteClearance : GuildQuestGenerator
    {
        public override string QuestScriptDefName => "Quest_RankUp_SiteClearance";

        public override void SetupParams(Map map)
        {
            this.map = map;
            //难度由在强制任务根节点中计算
        }

        public override RimWorld.Quest SummonQuest(Pawn envoy, Pawn acceptor)
        {
            Slate slate = new();
            slate.Set("map", map);
            slate.Set("category", Category);
            return QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDef, slate);
        }
    }
}