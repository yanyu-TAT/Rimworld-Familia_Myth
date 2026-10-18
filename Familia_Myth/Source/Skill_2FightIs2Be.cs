using Verse;

namespace FamiliaMyth.Abilities
{
    public class Skill_2FightIs2Be : Skill
    {
        private static HediffDef effectDef;
        private static HediffDef EffectDef =>
            effectDef ??= DefDatabase<HediffDef>.GetNamed("FightIs2BeInEffect");

        public void Notify_BattleStart()
        {
            Owner?.health?.GetOrAddHediff(EffectDef);
        }

        public override bool ApplyEffect(LocalTargetInfo target)
        {
            return true;
        }
    }
}
