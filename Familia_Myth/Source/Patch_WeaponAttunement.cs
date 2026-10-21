using FamiliaMyth.Abilities;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FamiliaMyth.Patch
{
    /// <summary>
    /// 武器同调：近战每次挥击按武器熟练度判定一次冷却重置。
    /// </summary>
    [HarmonyPatch(typeof(Stance_Cooldown), MethodType.Constructor, new[] { typeof(int), typeof(LocalTargetInfo), typeof(Verb) })]
    public static class Patch_WeaponAttunement_CooldownReset
    {
        public static void Postfix(Stance_Cooldown __instance, Verb verb)
        {
            try
            {
                if (__instance == null || verb == null || !verb.verbProps.IsMeleeAttack) return;

                Pawn pawn = verb.CasterPawn;
                if (pawn == null) return;

                DevelopableAbility_WeaponAttunement ability = pawn.GetWeaponAttunement();
                if (ability == null) return;

                ThingDef weaponDef = verb.EquipmentSource?.def;
                if (weaponDef == null || !weaponDef.IsMeleeWeapon) return;
                //Log.Message("Trying Reset Cooldown......");
                if (!Rand.Chance(ability.GetCooldownResetChance(weaponDef))) return;

                //留一点延迟，否则玩家看不出来
                __instance.ticksLeft = 10;
                //Log.Message("Reset Done!");
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_WeaponAttunement_CooldownReset)} error: {ex}");
            }
        }
    }

    /// <summary>
    /// 武器同调：近战命中结算时按武器熟练度判定暴击，暴击伤害翻倍。
    /// </summary>
    [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    public static class Patch_WeaponAttunement_Crit
    {
        //暴击伤害倍率
        private const float CritDamageMultiplier = 2f;
        //暴击飘字颜色（淡红）
        private static readonly Color CritTextColor = new(1f, 0.55f, 0.55f);

        public static void Prefix(Thing __instance, ref DamageInfo dinfo)
        {
            try
            {
                if (dinfo.Tool == null) return;
                if (dinfo.Instigator is not Pawn pawn) return;
                if (dinfo.Weapon == null) return;

                DevelopableAbility_WeaponAttunement ability = pawn.GetWeaponAttunement();
                if (ability == null) return;
                if (!Rand.Chance(ability.GetCritChance(dinfo.Weapon))) return;

                dinfo.SetAmount(dinfo.Amount * CritDamageMultiplier);

                if (__instance?.Map != null)
                    MoteMaker.ThrowText(__instance.DrawPos, __instance.Map,
                        "FamiliaMyth.WeaponAttunement.Crit".Translate(), CritTextColor, 1.9f);
            }
            catch (Exception ex)
            {
                Log.Error($"[FamiliaMyth] {nameof(Patch_WeaponAttunement_Crit)} error: {ex}");
            }
        }
    }
}
