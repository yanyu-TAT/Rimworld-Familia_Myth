using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace FamiliaMyth.Patch
{
    //美德状态期间令InMentalState返回 false。
    [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.InMentalState), MethodType.Getter)]
    public static class Patch_MentalState_Controllable_InMentalState
    {
        public static bool Prefix(MentalStateHandler __instance, ref bool __result)
        {
            try
            {
                if (__instance.CurStateDef != null && __instance.CurStateDef.stateClass == typeof(MentalState_Virtue))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_MentalState_Controllable_InMentalState)} error: {ex}");
                return true; //出错则放行原版精神状态判定
            }
        }
    }

    //美德状态期间令 Pawn.MentalStateDef 返回 null，使殖民者恢复玩家控制（征召按钮、右键命令）。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.MentalStateDef), MethodType.Getter)]
    public static class Patch_MentalState_Controllable_MentalStateDef
    {
        public static bool Prefix(Pawn __instance, ref MentalStateDef __result)
        {
            try
            {
                if (__instance.mindState?.mentalStateHandler?.CurStateDef?.stateClass == typeof(MentalState_Virtue))
                {
                    __result = null;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_MentalState_Controllable_MentalStateDef)} error: {ex}");
                return true; //出错则放行原版
            }
        }
    }

    //激活时若已征召则恢复征召
    [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
    public static class Patch_MentalState_Controllable_TryStart
    {
        public static void Prefix(MentalStateHandler __instance, MentalStateDef stateDef, ref (Pawn pawn, bool wasDrafted)? __state)
        {
            try
            {
                if (stateDef != null && stateDef.stateClass == typeof(MentalState_Virtue))
                {
                    var pawn = AccessTools.FieldRefAccess<MentalStateHandler, Pawn>("pawn")(__instance);
                    __state = (pawn, pawn != null && pawn.Drafted);
                }
                else
                {
                    __state = null;
                }
            }
            catch (Exception ex)
            {
                //置空，令 Postfix 直接跳过
                __state = null;
                Log.Error($"[FamiliaMyth] {nameof(Patch_MentalState_Controllable_TryStart)} prefix error: {ex}");
            }
        }

        public static void Postfix(MentalStateHandler __instance, (Pawn pawn, bool wasDrafted)? __state)
        {
            try
            {
                if (__state.HasValue && __state.Value.pawn != null && !__state.Value.pawn.Dead && __state.Value.wasDrafted)
                {
                    __state.Value.pawn.drafter.Drafted = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_MentalState_Controllable_TryStart)} postfix error: {ex}");
            }
        }
    }
}
