using FamiliaMyth.Incidents;
using FamiliaMyth.Quest;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetFloatMenuOptions))]
    public static class Patch_Pawn_FloatMenu
    {
        static void Postfix(Pawn __instance, ref IEnumerable<FloatMenuOption> __result, Pawn selPawn)
        {
            if (__instance.kindDef?.defName != "Guild_Envoy") return;
            if (selPawn == null || selPawn.Faction != Faction.OfPlayer) return;
            if (!GameComponent_GuildEnvoyTracker.HasEnvoy) return;

            var list = __result.ToList();

            if (!selPawn.CanReach(__instance, PathEndMode.OnCell, Danger.Deadly))
            {
                list.Add(new FloatMenuOption("无法对话：路径不可达", null));
            }
            else if (!selPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                list.Add(new FloatMenuOption("无法对话：无语言能力", null));
            }
            else
            {
                list.Add(FloatMenuUtility.DecoratePrioritizedTask(
                    new FloatMenuOption(("与公会使者对话"), () =>
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
    }
}