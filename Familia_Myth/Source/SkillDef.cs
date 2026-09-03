using FamiliaMyth.Pharna;
using RimWorld;
using Verse;

namespace FamiliaMyth.Abilities
{
    public class SkillDef : Def
    {
        public Type skillClass;

        public RewardRarity rarity;
        public SkillType skillType;

        public bool needTarget = false;

        public float cooldown = 0f;
        public float warmup = 0f;
        public float duration = 0f;
        public float checkInterval = 60f;
        public int maxCharge = 1;

        public string iconPath;

        public Skill CreateSkill()
        {
            var skill = (Skill)Activator.CreateInstance(skillClass);
            skill.def = this;
            return skill;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (skillType != SkillType.Active && needTarget)
                yield return $"Non-Active Skill {defName} Can't Have Target. \"needTarget\" Should Be False.";
        }
    }
    public enum SkillType
    {
        Active,     //主动
        Passive,    //被动（常态）
        Triggered   //被动（条件）
    }
}