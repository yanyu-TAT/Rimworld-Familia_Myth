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
            try
            {
                __state = ___pawn.Position;
            }
            catch (Exception ex)
            {
                //置无效值，令 Postfix 直接跳过本次结算
                __state = IntVec3.Invalid;
                Log.Error($"[FamiliaMyth] {nameof(Patch_Pawn_PathFollower)} prefix error: {ex}");
            }
        }

        public static void Postfix(Pawn ___pawn, IntVec3 __state)
        {
            try
            {
                if (!__state.IsValid) return;
                if (__state != ___pawn.Position)
                {
                    //移动每格0.25点
                    ___pawn.GetPharna()?.GainStatus(PharnaStatusType.Agility, 0.25f);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_Pawn_PathFollower)} postfix error: {ex}");
            }
        }
    }
}