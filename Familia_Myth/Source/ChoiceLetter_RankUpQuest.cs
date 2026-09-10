using FamiliaMyth.Familia;
using FamiliaMyth.Incidents;
using FamiliaMyth.Quest;
using RimWorld;
using Verse;

namespace Familia_Myth.Source
{
    public class ChoiceLetter_RankUpQuest : ChoiceLetter, IQuestProvider
    {
        public Pawn envoy;

        public GuildQuestDef questDef;
        public FamiliaCategoryDef Category => questDef?.category;

        public override bool CanDismissWithRightClick => false;

        public override bool CanShowInLetterStack
        {
            get
            {
                if (!base.CanShowInLetterStack) return false;
                if (quest == null) return envoy != null && envoy.Spawned;
                return quest.State == QuestState.Ongoing || quest.State == QuestState.NotYetAccepted;
            }
        }

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly || envoy == null || !envoy.Spawned)
                {
                    yield return Option_Close;
                    yield break;
                }

                yield return new("FamiliaMyth.QuestMisc.RankUpQuestConfirm".Translate())
                {
                    action = () =>
                    {
                        if (!envoy.Spawned) return;
                        GameComponent_GuildEnvoyTracker.RemoveEnvoy();
                        Find.LetterStack.RemoveLetter(this);
                    },
                    resolveTree = true
                };

                yield return new("FamiliaMyth.QuestMisc.RankUpQuestLearnMore".Translate())
                {
                    action = () =>
                    {
                        var tracker = GameComponent_FamiliaRankTracker.Instance;
                        var text = "FamiliaMyth.QuestMisc.RankUpQuestLearnMoreText".Translate(Category.LabelCap);
                        Find.WindowStack.Add(new Dialog_MessageBox(text));
                    },
                    resolveTree = false
                };

                if (lookTargets.IsValid)
                {
                    yield return Option_JumpToLocationAndPostpone;
                    yield break;
                }
                yield return Option_Postpone;
            }
        }

        public Pawn Envoy { get => envoy; set => envoy = value; }
        public GuildQuestDef QuestDef { get => questDef; set => questDef = value; }
    }
}
