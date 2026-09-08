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
        [DebugAction("Familia Myth", "Spawn Envoy With Quest",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TriggerGuildEnvoy()
        {
            Map map = Find.CurrentMap;
            if (map == null) return;

            //列出全部日常任务，指定使者携带其一出场
            var options = new List<DebugMenuOption>();
            foreach (var questDef in DefDatabase<GuildQuestDef>.AllDefs
                         .Where(d => d.type == QuestType.Daily)
                         .OrderBy(d => d.defName))
            {
                string catLabel = questDef.category?.label ?? "无分类";
                options.Add(new DebugMenuOption(
                    $"{questDef.label}（{catLabel}）<{questDef.defName}>",
                    DebugMenuOptionMode.Action,
                    () =>
                    {
                        if (!IncidentWorker_GuildEnvoy.SpawnEnvoyWithQuest(questDef, map))
                            Messages.Message(
                                "无法让使者携带该任务出场（任务或使者生成失败）。",
                                MessageTypeDefOf.RejectInput,
                                historical: false);
                    }));
            }

            if (options.Count == 0)
            {
                Log.Warning("[FamiliaMyth] No Daily GuildQuestDef available for debug envoy.");
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
