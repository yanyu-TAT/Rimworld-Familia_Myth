using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn_PathFollower), "TryEnterNextPathCell")]
    public static class Patch_Pawn_PathFollower
    {
        public static void Prefix(Pawn ___pawn, ref IntVec3 __state)
        {
            __state = ___pawn.Position;
        }

        public static void Postfix(Pawn ___pawn, IntVec3 __state)
        {
            if (__state != ___pawn.Position)
            {
                //Log.Message("冲刺！冲！");
                //移动每格0.25点
                ___pawn.GetPharna()?.GainStatus(PharnaStatusType.Agility, 0.25f);
            }
        }
    }
}