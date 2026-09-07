using FamiliaMyth.Familia;
using LudeonTK;
using RimWorld;
using Verse;

namespace FamiliaMyth.Debug
{
    public static class DebugAction_Quest
    {
        [DebugAction("Familia Myth", "Trigger Quest Daily", 
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TriggerGuildEnvoy()
        {
            Map map = Find.CurrentMap;
            if (map == null) return;

            var def = IncidentDef.Named("GiveQuest_Daily");
            var parms = StorytellerUtility.DefaultParmsNow(def.category, map);
            def.Worker.TryExecute(parms);
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