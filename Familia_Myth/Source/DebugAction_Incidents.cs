using LudeonTK;
using RimWorld;
using Verse;

namespace FamiliaMyth.Debug
{
    public static class DebugAction_Incidents
    {
        [DebugAction("Familia Myth", "Trigger Guild Envoy", 
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TriggerGuildEnvoy()
        {
            Map map = Find.CurrentMap;
            if (map == null) return;

            var def = IncidentDef.Named("GiveQuest_Mission");
            var parms = StorytellerUtility.DefaultParmsNow(def.category, map);
            def.Worker.TryExecute(parms);
        }
    }
}