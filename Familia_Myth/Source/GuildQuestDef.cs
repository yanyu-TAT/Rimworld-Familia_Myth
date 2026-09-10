using FamiliaMyth.Familia;
using RimWorld;
using Verse;

namespace FamiliaMyth.Quest
{
    public class GuildQuestDef : Def
    {
        public GuildQuestGenerator generator;

        //仅供读取 def 级元数据（Script 等）：本实例是缓存单例且带 map 运行时状态，
        //**不可当作任务生成器复用**（多任务共享会互相覆盖 map）；生成任务请用 GuildQuestGenerator.Create(this)
        public GuildQuestGenerator Generator => generator ??= GuildQuestGenerator.Create(this);

        public FamiliaCategoryDef category;
        public QuestScriptDef Script => Generator.QuestScriptDef;
        public QuestType type;
        public Type generatorClass;

        [MustTranslate]
        public string letterText;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var s in base.ConfigErrors())
                yield return s;

            if (Script == null)
                yield return $"Invalid Quest Script For Quest {defName}";

            if (category == null)
                yield return $"Invalid Category For Quest {defName}";

            string ss = "";
            GuildQuestGenerator gen = null;
            try
            {
                gen = Generator;
            } catch (Exception e)
            {
                ss = $"Invalid Generator For Quest {defName} : {e.ToString()}";
            }

            if (gen == null)
            {
                yield return ss;
            }
        }
    }

    public enum QuestType
    {
        Daily,
        RankUp
    }
}
