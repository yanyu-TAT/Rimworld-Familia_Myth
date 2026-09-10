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

        public virtual bool Available() => true;

        //统一生成入口：创建实例并绑定 def。
        //外部（分类筛选 / 调试菜单）必须走这里——漏绑 def 会导致 Category/Type/LetterText 丢失，
        //并在 GameComponent_FamiliaRankTracker.StartRankUpQuest 里对 def.category 抛 NRE。
        public static GuildQuestGenerator Create(GuildQuestDef def)
        {
            if (def?.generatorClass == null)
            {
                Log.Error($"[FamiliaMyth] GuildQuestDef {(def == null ? "null" : def.defName)} 未指定 generatorClass。");
                return null;
            }

            var gen = (GuildQuestGenerator)Activator.CreateInstance(def.generatorClass);
            gen.def = def;
            return gen;
        }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_References.Look(ref map, "map");
        }
    }
}