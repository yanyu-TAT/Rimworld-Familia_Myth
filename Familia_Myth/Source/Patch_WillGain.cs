using FamiliaMyth.Pharna;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(JobDriver_Meditate), "MeditationTick")]
    public static class Patch_WillGain_Meditate
    {
        //冥想0.0008/tick
        private const float WillPerTick = 2f / 2500f;

        public static void Postfix(JobDriver_Meditate __instance)
        {
            try
            {
                __instance.pawn.GetPharna()?.GainStatus(PharnaStatusType.Will, WillPerTick);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_WillGain_Meditate)} error: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(InspirationHandler), nameof(InspirationHandler.TryStartInspiration))]
    public static class Patch_WillGain_Inspiration
    {
        public static void Postfix(InspirationHandler __instance, ref bool __result)
        {
            try
            {
                if (__result)
                {
                    //灵感每次10点
                    __instance.pawn.GetPharna()?.GainStatus(PharnaStatusType.Will, 10f);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_WillGain_Inspiration)} error: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(HistoryEventsManager), nameof(HistoryEventsManager.RecordEvent))]
    public static class Patch_WillGain_Charity
    {
        public static void Postfix(HistoryEvent historyEvent)
        {
            try
            {
                if (historyEvent.def == null || !historyEvent.def.defName.StartsWith("CharityFulfilled_"))
                {
                    return;
                }
                var map = Find.CurrentMap;
                if (map == null) return; //无当前地图（世界层/读档期）时不结算
                foreach (var colonist in map.mapPawns.FreeColonistsSpawned)
                {
                    //仁善每次全员20点
                    colonist.GetPharna()?.GainStatus(PharnaStatusType.Will, 20f);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_WillGain_Charity)} error: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_InteractionsTracker), nameof(Pawn_InteractionsTracker.TryInteractWith))]
    public static class Patch_WillGain_Interaction
    {
        private static readonly AccessTools.FieldRef<Pawn_InteractionsTracker, Pawn> PawnRef =
            AccessTools.FieldRefAccess<Pawn_InteractionsTracker, Pawn>("pawn");

        public static void Postfix(Pawn_InteractionsTracker __instance, Pawn recipient, InteractionDef intDef)
        {
            try
            {
                if (intDef == null || !(intDef.defName == "DeepTalk" || intDef.defName == "KindWords"))
                {
                    return;
                }
                //深入交流/美言每次双方2点
                PawnRef(__instance).GetPharna()?.GainStatus(PharnaStatusType.Will, 2f);
                recipient.GetPharna()?.GainStatus(PharnaStatusType.Will, 2f);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_WillGain_Interaction)} error: {ex}");
            }
        }
    }
}
