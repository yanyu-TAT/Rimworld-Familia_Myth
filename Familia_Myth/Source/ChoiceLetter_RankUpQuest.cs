using FamiliaMyth.Familia;
using FamiliaMyth.Incidents;
using FamiliaMyth.Quest;
using RimWorld;
using Verse;
using Verse.AI.Group;

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

                yield return new("接受强制任务")
                {
                    action = () =>
                    {
                        if (!envoy.Spawned) return;
                        CreateRankUpQuest();
                        Find.LetterStack.RemoveLetter(this);
                        EnvoyLeave();
                    },
                    resolveTree = true
                };

                yield return new("了解更多")
                {
                    action = () =>
                    {
                        var tracker = GameComponent_FamiliaRankTracker.Instance;
                        var text = $"对于在某一方面有所建树的眷族，公会会发布强制任务以检验其实力。\n" +
                            $"该任务不可拒绝，若超时或失败则会冻结升级点数增长一段时间。\n\n" +
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

        private void EnvoyLeave()
        {
            var lord = envoy.GetLord();
            envoy.Map?.lordManager?.RemoveLord(lord);
            var job = new LordJob_ExitMapBest();
            var newLord = LordMaker.MakeNewLord(envoy.Faction, job, envoy.Map);
            newLord.AddPawn(envoy);
        }

        private void CreateRankUpQuest()
        {
            if (!GameComponent_GuildEnvoyTracker.Envoys.TryGetValue(envoy.thingIDNumber, out var gen)) return;

            var quest = gen.SummonQuest(envoy, null);
            if (quest != null)
            {
                this.quest = quest;
                GameComponent_FamiliaRankTracker.Instance?.GetState(gen.Category)?.OnRankUpQuestStart(quest.id);
            }
        }
    }
}
