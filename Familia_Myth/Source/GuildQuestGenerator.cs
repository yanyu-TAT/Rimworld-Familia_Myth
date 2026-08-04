using FamiliaMyth.Familia;
using RimWorld;
using Verse;

namespace FamiliaMyth.Quest
{
    public abstract class GuildQuestGenerator : IExposable
    {
        public GuildQuestDef def;
        public QuestType Type => def?.type ?? QuestType.Daily;
        public string LetterText => def?.letterText;
        public FamiliaCategoryDef Category => def?.category;

        public Map map;

        private QuestScriptDef questScriptDef;
        public QuestScriptDef QuestScriptDef =>
            questScriptDef ??= DefDatabase<QuestScriptDef>.GetNamed(QuestScriptDefName);
        public abstract string QuestScriptDefName { get; }

        //计算并存储任务参数
        public abstract void SetupParams(Map map);

        //生成任务
        public abstract RimWorld.Quest SummonQuest(Pawn envoy, Pawn acceptor);

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_References.Look(ref map, "map");
        }
    }
}