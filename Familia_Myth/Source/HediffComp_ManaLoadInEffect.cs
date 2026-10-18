using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class HediffComp_ManaLoadInEffect : HediffComp_SkillInEffect, IManaControlModifier, IStatusModifier
    {
        private const int EffectDurationTicks = GenDate.TicksPerHour * 2;
        private const float ManaControlModifyRate = 0.5f / 100;
        private const float StrengthModifyRate = 0.5f;

        private float manaSpent = 0;

        public void Notify_ManaConsumed(float amount)
        {
            if (parent?.TryGetComp<HediffComp_Disappears>() is HediffComp_Disappears timer)
                timer.ticksToDisappear = EffectDurationTicks;
            manaSpent += amount;
        }

        public bool ShouldApplyManaControl(SpellType type)
            => type == SpellType.Attack;

        public void ApplyManaControlModifier(ref float power)
        {
            power += ManaControlModifyRate * manaSpent;
        }

        public void ApplyStatusModifier(PharnaStatusType type, ref float val)
        {
            if (type == PharnaStatusType.Strength)
                val += StrengthModifyRate * manaSpent;
        }

        public string GetExplanation(PharnaStatusType type)
        {
            if (type == PharnaStatusType.Strength)
                return "FamiliaMyth.Ability.ManaLoadStrength".Translate((StrengthModifyRate * manaSpent).ToString("F1"));
            return "";
        }

        public override string CompTipStringExtra => "FamiliaMyth.Ability.ManaLoadManaSpent".Translate(manaSpent.ToString("F1"));

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref manaSpent, "manaSpent");
        }
    }
}
