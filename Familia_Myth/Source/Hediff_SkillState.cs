using Verse;

namespace FamiliaMyth.Abilities
{
    public class Hediff_SkillState : HediffWithComps
    {
        public HediffComp_SkillState comp;

        public HediffComp_SkillState Comp => comp ??= this.TryGetComp<HediffComp_SkillState>() ?? null;

        public override string LabelInBrackets => Comp.StateText;
    }
}