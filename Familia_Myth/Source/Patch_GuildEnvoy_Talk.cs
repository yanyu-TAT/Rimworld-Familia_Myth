using FamiliaMyth.Incidents;
using FamiliaMyth.Quest;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetFloatMenuOptions))]
    public static class Patch_GuildEnvoy_Talk
    {
        static void Postfix(Pawn __instance, ref IEnumerable<FloatMenuOption> __result, Pawn selPawn)
        {
            try
            {
                if (__instance.kindDef?.defName != "Guild_Envoy") return;
                if (selPawn == null || selPawn.Faction != Faction.OfPlayer) return;
                if (!GameComponent_GuildEnvoyTracker.HasEnvoy) return;
                if (__result == null) return;

                var list = __result.ToList();

                if (!selPawn.CanReach(__instance, PathEndMode.OnCell, Danger.Deadly))
                {
                    list.Add(new FloatMenuOption("FamiliaMyth.QuestMisc.CannotTalkPathUnreachable".Translate(), null));
                }
                else if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
                {
                    list.Add(new FloatMenuOption("FamiliaMyth.QuestMisc.CannotTalkNoSpeech".Translate(), null));
                }
                else
                {
                    list.Add(FloatMenuUtility.DecoratePrioritizedTask(
                        new FloatMenuOption(("FamiliaMyth.QuestMisc.TalkToEnvoy".Translate()), () =>
                        {
                            Job job = JobMaker.MakeJob(
                                DefDatabase<JobDef>.GetNamed("TalkGuildEnvoy"),
                                __instance);
                            job.playerForced = true;
                            selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        }), selPawn, __instance));
                }

                __result = list;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_GuildEnvoy_Talk)} error: {ex}");
            }
        }
    }
}