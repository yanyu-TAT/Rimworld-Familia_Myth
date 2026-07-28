using FamiliaMyth.Pharna;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class HediffCompProperties_SkillState : HediffCompProperties
    {
        public string skillDefName;
        public string inEffectHediff;

        public HediffCompProperties_SkillState(){
            compClass = typeof(HediffComp_SkillState);
        }
    }

    public class HediffComp_SkillState : HediffComp
    {
        public HediffCompProperties_SkillState Props => (HediffCompProperties_SkillState)props;

        public SkillDef cachedSkillDef;
        public SkillDef SkillDeff => cachedSkillDef ??= DefDatabase<SkillDef>.GetNamed(Props.skillDefName);

        public HediffDef cachedHediffDef;
        public HediffDef HediffDef => cachedHediffDef ??= DefDatabase<HediffDef>.GetNamed(Props.inEffectHediff);

        private Hediff_Pharna Pharna => Hediff_Pharna.GetPharna(Pawn);

        private Skill cachedSkill;
        private Skill Skill => cachedSkill ??= Pharna?.skills?.FirstOrDefault(t => t.def.defName == SkillDeff.defName);

        public enum SkillState
        {
            InEffect,
            Cooldown,
            Ready,
            ERROR
        }

        public SkillState CurrentState
        {
            get
            {
                if (Pawn == null) return SkillState.ERROR;
                if (Skill == null) return SkillState.ERROR;
                if (HediffDef == null) return SkillState.ERROR;

                bool f = Pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDef) == null;
                return f ? (Skill.IsCooldown ? SkillState.Cooldown : SkillState.Ready) : SkillState.InEffect;
            }
        }

        public string CooldownText
        {
            get
            {
                if (Skill == null) return null;
                if (Skill.IsCooldown) return $"冷却剩余: {Skill.RemainingCooldown:F1}秒";
                else return null;
            }
        }
    }
}