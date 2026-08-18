using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    
    [HarmonyPatch(typeof(JobDriver_Meditate), "MeditationTick")]
    public static class Patch_Meditate_Will
    {
        //冥想0.0008/tick
        private const float WillPerTick = 2f / 2500f;

        public static void Postfix(JobDriver_Meditate __instance)
        {
            __instance.pawn.GetPharna()?.GainStatus(PharnaStatusType.Will, WillPerTick);
        }
    }

    
    [HarmonyPatch(typeof(InspirationHandler), "TryStartInspiration")]
    public static class Patch_Inspiration_Will
    {
        public static void Postfix(InspirationHandler __instance, ref bool __result)
        {
            if (__result)
            {
                //灵感每次10点
                __instance.pawn.GetPharna()?.GainStatus(PharnaStatusType.Will, 10f);
            }
        }
    }

    
    [HarmonyPatch(typeof(HistoryEventsManager), "RecordEvent")]
    public static class Patch_Charity_Will
    {
        public static void Postfix(HistoryEvent historyEvent)
        {
            if (historyEvent.def == null || !historyEvent.def.defName.StartsWith("CharityFulfilled_"))
            {
                return;
            }
            foreach (var colonist in Find.CurrentMap.mapPawns.FreeColonistsSpawned)
            {
                //仁善每次全员20点
                colonist.GetPharna()?.GainStatus(PharnaStatusType.Will, 20f);
            }
        }
    }

    
    [HarmonyPatch(typeof(Pawn_InteractionsTracker), "TryInteractWith")]
    public static class Patch_Interaction_Will
    {
        private static readonly AccessTools.FieldRef<Pawn_InteractionsTracker, Pawn> PawnRef =
            AccessTools.FieldRefAccess<Pawn_InteractionsTracker, Pawn>("pawn");

        public static void Postfix(Pawn_InteractionsTracker __instance, Pawn recipient, InteractionDef intDef)
        {
            if (intDef == null || !(intDef.defName == "DeepTalk" || intDef.defName == "KindWords"))
            {
                return;
            }
            //深入交流/美言每次双方2点
            PawnRef(__instance).GetPharna()?.GainStatus(PharnaStatusType.Will, 2f);
            recipient.GetPharna()?.GainStatus(PharnaStatusType.Will, 2f);
        }
    }
}
