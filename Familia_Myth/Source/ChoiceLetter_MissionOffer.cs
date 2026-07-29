using FamiliaMyth.Pharna;
using RimWorld;
using RimWorld.QuestGen;
using Verse;
using Verse.AI.Group;

namespace FamiliaMyth.Incidents
{
    public class ChoiceLetter_MissionOffer : ChoiceLetter
    {
        public Pawn envoy;

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
                var acceptionOption = new DiaOption("接受委托")
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
                                if (Hediff_Pharna.GetPharna(pawn) == null) continue;
                                options.Add(new FloatMenuOption(pawn.LabelCap, () =>
                                {
                                    CreateQuest(pawn);
                                    Find.LetterStack.RemoveLetter(this);
                                    Find.WindowStack.Windows.FirstOrDefault(l => l is Dialog_NodeTreeWithFactionInfo)?.Close();
                                    EnvoyLeave();
                                }));
                            }

                            if (!options.Any())
                            {
                                var option = new FloatMenuOption("没有已有恩惠的殖民者", null)
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
                var rejectOption = new DiaOption("拒绝委托")
                {
                    action = () =>
                    {
                        if (envoy.Spawned)
                        {
                            // 使者直接离开
                            if (IncidentWorker_GuildEnvoy.Envoys.ContainsKey(envoy.thingIDNumber))
                                IncidentWorker_GuildEnvoy.Envoys.Remove(envoy.thingIDNumber);
                            Find.LetterStack.RemoveLetter(this);
                            EnvoyLeave();
                        }
                    },
                    resolveTree = true
                };
                yield return rejectOption;

                //了解更多
                var infoOption = new DiaOption("了解更多")
                {
                    action = () =>
                    {
                        var text = "冒险者公会是最初降临的神明所建立的组织，负责转交和发布委托。\n\n" +
                            "接受委托后，公会将在你的据点附近标记目标。\n" +
                            "带领冒险者前去清剿所有敌人以完成委托。\n\n" +
                            "提示：\n" +
                            "完成委托可以获取升华恩惠所需要的伟业值。\n" +
                            "由接取者获得50%的伟业值，\n" +
                            "其余部分将均分给队伍中其他的冒险者。";
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

        private void EnvoyLeave()
        {
            var lord = envoy.GetLord();
            envoy.Map?.lordManager?.RemoveLord(lord);

            // 创建新的离开 Lord
            var exitJob = new LordJob_ExitMapBest();
            var newLord = LordMaker.MakeNewLord(envoy.Faction, exitJob, envoy.Map);
            newLord.AddPawn(envoy);
        }

        private void CreateQuest(Pawn acceptor)
        {
            if (acceptor == null || envoy == null) return;

            if (!IncidentWorker_GuildEnvoy.Envoys.TryGetValue(envoy.thingIDNumber, out MissionParams missionParams))
            {
                Log.Error($"[FamiliaMyth] Can't Find Corresponding MissionParams For {envoy.Name.ToStringShort}");
                return;
            }

            Slate slate = new();
            slate.Set("map", missionParams.map);
            slate.Set("points", missionParams.points);
            slate.Set("siteFaction", missionParams.siteFaction);
            slate.Set("sitePartsParams", missionParams.sitePartsParams);
            slate.Set("envoy", envoy);
            slate.Set("acceptor", acceptor);
            slate.Set("initialAcceptor", acceptor);

            var quest = QuestUtility.GenerateQuestAndMakeAvailable(DefDatabase<QuestScriptDef>.GetNamed("Quest_Mission"), slate);
            if (quest != null)
            {
                // 移除使者的映射（任务已创建）
                IncidentWorker_GuildEnvoy.Envoys.Remove(envoy.thingIDNumber);
                // 将新Quest关联到当前信件
                this.quest = quest;
            }
        }
    }
}