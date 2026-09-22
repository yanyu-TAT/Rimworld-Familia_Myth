using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_Railgun : Skill
    {
        public const int StunTicks = 120;

        private static HediffDef inEffectDef;
        private static HediffDef InEffectDef =>
            inEffectDef ??= DefDatabase<HediffDef>.GetNamed("RailgunInEffect");

        public override AcceptanceReport CanCast
        {
            get
            {
                var canCast = base.CanCast;
                if (!canCast) return canCast;

                if (Owner.GetRangedEquippedVerb() == null)
                    return "FamiliaMyth.Ability.NoRangedWeapon".Translate();

                return true;
            }
        }

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            if (Owner?.health == null || Owner.Map == null || Owner.DeadOrDowned)
                return false;

            var hediff = Owner.health.GetOrAddHediff(InEffectDef);
            return hediff != null;
        }
    }
}
