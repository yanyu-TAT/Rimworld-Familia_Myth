using FamiliaMyth.Pharna;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class HediffComp_SkillInEffect : HediffComp
    {
        protected HediffCompProperties_SkillInEffect Props => (HediffCompProperties_SkillInEffect)props;

        protected Skill skill;
        protected Skill Skill =>
            skill ??= parent?.pawn?.GetPharna()?.skills?.FirstOrDefault(s => s.def == Props.skillDef);

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            Skill.Notify_EnffectStart();
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            Skill.Notify_EffectEnd();
        }
    }

    public class HediffCompProperties_SkillInEffect : HediffCompProperties
    {
        public SkillDef skillDef;
    }
}
