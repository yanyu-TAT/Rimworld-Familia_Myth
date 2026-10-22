using FamiliaMyth.Abilities;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    /// <summary>
    /// 连攻：每次近战出手计入一次攻速窗口（未命中 / 被闪避也计）。
    /// </summary>
    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    public static class Patch_ChainAttack_MeleeAttack
    {
        public static void Postfix(Verb_MeleeAttack __instance)
        {
            try
            {
                Pawn pawn = __instance.CasterPawn;
                if (pawn == null) return;
                pawn.GetChainAttack()?.Notify_MeleeAttack();
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_ChainAttack_MeleeAttack)} error: {ex}");
            }
        }
    }
}
