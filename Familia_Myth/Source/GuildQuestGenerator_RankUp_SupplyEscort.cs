using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace FamiliaMyth.Quest
{
    public class GuildQuestGenerator_RankUp_SupplyEscort : GuildQuestGenerator
    {
        public override string QuestScriptDefName => "Quest_RankUp_SupplyEscort";

        public override void SetupParams(Map map)
        {
            this.map = map;
        }

        //发布预筛：世界层凑不出满足约束的 A/B 定居点组合时不发布该任务（静默，等下一轮轮询）
        //注意：此处无法依赖 this.map（调用点在 SetupParams 之前），故自行解析当前据点
        public override bool Available() =>
            SupplyEscortUtility.TryFindSettlementPair(
                Find.AnyPlayerHomeMap ?? Find.CurrentMap, out _, out _);

        public override RimWorld.Quest SummonQuest(Pawn envoy, Pawn acceptor)
        {
            var slate = new Slate();
            slate.Set("map", map);
            slate.Set("category", Category);
            return QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDef, slate);
        }
    }
}
