using FamiliaMyth.Abilities;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Thing), "DoTick")]
    public static class Patch_Thing_DoTick_TimeStop
    {
        public static bool Prefix(Thing __instance)
        {
            if (!Spell_TheWorld.AnyActiveField) return true;
            return Spell_TheWorld.ShouldTick(__instance);
        }
    }
}
