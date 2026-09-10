using System.Linq;
using FamiliaMyth.Familia;
using FamiliaMyth.Incidents;
using FamiliaMyth.Quest;
using LudeonTK;
using RimWorld;
using Verse;

namespace FamiliaMyth.Debug
{
    public static class DebugAction_Quest
    {
        [DebugAction("Familia Myth", "Spawn Envoy With Daily Quest",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TriggerGuildEnvoyDaily()
        {
            Map map = Find.CurrentMap;
            if (map == null) return;

            //列出全部日常任务，指定使者携带其一出场
            //（不可用的仅标注，仍允许强制触发——调试期正需要跨过预筛做验证）
            var options = new List<DebugMenuOption>();
            foreach (var questDef in DefDatabase<GuildQuestDef>.AllDefs
                         .Where(d => d.type == QuestType.Daily)
                         .OrderBy(d => d.defName))
            {
                string catLabel = questDef.category?.label ?? "无分类";
                bool available = GuildQuestGenerator.Create(questDef)?.Available() ?? false;

                options.Add(new DebugMenuOption(
                    $"{(available ? "" : "【不可用】")}{questDef.label}（{catLabel}）<{questDef.defName}>",
                    DebugMenuOptionMode.Action,
                    () =>
                    {
                        var gen = GuildQuestGenerator.Create(questDef);
                        if (gen == null)
                        {
                            Messages.Message($"任务生成器无效：{questDef.defName}",
                                MessageTypeDefOf.RejectInput, false);
                            return;
                        }
                        if (!available)
                            Log.Warning($"[FamiliaMyth] 调试强制触发不可用的日常任务：{questDef.defName}");

                        if (!IncidentWorker_GuildEnvoy.SpawnEnvoyWithQuest(gen, map))
                            Messages.Message(
                                "无法让使者携带该任务出场（任务或使者生成失败）。",
                                MessageTypeDefOf.RejectInput,
                                historical: false);
                    }));
            }

            if (options.Count == 0)
            {
                Log.Warning("[FamiliaMyth] No Daily GuildQuestDef Available.");
                return;
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        [DebugAction("Familia Myth", "Spawn Envoy With Rank Up Quest",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TriggerGuildEnvoyRankUp()
        {
            Map map = Find.CurrentMap;
            if (map == null) return;

            var tracker = GameComponent_FamiliaRankTracker.Instance;
            if (tracker == null) return;

            //列出全部强制任务，指定使者携带其一出场
            //（不可用的仅标注，仍允许强制触发——调试期正需要跨过预筛做验证）
            var options = new List<DebugMenuOption>();
            foreach (var questDef in DefDatabase<GuildQuestDef>.AllDefs
                         .Where(d => d.type == QuestType.RankUp)
                         .OrderBy(d => d.defName))
            {
                string catLabel = questDef.category?.label ?? "无分类";
                bool available = GuildQuestGenerator.Create(questDef)?.Available() ?? false;

                options.Add(new DebugMenuOption(
                    $"{(available ? "" : "【不可用】")}{questDef.label}（{catLabel}）<{questDef.defName}>",
                    DebugMenuOptionMode.Action,
                    () =>
                    {
                        var gen = GuildQuestGenerator.Create(questDef);
                        if (gen == null)
                        {
                            Messages.Message($"任务生成器无效：{questDef.defName}",
                                MessageTypeDefOf.RejectInput, false);
                            return;
                        }
                        if (!available)
                            Log.Warning($"[FamiliaMyth] 调试强制触发不可用的强制任务：{questDef.defName}");

                        if (!tracker.StartRankUpQuest(gen, map))
                            Messages.Message(
                                "无法让使者携带该强制任务出场（任务或使者生成失败）。",
                                MessageTypeDefOf.RejectInput,
                                historical: false);
                    }));
            }

            if (options.Count == 0)
            {
                Log.Warning("[FamiliaMyth] No Rank Up GuildQuestDef Available.");
                return;
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        [DebugAction("Familia Myth", "Gain 100 Upgrade Point For",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.Playing)]
        public static void GainUpgradePoint()
        {
            var tracker = GameComponent_FamiliaRankTracker.Instance;
            if (tracker == null) return;

            var options = new List<DebugMenuOption>();
            foreach (var cat in DefDatabase<FamiliaCategoryDef>.AllDefs)
            {
                options.Add(new DebugMenuOption(
                    cat.label,
                    DebugMenuOptionMode.Action,
                    () =>
                    {
                        tracker.GainPoints(cat, 100);
                    }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }
    }
}
