using Verse;

namespace FamiliaMyth.Abilities
{
    public class Hediff_SkillState : HediffWithComps
    {
        public HediffComp_SkillState cachedComp;

        public HediffComp_SkillState Comp => cachedComp ??= this.TryGetComp<HediffComp_SkillState>() ?? null;
        public HediffComp_SkillState.SkillState State
        {
            get
            {;
                if (Comp == null) return HediffComp_SkillState.SkillState.ERROR;
                return Comp.CurrentState;
            }
        }

        public override string Label => base.Label + "(" +State switch
        {
            HediffComp_SkillState.SkillState.InEffect => "生效中",
            HediffComp_SkillState.SkillState.Ready => "可适用",
            HediffComp_SkillState.SkillState.Cooldown => Comp.CooldownText,
            _ => base.Label
        } + ")";
    }
}