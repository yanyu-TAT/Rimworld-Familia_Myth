using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    //强制任务「难民安置」生成器：走使者出场自动接取（RankUp 轨），slate 只需 map 与 category
    public class GuildQuestGenerator_RankUp_RefugeeResettle : GuildQuestGenerator
    {
        public override string QuestScriptDefName => "Quest_RankUp_RefugeeResettle";

        public override void SetupParams(Map map)
        {
            this.map = map;
        }

        //发布预筛：需要一张玩家据点地图（难民在据点边缘到访，无站点需求）
        public override bool Available() =>
            (Find.AnyPlayerHomeMap ?? Find.CurrentMap) != null;

        public override RimWorld.Quest SummonQuest(Pawn envoy, Pawn acceptor)
        {
            var slate = new Slate();
            slate.Set("map", map ?? Find.AnyPlayerHomeMap ?? Find.CurrentMap);
            slate.Set("category", Category);
            return QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDef, slate);
        }
    }
}
