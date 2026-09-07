using FamiliaMyth.Pharna;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class HediffCompProperties_SkillState : HediffCompProperties
    {
        public SkillDef skillDef;

        public HediffCompProperties_SkillState(){
            compClass = typeof(HediffComp_SkillState);
        }
    }

    public class HediffComp_SkillState : HediffComp
    {
        public HediffCompProperties_SkillState Props => (HediffCompProperties_SkillState)props;

        private Hediff_Pharna Pharna => Pawn.GetPharna();

        private Skill skill;
        private Skill Skill => skill ??= Pharna?.skills?.FirstOrDefault(t => t.def == Props.skillDef);

        public override string CompLabelInBracketsExtra => Skill.StateText;
    }
}