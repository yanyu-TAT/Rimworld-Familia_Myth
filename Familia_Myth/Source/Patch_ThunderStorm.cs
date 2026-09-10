using FamiliaMyth.Abilities;
using HarmonyLib;
using RimWorld;
using Verse;

namespace FamiliaMyth.Patch
{


    [HarmonyPatch(typeof(WeatherManager), nameof(WeatherManager.TransitionLerpFactor), MethodType.Getter)]
    public static class Patch_ThunderStorm
    {
        private static WeatherDef thunderstormDef;
        private static WeatherDef ThunderstormDef =>
                thunderstormDef ??= DefDatabase<WeatherDef>.GetNamed("RainyThunderstorm");

        private const int TransitionTicks = 240;

        public static bool Prefix(WeatherManager __instance, ref float __result)
        {
            try
            {
                //仅拦截施法中且天气为暴风雨
                if (!Spell_ThunderStorm.ThunderStorming || __instance.curWeather != ThunderstormDef)
                {
                    return true;
                }

                float num = (float)__instance.curWeatherAge / TransitionTicks;
                if (num > 1f) num = 1f;

                __result = num;
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_ThunderStorm)} error: {ex}");
                return true; //出错则放行原版天气过渡
            }
        }
    }
}
