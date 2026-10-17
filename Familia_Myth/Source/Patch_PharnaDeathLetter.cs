using FamiliaMyth.Pharna;
using FamiliaMyth.UI;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    /// <summary>
    /// 持有恩惠的殖民者死亡时，在原版死亡信件正文后追加神明感知到的恩惠陨落描述
    /// </summary>
    [HarmonyPatch(typeof(HealthUtility), nameof(HealthUtility.GetDiedLetterText))]
    public static class Patch_PharnaDeathLetter
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref TaggedString __result)
        {
            try
            {
                if (pawn?.GetPharna() == null) return;
                __result = "FamiliaMyth.PharnaDeath.DeathLetterText".Translate().Colorize(UI_Palette.Inscribed) + "\n\n" + __result;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_PharnaDeathLetter)} error: {ex}");
            }
        }
    }
}
