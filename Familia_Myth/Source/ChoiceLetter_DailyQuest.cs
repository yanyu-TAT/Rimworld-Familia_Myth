using FamiliaMyth.Pharna;
using FamiliaMyth.Quest;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace FamiliaMyth.Incidents
{
    public class ChoiceLetter_DailyQuest : ChoiceLetter, IQuestProvider
    {
        public Pawn envoy;
        public GuildQuestDef questDef;

        public override bool CanDismissWithRightClick => false;

        public override bool CanShowInLetterStack
        {
            get
            {
                if (!base.CanShowInLetterStack) return false;
                if (quest == null) return envoy != null && envoy.Spawned;
                if (quest.State == QuestState.Ongoing) return true; 
                return quest.State == QuestState.NotYetAccepted;
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

                //接受委托
                var acceptionOption = new DiaOption("FamiliaMyth.QuestLetter.Accept".Translate())
                {
                    action = () =>
                    {
                        if (envoy.Spawned)
                        {
                            List<FloatMenuOption> options = new();
                            var colonists = Find.CurrentMap.mapPawns.AllPawnsSpawned
                                .Where(p => p.IsColonist && p.Faction == Faction.OfPlayer);
                            foreach (var pawn in colonists)
                            {
                                if (pawn.GetPharna() == null) continue;
                                options.Add(new FloatMenuOption(pawn.LabelCap, () =>
                                {
                                    CreateQuest(pawn);
                                    Find.LetterStack.RemoveLetter(this);
                                    Find.WindowStack.Windows.FirstOrDefault(l => l is Dialog_NodeTreeWithFactionInfo)?.Close();
                                    GameComponent_GuildEnvoyTracker.RemoveEnvoy();
                                }));
                            }

                            if (!options.Any())
                            {
                                var option = new FloatMenuOption("FamiliaMyth.QuestLetter.NoPharnaColonist".Translate(), null)
                                {
                                    Disabled = true
                                };
                                options.Add(option);
                            }

                            Find.WindowStack.Add(new FloatMenu(options));
                        }
                    },
                    resolveTree = false
                };
                yield return acceptionOption;

                //拒绝委托
                var rejectOption = new DiaOption("FamiliaMyth.QuestLetter.Reject".Translate())
                {
                    action = () =>
                    {
                        if (envoy.Spawned)
                        {
                            //使者直接离开
                            GameComponent_GuildEnvoyTracker.RemoveEnvoy();
                            Find.LetterStack.RemoveLetter(this);
                        }
                    },
                    resolveTree = true
                };
                yield return rejectOption;

                //了解更多
                var infoOption = new DiaOption("FamiliaMyth.QuestLetter.LearnMore".Translate())
                {
                    action = () =>
                    {
                        var text = "FamiliaMyth.QuestLetter.InfoText".Translate();
                        Find.WindowStack.Add(new Dialog_MessageBox(text));
                    },
                    resolveTree = false
                };
                yield return infoOption;

                //推迟
                if (lookTargets.IsValid())
                {
                    yield return Option_JumpToLocationAndPostpone;
                    yield break;
                }

                yield return Option_Postpone;
            }
        }

        public Pawn Envoy { get => envoy; set => envoy = value; }

        public GuildQuestDef QuestDef{ get => questDef; set => questDef = value; }

        private void CreateQuest(Pawn acceptor)
        {
            if (acceptor == null || envoy == null) return;

            var gen = GameComponent_GuildEnvoyTracker.QuestGenerator;
            if (gen == null)
            {
                Log.Error($"[FamiliaMyth] Can't Find Corresponding Generator For {envoy.Name.ToStringShort}");
                return;
            }
            if (gen.def == null)
            {
                Log.Error($"[FamiliaMyth] Generator {gen.GetType().Name} 未绑定 GuildQuestDef，无法生成委托。");
                return;
            }

            var quest = gen.SummonQuest(envoy, acceptor);
            this.quest = quest;
            //移除使者
            GameComponent_GuildEnvoyTracker.RemoveEnvoy();
        }
    }
}