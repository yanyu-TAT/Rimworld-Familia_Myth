using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_ManaLoad : Skill
    {
        private static HediffDef effectDef;
        private static HediffDef EffectDef =>
            effectDef ??= DefDatabase<HediffDef>.GetNamed("ManaLoadInEffect");

        public void Notify_ManaConsumed(float amount)
        {
            var hediff = Owner?.health?.GetOrAddHediff(EffectDef);
            if (hediff?.TryGetComp<HediffComp_ManaLoadInEffect>() is HediffComp_ManaLoadInEffect comp)
                comp.Notify_ManaConsumed(amount);
        }

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            return true;
        }
    }
}
