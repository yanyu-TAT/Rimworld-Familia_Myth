using FamiliaMyth.Abilities;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{
    /// <summary>
    /// 精愈：按每次精神力实际扣除量喂养熟练度（正向恢复不计）。
    /// </summary>
    [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker), nameof(Pawn_PsychicEntropyTracker.OffsetPsyfocusDirectly))]
    public static class Patch_MindHealing_PsyfocusSpent
    {
        public static void Prefix(Pawn_PsychicEntropyTracker __instance, ref float __state)
        {
            try
            {
                __state = __instance.CurrentPsyfocus;
            }
            catch (Exception ex)
            {
                __state = float.NaN;
                Log.Error($"[FamiliaMyth] {nameof(Patch_MindHealing_PsyfocusSpent)} prefix error: {ex}");
            }
        }

        public static void Postfix(Pawn_PsychicEntropyTracker __instance, float __state)
        {
            try
            {
                if (float.IsNaN(__state)) return;

                float spent = __state - __instance.CurrentPsyfocus;
                if (spent <= 0f) return;

                Pawn pawn = __instance.Pawn;
                if (pawn == null) return;
                pawn.GetMindHealing()?.GainProficiency(
                    spent * 100f * DevelopableAbility_MindHealing.ProfGainPerPsyfocusPercent);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_MindHealing_PsyfocusSpent)} postfix error: {ex}");
            }
        }
    }
}
