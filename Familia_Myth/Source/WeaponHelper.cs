using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public static class WeaponHelper
    {
        /// <summary>
        /// 获取对应殖民者已装备且伤害最高的锐伤近战武器
        /// </summary>
        public static Verb GetMeleeSharpWeapon(this Pawn pawn)
        {
            var verb = pawn?.equipment?.AllEquipmentVerbs?
                .Where(v => v.verbProps.IsMeleeAttack && v.verbProps.meleeDamageDef?.armorCategory == DamageArmorCategoryDefOf.Sharp)
                .OrderByDescending(v => v.verbProps.AdjustedMeleeDamageAmount(v, pawn))
                .FirstOrDefault();

            return verb;
        }

        /// <summary>
        /// 获取对应殖民者空手时伤害最高的verb
        /// </summary>
        public static Verb GetUnarmedMeleeVerb(this Pawn pawn)
        {
            var verb = pawn?.VerbTracker?.AllVerbs?
                .Where(v => v.verbProps.IsMeleeAttack)
                .OrderByDescending(v => v.verbProps.AdjustedMeleeDamageAmount(v, pawn))
                .FirstOrDefault();

            return verb;
        }
    }
}
