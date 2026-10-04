using FamiliaMyth.Feat;
using HarmonyLib;
using Verse;

namespace FamiliaMyth.Patch
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
    public static class Patch_FeatContribution_Damage
    {
        public static void Postfix(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            try
            {
                GameComponent_FeatTracker tracker = GameComponent_FeatTracker.Instance;
                if (tracker == null || __instance == null || totalDamageDealt <= 0f) return;

                //输出侧记给攻击者；承受侧记给受击者（带来源，供承伤路径过滤友军误伤）
                if (dinfo.Instigator is Pawn attacker && attacker != __instance)
                    tracker.RecordDealt(attacker, __instance, totalDamageDealt);
                tracker.RecordTaken(__instance, dinfo.Instigator as Pawn, totalDamageDealt);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_FeatContribution_Damage)} error: {ex}");
            }
        }
    }
}
