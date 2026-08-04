using FamiliaMyth.Familia;
using RimWorld;
using Verse;

namespace FamiliaMyth.Quest
{
    public class GuildQuestDef : Def
    {
        public GuildQuestGenerator generator;
        public GuildQuestGenerator Generator =>
            generator ??= (GuildQuestGenerator)Activator.CreateInstance(generatorClass);

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
        }
    }

    public enum QuestType
    {
        Daily,
        RankUp
    }
}
