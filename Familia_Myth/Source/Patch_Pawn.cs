using FamiliaMyth.Abilities;
using FamiliaMyth.Pharna;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    public static class Patch_Pawn
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
        public static void PostApplyDamage(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            //每点承受伤害0.5点
            Hediff_Pharna.GetPharna(__instance)?.GainStatus(PharnaStatusType.Endurance, totalDamageDealt / 2f);
        }

        //防止重复处理
        [ThreadStatic] private static bool _handlingkill;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
        public static bool Kill(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit)
        {
            if (_handlingkill) return true;
            if (__instance == null || __instance.Dead) return true;

            try{
                _handlingkill = true;
                var pharna = Hediff_Pharna.GetPharna(__instance);
                if (pharna?.skills == null) return true;

                Skill_Ember ember = null;
                foreach (var skill in pharna.skills)
                    if (skill is Skill_Ember e) ember = e;

                if (ember == null) return true;

                //尝试应用濒死时的效果
                return !ember.TryApplyOnKill(__instance, dinfo, exactCulprit);
            }
            finally 
            { 
                _handlingkill = false; 
            }
        }
    }
}