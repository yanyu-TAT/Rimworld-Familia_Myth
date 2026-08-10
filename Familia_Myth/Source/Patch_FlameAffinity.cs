using FamiliaMyth.Abilities;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(ThinkNode_ConditionalBurning), "Satisfied")]
    public static class Patch_ThinkNode_ConditionalBurning
    {
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (!__result) return;
            //着火时阻止失控行为
            if (Skill_FlameAffinity.HasFlameAffinity(pawn))
                __result = false;
        }
    }

    [HarmonyPatch("WorkGiver_FightFires", "HasJobOnThing")]
    public static class Patch_WorkGiver_FightFires
    {
        //着火时阻止其他pawn自动进行灭火；玩家强制命令（forced=true）不受影响
        public static bool Prefix(Thing t, bool forced, ref bool __result)
        {
            if (forced) return true;
            if (t is Fire { parent: Pawn burningPawn } &&
                Skill_FlameAffinity.HasFlameAffinity(burningPawn))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "IsCurrentJobPlayerInterruptible")]
    public static class Patch_IsCurrentJobPlayerInterruptible
    {
        //防止着火时玩家无法操控
        public static void Postfix(Pawn ___pawn, ref bool __result)
        {
            //Log.Message($"Patching For {___pawn?.Name.ToStringShort ?? "?"}");
            if (__result) return;
            if (Skill_FlameAffinity.HasFlameAffinity(___pawn) && ___pawn.IsBurning())
                __result = true;
        }
    }
}
