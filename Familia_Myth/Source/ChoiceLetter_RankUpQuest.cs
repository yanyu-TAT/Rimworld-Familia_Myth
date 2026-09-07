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

                yield return new("确认")
                {
                    action = () =>
                    {
                        if (!envoy.Spawned) return;
                        GameComponent_GuildEnvoyTracker.RemoveEnvoy();
                        Find.LetterStack.RemoveLetter(this);
                    },
                    resolveTree = true
                };

                yield return new("了解更多")
                {
                    action = () =>
                    {
                        var tracker = GameComponent_FamiliaRankTracker.Instance;
                        var text = $"对于在某一方面有所建树的眷族，公会会发布强制任务以检验其实力。\n" +
                            $"该任务不可拒绝，若超时或失败则会在一段时间后重新发布。\n\n" +
                            $"请注意，该任务难度较高，请尽量派出全部战力。\n" +
                            $"完成任务即可提升眷族在「{Category.LabelCap}」方面的评级。\n" +
                            $"当然也能获取大量伟业值，将平均分配给全部眷族成员。";
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
