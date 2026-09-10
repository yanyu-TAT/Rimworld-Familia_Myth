using FamiliaMyth.Abilities;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Thing), "DoTick")]
    public static class Patch_TheWorld
    {
        public static bool Prefix(Thing __instance)
        {
            try
            {
                if (!Spell_TheWorld.AnyActiveField) return true;
                return Spell_TheWorld.ShouldTick(__instance);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_TheWorld)} error: {ex}");
                return true; //出错则放行原版 Tick
            }
        }
    }
}
